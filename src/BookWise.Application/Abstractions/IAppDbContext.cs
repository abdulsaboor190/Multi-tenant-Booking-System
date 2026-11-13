using BookWise.Domain.Tenants;
using BookWise.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace BookWise.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
