namespace Popsies.Shared.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AuthorizeAttribute : Attribute
{
    public string[]? Permissions { get; init; }
    public string[]? Roles { get; init; }
}
