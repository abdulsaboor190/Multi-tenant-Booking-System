using BookWise.Domain.Common;

namespace BookWise.Domain.Users;

public class RefreshToken : ITenantOwned
{
    private RefreshToken() { }

    public RefreshToken(
        Guid tenantId, Guid userId, Guid familyId, string tokenHash,
        DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        Id = Guid.CreateVersion7();
        TenantId = tenantId;
        UserId = userId;
        FamilyId = familyId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
