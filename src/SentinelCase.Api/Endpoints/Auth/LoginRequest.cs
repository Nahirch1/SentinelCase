namespace SentinelCase.Api.Endpoints.Auth;

public sealed record LoginRequest(
    string Email,
    string Password);
