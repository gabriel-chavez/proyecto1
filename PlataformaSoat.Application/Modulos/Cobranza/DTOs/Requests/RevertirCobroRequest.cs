namespace PlataformaSoat.Application.Modulos.Cobranza.DTOs.Requests;

public class RevertirCobroRequest
{
    public string UsuarioAut { get; set; } = string.Empty;
    public int SeguridadToken { get; set; }
    public int TPlanPagoDetalleFk { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Metodo { get; set; } = string.Empty;
    public string? OrigenTransaccionJson { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
