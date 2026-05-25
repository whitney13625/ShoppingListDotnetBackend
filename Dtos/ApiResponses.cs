namespace ShoppingListApi.Dtos;

public record ApiErrorResponse(
    bool Success,
    string Message,
    IReadOnlyList<ApiError>? Errors = null
);

public record ApiError(string Field, string Message);