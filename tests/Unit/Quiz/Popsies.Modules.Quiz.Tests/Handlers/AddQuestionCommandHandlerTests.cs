using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.AddQuestion;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Quiz.Tests.Unit.Handlers;

public sealed class AddQuestionCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AddQuestionCommandHandler _handler;

    public AddQuestionCommandHandlerTests()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _questionRepository = Substitute.For<IQuestionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new AddQuestionCommandHandler(
            _quizRepository,
            _questionRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldAddQuestion_WhenValidCommand()
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

        var command = new AddQuestionCommand(
            QuizId: quizId,
            Text: "What is the capital of France?",
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 30);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        quiz.Questions.Should().ContainSingle();
        await _questionRepository.Received(1).AddAsync(Arg.Any<Question>(), Arg.Any<CancellationToken>());
        _quizRepository.Received(1).Update(quiz);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuizNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var command = new AddQuestionCommand(
            QuizId: quizId,
            Text: "What is the capital of France?",
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 30);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns((Popsies.Modules.Quiz.Domain.Quizzes.Quiz?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Quiz.NotFound");
        await _questionRepository.DidNotReceive().AddAsync(Arg.Any<Question>(), Arg.Any<CancellationToken>());
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

        var existingQuestion = Question.Create(
            Guid.NewGuid(),
            quizId,
            "Existing question?",
            1).Value;

        // Add required items for validation (2-5 items, exactly 1 correct)
        var item1 = QuestionItem.Create(Guid.NewGuid(), existingQuestion.Id, "Option A", false, 1).Value;
        var item2 = QuestionItem.Create(Guid.NewGuid(), existingQuestion.Id, "Option B", false, 2).Value;
        existingQuestion.AddItem(item1);
        existingQuestion.AddItem(item2);
        existingQuestion.SetCorrectItem(item1.Id);

        quiz.AddQuestion(existingQuestion);
        quiz.Publish();

        var command = new AddQuestionCommand(
            QuizId: quizId,
            Text: "New question?",
            Sequence: 2,
            PointValue: 100,
            TimeLimit: 30);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only draft quizzes can be edited");
        await _questionRepository.DidNotReceive().AddAsync(Arg.Any<Question>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMaxQuestionsReached()
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

        // Add 10 questions (max)
        for (int i = 0; i < 10; i++)
        {
            var q = Question.Create(Guid.NewGuid(), quizId, $"Question {i}?", i).Value;
            quiz.AddQuestion(q);
        }

        var command = new AddQuestionCommand(
            QuizId: quizId,
            Text: "Extra question?",
            Sequence: 11,
            PointValue: 100,
            TimeLimit: 30);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("10 questions");
        await _questionRepository.DidNotReceive().AddAsync(Arg.Any<Question>(), Arg.Any<CancellationToken>());
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

        var command = new AddQuestionCommand(
            QuizId: quizId,
            Text: "Question?",
            Sequence: 1,
            PointValue: 100,
            TimeLimit: 30);

        _quizRepository.GetByIdAsync(quizId, Arg.Any<CancellationToken>())
            .Returns(quiz);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to add question");
    }
}
