using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace SentinelCase.IntegrationTests;

public sealed class RateLimitingTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RateLimitingTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TooManyRequests_FromSameClient_ShouldReturn429()
    {
        using var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        HttpResponseMessage? lastResponse = null;

        for (var requestNumber = 1;
             requestNumber <= 101;
             requestNumber++)
        {
            lastResponse?.Dispose();

            lastResponse = await client.GetAsync(
                "/health");
        }

        Assert.NotNull(lastResponse);

        using (lastResponse)
        {
            Assert.Equal(
                HttpStatusCode.TooManyRequests,
                lastResponse.StatusCode);
        }
    }
}
