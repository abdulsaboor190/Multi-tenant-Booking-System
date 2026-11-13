using BookWise.Domain.Users;

namespace BookWise.UnitTests.Domain;

public class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static RefreshToken Create(DateTimeOffset expiresAt)
        => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "hash", Now, expiresAt);

    [Fact]
    public void Token_is_active_before_it_expires()
        => Assert.True(Create(Now.AddDays(7)).IsActive(Now.AddMinutes(1)));

    [Fact]
    public void Token_is_not_active_after_it_expires()
        => Assert.False(Create(Now.AddDays(7)).IsActive(Now.AddDays(7)));

    [Fact]
    public void Revoked_token_is_not_active()
    {
        var token = Create(Now.AddDays(7));
        token.Revoke(Now.AddMinutes(1));
        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive(Now.AddMinutes(2)));
    }

    [Fact]
    public void Revoking_twice_keeps_the_first_timestamp()
    {
        var token = Create(Now.AddDays(7));
        token.Revoke(Now.AddMinutes(1));
        token.Revoke(Now.AddMinutes(30));
        Assert.Equal(Now.AddMinutes(1), token.RevokedAt);
    }
}
