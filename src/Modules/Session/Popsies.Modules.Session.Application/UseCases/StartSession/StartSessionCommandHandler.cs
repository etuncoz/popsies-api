using MediatR;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Application.UseCases.StartSession;

public sealed class StartSessionCommandHandler(
    ISessionRepository sessionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<StartSessionCommand, Result>
{
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        // Get session
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(Error.NotFound("Session", request.SessionId));
        }

        // Start session
        var startResult = session.Start();
        if (startResult.IsFailure)
        {
            return startResult;
        }

        // Persist changes
        _sessionRepository.Update(session);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Database.SaveFailed", $"Failed to start session: {ex.Message}"));
        }

        return Result.Success();
    }
}
