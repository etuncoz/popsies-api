using MediatR;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Application.UseCases.CreateSession;

/// <summary>
/// Handler for creating a new quiz session
/// </summary>
public sealed class CreateSessionCommandHandler(
    ISessionRepository sessionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSessionCommand, Result<Guid>>
{
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        // Generate unique session code
        var sessionCode = GenerateSessionCode();
        while (await _sessionRepository.ExistsBySessionCodeAsync(sessionCode, cancellationToken))
        {
            sessionCode = GenerateSessionCode();
        }

        // TODO: Fetch quiz to get total questions count
        // For now, using a placeholder value
        var totalQuestions = 10;

        // Create session aggregate
        var sessionId = Guid.NewGuid();
        var sessionResult = Domain.Sessions.Session.Create(
            sessionId,
            request.QuizId,
            request.HostId,
            sessionCode,
            request.MaxPlayers,
            totalQuestions);

        if (sessionResult.IsFailure)
        {
            return Result.Failure<Guid>(sessionResult.Error);
        }

        var session = sessionResult.Value;

        // Persist session
        await _sessionRepository.AddAsync(session, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Create("Database.SaveFailed", $"Failed to create session: {ex.Message}"));
        }

        return Result.Success(sessionId);
    }

    private static string GenerateSessionCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
