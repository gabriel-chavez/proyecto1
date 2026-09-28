using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Requests;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Responses;
using PlataformaSoat.Application.Modulos.SalesParams.Interfaces;
using PlataformaSoat.Infrastructure.Extensions;
using PlataformaSoat.Infrastructure.Persistence;

namespace PlataformaSoat.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para consultar catálogos de sales_params mediante Stored Procedures en PostgreSQL.
/// </summary>
public class SalesParamsRepository : BaseRepository, ISalesParamsRepository
{
    public SalesParamsRepository(ApplicationDbContext dbContext, ILogger<SalesParamsRepository> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Invoca sales_params.psp_01_list_soat_parameters
    /// </summary>
    public async Task<BaseResponse<SoatParametersResponse>> ListSoatParametersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento almacenado sales_params.psp_01_list_soat_parameters");

        return await _dbContext.ExecuteProcedureAsync<SoatParametersResponse>(
            procedureName: "sales_params.psp_01_list_soat_parameters",
            input: Array.Empty<InParam>(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales_params.psp_02_list_plate_types
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ListPlateTypesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento almacenado sales_params.psp_02_list_plate_types");

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales_params.psp_02_list_plate_types",
            input: Array.Empty<InParam>(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales_params.psp_03_list_sales_channels
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ListSalesChannelsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento almacenado sales_params.psp_03_list_sales_channels");

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales_params.psp_03_list_sales_channels",
            input: Array.Empty<InParam>(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales_params.psp_04_calculate_premium
    /// </summary>
    public async Task<BaseResponse<JsonElement>> CalculatePremiumAsync(
        CalculatePremiumRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento almacenado sales_params.psp_04_calculate_premium para Gestión: {PolicyYearId}, Uso: {UsageId}, TipoVehículo: {VehicleTypeId}, Depto: {DepartmentId}",
            request.TbPolicyYearId, request.TbUsageId, request.TbVehicleTypeId, request.TbDepartmentId);

        var input = new InParam[]
        {
            new("i_tb_policy_year_id", request.TbPolicyYearId, NpgsqlDbType.Integer),
            new("i_tb_usage_id", request.TbUsageId, NpgsqlDbType.Integer),
            new("i_tb_vehicle_type_id", request.TbVehicleTypeId, NpgsqlDbType.Integer),
            new("i_tb_department_id", request.TbDepartmentId, NpgsqlDbType.Integer)
        };

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales_params.psp_04_calculate_premium",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sobrecarga para invocar sales_params.psp_04_calculate_premium con parámetros individuales
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
