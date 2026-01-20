using FluentValidation;

namespace Popsies.Modules.Quiz.Application.UseCases.PublishQuiz;

public sealed class PublishQuizCommandValidator : AbstractValidator<PublishQuizCommand>
{
    public PublishQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Quiz ID is required");
    }
}
