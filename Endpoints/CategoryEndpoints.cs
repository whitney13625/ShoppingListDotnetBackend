using ShoppingListApi.Dtos;
using ShoppingListApi.Filters;
using ShoppingListApi.Services;

namespace ShoppingListApi.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app) // Extension method to register category endpoints
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        // GET /api/categories
        group.MapGet("/", async (ICategoryService svc) =>
            Results.Ok(await svc.GetAllAsync()));

        // GET /api/categories/{id}
        group.MapGet("/{id:guid}", async (Guid id, ICategoryService svc) =>
        {
            var category = await svc.GetByIdAsync(id);
            return category is null ? Results.NotFound() : Results.Ok(category);
        });

        // POST /api/categories
        group.MapPost("/", async (CreateCategoryDto dto, ICategoryService svc) =>
        {
            var category = await svc.CreateAsync(dto);
            return Results.Created($"/api/categories/{category.Id}", category);
        })
        .AddEndpointFilter<ValidationFilter<CreateCategoryDto>>();

        // PUT /api/categories/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryDto dto, ICategoryService svc) =>
        {
            var category = await svc.UpdateAsync(id, dto);
            return category is null ? Results.NotFound() : Results.Ok(category);
        })
        .AddEndpointFilter<ValidationFilter<UpdateCategoryDto>>();

        // DELETE /api/categories/{id}
        group.MapDelete("/{id:guid}", async (Guid id, ICategoryService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}