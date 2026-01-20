using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.CreateCategory;
using Popsies.Modules.Quiz.Domain.Categories;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Quiz.Tests.Unit.Handlers;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CreateCategoryCommandHandler(
            _categoryRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenValidCommandWithoutParent()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Science",
            Description: "Science quizzes",
            ParentCategoryId: null);

        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _categoryRepository.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenValidCommandWithParent()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid();
        var parentCategory = Category.Create(parentCategoryId, "Science", "Science quizzes").Value;

        var command = new CreateCategoryCommand(
            Name: "Physics",
            Description: "Physics quizzes",
            ParentCategoryId: parentCategoryId);

        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _categoryRepository.GetByIdAsync(parentCategoryId, Arg.Any<CancellationToken>())
            .Returns(parentCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _categoryRepository.Received(1).AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNameAlreadyExists()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Science",
            Description: "Science quizzes",
            ParentCategoryId: null);

        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
        result.Error.Message.Should().Contain("already exists");
        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenParentCategoryNotFound()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid();
        var command = new CreateCategoryCommand(
            Name: "Physics",
            Description: "Physics quizzes",
            ParentCategoryId: parentCategoryId);

        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _categoryRepository.GetByIdAsync(parentCategoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenParentCategoryIsInactive()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid();
        var parentCategory = Category.Create(parentCategoryId, "Science", "Science quizzes").Value;
        parentCategory.Deactivate();

        var command = new CreateCategoryCommand(
            Name: "Physics",
            Description: "Physics quizzes",
            ParentCategoryId: parentCategoryId);

        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _categoryRepository.GetByIdAsync(parentCategoryId, Arg.Any<CancellationToken>())
            .Returns(parentCategory);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("inactive parent category");
        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "Science",
            Description: "Science quizzes",
            ParentCategoryId: null);

        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to save category");
    }
}
