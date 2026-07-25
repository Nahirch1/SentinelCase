using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SentinelCase.IntegrationTests.Authentication;
using Microsoft.Extensions.Logging;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(
        options,
        logger,
        encoder)
{
    public const string AuthenticationScheme = "TestScheme";
    public const string UserHeaderName = "X-Test-User";
    public const string RoleHeaderName = "X-Test-Role";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeaderName, out var usernames))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var username = usernames.ToString();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("A test username is required."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, username),
            new(ClaimTypes.Name, username)
        };

        if (Request.Headers.TryGetValue(RoleHeaderName, out var roles))
        {
            foreach (var role in roles
                         .ToString()
                         .Split(',', StringSplitOptions.RemoveEmptyEntries |
                                     StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(
            claims,
            AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            AuthenticationScheme);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
