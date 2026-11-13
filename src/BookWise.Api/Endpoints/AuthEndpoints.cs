using BookWise.Api.Validation;
using BookWise.Application.Auth;

namespace BookWise.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register-tenant", async (
                RegisterTenantRequest request, IAuthService auth, CancellationToken cancellationToken) =>
            {
                var result = await auth.RegisterTenantAsync(request, cancellationToken);
                return Results.Json(result, statusCode: StatusCodes.Status201Created);
            })
            .WithValidation<RegisterTenantRequest>();
    }
}
