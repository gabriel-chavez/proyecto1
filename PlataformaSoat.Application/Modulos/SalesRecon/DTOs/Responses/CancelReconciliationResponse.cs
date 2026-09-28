using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Responses;

/// <summary>
/// Payload devuelto por sales_recon.psp_03_cancel_reconciliation
/// </summary>
public class CancelReconciliationResponse
{
    [JsonPropertyName("cancellation")]
    public JsonElement? Cancellation { get; set; }

    [JsonPropertyName("reconciled_soats")]
    public JsonElement? ReconciledSoats { get; set; }
}
