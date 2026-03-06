using AutoMapper;
using CSharpFunctionalExtensions;
using FamilyBudget.Application.Dtos.Category;
using FamilyBudget.Application.Interfaces;
using FamilyBudget.Core.Models;
using FamilyBudget.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<Result<Guid>> CreateAsync(CreateCategoryDto dto)
        {
            var existing = await _categoryRepository.GetByNameAsync(dto.FamilyId, dto.Name.Trim());

            if (existing != null)
            {
                return Result.Failure<Guid>("Category already exists");
            }

            var categoryResult = Category.Create(Guid.NewGuid(), dto.FamilyId, dto.Name);

            if (categoryResult.IsFailure)
            {
                return Result.Failure<Guid>(categoryResult.Error);
            }

            var id = await _categoryRepository.AddAsync(categoryResult.Value);
            return Result.Success(id);
        }

        public async Task<List<CategoryDto>> GetFamilyCategories(Guid familyId)
        {
            var categories = await _categoryRepository.GetByFamilyAsync(familyId);

            return categories.Select(c => new CategoryDto(c.Id, c.Name)).ToList();
        }

        public async Task<Result> RenameAsync(RenameCategoryDto dto)
        {
            var category = await _categoryRepository
                .GetByIdAsync(dto.CategoryId);

            if (category == null)
                return Result.Failure("Category not found");

            var existing = await _categoryRepository
                .GetByNameAsync(dto.FamilyId, dto.NewName.Trim());

            if (existing != null && existing.Id != dto.CategoryId)
                return Result.Failure("Category with this name already exists");

            var updatedResult = Category.Create(
                category.Id,
                dto.FamilyId,
                dto.NewName);

            if (updatedResult.IsFailure)
                return Result.Failure(updatedResult.Error);

            await _categoryRepository.UpdateAsync(updatedResult.Value);

            return Result.Success();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
        }
    }
}
