using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
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

        var request = new
        {
            title = "Suspicious PowerShell execution",
            description =
                "Encoded PowerShell command detected on workstation FIN-023.",
            severity = 3,
            detectedAt = DateTimeOffset.UtcNow
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/incidents",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        Assert.NotNull(response.Headers.Location);

        Assert.Equal(
            $"/api/incidents/{result.Id}",
            response.Headers.Location.ToString());
    }
}
