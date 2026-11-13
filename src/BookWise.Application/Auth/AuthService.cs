using BookWise.Application.Abstractions;
using BookWise.Application.Common;
using BookWise.Domain.Tenants;
using BookWise.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace BookWise.Application.Auth;

public sealed class AuthService(
    IAppDbContext db,
    ITenantContext tenantContext,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider) : IAuthService
{
    public async Task<RegisterTenantResponse> RegisterTenantAsync(
        RegisterTenantRequest request, CancellationToken cancellationToken)
    {
        var slug = request.Slug;

        if (await db.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            throw new ConflictException($"The slug '{slug}' is already taken.");
        }

        var now = timeProvider.GetUtcNow();
        var tenant = new Tenant(request.BusinessName.Trim(), slug, request.TimeZone.Trim(), now);

        // From here on, everything we write belongs to the new tenant.
        tenantContext.SetTenant(tenant.Id);

        var owner = new User(
            tenant.Id, request.OwnerEmail, passwordHasher.Hash(request.Password), UserRole.Owner, now);

        db.Tenants.Add(tenant);
        db.Users.Add(owner);

        try
        {
            // One SaveChanges is one database transaction: the tenant and its owner are saved together or not at all.
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            // Two people registered the same slug at the same moment; the unique index decided the winner.
            throw new ConflictException($"The slug '{slug}' is already taken.");
        }

        return new RegisterTenantResponse(tenant.Id, tenant.Slug, owner.Id);
    }
}
