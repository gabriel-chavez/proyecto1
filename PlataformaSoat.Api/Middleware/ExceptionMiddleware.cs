using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Common.Exceptions;

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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            BusinessException businessEx => new ApiResponse<object>
            {
                Exito = false,
                CodigoRetorno = businessEx.ErrorCode,
                Mensaje = businessEx.Message,
                Resultado = null
            },
            ValidationException validationEx => new ApiResponse<object>
            {
                Exito = false,
                CodigoRetorno = 400,
                Mensaje = validationEx.Message,
                Resultado = null
            },
            _ => new ApiResponse<object>
            {
                Exito = false,
                CodigoRetorno = 500,
                Mensaje = "Ocurrió un error interno en el servidor",
                Resultado = null
            }
        };

        // Incluir Correlation ID en respuestas
        response.CorrelationId = context.TraceIdentifier;

        context.Response.StatusCode = response.CodigoRetorno >= 400 && response.CodigoRetorno < 500 
            ? response.CodigoRetorno 
            : 500;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(json);
    }
}
