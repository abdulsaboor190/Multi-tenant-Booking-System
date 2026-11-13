using BookWise.Application.Auth;

namespace BookWise.UnitTests.Auth;

public class RegisterTenantRequestValidatorTests
{
    private static RegisterTenantRequest ValidRequest()
        => new("Glow Salon", "glow-salon", "Asia/Karachi", "owner@example.com", "CorrectHorse9Battery");

    private static FluentValidation.Results.ValidationResult Validate(RegisterTenantRequest request)
        => new RegisterTenantRequestValidator().Validate(request);

    [Fact]
    public void Valid_request_passes()
        => Assert.True(Validate(ValidRequest()).IsValid);

    [Fact]
    public void Slug_with_uppercase_is_invalid()
        => Assert.False(Validate(ValidRequest() with { Slug = "Glow-Salon" }).IsValid);

    [Fact]
    public void Slug_with_spaces_is_invalid()
        => Assert.False(Validate(ValidRequest() with { Slug = "glow salon" }).IsValid);

    [Fact]
    public void Slug_that_is_too_short_is_invalid()
        => Assert.False(Validate(ValidRequest() with { Slug = "gl" }).IsValid);

    [Fact]
    public void Reserved_slug_is_invalid()
        => Assert.False(Validate(ValidRequest() with { Slug = "admin" }).IsValid);

    [Fact]
    public void Unknown_time_zone_is_invalid()
        => Assert.False(Validate(ValidRequest() with { TimeZone = "Mars/Olympus" }).IsValid);

    [Fact]
    public void Invalid_email_is_invalid()
        => Assert.False(Validate(ValidRequest() with { OwnerEmail = "not-an-email" }).IsValid);

    [Fact]
    public void Short_password_is_invalid()
        => Assert.False(Validate(ValidRequest() with { Password = "Abc123" }).IsValid);

    [Fact]
    public void Password_without_a_digit_is_invalid()
        => Assert.False(Validate(ValidRequest() with { Password = "CorrectHorseBattery" }).IsValid);

    [Fact]
    public void Password_without_a_letter_is_invalid()
        => Assert.False(Validate(ValidRequest() with { Password = "1234567890" }).IsValid);
}
