using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Domain.Categories;
using Popsies.Modules.Quiz.Domain.Categories.Events;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Tests.Unit.Domain.Aggregates;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCategoryAndRaiseEvent()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var name = "Science";
        var description = "Science related quizzes";
        var parentCategoryId = Guid.NewGuid();

        // Act
        var result = Category.Create(categoryId, name, description, parentCategoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var category = result.Value;
        category.Id.Should().Be(categoryId);
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.ParentCategoryId.Should().Be(parentCategoryId);
        category.IsActive.Should().BeTrue();
        category.QuizCount.Should().Be(0);
        category.IconUrl.Should().BeNull();

        category.DomainEvents.Should().ContainSingle();
        category.DomainEvents.Should().ContainItemsAssignableTo<CategoryCreatedEvent>();

        var domainEvent = category.DomainEvents.First() as CategoryCreatedEvent;
        domainEvent!.CategoryId.Should().Be(categoryId);
        domainEvent.Name.Should().Be(name);
        domainEvent.ParentCategoryId.Should().Be(parentCategoryId);
    }

    [Fact]
    public void Create_WithNullParentCategoryId_ShouldSucceed()
    {
        // Arrange & Act
        var result = Category.Create(Guid.NewGuid(), "Science", "Science quizzes", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentCategoryId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldReturnFailure(string? name)
    {
        // Arrange & Act
        var result = Category.Create(Guid.NewGuid(), name!, "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("name cannot be empty");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("X")]
    public void Create_WithNameTooShort_ShouldReturnFailure(string name)
    {
        // Arrange & Act
        var result = Category.Create(Guid.NewGuid(), name, "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("2-50 characters");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        var name = new string('A', 51);

        // Act
        var result = Category.Create(Guid.NewGuid(), name, "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("2-50 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldReturnFailure(string? description)
    {
        // Arrange & Act
        var result = Category.Create(Guid.NewGuid(), "Science", description!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("description cannot be empty");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldReturnFailure()
    {
        // Arrange
        var description = new string('A', 201);

        // Act
        var result = Category.Create(Guid.NewGuid(), "Science", description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("200 characters");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateAndRaiseEvent()
    {
        // Arrange
        var category = CreateTestCategory();
        var newName = "Updated Science";
        var newDescription = "Updated description for science quizzes";

        // Act
        var result = category.UpdateDetails(newName, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);

        category.DomainEvents.Should().HaveCount(2); // CategoryCreated + CategoryUpdated
        category.DomainEvents.Last().Should().BeOfType<CategoryUpdatedEvent>();

        var updateEvent = category.DomainEvents.Last() as CategoryUpdatedEvent;
        updateEvent!.CategoryId.Should().Be(category.Id);
        updateEvent.Name.Should().Be(newName);
        updateEvent.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        var result = category.UpdateDetails("", "Valid Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("name cannot be empty");
    }

    [Fact]
    public void UpdateDetails_WithInvalidDescription_ShouldReturnFailure()
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        var result = category.UpdateDetails("Valid Name", "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("description cannot be empty");
    }

    [Theory]
    [InlineData("https://example.com/icon.png")]
    [InlineData("http://cdn.example.com/icons/science.svg")]
    public void SetIconUrl_WithValidUrl_ShouldSetIconUrl(string iconUrl)
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        var result = category.SetIconUrl(iconUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IconUrl.Should().Be(iconUrl);
    }

    [Fact]
    public void SetIconUrl_WithNull_ShouldClearIconUrl()
    {
        // Arrange
        var category = CreateTestCategory();
        category.SetIconUrl("https://example.com/icon.png");

        // Act
        var result = category.SetIconUrl(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IconUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/icon.png")]
    [InlineData("javascript:alert('xss')")]
    public void SetIconUrl_WithInvalidUrl_ShouldReturnFailure(string iconUrl)
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        var result = category.SetIconUrl(iconUrl);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Invalid icon URL");
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateAndRaiseEvent()
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        var result = category.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();

        category.DomainEvents.Should().Contain(e => e is CategoryDeactivatedEvent);
        var deactivateEvent = category.DomainEvents.OfType<CategoryDeactivatedEvent>().First();
        deactivateEvent.CategoryId.Should().Be(category.Id);
        deactivateEvent.Name.Should().Be(category.Name);
    }

    [Fact]
    public void Deactivate_WhenAlreadyDeactivated_ShouldReturnFailure()
    {
        // Arrange
        var category = CreateTestCategory();
        category.Deactivate();

        // Act
        var result = category.Deactivate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already deactivated");
    }

    [Fact]
    public void Activate_WhenDeactivated_ShouldActivate()
    {
        // Arrange
        var category = CreateTestCategory();
        category.Deactivate();

        // Act
        var result = category.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFailure()
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        var result = category.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already active");
    }

    [Fact]
    public void IncrementQuizCount_ShouldIncreaseCount()
    {
        // Arrange
        var category = CreateTestCategory();
        var initialCount = category.QuizCount;

        // Act
        category.IncrementQuizCount();

        // Assert
        category.QuizCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void IncrementQuizCount_CalledMultipleTimes_ShouldIncreaseCount()
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        category.IncrementQuizCount();
        category.IncrementQuizCount();
        category.IncrementQuizCount();

        // Assert
        category.QuizCount.Should().Be(3);
    }

    [Fact]
    public void DecrementQuizCount_WhenCountIsPositive_ShouldDecreaseCount()
    {
        // Arrange
        var category = CreateTestCategory();
        category.IncrementQuizCount();
        category.IncrementQuizCount();
        category.IncrementQuizCount();

        // Act
        category.DecrementQuizCount();

        // Assert
        category.QuizCount.Should().Be(2);
    }

    [Fact]
    public void DecrementQuizCount_WhenCountIsZero_ShouldRemainZero()
    {
        // Arrange
        var category = CreateTestCategory();

        // Act
        category.DecrementQuizCount();

        // Assert
        category.QuizCount.Should().Be(0);
    }

    private static Category CreateTestCategory()
    {
        var result = Category.Create(
            Guid.NewGuid(),
            "Science",
            "Science related quizzes",
            null);

        return result.Value;
    }
}
