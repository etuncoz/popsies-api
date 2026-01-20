using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Categories;
using FluentAssertions;
using Popsies.Modules.Quiz.Domain.Questions;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Modules.Quiz.Domain.Quizzes.Events;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Quiz.Tests.Unit.Domain.Aggregates;

public class QuizTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateQuizAndRaiseEvent()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var title = "Test Quiz";
        var description = "This is a test quiz description";
        var categoryId = Guid.NewGuid();
        var difficulty = QuizDifficulty.Medium;

        // Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(quizId, creatorId, title, description, categoryId, difficulty);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var quiz = result.Value;
        quiz.Id.Should().Be(quizId);
        quiz.CreatorId.Should().Be(creatorId);
        quiz.Title.Should().Be(title);
        quiz.Description.Should().Be(description);
        quiz.CategoryId.Should().Be(categoryId);
        quiz.Difficulty.Should().Be(difficulty);
        quiz.State.Should().Be(QuizState.Draft);
        quiz.TotalTimesPlayed.Should().Be(0);
        quiz.AverageScore.Should().Be(0);
        quiz.Questions.Should().BeEmpty();

        quiz.DomainEvents.Should().ContainSingle();
        quiz.DomainEvents.Should().ContainItemsAssignableTo<QuizCreatedEvent>();

        var domainEvent = quiz.DomainEvents.First() as QuizCreatedEvent;
        domainEvent!.QuizId.Should().Be(quizId);
        domainEvent.CreatorId.Should().Be(creatorId);
        domainEvent.Title.Should().Be(title);
        domainEvent.Difficulty.Should().Be(difficulty);
    }

    [Fact]
    public void Create_WithNullCategoryId_ShouldSucceed()
    {
        // Arrange & Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Quiz",
            "Description",
            null,
            QuizDifficulty.Easy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CategoryId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyCreatorId_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "Test Quiz",
            "Description",
            null,
            QuizDifficulty.Easy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Creator ID cannot be empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        // Arrange & Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            title!,
            "Description",
            null,
            QuizDifficulty.Easy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("title cannot be empty");
    }

    [Theory]
    [InlineData("AB")]
    [InlineData("X")]
    public void Create_WithTitleTooShort_ShouldReturnFailure(string title)
    {
        // Arrange & Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            title,
            "Description",
            null,
            QuizDifficulty.Easy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("3-100 characters");
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldReturnFailure()
    {
        // Arrange
        var title = new string('A', 101);

        // Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            title,
            "Description",
            null,
            QuizDifficulty.Easy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("3-100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldReturnFailure(string? description)
    {
        // Arrange & Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Quiz",
            description!,
            null,
            QuizDifficulty.Easy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("description cannot be empty");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldReturnFailure()
    {
        // Arrange
        var description = new string('A', 501);

        // Act
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Quiz",
            description,
            null,
            QuizDifficulty.Easy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("500 characters");
    }

    [Fact]
    public void UpdateDetails_WhenInDraftState_ShouldUpdateAndRaiseEvent()
    {
        // Arrange
        var quiz = CreateTestQuiz();
        var newTitle = "Updated Quiz Title";
        var newDescription = "This is the updated description";

        // Act
        var result = quiz.UpdateDetails(newTitle, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quiz.Title.Should().Be(newTitle);
        quiz.Description.Should().Be(newDescription);

        quiz.DomainEvents.Should().HaveCount(2); // QuizCreated + QuizUpdated
        quiz.DomainEvents.Last().Should().BeOfType<QuizUpdatedEvent>();

        var updateEvent = quiz.DomainEvents.Last() as QuizUpdatedEvent;
        updateEvent!.QuizId.Should().Be(quiz.Id);
        updateEvent.Title.Should().Be(newTitle);
        updateEvent.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateDetails_WhenPublished_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuizWithQuestions();
        quiz.Publish();

        // Act
        var result = quiz.UpdateDetails("New Title", "New Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only draft quizzes can be edited");
    }

    [Fact]
    public void UpdateDetails_WithInvalidTitle_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuiz();

        // Act
        var result = quiz.UpdateDetails("", "Valid Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("title cannot be empty");
    }

    [Fact]
    public void Publish_WithValidQuizAndQuestions_ShouldPublishAndRaiseEvent()
    {
        // Arrange
        var quiz = CreateTestQuizWithQuestions();

        // Act
        var result = quiz.Publish();

        // Assert
        result.IsSuccess.Should().BeTrue();
        quiz.State.Should().Be(QuizState.Published);

        quiz.DomainEvents.Should().Contain(e => e is QuizPublishedEvent);
        var publishEvent = quiz.DomainEvents.OfType<QuizPublishedEvent>().First();
        publishEvent.QuizId.Should().Be(quiz.Id);
        publishEvent.Title.Should().Be(quiz.Title);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuizWithQuestions();
        quiz.Publish();

        // Act
        var result = quiz.Publish();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already published");
    }

    [Fact]
    public void Publish_WithoutQuestions_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuiz();

        // Act
        var result = quiz.Publish();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("at least 1 question");
    }

    [Fact]
    public void Archive_WhenPublished_ShouldArchiveAndRaiseEvent()
    {
        // Arrange
        var quiz = CreateTestQuizWithQuestions();
        quiz.Publish();

        // Act
        var result = quiz.Archive();

        // Assert
        result.IsSuccess.Should().BeTrue();
        quiz.State.Should().Be(QuizState.Archived);

        quiz.DomainEvents.Should().Contain(e => e is QuizArchivedEvent);
        var archiveEvent = quiz.DomainEvents.OfType<QuizArchivedEvent>().First();
        archiveEvent.QuizId.Should().Be(quiz.Id);
        archiveEvent.Title.Should().Be(quiz.Title);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuizWithQuestions();
        quiz.Publish();
        quiz.Archive();

        // Act
        var result = quiz.Archive();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already archived");
    }

    [Fact]
    public void AddQuestion_WhenInDraftState_ShouldAddQuestion()
    {
        // Arrange
        var quiz = CreateTestQuiz();
        var question = CreateTestQuestion(quiz.Id);

        // Act
        var result = quiz.AddQuestion(question);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quiz.Questions.Should().ContainSingle();
        quiz.Questions.First().Should().Be(question);
    }

    [Fact]
    public void AddQuestion_WhenPublished_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuizWithQuestions();
        quiz.Publish();
        var newQuestion = CreateTestQuestion(quiz.Id, 2);

        // Act
        var result = quiz.AddQuestion(newQuestion);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only draft quizzes can be edited");
    }

    [Fact]
    public void AddQuestion_WhenMaxQuestionsReached_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuiz();
        for (int i = 0; i < 10; i++)
        {
            quiz.AddQuestion(CreateTestQuestion(quiz.Id, i));
        }
        var extraQuestion = CreateTestQuestion(quiz.Id, 11);

        // Act
        var result = quiz.AddQuestion(extraQuestion);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("10 questions");
    }

    [Fact]
    public void AddQuestion_WithDuplicateId_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuiz();
        var questionId = Guid.NewGuid();
        var question1 = Question.Create(questionId, quiz.Id, "Question 1?", 1).Value;
        var question2 = Question.Create(questionId, quiz.Id, "Question 2?", 2).Value;
        quiz.AddQuestion(question1);

        // Act
        var result = quiz.AddQuestion(question2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already exists");
    }

    [Fact]
    public void RemoveQuestion_WhenQuestionExists_ShouldRemoveQuestion()
    {
        // Arrange
        var quiz = CreateTestQuiz();
        var question = CreateTestQuestion(quiz.Id);
        quiz.AddQuestion(question);

        // Act
        var result = quiz.RemoveQuestion(question.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quiz.Questions.Should().BeEmpty();
    }

    [Fact]
    public void RemoveQuestion_WhenQuestionNotFound_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuiz();

        // Act
        var result = quiz.RemoveQuestion(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("not found");
    }

    [Fact]
    public void RemoveQuestion_WhenPublished_ShouldReturnFailure()
    {
        // Arrange
        var quiz = CreateTestQuizWithQuestions();
        var questionId = quiz.Questions.First().Id;
        quiz.Publish();

        // Act
        var result = quiz.RemoveQuestion(questionId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Only draft quizzes can be edited");
    }

    [Fact]
    public void UpdateStatistics_WithValidValues_ShouldUpdateStatistics()
    {
        // Arrange
        var quiz = CreateTestQuiz();

        // Act
        quiz.UpdateStatistics(timesPlayed: 100, averageScore: 85.5);

        // Assert
        quiz.TotalTimesPlayed.Should().Be(100);
        quiz.AverageScore.Should().Be(85.5);
    }

    [Fact]
    public void UpdateStatistics_WithNegativeValues_ShouldSetToZero()
    {
        // Arrange
        var quiz = CreateTestQuiz();

        // Act
        quiz.UpdateStatistics(timesPlayed: -10, averageScore: -5.5);

        // Assert
        quiz.TotalTimesPlayed.Should().Be(0);
        quiz.AverageScore.Should().Be(0);
    }

    private static Popsies.Modules.Quiz.Domain.Quizzes.Quiz CreateTestQuiz()
    {
        var quizId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var result = Popsies.Modules.Quiz.Domain.Quizzes.Quiz.Create(
            quizId,
            creatorId,
            "Test Quiz",
            "This is a test quiz description",
            null,
            QuizDifficulty.Medium);

        return result.Value;
    }

    private static Popsies.Modules.Quiz.Domain.Quizzes.Quiz CreateTestQuizWithQuestions()
    {
        var quiz = CreateTestQuiz();
        var question = CreateTestQuestion(quiz.Id);
        quiz.AddQuestion(question);
        return quiz;
    }

    private static Question CreateTestQuestion(Guid quizId, int sequence = 1)
    {
        var questionResult = Question.Create(
            Guid.NewGuid(),
            quizId,
            $"What is the answer to question {sequence}?",
            sequence,
            100,
            30);

        var question = questionResult.Value;

        // Add required items for validation (2-5 items, exactly 1 correct)
        var item1 = QuestionItem.Create(Guid.NewGuid(), question.Id, "Option A", false, 1).Value;
        var item2 = QuestionItem.Create(Guid.NewGuid(), question.Id, "Option B", false, 2).Value;
        question.AddItem(item1);
        question.AddItem(item2);
        question.SetCorrectItem(item1.Id); // Mark first item as correct

        return question;
    }
}
