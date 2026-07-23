using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Domain.Enums;

namespace SentinelCase.UnitTests.Application.Incidents.Commands.CreateIncident;

public sealed class CreateIncidentCommandValidatorTests
{
    private readonly CreateIncidentCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldSucceed()
    {
        var command = new CreateIncidentCommand(
            "Unauthorized access attempt",
            "Several failed login attempts were detected.",
            IncidentSeverity.Medium,
            DateTimeOffset.UtcNow);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_WithEmptyTitle_ShouldFail()
    {
        var command = new CreateIncidentCommand(
            string.Empty,
            "A valid description.",
            IncidentSeverity.Low,
            DateTimeOffset.UtcNow);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_WithInvalidSeverity_ShouldFail()
    {
        var command = new CreateIncidentCommand(
            "Invalid severity",
            "The severity value is outside the enum.",
            (IncidentSeverity)999,
            DateTimeOffset.UtcNow);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(command.Severity));
    }
}