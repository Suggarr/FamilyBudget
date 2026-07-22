using System.Globalization;
using System.Text;
using FamilyBudget.Application.Dtos.History;

namespace FamilyBudget.Telegram.History;

public static class FamilyHistoryMessageFormatter
{
    private const int MaxDescriptionLength = 120;
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("ru-RU");

    public static string Format(FamilyHistoryPageDto history)
    {
        if (history.Items.Count == 0)
        {
            return """
                   📖 История семьи

                   Операций пока нет. Добавьте доход, расход или операцию с копилкой.
                   """;
        }

        var builder = new StringBuilder();
        builder.AppendLine("📖 История семьи");
        builder.AppendLine($"Страница {history.Page} из {history.TotalPages} · всего операций: {history.TotalCount}");

        foreach (var operation in history.Items)
        {
            builder.AppendLine();
            builder.AppendLine(FormatTitle(operation));
            builder.AppendLine($"{operation.Date.ToString("dd.MM.yyyy HH:mm", DisplayCulture)} · {operation.UserName}");
            builder.AppendLine(Shorten(operation.Description));
        }

        return builder.ToString().TrimEnd();
    }

    private static string FormatTitle(FamilyOperationDto operation)
    {
        var (icon, direction, title) = operation.Type switch
        {
            FamilyOperationType.Income => ("🟢", "+", "Доход"),
            FamilyOperationType.Expense => ("🔴", "−", "Расход"),
            FamilyOperationType.SavingsContribution => ("🏦", "↗", "Внесение в копилку"),
            FamilyOperationType.SavingsWithdrawal => ("🏦", "↙", "Снятие из копилки"),
            _ => ("•", string.Empty, "Операция")
        };

        return $"{icon} {direction}{operation.Amount.ToString("0.00", DisplayCulture)} BYN — {title}";
    }

    private static string Shorten(string description)
    {
        var normalized = string.Join(
            ' ',
            description.Split(
                ['\r', '\n', '\t'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        if (string.IsNullOrWhiteSpace(normalized))
            return "Без описания";

        return normalized.Length <= MaxDescriptionLength
            ? normalized
            : $"{normalized[..(MaxDescriptionLength - 1)]}…";
    }
}
