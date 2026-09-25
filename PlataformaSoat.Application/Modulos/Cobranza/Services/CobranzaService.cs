using System;
using System.Threading;
using System.Threading.Tasks;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Configuration;
using PlataformaSoat.Application.Modulos.Cobranza.DTOs.Requests;
using PlataformaSoat.Application.Modulos.Cobranza.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PlataformaSoat.Application.Modulos.Cobranza.Services;

public class CobranzaService
{
    private readonly ICobranzaRepository _repository;
    private readonly ILogger<CobranzaService> _logger;
    private readonly ApiSettings _apiSettings;

    public CobranzaService(
        ICobranzaRepository repository,
        IOptions<ApiSettings> apiSettings,
        ILogger<CobranzaService> logger)
    {
        _repository = repository;
        _logger = logger;
        _apiSettings = apiSettings.Value;
    }

    public async Task<ApiResponse<object>> RegisterSoatPolicyAsync(
        RegisterSoatPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando registro de póliza SOAT - Cliente: {Cliente}, Vehículo: {Vehiculo}",
                request.ClientId, request.VehicleId);

            var resultado = await _repository.RegisterSoatPolicyAsync(
                request,
                _apiSettings.CodigoSistema,
                cancellationToken);

            if (resultado.Exito)
            {
                _logger.LogInformation("Registro de póliza SOAT exitoso.");
                return ApiResponse<object>.Success(
                    resultado.Resultado,
                    resultado.Mensaje,
                    200);
            }
            else
            {
                _logger.LogWarning("Registro de póliza SOAT fallido: {Mensaje}", resultado.Mensaje);
                return ApiResponse<object>.Failure(
                    resultado.Mensaje,
                    resultado.CodigoRetorno);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al registrar póliza SOAT");
            return ApiResponse<object>.Failure($"Error interno: {ex.Message}", 500);
        }
    }
    public async Task<ApiResponse<object>> RevertirCobroAsync(
        RevertirCobroRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando reversión de cobro - Usuario: {Usuario}, Detalle: {Detalle}",
                request.UsuarioAut, request.TPlanPagoDetalleFk);

            var resultado = await _repository.RevertirCobroAsync(
                request,
                _apiSettings.CodigoSistema,
                cancellationToken);

            if (resultado.Exito)
            {
                _logger.LogInformation("Reversión de cobro exitosa - Detalle: {Detalle}",
                    request.TPlanPagoDetalleFk);
                return ApiResponse<object>.Success(
                    resultado.Resultado,
                    resultado.Mensaje,
                    resultado.CodigoRetorno);
            }
            else
            {
                _logger.LogWarning("Reversión de cobro fallida - Detalle: {Detalle}, Motivo: {Motivo}",
                    request.TPlanPagoDetalleFk, resultado.Mensaje);
                return ApiResponse<object>.Failure(
                    resultado.Mensaje,
                    resultado.CodigoRetorno);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en el servicio de reversión de cobro");
            return ApiResponse<object>.Failure($"Error interno: {ex.Message}", 500);
        }
    }

}
