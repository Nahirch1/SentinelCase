using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using SentinelCase.Infrastructure.Persistence;

namespace SentinelCase.Infrastructure.Identity.Tokens;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public JwtTokenService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        _configuration = configuration;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<(string AccessToken, string RefreshToken)> CreateAsync(
        ApplicationUser user)
    {
        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        var signingKey = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is missing.");

        var accessTokenMinutes =
            int.TryParse(
                _configuration["Jwt:AccessTokenMinutes"],
                out var minutes)
                ? minutes
                : 30;

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email ?? user.UserName ?? user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("display_name", user.DisplayName)
        };

        claims.AddRange(
            roles.Select(
                role => new Claim(
                    ClaimTypes.Role,
                    role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        var refreshToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var refreshTokenDays =
            int.TryParse(
                _configuration["Jwt:RefreshTokenDays"],
                out var days)
                ? days
                : 7;

        var refreshTokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(refreshToken)));

        _dbContext.RefreshTokens.Add(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenDays)
            });

        await _dbContext.SaveChangesAsync();

        return (accessToken, refreshToken);
    }
    public async Task<(string AccessToken, string RefreshToken)?> RefreshAsync(
        string refreshToken)
    {
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(refreshToken)));

        var storedToken = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash);

        if (storedToken is null ||
            storedToken.RevokedAt is not null ||
            storedToken.ExpiresAt <= DateTimeOffset.UtcNow ||
            !storedToken.User.IsActive)
        {
            return null;
        }

        var newTokens =
            await CreateAsync(storedToken.User);

        var newTokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    newTokens.RefreshToken)));

        storedToken.RevokedAt =
            DateTimeOffset.UtcNow;

        storedToken.ReplacedByTokenHash =
            newTokenHash;

        await _dbContext.SaveChangesAsync();

        return newTokens;
    }

    public async Task<bool> RevokeAsync(
        string refreshToken)
    {
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(refreshToken)));

        var storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash);

        if (storedToken is null ||
            storedToken.RevokedAt is not null)
        {
            return false;
        }

        storedToken.RevokedAt =
            DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

}
