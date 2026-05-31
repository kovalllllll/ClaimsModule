using System.Text.Json;
using ClaimsModule.Application.Abstractions.Persistence;
using MediatR;

namespace ClaimsModule.Application.Policies.Queries.GetPolicyCoverage;

public sealed class GetPolicyCoverageQueryHandler(IPolicyRepository policies)
    : IRequestHandler<GetPolicyCoverageQuery, PolicyCoverageDto?>
{
    public async Task<PolicyCoverageDto?> Handle(
        GetPolicyCoverageQuery request,
        CancellationToken cancellationToken)
    {
        var policy = await policies.GetByIdAsync(
            request.PolicyId, request.OrganisationId, cancellationToken);

        if (policy is null) return null;

        var coverageTypes = JsonSerializer.Deserialize<List<string>>(policy.CoverageTypes) ?? [];
        return new PolicyCoverageDto(policy.Id, coverageTypes);
    }
}
