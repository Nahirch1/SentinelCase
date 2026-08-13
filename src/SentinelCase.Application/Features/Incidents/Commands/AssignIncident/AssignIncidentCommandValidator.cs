using FluentValidation;

namespace SentinelCase.Application.Features.Incidents.Commands.AssignIncident;

public sealed class AssignIncidentCommandValidator
    : AbstractValidator<AssignIncidentCommand>
{
    public AssignIncidentCommandValidator()
    {
        RuleFor(command => command.IncidentId)
            .NotEmpty();

        RuleFor(command => command.AnalystIdentifier)
            .NotEmpty()
            .MaximumLength(200);
    }
}
