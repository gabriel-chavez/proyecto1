using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Responses;
using PlataformaSoat.Application.Modulos.SalesParams.Interfaces;

namespace PlataformaSoat.Application.Modulos.SalesParams.Services;

public class SalesParamsService
{
    private readonly ISalesParamsRepository _repository;
    private readonly ILogger<SalesParamsService> _logger;

    public SalesParamsService(
        ISalesParamsRepository repository,
        ILogger<SalesParamsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene los parámetros de venta SOAT consolidados (departamentos, gestiones, usos y tipos de vehículo).
    /// </summary>
    public async Task<BaseResponse<SoatParametersResponse>> GetSoatParametersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando parámetros SOAT consolidados...");
        return await _repository.ListSoatParametersAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene el catálogo de tipos de placa habilitados.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> GetPlateTypesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando catálogo de tipos de placa...");
        return await _repository.ListPlateTypesAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene las modalidades y canales de venta habilitados.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> GetSalesChannelsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando catálogo de canales y modalidades de venta...");
        return await _repository.ListSalesChannelsAsync(cancellationToken);
    }
}
