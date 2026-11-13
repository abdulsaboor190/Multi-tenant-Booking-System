using BookWise.Domain.Users;

namespace BookWise.UnitTests.Domain;

public class UserTests
{
    private static User CreateUser(string email = "owner@example.com")
        => new(Guid.NewGuid(), email, "hash", UserRole.Owner, DateTimeOffset.UtcNow);

    [Fact]
    public void Email_is_trimmed_and_lowercased()
    {
        var user = CreateUser("  Owner@Example.COM ");
        Assert.Equal("owner@example.com", user.Email);
    }

    [Fact]
    public void New_user_is_active_and_has_an_id()
    {
        var user = CreateUser();
        Assert.True(user.IsActive);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Deactivate_makes_the_user_inactive()
    {
        var user = CreateUser();
        user.Deactivate();
        Assert.False(user.IsActive);
    }
}
