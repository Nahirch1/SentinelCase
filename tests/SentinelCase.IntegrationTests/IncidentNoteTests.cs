using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using SentinelCase.Application.Features.Incidents.Commands.AddIncidentNote;
using SentinelCase.Application.Features.Incidents.Commands.CreateIncident;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentHistory;
using SentinelCase.Application.Features.Incidents.Queries.GetIncidentNotes;
using SentinelCase.Domain.Enums;
using SentinelCase.IntegrationTests.Authentication;

namespace SentinelCase.IntegrationTests;

public sealed class IncidentNoteTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IncidentNoteTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddAndGetIncidentNote_ShouldPersistAndCreateHistory()
    {
        using var client = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        var createResponse = await client.PostAsJsonAsync(
            "/api/incidents",
            new
            {
                title = $"Note incident {Guid.NewGuid():N}",
                description = "Incident created to verify notes.",
                severity = IncidentSeverity.High,
                detectedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var incident =
            await createResponse.Content
                .ReadFromJsonAsync<CreateIncidentResult>();

        Assert.NotNull(incident);

        var noteContent =
            "Firewall and authentication logs were reviewed.";

        var noteResponse = await client.PostAsJsonAsync(
            $"/api/incidents/{incident.Id}/notes",
            new
            {
                content = noteContent
            });

        Assert.Equal(
            HttpStatusCode.Created,
            noteResponse.StatusCode);

        var createdNote =
            await noteResponse.Content
                .ReadFromJsonAsync<AddIncidentNoteResult>();

        Assert.NotNull(createdNote);
        Assert.Equal(incident.Id, createdNote.IncidentId);
        Assert.Equal(noteContent, createdNote.Content);
        Assert.Equal(
            "analyst@sentinelcase.test",
            createdNote.CreatedBy);

        var getNotesResponse = await client.GetAsync(
            $"/api/incidents/{incident.Id}/notes");

        Assert.Equal(
            HttpStatusCode.OK,
            getNotesResponse.StatusCode);

        var notes =
            await getNotesResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyCollection<GetIncidentNoteItem>>();

        Assert.NotNull(notes);

        var storedNote = Assert.Single(notes);

        Assert.Equal(createdNote.Id, storedNote.Id);
        Assert.Equal(noteContent, storedNote.Content);
        Assert.Equal(
            "analyst@sentinelcase.test",
            storedNote.CreatedBy);

        var historyResponse = await client.GetAsync(
            $"/api/incidents/{incident.Id}/history");

        Assert.Equal(
            HttpStatusCode.OK,
            historyResponse.StatusCode);

        var history =
            await historyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyCollection<GetIncidentHistoryItem>>();

        Assert.NotNull(history);

        var noteHistory = Assert.Single(
            history,
            entry =>
                entry.EventType ==
                IncidentHistoryEventType.NoteAdded);

        Assert.Equal(
            createdNote.Id.ToString(),
            noteHistory.NewValue);

        Assert.Equal(
            "analyst@sentinelcase.test",
            noteHistory.PerformedBy);
    }

    [Fact]
    public async Task AddNote_WithUnknownIncident_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        var response = await client.PostAsJsonAsync(
            $"/api/incidents/{Guid.NewGuid()}/notes",
            new
            {
                content = "This note should not be created."
            });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetNotes_WithUnknownIncident_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient(
            "analyst@sentinelcase.test",
            "Analyst");

        var response = await client.GetAsync(
            $"/api/incidents/{Guid.NewGuid()}/notes");

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
