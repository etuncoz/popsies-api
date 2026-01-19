using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Identity.Contracts;

public sealed record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;

    public string? DeviceInfo { get; init; }
}
