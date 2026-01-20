using FluentValidation;

namespace Popsies.Modules.Session.Application.UseCases.StartSession;

public sealed class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");
    }
}
