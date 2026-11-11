using FluentValidation;

namespace BookWise.Api.Validation;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
        {
            return await next(context);
        }

        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "A request body is required.");
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}

public static class ValidationExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
        => builder.AddEndpointFilter<ValidationFilter<T>>();
}
