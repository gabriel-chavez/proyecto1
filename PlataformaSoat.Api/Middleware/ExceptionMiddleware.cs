using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Common.DTOs.Logs;
using PlataformaSoat.Application.Common.Exceptions;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Configuration;
using PlataformaSoat.Domain.Exceptions;

namespace PlataformaSoat.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IErrorLogService errorLogService,
        ICurrentUserService currentUserService,
        IOptions<ApiSettings> apiSettings)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturado en middleware de excepciones");
            await LogErrorToDbAsync(context, ex, errorLogService, currentUserService, apiSettings);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task LogErrorToDbAsync(
        HttpContext context,
        Exception ex,
        IErrorLogService errorLogService,
        ICurrentUserService currentUserService,
        IOptions<ApiSettings> apiSettings)
    {
        try
        {
            var (layer, statusCode) = ex switch
            {
                ValidationException => ("APPLICATION", StatusCodes.Status400BadRequest),
                BusinessException => ("APPLICATION", StatusCodes.Status400BadRequest),
                DomainException => ("DOMAIN", StatusCodes.Status422UnprocessableEntity),
                _ when ex.GetType().Name.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) => ("DATABASE", StatusCodes.Status500InternalServerError),
                _ => ("API", StatusCodes.Status500InternalServerError)
            };

            var systemName = apiSettings?.Value?.NombreSistema ?? "PlataformaSoat";
            var userName = currentUserService.Username ?? context.User.Identity?.Name ?? "anonymous";

            var originJson = JsonSerializer.Serialize(new
            {
                trace_id = context.TraceIdentifier,
                endpoint = context.Request.Path.Value,
                method = context.Request.Method
            });

            var contextJson = JsonSerializer.Serialize(new
            {
                query = context.Request.QueryString.Value,
                client_ip = currentUserService.IPAddress ?? context.Connection.RemoteIpAddress?.ToString()
            });

            await errorLogService.AddErrorLogAsync(new AddErrorLogRequest
            {
                Layer = layer,
                System = systemName,
                UserName = userName,
                Severity = statusCode >= 500 ? "ERROR" : "WARNING",
                ErrorCode = statusCode.ToString(),
                ErrorType = ex.GetType().Name,
                Message = ex.Message,
                ExceptionMessage = ex.Message,
                ExceptionSource = ex.Source,
                ExceptionStackTrace = ex.StackTrace,
                ExceptionInner = ex.InnerException?.Message,
                Origin = originJson,
                Context = contextJson
            });
        }
        catch
        {
            // Seguridad: la auditoría de error nunca debe impedir enviar la respuesta de fallo al cliente
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (StatusCodes.Status400BadRequest, validationEx.Message),
            BusinessException businessEx => (StatusCodes.Status400BadRequest, businessEx.Message),
            DomainException domainEx => (StatusCodes.Status422UnprocessableEntity, domainEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error interno en el servidor")
        };

        var response = ApiResponse<object>.Failure(
            errorMessage: message,
            responseMessage: statusCode >= 500 ? "Ocurrió un error interno en el servidor" : message,
            statusMessage: "ERROR");
        response.CorrelationId = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
