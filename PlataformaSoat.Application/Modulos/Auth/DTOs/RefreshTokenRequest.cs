using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Auth.DTOs;

public class RefreshTokenRequest
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
