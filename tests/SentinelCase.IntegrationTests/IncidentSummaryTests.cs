using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentSummaryTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IncidentSummaryTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task GetSummary_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response =
            await _client.GetAsync("/api/incidents/summary");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}
