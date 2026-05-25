using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data;
using ShoppingListApi.Dtos;
using ShoppingListApi.Models;

namespace ShoppingListApi.Services;

public class ShoppingItemService(AppDbContext db) : IShoppingItemService
{
    // Abstract the projection to avoid code duplication
    private static readonly Expression<Func<ShoppingItem, ShoppingItemDto>> ToDto =
        i => new ShoppingItemDto(
            i.Id, i.Name, i.Quantity, i.Purchased,
            i.CategoryId,
            i.Category != null ? i.Category.Name : null,
            i.CreatedAt, i.UpdatedAt);

    public async Task<IReadOnlyList<ShoppingItemDto>> GetAllAsync()
    {
        return await db.ShoppingItems.Select(ToDto).ToListAsync();
    }

    public async Task<ShoppingItemDto?> GetByIdAsync(Guid id)
    {
        return await db.ShoppingItems
            .Where(i => i.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();
    }

    public async Task<(ShoppingItemDto? Item, string? Error)> CreateAsync(CreateShoppingItemDto dto)
    {
        var item = new ShoppingItem
        {
            Name = dto.Name,
            Quantity = dto.Quantity
        };

        if (dto.CategoryId.HasValue)
        {
            var exists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
            if (!exists) return (null, "Category not found");
            item.CategoryId = dto.CategoryId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.CategoryName))
        {
            var trimmedName = dto.CategoryName.Trim();
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Name == trimmedName);

            if (category is null)
            {
                category = new Category { Name = trimmedName };
                db.Categories.Add(category);
            }
            item.Category = category;
        }
        else
        {
            return (null, "Either categoryId or categoryName must be provided");
        }

        db.ShoppingItems.Add(item);
        await db.SaveChangesAsync();

        var result = await db.ShoppingItems
            .Where(i => i.Id == item.Id)
            .Select(ToDto)
            .FirstAsync();

        return (result, null);
    }

    public async Task<(ShoppingItemDto? Item, string? Error)> UpdateAsync(Guid id, UpdateShoppingItemDto dto)
    {
        var item = await db.ShoppingItems.FindAsync(id);
        if (item is null) return (null, null);  // null Error = not found（區隔於 bad request）

        if (dto.CategoryId.HasValue)
        {
            var exists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
            if (!exists) return (null, "Category not found"); // TODO: Create new category if categoryName provided
            item.CategoryId = dto.CategoryId.Value;
        }

        if (dto.Name is not null) item.Name = dto.Name;
        if (dto.Quantity.HasValue) item.Quantity = dto.Quantity.Value;
        if (dto.Purchased.HasValue) item.Purchased = dto.Purchased.Value;
        item.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var result = await db.ShoppingItems
            .Where(i => i.Id == item.Id)
            .Select(ToDto)
            .FirstAsync();

        return (result, null);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await db.ShoppingItems.FindAsync(id);
        if (item is null) return false;

        db.ShoppingItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }
}