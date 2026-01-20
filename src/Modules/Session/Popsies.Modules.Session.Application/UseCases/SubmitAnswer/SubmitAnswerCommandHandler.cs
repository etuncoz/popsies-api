using MediatR;
using Popsies.Modules.Session.Application.Common.Repositories;
using Popsies.Shared.Abstractions.Persistence;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Application.UseCases.SubmitAnswer;

public sealed class SubmitAnswerCommandHandler(
    ISessionRepository sessionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SubmitAnswerCommand, Result<Guid>>
{
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        // Get session
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Session", request.SessionId));
        }

        // TODO: Validate answer against quiz question
        // For now, using placeholder values
        var isCorrect = true;
        var pointsEarned = 100;

        // Submit answer
        var answerId = Guid.NewGuid();
        var answerResult = session.SubmitAnswer(
            answerId,
            request.PlayerId,
            request.QuestionId,
            request.SelectedItemId,
            isCorrect,
            pointsEarned,
            request.TimeTakenSeconds);

        if (answerResult.IsFailure)
        {
            return Result.Failure<Guid>(answerResult.Error);
        }

        // Persist changes
        _sessionRepository.Update(session);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Create("Database.SaveFailed", $"Failed to submit answer: {ex.Message}"));
        }

        return Result.Success(answerId);
    }
}
