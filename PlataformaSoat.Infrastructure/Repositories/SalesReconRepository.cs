using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Responses;
using PlataformaSoat.Application.Modulos.SalesRecon.Interfaces;
using PlataformaSoat.Infrastructure.Extensions;
using PlataformaSoat.Infrastructure.Persistence;

namespace PlataformaSoat.Infrastructure.Repositories;

/// <summary>
/// Implementación de persistencia para el módulo de conciliación de ventas mediante Stored Procedures en PostgreSQL.
/// </summary>
public class SalesReconRepository : BaseRepository, ISalesReconRepository
{
    public SalesReconRepository(ApplicationDbContext dbContext, ILogger<SalesReconRepository> logger)
        : base(dbContext, logger)
    {
    }

    /// <summary>
    /// Invoca sales_recon.psp_01_list_unreconciled_soat
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ListUnreconciledSoatAsync(
        ListUnreconciledSoatFilter filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales_recon.psp_01_list_unreconciled_soat para Broker {BrokerId}",
            filter.TbBrokerId);

        var input = new InParam[]
        {
            new("i_tb_broker_id", filter.TbBrokerId, NpgsqlDbType.Integer),
            new("i_start_date", filter.StartDate, NpgsqlDbType.Date),
            new("i_end_date", filter.EndDate, NpgsqlDbType.Date),
            new("i_is_valid", filter.IsValid, NpgsqlDbType.Boolean)
        };

        return await _dbContext.ExecuteProcedureAsync<JsonElement>(
            procedureName: "sales_recon.psp_01_list_unreconciled_soat",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales_recon.psp_02_reconcile_soat
    /// </summary>
    public async Task<BaseResponse<ReconcileSoatResponse>> ReconcileSoatAsync(
        ReconcileSoatRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales_recon.psp_02_reconcile_soat para Broker {BrokerId}",
            request.TbBrokerId);

        var input = new InParam[]
        {
            new("i_registered_by", request.RegisteredBy, NpgsqlDbType.Varchar),
            new("i_tb_reconciliation_type_id", request.TbReconciliationTypeId, NpgsqlDbType.Integer),
            new("i_tb_broker_id", request.TbBrokerId, NpgsqlDbType.Integer),
            new("i_start_date", request.StartDate, NpgsqlDbType.Date),
            new("i_end_date", request.EndDate, NpgsqlDbType.Date),
            new("i_is_valid", request.IsValid, NpgsqlDbType.Boolean),
            new("i_financial_institution", request.FinancialInstitution, NpgsqlDbType.Varchar),
            new("i_transfer_transaction_count", request.TransferTransactionCount, NpgsqlDbType.Integer),
            new("i_transfer_date", request.TransferDate, NpgsqlDbType.Date)
        };

        return await _dbContext.ExecuteProcedureAsync<ReconcileSoatResponse>(
            procedureName: "sales_recon.psp_02_reconcile_soat",
            input: input,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invoca sales_recon.psp_03_cancel_reconciliation
    /// </summary>
    public async Task<BaseResponse<CancelReconciliationResponse>> CancelReconciliationAsync(
        CancelReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ejecutando procedimiento sales_recon.psp_03_cancel_reconciliation para conciliación {ReconciliationId}",
            request.TbReconciliationId);

        var input = new InParam[]
        {
            new("i_registered_by", request.RegisteredBy, NpgsqlDbType.Varchar),
            new("i_tb_reconciliation_id", request.TbReconciliationId, NpgsqlDbType.Bigint),
            new("i_cancelled_by", request.CancelledBy, NpgsqlDbType.Varchar),
            new("i_cancellation_reason", request.CancellationReason, NpgsqlDbType.Text)
        };

        return await _dbContext.ExecuteProcedureAsync<CancelReconciliationResponse>(
            procedureName: "sales_recon.psp_03_cancel_reconciliation",
            input: input,
            cancellationToken: cancellationToken);
    }
}
