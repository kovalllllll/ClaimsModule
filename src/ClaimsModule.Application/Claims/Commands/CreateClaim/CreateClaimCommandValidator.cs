using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Domain.Enums;
using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.CreateClaim;

public sealed class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator(IValidationQueries validationQueries, ISystemClock clock)
    {
        RuleFor(x => x.LossDate)
            .NotEmpty()
                .WithMessage("Loss date is required.")
            .Must(d => d <= clock.UtcNow)
                .WithMessage("Loss date cannot be in the future.");

        RuleFor(x => x.LossDescription)
            .NotEmpty()
                .WithMessage("Loss description is required and must be at least 20 characters.")
            .MinimumLength(20)
                .WithMessage("Loss description is required and must be at least 20 characters.");

        RuleFor(x => x.CauseOfLossCode)
            .NotEmpty()
                .WithMessage("Cause of loss code is required.")
            .MustAsync(async (code, ct) =>
                await validationQueries.CauseOfLossCodeIsActiveAsync(code, ct))
                .WithMessage("Cause of loss code is not recognised or is inactive.");

        When(x => x.EstimatedLossAmount.HasValue, () =>
        {
            RuleFor(x => x.EstimatedLossAmount!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Estimated loss amount cannot be negative.");
        });

        When(x => x.Severity.HasValue, () =>
        {
            RuleFor(x => x.Severity!.Value)
                .IsInEnum()
                .WithMessage("Claim severity must be Catastrophic, Critical, Standard, or Minor.");
        });

        RuleFor(x => x.PolicyId)
            .Must(id => id.HasValue)
                .WithMessage(ClaimValidationMessages.NoPolicyLinkedWarning)
                .WithSeverity(Severity.Warning);

        When(x => x.PolicyId.HasValue, () =>
        {
            RuleFor(x => x.LossDate)
                .MustAsync(async (command, lossDate, ct) =>
                {
                    var policy = await validationQueries.GetPolicyByIdAsync(command.PolicyId!.Value, ct);
                    if (policy is null)
                        return true;

                    var lossDateOnly = DateOnly.FromDateTime(lossDate.UtcDateTime.Date);
                    return lossDateOnly >= policy.EffectiveDate
                        && lossDateOnly <= policy.ExpirationDate;
                })
                .WithMessage(ClaimValidationMessages.LossDateOutsidePolicyPeriod)
                .WithSeverity(Severity.Warning);
        });

        RuleFor(x => x.RiskObjects)
            .Must(ro => ro.Count > 0)
            .WithMessage("No risk objects linked to this claim.")
            .WithSeverity(Severity.Warning);

        When(x => x.InitialReserve is not null, () =>
        {
            RuleFor(x => x.InitialReserve!)
                .Must(r => ReserveAmountRules.IsValidTransactionAmount(r.ComponentType, r.Amount))
                .WithMessage(ReserveAmountRules.TransactionAmountMessage);

            RuleFor(x => x.InitialReserve!.ChangeReason)
                .NotEmpty()
                    .WithMessage("A change reason is required for the initial reserve.");

            RuleFor(x => x.InitialReserve!.Amount)
                .Must(amount => amount <= ReserveAggregateValidationRules.Threshold)
                .WithName("ReserveAmount")
                .WithMessage(ClaimValidationMessages.AggregateReserveWarning)
                .WithSeverity(Severity.Warning);
        });
    }
}
