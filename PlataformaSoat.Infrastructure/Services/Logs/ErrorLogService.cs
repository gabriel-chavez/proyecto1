using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using PlataformaSoat.Application.Common.DTOs.Logs;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Infrastructure.Configuration;

namespace PlataformaSoat.Infrastructure.Services.Logs;

public class ErrorLogService : IErrorLogService
{
    private readonly string _connectionString;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(
        IOptions<PostgresSettings> postgresSettings,
        ILogger<ErrorLogService> logger)
    {
        _connectionString = postgresSettings.Value.ConnectionString;
        _logger = logger;
    }

    public Task<LogOperationResult> AddErrorLogAsync(
        AddErrorLogRequest request,
        CancellationToken cancellationToken = default)
    {
        const string callSql = @"CALL logs.psp_add_error_log(
            @i_layer, @i_system, @i_user_name, @i_severity, @i_error_code, 
            @i_error_type, @i_error_number, @i_error_severity, @i_error_state, @i_message, 
            @i_exception_message, @i_exception_source, @i_exception_stack_trace, @i_exception_inner, 
            @i_request_object, @i_origin, @i_context,
            @o_success, @o_response, @o_status_message, @o_response_message, @o_error_message);";

        return LogDbExecutor.ExecuteLogProcedureAsync(
            _connectionString,
            callSql,
            p =>
            {
                p.Add(new NpgsqlParameter("i_layer", NpgsqlDbType.Varchar) { Value = request.Layer ?? "API" });
                p.Add(new NpgsqlParameter("i_system", NpgsqlDbType.Varchar) { Value = (object?)request.System ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_user_name", NpgsqlDbType.Varchar) { Value = (object?)request.UserName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_severity", NpgsqlDbType.Varchar) { Value = request.Severity ?? "ERROR" });
                p.Add(new NpgsqlParameter("i_error_code", NpgsqlDbType.Varchar) { Value = (object?)request.ErrorCode ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_error_type", NpgsqlDbType.Varchar) { Value = (object?)request.ErrorType ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_error_number", NpgsqlDbType.Integer) { Value = (object?)request.ErrorNumber ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_error_severity", NpgsqlDbType.Integer) { Value = (object?)request.ErrorSeverity ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_error_state", NpgsqlDbType.Integer) { Value = (object?)request.ErrorState ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_message", NpgsqlDbType.Text) { Value = (object?)request.Message ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_exception_message", NpgsqlDbType.Text) { Value = (object?)request.ExceptionMessage ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_exception_source", NpgsqlDbType.Varchar) { Value = (object?)request.ExceptionSource ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_exception_stack_trace", NpgsqlDbType.Text) { Value = (object?)request.ExceptionStackTrace ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_exception_inner", NpgsqlDbType.Text) { Value = (object?)request.ExceptionInner ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_object", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.RequestObject) ? "{}" : request.RequestObject });
                p.Add(new NpgsqlParameter("i_origin", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.Origin) ? "{}" : request.Origin });
                p.Add(new NpgsqlParameter("i_context", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.Context) ? "{}" : request.Context });
            },
            _logger,
            "ErrorLog",
            cancellationToken);
    }
}
