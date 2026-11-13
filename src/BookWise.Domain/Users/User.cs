using BookWise.Domain.Common;

namespace BookWise.Domain.Users;

public class User : ITenantOwned
{
    private User() { }

    public User(Guid tenantId, string email, string passwordHash, UserRole role, DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        TenantId = tenantId;
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public void Deactivate() => IsActive = false;
}
