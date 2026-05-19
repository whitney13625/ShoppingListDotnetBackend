using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data;
using ShoppingListApi.Dtos;
using ShoppingListApi.Models;

namespace ShoppingListApi.Endpoints;

public static class ShoppingItemEndpoints
{
    public static void MapShoppingItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/items").WithTags("ShoppingItems");

        // GET /api/items
        group.MapGet("/", async (AppDbContext db) =>
        {
            var items = await db.ShoppingItems
                .Select(i => new ShoppingItemDto(
                    i.Id, i.Name, i.Quantity, i.Purchased,
                    i.CategoryId,
                    i.Category != null ? i.Category.Name : null,
                    i.CreatedAt, i.UpdatedAt))
                .ToListAsync();
            return Results.Ok(items);
        });

        // GET /api/items/{id}
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var item = await db.ShoppingItems
                .Where(i => i.Id == id)
                .Select(i => new ShoppingItemDto(
                    i.Id, i.Name, i.Quantity, i.Purchased,
                    i.CategoryId,
                    i.Category != null ? i.Category.Name : null,
                    i.CreatedAt, i.UpdatedAt))
                .FirstOrDefaultAsync();

            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        // POST /api/items
        group.MapPost("/", async (CreateShoppingItemDto dto, AppDbContext db) =>
        {
            // 如果有給 CategoryId，先確認它存在
            if (dto.CategoryId.HasValue)
            {
                var exists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                if (!exists)
                    return Results.BadRequest(new { error = "Category not found" });
            }

            var item = new ShoppingItem
            {
                Name = dto.Name,
                Quantity = dto.Quantity,
                CategoryId = dto.CategoryId
            };

            db.ShoppingItems.Add(item);
            await db.SaveChangesAsync();

            //  Query the category name to return
            var result = await db.ShoppingItems
                .Where(i => i.Id == item.Id)
                .Select(i => new ShoppingItemDto(
                    i.Id, i.Name, i.Quantity, i.Purchased,
                    i.CategoryId,
                    i.Category != null ? i.Category.Name : null, // Category is navigation property, may be null
                    i.CreatedAt, i.UpdatedAt))
                .FirstAsync();

            return Results.Created($"/api/items/{item.Id}", result);
        });

        // PUT /api/items/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateShoppingItemDto dto, AppDbContext db) =>
        {
            var item = await db.ShoppingItems.FindAsync(id);
            if (item is null) return Results.NotFound();

            if (dto.CategoryId.HasValue)
            {
                var exists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                if (!exists)
                    return Results.BadRequest(new { error = "Category not found" });
            }

            if (dto.Name is not null) item.Name = dto.Name;
            if (dto.Quantity.HasValue) item.Quantity = dto.Quantity.Value;
            if (dto.Purchased.HasValue) item.Purchased = dto.Purchased.Value;
            if (dto.CategoryId.HasValue) item.CategoryId = dto.CategoryId.Value;
            item.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var result = await db.ShoppingItems
                .Where(i => i.Id == item.Id)
                .Select(i => new ShoppingItemDto(
                    i.Id, i.Name, i.Quantity, i.Purchased,
                    i.CategoryId,
                    i.Category != null ? i.Category.Name : null,
                    i.CreatedAt, i.UpdatedAt))
                .FirstAsync();

            return Results.Ok(result);
        });

        // DELETE /api/items/{id}
        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var item = await db.ShoppingItems.FindAsync(id);
            if (item is null) return Results.NotFound();

            db.ShoppingItems.Remove(item);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}