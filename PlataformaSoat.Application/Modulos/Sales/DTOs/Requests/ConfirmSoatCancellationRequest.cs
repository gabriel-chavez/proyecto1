using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

/// <summary>
/// Parámetros para confirmar la anulación de una póliza SOAT (sales.psp_03_confirm_soat_cancellation).
/// </summary>
public class ConfirmSoatCancellationRequest
{
    [JsonPropertyName("tb_soat_cancellation_id")]
    public long TbSoatCancellationId { get; set; }

    [JsonPropertyName("confirmed_by")]
    public string ConfirmedBy { get; set; } = string.Empty;
}
