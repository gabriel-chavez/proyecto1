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

public class TransactionIoLogService : ITransactionIoLogService
{
    private readonly string _connectionString;
    private readonly ILogger<TransactionIoLogService> _logger;

    public TransactionIoLogService(
        IOptions<PostgresSettings> postgresSettings,
        ILogger<TransactionIoLogService> logger)
    {
        _connectionString = postgresSettings.Value.ConnectionString;
        _logger = logger;
    }

    public Task<LogOperationResult> AddTransactionIoLogAsync(
        AddTransactionIoLogRequest request,
        CancellationToken cancellationToken = default)
    {
        const string callSql = @"CALL logs.psp_add_transaction_io_log(
            @i_system, @i_database, @i_schema_name, @i_stored_procedure, @i_user_name, 
            @i_request_host_name, @i_request_host_ip, @i_request_at, @i_request_object, 
            @i_response_at, @i_response_object, @i_transaction_success, @i_message, @i_exception, 
            @i_error_code, @i_response_time_ms,
            @o_success, @o_response, @o_status_message, @o_response_message, @o_error_message);";

        return LogDbExecutor.ExecuteLogProcedureAsync(
            _connectionString,
            callSql,
            p =>
            {
                p.Add(new NpgsqlParameter("i_system", NpgsqlDbType.Varchar) { Value = (object?)request.System ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_database", NpgsqlDbType.Varchar) { Value = (object?)request.Database ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_schema_name", NpgsqlDbType.Varchar) { Value = (object?)request.SchemaName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_stored_procedure", NpgsqlDbType.Varchar) { Value = (object?)request.StoredProcedure ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_user_name", NpgsqlDbType.Varchar) { Value = (object?)request.UserName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_host_name", NpgsqlDbType.Varchar) { Value = (object?)request.RequestHostName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_host_ip", NpgsqlDbType.Varchar) { Value = (object?)request.RequestHostIp ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_at", NpgsqlDbType.TimestampTz) { Value = (object?)request.RequestAt ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_object", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.RequestObject) ? "{}" : request.RequestObject });
                p.Add(new NpgsqlParameter("i_response_at", NpgsqlDbType.TimestampTz) { Value = (object?)request.ResponseAt ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_response_object", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.ResponseObject) ? "{}" : request.ResponseObject });
                p.Add(new NpgsqlParameter("i_transaction_success", NpgsqlDbType.Boolean) { Value = request.TransactionSuccess });
                p.Add(new NpgsqlParameter("i_message", NpgsqlDbType.Text) { Value = (object?)request.Message ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_exception", NpgsqlDbType.Text) { Value = (object?)request.Exception ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_error_code", NpgsqlDbType.Varchar) { Value = (object?)request.ErrorCode ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_response_time_ms", NpgsqlDbType.Integer) { Value = (object?)request.ResponseTimeMs ?? DBNull.Value });
            },
            _logger,
            "TransactionIoLog",
            cancellationToken);
    }
}
