using Microsoft.EntityFrameworkCore;

namespace BookWise.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Needed in Phase 4 for the no-overlapping-bookings constraint
        modelBuilder.HasPostgresExtension("btree_gist");

        base.OnModelCreating(modelBuilder);
    }
}
