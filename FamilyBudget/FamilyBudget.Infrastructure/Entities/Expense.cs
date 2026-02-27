namespace FamilyBudget.Infrastructure.Entities;

public class Expense
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }
    public Family Family = null!;

    public Guid UserId { get; set; }
    public User User = null!;

    public Guid CategoryId { get; set; }
    public Category Category = null!;

    public decimal Amount{ get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
