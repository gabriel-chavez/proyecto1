using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Requests;
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

    /// <summary>
    /// Calcula la prima SOAT según gestión, uso, tipo de vehículo y departamento.
    /// Invoca sales_params.psp_04_calculate_premium.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> CalculatePremiumAsync(
        CalculatePremiumRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculando prima SOAT para Gestión: {PolicyYearId}, Uso: {UsageId}, TipoVehículo: {VehicleTypeId}, Depto: {DepartmentId}...",
            request.TbPolicyYearId, request.TbUsageId, request.TbVehicleTypeId, request.TbDepartmentId);

        return await _repository.CalculatePremiumAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sobrecarga para calcular la prima SOAT pasando identificadores individuales.
    /// </summary>
    public Task<BaseResponse<JsonElement>> CalculatePremiumAsync(
        int policyYearId,
        int usageId,
        int vehicleTypeId,
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        return CalculatePremiumAsync(new CalculatePremiumRequest
        {
            TbPolicyYearId = policyYearId,
            TbUsageId = usageId,
            TbVehicleTypeId = vehicleTypeId,
            TbDepartmentId = departmentId
        }, cancellationToken);
    }
}
