using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentHistory;
using SentinelCase.Domain.Enums;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentHistoryTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IncidentHistoryTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task IncidentHistory_ShouldReflectCreatedUpdatedAndStatusChanges()
    {
        using var analystClient = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        using var managerClient = CreateAuthenticatedClient(
            "manager@sentinelcase.test",
            "SocManager");

        var title = $"History incident {Guid.NewGuid():N}";

        var createRequest = new
        {
            title,
            description = "Initial incident description.",
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

        var updateRequest = new
        {
            title = $"{title} updated",
            description = "Updated incident description.",
            severity = IncidentSeverity.Critical
        };

        using var updateResponse =
            await analystClient.PutAsJsonAsync(
                $"/api/incidents/{created.Id}",
                updateRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        using var statusRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/incidents/{created.Id}/status")
        {
            Content = JsonContent.Create(new
            {
                status = IncidentStatus.UnderInvestigation
            })
        };

        using var statusResponse =
            await managerClient.SendAsync(statusRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            statusResponse.StatusCode);

        using var historyResponse =
            await analystClient.GetAsync(
                $"/api/incidents/{created.Id}/history");

        Assert.Equal(
            HttpStatusCode.OK,
            historyResponse.StatusCode);

        var history =
            await historyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyCollection<GetIncidentHistoryItem>>();

        Assert.NotNull(history);

        var entries = history.ToArray();

        Assert.Equal(4, entries.Length);

        Assert.Equal(
            IncidentHistoryEventType.Created,
            entries[0].EventType);

        Assert.Equal(
            IncidentHistoryEventType.DetailsUpdated,
            entries[1].EventType);

        Assert.Equal(
            IncidentHistoryEventType.SeverityChanged,
            entries[2].EventType);

        Assert.Equal(
            IncidentHistoryEventType.StatusChanged,
            entries[3].EventType);

        Assert.Equal(
            "analyst@sentinelcase.test",
            entries[0].PerformedBy);

        Assert.Equal(
            "analyst@sentinelcase.test",
            entries[1].PerformedBy);

        Assert.Equal(
            "analyst@sentinelcase.test",
            entries[2].PerformedBy);

        Assert.Equal(
            "manager@sentinelcase.test",
            entries[3].PerformedBy);

        Assert.Equal(
            IncidentStatus.Open.ToString(),
            entries[0].NewValue);

        Assert.Equal(
            IncidentSeverity.High.ToString(),
            entries[2].PreviousValue);

        Assert.Equal(
            IncidentSeverity.Critical.ToString(),
            entries[2].NewValue);

        Assert.Equal(
            IncidentStatus.Open.ToString(),
            entries[3].PreviousValue);

        Assert.Equal(
            IncidentStatus.UnderInvestigation.ToString(),
            entries[3].NewValue);
    }

    [Fact]
    public async Task GetIncidentHistory_WithUnknownIncident_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        using var response = await client.GetAsync(
            $"/api/incidents/{Guid.NewGuid()}/history");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
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
}
