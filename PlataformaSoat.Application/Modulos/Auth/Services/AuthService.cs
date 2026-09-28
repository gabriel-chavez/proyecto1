using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Modulos.Auth.DTOs;
using PlataformaSoat.Application.Modulos.Auth.Interfaces;

namespace PlataformaSoat.Application.Modulos.Auth.Services;

/// <summary>
/// Servicio de aplicación para la autenticación, emisión y renovación de sesiones con tokens JWT.
/// </summary>
public class AuthService
{
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ITokenService tokenService,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        ILogger<AuthService> logger)
    {
        _tokenService = tokenService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Inicia sesión validando credenciales en la base de datos y generando Access Token (JWT) y Refresh Token.
    /// </summary>
    public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Intento de inicio de sesión para el usuario {Username}", request.Username);

        var userRecord = await _userRepository.GetByUsernameAsync(request.Username);

        if (userRecord == null || userRecord.Value.PasswordHash != request.Password)
        {
            _logger.LogWarning("Autenticación fallida para el usuario {Username}: credenciales incorrectas o usuario inactivo", request.Username);
            return BaseResponse<LoginResponse>.Fail(
                errorMessage: "Credenciales inválidas. Verifique su usuario y contraseña.",
                responseMessage: "Error de autenticación",
                statusMessage: "INVALID_CREDENTIALS");
        }

        var tokenResponse = _tokenService.GenerateTokens(userRecord.Value.UserInfo);
        _logger.LogInformation("Inicio de sesión exitoso para {Username}. Token generado.", request.Username);

        return BaseResponse<LoginResponse>.Ok(
            response: tokenResponse,
            responseMessage: "Sesión iniciada correctamente",
            statusMessage: "SUCCESS");
    }

    /// <summary>
    /// Renueva el token de acceso JWT utilizando un Refresh Token válido.
    /// </summary>
    public Task<BaseResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        _logger.LogInformation("Solicitud de renovación de token");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Task.FromResult(BaseResponse<LoginResponse>.Fail(
                errorMessage: "El token de actualización ('refresh_token') es requerido.",
                responseMessage: "Parámetro inválido",
                statusMessage: "INVALID_ARGUMENT"));
        }

        if (!_tokenService.ValidateRefreshToken(request.RefreshToken, out var user) || user == null)
        {
            _logger.LogWarning("Intento de renovación con Refresh Token inválido o expirado");
            return Task.FromResult(BaseResponse<LoginResponse>.Fail(
                errorMessage: "El token de actualización ha expirado o no es válido.",
                responseMessage: "No autorizado",
                statusMessage: "INVALID_REFRESH_TOKEN"));
        }

        // Revocar el token anterior y generar uno nuevo (rotación de refresh tokens)
        _tokenService.RevokeRefreshToken(request.RefreshToken);
        var newTokens = _tokenService.GenerateTokens(user);

        _logger.LogInformation("Token renovado exitosamente para el usuario {Username}", user.Username);

        return Task.FromResult(BaseResponse<LoginResponse>.Ok(
            response: newTokens,
            responseMessage: "Token renovado correctamente",
            statusMessage: "SUCCESS"));
    }

    /// <summary>
    /// Cierra la sesión revocando el Refresh Token.
    /// </summary>
    public Task<BaseResponse<bool>> RevokeTokenAsync(RevokeTokenRequest request)
    {
        _logger.LogInformation("Solicitud de revocación de token");

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _tokenService.RevokeRefreshToken(request.RefreshToken);
        }

        return Task.FromResult(BaseResponse<bool>.Ok(
            response: true,
            responseMessage: "Sesión revocada exitosamente",
            statusMessage: "SUCCESS"));
    }

    /// <summary>
    /// Obtiene la información del usuario autenticado en la petición actual.
    /// </summary>
    public Task<BaseResponse<UserInfoDto>> GetCurrentUserAsync()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Task.FromResult(BaseResponse<UserInfoDto>.Fail(
                errorMessage: "No hay una sesión activa de usuario.",
                responseMessage: "No autenticado",
                statusMessage: "UNAUTHORIZED"));
        }

        var userInfo = new UserInfoDto
        {
            UserId = _currentUserService.UserId ?? string.Empty,
            Username = _currentUserService.Username ?? string.Empty,
            Email = _currentUserService.Email ?? string.Empty,
            Roles = new List<string>(_currentUserService.Roles)
        };

        return Task.FromResult(BaseResponse<UserInfoDto>.Ok(
            response: userInfo,
            responseMessage: "Datos del usuario obtenidos correctamente",
            statusMessage: "SUCCESS"));
    }
}
