using BookWise.Infrastructure.Security;

namespace BookWise.UnitTests.Security;

public class PasswordHasherServiceTests
{
    private const string Password = "CorrectHorse9Battery";

    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void Hash_is_not_the_password()
        => Assert.NotEqual(Password, _hasher.Hash(Password));

    [Fact]
    public void Correct_password_verifies()
        => Assert.True(_hasher.Verify(_hasher.Hash(Password), Password));

    [Fact]
    public void Wrong_password_does_not_verify()
        => Assert.False(_hasher.Verify(_hasher.Hash(Password), "WrongHorse9Battery"));

    [Fact]
    public void Same_password_hashes_differently_each_time()
        => Assert.NotEqual(_hasher.Hash(Password), _hasher.Hash(Password));

    [Fact]
    public void Missing_hash_never_verifies()
        => Assert.False(_hasher.Verify(null, "anything"));
}
