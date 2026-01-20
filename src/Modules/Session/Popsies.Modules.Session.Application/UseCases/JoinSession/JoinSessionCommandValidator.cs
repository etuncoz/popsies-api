using FluentValidation;

namespace Popsies.Modules.Session.Application.UseCases.JoinSession;

public sealed class JoinSessionCommandValidator : AbstractValidator<JoinSessionCommand>
{
    public JoinSessionCommandValidator()
    {
        RuleFor(x => x.SessionCode)
            .NotEmpty().WithMessage("Session code is required")
            .Length(6).WithMessage("Session code must be exactly 6 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(50).WithMessage("Display name must not exceed 50 characters");
    }
}
