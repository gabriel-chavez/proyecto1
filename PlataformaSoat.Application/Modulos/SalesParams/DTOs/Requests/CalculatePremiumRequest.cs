using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.SalesParams.DTOs.Requests;

/// <summary>
/// Parámetros requeridos para calcular la prima SOAT mediante sales_params.psp_04_calculate_premium.
/// </summary>
public class CalculatePremiumRequest
{
    [JsonPropertyName("tb_policy_year_id")]
    public int TbPolicyYearId { get; set; }

    [JsonPropertyName("tb_usage_id")]
    public int TbUsageId { get; set; }

    [JsonPropertyName("tb_vehicle_type_id")]
    public int TbVehicleTypeId { get; set; }

    [JsonPropertyName("tb_department_id")]
    public int TbDepartmentId { get; set; }
}
