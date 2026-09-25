using System;

namespace PlataformaSoat.Application.Modulos.Cobranza.DTOs.Responses;

public class RevertirCobroResponse
{
    public string? CodigoReversion { get; set; }
    public string? EstadoTransaccion { get; set; }
    public DateTime? FechaTransaccion { get; set; }
}
