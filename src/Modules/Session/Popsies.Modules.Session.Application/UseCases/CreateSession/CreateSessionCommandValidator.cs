using FluentValidation;

namespace Popsies.Modules.Session.Application.UseCases.CreateSession;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Quiz ID is required");

        RuleFor(x => x.HostId)
            .NotEmpty().WithMessage("Host ID is required");

        RuleFor(x => x.MaxPlayers)
            .GreaterThanOrEqualTo(2).WithMessage("Max players must be at least 2")
            .LessThanOrEqualTo(100).WithMessage("Max players cannot exceed 100");
    }
}
