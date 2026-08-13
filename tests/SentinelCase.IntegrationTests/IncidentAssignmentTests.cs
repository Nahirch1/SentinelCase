using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.AssignIncident;
using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentHistory;
using SentinelCase.Domain.Enums;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentAssignmentTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IncidentAssignmentTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AssignIncident_AsSocManager_ReturnsOkAndCreatesHistory()
    {
        using var analystClient = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        using var managerClient = CreateAuthenticatedClient(
            "manager@sentinelcase.test",
            "SocManager");

        var createRequest = new
        {
            title = $"Assignment incident {Guid.NewGuid():N}",
            description = "Incident created to verify analyst assignment.",
            severity = IncidentSeverity.High,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
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

        var assignmentRequest = new
        {
            analystIdentifier = "tier1.analyst@sentinelcase.test"
        };

        using var assignResponse =
            await managerClient.PatchAsJsonAsync(
                $"/api/incidents/{created.Id}/assignment",
                assignmentRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            assignResponse.StatusCode);

        var assigned =
            await assignResponse.Content
                .ReadFromJsonAsync<AssignIncidentResult>();

        Assert.NotNull(assigned);
        Assert.Equal(created.Id, assigned.Id);

        Assert.Equal(
            "tier1.analyst@sentinelcase.test",
            assigned.AssignedTo);

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

        var assignedEntry = Assert.Single(
            history,
            entry =>
                entry.EventType ==
                IncidentHistoryEventType.Assigned);

        Assert.Null(assignedEntry.PreviousValue);

        Assert.Equal(
            "tier1.analyst@sentinelcase.test",
            assignedEntry.NewValue);

        Assert.Equal(
            "manager@sentinelcase.test",
            assignedEntry.PerformedBy);
    }

    [Fact]
    public async Task AssignedIncident_GetById_ShouldExposeAssignment()
    {
        using var analystClient = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        using var managerClient = CreateAuthenticatedClient(
            "manager@sentinelcase.test",
            "SocManager");

        var createRequest = new
        {
            title = $"Readable assignment incident {Guid.NewGuid():N}",
            description =
                "Incident created to verify assignment fields in retrieval.",
            severity = IncidentSeverity.High,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
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

        using var assignResponse =
            await managerClient.PatchAsJsonAsync(
                $"/api/incidents/{created.Id}/assignment",
                new
                {
                    analystIdentifier =
                        "assigned.analyst@sentinelcase.test"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            assignResponse.StatusCode);

        var assigned =
            await assignResponse.Content
                .ReadFromJsonAsync<AssignIncidentResult>();

        Assert.NotNull(assigned);

        using var getResponse =
            await analystClient.GetAsync(
                $"/api/incidents/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var incident =
            await getResponse.Content
                .ReadFromJsonAsync<GetIncidentByIdResult>();

        Assert.NotNull(incident);

        Assert.Equal(
            "assigned.analyst@sentinelcase.test",
            incident.AssignedTo);

        Assert.Equal(
            assigned.AssignedAt,
            incident.AssignedAt);
    }

    [Fact]
    public async Task AssignIncident_AsAnalyst_ReturnsForbidden()
    {
        using var analystClient = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/incidents/{Guid.NewGuid()}/assignment")
        {
            Content = JsonContent.Create(new
            {
                analystIdentifier =
                    "another.analyst@sentinelcase.test"
            })
        };

        using var response =
            await analystClient.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task AssignIncident_WithUnknownIncident_ReturnsNotFound()
    {
        using var managerClient = CreateAuthenticatedClient(
            "manager@sentinelcase.test",
            "SocManager");

        using var response =
            await managerClient.PatchAsJsonAsync(
                $"/api/incidents/{Guid.NewGuid()}/assignment",
                new
                {
                    analystIdentifier =
                        "tier1.analyst@sentinelcase.test"
                });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task ReassignIncident_ShouldUpdateAssigneeAndHistory()
    {
        using var analystClient = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        using var managerClient = CreateAuthenticatedClient(
            "manager@sentinelcase.test",
            "SocManager");

        var createRequest = new
        {
            title = $"Reassignment incident {Guid.NewGuid():N}",
            description = "Incident created to verify reassignment.",
            severity = IncidentSeverity.Medium,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
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

        using var firstAssignment =
            await managerClient.PatchAsJsonAsync(
                $"/api/incidents/{created.Id}/assignment",
                new
                {
                    analystIdentifier =
                        "first.analyst@sentinelcase.test"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            firstAssignment.StatusCode);

        using var secondAssignment =
            await managerClient.PatchAsJsonAsync(
                $"/api/incidents/{created.Id}/assignment",
                new
                {
                    analystIdentifier =
                        "second.analyst@sentinelcase.test"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            secondAssignment.StatusCode);

        var result =
            await secondAssignment.Content
                .ReadFromJsonAsync<AssignIncidentResult>();

        Assert.NotNull(result);

        Assert.Equal(
            "second.analyst@sentinelcase.test",
            result.AssignedTo);

        using var historyResponse =
            await analystClient.GetAsync(
                $"/api/incidents/{created.Id}/history");

        var history =
            await historyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyCollection<GetIncidentHistoryItem>>();

        Assert.NotNull(history);

        var assignments = history
            .Where(entry =>
                entry.EventType ==
                IncidentHistoryEventType.Assigned)
            .ToArray();

        Assert.Equal(2, assignments.Length);

        Assert.Null(assignments[0].PreviousValue);

        Assert.Equal(
            "first.analyst@sentinelcase.test",
            assignments[0].NewValue);

        Assert.Equal(
            "first.analyst@sentinelcase.test",
            assignments[1].PreviousValue);

        Assert.Equal(
            "second.analyst@sentinelcase.test",
            assignments[1].NewValue);
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
