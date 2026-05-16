using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using CoreApp.Dto;
using CoreApp.Enums;
using CoreApp.Services;
using Infrastructure.EntityFramework.Context;
using Infrastructure.EntityFramework.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class AuthService(
    UserManager<AppUser> userManager,
    ParkingDbContext context,
    JwtSettings jwtSettings) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email)
                   ?? throw new Exception("Nieprawidłowy email lub hasło.");

        if (!await userManager.CheckPasswordAsync(user, dto.Password))
        {
            await userManager.AccessFailedAsync(user);
            throw new Exception("Nieprawidłowy email lub hasło.");
        }

        if (user.Status != SystemUserStatus.Active)
            throw new Exception("Konto jest nieaktywne.");

        if (await userManager.IsLockedOutAsync(user))
            throw new Exception("Konto jest zablokowane.");

        await userManager.ResetAccessFailedCountAsync(user);
        user.RecordLogin(DateTime.UtcNow);
        await userManager.UpdateAsync(user);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new Exception("Nieprawidłowy token.");

        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new Exception("Użytkownik nie istnieje.");

        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && t.UserId == userId)
            ?? throw new Exception("Nieprawidłowy refresh token.");

        if (!refreshToken.IsActive)
            throw new Exception("Refresh token wygasł lub został odwołany.");

        var newResponse = await GenerateAuthResponseAsync(user);
        refreshToken.Revoke(newResponse.RefreshToken);
        await context.SaveChangesAsync();
        return newResponse;
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken)
            ?? throw new Exception("Token nie istnieje.");

        if (!token.IsActive)
            throw new Exception("Token jest już nieaktywny.");

        token.Revoke();
        await context.SaveChangesAsync();
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Department = user.Department,
                Status = user.Status,
                Roles = roles
            }
        };
    }

    private string GenerateAccessToken(AppUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("department", user.Department),
            new("status", user.Status.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var credentials = new SigningCredentials(
            jwtSettings.GetSymmetricKey(),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var activeTokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();
        foreach (var t in activeTokens) t.Revoke();

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenDays)
        };

        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();
        return refreshToken;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = jwtSettings.GetSymmetricKey()
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(accessToken, parameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
            throw new Exception("Nieprawidłowy token.");

        return principal;
    }
}