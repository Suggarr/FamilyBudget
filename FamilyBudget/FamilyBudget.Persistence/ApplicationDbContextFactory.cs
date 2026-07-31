using FamilyBudget.Persistence.DesignTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FamilyBudget.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DesignTimeEnvironmentFileLoader.Load();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__FamilyBudgetDbContext");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings__FamilyBudgetDbContext is not configured.");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
