using FluentValidation;

namespace SentinelCase.Application.Features.Incidents.Commands.AddIncidentNote;

public sealed class AddIncidentNoteCommandValidator
    : AbstractValidator<AddIncidentNoteCommand>
{
    public AddIncidentNoteCommandValidator()
    {
        RuleFor(command => command.IncidentId)
            .NotEmpty();

        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
