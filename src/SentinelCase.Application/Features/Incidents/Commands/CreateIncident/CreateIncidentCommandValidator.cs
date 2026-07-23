using FluentValidation;

namespace SentinelCase.Application.Features.Incidents.Commands.CreateIncident;

public sealed class CreateIncidentCommandValidator
    : AbstractValidator<CreateIncidentCommand>
{
    public CreateIncidentCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(command => command.Severity)
            .IsInEnum();

        RuleFor(command => command.DetectedAt)
            .NotEmpty();
    }
}