using System.Text.Json;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;

namespace FamilyBudget.Infrastructure.AI;

public sealed class OllamaReceiptParser : IReceiptParser
{
    private const string SystemPrompt = """
        Ты точный помощник по распознаванию кассовых чеков.
        Извлекай только информацию, которая видна на переданном изображении чека.
        Для каждой позиции извлеки название, количество, цену за единицу, скидку и итоговую сумму позиции.
        Количество может быть десятичным числом, например вес в килограммах: 0.550 или 1.105.
        Извлеки магазин, дату покупки, сумму до скидки, скидку, налог при наличии и итоговую сумму чека.
        Итоговая сумма — фактически уплаченная сумма после скидок.
        Внимательно читай каждую цифру: не округляй и не приближай значения.
        Например, если на чеке указано 1.105, верни именно 1.105, а не 1.1 или 1.2.
        Если данные позиции различимы, проверь, что quantity * unitPrice с учётом скидки соответствует totalAmount.
        Не придумывай товары, магазин, даты, цены, скидки или налоги.
        Для отсутствующей строки возвращай null, для неизвестного числового значения — 0, а нечитаемые позиции не включай в список.

        ФОРМАТ ЧИСЕЛ: чек может использовать точку или запятую как десятичный разделитель.
        Сначала определи формат по значениям на чеке, затем всегда возвращай числа в JSON с точкой как десятичным разделителем.
        Например: 7499.00, а не 7.499,00 или 7,499.00.

        ВАЛЮТА: приложение ведёт расходы в белорусских рублях. Всегда указывай currency как BYN.
        Сохраняй названия товаров на языке, напечатанном в чеке.
        """;

    private const string UserPrompt = "Распознай чек и верни данные строго в запрошенной JSON-структуре.";
    private const string DiscountSignPrompt = """
        Денежные значения скидок в JSON всегда должны быть неотрицательными.
        Если на чеке напечатано «СКИДКА -0,01», верни item.discountAmount=0.01.
        Строка «СКИДКА» не является отдельным товаром и не должна попадать в items.

        Читай позиции строго сверху вниз блоками. Числовая строка вида «цена x количество = сумма»
        относится к ближайшему ПРЕДЫДУЩЕМУ названию товара, а не к следующему названию.
        Строка «СКИДКА» внутри такого блока относится к этому же предыдущему товару и не может
        быть перенесена на соседнюю позицию. Не начинай новую позицию по строке скидки.

        Отдельно найди итоговую скидку во всём чеке: её метки могут быть «ИТОГО СКИДКА»,
        «СКИДКА ИТОГО», «ОБЩАЯ СКИДКА», «ВАША СКИДКА» или просто «СКИДКА» в блоке итогов.
        Она может находиться после строк «ИТОГО» или «ИТОГО К ОПЛАТЕ».
        Это discountAmount всего чека, а не скидка товара. Если итоговой строки нет,
        discountAmount всего чека равен сумме item.discountAmount всех позиций.
        """;
    private const string TotalsAndItemsPrompt = """
        Обязательно различай промежуточную сумму и сумму к оплате:
        - надписи «ИТОГ», «К ОПЛАТЕ» и «ОПЛАЧЕНО» означают totalAmount — фактически уплаченную сумму;
        - надписи «ВСЕГО» и «СУММА» перед общей скидкой означают subtotal — сумму до общей скидки;
        - строка «СКИДКА» непосредственно перед итогом означает discountAmount всего чека;
        - скидка, напечатанная внутри блока товара, относится только к этому товару и записывается в item.discountAmount;
        - сумма строк товаров может совпадать с subtotal, а не с totalAmount, если ниже указана дополнительная скидка на весь чек.

        Пример: «ВСЕГО 546,52; СКИДКА 27,33; ИТОГ 519,19» означает
        subtotal=546.52, discountAmount=27.33, totalAmount=519.19.

        Пример с построчными скидками: «ИТОГО 34,80; ИТОГО К ОПЛАТЕ 34,73;
        ИТОГО СКИДКА -0,07» означает subtotal=34.80, discountAmount=0.07,
        totalAmount=34.73. Не возвращай discountAmount=0, если на чеке видна
        итоговая скидка или скидки позиций.

        Для каждой позиции всегда извлекай quantity и unitPrice из строки вида «количество X цена».
        Сохраняй дробное количество без округления: «1,853 X 286,00» означает quantity=1.853 и unitPrice=286.00.
        Если «2,000 X 9,20» сопровождается скидкой 1,84 и суммой 16,56, верни
        quantity=2.000, unitPrice=9.20, discountAmount=1.84, totalAmount=16.56.
        В качестве merchantName используй видимое название в верхней части чека, даже если там написано просто «Магазин».

        Перед ответом проверь арифметику:
        quantity * unitPrice - item.discountAmount приблизительно равно item.totalAmount,
        subtotal - discountAmount + taxAmount приблизительно равно totalAmount.
        Возвращай только JSON без пояснений.
        """;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IChatClient _chatClient;

    public OllamaReceiptParser(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<ReceiptParseResult> ParseAsync(
        byte[] imageBytes,
        string mediaType,
        CancellationToken cancellationToken = default)
    {
        if (imageBytes.Length == 0)
            throw new InvalidOperationException("Receipt image is empty.");

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User, new List<AIContent>
            {
                new TextContent($"{UserPrompt}\n\n{TotalsAndItemsPrompt}\n\n{DiscountSignPrompt}"),
                new DataContent(imageBytes, mediaType)
            })
        };

        var chatOptions = new ChatOptions
        {
            Temperature = 0,
            MaxOutputTokens = 1024,
            ResponseFormat = ChatResponseFormat.ForJsonSchema<ReceiptParsePayload>(JsonOptions, "receipt")
        };
        chatOptions.AddOllamaOption(OllamaOption.Think, false);

        var response = await _chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);

        if (string.IsNullOrWhiteSpace(response.Text))
            throw new InvalidOperationException("Ollama returned an empty receipt response.");

        var parsed = JsonSerializer.Deserialize<ReceiptParsePayload>(response.Text, JsonOptions)
            ?? throw new InvalidOperationException("Ollama returned invalid receipt JSON.");

        var items = parsed.Items?
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !IsDiscountLine(item))
            .Select(item => new ReceiptParseItem(
                item.Name!.Trim(),
                item.Quantity > 0 ? item.Quantity : 1,
                item.UnitPrice,
                Math.Abs(item.DiscountAmount),
                item.TotalAmount))
            .ToList() ?? [];

        var itemDiscountAmount = items.Sum(item => item.DiscountAmount);
        var receiptDiscountAmount = ResolveReceiptDiscountAmount(parsed.DiscountAmount, itemDiscountAmount);
        var subtotal = parsed.Subtotal > 0
            ? parsed.Subtotal
            : items.Sum(item => item.TotalAmount);
        var totalAmount = ResolveTotalAmount(parsed, items, subtotal, receiptDiscountAmount);
        if (totalAmount <= 0)
            throw new InvalidOperationException("Ollama could not extract a positive receipt total.");

        return new ReceiptParseResult(
            parsed.MerchantName,
            parsed.PurchasedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            subtotal,
            receiptDiscountAmount,
            parsed.TaxAmount,
            totalAmount,
            "BYN",
            items,
            response.Text);
    }

    private static decimal ResolveTotalAmount(
        ReceiptParsePayload parsed,
        IReadOnlyCollection<ReceiptParseItem> items,
        decimal subtotal,
        decimal receiptDiscountAmount)
    {
        if (parsed.TotalAmount > 0)
            return parsed.TotalAmount;

        var calculatedTotal = subtotal - receiptDiscountAmount + parsed.TaxAmount;

        return calculatedTotal > 0 ? calculatedTotal : items.Sum(item => item.TotalAmount);
    }

    private static decimal ResolveReceiptDiscountAmount(decimal parsedDiscountAmount, decimal itemDiscountAmount)
    {
        var extractedDiscount = Math.Abs(parsedDiscountAmount);
        if (extractedDiscount > 0)
            return extractedDiscount;

        return itemDiscountAmount;
    }

    private static bool IsDiscountLine(ReceiptParsePayloadItem item)
    {
        var name = item.Name?.Trim();
        return !string.IsNullOrWhiteSpace(name) &&
               name.Contains("скидк", StringComparison.OrdinalIgnoreCase) &&
               item.TotalAmount <= 0;
    }

    private sealed class ReceiptParsePayload
    {
        public string? MerchantName { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Currency { get; set; }
        public List<ReceiptParsePayloadItem>? Items { get; set; }
    }

    private sealed class ReceiptParsePayloadItem
    {
        public string? Name { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
