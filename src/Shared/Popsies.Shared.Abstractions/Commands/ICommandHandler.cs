using Popsies.Shared.Abstractions.Results;

namespace Popsies.Shared.Abstractions.Commands;

/// <summary>
/// Handler for commands that don't return a value
/// </summary>
public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for commands that return a value
/// </summary>
public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken = default);
}
