using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;

namespace ClaimsModule.Application.Claims.Services;

public sealed class ClaimClosureEvaluator(IClaimRepository claims, IReserveRepository reserves) : IClaimClosureEvaluator
{
    public async Task<ClaimClosureConditionsResult> EvaluateAsync(
        Guid claimId,
        Guid organisationId,
        string? closureReason,
        CancellationToken cancellationToken = default)
    {
        var claim = await claims.GetByIdWithPartiesReadOnlyAsync(claimId, organisationId, cancellationToken);

        if (claim is null)
        {
            return new ClaimClosureConditionsResult
            {
                CanClose = false,
                Conditions =
                [
                    new ClaimClosureConditionDto
                    {
                        Code = "Claim",
                        Description = "Claim not found.",
                        Passed = false
                    }
                ]
            };
        }

        var conditions = new List<ClaimClosureConditionDto>();

        var hasPending = await reserves.HasPendingApprovalAsync(claimId, cancellationToken);
        conditions.Add(new ClaimClosureConditionDto
        {
            Code = "CC-01",
            Description = "No reserve components with PendingApproval remain.",
            Passed = !hasPending,
            Detail = hasPending ? "One or more reserves are awaiting approval." : null
        });

        var hasCritical = await claims.HasUnresolvedCriticalValidationIssuesAsync(claimId, cancellationToken);
        conditions.Add(new ClaimClosureConditionDto
        {
            Code = "CC-02",
            Description = "No unresolved critical validation issues.",
            Passed = !hasCritical,
            Detail = hasCritical ? "Critical validation issues must be resolved." : null
        });

        var hasClaimant = claim.Parties.Any(p => p.IsActive && p.PartyRole == PartyRole.Claimant);
        conditions.Add(new ClaimClosureConditionDto
        {
            Code = "CC-03",
            Description = "At least one active Claimant party exists.",
            Passed = hasClaimant,
            Detail = hasClaimant ? null : "Add a Claimant party before closing."
        });

        var hasOutstanding = await reserves.HasOutstandingReservesAsync(claimId, cancellationToken);
        var cc04Passed = !hasOutstanding || !string.IsNullOrWhiteSpace(closureReason);
        conditions.Add(new ClaimClosureConditionDto
        {
            Code = "CC-04",
            Description = "Closure justification provided when reserves remain open.",
            Passed = cc04Passed,
            Detail = hasOutstanding && string.IsNullOrWhiteSpace(closureReason)
                ? "Provide a closure justification (reason) when reserves have a balance."
                : null
        });

        return new ClaimClosureConditionsResult
        {
            CanClose = conditions.All(c => c.Passed),
            Conditions = conditions
        };
    }
}
