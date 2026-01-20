using Popsies.Shared.Abstractions.Commands;

namespace Popsies.Modules.Session.Application.UseCases.CompleteSession;

/// <summary>
/// Command to complete a quiz session
/// </summary>
public sealed record CompleteSessionCommand(Guid SessionId) : ICommand;
