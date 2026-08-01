using SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;
using SentinelCase.Domain.Entities;
using SentinelCase.Domain.Enums;
using SentinelCase.Domain.Exceptions;
using SentinelCase.UnitTests.TestDoubles;

namespace SentinelCase.UnitTests.Application.Incidents.Commands.UpdateIncident;

public sealed class UpdateIncidentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateIncident()
    {
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident();

        await repository.AddAsync(incident);

        var handler = new UpdateIncidentCommandHandler(repository);

        var command = new UpdateIncidentCommand(
            incident.Id,
            "Updated suspicious activity",
            "Updated incident description.",
            IncidentSeverity.Critical);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(incident.Id, result.Id);
        Assert.Equal(command.Title, result.Title);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(IncidentSeverity.Critical, result.Severity);
        Assert.Equal(IncidentStatus.Open, result.Status);

        Assert.Equal(command.Title, incident.Title);
        Assert.Equal(command.Description, incident.Description);
        Assert.Equal(IncidentSeverity.Critical, incident.Severity);
    }

    [Fact]
    public async Task Handle_WithUnknownIncident_ShouldReturnNull()
    {
        var repository = new FakeSecurityIncidentRepository();
        var handler = new UpdateIncidentCommandHandler(repository);

        var command = new UpdateIncidentCommand(
            Guid.NewGuid(),
            "Updated title",
            "Updated description.",
            IncidentSeverity.High);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_WithDuplicatedTitle_ShouldThrowDomainException()
    {
        var repository = new FakeSecurityIncidentRepository();

        var incident = CreateIncident();

        var existingIncident = SecurityIncident.Create(
            "Existing incident title",
            "Existing incident description.",
            IncidentSeverity.Medium,
            DateTimeOffset.UtcNow.AddMinutes(-20),
            DateTimeOffset.UtcNow.AddMinutes(-15));

        await repository.AddAsync(incident);
        await repository.AddAsync(existingIncident);

        var handler = new UpdateIncidentCommandHandler(repository);

        var command = new UpdateIncidentCommand(
            incident.Id,
            "Existing incident title",
            "Updated incident description.",
            IncidentSeverity.High);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal(
            "An incident with the same title already exists.",
            exception.Message);

        Assert.NotEqual(
            command.Title,
            incident.Title);
    }

    [Fact]
    public async Task Handle_WithSameTitleDifferentCase_ShouldUpdateIncident()
    {
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident();

        await repository.AddAsync(incident);

        var handler = new UpdateIncidentCommandHandler(repository);

        var command = new UpdateIncidentCommand(
            incident.Id,
            incident.Title.ToUpperInvariant(),
            "Updated description.",
            IncidentSeverity.Low);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(
            command.Title,
            result.Title);

        Assert.Equal(
            "Updated description.",
            result.Description);

        Assert.Equal(
            IncidentSeverity.Low,
            result.Severity);
    }

    [Fact]
    public async Task Handle_WithClosedIncident_ShouldThrowDomainException()
    {
        var repository = new FakeSecurityIncidentRepository();
        var incident = CreateIncident();

        incident.StartInvestigation();
        incident.Contain();
        incident.Resolve();
        incident.Close(DateTimeOffset.UtcNow);

        await repository.AddAsync(incident);

        var handler = new UpdateIncidentCommandHandler(repository);

        var command = new UpdateIncidentCommand(
            incident.Id,
            "Updated closed incident",
            "Closed incidents cannot be updated.",
            IncidentSeverity.Critical);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal(
            "A closed incident cannot be modified.",
            exception.Message);
    }

    private static SecurityIncident CreateIncident()
    {
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-10);

        return SecurityIncident.Create(
            "Suspicious endpoint activity",
            "Suspicious process execution detected.",
            IncidentSeverity.High,
            createdAt.AddMinutes(-5),
            createdAt);
    }
}
