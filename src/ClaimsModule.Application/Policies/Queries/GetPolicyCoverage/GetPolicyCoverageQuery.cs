using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries.GetPolicyCoverage;

public sealed record PolicyCoverageDto(Guid PolicyId, IReadOnlyList<string> CoverageTypes);

public sealed record GetPolicyCoverageQuery(Guid PolicyId, Guid OrganisationId) : IRequest<PolicyCoverageDto?>;
