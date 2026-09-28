using System;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PlataformaSoat.Infrastructure.Services;

/// <summary>
/// Implementación genérica del servicio de generación de comprobantes PDF para pólizas SOAT utilizando QuestPDF.
/// </summary>
public class SoatReceiptPdfService : ISoatReceiptPdfService
{
    private readonly ILogger<SoatReceiptPdfService> _logger;

    public SoatReceiptPdfService(ILogger<SoatReceiptPdfService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateSoatReceiptPdf(long policyId, JsonElement saleData)
    {
        _logger.LogInformation("Generando comprobante PDF genérico para póliza SOAT {PolicyId}", policyId);

        // Extracción de datos del response con nombres exactos de campos
        var policyNumber = GetString(saleData, "policy_number", policyId > 0 ? policyId.ToString() : "1");
        var policyholder = GetString(saleData, "policyholder", "N/D");
        var businessName = GetString(saleData, "business_name", policyholder);
        var taxOrId = GetString(saleData, "tax_or_identity_number", "N/D");
        var plateOrChassis = GetString(saleData, "plate_or_chassis", "N/D");
        var plateTypeName = GetString(saleData, "plate_type_name", "Placa normal");
        var branchName = GetString(saleData, "branch_name", "Sucursal Principal");
        var rawSaleDate = GetString(saleData, "sale_date", null);
        var rawStartDate = GetString(saleData, "coverage_start_date", null);
        var rawEndDate = GetString(saleData, "coverage_end_date", null);
        var invoiceNumber = GetString(saleData, "invoice_number", null);
        var cufd = GetString(saleData, "cufd", null);
        var rawPremium = GetString(saleData, "paid_premium", "0");

        // Formato amigable de fechas e importes
        var saleDateFormatted = FormatDateTime(rawSaleDate);
        var coverageStartFormatted = FormatDate(rawStartDate);
        var coverageEndFormatted = FormatDate(rawEndDate);
        var premiumFormatted = FormatCurrency(rawPremium);
        var invoiceDisplay = string.IsNullOrWhiteSpace(invoiceNumber) ? "En trámite / No aplica" : invoiceNumber;
        var cufdDisplay = string.IsNullOrWhiteSpace(cufd) ? "No aplica" : cufd;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                // ------------------ ENCABEZADO ------------------
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("SEGURO OBLIGATORIO DE ACCIDENTES DE TRÁNSITO").FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().Text("COMPROBANTE DE EMISIÓN DE PÓLIZA SOAT").FontSize(11).SemiBold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text("Certificado Digital de Cobertura").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(210).Border(1.5f).BorderColor(Colors.Blue.Darken2).Padding(8).Column(c =>
                        {
                            c.Item().Text($"PÓLIZA N°: {policyNumber}").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                            c.Item().Text($"FACTURA N°: {invoiceDisplay}").FontSize(9).SemiBold();
                            c.Item().Text($"FECHA VENTA: {saleDateFormatted}").FontSize(8);
                        });
                    });

                    col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken3);
                });

                // ------------------ CONTENIDO ------------------
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // 1. Datos del Titular / Contratante
                    col.Item().PaddingTop(4).Text("1. DATOS CONTRATANTE").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Text("Contratante:").SemiBold();
                        table.Cell().Text(policyholder).Bold();

                        table.Cell().Text("Razón Social:").SemiBold();
                        table.Cell().Text(businessName);

                        table.Cell().Text("NIT / C.I.:").SemiBold();
                        table.Cell().Text(taxOrId);

                        table.Cell().Text("Sucursal:").SemiBold();
                        table.Cell().Text(branchName);
                    });

                    // 2. Datos del Vehículo y Cobertura
                    col.Item().PaddingTop(12).Text("2. DATOS DEL VEHÍCULO Y VIGENCIA DE COBERTURA").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Text("Placa / Chasis:").SemiBold();
                        table.Cell().Text(plateOrChassis).Bold().FontSize(11).FontColor(Colors.Blue.Darken3);

                        table.Cell().Text("Tipo de Placa:").SemiBold();
                        table.Cell().Text(plateTypeName);

                        table.Cell().Text("Inicio Vigencia:").SemiBold();
                        table.Cell().Text(coverageStartFormatted).SemiBold();

                        table.Cell().Text("Fin Vigencia:").SemiBold();
                        table.Cell().Text(coverageEndFormatted).SemiBold();
                    });

                    // 3. Detalle de Prima y Facturación
                    col.Item().PaddingTop(12).Text("3. DETALLE ECONÓMICO").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(4).Text("Concepto de Cobertura").Bold();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(4).AlignRight().Text("Importe (BS)").Bold();

                        table.Cell().PaddingVertical(5).Text($"Seguro Obligatorio de Accidentes de Tránsito (SOAT) - Período {coverageStartFormatted} al {coverageEndFormatted}");
                        table.Cell().PaddingVertical(5).AlignRight().Text($"{premiumFormatted} Bs.");

                        table.Cell().BorderTop(1.5f).BorderColor(Colors.Blue.Darken2).PaddingTop(6).Text("TOTAL PAGADO:").Bold().FontSize(10);
                        table.Cell().BorderTop(1.5f).BorderColor(Colors.Blue.Darken2).PaddingTop(6).AlignRight().Text($"{premiumFormatted} BS").Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                    });

                   
                });

                // ------------------ PIE DE PÁGINA ------------------
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text(x =>
                        {
                            x.Span("Plataforma SOAT").SemiBold().FontSize(8);
                            x.Span($" | Fecha y Hora de Impresión: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Página ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" de ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static string GetString(JsonElement element, string propName, string? defaultValue)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty(propName, out var prop))
            {
                return prop.ValueKind switch
                {
                    JsonValueKind.String => prop.GetString() ?? defaultValue ?? string.Empty,
                    JsonValueKind.Number => prop.ToString(),
                    JsonValueKind.True => "Sí",
                    JsonValueKind.False => "No",
                    JsonValueKind.Null => defaultValue ?? string.Empty,
                    _ => prop.ToString()
                };
            }

            // Búsqueda en propiedades anidadas si viniera envuelto
            foreach (var innerObjName in new[] { "response", "soat", "policy", "sale" })
            {
                if (element.TryGetProperty(innerObjName, out var inner) && inner.ValueKind == JsonValueKind.Object)
                {
                    if (inner.TryGetProperty(propName, out var innerProp))
                    {
                        return innerProp.ValueKind switch
                        {
                            JsonValueKind.String => innerProp.GetString() ?? defaultValue ?? string.Empty,
                            JsonValueKind.Number => innerProp.ToString(),
                            JsonValueKind.True => "Sí",
                            JsonValueKind.False => "No",
                            JsonValueKind.Null => defaultValue ?? string.Empty,
                            _ => innerProp.ToString()
                        };
                    }
                }
            }
        }

        return defaultValue ?? string.Empty;
    }

    private static string FormatDateTime(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
            DateTime.TryParse(dateStr, out dt))
        {
            return dt.ToString("dd/MM/yyyy HH:mm:ss");
        }
        return dateStr;
    }

    private static string FormatDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return "N/D";
        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
            DateTime.TryParse(dateStr, out dt))
        {
            return dt.ToString("dd/MM/yyyy");
        }
        return dateStr;
    }

    private static string FormatCurrency(string? amountStr)
    {
        if (string.IsNullOrWhiteSpace(amountStr)) return "0.00";
        if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ||
            decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.CurrentCulture, out val))
        {
            return val.ToString("N2", CultureInfo.InvariantCulture);
        }
        return amountStr;
    }
}
