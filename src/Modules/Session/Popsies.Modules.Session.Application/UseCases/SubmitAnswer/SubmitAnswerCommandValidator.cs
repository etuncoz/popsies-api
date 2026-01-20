using FluentValidation;

namespace Popsies.Modules.Session.Application.UseCases.SubmitAnswer;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required");

        RuleFor(x => x.PlayerId)
            .NotEmpty().WithMessage("Player ID is required");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Question ID is required");

        RuleFor(x => x.SelectedItemId)
            .NotEmpty().WithMessage("Selected item ID is required");

        RuleFor(x => x.TimeTakenSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Time taken cannot be negative");
    }
}
