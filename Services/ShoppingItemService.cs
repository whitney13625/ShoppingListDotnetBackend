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

    public async Task<PagedResponse<ShoppingItemDto>> GetAllAsync(ShoppingItemQuery query)
    {
        // Start with an IQueryable —— nothing has run yet
        var items = db.ShoppingItems.AsQueryable();

        // Condition assembly: add Where only if a value is provided (Composable)
        if (query.CategoryId.HasValue)
            items = items.Where(i => i.CategoryId == query.CategoryId.Value);

        if (query.Purchased.HasValue)
            items = items.Where(i => i.Purchased == query.Purchased.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            items = items.Where(i => i.Name.Contains(query.Search));

        // Calculate the total count of records matching the criteria (before pagination)
        var total = await items.CountAsync();

        // Normalize pagination input (defensive programming)
        var page = query.Page < 1 ? 1 : query.Page;
        var limit = query.Limit switch
        {
            < 1 => 20,
            > 100 => 100, // Always enforce a maximum limit to prevent abuse
            _ => query.Limit
        };

        // Apply sorting + pagination + projection
        var data = await items
            .OrderByDescending(i => i.CreatedAt)   // Pagination requires a deterministic order
            .Skip((page - 1) * limit)              // = SQL OFFSET
            .Take(limit)                           // = SQL LIMIT
            .Select(ToDto)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(total / (double)limit);

        return new PagedResponse<ShoppingItemDto>(
            Count: data.Count,
            Total: total,
            Page: page,
            TotalPages: totalPages,
            Data: data);
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
        Guid? categoryId;

        if (dto.CategoryId.HasValue)
        {
            var exists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
            if (!exists) return (null, "Category not found");
            categoryId = dto.CategoryId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(dto.CategoryName))
        {
            var category = await GetOrCreateCategoryAsync(dto.CategoryName.Trim());
            categoryId = category.Id;
        }
        else
        {
            return (null, "Either categoryId or categoryName must be provided");
        }

        var item = new ShoppingItem
        {
            Name = dto.Name,
            Quantity = dto.Quantity,
            CategoryId = categoryId
        };

        db.ShoppingItems.Add(item);
        await db.SaveChangesAsync();

        var result = await db.ShoppingItems
            .Where(i => i.Id == item.Id)
            .Select(ToDto)
            .FirstAsync();

        return (result, null);
    }

    private async Task<Category> GetOrCreateCategoryAsync(string name)
    {
        var existing = await db.Categories.FirstOrDefaultAsync(c => c.Name == name);
        if (existing is not null) return existing;

        var category = new Category { Name = name };
        db.Categories.Add(category);

        try
        {
            await db.SaveChangesAsync();
            return category;
        }
        catch (DbUpdateException)
        {
            // Race condition: another user just created the category between our "find" and "insert"
            // → DB's unique index blocks our attempt, throwing DbUpdateException

            // 1. Remove the failed entry from the change tracker to prevent it from affecting future operations
            db.Entry(category).State = EntityState.Detached;

            // 2. Re-query — the category might have been created by another user
            var winner = await db.Categories.FirstOrDefaultAsync(c => c.Name == name);
            if (winner is not null) return winner;

            // 3. Still doesn't exist → not a unique constraint violation, but another DB error → bubble up
            throw;
        }
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