using SentinelCase.Application.Features.Incidents.Queries.GetIncidents;

namespace SentinelCase.UnitTests.Application.Incidents.Queries.GetIncidents;

public sealed class GetIncidentsQueryValidatorTests
{
    private readonly GetIncidentsQueryValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidPagination_ShouldSucceed()
    {
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: 20);

        var result = await _validator.ValidateAsync(query);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_WithInvalidPageNumber_ShouldFail(
        int pageNumber)
    {
        var query = new GetIncidentsQuery(
            PageNumber: pageNumber,
            PageSize: 20);

        var result = await _validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetIncidentsQuery.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validate_WithInvalidPageSize_ShouldFail(
        int pageSize)
    {
        var query = new GetIncidentsQuery(
            PageNumber: 1,
            PageSize: pageSize);

        var result = await _validator.ValidateAsync(query);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetIncidentsQuery.PageSize));
    }
}
