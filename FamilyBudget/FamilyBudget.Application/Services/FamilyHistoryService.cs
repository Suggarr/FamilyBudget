using FamilyBudget.Application.Dtos.History;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;

namespace FamilyBudget.Application.Services
{
    public class FamilyHistoryService : IFamilyHistoryService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IIncomeRepository _incomeRepository;
        private readonly ISavingsContributionRepository _savingsContributionRepository;
        private readonly ISavingsWithdrawalRepository _savingsWithdrawalRepository;
        private readonly IUserRepository _userRepository;

        public FamilyHistoryService(IExpenseRepository expenseRepository, IIncomeRepository incomeRepository, 
            ISavingsContributionRepository savingsContributionRepository, ISavingsWithdrawalRepository savingsWithdrawalRepository,
            IUserRepository userRepository)
        {
            _expenseRepository = expenseRepository;
            _incomeRepository = incomeRepository;
            _savingsContributionRepository = savingsContributionRepository;
            _savingsWithdrawalRepository = savingsWithdrawalRepository;
            _userRepository = userRepository;
        }

        public async Task<FamilyHistoryPageDto> GetFamilyHistoryAsync(Guid familyId, int page, int pageSize = 10)
        {
            if (familyId == Guid.Empty)
            {
                throw new ArgumentException("Family ID cannot be empty.", nameof(familyId));
            }
            if (page<=0)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(page));
            }
            if (pageSize<1 || pageSize > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 50.");
            }

            var incomesFamily = await _incomeRepository.GetByFamilyIdAsync(familyId);
            var expensesFamily = await _expenseRepository.GetByFamilyIdAsync(familyId);
            var savingsContributionsFamily = await _savingsContributionRepository.GetByFamilyIdAsync(familyId);
            var savingsWithdrawalsFamily = await _savingsWithdrawalRepository.GetByFamilyIdAsync(familyId);
            var usersFamily = await _userRepository.GetByFamilyIdAsync(familyId);

            var userNames = usersFamily.ToDictionary(u => u.Id, u => u.Name);

            string GetUserName(Guid userId) => userNames.TryGetValue(userId, out var name) ? name : "Неизвестный пользователь";

            var operations = new List<FamilyOperationDto>();


            operations.AddRange(incomesFamily.Select(i => new FamilyOperationDto
            (
                i.Id,
                FamilyOperationType.Income,
                i.UserId,
                GetUserName(i.UserId),
                i.Amount,
                i.Source,
                i.Date,
                null
            )));

            operations.AddRange(expensesFamily.Select(e => new FamilyOperationDto(
                e.Id,
                FamilyOperationType.Expense,
                e.UserId,
                GetUserName(e.UserId),
                e.Amount,
                e.Description,
                e.Date,
                null
            )));

            operations.AddRange(savingsContributionsFamily.Select(sc => new FamilyOperationDto(
                sc.Id,
                FamilyOperationType.SavingsContribution,
                sc.UserId,
                GetUserName(sc.UserId),
                sc.Amount,
                "Внесение в копилку",
                sc.CreatedAt,
                null
            )));

            operations.AddRange(savingsWithdrawalsFamily.Select(sw => new FamilyOperationDto(
                sw.Id,
                FamilyOperationType.SavingsWithdrawal,
                sw.UserId,
                GetUserName(sw.UserId),
                sw.Amount,
                "Снятие из копилки",
                sw.CreatedAt,
                null
            )));

            var orderedOperations = operations
                .OrderByDescending(op => op.Date)
                .ToList();

            var totalCount = orderedOperations.Count;
            var totalPages = Math.Max(1,(int)Math.Ceiling(totalCount / (double)pageSize));

            var actualPage = Math.Min(page, totalPages);

            var items = orderedOperations
                .Skip((actualPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new FamilyHistoryPageDto(items, actualPage, pageSize, totalCount, totalPages);
        }
    }
}
