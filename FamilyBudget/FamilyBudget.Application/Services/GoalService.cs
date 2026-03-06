using AutoMapper;
using FamilyBudget.Application.Dtos.Goal;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class GoalService : IGoalService
    {
        private readonly IGoalRepository _goalRepository;

        public GoalService(IGoalRepository goalRepository)
        {
            _goalRepository = goalRepository;
        }

        public async Task<Guid> CreateAsync(CreateGoalDto dto)
        {
            var goalResult = Goal.Create(
                Guid.NewGuid(),
                dto.FamilyId,
                dto.Title,
                dto.TargetAmount,
                0);

            if (goalResult.IsFailure)
            {
                throw new Exception(goalResult.Error);
            }

            var goal = goalResult.Value;
            return await _goalRepository.AddAsync(goal);
        }

        public async Task<List<GoalDto>> GetByFamilyAsync(Guid familyId)
        {
            var goals = await _goalRepository.GetByFamilyIdAsync(familyId);

            return goals.Select(g => new GoalDto(
                g.Id,
                g.FamilyId,
                g.Title,
                g.TargetAmount,
                g.CurrentAmount,
                g.GetProgressPercent(),
                g.IsCompleted
            )).ToList();
        }

        public async Task AddMoneyAsync(Guid id, decimal amount)
        {
            var goal = await _goalRepository.GetByIdAsync(id) ?? throw new Exception("Goal not found");
            var addMoneyResult = goal.AddMoney(amount);

            if (addMoneyResult.IsFailure)
            {
                throw new Exception(addMoneyResult.Error);
            }

            await _goalRepository.UpdateAsync(goal);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _goalRepository.DeleteAsync(id);
        }


    }
}
