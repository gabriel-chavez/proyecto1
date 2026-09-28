using System;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using PlataformaSoat.Application.Common.DTOs.Logs;

namespace PlataformaSoat.Infrastructure.Services.Logs;

internal static class LogDbExecutor
{
    public static async Task<LogOperationResult> ExecuteLogProcedureAsync(
        string connectionString,
        string procedureCallSql,
        Action<NpgsqlParameterCollection> addParameters,
        ILogger logger,
        string logContextName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = procedureCallSql;
            cmd.CommandType = CommandType.Text;

            addParameters(cmd.Parameters);

            var oSuccess = cmd.Parameters.Add(new NpgsqlParameter("o_success", NpgsqlDbType.Boolean) { Direction = ParameterDirection.InputOutput, Value = DBNull.Value });
            var oResponse = cmd.Parameters.Add(new NpgsqlParameter("o_response", NpgsqlDbType.Jsonb) { Direction = ParameterDirection.InputOutput, Value = DBNull.Value });
            var oStatusMessage = cmd.Parameters.Add(new NpgsqlParameter("o_status_message", NpgsqlDbType.Text) { Direction = ParameterDirection.InputOutput, Value = DBNull.Value });
            var oResponseMessage = cmd.Parameters.Add(new NpgsqlParameter("o_response_message", NpgsqlDbType.Text) { Direction = ParameterDirection.InputOutput, Value = DBNull.Value });
            var oErrorMessage = cmd.Parameters.Add(new NpgsqlParameter("o_error_message", NpgsqlDbType.Text) { Direction = ParameterDirection.InputOutput, Value = DBNull.Value });

            bool success = false;
            string? responseJson = null;
            string? statusMessage = null;
            string? responseMessage = null;
            string? errorMessage = null;

            try
            {
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i).TrimStart('@');
                        var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        if (name.Equals("o_success", StringComparison.OrdinalIgnoreCase)) success = val is bool b && b;
                        else if (name.Equals("o_response", StringComparison.OrdinalIgnoreCase)) responseJson = val?.ToString();
                        else if (name.Equals("o_status_message", StringComparison.OrdinalIgnoreCase)) statusMessage = val?.ToString();
                        else if (name.Equals("o_response_message", StringComparison.OrdinalIgnoreCase)) responseMessage = val?.ToString();
                        else if (name.Equals("o_error_message", StringComparison.OrdinalIgnoreCase)) errorMessage = val?.ToString();
                    }
                }
            }
            catch (Exception)
            {
                // Fallback si no expone DataReader
                if (oSuccess.Value != DBNull.Value && oSuccess.Value is bool b) success = b;
                responseJson = oResponse.Value != DBNull.Value ? oResponse.Value?.ToString() : null;
                statusMessage = oStatusMessage.Value != DBNull.Value ? oStatusMessage.Value?.ToString() : null;
                responseMessage = oResponseMessage.Value != DBNull.Value ? oResponseMessage.Value?.ToString() : null;
                errorMessage = oErrorMessage.Value != DBNull.Value ? oErrorMessage.Value?.ToString() : null;
            }

            long? insertedId = null;
            if (!string.IsNullOrWhiteSpace(responseJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("inserted_id", out var idProp) && idProp.TryGetInt64(out var idVal))
                        insertedId = idVal;
                    else if (root.TryGetProperty("event_log_id", out var evtProp) && evtProp.TryGetInt64(out var evtVal))
                        insertedId = evtVal;
                    else if (root.TryGetProperty("transaction_log_id", out var txProp) && txProp.TryGetInt64(out var txVal))
                        insertedId = txVal;
                }
                catch
                {
                    // Ignorar error al parsear id de respuesta
                }
            }

            return new LogOperationResult
            {
                Success = success,
                InsertedId = insertedId,
                StatusMessage = statusMessage,
                ResponseMessage = responseMessage,
                ErrorMessage = errorMessage
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al persistir log en base de datos para {LogContext}", logContextName);
            return new LogOperationResult
            {
                Success = false,
                StatusMessage = "LOG_DB_ERROR",
                ResponseMessage = "No se pudo registrar el log en la base de datos.",
                ErrorMessage = ex.Message
            };
        }
    }
}
