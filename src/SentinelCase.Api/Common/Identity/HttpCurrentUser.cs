using System.Security.Claims;

using SentinelCase.Application.Common.Interfaces;

namespace SentinelCase.Api.Common.Identity;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Identifier
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.Identity?.Name
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user?.FindFirstValue("sub")
                ?? "system";
        }
    }
}
