using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace SentinelCase.IntegrationTests;

public sealed class AuthFlowTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthFlowTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "unknown@test.local",
                password = "InvalidPassword_2026!"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var response = await LoginAsync();

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body =
            await response.Content
                .ReadFromJsonAsync<LoginResult>();

        Assert.NotNull(body);
        Assert.False(
            string.IsNullOrWhiteSpace(
                body.AccessToken));
        Assert.False(
            string.IsNullOrWhiteSpace(
                body.RefreshToken));
    }

    [Fact]
    public async Task RefreshToken_CannotBeReused()
    {
        var login = await GetLoginResultAsync();

        var firstRefresh =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    refreshToken =
                        login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.OK,
            firstRefresh.StatusCode);

        var reused =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    refreshToken =
                        login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            reused.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var login = await GetLoginResultAsync();

        var logout =
            await _client.PostAsJsonAsync(
                "/api/auth/logout",
                new
                {
                    refreshToken =
                        login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.NoContent,
            logout.StatusCode);

        var refresh =
            await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new
                {
                    refreshToken =
                        login.RefreshToken
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            refresh.StatusCode);
    }

    private Task<HttpResponseMessage> LoginAsync()
    {
        return _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "manager@test.local",
                password = "TestPassword_2026!"
            });
    }

    private async Task<LoginResult> GetLoginResultAsync()
    {
        var response = await LoginAsync();

        response.EnsureSuccessStatusCode();

        return (await response.Content
            .ReadFromJsonAsync<LoginResult>())!;
    }

    private sealed record LoginResult(
        string AccessToken,
        string RefreshToken);
}
