namespace ShoppingListApi.Dtos;

public record ApiErrorResponse(
    bool Success,
    string Message,
    IReadOnlyList<ApiError>? Errors = null
);

public record PagedResponse<T>(
    int Count,        // counts per page
    int Total,        // total number of records matching the criteria
    int Page,         // current page number
    int TotalPages,   // total number of pages
    IReadOnlyList<T> Data
);

public record ApiError(string Field, string Message);