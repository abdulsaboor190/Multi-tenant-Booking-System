using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookWise.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException badRequestException)
        {
            httpContext.Response.StatusCode = badRequestException.StatusCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = badRequestException.StatusCode,
                    Title = "The request could not be processed."
                }
            });
        }

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            logger.LogInformation("Validation failed on {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var validationProblem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };
            validationProblem.Extensions["errors"] = errors;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = validationProblem
            });
        }

        logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Something went wrong on our side. Please try again, and quote the traceId if you contact support."
            }
        });
    }
}
