using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.DeleteQuiz;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Quiz.Tests.Unit.Handlers;

public sealed class DeleteQuizCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteQuizCommandHandler _handler;

    public DeleteQuizCommandHandlerTests()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new DeleteQuizCommandHandler(
            _quizRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldDeleteQuiz_WhenQuizIsDraft()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            quizId,
            Guid.NewGuid(),
            "Quiz",
            "Description",
            null,
            QuizDifficulty.Easy).Value;

        var command = new DeleteQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _quizRepository.Received(1).Remove(quiz);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuizNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var command = new DeleteQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns((Popsies.Modules.Quiz.Domain.Quizzes.Quiz?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Quiz.NotFound");
        _quizRepository.DidNotReceive().Remove(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuizIsPublished()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            quizId,
            Guid.NewGuid(),
            "Quiz",
            "Description",
            null,
            QuizDifficulty.Easy).Value;

        var question = Question.Create(
            Guid.NewGuid(),
            quizId,
            "Question?",
            1).Value;

        // Add required items for validation (2-5 items, exactly 1 correct)
        var item1 = QuestionItem.Create(Guid.NewGuid(), question.Id, "Option A", false, 1).Value;
        var item2 = QuestionItem.Create(Guid.NewGuid(), question.Id, "Option B", false, 2).Value;
        question.AddItem(item1);
        question.AddItem(item2);
        question.SetCorrectItem(item1.Id);

        quiz.AddQuestion(question);
        quiz.Publish();

        var command = new DeleteQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Published quizzes cannot be deleted");
        _quizRepository.DidNotReceive().Remove(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            quizId,
            Guid.NewGuid(),
            "Quiz",
            "Description",
            null,
            QuizDifficulty.Easy).Value;

        var command = new DeleteQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to delete quiz");
    }
}
