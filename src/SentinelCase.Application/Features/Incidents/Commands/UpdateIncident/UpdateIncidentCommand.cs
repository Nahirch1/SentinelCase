using MediatR;

using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;

public sealed record UpdateIncidentCommand(
    Guid IncidentId,
    string Title,
    string Description,
    IncidentSeverity Severity)
    : IRequest<UpdateIncidentResult?>;
