using System.ComponentModel.DataAnnotations;

namespace Popsies.Modules.Identity.Contracts;

public sealed record CreateGuestRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(20)]
    public string DisplayName { get; init; } = string.Empty;

    public string? DeviceInfo { get; init; }
}
