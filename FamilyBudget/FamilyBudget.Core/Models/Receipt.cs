using CSharpFunctionalExtensions;

namespace FamilyBudget.Core.Models;

public class Receipt
{
    private Receipt(Guid id, Guid familyId, Guid userId, string filePath, bool isProcessed, decimal? totalAmount)
    {
        Id = id;
        FamilyId = familyId;
        UserId = userId;
        FilePath = filePath;
        IsProcessed = isProcessed;
        TotalAmount = totalAmount;
    }

    public Guid Id { get; }

    public Guid FamilyId { get; }
    public Guid UserId { get; }

    public string FilePath { get; }
    public bool IsProcessed { get;  }
    public decimal? TotalAmount { get; }

    public static Result<Receipt> Create(Guid id, Guid familyId, Guid userId, string filePath, bool isProcessed, decimal? totalAmount)
    {
        if (string.IsNullOrWhiteSpace(filePath) )
            return Result.Failure<Receipt>("File path is required.");

        var receipt = new Receipt(id, familyId, userId, filePath.Trim(), isProcessed, totalAmount);
        return Result.Success(receipt);
    }
}