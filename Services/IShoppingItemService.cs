using ShoppingListApi.Dtos;

namespace ShoppingListApi.Services;

public interface IShoppingItemService
{
    Task<PagedResponse<ShoppingItemDto>> GetAllAsync(ShoppingItemQuery query);
    Task<ShoppingItemDto?> GetByIdAsync(Guid id);
    Task<(ShoppingItemDto? Item, string? Error)> CreateAsync(CreateShoppingItemDto dto);
    Task<(ShoppingItemDto? Item, string? Error)> UpdateAsync(Guid id, UpdateShoppingItemDto dto);
    Task<bool> DeleteAsync(Guid id);
}