using System;

namespace PlataformaSoat.Application.Common.DTOs;

public class ApiResponse<T>
{
    public bool Exito { get; set; }
    public int CodigoRetorno { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Resultado { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public static ApiResponse<T> Success(T data, string message = "Operación exitosa", int code = 0)
    {
        return new ApiResponse<T>
        {
            Exito = true,
            CodigoRetorno = code,
            Mensaje = message,
            Resultado = data
        };
    }
    
    public static ApiResponse<T> Failure(string message, int code = 500)
    {
        return new ApiResponse<T>
        {
            Exito = false,
            CodigoRetorno = code,
            Mensaje = message,
            Resultado = default
        };
    }
}
