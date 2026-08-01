using FluentValidation;

namespace SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;

public sealed class UpdateIncidentCommandValidator
    : AbstractValidator<UpdateIncidentCommand>
{
    public UpdateIncidentCommandValidator()
    {
        RuleFor(command => command.IncidentId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(command => command.Severity)
            .IsInEnum();
    }
}
