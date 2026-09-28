using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

/// <summary>
/// Parámetros para registrar una venta masiva SOAT (sales.psp_01_register_bulk_soat_sale).
/// </summary>
public class RegisterBulkSoatSaleRequest
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

    [JsonPropertyName("soats")]
    public JsonElement Soats { get; set; }
}
