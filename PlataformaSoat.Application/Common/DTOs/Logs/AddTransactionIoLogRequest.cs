using System;

namespace PlataformaSoat.Application.Common.DTOs.Logs;

/// <summary>
/// Parámetros de entrada para el procedimiento logs.psp_add_transaction_io_log.
/// Registra entradas y salidas completas del consumo de procedimientos almacenados.
/// </summary>
public record AddTransactionIoLogRequest
{
    public string? System { get; init; }
    public string? Database { get; init; }
    public string? SchemaName { get; init; }
    public string? StoredProcedure { get; init; }
    public string? UserName { get; init; }
    public string? RequestHostName { get; init; }
    public string? RequestHostIp { get; init; }
    public DateTimeOffset? RequestAt { get; init; }
    public string? RequestObject { get; init; } // JSON
    public DateTimeOffset? ResponseAt { get; init; }
    public string? ResponseObject { get; init; } // JSON
    public bool TransactionSuccess { get; init; } = true;
    public string? Message { get; init; }
    public string? Exception { get; init; }
    public string? ErrorCode { get; init; }
    public int? ResponseTimeMs { get; init; }
}
