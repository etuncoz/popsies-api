using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using NSubstitute;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.UpdateCategory;
using Popsies.Modules.Quiz.Domain.Categories;
using Popsies.Shared.Abstractions.Persistence;

namespace Popsies.Modules.Quiz.Tests.Unit.Handlers;

public sealed class UpdateCategoryCommandHandlerTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new UpdateCategoryCommandHandler(
            _categoryRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory_WhenValidCommand()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(categoryId, "Science", "Science quizzes").Value;

        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Updated Science",
            Description: "Updated description");

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Updated Science");
        category.Description.Should().Be("Updated description");
        _categoryRepository.Received(1).Update(category);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory_WhenNameUnchanged()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(categoryId, "Science", "Science quizzes").Value;

        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Science", // Same name
            Description: "Updated description");

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _categoryRepository.DidNotReceive().ExistsByNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Updated Science",
            Description: "Updated description");

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
        _categoryRepository.DidNotReceive().Update(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNewNameAlreadyExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(categoryId, "Science", "Science quizzes").Value;

        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Math", // Different name that already exists
            Description: "Updated description");

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        _categoryRepository.ExistsByNameAsync("Math", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conflict");
        result.Error.Message.Should().Contain("already exists");
        _categoryRepository.DidNotReceive().Update(Arg.Any<Category>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseSaveFails()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(categoryId, "Science", "Science quizzes").Value;

        var command = new UpdateCategoryCommand(
            CategoryId: categoryId,
            Name: "Updated Science",
            Description: "Updated description");

        _categoryRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        _categoryRepository.ExistsByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Failed to update category");
    }
}
