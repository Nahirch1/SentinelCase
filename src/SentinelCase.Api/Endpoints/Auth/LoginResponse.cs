namespace SentinelCase.Api.Endpoints.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles);
