using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Responses;

namespace PlataformaSoat.Application.Modulos.Sales.Interfaces;

/// <summary>
/// Contrato de persistencia para el módulo de ventas SOAT mediante Stored Procedures en PostgreSQL.
/// </summary>
public interface ISalesRepository
{
    /// <summary>
    /// Invoca sales.psp_01_register_soat_sale
    /// </summary>
    Task<BaseResponse<JsonElement>> RegisterSoatSaleAsync(RegisterSoatSaleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales.psp_01_register_bulk_soat_sale
    /// </summary>
    Task<BaseResponse<BulkSoatSaleResponse>> RegisterBulkSoatSaleAsync(RegisterBulkSoatSaleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales.psp_02_request_soat_cancellation
    /// </summary>
    Task<BaseResponse<JsonElement>> RequestSoatCancellationAsync(RequestSoatCancellationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales.psp_03_confirm_soat_cancellation
    /// </summary>
    Task<BaseResponse<JsonElement>> ConfirmSoatCancellationAsync(ConfirmSoatCancellationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales.psp_04_list_soat_sales
    /// </summary>
    Task<BaseResponse<JsonElement>> ListSoatSalesAsync(ListSoatSalesFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales.psp_05_get_soat_sale
    /// </summary>
    Task<BaseResponse<JsonElement>> GetSoatSaleAsync(long tbSoatPolicyId, CancellationToken cancellationToken = default);
}
