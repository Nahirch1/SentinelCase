using MediatR;

namespace SentinelCase.Application.Features.Incidents.Commands.AddIncidentNote;

public sealed record AddIncidentNoteCommand(
    Guid IncidentId,
    string Content)
    : IRequest<AddIncidentNoteResult?>;
