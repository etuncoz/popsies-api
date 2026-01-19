namespace Popsies.Shared.Abstractions.Results;

/// <summary>
/// Represents an error with a code and message
/// </summary>
public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public string Code { get; }
    public string Message { get; }

    private Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error Create(string code, string message) => new(code, message);

    public static Error NotFound(string entityName, object key) =>
        new($"{entityName}.NotFound", $"{entityName} with key '{key}' was not found");

    public static Error Validation(string field, string message) =>
        new($"Validation.{field}", message);

    public static Error Conflict(string message) =>
        new("Conflict", message);

    public static Error Unauthorized(string message = "Unauthorized access") =>
        new("Unauthorized", message);

    public static Error Forbidden(string message = "Forbidden access") =>
        new("Forbidden", message);
}
