namespace PlataformaSoat.Application.Common.DTOs;

public class ErrorResponse
{
    public string Mensaje { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public List<string> Errores { get; set; } = new();
}
