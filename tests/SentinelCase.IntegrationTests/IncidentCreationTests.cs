using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;
using SentinelCase.Domain.Enums;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentCreationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IncidentCreationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateIncident_AsAnalyst_ReturnsCreated()
    {
        using var client = CreateAnalystClient();

        var detectedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

        var request = new
        {
            title = "Suspicious PowerShell execution",
            description =
                "Encoded PowerShell command detected on workstation FIN-023.",
            severity = IncidentSeverity.High,
            detectedAt
        };

        using var response = await client.PostAsJsonAsync(
            "/api/incidents",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.title, result.Title);
        Assert.Equal(IncidentSeverity.High, result.Severity);
        Assert.Equal(IncidentStatus.Open, result.Status);
        Assert.Equal(detectedAt, result.DetectedAt);

        Assert.NotNull(response.Headers.Location);

        Assert.Equal(
            $"/api/incidents/{result.Id}",
            response.Headers.Location.ToString());
    }

    [Fact]
    public async Task CreateThenGetIncident_AsAnalyst_ReturnsStoredIncident()
    {
        using var client = CreateAnalystClient();

        var detectedAt = DateTimeOffset.UtcNow.AddMinutes(-15);

        var request = new
        {
            title = $"Credential stuffing detected {Guid.NewGuid()}",
            description =
                "Repeated authentication attempts were detected from multiple addresses.",
            severity = IncidentSeverity.Critical,
            detectedAt
        };

        using var createResponse = await client.PostAsJsonAsync(
            "/api/incidents",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(created);

        using var getResponse = await client.GetAsync(
            $"/api/incidents/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var stored =
            await getResponse.Content
                .ReadFromJsonAsync<GetIncidentByIdResult>();

        Assert.NotNull(stored);
        Assert.Equal(created.Id, stored.Id);
        Assert.Equal(request.title, stored.Title);
        Assert.Equal(request.description, stored.Description);
        Assert.Equal(IncidentSeverity.Critical, stored.Severity);
        Assert.Equal(IncidentStatus.Open, stored.Status);
        Assert.Equal(detectedAt, stored.DetectedAt);
        Assert.Equal(created.CreatedAt, stored.CreatedAt);
    }

    private HttpClient CreateAnalystClient()
    {
        var client = _factory.CreateClient(
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

        return client;
    }
}
