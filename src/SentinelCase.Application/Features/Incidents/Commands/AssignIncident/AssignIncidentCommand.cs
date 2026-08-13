using MediatR;

namespace SentinelCase.Application.Features.Incidents.Commands.AssignIncident;

public sealed record AssignIncidentCommand(
    Guid IncidentId,
    string AnalystIdentifier)
    : IRequest<AssignIncidentResult?>;
