using AutoMapper;
using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries.SearchPolicies;

public sealed class SearchPoliciesQueryHandler(IPolicyRepository policies, IMapper mapper)
    : IRequestHandler<SearchPoliciesQuery, PagedResult<PolicySummaryDto>>
{
    public async Task<PagedResult<PolicySummaryDto>> Handle(
        SearchPoliciesQuery request,
        CancellationToken cancellationToken)
    {
        var (policies1, totalCount) = await policies.SearchPagedAsync(
            request.OrganisationId,
            request.Search,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var items = policies1.Select(mapper.Map<PolicySummaryDto>).ToList();
        return PagedResult<PolicySummaryDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}
