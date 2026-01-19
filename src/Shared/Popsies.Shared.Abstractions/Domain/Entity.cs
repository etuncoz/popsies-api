using Popsies.Shared.Abstractions.Events;

namespace Popsies.Shared.Abstractions.Domain;

/// <summary>
/// Base class for domain entities
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    private readonly List<IEvent> _domainEvents = new();

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity()
    {
    }

    protected void RaiseDomainEvent(IEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not Entity entity)
        {
            return false;
        }

        if (ReferenceEquals(this, entity))
        {
            return true;
        }

        if (GetType() != entity.GetType())
        {
            return false;
        }

        return Id == entity.Id;
    }

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
