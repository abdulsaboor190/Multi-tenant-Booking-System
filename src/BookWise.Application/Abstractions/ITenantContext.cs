namespace BookWise.Application.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }

    Guid RequireTenantId();

    void SetTenant(Guid tenantId);
}
