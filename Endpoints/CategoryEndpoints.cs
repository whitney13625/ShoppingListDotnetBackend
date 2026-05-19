using Microsoft.EntityFrameworkCore;
using ShoppingListApi.Data;
using ShoppingListApi.Dtos;
using ShoppingListApi.Models;

namespace ShoppingListApi.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app) // Extension method to register category endpoints
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        // GET /api/categories
        group.MapGet("/", async (AppDbContext db) =>
        {
            var categories = await db.Categories
                .Select(c => new CategoryDto(
                    c.Id, c.Name, c.Description, c.Icon, c.CreatedAt, c.UpdatedAt))
                .ToListAsync();
            return Results.Ok(categories);
        });

        // GET /api/categories/{id}
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var category = await db.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto(
                    c.Id, c.Name, c.Description, c.Icon, c.CreatedAt, c.UpdatedAt))
                .FirstOrDefaultAsync();

            return category is null ? Results.NotFound() : Results.Ok(category);
        });

        // POST /api/categories
        group.MapPost("/", async (CreateCategoryDto dto, AppDbContext db) =>
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Icon = dto.Icon
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var result = new CategoryDto(
                category.Id, category.Name, category.Description,
                category.Icon, category.CreatedAt, category.UpdatedAt);

            return Results.Created($"/api/categories/{category.Id}", result);
        });

        // PUT /api/categories/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryDto dto, AppDbContext db) =>
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null) return Results.NotFound();

            if (dto.Name is not null) category.Name = dto.Name;
            if (dto.Description is not null) category.Description = dto.Description;
            if (dto.Icon is not null) category.Icon = dto.Icon;
            category.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.Ok(new CategoryDto(
                category.Id, category.Name, category.Description,
                category.Icon, category.CreatedAt, category.UpdatedAt));
        });

        // DELETE /api/categories/{id}
        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var category = await db.Categories.FindAsync(id);
            if (category is null) return Results.NotFound();

            db.Categories.Remove(category);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}