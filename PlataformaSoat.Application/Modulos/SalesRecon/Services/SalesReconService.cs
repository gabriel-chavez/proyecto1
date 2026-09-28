using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Responses;
using PlataformaSoat.Application.Modulos.SalesRecon.Interfaces;

namespace PlataformaSoat.Application.Modulos.SalesRecon.Services;

/// <summary>
/// Servicio de aplicación para la gestión de conciliaciones de ventas SOAT.
/// </summary>
public class SalesReconService
{
    private readonly ISalesReconRepository _repository;
    private readonly ILogger<SalesReconService> _logger;

    public SalesReconService(
        ISalesReconRepository repository,
        ILogger<SalesReconService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Consulta los SOAT pendientes de conciliación ejecutando sales_recon.psp_01_list_unreconciled_soat.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ListUnreconciledSoatAsync(
        ListUnreconciledSoatFilter filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando SOAT pendientes de conciliación para Broker {BrokerId} ({StartDate} a {EndDate})",
            filter.TbBrokerId, filter.StartDate, filter.EndDate);

        return await _repository.ListUnreconciledSoatAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Ejecuta la conciliación de pólizas SOAT ejecutando sales_recon.psp_02_reconcile_soat.
    /// </summary>
    public async Task<BaseResponse<ReconcileSoatResponse>> ReconcileSoatAsync(
        ReconcileSoatRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando conciliación SOAT para Broker {BrokerId} por {RegisteredBy} en {Institution}",
            request.TbBrokerId, request.RegisteredBy, request.FinancialInstitution);

        return await _repository.ReconcileSoatAsync(request, cancellationToken);
    }

    /// <summary>
    /// Anula una conciliación previamente registrada ejecutando sales_recon.psp_03_cancel_reconciliation.
    /// </summary>
    public async Task<BaseResponse<CancelReconciliationResponse>> CancelReconciliationAsync(
        CancelReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando anulación de conciliación {ReconciliationId} por {CancelledBy}",
            request.TbReconciliationId, request.CancelledBy);

        return await _repository.CancelReconciliationAsync(request, cancellationToken);
    }
}
