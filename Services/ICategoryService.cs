using ShoppingListApi.Dtos;

namespace ShoppingListApi.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(Guid id);
}