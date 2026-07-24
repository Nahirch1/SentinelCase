using FluentValidation;

namespace SentinelCase.Application.Features.Incidents.Queries.GetIncidents;

public sealed class GetIncidentsQueryValidator
    : AbstractValidator<GetIncidentsQuery>
{
    public GetIncidentsQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
