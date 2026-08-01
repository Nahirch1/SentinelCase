using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Commands.UpdateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;
using SentinelCase.Domain.Enums;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentUpdateTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IncidentUpdateTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateIncident_AsAnalyst_PersistsChanges()
    {
        using var client = CreateAnalystClient();

        var createRequest = new
        {
            title = $"Initial incident {Guid.NewGuid():N}",
            description = "Initial incident description.",
            severity = IncidentSeverity.Low,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
        };

        using var createResponse = await client.PostAsJsonAsync(
            "/api/incidents",
            createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(created);

        var updateRequest = new
        {
            title = $"Updated incident {Guid.NewGuid():N}",
            description = "Updated incident description.",
            severity = IncidentSeverity.Critical
        };

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/incidents/{created.Id}",
            updateRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updated =
            await updateResponse.Content
                .ReadFromJsonAsync<UpdateIncidentResult>();

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(updateRequest.title, updated.Title);
        Assert.Equal(updateRequest.description, updated.Description);
        Assert.Equal(IncidentSeverity.Critical, updated.Severity);
        Assert.Equal(IncidentStatus.Open, updated.Status);
        Assert.Equal(created.DetectedAt, updated.DetectedAt);
        Assert.Equal(created.CreatedAt, updated.CreatedAt);
        Assert.Null(updated.ClosedAt);

        using var getResponse = await client.GetAsync(
            $"/api/incidents/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var stored =
            await getResponse.Content
                .ReadFromJsonAsync<GetIncidentByIdResult>();

        Assert.NotNull(stored);
        Assert.Equal(updateRequest.title, stored.Title);
        Assert.Equal(updateRequest.description, stored.Description);
        Assert.Equal(IncidentSeverity.Critical, stored.Severity);
    }

    [Fact]
    public async Task UpdateIncident_WithUnknownId_ReturnsNotFound()
    {
        using var client = CreateAnalystClient();

        var request = new
        {
            title = $"Unknown incident update {Guid.NewGuid():N}",
            description = "Updated description for an unknown incident.",
            severity = IncidentSeverity.Medium
        };

        using var response = await client.PutAsJsonAsync(
            $"/api/incidents/{Guid.NewGuid()}",
            request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateIncident_WithDuplicatedTitle_ReturnsConflict()
    {
        using var client = CreateAnalystClient();

        var firstTitle =
            $"First update incident {Guid.NewGuid():N}";

        var secondTitle =
            $"Second update incident {Guid.NewGuid():N}";

        var firstCreateRequest = new
        {
            title = firstTitle,
            description = "Description for the first incident.",
            severity = IncidentSeverity.Low,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        };

        var secondCreateRequest = new
        {
            title = secondTitle,
            description = "Description for the second incident.",
            severity = IncidentSeverity.Medium,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
        };

        using var firstCreateResponse =
            await client.PostAsJsonAsync(
                "/api/incidents",
                firstCreateRequest);

        using var secondCreateResponse =
            await client.PostAsJsonAsync(
                "/api/incidents",
                secondCreateRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            firstCreateResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Created,
            secondCreateResponse.StatusCode);

        var secondIncident =
            await secondCreateResponse.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(secondIncident);

        var updateRequest = new
        {
            title = firstTitle,
            description =
                "Attempting to reuse the title of another incident.",
            severity = IncidentSeverity.High
        };

        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/incidents/{secondIncident.Id}",
            updateRequest);

        Assert.Equal(
            HttpStatusCode.Conflict,
            updateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateIncident_WhenClosed_ReturnsConflict()
    {
        using var analystClient = CreateAnalystClient();
        using var managerClient = CreateAuthenticatedClient(
            "manager@sentinelcase.test",
            "SocManager");

        var createRequest = new
        {
            title = $"Closed incident {Guid.NewGuid():N}",
            description =
                "Incident created to verify closed records cannot be edited.",
            severity = IncidentSeverity.High,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        };

        using var createResponse =
            await analystClient.PostAsJsonAsync(
                "/api/incidents",
                createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(created);

        await ChangeStatusAsync(
            managerClient,
            created.Id,
            IncidentStatus.UnderInvestigation);

        await ChangeStatusAsync(
            managerClient,
            created.Id,
            IncidentStatus.Contained);

        await ChangeStatusAsync(
            managerClient,
            created.Id,
            IncidentStatus.Resolved);

        await ChangeStatusAsync(
            managerClient,
            created.Id,
            IncidentStatus.Closed);

        var updateRequest = new
        {
            title = $"Modified closed incident {Guid.NewGuid():N}",
            description =
                "Attempting to modify an incident after closure.",
            severity = IncidentSeverity.Critical
        };

        using var updateResponse =
            await analystClient.PutAsJsonAsync(
                $"/api/incidents/{created.Id}",
                updateRequest);

        Assert.Equal(
            HttpStatusCode.Conflict,
            updateResponse.StatusCode);
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

    private HttpClient CreateAuthenticatedClient(
        string username,
        string role)
    {
        var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.UserHeaderName,
            username);

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.RoleHeaderName,
            role);

        return client;
    }

    private static async Task ChangeStatusAsync(
        HttpClient client,
        Guid incidentId,
        IncidentStatus status)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/incidents/{incidentId}/status")
        {
            Content = JsonContent.Create(new
            {
                status
            })
        };

        using var response = await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

}
