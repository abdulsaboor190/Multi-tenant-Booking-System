// TEMPORARY (remove in Phase 2)
using FluentValidation;

namespace BookWise.Application.Dev;

public sealed record EchoRequest(string Name);

public sealed class EchoRequestValidator : AbstractValidator<EchoRequest>
{
    public EchoRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}
