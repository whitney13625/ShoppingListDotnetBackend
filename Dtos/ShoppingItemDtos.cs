namespace ShoppingListApi.Dtos;

public record ShoppingItemDto(
    Guid Id,
    string Name,
    int Quantity,
    bool Purchased,
    Guid? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateShoppingItemDto(
    string Name,
    int Quantity,
    Guid? CategoryId,
    string? CategoryName
);

public record UpdateShoppingItemDto(
    string? Name,
    int? Quantity,
    bool? Purchased,
    Guid? CategoryId
);