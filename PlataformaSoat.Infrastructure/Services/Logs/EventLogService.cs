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

public class EventLogService : IEventLogService
{
    private readonly string _connectionString;
    private readonly ILogger<EventLogService> _logger;

    public EventLogService(
        IOptions<PostgresSettings> postgresSettings,
        ILogger<EventLogService> logger)
    {
        _connectionString = postgresSettings.Value.ConnectionString;
        _logger = logger;
    }

    public Task<LogOperationResult> AddEventLogAsync(
        AddEventLogRequest request,
        CancellationToken cancellationToken = default)
    {
        const string callSql = @"CALL logs.psp_add_event_log(
            @i_layer, @i_event_type, @i_event_name, @i_severity, @i_category, 
            @i_message, @i_details, @i_origin, @i_system, @i_user_name, 
            @i_request_host_name, @i_request_host_ip, @i_event_object, @i_context, 
            @i_event_success, @i_duration_ms,
            @o_success, @o_response, @o_status_message, @o_response_message, @o_error_message);";

        return LogDbExecutor.ExecuteLogProcedureAsync(
            _connectionString,
            callSql,
            p =>
            {
                p.Add(new NpgsqlParameter("i_layer", NpgsqlDbType.Varchar) { Value = request.Layer ?? "APPLICATION" });
                p.Add(new NpgsqlParameter("i_event_type", NpgsqlDbType.Varchar) { Value = request.EventType ?? "INFO" });
                p.Add(new NpgsqlParameter("i_event_name", NpgsqlDbType.Varchar) { Value = (object?)request.EventName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_severity", NpgsqlDbType.Varchar) { Value = request.Severity ?? "INFO" });
                p.Add(new NpgsqlParameter("i_category", NpgsqlDbType.Varchar) { Value = (object?)request.Category ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_message", NpgsqlDbType.Text) { Value = (object?)request.Message ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_details", NpgsqlDbType.Text) { Value = (object?)request.Details ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_origin", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.Origin) ? "{}" : request.Origin });
                p.Add(new NpgsqlParameter("i_system", NpgsqlDbType.Varchar) { Value = (object?)request.System ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_user_name", NpgsqlDbType.Varchar) { Value = (object?)request.UserName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_host_name", NpgsqlDbType.Varchar) { Value = (object?)request.RequestHostName ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_request_host_ip", NpgsqlDbType.Varchar) { Value = (object?)request.RequestHostIp ?? DBNull.Value });
                p.Add(new NpgsqlParameter("i_event_object", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.EventObject) ? "{}" : request.EventObject });
                p.Add(new NpgsqlParameter("i_context", NpgsqlDbType.Jsonb) { Value = string.IsNullOrWhiteSpace(request.Context) ? "{}" : request.Context });
                p.Add(new NpgsqlParameter("i_event_success", NpgsqlDbType.Boolean) { Value = (object?)request.EventSuccess ?? true });
                p.Add(new NpgsqlParameter("i_duration_ms", NpgsqlDbType.Integer) { Value = (object?)request.DurationMs ?? DBNull.Value });
            },
            _logger,
            "EventLog",
            cancellationToken);
    }
}
