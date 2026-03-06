using CSharpFunctionalExtensions;
using FamilyBudget.Application.Dtos.Category;

namespace FamilyBudget.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<Guid>> CreateAsync(CreateCategoryDto dto);
        Task DeleteAsync(Guid id);
        Task<List<CategoryDto>> GetFamilyCategories(Guid familyId);
        Task<Result> RenameAsync(RenameCategoryDto dto);
    }
}