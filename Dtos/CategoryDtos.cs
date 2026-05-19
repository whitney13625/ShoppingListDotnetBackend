namespace ShoppingListApi.Dtos;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string? Icon,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateCategoryDto(
    string Name,
    string? Description,
    string? Icon
);

public record UpdateCategoryDto(
    string? Name,
    string? Description,
    string? Icon
);