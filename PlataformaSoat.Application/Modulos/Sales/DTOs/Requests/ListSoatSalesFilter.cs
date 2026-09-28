using System;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;

/// <summary>
/// Filtros para consultar ventas SOAT (sales.psp_04_list_soat_sales).
/// </summary>
public class ListSoatSalesFilter
{
    [JsonPropertyName("tb_broker_id")]
    public int? TbBrokerId { get; set; }

    [JsonPropertyName("tb_commercializer_id")]
    public int? TbCommercializerId { get; set; }

    [JsonPropertyName("tb_sales_channel_id")]
    public int? TbSalesChannelId { get; set; }

    [JsonPropertyName("start_date")]
    public DateOnly? StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateOnly? EndDate { get; set; }
}
