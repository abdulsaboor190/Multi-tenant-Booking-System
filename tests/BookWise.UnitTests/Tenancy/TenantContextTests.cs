using BookWise.Domain.Common;
using BookWise.Infrastructure.Tenancy;

namespace BookWise.UnitTests.Tenancy;

public class TenantContextTests
{
    [Fact]
    public void Tenant_is_null_until_it_is_set()
        => Assert.Null(new TenantContext().TenantId);

    [Fact]
    public void Require_throws_when_no_tenant_is_set()
        => Assert.Throws<InvalidOperationException>(() => new TenantContext().RequireTenantId());

    [Fact]
    public void Set_then_require_returns_the_tenant()
    {
        var context = new TenantContext();
        var id = Guid.NewGuid();
        context.SetTenant(id);
        Assert.Equal(id, context.RequireTenantId());
    }

    [Fact]
    public void Setting_the_same_tenant_twice_is_allowed()
    {
        var context = new TenantContext();
        var id = Guid.NewGuid();
        context.SetTenant(id);
        context.SetTenant(id);
        Assert.Equal(id, context.TenantId);
    }

    [Fact]
    public void Switching_to_a_different_tenant_is_rejected()
    {
        var context = new TenantContext();
        context.SetTenant(Guid.NewGuid());
        Assert.Throws<TenantIsolationException>(() => context.SetTenant(Guid.NewGuid()));
    }

    [Fact]
    public void Empty_tenant_id_is_rejected()
        => Assert.Throws<ArgumentException>(() => new TenantContext().SetTenant(Guid.Empty));
}
