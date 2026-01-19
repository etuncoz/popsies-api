using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Identity.Contracts;

public sealed record LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    public string? DeviceInfo { get; init; }
}
