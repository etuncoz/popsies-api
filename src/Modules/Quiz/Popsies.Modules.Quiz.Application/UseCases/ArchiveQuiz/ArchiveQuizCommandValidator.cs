using FluentValidation;

namespace Popsies.Modules.Quiz.Application.UseCases.ArchiveQuiz;

public sealed class ArchiveQuizCommandValidator : AbstractValidator<ArchiveQuizCommand>
{
    public ArchiveQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Quiz ID is required");
    }
}
