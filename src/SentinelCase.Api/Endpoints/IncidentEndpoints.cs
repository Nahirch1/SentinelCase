using MediatR;

using SentinelCase.Application.Common.Models;
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
            .Produces<CreateIncidentResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/", GetIncidentsAsync)
            .WithName("GetIncidents")
            .Produces<PagedResult<GetIncidentsItem>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetIncidentByIdAsync)
            .WithName("GetIncidentById")
            .Produces<GetIncidentByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

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
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetIncidentsQuery(
            pageNumber,
            pageSize);

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

    public sealed record CreateIncidentRequest(
        string Title,
        string Description,
        IncidentSeverity Severity,
        DateTimeOffset DetectedAt);
}
