using BookWise.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace BookWise.Infrastructure.Security;

public sealed class PasswordHasherService : IPasswordHasher
{
    private static readonly object Subject = new();

    private readonly PasswordHasher<object> _hasher = new();
    private readonly string _dummyHash;

    public PasswordHasherService()
        => _dummyHash = _hasher.HashPassword(Subject, Guid.NewGuid().ToString("N"));

    public string Hash(string password) => _hasher.HashPassword(Subject, password);

    public bool Verify(string? hash, string password)
    {
        if (hash is null)
        {
            // Spend the same time as a real check, so response time does not reveal whether the user exists.
            _hasher.VerifyHashedPassword(Subject, _dummyHash, password);
            return false;
        }

        return _hasher.VerifyHashedPassword(Subject, hash, password) != PasswordVerificationResult.Failed;
    }
}
