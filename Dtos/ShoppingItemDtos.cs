using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Dtos;

public record ShoppingItemQuery
{
    public int? Page { get; init; } = 1;
    public int? Limit { get; init; } = 20;
    public Guid? CategoryId { get; init; }
    public bool? Purchased { get; init; }
    public string? Search { get; init; }
}

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
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be 1-100 characters")]
    string Name,
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    int Quantity,
    Guid? CategoryId,

    [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be 1-50 characters")]
    string? CategoryName
);

public record UpdateShoppingItemDto(
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be 1-100 characters")]
    string? Name,
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    int? Quantity,
    bool? Purchased,
    Guid? CategoryId
);