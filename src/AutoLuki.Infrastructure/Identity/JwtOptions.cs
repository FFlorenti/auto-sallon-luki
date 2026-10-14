using System.ComponentModel.DataAnnotations;

namespace AutoLuki.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = "AutoLuki";

    [Required]
    public string Audience { get; init; } = "AutoLuki";

    [Required, MinLength(32)]
    public string SigningKey { get; init; } = "";

    [Range(5, 24 * 60)]
    public int ExpiryMinutes { get; init; } = 8 * 60;
}
