using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Requests;
using PlataformaSoat.Application.Modulos.SalesParams.DTOs.Responses;
using PlataformaSoat.Application.Modulos.SalesParams.Services;

namespace PlataformaSoat.Api.Controllers;

[ApiController]
[Route("api/sales-params")]
[Produces("application/json")]
public class SalesParamsController : BaseApiController
{
    private readonly SalesParamsService _salesParamsService;
    private readonly ILogger<SalesParamsController> _logger;

    public SalesParamsController(
        SalesParamsService salesParamsService,
        ILogger<SalesParamsController> logger)
    {
        _salesParamsService = salesParamsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene los parámetros SOAT consolidados (departamentos, gestiones, usos y tipos de vehículo).
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional</param>
    /// <returns>Catálogos consolidados para emisión SOAT</returns>
    [HttpGet("soat-parameters")]
    [ProducesResponseType(typeof(BaseResponse<SoatParametersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSoatParameters(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales-params/soat-parameters invocado");
        var result = await _salesParamsService.GetSoatParametersAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene los tipos de placa habilitados para vehículos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional</param>
    /// <returns>Lista de tipos de placa</returns>
    [HttpGet("plate-types")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPlateTypes(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales-params/plate-types invocado");
        var result = await _salesParamsService.GetPlateTypesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene las modalidades y canales de venta habilitados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional</param>
    /// <returns>Lista de canales de venta</returns>
    [HttpGet("sales-channels")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalesChannels(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales-params/sales-channels invocado");
        var result = await _salesParamsService.GetSalesChannelsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calcula la prima SOAT según gestión, uso, tipo de vehículo y departamento mediante parámetros de consulta (GET).
    /// (Invoca sales_params.psp_04_calculate_premium)
    /// </summary>
    /// <param name="request">Parámetros de consulta para el cálculo de prima</param>
    /// <param name="cancellationToken">Token de cancelación opcional</param>
    /// <returns>Resultado del cálculo de la prima SOAT</returns>
    [HttpGet("calculate-premium")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CalculatePremium(
        [FromQuery] CalculatePremiumRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales-params/calculate-premium invocado para Gestión: {PolicyYearId}, Uso: {UsageId}, TipoVehículo: {VehicleTypeId}, Depto: {DepartmentId}",
            request.TbPolicyYearId, request.TbUsageId, request.TbVehicleTypeId, request.TbDepartmentId);

        var result = await _salesParamsService.CalculatePremiumAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calcula la prima SOAT según gestión, uso, tipo de vehículo y departamento mediante cuerpo JSON (POST).
    /// (Invoca sales_params.psp_04_calculate_premium)
    /// </summary>
    /// <param name="request">Cuerpo con los parámetros de cálculo de prima</param>
    /// <param name="cancellationToken">Token de cancelación opcional</param>
    /// <returns>Resultado del cálculo de la prima SOAT</returns>
    [HttpPost("calculate-premium")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CalculatePremiumPost(
        [FromBody] CalculatePremiumRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint POST api/sales-params/calculate-premium invocado para Gestión: {PolicyYearId}, Uso: {UsageId}, TipoVehículo: {VehicleTypeId}, Depto: {DepartmentId}",
            request.TbPolicyYearId, request.TbUsageId, request.TbVehicleTypeId, request.TbDepartmentId);

        var result = await _salesParamsService.CalculatePremiumAsync(request, cancellationToken);
        return Ok(result);
    }
}
