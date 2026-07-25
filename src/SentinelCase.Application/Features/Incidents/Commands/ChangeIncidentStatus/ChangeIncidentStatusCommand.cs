using MediatR;

using SentinelCase.Domain.Enums;

namespace SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public sealed record ChangeIncidentStatusCommand(
    Guid IncidentId,
    IncidentStatus Status)
    : IRequest<ChangeIncidentStatusResult?>;
