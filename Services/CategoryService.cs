using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data;
using ShoppingListApi.Dtos;
using ShoppingListApi.Models;

namespace ShoppingListApi.Services;

public class CategoryService(AppDbContext db) : ICategoryService
{
    private static readonly Expression<Func<Category, CategoryDto>> ToDto =
        c => new CategoryDto(c.Id, c.Name, c.Description, c.Icon, c.CreatedAt, c.UpdatedAt);

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        => await db.Categories.Select(ToDto).ToListAsync();

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
        => await db.Categories.Where(c => c.Id == id).Select(ToDto).FirstOrDefaultAsync();

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return new CategoryDto(
            category.Id, category.Name, category.Description,
            category.Icon, category.CreatedAt, category.UpdatedAt);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return null;

        if (dto.Name is not null) category.Name = dto.Name;
        if (dto.Description is not null) category.Description = dto.Description;
        if (dto.Icon is not null) category.Icon = dto.Icon;
        category.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return new CategoryDto(
            category.Id, category.Name, category.Description,
            category.Icon, category.CreatedAt, category.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return false;

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return true;
    }
}