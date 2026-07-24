using FamilyBudget.Core.Models;

namespace FamilyBudget.Application.Exceptions;

public sealed class ReceiptAmountsValidationException : InvalidOperationException
{
    public ReceiptAmountsValidationException(string message, ReceiptParseResult parsedReceipt)
        : base(message)
    {
        ParsedReceipt = parsedReceipt;
    }

    public ReceiptParseResult ParsedReceipt { get; }
}
