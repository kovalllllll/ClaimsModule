using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using MediatR;

namespace ClaimsModule.Application.Claims.Queries.GetClaimClosureConditions;

public sealed class GetClaimClosureConditionsQueryHandler(
    IClaimRepository claims,
    IClaimClosureEvaluator evaluator)
    : IRequestHandler<GetClaimClosureConditionsQuery, ClaimClosureConditionsResult?>
{
    public async Task<ClaimClosureConditionsResult?> Handle(
        GetClaimClosureConditionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await claims.ExistsAsync(request.ClaimId, request.OrganisationId, cancellationToken))
            return null;

        return await evaluator.EvaluateAsync(
            request.ClaimId,
            request.OrganisationId,
            request.ClosureReason,
            cancellationToken);
    }
}
