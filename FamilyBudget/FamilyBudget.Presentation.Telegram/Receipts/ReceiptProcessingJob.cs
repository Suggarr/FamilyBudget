using FamilyBudget.Application.Dtos.Receipt;

namespace FamilyBudget.Presentation.Telegram.Receipts;

public sealed record ReceiptProcessingJob(
    long ChatId,
    CreateReceiptDto Receipt);
