using System.Text.Json;

namespace PlataformaSoat.Application.Common.Interfaces;

/// <summary>
/// Servicio para la generación de comprobantes de venta SOAT en formato PDF.
/// </summary>
public interface ISoatReceiptPdfService
{
    /// <summary>
    /// Genera el archivo PDF del comprobante de venta SOAT a partir de la información del response.
    /// </summary>
    /// <param name="policyId">Identificador de la póliza SOAT</param>
    /// <param name="saleData">Datos de la póliza y factura obtenidos de la base de datos</param>
    /// <returns>Bytes del archivo PDF generado</returns>
    byte[] GenerateSoatReceiptPdf(long policyId, JsonElement saleData);
}
