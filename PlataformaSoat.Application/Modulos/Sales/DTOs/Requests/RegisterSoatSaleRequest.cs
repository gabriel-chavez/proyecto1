using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

/// <summary>
/// Parámetros para registrar una venta individual SOAT (sales.psp_01_register_soat_sale).
/// </summary>
public class RegisterSoatSaleRequest
{
    [JsonPropertyName("registered_by")]
    public string RegisteredBy { get; set; } = string.Empty;

    [JsonPropertyName("tb_branch_id")]
    public int TbBranchId { get; set; }

    [JsonPropertyName("tb_broker_id")]
    public int TbBrokerId { get; set; }

    [JsonPropertyName("tb_commercializer_id")]
    public int TbCommercializerId { get; set; }

    [JsonPropertyName("tb_sales_channel_id")]
    public int TbSalesChannelId { get; set; }

    [JsonPropertyName("business_name")]
    public string BusinessName { get; set; } = string.Empty;

    [JsonPropertyName("tax_or_identity_number")]
    public string TaxOrIdentityNumber { get; set; } = string.Empty;

    [JsonPropertyName("cufd")]
    public string Cufd { get; set; } = string.Empty;

    [JsonPropertyName("invoice_number")]
    public long InvoiceNumber { get; set; }

    [JsonPropertyName("tb_department_id")]
    public int TbDepartmentId { get; set; }

    [JsonPropertyName("tb_policy_year_id")]
    public int TbPolicyYearId { get; set; }

    [JsonPropertyName("tb_usage_id")]
    public int TbUsageId { get; set; }

    [JsonPropertyName("tb_vehicle_type_id")]
    public int TbVehicleTypeId { get; set; }

    [JsonPropertyName("tb_plate_type_id")]
    public int TbPlateTypeId { get; set; }

    [JsonPropertyName("paid_premium")]
    public decimal PaidPremium { get; set; }

    [JsonPropertyName("plate_or_chassis")]
    public string PlateOrChassis { get; set; } = string.Empty;

    [JsonPropertyName("policyholder")]
    public string Policyholder { get; set; } = string.Empty;
}
