using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Popsies.Modules.Session.Application.UseCases.CompleteSession;
using Popsies.Modules.Session.Application.UseCases.CreateSession;
using Popsies.Modules.Session.Application.UseCases.JoinSession;
using Popsies.Modules.Session.Application.UseCases.StartSession;
using Popsies.Modules.Session.Application.UseCases.SubmitAnswer;
using Popsies.Modules.Session.Contracts.Requests;
using Popsies.Modules.Session.Contracts.Responses;
using Popsies.Shared.Abstractions.Users;

namespace Popsies.Modules.Session.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public sealed class SessionController(ISender sender, ICurrentUserProvider currentUserProvider) : ControllerBase
{
    private readonly ISender _sender = sender;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SessionResponse>> CreateSession(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();

        var command = new CreateSessionCommand(
            request.QuizId,
            currentUser.Id,
            request.MaxPlayers);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new SessionResponse(
            result.Value,
            string.Empty, // Session code will be retrieved separately
            "Waiting",
            0,
            request.MaxPlayers,
            0,
            0));
    }

    [HttpPost("{sessionCode}/join")]
    public async Task<ActionResult<PlayerResponse>> JoinSession(
        string sessionCode,
        [FromBody] JoinSessionRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();

        var command = new JoinSessionCommand(
            sessionCode,
            currentUser.Id,
            request.DisplayName);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new PlayerResponse(
            result.Value,
            request.DisplayName,
            0,
            0,
            0));
    }

    [HttpPost("{sessionId}/start")]
    [Authorize]
    public async Task<ActionResult> StartSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var command = new StartSessionCommand(sessionId);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok();
    }

    [HttpPost("{sessionId}/players/{playerId}/answers")]
    public async Task<ActionResult<AnswerResponse>> SubmitAnswer(
        Guid sessionId,
        Guid playerId,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitAnswerCommand(
            sessionId,
            playerId,
            request.QuestionId,
            request.SelectedItemId,
            request.TimeTakenSeconds);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(new AnswerResponse(
            result.Value,
            true, // Placeholder
            100)); // Placeholder
    }

    [HttpPost("{sessionId}/complete")]
    [Authorize]
    public async Task<ActionResult> CompleteSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var command = new CompleteSessionCommand(sessionId);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok();
    }
}
