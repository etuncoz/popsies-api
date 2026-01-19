namespace Popsies.Shared.Abstractions.Domain;

/// <summary>
/// Base class for aggregate roots
/// </summary>
public abstract class AggregateRoot : Entity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected AggregateRoot(Guid id) : base(id)
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected AggregateRoot()
    {
    }

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
