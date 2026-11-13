using BookWise.Application.Abstractions;
using BookWise.Domain.Common;

namespace BookWise.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public Guid RequireTenantId()
        => TenantId ?? throw new InvalidOperationException("No tenant is set for the current scope.");

    public void SetTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        if (TenantId is { } existing && existing != tenantId)
        {
            throw new TenantIsolationException("The tenant of the current scope cannot be changed once it is set.");
        }

        TenantId = tenantId;
    }
}
