namespace FamilyBudget.Core.Dtos.Family;

public record JoinFamilyResult(bool IsSuccess, string? Error)
{
    public static JoinFamilyResult Success() => new(true, null);
    public static JoinFamilyResult Failure(string error) => new(false, error);
}
