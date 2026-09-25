namespace PlataformaSoat.Application.Common.DTOs;

public class BaseResponse
{
    public bool Exito { get; set; }
    public int CodigoRetorno { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

public class BaseResponse<T> : BaseResponse
{
    public T? Resultado { get; set; }
}
