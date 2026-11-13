using BookWise.Application.Abstractions;
using BookWise.Domain.Tenants;
using BookWise.Domain.Users;
using BookWise.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BookWise.IntegrationTests.Infrastructure;

public sealed record SeededTenant(Tenant Tenant, User Owner);

public static class TestData
{
    /// Creates a new service scope, optionally with the tenant already set (like a real request would).
    public static IServiceScope CreateScope(ApiFactory factory, Guid? tenantId)
    {
        var scope = factory.Services.CreateScope();

        if (tenantId is { } id)
        {
            scope.ServiceProvider.GetRequiredService<ITenantContext>().SetTenant(id);
        }

        return scope;
    }

    public static async Task<SeededTenant> CreateTenantWithOwnerAsync(
        ApiFactory factory, string? slug = null, string email = "owner@example.com")
    {
        using var scope = CreateScope(factory, null);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        var now = DateTimeOffset.UtcNow;
        var tenant = new Tenant("Test Business", slug ?? $"t-{Guid.NewGuid():N}", "Asia/Karachi", now);
        tenantContext.SetTenant(tenant.Id);

        var owner = new User(tenant.Id, email, "not-a-real-hash", UserRole.Owner, now);

        db.Tenants.Add(tenant);
        db.Users.Add(owner);
        await db.SaveChangesAsync();

        return new SeededTenant(tenant, owner);
    }

    public static async Task<RefreshToken> AddRefreshTokenAsync(
        ApiFactory factory, SeededTenant seeded, string tokenHash, DateTimeOffset? expiresAt = null)
    {
        using var scope = CreateScope(factory, seeded.Tenant.Id);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTimeOffset.UtcNow;
        var token = new RefreshToken(
            seeded.Tenant.Id, seeded.Owner.Id, Guid.NewGuid(), tokenHash, now, expiresAt ?? now.AddDays(7));

        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        return token;
    }
}
