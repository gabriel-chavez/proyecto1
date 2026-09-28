using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlataformaSoat.Application.Common.Interfaces;
using PlataformaSoat.Application.Configuration;
using PlataformaSoat.Application.Modulos.Auth.DTOs;

namespace PlataformaSoat.Infrastructure.Services;

/// <summary>
/// Implementación de ITokenService para generación, validación y gestión de ciclo de vida de tokens JWT.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtSettings> settings,
        IMemoryCache cache,
        ILogger<TokenService> logger)
    {
        _settings = settings.Value;
        _cache = cache;
        _logger = logger;
    }

    public LoginResponse GenerateTokens(UserInfoDto user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_settings.SecretKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        // Generar Refresh Token seguro
        var refreshToken = GenerateSecureRefreshToken();
        var refreshExpiry = TimeSpan.FromDays(_settings.RefreshTokenExpirationDays);

        _cache.Set(GetRefreshCacheKey(refreshToken), user, refreshExpiry);

        _logger.LogInformation("Tokens generados para el usuario {Username} con expiración de acceso a las {Expires}",
            user.Username, expires);

        return new LoginResponse
        {
            TokenType = "Bearer",
            AccessToken = accessToken,
            ExpiresIn = _settings.ExpirationMinutes * 60,
            RefreshToken = refreshToken,
            User = user
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
            ValidateLifetime = false // Permitir tokens expirados para el flujo de renovación
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al extraer ClaimsPrincipal desde token expirado");
            return null;
        }
    }

    public bool ValidateRefreshToken(string refreshToken, out UserInfoDto? user)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            user = null;
            return false;
        }

        return _cache.TryGetValue(GetRefreshCacheKey(refreshToken), out user);
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            _cache.Remove(GetRefreshCacheKey(refreshToken));
            _logger.LogInformation("Refresh Token revocado.");
        }
    }

    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static string GetRefreshCacheKey(string refreshToken) => $"refresh_token_{refreshToken}";
}
