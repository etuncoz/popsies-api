namespace Popsies.Shared.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : PopsiesException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation", "One or more validation errors occurred")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message)
        : base($"Validation.{field}", message)
    {
        Errors = new Dictionary<string, string[]>
        {
            [field] = new[] { message }
        };
    }
}
