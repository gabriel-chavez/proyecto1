namespace PlataformaSoat.Application.Configuration;

/// <summary>
/// Configuración de seguridad para la emisión y validación de tokens JWT.
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = "PlataformaSoatSuperSecretKeyForJwtTokens2026CleanArchitecture!";
    public string Issuer { get; set; } = "PlataformaSoat";
    public string Audience { get; set; } = "PlataformaSoatClient";
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
