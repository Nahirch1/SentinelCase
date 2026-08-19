namespace SentinelCase.Api.Endpoints.Auth;

public sealed record LogoutRequest(
    string RefreshToken);
