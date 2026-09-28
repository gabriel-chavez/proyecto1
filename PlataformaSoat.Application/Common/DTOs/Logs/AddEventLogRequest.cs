namespace PlataformaSoat.Application.Common.DTOs.Logs;

/// <summary>
/// Parámetros de entrada para el procedimiento logs.psp_add_event_log.
/// Registra eventos comunes para seguimiento y stack trace funcional.
/// </summary>
public record AddEventLogRequest
{
    public string Layer { get; init; } = "APPLICATION";
    public string EventType { get; init; } = "INFO";
    public string? EventName { get; init; }
    public string Severity { get; init; } = "INFO";
    public string? Category { get; init; }
    public string? Message { get; init; }
    public string? Details { get; init; }
    public string? Origin { get; init; } // JSON
    public string? System { get; init; }
    public string? UserName { get; init; }
    public string? RequestHostName { get; init; }
    public string? RequestHostIp { get; init; }
    public string? EventObject { get; init; } // JSON
    public string? Context { get; init; } // JSON
    public bool? EventSuccess { get; init; } = true;
    public int? DurationMs { get; init; }
}
