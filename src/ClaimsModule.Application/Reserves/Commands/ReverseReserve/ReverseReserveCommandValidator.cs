using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common.Validation;
using FluentValidation;

namespace ClaimsModule.Application.Reserves.Commands.ReverseReserve;

public sealed class ReverseReserveCommandValidator : AbstractValidator<ReverseReserveCommand>
{
    public ReverseReserveCommandValidator(IValidationQueries validationQueries, IReserveRepository reserves)
    {
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.OrganisationId).NotEmpty();
        RuleFor(x => x.Amount).NotEqual(0m).WithMessage("Reverse amount must not be zero.");
        RuleFor(x => x.ChangeReason).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await validationQueries.ReserveComponentExistsAsync(
                    cmd.ClaimId, cmd.OrganisationId, cmd.Component, ct))
            .WithName("Component")
            .WithMessage("Reserve component not found for this claim. Open a reserve first.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                !await validationQueries.HasPendingApprovalForComponentTypeAsync(
                    cmd.ClaimId, cmd.Component, ct))
            .WithMessage(
                "This reserve component already has a PendingApproval transaction. " +
                "Retract it before submitting a reverse (BR-R-07).");

        ReserveAggregateValidationRules.ApplyAggregateWarningRule(
            this,
            cmd => cmd.ClaimId,
            cmd => cmd.Amount,
            reserves);
    }
}
