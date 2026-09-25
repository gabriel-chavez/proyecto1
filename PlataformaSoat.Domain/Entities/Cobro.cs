using PlataformaSoat.Domain.Core;
using PlataformaSoat.Domain.Enums;

namespace PlataformaSoat.Domain.Entities;

public class Cobro : BaseEntity
{
    public int TPlanPagoDetalleFk { get; set; }
    public decimal Monto { get; set; }
    public EstadoCobro Estado { get; set; }
    public string MotivoReversion { get; set; } = string.Empty;

    public void Revertir(string motivo)
    {
        if (Estado == EstadoCobro.Revertido)
            throw new InvalidOperationException("El cobro ya se encuentra revertido.");

        Estado = EstadoCobro.Revertido;
        MotivoReversion = motivo;
    }
}
