using ShoppingListApi.Dtos;
using ShoppingListApi.Filters;
using ShoppingListApi.Services;

namespace ShoppingListApi.Endpoints;

public static class ShoppingItemEndpoints
{
    public static void MapShoppingItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/items").WithTags("ShoppingItems");

        // GET /api/items
        group.MapGet("/", async ([AsParameters] ShoppingItemQuery query, IShoppingItemService svc) =>
            Results.Ok(await svc.GetAllAsync(query)));


        // GET /api/items/{id}
        group.MapGet("/{id:guid}", async (Guid id, IShoppingItemService svc) =>
        {
            var item = await svc.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        // POST /api/items
        group.MapPost("/", async (CreateShoppingItemDto dto, IShoppingItemService svc) =>
        {
            var (item, error) = await svc.CreateAsync(dto);
            return error is not null
                ? Results.BadRequest(new { error })
                : Results.Created($"/api/items/{item!.Id}", item);
        })
        .AddEndpointFilter<ValidationFilter<CreateShoppingItemDto>>();

        // PUT /api/items/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateShoppingItemDto dto, IShoppingItemService svc) =>
        {
            var (item, error) = await svc.UpdateAsync(id, dto);
            return (item, error) switch
            {
                (not null, _) => Results.Ok(item),
                (null, not null) => Results.BadRequest(new { error }),
                (null, null) => Results.NotFound()
            };
        })
        .AddEndpointFilter<ValidationFilter<UpdateShoppingItemDto>>();

        // DELETE /api/items/{id}
        group.MapDelete("/{id:guid}", async (Guid id, IShoppingItemService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}