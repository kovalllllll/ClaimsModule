using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries.SearchPolicies;

public sealed record SearchPoliciesQuery(
    Guid OrganisationId,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<PolicySummaryDto>>;
