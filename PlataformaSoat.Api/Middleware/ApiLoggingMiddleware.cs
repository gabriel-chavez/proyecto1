using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaSoat.Application.Common.DTOs.Logs;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Configuration;

namespace PlataformaSoat.Api.Middleware;

/// <summary>
/// Middleware de auditoría que registra obligatoriamente petición y respuesta de consumo de API en logs.psp_add_api_log.
/// </summary>
public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IApiLogService apiLogService,
        ICurrentUserService currentUserService,
        IOptions<ApiSettings> apiSettings)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Registrar principalmente endpoints de negocio API
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var requestAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        // 1. Leer y almacenar cuerpo de la petición sin agotarlo
        context.Request.EnableBuffering();
        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0 || context.Request.Body.CanSeek)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // Serializar encabezados de la petición en JSON (protegiendo tokens)
        var reqHeadersDict = new Dictionary<string, string>();
        foreach (var (k, v) in context.Request.Headers)
        {
            if (k.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                reqHeadersDict[k] = "Bearer [PROTECTED]";
            else
                reqHeadersDict[k] = v.ToString();
        }
        var requestHeadersJson = JsonSerializer.Serialize(reqHeadersDict);

        // Asegurar que request_object sea JSON válido
        string requestObjectJson;
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            requestObjectJson = context.Request.QueryString.HasValue
                ? JsonSerializer.Serialize(new { query = context.Request.QueryString.Value })
                : "{}";
        }
        else
        {
            try
            {
                using var doc = JsonDocument.Parse(requestBody);
                requestObjectJson = requestBody;
            }
            catch
            {
                requestObjectJson = JsonSerializer.Serialize(new { raw_content = requestBody });
            }
        }

        int requestSizeBytes = Encoding.UTF8.GetByteCount(requestBody);

        // 2. Interceptar flujo de respuesta para capturar el cuerpo de salida
        var originalBodyStream = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        Exception? caughtException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var responseAt = DateTimeOffset.UtcNow;
            var responseTimeMs = (int)stopwatch.ElapsedMilliseconds;

            responseBodyStream.Position = 0;
            string responseBody = await new StreamReader(responseBodyStream, Encoding.UTF8).ReadToEndAsync();
            responseBodyStream.Position = 0;

            // Devolver cuerpo al cliente
            await responseBodyStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            // Determinar contenido del response_object según tipo de medio
            var contentType = context.Response.ContentType ?? string.Empty;
            string responseObjectJson;
            if (contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                responseObjectJson = JsonSerializer.Serialize(new
                {
                    content_type = contentType,
                    size_bytes = responseBodyStream.Length,
                    note = "Binary content stream"
                });
            }
            else if (string.IsNullOrWhiteSpace(responseBody))
            {
                responseObjectJson = "{}";
            }
            else
            {
                try
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    responseObjectJson = responseBody;
                }
                catch
                {
                    responseObjectJson = JsonSerializer.Serialize(new { raw_content = responseBody });
                }
            }

            // Serializar encabezados de respuesta
            var respHeadersDict = new Dictionary<string, string>();
            foreach (var (k, v) in context.Response.Headers)
            {
                respHeadersDict[k] = v.ToString();
            }
            var responseHeadersJson = JsonSerializer.Serialize(respHeadersDict);

            // Metadatos
            var correlationId = Guid.TryParse(context.TraceIdentifier, out var parsedGuid)
                ? parsedGuid
                : Guid.NewGuid();

            Guid? transactionRef = null;
            if (context.Request.Headers.TryGetValue("X-Transaction-Ref", out var txRefHeader) &&
                Guid.TryParse(txRefHeader, out var parsedTxRef))
            {
                transactionRef = parsedTxRef;
            }

            var settings = apiSettings?.Value;
            var systemName = settings?.NombreSistema ?? "PlataformaSoat";
            var apiVersion = settings?.Version ?? "1.0.0";
            var userName = currentUserService.Username ?? context.User.Identity?.Name ?? "anonymous";
            var hostIp = currentUserService.IPAddress ?? context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var hostName = context.Request.Host.Host;
            var statusCode = context.Response.StatusCode;
            var isSuccess = statusCode >= 200 && statusCode < 400 && caughtException == null;

            var logRequest = new AddApiLogRequest
            {
                Direction = "INBOUND",
                CorrelationId = correlationId,
                TransactionRef = transactionRef,
                ChannelType = "HTTP",
                ApiName = systemName,
                ApiVersion = apiVersion,
                RequestAt = requestAt,
                Endpoint = $"{context.Request.Path}{context.Request.QueryString}",
                HttpMethod = context.Request.Method,
                RequestHeaders = requestHeadersJson,
                RequestObject = requestObjectJson,
                RequestSizeBytes = requestSizeBytes,
                ResponseAt = responseAt,
                HttpStatus = statusCode,
                ResponseHeaders = responseHeadersJson,
                ResponseObject = responseObjectJson,
                ResponseSizeBytes = (int)responseBodyStream.Length,
                ResponseTimeMs = responseTimeMs,
                System = systemName,
                UserName = userName,
                RequestHostName = hostName,
                RequestHostIp = hostIp,
                ApiSuccess = isSuccess,
                ErrorCode = !isSuccess ? statusCode.ToString() : null,
                Message = !isSuccess ? $"HTTP Error {statusCode}" : "OK",
                Exception = caughtException?.ToString()
            };

            try
            {
                await apiLogService.AddApiLogAsync(logRequest);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error al invocar logs.psp_add_api_log para el endpoint {Endpoint}", logRequest.Endpoint);
            }
        }
    }
}
