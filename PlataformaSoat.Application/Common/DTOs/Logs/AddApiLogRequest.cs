using System;

namespace PlataformaSoat.Application.Common.DTOs.Logs;

/// <summary>
/// Parámetros de entrada para el procedimiento logs.psp_add_api_log.
/// Registra peticiones y respuestas obligatorias de consumo de API.
/// </summary>
public record AddApiLogRequest
{
    public string Direction { get; init; } = "INBOUND"; // 'INBOUND' o 'OUTBOUND'
    public Guid? CorrelationId { get; init; }
    public Guid? TransactionRef { get; init; }
    public string ChannelType { get; init; } = "HTTP";
    public string? ApiName { get; init; }
    public string? ApiVersion { get; init; }
    public DateTimeOffset? RequestAt { get; init; }
    public string? Endpoint { get; init; }
    public string? HttpMethod { get; init; }
    public string? RequestHeaders { get; init; } // JSON
    public string? RequestObject { get; init; } // JSON
    public int? RequestSizeBytes { get; init; }
    public DateTimeOffset? ResponseAt { get; init; }
    public int? HttpStatus { get; init; }
    public string? ResponseHeaders { get; init; } // JSON
    public string? ResponseObject { get; init; } // JSON
    public int? ResponseSizeBytes { get; init; }
    public int? ResponseTimeMs { get; init; }
    public string? System { get; init; }
    public string? UserName { get; init; }
    public string? RequestHostName { get; init; }
    public string? RequestHostIp { get; init; }
    public bool ApiSuccess { get; init; } = true;
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }
    public string? Exception { get; init; }
}
