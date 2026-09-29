using System;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;

/// <summary>
/// Parámetros para registrar una conciliación SOAT (sales_recon.psp_02_reconcile_soat).
/// </summary>
public class ReconcileSoatRequest
{
    [JsonPropertyName("tb_reconciliation_type_id")]
    public int TbReconciliationTypeId { get; set; }

    [JsonPropertyName("tb_broker_id")]
    public int TbBrokerId { get; set; }

    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; set; }

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; } = true;

    [JsonPropertyName("financial_institution")]
    public string FinancialInstitution { get; set; } = string.Empty;

    [JsonPropertyName("transfer_transaction_count")]
    public int TransferTransactionCount { get; set; }

    [JsonPropertyName("transfer_date")]
    public DateOnly TransferDate { get; set; }
}
