using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Common.DTOs.Logs;

/// <summary>
/// Resultado estándar de la ejecución de procedimientos almacenados de logging.
/// </summary>
public record LogOperationResult
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("inserted_id")]
    public long? InsertedId { get; init; }

    [JsonPropertyName("status_message")]
    public string? StatusMessage { get; init; }

    [JsonPropertyName("response_message")]
    public string? ResponseMessage { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }
}
