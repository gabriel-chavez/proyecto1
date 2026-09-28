using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Responses;
using PlataformaSoat.Application.Modulos.Sales.Services;

namespace PlataformaSoat.Api.Controllers;

[ApiController]
[Route("api/sales")]
[Produces("application/json")]
public class SalesController : BaseApiController
{
    private readonly SalesService _salesService;
    private readonly ILogger<SalesController> _logger;

    public SalesController(
        SalesService salesService,
        ILogger<SalesController> logger)
    {
        _salesService = salesService;
        _logger = logger;
    }

    /// <summary>
    /// Registra una venta individual SOAT, generando factura, detalle y póliza.
    /// (Invoca sales.psp_01_register_soat_sale)
    /// </summary>
    /// <param name="request">Datos para la venta de la póliza SOAT individual</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la emisión de la póliza</returns>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterSoatSale(
        [FromBody] RegisterSoatSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint POST api/sales invocado para placa/chasis {PlateOrChassis}", request.PlateOrChassis);
        var result = await _salesService.RegisterSoatSaleAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra una venta masiva SOAT, generando la cabecera de factura, detalles y las pólizas SOAT.
    /// (Invoca sales.psp_01_register_bulk_soat_sale)
    /// </summary>
    /// <param name="request">Datos para la venta masiva de pólizas SOAT</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la emisión masiva</returns>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BaseResponse<BulkSoatSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterBulkSoatSale(
        [FromBody] RegisterBulkSoatSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint POST api/sales/bulk invocado para {BusinessName}", request.BusinessName);
        var result = await _salesService.RegisterBulkSoatSaleAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra una solicitud de anulación para una póliza SOAT.
    /// (Invoca sales.psp_02_request_soat_cancellation)
    /// </summary>
    /// <param name="request">Datos de la solicitud de anulación</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del registro de la solicitud</returns>
    [HttpPost("cancellations")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestSoatCancellation(
        [FromBody] RequestSoatCancellationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint POST api/sales/cancellations invocado para póliza {PolicyId}", request.TbSoatPolicyId);
        var result = await _salesService.RequestSoatCancellationAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirma la anulación de una póliza SOAT previamente solicitada.
    /// (Invoca sales.psp_03_confirm_soat_cancellation)
    /// </summary>
    /// <param name="request">Identificador de la solicitud y usuario confirmador</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la confirmación de la anulación</returns>
    [HttpPost("cancellations/confirm")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmSoatCancellation(
        [FromBody] ConfirmSoatCancellationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint POST api/sales/cancellations/confirm invocado para solicitud {CancellationId}", request.TbSoatCancellationId);
        var result = await _salesService.ConfirmSoatCancellationAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulta el listado de ventas SOAT aplicando filtros opcionales.
    /// (Invoca sales.psp_04_list_soat_sales)
    /// </summary>
    /// <param name="filter">Filtros de búsqueda: broker, comercializador, canal y rango de fechas</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Listado de ventas SOAT</returns>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListSoatSales(
        [FromQuery] ListSoatSalesFilter filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales invocado con rango {StartDate} a {EndDate}", filter.StartDate, filter.EndDate);
        var result = await _salesService.ListSoatSalesAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Consulta el detalle de una venta SOAT por identificador de póliza.
    /// (Invoca sales.psp_05_get_soat_sale)
    /// </summary>
    /// <param name="id">Identificador único de la póliza SOAT (tb_soat_policy_id)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Detalle de la póliza y factura emitida</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(BaseResponse<JsonElement>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSoatSale(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales/{Id} invocado", id);
        var result = await _salesService.GetSoatSaleAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Genera y obtiene el comprobante oficial de la venta SOAT en formato PDF dentro del objeto de respuesta estándar.
    /// (Invoca sales.psp_05_get_soat_sale y renderiza el PDF en Base64 manteniendo la estructura de respuesta de 5 campos)
    /// </summary>
    /// <param name="id">Identificador único de la póliza SOAT (tb_soat_policy_id)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Comprobante SOAT con documento PDF en Base64 en el objeto de salida</returns>
    [HttpGet("{id:long}/comprobante")]
    [ProducesResponseType(typeof(BaseResponse<SoatReceiptDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSoatSaleReceipt(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales/{Id}/comprobante invocado", id);
        var result = await _salesService.GetSoatSaleReceiptPdfAsync(id, cancellationToken);
        if (!result.Success || result.Response == null)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Descarga directamente el archivo binario PDF del comprobante SOAT.
    /// </summary>
    /// <param name="id">Identificador único de la póliza SOAT (tb_soat_policy_id)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Archivo binario PDF para descarga directa</returns>
    [HttpGet("{id:long}/comprobante/download")]
    [Produces("application/pdf", "application/json")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound, "application/json")]
    public async Task<IActionResult> DownloadSoatSaleReceiptPdf(
        [FromRoute] long id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Endpoint GET api/sales/{Id}/comprobante/download invocado", id);
        var result = await _salesService.GetSoatSaleReceiptPdfAsync(id, cancellationToken);
        if (!result.Success || result.Response == null || string.IsNullOrWhiteSpace(result.Response.FileContentBase64))
        {
            return NotFound(result);
        }

        var pdfBytes = Convert.FromBase64String(result.Response.FileContentBase64);
        return File(pdfBytes, result.Response.MimeType, result.Response.FileName);
    }
}
