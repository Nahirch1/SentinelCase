using SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;
using SentinelCase.Domain.Enums;

namespace SentinelCase.UnitTests.Application.Incidents.Commands.UpdateIncident;

public sealed class UpdateIncidentCommandValidatorTests
{
    private readonly UpdateIncidentCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new UpdateIncidentCommand(
            Guid.NewGuid(),
            "Updated incident",
            "Updated incident description.",
            IncidentSeverity.High);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithInvalidFields_ShouldFail()
    {
        var command = new UpdateIncidentCommand(
            Guid.Empty,
            "",
            "",
            (IncidentSeverity)999);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateIncidentCommand.IncidentId));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateIncidentCommand.Title));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateIncidentCommand.Description));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                nameof(UpdateIncidentCommand.Severity));
    }
}
