using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.ChangeIncidentStatus;
using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentById;
using SentinelCase.Domain.Enums;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentStatusFlowTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IncidentStatusFlowTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangeStatusThroughFullWorkflow_PersistsFinalState()
    {
        using var analystClient = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        using var managerClient = CreateAuthenticatedClient(
            "manager@sentinelcase.test",
            "SocManager");

        var title =
            $"Suspicious lateral movement {Guid.NewGuid():N}";

        var createRequest = new
        {
            title,
            description =
                "Lateral movement activity detected between internal hosts.",
            severity = IncidentSeverity.Critical,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-15)
        };

        using var createResponse = await analystClient.PostAsJsonAsync(
            "/api/incidents",
            createRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var createdIncident =
            await createResponse.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(createdIncident);

        await AssertStatusChangeAsync(
            managerClient,
            createdIncident.Id,
            IncidentStatus.UnderInvestigation);

        await AssertStatusChangeAsync(
            managerClient,
            createdIncident.Id,
            IncidentStatus.Contained);

        await AssertStatusChangeAsync(
            managerClient,
            createdIncident.Id,
            IncidentStatus.Resolved);

        var closeResult = await AssertStatusChangeAsync(
            managerClient,
            createdIncident.Id,
            IncidentStatus.Closed);

        Assert.NotNull(closeResult.ClosedAt);

        using var getResponse = await analystClient.GetAsync(
            $"/api/incidents/{createdIncident.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var retrievedIncident =
            await getResponse.Content
                .ReadFromJsonAsync<GetIncidentByIdResult>();

        Assert.NotNull(retrievedIncident);
        Assert.Equal(
            IncidentStatus.Closed,
            retrievedIncident.Status);
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

    private static async Task<ChangeIncidentStatusResult>
        AssertStatusChangeAsync(
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

        var result =
            await response.Content
                .ReadFromJsonAsync<ChangeIncidentStatusResult>();

        Assert.NotNull(result);
        Assert.Equal(status, result.Status);

        return result;
    }
}
