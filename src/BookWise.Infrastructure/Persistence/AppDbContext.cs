using System.Reflection;
using BookWise.Application.Abstractions;
using BookWise.Domain.Common;
using BookWise.Domain.Tenants;
using BookWise.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace BookWise.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Read by the global query filters. EF Core re-evaluates it for every context instance.
    // When no tenant is set this is null, which matches no rows (fail closed).
    private Guid? CurrentTenantId => tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Needed in Phase 4 for the no-overlapping-bookings constraint
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Every entity tagged ITenantOwned automatically gets the tenant filter.
        var applyFilter = typeof(AppDbContext).GetMethod(
            nameof(ApplyTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)!;

        var tenantOwnedTypes = modelBuilder.Model.GetEntityTypes()
            .Where(entityType => typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType))
            .Select(entityType => entityType.ClrType)
            .ToList();

        foreach (var clrType in tenantOwnedTypes)
        {
            applyFilter.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
        }

        base.OnModelCreating(modelBuilder);
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantOwned
        => modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceTenantRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnforceTenantRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnforceTenantRules()
    {
        var currentTenantId = tenantContext.TenantId;

        foreach (var entry in ChangeTracker.Entries<ITenantOwned>())
        {
            var entityName = entry.Metadata.ClrType.Name;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (currentTenantId is null)
                    {
                        throw new TenantIsolationException(
                            $"Cannot add a {entityName}: no tenant is set for the current scope.");
                    }

                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = currentTenantId.Value;
                    }
                    else if (entry.Entity.TenantId != currentTenantId.Value)
                    {
                        throw new TenantIsolationException(
                            $"Cannot add a {entityName} that belongs to another tenant.");
                    }

                    break;

                case EntityState.Modified or EntityState.Deleted:
                    if (currentTenantId is null || entry.Entity.TenantId != currentTenantId.Value)
                    {
                        throw new TenantIsolationException(
                            $"Cannot change or delete a {entityName} that belongs to another tenant.");
                    }

                    if (entry.Property(nameof(ITenantOwned.TenantId)).IsModified)
                    {
                        throw new TenantIsolationException(
                            $"The tenant of an existing {entityName} cannot be changed.");
                    }

                    break;
            }
        }
    }
}
