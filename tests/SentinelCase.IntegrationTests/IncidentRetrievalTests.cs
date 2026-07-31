using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;
using SentinelCase.Domain.Enums;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentRetrievalTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IncidentRetrievalTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateThenGetIncident_ReturnsPersistedIncident()
    {
        // Arrange
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.UserHeaderName,
            "analyst@sentinelcase.test");

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.RoleHeaderName,
            "Analyst");

        var detectedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

        var title =
            $"Suspicious PowerShell execution {Guid.NewGuid():N}";

        var description =
            "Encoded PowerShell command detected on workstation FIN-023.";

        var createRequest = new
        {
            title,
            description,
            severity = IncidentSeverity.High,
            detectedAt
        };

        // Act: create
        using var createResponse = await client.PostAsJsonAsync(
            "/api/incidents",
            createRequest);

        // Assert: create
        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var createdIncident =
            await createResponse.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(createdIncident);
        Assert.NotEqual(Guid.Empty, createdIncident.Id);

        // Act: retrieve
        using var getResponse = await client.GetAsync(
            $"/api/incidents/{createdIncident.Id}");

        // Assert: retrieve
        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var retrievedIncident =
            await getResponse.Content
                .ReadFromJsonAsync<GetIncidentByIdResult>();

        Assert.NotNull(retrievedIncident);

        Assert.Equal(
            createdIncident.Id,
            retrievedIncident.Id);

        Assert.Equal(
            title,
            retrievedIncident.Title);

        Assert.Equal(
            description,
            retrievedIncident.Description);

        Assert.Equal(
            IncidentSeverity.High,
            retrievedIncident.Severity);

        Assert.Equal(
            IncidentStatus.Open,
            retrievedIncident.Status);

        Assert.Equal(
            detectedAt,
            retrievedIncident.DetectedAt);

        Assert.Equal(
            createdIncident.CreatedAt,
            retrievedIncident.CreatedAt);
    }
}
