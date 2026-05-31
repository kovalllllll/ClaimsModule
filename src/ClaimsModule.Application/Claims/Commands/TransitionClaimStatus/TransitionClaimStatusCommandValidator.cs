using ClaimsModule.Domain.Enums;
using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;

/// <summary>
/// Structural and static validation for TransitionClaimStatusCommand.
/// Rules that require database access (state-machine validity, role authority,
/// closure conditions, claimant presence) are enforced in the handler.
/// </summary>
public sealed class TransitionClaimStatusCommandValidator
    : AbstractValidator<TransitionClaimStatusCommand>
{
    public TransitionClaimStatusCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty()
            .WithMessage("OrganisationId is required.");

        // BR-ST-01 (static part): TargetStatus must be a recognised enum value.
        RuleFor(x => x.TargetStatus)
            .IsInEnum()
            .WithMessage("TargetStatus must be a valid claim status value.");

        // Withdrawal reason required — static rule, no DB needed.
        When(x => x.TargetStatus == ClaimStatus.Withdrawn, () =>
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Withdrawal reason is required."));

        // BR-ST-04 (static part): reopen reason must be provided.
        // Role authority is checked in the handler (requires current user context).
        When(x => x.TargetStatus == ClaimStatus.Reopened, () =>
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Reopen reason is required."));
    }
}
