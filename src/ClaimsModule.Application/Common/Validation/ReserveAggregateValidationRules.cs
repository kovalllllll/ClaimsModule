using ClaimsModule.Application.Abstractions.Persistence;
using FluentValidation;

namespace ClaimsModule.Application.Common.Validation;

public static class ReserveAggregateValidationRules
{
    public const decimal Threshold = 10_000_000m;

    public static void ApplyAggregateWarningRule<T>(
        AbstractValidator<T> validator,
        Func<T, Guid> claimIdSelector,
        Func<T, decimal> pendingAmountSelector,
        IReserveRepository reserves)
    {
        validator.RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var projected = await GetProjectedClaimTotalAfterApprovalAsync(
                    reserves,
                    claimIdSelector(cmd),
                    pendingAmountSelector(cmd),
                    ct);
                return projected <= Threshold;
            })
            .WithName("ReserveAmount")
            .WithMessage(ClaimValidationMessages.AggregateReserveWarning)
            .WithSeverity(Severity.Warning);
    }

    public static async Task<decimal> GetProjectedClaimTotalAfterApprovalAsync(
        IReserveRepository reserves,
        Guid claimId,
        decimal pendingApprovalAmount,
        CancellationToken ct)
    {
        var components = await reserves.GetComponentsByClaimIdAsync(claimId, ct);
        var currentTotal = components.Sum(rc => rc.CurrentAmount.Amount);
        return currentTotal + pendingApprovalAmount;
    }
}
