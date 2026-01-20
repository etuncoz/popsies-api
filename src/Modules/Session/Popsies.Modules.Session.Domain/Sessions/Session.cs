using Popsies.Modules.Session.Domain.Answers;
using Popsies.Modules.Session.Domain.Answers.Events;
using Popsies.Modules.Session.Domain.Players;
using Popsies.Modules.Session.Domain.Players.Events;
using Popsies.Modules.Session.Domain.Sessions.Events;
using Popsies.Shared.Abstractions.Domain;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Modules.Session.Domain.Sessions;

/// <summary>
/// Session aggregate root - manages a quiz session
/// Invariants:
/// - Session code must be 6 characters (alphanumeric)
/// - Max players must be 2-100
/// - Only Waiting sessions can accept new players
/// - Only Active sessions can accept answers
/// - Session must have at least 1 player to start
/// - Current question index cannot exceed total questions
/// </summary>
public sealed class Session : AggregateRoot
{
    private const int SessionCodeLength = 6;
    private const int MinPlayers = 1;
    private const int MaxPlayersLimit = 100;
    private const int MinMaxPlayers = 2;

    private readonly List<Player> _players = new();
    private readonly List<Answer> _answers = new();

    public Guid QuizId { get; private set; }
    public Guid HostId { get; private set; }
    public string SessionCode { get; private set; }
    public SessionState State { get; private set; }
    public int MaxPlayers { get; private set; }
    public int CurrentQuestionIndex { get; private set; }
    public int TotalQuestions { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyCollection<Player> Players => _players.AsReadOnly();
    public IReadOnlyCollection<Answer> Answers => _answers.AsReadOnly();

    private Session() { }

    private Session(
        Guid id,
        Guid quizId,
        Guid hostId,
        string sessionCode,
        int maxPlayers,
        int totalQuestions) : base(id)
    {
        QuizId = quizId;
        HostId = hostId;
        SessionCode = sessionCode;
        State = SessionState.Waiting;
        MaxPlayers = maxPlayers;
        CurrentQuestionIndex = 0;
        TotalQuestions = totalQuestions;
    }

    /// <summary>
    /// Creates a new quiz session
    /// </summary>
    public static Result<Session> Create(
        Guid id,
        Guid quizId,
        Guid hostId,
        string sessionCode,
        int maxPlayers,
        int totalQuestions)
    {
        if (quizId == Guid.Empty)
        {
            return Result.Failure<Session>(Error.Validation("QuizId", "Quiz ID cannot be empty"));
        }

        if (hostId == Guid.Empty)
        {
            return Result.Failure<Session>(Error.Validation("HostId", "Host ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(sessionCode))
        {
            return Result.Failure<Session>(Error.Validation("SessionCode", "Session code cannot be empty"));
        }

        if (sessionCode.Length != SessionCodeLength)
        {
            return Result.Failure<Session>(Error.Validation("SessionCode",
                $"Session code must be exactly {SessionCodeLength} characters"));
        }

        if (!sessionCode.All(char.IsLetterOrDigit))
        {
            return Result.Failure<Session>(Error.Validation("SessionCode",
                "Session code must contain only letters and numbers"));
        }

        if (maxPlayers < MinMaxPlayers || maxPlayers > MaxPlayersLimit)
        {
            return Result.Failure<Session>(Error.Validation("MaxPlayers",
                $"Max players must be between {MinMaxPlayers} and {MaxPlayersLimit}"));
        }

        if (totalQuestions <= 0)
        {
            return Result.Failure<Session>(Error.Validation("TotalQuestions",
                "Total questions must be greater than zero"));
        }

        var session = new Session(id, quizId, hostId, sessionCode, maxPlayers, totalQuestions);

        session.RaiseDomainEvent(new SessionCreatedEvent(id, quizId, hostId, sessionCode, maxPlayers));

        return Result.Success(session);
    }

    /// <summary>
    /// Adds a player to the session
    /// </summary>
    public Result<Player> AddPlayer(Guid playerId, Guid userId, string displayName)
    {
        if (State != SessionState.Waiting)
        {
            return Result.Failure<Player>(Error.Create("Session.NotWaiting",
                "Players can only join sessions in waiting state"));
        }

        if (_players.Count >= MaxPlayers)
        {
            return Result.Failure<Player>(Error.Create("Session.Full",
                "Session has reached maximum player capacity"));
        }

        if (_players.Any(p => p.UserId == userId && p.IsActive))
        {
            return Result.Failure<Player>(Error.Create("Session.PlayerAlreadyJoined",
                "User has already joined this session"));
        }

        var playerResult = Player.Create(playerId, Id, userId, displayName);
        if (playerResult.IsFailure)
        {
            return Result.Failure<Player>(playerResult.Error);
        }

        var player = playerResult.Value;
        _players.Add(player);
        MarkAsUpdated();

        RaiseDomainEvent(new PlayerJoinedEvent(Id, playerId, userId, displayName, player.JoinedAt));

        return Result.Success(player);
    }

    /// <summary>
    /// Removes a player from the session
    /// </summary>
    public Result RemovePlayer(Guid playerId)
    {
        var player = _players.FirstOrDefault(p => p.Id == playerId);
        if (player is null)
        {
            return Result.Failure(Error.NotFound("Player", playerId));
        }

        var leaveResult = player.Leave();
        if (leaveResult.IsFailure)
        {
            return leaveResult;
        }

        MarkAsUpdated();

        RaiseDomainEvent(new PlayerLeftEvent(Id, playerId, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Starts the session
    /// </summary>
    public Result Start()
    {
        if (State != SessionState.Waiting)
        {
            return Result.Failure(Error.Create("Session.NotWaiting",
                "Only waiting sessions can be started"));
        }

        var activePlayers = _players.Count(p => p.IsActive);
        if (activePlayers < MinPlayers)
        {
            return Result.Failure(Error.Create("Session.InsufficientPlayers",
                $"Session must have at least {MinPlayers} active player(s) to start"));
        }

        State = SessionState.Active;
        StartedAt = DateTime.UtcNow;
        CurrentQuestionIndex = 0;
        MarkAsUpdated();

        RaiseDomainEvent(new SessionStartedEvent(Id, activePlayers, StartedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Submits an answer for a player
    /// </summary>
    public Result<Answer> SubmitAnswer(
        Guid answerId,
        Guid playerId,
        Guid questionId,
        Guid selectedItemId,
        bool isCorrect,
        int pointsEarned,
        int timeTakenSeconds)
    {
        if (State != SessionState.Active)
        {
            return Result.Failure<Answer>(Error.Create("Session.NotActive",
                "Answers can only be submitted to active sessions"));
        }

        var player = _players.FirstOrDefault(p => p.Id == playerId && p.IsActive);
        if (player is null)
        {
            return Result.Failure<Answer>(Error.NotFound("Player", playerId));
        }

        // Check if player already answered this question
        if (_answers.Any(a => a.PlayerId == playerId && a.QuestionId == questionId))
        {
            return Result.Failure<Answer>(Error.Create("Session.AlreadyAnswered",
                "Player has already answered this question"));
        }

        var answerResult = Answer.Create(
            answerId,
            Id,
            playerId,
            questionId,
            selectedItemId,
            isCorrect,
            pointsEarned,
            timeTakenSeconds);

        if (answerResult.IsFailure)
        {
            return Result.Failure<Answer>(answerResult.Error);
        }

        var answer = answerResult.Value;
        _answers.Add(answer);

        // Update player score
        var scoreResult = player.AddScore(pointsEarned, isCorrect);
        if (scoreResult.IsFailure)
        {
            return Result.Failure<Answer>(scoreResult.Error);
        }

        MarkAsUpdated();

        RaiseDomainEvent(new AnswerSubmittedEvent(
            Id,
            playerId,
            questionId,
            selectedItemId,
            isCorrect,
            pointsEarned,
            answer.SubmittedAt));

        return Result.Success(answer);
    }

    /// <summary>
    /// Advances to the next question
    /// </summary>
    public Result AdvanceToNextQuestion()
    {
        if (State != SessionState.Active)
        {
            return Result.Failure(Error.Create("Session.NotActive",
                "Only active sessions can advance questions"));
        }

        if (CurrentQuestionIndex >= TotalQuestions - 1)
        {
            return Result.Failure(Error.Create("Session.NoMoreQuestions",
                "No more questions available"));
        }

        CurrentQuestionIndex++;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Completes the session
    /// </summary>
    public Result Complete()
    {
        if (State != SessionState.Active)
        {
            return Result.Failure(Error.Create("Session.NotActive",
                "Only active sessions can be completed"));
        }

        State = SessionState.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsUpdated();

        var activePlayers = _players.Count(p => p.IsActive);
        RaiseDomainEvent(new SessionCompletedEvent(Id, activePlayers, CompletedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Cancels the session
    /// </summary>
    public Result Cancel()
    {
        if (State == SessionState.Completed)
        {
            return Result.Failure(Error.Create("Session.AlreadyCompleted",
                "Cannot cancel completed session"));
        }

        if (State == SessionState.Cancelled)
        {
            return Result.Failure(Error.Create("Session.AlreadyCancelled",
                "Session is already cancelled"));
        }

        State = SessionState.Cancelled;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Gets the leaderboard (players ordered by score)
    /// </summary>
    public IReadOnlyList<Player> GetLeaderboard()
    {
        return _players
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.TotalScore)
            .ThenByDescending(p => p.CorrectAnswers)
            .ThenBy(p => p.JoinedAt)
            .ToList()
            .AsReadOnly();
    }
}
