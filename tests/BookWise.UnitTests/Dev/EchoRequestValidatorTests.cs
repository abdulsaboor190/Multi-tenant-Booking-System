// TEMPORARY (remove in Phase 2)
using BookWise.Application.Dev;

namespace BookWise.UnitTests.Dev;

public class EchoRequestValidatorTests
{
    private readonly EchoRequestValidator _validator = new();

    [Fact]
    public void Empty_name_is_invalid()
        => Assert.False(_validator.Validate(new EchoRequest("")).IsValid);

    [Fact]
    public void Name_longer_than_50_characters_is_invalid()
        => Assert.False(_validator.Validate(new EchoRequest(new string('a', 51))).IsValid);

    [Fact]
    public void Normal_name_is_valid()
        => Assert.True(_validator.Validate(new EchoRequest("Ali")).IsValid);
}
