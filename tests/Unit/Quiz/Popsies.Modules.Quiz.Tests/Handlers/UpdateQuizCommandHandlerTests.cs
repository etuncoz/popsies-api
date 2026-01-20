using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.UpdateQuiz;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Quiz.Tests.Unit.Handlers;

public sealed class UpdateQuizCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateQuizCommandHandler _handler;

    public UpdateQuizCommandHandlerTests()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new UpdateQuizCommandHandler(
            _quizRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldUpdateQuiz_WhenValidCommand()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            quizId,
            Guid.NewGuid(),
            "Original Title",
            "Original Description",
            null,
            QuizDifficulty.Easy).Value;

        var command = new UpdateQuizCommand(
            QuizId: quizId,
            Title: "Updated Title",
            Description: "Updated Description");

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quiz.Title.Should().Be("Updated Title");
        quiz.Description.Should().Be("Updated Description");
        _quizRepository.Received(1).Update(quiz);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuizNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var command = new UpdateQuizCommand(
            QuizId: quizId,
            Title: "Updated Title",
            Description: "Updated Description");

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
    public async Task Handle_ShouldReturnFailure_WhenQuizIsPublished()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            quizId,
            Guid.NewGuid(),
            "Original Title",
            "Original Description",
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

        var command = new UpdateQuizCommand(
            QuizId: quizId,
            Title: "Updated Title",
            Description: "Updated Description");

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only draft quizzes can be edited");
        _quizRepository.DidNotReceive().Update(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            quizId,
            Guid.NewGuid(),
            "Original Title",
            "Original Description",
            null,
            QuizDifficulty.Easy).Value;

        var command = new UpdateQuizCommand(
            QuizId: quizId,
            Title: "Updated Title",
            Description: "Updated Description");

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to update quiz");
    }
}
