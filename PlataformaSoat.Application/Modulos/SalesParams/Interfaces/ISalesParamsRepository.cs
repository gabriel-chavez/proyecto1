using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Responses;

namespace PlataformaSoat.Application.Modulos.SalesParams.Interfaces;

/// <summary>
/// Contrato del repositorio para la consulta de parámetros de venta desde procedimientos almacenados en PostgreSQL.
/// </summary>
public interface ISalesParamsRepository
{
    /// <summary>
    /// Invoca sales_params.psp_01_list_soat_parameters
    /// </summary>
    Task<BaseResponse<SoatParametersResponse>> ListSoatParametersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales_params.psp_02_list_plate_types
    /// </summary>
    Task<BaseResponse<JsonElement>> ListPlateTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invoca sales_params.psp_03_list_sales_channels
    /// </summary>
    Task<BaseResponse<JsonElement>> ListSalesChannelsAsync(CancellationToken cancellationToken = default);
}
