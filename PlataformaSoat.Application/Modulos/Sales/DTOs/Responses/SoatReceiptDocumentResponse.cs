using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaSoat.Application.Modulos.Sales.DTOs.Responses;

/// <summary>
/// Contenedor del comprobante de venta SOAT en formato PDF codificado en Base64.
/// </summary>
public class SoatReceiptDocumentResponse
{
    [JsonPropertyName("policy_number")]
    public long PolicyNumber { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = "application/pdf";

    [JsonPropertyName("file_content_base64")]
    public string FileContentBase64 { get; set; } = string.Empty;

    [JsonPropertyName("file_size_bytes")]
    public long FileSizeBytes { get; set; }
}
