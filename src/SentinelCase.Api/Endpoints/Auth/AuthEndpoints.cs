using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using SentinelCase.Infrastructure.Identity;
using SentinelCase.Infrastructure.Identity.Tokens;
using SentinelCase.Infrastructure.Persistence;

namespace SentinelCase.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost(
            "/login",
            async (
                LoginRequest request,
                UserManager<ApplicationUser> userManager,
                JwtTokenService tokenService,
                IConfiguration configuration) =>
            {
                var email = request.Email.Trim();

                var user = await userManager.Users
                    .SingleOrDefaultAsync(
                        x => x.NormalizedEmail ==
                             email.ToUpperInvariant());

                if (user is null || !user.IsActive)
                {
                    return Results.Unauthorized();
                }

                var passwordValid =
                    await userManager.CheckPasswordAsync(
                        user,
                        request.Password);

                if (!passwordValid)
                {
                    return Results.Unauthorized();
                }

                var roles =
                    await userManager.GetRolesAsync(user);

                var tokens =
                    await tokenService.CreateAsync(user);

                var accessTokenMinutes =
                    int.TryParse(
                        configuration["Jwt:AccessTokenMinutes"],
                        out var minutes)
                        ? minutes
                        : 30;

                return Results.Ok(
                    new LoginResponse(
                        tokens.AccessToken,
                        tokens.RefreshToken,
                        DateTimeOffset.UtcNow.AddMinutes(
                            accessTokenMinutes),
                        user.Email ?? string.Empty,
                        user.DisplayName,
                        roles.ToArray()));
            })
            .AllowAnonymous();


        group.MapPost(
            "/refresh",
            async (
                RefreshTokenRequest request,
                JwtTokenService tokenService,
                IConfiguration configuration) =>
            {
                var tokens =
                    await tokenService.RefreshAsync(
                        request.RefreshToken);

                if (tokens is null)
                {
                    return Results.Unauthorized();
                }

                var accessTokenMinutes =
                    int.TryParse(
                        configuration["Jwt:AccessTokenMinutes"],
                        out var minutes)
                        ? minutes
                        : 30;

                return Results.Ok(
                    new
                    {
                        accessToken =
                            tokens.Value.AccessToken,
                        refreshToken =
                            tokens.Value.RefreshToken,
                        expiresAt =
                            DateTimeOffset.UtcNow
                                .AddMinutes(
                                    accessTokenMinutes)
                    });
            })
            .AllowAnonymous();

        group.MapPost(
            "/logout",
            async (
                LogoutRequest request,
                JwtTokenService tokenService) =>
            {
                var revoked =
                    await tokenService.RevokeAsync(
                        request.RefreshToken);

                return revoked
                    ? Results.NoContent()
                    : Results.Unauthorized();
            })
            .AllowAnonymous();

        return endpoints;
    }
}
