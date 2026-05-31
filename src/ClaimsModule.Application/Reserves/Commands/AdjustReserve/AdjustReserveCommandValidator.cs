using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Validation;
using FluentValidation;

namespace ClaimsModule.Application.Reserves.Commands.AdjustReserve;

public sealed class AdjustReserveCommandValidator : AbstractValidator<AdjustReserveCommand>
{
    public AdjustReserveCommandValidator(IValidationQueries validationQueries, IReserveRepository reserves)
    {
        RuleFor(x => x.ReserveComponentId)
            .NotEmpty().WithMessage("ReserveComponentId is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");

        RuleFor(x => x.Amount)
            .NotEqual(0m)
            .WithMessage("Adjustment amount must not be zero.");

        RuleFor(x => x.ChangeReason)
            .NotEmpty().WithMessage("Change reason is required.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await validationQueries.ReserveComponentExistsByIdAsync(
                    cmd.ReserveComponentId, cmd.ClaimId, cmd.OrganisationId, ct))
            .WithName("ReserveComponentId")
            .WithMessage("Reserve component not found for this claim.");

        RuleFor(x => x.ReserveComponentId)
            .MustAsync(async (componentId, ct) =>
                !await validationQueries.HasPendingApprovalForComponentIdAsync(componentId, ct))
            .WithMessage(
                "This reserve component already has a PendingApproval transaction. " +
                "Retract it before submitting a new adjustment (BR-R-07).");

        ReserveAggregateValidationRules.ApplyAggregateWarningRule(
            this,
            cmd => cmd.ClaimId,
            cmd => cmd.Amount,
            reserves);
    }
}
