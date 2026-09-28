using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

/// <summary>
/// Parámetros para solicitar la anulación de una póliza SOAT (sales.psp_02_request_soat_cancellation).
/// </summary>
public class RequestSoatCancellationRequest
{
    [JsonPropertyName("registered_by")]
    public string RegisteredBy { get; set; } = string.Empty;

    [JsonPropertyName("tb_soat_policy_id")]
    public long TbSoatPolicyId { get; set; }

    [JsonPropertyName("cancellation_reason")]
    public string CancellationReason { get; set; } = string.Empty;

    [JsonPropertyName("requested_by")]
    public string RequestedBy { get; set; } = string.Empty;
}
