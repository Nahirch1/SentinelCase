using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;
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

    [Fact]
    public async Task CreateIncident_WithInvalidRequest_ReturnsValidationProblem()
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
            title = "",
            description = "",
            severity = 99,
            detectedAt = DateTimeOffset.MinValue
        };

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/incidents",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(
            "Validation failed",
            problem.Title);

        Assert.Contains(
            "Title",
            problem.Errors.Keys);

        Assert.Contains(
            "Description",
            problem.Errors.Keys);

        Assert.Contains(
            "Severity",
            problem.Errors.Keys);

        Assert.Contains(
            "DetectedAt",
            problem.Errors.Keys);
    }


    [Fact]
    public async Task CreateIncident_WithDuplicatedTitle_ReturnsConflict()
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

        var title =
            $"Duplicated incident {Guid.NewGuid():N}";

        var firstRequest = new
        {
            title,
            description = "First incident description.",
            severity = 3,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        var secondRequest = new
        {
            title,
            description = "Second incident description.",
            severity = 2,
            detectedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
        };

        using var firstResponse = await client.PostAsJsonAsync(
            "/api/incidents",
            firstRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        // Act
        using var secondResponse = await client.PostAsJsonAsync(
            "/api/incidents",
            secondRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);

        var problem =
            await secondResponse.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(
            "Domain rule violation",
            problem.Title);

        Assert.Equal(
            "An incident with the same title already exists.",
            problem.Detail);
    }

}
