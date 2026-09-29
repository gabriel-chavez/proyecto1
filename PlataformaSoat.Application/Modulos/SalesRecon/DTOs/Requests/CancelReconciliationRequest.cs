using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;

/// <summary>
/// Parámetros para anular una conciliación SOAT (sales_recon.psp_03_cancel_reconciliation).
/// </summary>
public class CancelReconciliationRequest
{
    [JsonPropertyName("tb_reconciliation_id")]
    public long TbReconciliationId { get; set; }

    [JsonPropertyName("cancelled_by")]
    public string CancelledBy { get; set; } = string.Empty;

    [JsonPropertyName("cancellation_reason")]
    public string CancellationReason { get; set; } = string.Empty;
}
