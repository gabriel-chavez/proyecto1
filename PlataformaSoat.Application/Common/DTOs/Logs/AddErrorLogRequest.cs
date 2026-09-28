namespace PlataformaSoat.Application.Common.DTOs.Logs;

/// <summary>
/// Parámetros de entrada para el procedimiento logs.psp_add_error_log.
/// Registra errores detallados por cada capa de la arquitectura (API, APPLICATION, DOMAIN, INFRASTRUCTURE, DATABASE).
/// </summary>
public record AddErrorLogRequest
{
    public string Layer { get; init; } = "API";
    public string? System { get; init; }
    public string? UserName { get; init; }
    public string Severity { get; init; } = "ERROR";
    public string? ErrorCode { get; init; }
    public string? ErrorType { get; init; }
    public int? ErrorNumber { get; init; }
    public int? ErrorSeverity { get; init; }
    public int? ErrorState { get; init; }
    public string? Message { get; init; }
    public string? ExceptionMessage { get; init; }
    public string? ExceptionSource { get; init; }
    public string? ExceptionStackTrace { get; init; }
    public string? ExceptionInner { get; init; }
    public string? RequestObject { get; init; } // JSON
    public string? Origin { get; init; } // JSON
    public string? Context { get; init; } // JSON
}
