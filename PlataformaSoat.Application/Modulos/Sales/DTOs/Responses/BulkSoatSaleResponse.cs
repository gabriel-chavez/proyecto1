using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Responses;

/// <summary>
/// Payload devuelto por sales.psp_01_register_bulk_soat_sale
/// </summary>
public class BulkSoatSaleResponse
{
    [JsonPropertyName("invoice_header")]
    public JsonElement? InvoiceHeader { get; set; }

    [JsonPropertyName("invoice_details")]
    public JsonElement? InvoiceDetails { get; set; }

    [JsonPropertyName("soat_policies")]
    public JsonElement? SoatPolicies { get; set; }
}
