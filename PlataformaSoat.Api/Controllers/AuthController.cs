using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlataformaSoat.Application.Common.DTOs;
using PlataformaSoat.Application.Modulos.Auth.DTOs;
using PlataformaSoat.Application.Modulos.Auth.Services;

namespace PlataformaSoat.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : BaseApiController
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Inicia sesión y genera un Access Token (JWT Bearer) y un Refresh Token.
    /// </summary>
    /// <param name="request">Credenciales del usuario (usuario y contraseña)</param>
    /// <returns>Tokens de acceso y datos del usuario</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Solicitud de login para el usuario {Username}", request.Username);
        var result = await _authService.LoginAsync(request);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Renueva el Access Token utilizando un Refresh Token válido sin requerir reingreso de credenciales.
    /// </summary>
    /// <param name="request">Tokens para renovación</param>
    /// <returns>Nuevo Access Token y nuevo Refresh Token</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Solicitud de renovación de token");
        var result = await _authService.RefreshTokenAsync(request);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Revoca un Refresh Token activo (cierre de sesión / logout).
    /// </summary>
    /// <param name="request">Refresh Token a revocar</param>
    /// <returns>Confirmación de revocación</returns>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
    {
        _logger.LogInformation("Solicitud de revocación de token");
        var result = await _authService.RevokeTokenAsync(request);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene la información del usuario autenticado en la sesión actual a partir del token JWT.
    /// </summary>
    /// <returns>Perfil, identificadores y roles del usuario</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(BaseResponse<UserInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _authService.GetCurrentUserAsync();
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
