using System.Security.Claims;
using PlataformaSoat.Application.Modulos.Auth.DTOs;

namespace PlataformaSoat.Application.Common.Interfaces;

/// <summary>
/// Contrato para la generación y validación de tokens JWT y Refresh Tokens.
/// </summary>
public interface ITokenService
{
    LoginResponse GenerateTokens(UserInfoDto user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateRefreshToken(string refreshToken, out UserInfoDto? user);
    void RevokeRefreshToken(string refreshToken);
}
