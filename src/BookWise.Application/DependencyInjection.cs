using BookWise.Application.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookWise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
