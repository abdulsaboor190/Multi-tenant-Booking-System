using BookWise.Domain.Common;
using BookWise.Domain.Users;
using BookWise.Infrastructure.Persistence;
using BookWise.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookWise.IntegrationTests.Tenancy;

[Collection(IntegrationCollection.Name)]
public class TenantFilterTests(ApiFactory factory)
{
    [Fact]
    public async Task Queries_only_return_rows_of_the_current_tenant()
    {
        var a = await TestData.CreateTenantWithOwnerAsync(factory);
        var b = await TestData.CreateTenantWithOwnerAsync(factory);

        using var scope = TestData.CreateScope(factory, a.Tenant.Id);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var users = await db.Users.ToListAsync();

        Assert.Contains(users, u => u.Id == a.Owner.Id);
        Assert.DoesNotContain(users, u => u.Id == b.Owner.Id);
        Assert.All(users, u => Assert.Equal(a.Tenant.Id, u.TenantId));
    }

    [Fact]
    public async Task Queries_return_nothing_when_no_tenant_is_set()
    {
        await TestData.CreateTenantWithOwnerAsync(factory);

        using var scope = TestData.CreateScope(factory, null);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Empty(await db.Users.ToListAsync());
        Assert.NotEmpty(await db.Users.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task Saving_a_new_entity_stamps_the_current_tenant()
    {
        var a = await TestData.CreateTenantWithOwnerAsync(factory);

        using var scope = TestData.CreateScope(factory, a.Tenant.Id);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var staff = new User(
            Guid.Empty, "staff@example.com", "hash", UserRole.Staff, DateTimeOffset.UtcNow);

        db.Users.Add(staff);
        await db.SaveChangesAsync();

        Assert.Equal(a.Tenant.Id, staff.TenantId);
    }

    [Fact]
    public async Task Adding_an_entity_for_another_tenant_is_rejected()
    {
        var a = await TestData.CreateTenantWithOwnerAsync(factory);
        var b = await TestData.CreateTenantWithOwnerAsync(factory);

        using var scope = TestData.CreateScope(factory, a.Tenant.Id);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Users.Add(new User(
            b.Tenant.Id, "intruder@example.com", "hash", UserRole.Staff, DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<TenantIsolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Adding_an_entity_without_a_tenant_is_rejected()
    {
        await TestData.CreateTenantWithOwnerAsync(factory);

        using var scope = TestData.CreateScope(factory, null);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Users.Add(new User(
            Guid.NewGuid(), "orphan@example.com", "hash", UserRole.Staff, DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<TenantIsolationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Modifying_another_tenants_row_is_rejected()
    {
        var a = await TestData.CreateTenantWithOwnerAsync(factory);
        var b = await TestData.CreateTenantWithOwnerAsync(factory);

        using (var scope = TestData.CreateScope(factory, a.Tenant.Id))
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var bOwner = await db.Users.IgnoreQueryFilters().SingleAsync(u => u.Id == b.Owner.Id);

            bOwner.Deactivate();

            await Assert.ThrowsAsync<TenantIsolationException>(() => db.SaveChangesAsync());
        }

        using (var verifyScope = TestData.CreateScope(factory, b.Tenant.Id))
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var bOwner = await db.Users.SingleAsync(u => u.Id == b.Owner.Id);

            Assert.True(bOwner.IsActive);
        }
    }

    [Fact]
    public async Task Bulk_delete_cannot_touch_another_tenants_rows()
    {
        var a = await TestData.CreateTenantWithOwnerAsync(factory);
        var b = await TestData.CreateTenantWithOwnerAsync(factory);

        using (var scope = TestData.CreateScope(factory, a.Tenant.Id))
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var deleted = await db.Users.Where(u => u.Id == b.Owner.Id).ExecuteDeleteAsync();

            Assert.Equal(0, deleted);
        }

        using var verifyScope = TestData.CreateScope(factory, a.Tenant.Id);
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True(await verifyDb.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == b.Owner.Id));
    }

    [Fact]
    public async Task Refresh_tokens_are_also_filtered_by_tenant()
    {
        var a = await TestData.CreateTenantWithOwnerAsync(factory);
        var b = await TestData.CreateTenantWithOwnerAsync(factory);

        await TestData.AddRefreshTokenAsync(factory, a, $"hash-a-{Guid.NewGuid():N}");
        await TestData.AddRefreshTokenAsync(factory, b, $"hash-b-{Guid.NewGuid():N}");

        using var scope = TestData.CreateScope(factory, a.Tenant.Id);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tokens = await db.RefreshTokens.ToListAsync();

        Assert.Single(tokens);
        Assert.All(tokens, t => Assert.Equal(a.Tenant.Id, t.TenantId));
    }
}
