using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Common.DTOs.Logs;
using PlataformaSoat.Infrastructure.Helpers;
using PlataformaSoat.Infrastructure.Persistence;

namespace PlataformaSoat.Infrastructure.Extensions;

/// <summary>
/// Definición de un parámetro de entrada (IN).
/// </summary>
public readonly record struct InParam(string Name, object? Value, NpgsqlDbType Type);

/// <summary>
/// Resultado estándar de un PROCEDURE que sigue la convención corporativa de PostgreSQL:
/// INOUT o_success boolean,
/// INOUT o_response jsonb,
/// INOUT o_status_message text,
/// INOUT o_response_message text,
/// INOUT o_error_message text
/// </summary>
public record ProcedureResult(
    bool Success,
    string? Response,
    string? StatusMessage,
    string? ResponseMessage,
    string? ErrorMessage)
{
    public static ProcedureResult From(Dictionary<string, object?> dict) => new(
        Success: (dict.TryGetValue("o_success", out var s) || dict.TryGetValue("p_success", out s) || dict.TryGetValue("success", out s)) && s is bool b && b,
        Response: (dict.TryGetValue("o_response", out var r) || dict.TryGetValue("p_response", out r) || dict.TryGetValue("response", out r)) ? r?.ToString() : null,
        StatusMessage: (dict.TryGetValue("o_status_message", out var st) || dict.TryGetValue("p_status_message", out st) || dict.TryGetValue("status_message", out st)) ? st?.ToString() : null,
        ResponseMessage: (dict.TryGetValue("o_response_message", out var rm) || dict.TryGetValue("p_response_message", out rm) || dict.TryGetValue("response_message", out rm)) ? rm?.ToString() : null,
        ErrorMessage: (dict.TryGetValue("o_error_message", out var e) || dict.TryGetValue("p_error_message", out e) || dict.TryGetValue("error_message", out e)) ? e?.ToString() : null);

    public BaseResponse<T> ToBaseResponse<T>() => BaseResponse<T>.FromDictionary(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
    {
        ["o_success"] = Success,
        ["o_response"] = Response,
        ["o_status_message"] = StatusMessage,
        ["o_response_message"] = ResponseMessage,
        ["o_error_message"] = ErrorMessage
    });
}

public static class NpgsqlProcedureExtensions
{
    private static readonly (string Name, NpgsqlDbType Type)[] StandardOutputParams =
    {
        ("o_success",          NpgsqlDbType.Boolean),
        ("o_response",         NpgsqlDbType.Jsonb),
        ("o_status_message",   NpgsqlDbType.Text),
        ("o_response_message", NpgsqlDbType.Text),
        ("o_error_message",    NpgsqlDbType.Text)
    };

    /// <summary>
    /// Ejecuta un PROCEDURE indicando solo su nombre y la lista ordenada de parámetros IN.
    /// Registra automáticamente I/O en logs.psp_add_transaction_io_log y errores en logs.psp_add_error_log.
    /// </summary>
    public static async Task<ProcedureResult> ExecuteProcedureAsync(
        this DbContext dbContext,
        string procedureName,
        IReadOnlyList<InParam> input,
        CancellationToken cancellationToken = default)
    {
        if (IsLoggingProcedure(procedureName))
        {
            var dict = await ExecuteCoreAsync(dbContext, procedureName, input, StandardOutputParams, cancellationToken);
            return ProcedureResult.From(dict);
        }

        var requestAt = DateTimeOffset.UtcNow;
        var sw = Stopwatch.StartNew();

        var inputDict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in input)
        {
            inputDict[p.Name] = p.Value is DBNull ? null : p.Value;
        }
        var requestObjectJson = JsonSerializer.Serialize(inputDict);

        ProcedureResult? result = null;
        Exception? caughtException = null;

        try
        {
            var dict = await ExecuteCoreAsync(dbContext, procedureName, input, StandardOutputParams, cancellationToken);
            result = ProcedureResult.From(dict);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            result = new ProcedureResult(
                Success: false,
                Response: null,
                StatusMessage: "ERROR",
                ResponseMessage: "Excepción al ejecutar procedimiento en base de datos",
                ErrorMessage: ex.Message);
        }
        finally
        {
            sw.Stop();
        }

        var responseAt = DateTimeOffset.UtcNow;
        var responseTimeMs = (int)sw.ElapsedMilliseconds;

        var responseDict = new Dictionary<string, object?>
        {
            ["o_success"] = result.Success,
            ["o_response"] = result.Response,
            ["o_status_message"] = result.StatusMessage,
            ["o_response_message"] = result.ResponseMessage,
            ["o_error_message"] = result.ErrorMessage
        };
        var responseObjectJson = JsonSerializer.Serialize(responseDict);

        var conn = dbContext.Database.GetDbConnection();
        var connString = conn.ConnectionString;
        var database = conn.Database ?? "kernel_dev";

        var parts = procedureName.Split('.');
        var schemaName = parts.Length > 1 ? parts[0] : "public";
        var spName = parts.Length > 1 ? parts[1] : parts[0];

        string system = "PlataformaSoat";
        string userName = Environment.UserName;
        string hostIp = "127.0.0.1";
        string hostName = Environment.MachineName;

        if (dbContext is ApplicationDbContext appDb)
        {
            system = appDb.ApiSettings?.NombreSistema ?? system;
            userName = appDb.CurrentUserService?.Username ?? userName;
            hostIp = appDb.CurrentUserService?.IPAddress ?? hostIp;

            // 1. Registrar logs.psp_add_transaction_io_log mediante servicio inyectado
            if (appDb.TransactionIoLogService != null)
            {
                await appDb.TransactionIoLogService.AddTransactionIoLogAsync(new AddTransactionIoLogRequest
                {
                    System = system,
                    Database = database,
                    SchemaName = schemaName,
                    StoredProcedure = spName,
                    UserName = userName,
                    RequestHostName = hostName,
                    RequestHostIp = hostIp,
                    RequestAt = requestAt,
                    RequestObject = requestObjectJson,
                    ResponseAt = responseAt,
                    ResponseObject = responseObjectJson,
                    TransactionSuccess = result.Success,
                    Message = result.ResponseMessage ?? result.StatusMessage,
                    Exception = caughtException?.ToString(),
                    ErrorCode = result.StatusMessage,
                    ResponseTimeMs = responseTimeMs
                }, cancellationToken);
            }

            // 2. Si falla el SP o retorna o_status_message = 'ERROR' o hubo excepción, registrar logs.psp_add_error_log en capa DATABASE
            if ((!result.Success || string.Equals(result.StatusMessage, "ERROR", StringComparison.OrdinalIgnoreCase) || caughtException != null)
                && appDb.ErrorLogService != null)
            {
                await appDb.ErrorLogService.AddErrorLogAsync(new AddErrorLogRequest
                {
                    Layer = "DATABASE",
                    System = system,
                    UserName = userName,
                    Severity = "ERROR",
                    ErrorCode = result.StatusMessage ?? "DB_ERROR",
                    ErrorType = caughtException?.GetType().Name ?? "ProcedureError",
                    Message = result.ResponseMessage ?? "El procedimiento almacenado retornó estado de error.",
                    ExceptionMessage = result.ErrorMessage ?? caughtException?.Message,
                    ExceptionSource = caughtException?.Source ?? procedureName,
                    ExceptionStackTrace = caughtException?.StackTrace,
                    RequestObject = requestObjectJson,
                    Origin = JsonSerializer.Serialize(new { procedure = procedureName }),
                    Context = responseObjectJson
                }, cancellationToken);
            }
        }

        if (caughtException != null)
        {
            throw caughtException;
        }

        return result;
    }

    /// <summary>
    /// Ejecuta un PROCEDURE y mapea directamente a BaseResponse&lt;T&gt; respetando el tipo T del response.
    /// </summary>
    public static async Task<BaseResponse<T>> ExecuteProcedureAsync<T>(
        this DbContext dbContext,
        string procedureName,
        IReadOnlyList<InParam> input,
        CancellationToken cancellationToken = default)
    {
        var result = await dbContext.ExecuteProcedureAsync(procedureName, input, cancellationToken);
        return result.ToBaseResponse<T>();
    }

    /// <summary>
    /// Permite definir INOUT personalizados para SPs que no sigan la convención estándar.
    /// </summary>
    public static async Task<Dictionary<string, object?>> ExecuteProcedureRawAsync(
        this DbContext dbContext,
        string procedureName,
        IReadOnlyList<InParam> input,
        IReadOnlyList<(string Name, NpgsqlDbType Type)> outputParams,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteCoreAsync(dbContext, procedureName, input, outputParams, cancellationToken);
    }

    // ---------- Helpers privados ----------

    private static bool IsLoggingProcedure(string procedureName)
    {
        return procedureName.StartsWith("logs.", StringComparison.OrdinalIgnoreCase)
            || procedureName.Contains("psp_add_api_log", StringComparison.OrdinalIgnoreCase)
            || procedureName.Contains("psp_add_error_log", StringComparison.OrdinalIgnoreCase)
            || procedureName.Contains("psp_add_event_log", StringComparison.OrdinalIgnoreCase)
            || procedureName.Contains("psp_add_transaction_io_log", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, object?>> ExecuteCoreAsync(
        DbContext dbContext,
        string procedureName,
        IReadOnlyList<InParam> input,
        IReadOnlyList<(string Name, NpgsqlDbType Type)> outputParams,
        CancellationToken cancellationToken)
    {
        var allParams = BuildParameters(input, outputParams);
        var callSql = PgHelper.BuildCall(procedureName, allParams);

        var conn = dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = callSql;
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddRange(allParams.ToArray());

        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i);
                    var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    dict[colName] = val;
                    dict[colName.TrimStart('@')] = val;
                }
            }
        }
        catch (Exception)
        {
            if (dict.Count == 0)
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        var paramDict = PgHelper.ToDictionary(allParams);
        foreach (var kvp in paramDict)
        {
            if (!dict.ContainsKey(kvp.Key) || dict[kvp.Key] == null)
            {
                dict[kvp.Key] = kvp.Value;
            }
        }

        return dict;
    }



    private static List<NpgsqlParameter> BuildParameters(
        IReadOnlyList<InParam> input,
        IReadOnlyList<(string Name, NpgsqlDbType Type)> outputParams)
    {
        var list = new List<NpgsqlParameter>(input.Count + outputParams.Count);

        for (int i = 0; i < input.Count; i++)
        {
            var p = input[i];
            list.Add(PgHelper.In(p.Name, p.Type, p.Value));
        }

        for (int i = 0; i < outputParams.Count; i++)
        {
            var (name, type) = outputParams[i];
            list.Add(PgHelper.InOut(name, type));
        }

        return list;
    }
}