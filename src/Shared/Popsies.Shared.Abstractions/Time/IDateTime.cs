namespace Popsies.Shared.Abstractions.Time;

/// <summary>
/// Abstraction for DateTime to support testing
/// </summary>
public interface IDateTime
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}
