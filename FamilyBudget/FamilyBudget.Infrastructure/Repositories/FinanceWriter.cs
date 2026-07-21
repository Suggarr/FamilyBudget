using AutoMapper;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Infrastructure.Repositories;

public class FinanceWriter : IFinanceWriter
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FinanceWriter(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task AddIncomeAsync(Income income, User updatedUser) =>
        SaveCreditAsync(() => _context.Incomes.Add(_mapper.Map<IncomeEntity>(income)), updatedUser.Id, income.Amount);

    public Task DeleteIncomeAsync(Income income, User updatedUser) =>
        SaveDebitAsync(() => _context.Incomes.Remove(_mapper.Map<IncomeEntity>(income)), updatedUser.Id, income.Amount);

    public Task AddExpenseAsync(Expense expense, User updatedUser) =>
        SaveDebitAsync(() => _context.Expenses.Add(_mapper.Map<ExpenseEntity>(expense)), updatedUser.Id, expense.Amount);

    public Task DeleteExpenseAsync(Expense expense, User updatedUser) =>
        SaveCreditAsync(() => _context.Expenses.Remove(_mapper.Map<ExpenseEntity>(expense)), updatedUser.Id, expense.Amount);

    public Task AddSavingsContributionAsync(SavingsContribution contribution, User updatedUser) =>
        SaveDebitAsync(() => _context.SavingsContributions.Add(_mapper.Map<SavingsContributionEntity>(contribution)), updatedUser.Id, contribution.Amount);

    public async Task AddSavingsWithdrawalAsync(SavingsWithdrawal withdrawal, User updatedUser)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var lockedFamily = await _context.Families
                .FromSqlInterpolated($"SELECT * FROM \"Families\" WHERE \"Id\" = {withdrawal.FamilyId} FOR UPDATE")
                .AsNoTracking()
                .ToListAsync();

            if (lockedFamily.Count != 1)
                throw new InvalidOperationException("Family not found.");

            var totalContributions = await _context.SavingsContributions
                .Where(c => c.FamilyId == withdrawal.FamilyId)
                .SumAsync(c => c.Amount);
            var totalWithdrawals = await _context.SavingsWithdrawals
                .Where(w => w.FamilyId == withdrawal.FamilyId)
                .SumAsync(w => w.Amount);

            if (totalContributions - totalWithdrawals < withdrawal.Amount)
                throw new InvalidOperationException("Insufficient savings.");

            var changedUsers = await _context.Users
                .Where(u => u.Id == updatedUser.Id && u.FamilyId == withdrawal.FamilyId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.Balance, u => u.Balance + withdrawal.Amount));

            if (changedUsers != 1)
                throw new InvalidOperationException("User does not belong to this family.");

            await _context.SavingsWithdrawals.AddAsync(_mapper.Map<SavingsWithdrawalEntity>(withdrawal));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task SaveCreditAsync(Action change, Guid userId, decimal amount)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Users
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.Balance, u => u.Balance + amount));
            change();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task SaveDebitAsync(Action change, Guid userId, decimal amount)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var changedRows = await _context.Users
                .Where(u => u.Id == userId && u.Balance >= amount)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.Balance, u => u.Balance - amount));

            if (changedRows == 0)
                throw new InvalidOperationException("Insufficient funds.");

            change();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }
}
