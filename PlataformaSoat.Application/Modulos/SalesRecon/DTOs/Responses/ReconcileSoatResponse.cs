using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Responses;

/// <summary>
/// Payload devuelto por sales_recon.psp_02_reconcile_soat
/// </summary>
public class ReconcileSoatResponse
{
    [JsonPropertyName("reconciliation")]
    public JsonElement? Reconciliation { get; set; }

    [JsonPropertyName("reconciled_soats")]
    public JsonElement? ReconciledSoats { get; set; }

    [JsonPropertyName("previously_reconciled")]
    public JsonElement? PreviouslyReconciled { get; set; }
}
