using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.CreateQuiz;
using Popsies.Modules.Quiz.Domain.Categories;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Tests.Unit.Handlers;

public sealed class CreateQuizCommandHandlerTests
{
    private readonly IQuizRepository _quizRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateQuizCommandHandler _handler;

    public CreateQuizCommandHandlerTests()
    {
        _quizRepository = Substitute.For<IQuizRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CreateQuizCommandHandler(
            _quizRepository,
            _categoryRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCreateQuiz_WhenValidCommandWithCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(categoryId, "Science", "Science quizzes").Value;
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Science Quiz",
            Description: "Test your science knowledge",
            CategoryId: categoryId,
            Difficulty: QuizDifficulty.Medium);

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _quizRepository.Received(1).AddAsync(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateQuiz_WhenValidCommandWithoutCategory()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "General Quiz",
            Description: "General knowledge quiz",
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _quizRepository.Received(1).AddAsync(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Science Quiz",
            Description: "Test your science knowledge",
            CategoryId: categoryId,
            Difficulty: QuizDifficulty.Medium);

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
        await _quizRepository.DidNotReceive().AddAsync(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryIsInactive()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(categoryId, "Science", "Science quizzes").Value;
        category.Deactivate();

        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Science Quiz",
            Description: "Test your science knowledge",
            CategoryId: categoryId,
            Difficulty: QuizDifficulty.Medium);

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("inactive category");
        await _quizRepository.DidNotReceive().AddAsync(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCreatorIdIsEmpty()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.Empty,
            Title: "Quiz",
            Description: "Description",
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Creator ID cannot be empty");
        await _quizRepository.DidNotReceive().AddAsync(Arg.Any<Popsies.Modules.Quiz.Domain.Quizzes.Quiz>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var command = new CreateQuizCommand(
            CreatorId: Guid.NewGuid(),
            Title: "Quiz",
            Description: "Description",
            CategoryId: null,
            Difficulty: QuizDifficulty.Easy);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database connection failed")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to save quiz");
    }
}
