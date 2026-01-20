using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Popsies.Modules.Quiz.Application.Common.Repositories;
using Popsies.Modules.Quiz.Application.UseCases.AddQuestion;
using Popsies.Modules.Quiz.Application.UseCases.ArchiveQuiz;
using Popsies.Modules.Quiz.Application.UseCases.CreateQuiz;
using Popsies.Modules.Quiz.Application.UseCases.DeleteQuiz;
using Popsies.Modules.Quiz.Application.UseCases.PublishQuiz;
using Popsies.Modules.Quiz.Application.UseCases.UpdateQuiz;
using Popsies.Modules.Quiz.Contracts.Requests;
using Popsies.Modules.Quiz.Contracts.Responses;
using Popsies.Modules.Quiz.Domain.Quizzes;
using Popsies.Shared.Abstractions.Commands;
using Popsies.Shared.Abstractions.Users;

namespace Popsies.Modules.Quiz.Api.Controllers;

/// <summary>
/// Quiz management endpoints
/// </summary>
[Authorize]
[ApiController]
[Route("api/quizzes")]
public sealed class QuizController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQuizRepository _quizRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuizController(
        ICommandDispatcher commandDispatcher,
        IQuizRepository quizRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _commandDispatcher = commandDispatcher;
        _quizRepository = quizRepository;
        _currentUserProvider = currentUserProvider;
    }

    /// <summary>
    /// Create a new quiz
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateQuizResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateQuizResponse>> CreateQuiz(
        [FromBody] CreateQuizRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();

        var command = new CreateQuizCommand(
            currentUser.Id,
            request.Title,
            request.Description,
            request.CategoryId,
            (QuizDifficulty)request.Difficulty);

        var result = await _commandDispatcher.SendAsync<CreateQuizCommand, Guid>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        var quizId = result.Value;
        var quiz = await _quizRepository.GetByIdAsync(quizId, cancellationToken);

        var response = new CreateQuizResponse(
            quizId,
            quiz!.Title,
            quiz.State.ToString(),
            "Quiz created successfully");

        return CreatedAtAction(nameof(CreateQuiz), new { id = quizId }, response);
    }

    /// <summary>
    /// Update an existing quiz
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateQuiz(
        Guid id,
        [FromBody] UpdateQuizRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateQuizCommand(
            id,
            request.Title,
            request.Description);

        var result = await _commandDispatcher.SendAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Quiz.NotFound" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { message = "Quiz updated successfully" });
    }

    /// <summary>
    /// Publish a quiz
    /// </summary>
    [HttpPost("{id}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishQuiz(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PublishQuizCommand(id);

        var result = await _commandDispatcher.SendAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Quiz.NotFound" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { message = "Quiz published successfully" });
    }

    /// <summary>
    /// Archive a quiz
    /// </summary>
    [HttpPost("{id}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ArchiveQuiz(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveQuizCommand(id);

        var result = await _commandDispatcher.SendAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Quiz.NotFound" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { message = "Quiz archived successfully" });
    }

    /// <summary>
    /// Delete a quiz
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuiz(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteQuizCommand(id);

        var result = await _commandDispatcher.SendAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Quiz.NotFound" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(new { message = "Quiz deleted successfully" });
    }

    /// <summary>
    /// Add a question to a quiz
    /// </summary>
    [HttpPost("{id}/questions")]
    [ProducesResponseType(typeof(AddQuestionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddQuestionResponse>> AddQuestion(
        Guid id,
        [FromBody] AddQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddQuestionCommand(
            id,
            request.Text,
            request.Sequence,
            request.PointValue,
            request.TimeLimit);

        var result = await _commandDispatcher.SendAsync<AddQuestionCommand, Guid>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "Quiz.NotFound" => NotFound(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        var questionId = result.Value;
        var response = new AddQuestionResponse(
            questionId,
            id,
            request.Text,
            request.Sequence,
            "Question added successfully");

        return CreatedAtAction(nameof(AddQuestion), new { id, questionId }, response);
    }
}
