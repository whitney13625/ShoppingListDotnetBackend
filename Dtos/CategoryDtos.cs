using System.ComponentModel.DataAnnotations;

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
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be 1-50 characters")]
    string Name,

    [StringLength(200, ErrorMessage = "Description must be at most 200 characters")]
    string? Description,
    string? Icon
);

public record UpdateCategoryDto(
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be 1-50 characters")]
    string? Name,
    
    [StringLength(200, ErrorMessage = "Description must be at most 200 characters")]
    string? Description,
    string? Icon
);