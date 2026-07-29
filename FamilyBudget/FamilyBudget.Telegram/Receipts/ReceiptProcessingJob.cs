using FamilyBudget.Application.Dtos.Receipt;

namespace FamilyBudget.Telegram.Receipts;

public sealed record ReceiptProcessingJob(
    long ChatId,
    CreateReceiptDto Receipt);
