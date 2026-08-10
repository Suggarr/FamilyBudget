namespace FamilyBudget.Persistence.Entities;

public class UserEntity
{
    public const int MAX_NAME_LENGTH = 100;

    public Guid Id { get; set; }
    public AccountEntity Account { get; set; } = null!;

    public Guid? FamilyId { get; set;}
    public FamilyEntity? Family { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
    public ICollection<IncomeEntity> Incomes { get; set; } = new List<IncomeEntity>();
    public ICollection<SavingsContributionEntity> SavingsContributions { get; set; } = new List<SavingsContributionEntity>();
    public ICollection<SavingsWithdrawalEntity> SavingsWithdrawals { get; set; } = new List<SavingsWithdrawalEntity>();
    public ICollection<ReceiptEntity> Receipts { get; set; } = new List<ReceiptEntity>();

    public ReportSubscriptionEntity? ReportSubscription { get; set; }
}
