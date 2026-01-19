using MediatR;
using Popsies.Shared.Abstractions.Results;

namespace Popsies.Shared.Abstractions.Commands;

/// <summary>
/// Marker interface for commands that don't return a value
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Marker interface for commands that return a value
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
