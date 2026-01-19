using Popsies.Shared.Abstractions.Time;

namespace Popsies.Shared.Infrastructure.Time;

/// <summary>
/// Default implementation of IDateTime
/// </summary>
public sealed class DateTimeProvider : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
