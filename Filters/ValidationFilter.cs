using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ShoppingListApi.Dtos;

namespace ShoppingListApi.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // Find the argument of type T (the DTO to validate)
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null) return await next(context);

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            argument,
            new ValidationContext(argument),
            results,
            validateAllProperties: true);

        if (!isValid)
        {
            // Conform to pre-defined json contract, instead of RFC 7807 Standard
            var errors = results
                .Where(r => r.MemberNames.Any())
                .SelectMany(r => r.MemberNames.Select(member => new ApiError(
                    Field: JsonNamingPolicy.CamelCase.ConvertName(member),  // "Name" → "name"
                    Message: r.ErrorMessage ?? "Invalid")))
                .ToList();

            return Results.BadRequest(new ApiErrorResponse(
                    Success: false,
                    Message: "Validation failed",
                    Errors: errors));
        }

        return await next(context);
    }
}