using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Modulos.Cobranza.DTOs.Requests;
using PlataformaSoat.Application.Modulos.Cobranza.Services;
using PlataformaSoat.Application.Common.DTOs;

namespace PlataformaSoat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CobranzaController : BaseApiController
{
    private readonly CobranzaService _cobranzaService;
    private readonly ILogger<CobranzaController> _logger;

    public CobranzaController(
        CobranzaService cobranzaService,
        ILogger<CobranzaController> logger)
    {
        _cobranzaService = cobranzaService;
        _logger = logger;
    }

    /// <summary>
    /// Revertir un cobro existente
    /// </summary>
    /// <param name="request">Datos para la reversión</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("cobros/reversion")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevertirCobro(
        [FromBody] RevertirCobroRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recibida solicitud de reversión de cobro - Detalle: {Detalle}", 
            request.TPlanPagoDetalleFk);
      

        var resultado = await _cobranzaService.RevertirCobroAsync(request, cancellationToken);

        if (!resultado.Exito)
        {
            if (resultado.CodigoRetorno >= 400 && resultado.CodigoRetorno < 500)
                return BadRequest(resultado);
            
            if (resultado.CodigoRetorno >= 500 || resultado.CodigoRetorno == -1)
                return StatusCode(StatusCodes.Status500InternalServerError, resultado);
        }

        return Ok(resultado);
    }
    /// <summary>
    /// Registrar una nueva póliza SOAT
    /// </summary>
    /// <param name="request">Datos de la póliza a registrar</param>
    /// <param name="cancellationToken">Token de cancelación opcional</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("soat/registrar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterSoatPolicy(
        [FromBody] RegisterSoatPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recibida solicitud de registro de póliza SOAT - Cliente: {Cliente}, Vehículo: {Vehiculo}",
            request.ClientId, request.VehicleId);

        var resultado = await _cobranzaService.RegisterSoatPolicyAsync(request, cancellationToken);

        if (!resultado.Exito)
        {
            if (resultado.CodigoRetorno >= 400 && resultado.CodigoRetorno < 500)
                return BadRequest(resultado);
            return StatusCode(StatusCodes.Status500InternalServerError, resultado);
        }

        return Ok(resultado);
    }
}

