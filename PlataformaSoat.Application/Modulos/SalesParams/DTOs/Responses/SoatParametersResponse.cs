using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.SalesParams.DTOs.Responses;

/// <summary>
/// DTO de respuesta para los parámetros SOAT consolidados:
/// departments, policy_years, usages, vehicle_types.
/// </summary>
public class SoatParametersResponse
{
    [JsonPropertyName("departments")]
    public JsonElement? Departments { get; set; }

    [JsonPropertyName("policy_years")]
    public JsonElement? PolicyYears { get; set; }

    [JsonPropertyName("usages")]
    public JsonElement? Usages { get; set; }

    [JsonPropertyName("vehicle_types")]
    public JsonElement? VehicleTypes { get; set; }
}
