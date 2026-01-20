using MediatR;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Application.UseCases.CompleteSession;

public sealed class CompleteSessionCommandHandler(
    ISessionRepository sessionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CompleteSessionCommand, Result>
{
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> Handle(CompleteSessionCommand request, CancellationToken cancellationToken)
    {
        // Get session
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session", request.SessionId));
        }

        // Complete session
        var completeResult = session.Complete();
        if (completeResult.IsFailure)
        {
            return completeResult;
        }

        // Persist changes
        _sessionRepository.Update(session);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Database.SaveFailed", $"Failed to complete session: {ex.Message}"));
        }

        return Result.Success();
    }
}
