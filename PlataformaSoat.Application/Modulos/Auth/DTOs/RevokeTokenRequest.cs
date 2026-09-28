using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Auth.DTOs;

public class RevokeTokenRequest
{
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
