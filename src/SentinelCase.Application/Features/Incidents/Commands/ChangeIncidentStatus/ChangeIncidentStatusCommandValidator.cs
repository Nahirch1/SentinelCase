using FluentValidation;

namespace SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public sealed class ChangeIncidentStatusCommandValidator
    : AbstractValidator<ChangeIncidentStatusCommand>
{
    public ChangeIncidentStatusCommandValidator()
    {
        RuleFor(command => command.IncidentId)
            .NotEmpty();

        RuleFor(command => command.Status)
            .IsInEnum();
    }
}
