using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Auth.DTOs;

public class UserInfoDto
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}
