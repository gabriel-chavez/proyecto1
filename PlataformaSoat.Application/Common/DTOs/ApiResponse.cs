using System;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Common.DTOs;

/// <summary>
/// Envoltorio estándar de respuesta HTTP para la API.
/// Comparte e implementa la misma estructura exacta que los procedimientos almacenados en PostgreSQL:
/// - response (jsonb / T?)
/// - success (boolean)
/// - status_message (text)
/// - response_message (text)
/// - error_message (text)
///
/// Nota: CorrelationId y Timestamp se ignoran en el cuerpo JSON para mantener
/// uniformidad 1:1 con los parámetros del SP. El CorrelationId se envía en el encabezado HTTP 'X-Correlation-ID'.
/// </summary>
/// <typeparam name="T">Tipo del payload contenido en response</typeparam>
public class ApiResponse<T> : BaseResponse<T>
{
    [JsonIgnore]
    public string? CorrelationId { get; set; }

    [JsonIgnore]
    public DateTime? Timestamp { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(
        bool success,
        T? response = default,
        string? responseMessage = null,
        string? statusMessage = null,
        string? errorMessage = null,
        string? correlationId = null)
        : base(success, response, responseMessage, statusMessage, errorMessage)
    {
        CorrelationId = correlationId;
        Timestamp = DateTime.UtcNow;
    }

    public new static ApiResponse<T> Success(
        T? data,
        string? responseMessage = "Operación exitosa",
        string? statusMessage = "SUCCESS")
    {
        return new ApiResponse<T>(
            success: true,
            response: data,
            responseMessage: responseMessage,
            statusMessage: statusMessage,
            errorMessage: null);
    }

    public new static ApiResponse<T> Success(
        T? data,
        string? responseMessage,
        int statusCode,
        string? statusMessage = "SUCCESS")
    {
        return new ApiResponse<T>(
            success: true,
            response: data,
            responseMessage: responseMessage,
            statusMessage: statusMessage,
            errorMessage: null);
    }

    public static ApiResponse<T> Failure(
        string? errorMessage,
        string? responseMessage = null,
        string? statusMessage = "ERROR",
        T? data = default)
    {
        return new ApiResponse<T>(
            success: false,
            response: data,
            responseMessage: responseMessage ?? errorMessage,
            statusMessage: statusMessage,
            errorMessage: errorMessage);
    }

    public static ApiResponse<T> Failure(
        string? errorMessage,
        int statusCode,
        string? responseMessage = null,
        string? statusMessage = "ERROR",
        T? data = default)
    {
        return new ApiResponse<T>(
            success: false,
            response: data,
            responseMessage: responseMessage ?? errorMessage,
            statusMessage: statusMessage,
            errorMessage: errorMessage);
    }

    /// <summary>
    /// Construye un ApiResponse envolviendo directamente el BaseResponse devuelto por el procedimiento almacenado.
    /// </summary>
    public static ApiResponse<T> FromBaseResponse(BaseResponse<T> baseResponse, string? correlationId = null)
    {
        return new ApiResponse<T>(
            success: baseResponse.Success,
            response: baseResponse.Response,
            responseMessage: baseResponse.ResponseMessage,
            statusMessage: baseResponse.StatusMessage,
            errorMessage: baseResponse.ErrorMessage,
            correlationId: correlationId);
    }
}

/// <summary>
/// Métodos estáticos de conveniencia no genéricos para construir respuestas uniformes.
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<object> Success(
        object? data = null,
        string? responseMessage = "Operación exitosa",
        string? statusMessage = "SUCCESS")
        => ApiResponse<object>.Success(data, responseMessage, statusMessage);

    public static ApiResponse<object> Failure(
        string? errorMessage,
        string? responseMessage = null,
        string? statusMessage = "ERROR",
        object? data = null)
        => ApiResponse<object>.Failure(errorMessage, responseMessage, statusMessage, data);
}
