using FluentValidation;

namespace Popsies.Modules.Quiz.Application.UseCases.DeleteQuiz;

public sealed class DeleteQuizCommandValidator : AbstractValidator<DeleteQuizCommand>
{
    public DeleteQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Quiz ID is required");
    }
}
