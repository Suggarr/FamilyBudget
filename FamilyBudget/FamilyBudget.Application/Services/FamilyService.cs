using AutoMapper;
using CSharpFunctionalExtensions;
using FamilyBudget.Core.Dtos.Family;
using FamilyBudget.Core.Dtos.User;
using FamilyBudget.Core.Interfaces;
using FamilyBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class FamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IMapper _mapper;

        public FamilyService(IFamilyRepository familyRepository, IMapper mapper)
        {
            _familyRepository = familyRepository;
            _mapper = mapper;
        }

        public async Task<Guid> CreateAsync(CreateUserDto dto)
        {
            var familyResult = Family.Create(Guid.NewGuid(), dto.Name);

            if (familyResult.IsFailure)
            {
                throw new Exception(familyResult.Error);
            }

            var family = familyResult.Value;

            return await _familyRepository.AddAsync(family);
        }

        public async Task<FamilyDto?> GetByIdAsync(Guid id)
        {
            var family = await _familyRepository.GetByIdAsync(id);

            return family is null ? null : _mapper.Map<FamilyDto>(family);
        }

        public async Task<List<FamilyDto>> GetAllAsync()
        {
            var families = await _familyRepository.GetAllAsync();
            return _mapper.Map<List<FamilyDto>>(families);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _familyRepository.DeleteAsync(id);
        }
    }
}
