using FluentValidation;

namespace BookWise.Application.Auth;

public sealed record RegisterTenantRequest(
    string BusinessName, string Slug, string TimeZone, string OwnerEmail, string Password);

public sealed record RegisterTenantResponse(Guid TenantId, string Slug, Guid OwnerUserId);

public sealed class RegisterTenantRequestValidator : AbstractValidator<RegisterTenantRequest>
{
    private static readonly HashSet<string> ReservedSlugs =
    [
        "api", "admin", "www", "app", "dev", "health", "scalar", "openapi",
        "hangfire", "static", "assets", "login", "register", "auth", "b"
    ];

    public RegisterTenantRequestValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MinimumLength(2).MaximumLength(100);

        RuleFor(x => x.Slug)
            .NotEmpty().MinimumLength(3).MaximumLength(50)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
                .WithMessage("Slug may only contain lowercase letters, digits and single hyphens between words.")
            .Must(slug => slug is null || !ReservedSlugs.Contains(slug))
                .WithMessage("This slug is reserved.");

        RuleFor(x => x.TimeZone)
            .NotEmpty().MaximumLength(64)
            .Must(BeAKnownTimeZone)
                .WithMessage("Time zone must be a valid IANA id such as 'Asia/Karachi'.");

        RuleFor(x => x.OwnerEmail).NotEmpty().MaximumLength(254).EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(10).MaximumLength(128)
            .Matches("[A-Za-z]").WithMessage("Password must contain a letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }

    private static bool BeAKnownTimeZone(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(id);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
