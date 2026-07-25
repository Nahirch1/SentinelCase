using MediatR;

using SentinelCase.Api.Common.Authorization;
using SentinelCase.Application.Common.Models;
using SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;
using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidents;
using SentinelCase.Domain.Enums;

namespace SentinelCase.Api.Endpoints;

public static class IncidentEndpoints
{
    public static IEndpointRouteBuilder MapIncidentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/incidents")
            .WithTags("Incidents");

        group.MapPost("/", CreateIncidentAsync)
            .WithName("CreateIncident")
            .RequireAuthorization(AppPolicies.CanCreateIncident)
            .Produces<CreateIncidentResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem();

        group.MapGet("/", GetIncidentsAsync)
            .WithName("GetIncidents")
            .RequireAuthorization()
            .Produces<PagedResult<GetIncidentsItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetIncidentByIdAsync)
            .WithName("GetIncidentById")
            .RequireAuthorization()
            .Produces<GetIncidentByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", ChangeIncidentStatusAsync)
            .WithName("ChangeIncidentStatus")
            .RequireAuthorization(AppPolicies.CanManageIncidentStatus)
            .Produces<ChangeIncidentStatusResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return endpoints;
    }

    private static async Task<IResult> CreateIncidentAsync(
        CreateIncidentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateIncidentCommand(
            request.Title,
            request.Description,
            request.Severity,
            request.DetectedAt);

        var result = await sender.Send(command, cancellationToken);

        return Results.Created(
            $"/api/incidents/{result.Id}",
            result);
    }

    private static async Task<IResult> GetIncidentsAsync(
        int pageNumber,
        int pageSize,
        IncidentStatus? status,
        IncidentSeverity? severity,
        string? searchTerm,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetIncidentsQuery(
            pageNumber,
            pageSize,
            status,
            severity,
            searchTerm);

        var result = await sender.Send(
            query,
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetIncidentByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetIncidentByIdQuery(id);

        var result = await sender.Send(
            query,
            cancellationToken);

        return result is null
            ? Results.NotFound()
            : Results.Ok(result);
    }

    private static async Task<IResult> ChangeIncidentStatusAsync(
        Guid id,
        ChangeIncidentStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ChangeIncidentStatusCommand(
            id,
            request.Status);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result is null
            ? Results.NotFound()
            : Results.Ok(result);
    }

    public sealed record CreateIncidentRequest(
        string Title,
        string Description,
        IncidentSeverity Severity,
        DateTimeOffset DetectedAt);

    public sealed record ChangeIncidentStatusRequest(
        IncidentStatus Status);
}
