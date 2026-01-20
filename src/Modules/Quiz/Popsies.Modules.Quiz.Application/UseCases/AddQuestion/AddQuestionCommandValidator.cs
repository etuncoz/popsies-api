using FluentValidation;

namespace Popsies.Modules.Quiz.Application.UseCases.AddQuestion;

public sealed class AddQuestionCommandValidator : AbstractValidator<AddQuestionCommand>
{
    public AddQuestionCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Quiz ID is required");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required")
            .MaximumLength(500).WithMessage("Question text must not exceed 500 characters");

        RuleFor(x => x.Sequence)
            .GreaterThan(0).WithMessage("Sequence must be greater than 0");

        RuleFor(x => x.PointValue)
            .GreaterThan(0).WithMessage("Point value must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Point value must not exceed 10000");

        RuleFor(x => x.TimeLimit)
            .GreaterThan(0).WithMessage("Time limit must be greater than 0")
            .LessThanOrEqualTo(300).WithMessage("Time limit must not exceed 300 seconds");
    }
}
