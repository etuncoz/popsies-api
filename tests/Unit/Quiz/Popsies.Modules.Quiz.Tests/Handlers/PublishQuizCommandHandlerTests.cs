using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using QuestionItem = Popsies.Modules.Quiz.Domain.Questions.QuestionItem;
using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.PublishQuiz;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Quiz.Tests.Unit.Handlers;

public sealed class PublishQuizCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PublishQuizCommandHandler _handler;

    public PublishQuizCommandHandlerTests()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new PublishQuizCommandHandler(
            _quizRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldPublishQuiz_WhenValidQuizWithQuestions()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = CreateQuizWithQuestions(quizId);
        var command = new PublishQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quiz.State.Should().Be(QuizState.Published);
        _quizRepository.Received(1).Update(quiz);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuizNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var command = new PublishQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns((Popsies.Modules.Quiz.Domain.Quizzes.Quiz?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Quiz.NotFound");
        _quizRepository.DidNotReceive().Update(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuizHasNoQuestions()
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

        var command = new PublishQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("at least 1 question");
        _quizRepository.DidNotReceive().Update(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuizAlreadyPublished()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = CreateQuizWithQuestions(quizId);
        quiz.Publish();

        var command = new PublishQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already published");
        _quizRepository.DidNotReceive().Update(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = CreateQuizWithQuestions(quizId);
        var command = new PublishQuizCommand(QuizId: quizId);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to publish quiz");
    }

    private static Popsies.Modules.Quiz.Domain.Quizzes.Quiz CreateQuizWithQuestions(Guid quizId)
    {
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
        return quiz;
    }
}
