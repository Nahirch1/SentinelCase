using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using SentinelCase.Domain.Exceptions;

namespace SentinelCase.Api.Common.ExceptionHandling;

public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
        {
            return false;
        }

        logger.LogWarning(
            domainException,
            "A domain rule prevented the request from completing.");

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = domainException,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Domain rule violation",
                    Detail = domainException.Message,
                    Type = "https://httpstatuses.com/409",
                    Instance = httpContext.Request.Path
                }
            });
    }
}
