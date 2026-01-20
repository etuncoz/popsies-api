using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Session.Application.UseCases.CreateSession;

/// <summary>
/// Command to create a new quiz session
/// </summary>
public sealed record CreateSessionCommand(
    Guid QuizId,
    Guid HostId,
    int MaxPlayers) : ICommand<Guid>;
