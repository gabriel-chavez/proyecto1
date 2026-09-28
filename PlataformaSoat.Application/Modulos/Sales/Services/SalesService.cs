using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Requests;
using PlataformaSoat.Application.Modulos.Sales.DTOs.Responses;
using PlataformaSoat.Application.Modulos.Sales.Interfaces;

namespace PlataformaSoat.Application.Modulos.Sales.Services;

/// <summary>
/// Servicio de aplicación para la gestión de ventas y anulaciones SOAT.
/// </summary>
public class SalesService
{
    private readonly ISalesRepository _repository;
    private readonly ISoatReceiptPdfService _pdfService;
    private readonly ILogger<SalesService> _logger;

    public SalesService(
        ISalesRepository repository,
        ISoatReceiptPdfService pdfService,
        ILogger<SalesService> logger)
    {
        _repository = repository;
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Registra una venta individual SOAT ejecutando sales.psp_01_register_soat_sale.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> RegisterSoatSaleAsync(
        RegisterSoatSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando registro de venta individual SOAT para {PlateOrChassis} (Factura: {InvoiceNumber})",
            request.PlateOrChassis, request.InvoiceNumber);

        return await _repository.RegisterSoatSaleAsync(request, cancellationToken);
    }

    /// <summary>
    /// Registra una venta masiva SOAT ejecutando sales.psp_01_register_bulk_soat_sale.
    /// </summary>
    public async Task<BaseResponse<BulkSoatSaleResponse>> RegisterBulkSoatSaleAsync(
        RegisterBulkSoatSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando registro de venta masiva SOAT para {BusinessName} (NIT: {TaxId})",
            request.BusinessName, request.TaxOrIdentityNumber);

        return await _repository.RegisterBulkSoatSaleAsync(request, cancellationToken);
    }

    /// <summary>
    /// Registra una solicitud de anulación de póliza SOAT ejecutando sales.psp_02_request_soat_cancellation.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> RequestSoatCancellationAsync(
        RequestSoatCancellationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando solicitud de anulación para póliza {PolicyId} por {RequestedBy}",
            request.TbSoatPolicyId, request.RequestedBy);

        return await _repository.RequestSoatCancellationAsync(request, cancellationToken);
    }

    /// <summary>
    /// Confirma la anulación de una póliza SOAT ejecutando sales.psp_03_confirm_soat_cancellation.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ConfirmSoatCancellationAsync(
        ConfirmSoatCancellationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando confirmación de anulación de póliza {CancellationId} por {ConfirmedBy}",
            request.TbSoatCancellationId, request.ConfirmedBy);

        return await _repository.ConfirmSoatCancellationAsync(request, cancellationToken);
    }

    /// <summary>
    /// Consulta las ventas SOAT filtradas ejecutando sales.psp_04_list_soat_sales.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> ListSoatSalesAsync(
        ListSoatSalesFilter filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando ventas SOAT con rango {StartDate} a {EndDate}",
            filter.StartDate, filter.EndDate);

        return await _repository.ListSoatSalesAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Consulta el detalle de una venta SOAT por identificador de póliza ejecutando sales.psp_05_get_soat_sale.
    /// </summary>
    public async Task<BaseResponse<JsonElement>> GetSoatSaleAsync(
        long tbSoatPolicyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando detalle de venta SOAT para póliza {PolicyId}", tbSoatPolicyId);

        return await _repository.GetSoatSaleAsync(tbSoatPolicyId, cancellationToken);
    }

    /// <summary>
    /// Genera el comprobante de venta SOAT en formato PDF por identificador de póliza.
    /// Consulta sales.psp_05_get_soat_sale y retorna el documento en Base64 dentro del objeto de respuesta estándar.
    /// </summary>
    public async Task<BaseResponse<SoatReceiptDocumentResponse>> GetSoatSaleReceiptPdfAsync(
        long tbSoatPolicyId,
        CancellationToken cancellationToken = default)
    {
        if (tbSoatPolicyId <= 0)
        {
            _logger.LogInformation("Generando comprobante de muestra (demo) para previsualización");
            var sampleJson = """
            {
                "cufd": null,
                "sale_date": "2026-09-26T06:54:38.83229",
                "branch_name": "Sucursal La Paz",
                "paid_premium": 200,
                "policyholder": "JUAN PÉREZ",
                "business_name": "JUAN PÉREZ",
                "policy_number": 1,
                "invoice_number": null,
                "plate_type_name": "Placa normal",
                "plate_or_chassis": "1234ABC",
                "coverage_end_date": "2027-12-31",
                "coverage_start_date": "2027-01-01",
                "tax_or_identity_number": "12345678"
            }
            """;
            using var doc = JsonDocument.Parse(sampleJson);
            var samplePdfBytes = _pdfService.GenerateSoatReceiptPdf(1, doc.RootElement);

            var demoResponse = new SoatReceiptDocumentResponse
            {
                PolicyNumber = 1,
                FileName = "comprobante_soat_1.pdf",
                MimeType = "application/pdf",
                FileContentBase64 = Convert.ToBase64String(samplePdfBytes),
                FileSizeBytes = samplePdfBytes.Length
            };

            return BaseResponse<SoatReceiptDocumentResponse>.Ok(demoResponse, "Comprobante de muestra generado exitosamente");
        }

        _logger.LogInformation("Obteniendo datos de venta para generar comprobante PDF de póliza {PolicyId}", tbSoatPolicyId);

        var saleResult = await _repository.GetSoatSaleAsync(tbSoatPolicyId, cancellationToken);
        if (!saleResult.Success || saleResult.Response.ValueKind == JsonValueKind.Undefined || saleResult.Response.ValueKind == JsonValueKind.Null)
        {
            _logger.LogWarning("No se pudo obtener la póliza {PolicyId} para generar el comprobante. Mensaje: {Message}",
                tbSoatPolicyId, saleResult.ResponseMessage);

            return BaseResponse<SoatReceiptDocumentResponse>.Fail(
                saleResult.ErrorMessage,
                saleResult.ResponseMessage ?? "No se encontró la póliza para generar el comprobante.",
                saleResult.StatusMessage ?? "NOT_FOUND");
        }

        try
        {
            var pdfBytes = _pdfService.GenerateSoatReceiptPdf(tbSoatPolicyId, saleResult.Response);

            long policyNum = tbSoatPolicyId;
            if (saleResult.Response.TryGetProperty("policy_number", out var pn) && pn.TryGetInt64(out var parsedPn))
            {
                policyNum = parsedPn;
            }

            var receiptResponse = new SoatReceiptDocumentResponse
            {
                PolicyNumber = policyNum,
                FileName = $"comprobante_soat_{policyNum}.pdf",
                MimeType = "application/pdf",
                FileContentBase64 = Convert.ToBase64String(pdfBytes),
                FileSizeBytes = pdfBytes.Length
            };

            return BaseResponse<SoatReceiptDocumentResponse>.Ok(receiptResponse, "Comprobante SOAT generado correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar el comprobante PDF para la póliza {PolicyId}", tbSoatPolicyId);
            return BaseResponse<SoatReceiptDocumentResponse>.Fail(
                ex.Message,
                "Error interno al renderizar el comprobante PDF",
                "PDF_GENERATION_ERROR");
        }
    }
}
