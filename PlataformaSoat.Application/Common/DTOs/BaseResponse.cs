using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Common.DTOs;

/// <summary>
/// Respuesta base estándar alineada a las convenciones de procedimientos almacenados en PostgreSQL:
/// INOUT o_success boolean,
/// INOUT o_status_message text,
/// INOUT o_response_message text,
/// INOUT o_error_message text
/// </summary>
public class BaseResponse
{
    [JsonPropertyOrder(2)]
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("status_message")]
    public string? StatusMessage { get; set; }

    [JsonPropertyOrder(4)]
    [JsonPropertyName("response_message")]
    public string? ResponseMessage { get; set; }

    [JsonPropertyOrder(5)]
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    public BaseResponse()
    {
    }

    public BaseResponse(bool success, string? responseMessage = null, string? statusMessage = null, string? errorMessage = null)
    {
        Success = success;
        ResponseMessage = responseMessage;
        StatusMessage = statusMessage;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Respuesta base genérica que incluye el payload tipado 'response' (T?), correspondiente a:
/// INOUT o_response jsonb
/// </summary>
/// <typeparam name="T">Tipo del payload devuelto</typeparam>
public class BaseResponse<T> : BaseResponse
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("response")]
    public T? Response { get; set; }

    public BaseResponse()
    {
    }

    public BaseResponse(bool success, T? response = default, string? responseMessage = null, string? statusMessage = null, string? errorMessage = null)
        : base(success, responseMessage, statusMessage, errorMessage)
    {
        Response = response;
    }

    public static BaseResponse<T> Ok(T? response, string? responseMessage = null, string? statusMessage = "SUCCESS")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Response = response,
            ResponseMessage = responseMessage,
            StatusMessage = statusMessage,
            ErrorMessage = null
        };
    }

    public static BaseResponse<T> Fail(string? errorMessage, string? responseMessage = null, string? statusMessage = "ERROR", T? response = default)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Response = response,
            ResponseMessage = responseMessage ?? errorMessage,
            StatusMessage = statusMessage,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Mapea directamente desde el diccionario de parámetros INOUT devueltos por PostgreSQL
    /// (soporta tanto prefijo 'o_' como 'p_' o directo).
    /// </summary>
    public static BaseResponse<T> FromDictionary(IReadOnlyDictionary<string, object?> dict)
    {
        bool success = false;
        if (dict.TryGetValue("o_success", out var s) || dict.TryGetValue("p_success", out s) || dict.TryGetValue("success", out s))
        {
            if (s is bool b) success = b;
            else if (bool.TryParse(s?.ToString(), out var parsedB)) success = parsedB;
        }

        string? statusMessage = (dict.TryGetValue("o_status_message", out var sm) || dict.TryGetValue("p_status_message", out sm) || dict.TryGetValue("status_message", out sm))
            ? sm?.ToString() : null;

        string? responseMessage = (dict.TryGetValue("o_response_message", out var rm) || dict.TryGetValue("p_response_message", out rm) || dict.TryGetValue("response_message", out rm))
            ? rm?.ToString() : null;

        string? errorMessage = (dict.TryGetValue("o_error_message", out var em) || dict.TryGetValue("p_error_message", out em) || dict.TryGetValue("error_message", out em))
            ? em?.ToString() : null;

        T? response = default;
        if (dict.TryGetValue("o_response", out var r) || dict.TryGetValue("p_response", out r) || dict.TryGetValue("response", out r))
        {
            if (r != null && r != DBNull.Value)
            {
                if (r is T directVal)
                {
                    response = directVal;
                }
                else if (r is JsonDocument jDoc && typeof(T) == typeof(JsonElement))
                {
                    response = (T)(object)jDoc.RootElement.Clone();
                }
                else if (typeof(T) == typeof(string))
                {
                    response = (T)(object)r.ToString()!;
                }
                else
                {
                    var jsonStr = r.ToString();
                    if (!string.IsNullOrWhiteSpace(jsonStr))
                    {
                        try
                        {
                            response = JsonSerializer.Deserialize<T>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch
                        {
                            // Ignora o mantiene default si no se puede deserializar
                        }
                    }
                }
            }
        }

        return new BaseResponse<T>
        {
            Success = success,
            Response = response,
            StatusMessage = statusMessage,
            ResponseMessage = responseMessage,
            ErrorMessage = errorMessage
        };
    }
}
