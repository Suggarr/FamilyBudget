using FamilyBudget.Core.Models;

namespace FamilyBudget.Core.Interfaces;

public interface IReceiptParser
{
    Task<ReceiptParseResult> ParseAsync(byte[] imageBytes, string mediaType, CancellationToken cancellationToken = default);
}
