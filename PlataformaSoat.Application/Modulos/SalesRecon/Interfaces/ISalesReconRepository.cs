using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Responses;

namespace PlataformaSoat.Application.Modulos.SalesRecon.Interfaces;

/// <summary>
/// Contrato de persistencia para el módulo de conciliación de ventas SOAT mediante Stored Procedures en PostgreSQL.
/// </summary>
public interface ISalesReconRepository
{
    /// <summary>
    /// Invoca sales_recon.psp_01_list_unreconciled_soat
    /// </summary>
    Task<BaseResponse<JsonElement>> ListUnreconciledSoatAsync(ListUnreconciledSoatFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales_recon.psp_02_reconcile_soat
    /// </summary>
    Task<BaseResponse<ReconcileSoatResponse>> ReconcileSoatAsync(ReconcileSoatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales_recon.psp_03_cancel_reconciliation
    /// </summary>
    Task<BaseResponse<CancelReconciliationResponse>> CancelReconciliationAsync(CancelReconciliationRequest request, CancellationToken cancellationToken = default);
}
