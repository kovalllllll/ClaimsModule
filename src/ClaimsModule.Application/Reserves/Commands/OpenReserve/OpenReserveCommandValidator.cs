using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.ValueObjects;
using FluentValidation;

namespace ClaimsModule.Application.Reserves.Commands.OpenReserve;

public sealed class OpenReserveCommandValidator : AbstractValidator<OpenReserveCommand>
{
    public OpenReserveCommandValidator(IValidationQueries validationQueries, IReserveRepository reserves)
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");

        RuleFor(x => x.Amount)
            .Must((cmd, amount) => ReserveAmountRules.IsValidTransactionAmount(cmd.ComponentType, amount))
            .WithMessage(ReserveAmountRules.TransactionAmountMessage);

        RuleFor(x => x.ChangeReason)
            .NotEmpty().WithMessage("Change reason is required.");

        RuleFor(x => x.ClaimId)
            .MustAsync(validationQueries.ClaimHasLinkedPolicyAsync)
            .WithMessage("A policy must be linked to the claim before opening a reserve.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                !await validationQueries.HasPendingApprovalForComponentTypeAsync(
                    cmd.ClaimId, cmd.ComponentType, ct))
            .WithName("ComponentType")
            .WithMessage(
                "This reserve component already has a PendingApproval transaction. " +
                "Retract it before submitting a new one (BR-R-07).");

        ReserveAggregateValidationRules.ApplyAggregateWarningRule(
            this,
            cmd => cmd.ClaimId,
            cmd => cmd.Amount,
            reserves);
    }
}
