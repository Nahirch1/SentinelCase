using MediatR;

using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.CreateIncident;

public sealed record CreateIncidentCommand(
    string Title,
    string Description,
    IncidentSeverity Severity,
    DateTimeOffset DetectedAt) : IRequest<CreateIncidentResult>;