using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Responses;
using PlataformaSoat.Application.Modulos.Sales.Interfaces;
using PlataformaSoat.Infrastructure.Extensions;
using PlataformaSoat.Infrastructure.Persistence;

namespace PlataformaSoat.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para el módulo de ventas mediante Stored Procedures en PostgreSQL.
/// </summary>
public class SalesRepository : BaseRepository, ISalesRepository
{
    public SalesRepository(ApplicationDbContext dbContext, ILogger<SalesRepository> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Invoca sales.psp_01_register_soat_sale
    /// </summary>
    public async Task<BaseResponse<JsonElement>> RegisterSoatSaleAsync(
        RegisterSoatSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales.psp_01_register_soat_sale para {PlateOrChassis} - Factura: {InvoiceNumber}",
            request.PlateOrChassis, request.InvoiceNumber);

        var registeredBy = ResolveRegisteredBy(request.RegisteredBy);

        var input = new InParam[]
        {
            new("i_registered_by", registeredBy, NpgsqlDbType.Varchar),
            new("i_tb_branch_id", request.TbBranchId, NpgsqlDbType.Integer),
            new("i_tb_broker_id", request.TbBrokerId, NpgsqlDbType.Integer),
            new("i_tb_commercializer_id", request.TbCommercializerId, NpgsqlDbType.Integer),
            new("i_tb_sales_channel_id", request.TbSalesChannelId, NpgsqlDbType.Integer),
            new("i_business_name", request.BusinessName, NpgsqlDbType.Varchar),
            new("i_tax_or_identity_number", request.TaxOrIdentityNumber, NpgsqlDbType.Varchar),
            new("i_cufd", request.Cufd, NpgsqlDbType.Varchar),
            new("i_invoice_number", request.InvoiceNumber, NpgsqlDbType.Bigint),
            new("i_tb_department_id", request.TbDepartmentId, NpgsqlDbType.Integer),
            new("i_tb_policy_year_id", request.TbPolicyYearId, NpgsqlDbType.Integer),
            new("i_tb_usage_id", request.TbUsageId, NpgsqlDbType.Integer),
            new("i_tb_vehicle_type_id", request.TbVehicleTypeId, NpgsqlDbType.Integer),
            new("i_tb_plate_type_id", request.TbPlateTypeId, NpgsqlDbType.Integer),
            new("i_paid_premium", request.PaidPremium, NpgsqlDbType.Numeric),
            new("i_plate_or_chassis", request.PlateOrChassis, NpgsqlDbType.Varchar),
            new("i_policyholder", request.Policyholder, NpgsqlDbType.Varchar)
        };

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales.psp_01_register_soat_sale",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales.psp_01_register_bulk_soat_sale
    /// </summary>
    public async Task<BaseResponse<BulkSoatSaleResponse>> RegisterBulkSoatSaleAsync(
        RegisterBulkSoatSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales.psp_01_register_bulk_soat_sale para {BusinessName}",
            request.BusinessName);

        var soatsJson = request.Soats.ValueKind == JsonValueKind.Undefined
            ? "[]"
            : request.Soats.GetRawText();

        var registeredBy = ResolveRegisteredBy(request.RegisteredBy);

        var input = new InParam[]
        {
            new("i_registered_by", registeredBy, NpgsqlDbType.Varchar),
            new("i_tb_branch_id", request.TbBranchId, NpgsqlDbType.Integer),
            new("i_tb_broker_id", request.TbBrokerId, NpgsqlDbType.Integer),
            new("i_tb_commercializer_id", request.TbCommercializerId, NpgsqlDbType.Integer),
            new("i_tb_sales_channel_id", request.TbSalesChannelId, NpgsqlDbType.Integer),
            new("i_business_name", request.BusinessName, NpgsqlDbType.Varchar),
            new("i_tax_or_identity_number", request.TaxOrIdentityNumber, NpgsqlDbType.Varchar),
            new("i_soats", soatsJson, NpgsqlDbType.Jsonb)
        };

        return await _dbContext.ExecuteProcedureAsync<BulkSoatSaleResponse>(
            procedureName: "sales.psp_01_register_bulk_soat_sale",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales.psp_02_request_soat_cancellation
    /// </summary>
    public async Task<BaseResponse<JsonElement>> RequestSoatCancellationAsync(
        RequestSoatCancellationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales.psp_02_request_soat_cancellation para póliza {PolicyId}",
            request.TbSoatPolicyId);

        var registeredBy = ResolveRegisteredBy(request.RegisteredBy);

        var input = new InParam[]
        {
            new("i_registered_by", registeredBy, NpgsqlDbType.Varchar),
            new("i_tb_soat_policy_id", request.TbSoatPolicyId, NpgsqlDbType.Bigint),
            new("i_cancellation_reason", request.CancellationReason, NpgsqlDbType.Text),
            new("i_requested_by", request.RequestedBy, NpgsqlDbType.Varchar)
        };

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales.psp_02_request_soat_cancellation",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales.psp_03_confirm_soat_cancellation
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ConfirmSoatCancellationAsync(
        ConfirmSoatCancellationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales.psp_03_confirm_soat_cancellation para solicitud {CancellationId}",
            request.TbSoatCancellationId);

        var input = new InParam[]
        {
            new("i_tb_soat_cancellation_id", request.TbSoatCancellationId, NpgsqlDbType.Bigint),
            new("i_confirmed_by", request.ConfirmedBy, NpgsqlDbType.Varchar)
        };

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales.psp_03_confirm_soat_cancellation",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales.psp_04_list_soat_sales
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ListSoatSalesAsync(
        ListSoatSalesFilter filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales.psp_04_list_soat_sales");

        var input = new InParam[]
        {
            new("i_tb_broker_id", filter.TbBrokerId, NpgsqlDbType.Integer),
            new("i_tb_commercializer_id", filter.TbCommercializerId, NpgsqlDbType.Integer),
            new("i_tb_sales_channel_id", filter.TbSalesChannelId, NpgsqlDbType.Integer),
            new("i_start_date", filter.StartDate, NpgsqlDbType.Date),
            new("i_end_date", filter.EndDate, NpgsqlDbType.Date)
        };

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales.psp_04_list_soat_sales",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales.psp_05_get_soat_sale
    /// </summary>
    public async Task<BaseResponse<JsonElement>> GetSoatSaleAsync(
        long tbSoatPolicyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales.psp_05_get_soat_sale para póliza {PolicyId}",
            tbSoatPolicyId);

        var input = new InParam[]
        {
            new("i_tb_soat_policy_id", tbSoatPolicyId, NpgsqlDbType.Bigint)
        };

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales.psp_05_get_soat_sale",
            input: input,
            cancellationToken: cancellationToken);
    }
}
