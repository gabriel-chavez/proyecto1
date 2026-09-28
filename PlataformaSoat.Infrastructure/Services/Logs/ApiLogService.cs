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

public class ApiLogService : IApiLogService
{
    private readonly string _connectionString;
    private readonly ILogger<ApiLogService> _logger;

    public ApiLogService(
        IOptions<PostgresSettings> postgresSettings,
        ILogger<ApiLogService> logger)
    {
        _connectionString = postgresSettings.Value.ConnectionString;
        _logger = logger;
    }

    public Task<LogOperationResult> AddApiLogAsync(
        AddApiLogRequest request,
        CancellationToken cancellationToken = default)
    {
        const string callSql = @"CALL logs.psp_add_api_log(
            @i_direction, @i_correlation_id, @i_transaction_ref, @i_channel_type, @i_api_name,
            @i_api_version, @i_request_at, @i_endpoint, @i_http_method, @i_request_headers,
            @i_request_object, @i_request_size_bytes, @i_response_at, @i_http_status,
            @i_response_headers, @i_response_object, @i_response_size_bytes, @i_response_time_ms,
            @i_system, @i_user_name, @i_request_host_name, @i_request_host_ip, @i_api_success,
            @i_error_code, @i_message, @i_exception,
            @o_success, @o_response, @o_status_message, @o_response_message, @o_error_message);";

        return LogDbExecutor.ExecuteLogProcedureAsync(
            _connectionString,
            callSql,
            p =>
            {
                p.Add(new NpgsqlParameter("i_direction", NpgsqlDbType.Varchar) { Value = request.Direction ?? "INBOUND" });
                p.Add(new NpgsqlParameter("i_correlation_id", NpgsqlDbType.Uuid) { Value = (object?)request.CorrelationId ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_transaction_ref", NpgsqlDbType.Uuid) { Value = (object?)request.TransactionRef ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_channel_type", NpgsqlDbType.Varchar) { Value = request.ChannelType ?? "HTTP" });
                p.Add(new NpgsqlParameter("i_api_name", NpgsqlDbType.Varchar) { Value = (object?)request.ApiName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_api_version", NpgsqlDbType.Varchar) { Value = (object?)request.ApiVersion ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_at", NpgsqlDbType.TimestampTz) { Value = (object?)request.RequestAt ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_endpoint", NpgsqlDbType.Varchar) { Value = (object?)request.Endpoint ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_http_method", NpgsqlDbType.Varchar) { Value = (object?)request.HttpMethod ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_headers", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.RequestHeaders) ? "{}" : request.RequestHeaders });
                p.Add(new NpgsqlParameter("i_request_object", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.RequestObject) ? "{}" : request.RequestObject });
                p.Add(new NpgsqlParameter("i_request_size_bytes", NpgsqlDbType.Integer) { Value = (object?)request.RequestSizeBytes ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_response_at", NpgsqlDbType.TimestampTz) { Value = (object?)request.ResponseAt ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_http_status", NpgsqlDbType.Integer) { Value = (object?)request.HttpStatus ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_response_headers", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.ResponseHeaders) ? "{}" : request.ResponseHeaders });
                p.Add(new NpgsqlParameter("i_response_object", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.ResponseObject) ? "{}" : request.ResponseObject });
                p.Add(new NpgsqlParameter("i_response_size_bytes", NpgsqlDbType.Integer) { Value = (object?)request.ResponseSizeBytes ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_response_time_ms", NpgsqlDbType.Integer) { Value = (object?)request.ResponseTimeMs ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_system", NpgsqlDbType.Varchar) { Value = (object?)request.System ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_user_name", NpgsqlDbType.Varchar) { Value = (object?)request.UserName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_host_name", NpgsqlDbType.Varchar) { Value = (object?)request.RequestHostName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_host_ip", NpgsqlDbType.Varchar) { Value = (object?)request.RequestHostIp ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_api_success", NpgsqlDbType.Boolean) { Value = request.ApiSuccess });
                p.Add(new NpgsqlParameter("i_error_code", NpgsqlDbType.Varchar) { Value = (object?)request.ErrorCode ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_message", NpgsqlDbType.Text) { Value = (object?)request.Message ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_exception", NpgsqlDbType.Text) { Value = (object?)request.Exception ?? DBNull.Value });
            },
            _logger,
            "ApiLog",
            cancellationToken);
    }
}
