using System;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;

/// <summary>
/// Filtros para consultar SOAT pendientes de conciliación (sales_recon.psp_01_list_unreconciled_soat).
/// </summary>
public class ListUnreconciledSoatFilter
{
    [JsonPropertyName("tb_broker_id")]
    public int TbBrokerId { get; set; }

    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; set; }

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; } = true;
}
