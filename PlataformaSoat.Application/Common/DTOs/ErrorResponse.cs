using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Common.DTOs;

public class ErrorResponse
{
    [JsonPropertyName("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("detalle")]
    public string? Detalle { get; set; }

    [JsonPropertyName("errores")]
    public List<string> Errores { get; set; } = new();
}
