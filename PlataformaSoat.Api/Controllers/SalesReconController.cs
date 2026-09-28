using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Requests;
using PlataformaSoat.Application.Modulos.SalesRecon.DTOs.Responses;
using PlataformaSoat.Application.Modulos.SalesRecon.Services;

namespace PlataformaSoat.Api.Controllers;

[ApiController]
[Route("api/sales-recon")]
[Produces("application/json")]
public class SalesReconController : BaseApiController
{
    private readonly SalesReconService _salesReconService;
    private readonly ILogger<SalesReconController> _logger;

    public SalesReconController(
        SalesReconService salesReconService,
        ILogger<SalesReconController> logger)
    {
        _salesReconService = salesReconService;
        _logger = logger;
    }

    /// <summary>
    /// Consulta los SOAT pendientes de conciliación para un broker y rango de fechas.
    /// (Invoca sales_recon.psp_01_list_unreconciled_soat)
    /// </summary>
    /// <param name="filter">Filtros de consulta: broker, rango de fechas y validez</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Detalle de pólizas SOAT pendientes y total conciliado</returns>
    [HttpGet("unreconciled")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListUnreconciledSoat(
        [FromQuery] ListUnreconciledSoatFilter filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales-recon/unreconciled invocado para Broker {BrokerId}", filter.TbBrokerId);
        var result = await _salesReconService.ListUnreconciledSoatAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra la conciliación de ventas SOAT generando la cabecera y asociando los SOAT en bloque.
    /// (Invoca sales_recon.psp_02_reconcile_soat)
    /// </summary>
    /// <param name="request">Datos para la conciliación bancaria y pólizas SOAT</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la conciliación registrada</returns>
    [HttpPost("reconcile")]
    [ProducesResponseType(typeof(BaseResponse<ReconcileSoatResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReconcileSoat(
        [FromBody] ReconcileSoatRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint POST api/sales-recon/reconcile invocado para Broker {BrokerId}", request.TbBrokerId);
        var result = await _salesReconService.ReconcileSoatAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Anula una conciliación previamente efectuada y libera las pólizas SOAT para futuras conciliaciones.
    /// (Invoca sales_recon.psp_03_cancel_reconciliation)
    /// </summary>
    /// <param name="request">Identificador de conciliación, usuario y motivo de anulación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la anulación de conciliación</returns>
    [HttpPost("cancellations")]
    [ProducesResponseType(typeof(BaseResponse<CancelReconciliationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelReconciliation(
        [FromBody] CancelReconciliationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint POST api/sales-recon/cancellations invocado para conciliación {ReconciliationId}", request.TbReconciliationId);
        var result = await _salesReconService.CancelReconciliationAsync(request, cancellationToken);
        return Ok(result);
    }
}
