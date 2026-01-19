namespace Popsies.Shared.Abstractions.Domain;

/// <summary>
/// Value object representing a user identifier
/// </summary>
public sealed class UserId : ValueObject
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public static UserId Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(value));
        }

        return new UserId(value);
    }

    public static UserId CreateNew() => new(Guid.NewGuid());

    public static implicit operator Guid(UserId userId) => userId.Value;

    public static implicit operator UserId(Guid value) => Create(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
