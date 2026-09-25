using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PlataformaSoat.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        _logger.LogInformation("HTTP Request In: {Method} {Path} {QueryString}", 
            request.Method, request.Path, request.QueryString);

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            var response = context.Response;

            _logger.LogInformation("HTTP Request Out: {Method} {Path} responded {StatusCode} in {ElapsedMs} ms", 
                request.Method, request.Path, response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "HTTP Request Failed: {Method} {Path} failed in {ElapsedMs} ms", 
                request.Method, request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
