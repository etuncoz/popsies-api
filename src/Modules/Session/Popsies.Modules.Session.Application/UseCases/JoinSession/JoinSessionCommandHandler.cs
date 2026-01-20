using MediatR;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Application.UseCases.JoinSession;

public sealed class JoinSessionCommandHandler(
    ISessionRepository sessionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<JoinSessionCommand, Result<Guid>>
{
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(JoinSessionCommand request, CancellationToken cancellationToken)
    {
        // Get session by code
        var session = await _sessionRepository.GetBySessionCodeAsync(request.SessionCode, cancellationToken);
        if (session is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Session", request.SessionCode));
        }

        // Add player to session
        var playerId = Guid.NewGuid();
        var playerResult = session.AddPlayer(playerId, request.UserId, request.DisplayName);
        if (playerResult.IsFailure)
        {
            return Result.Failure<Guid>(playerResult.Error);
        }

        // Persist changes
        _sessionRepository.Update(session);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Create("Database.SaveFailed", $"Failed to join session: {ex.Message}"));
        }

        return Result.Success(playerId);
    }
}
