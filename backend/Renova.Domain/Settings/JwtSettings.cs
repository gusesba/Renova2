namespace Renova.Domain.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpirationMinutes { get; set; }
}
