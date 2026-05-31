using FluentValidation;

namespace ClaimsModule.Application.Reserves.Commands.ApproveReserve;

/// <summary>
/// Structural validation for ApproveReserveCommand.
/// Role authority, self-approval, and aggregate total checks are
/// enforced in the handler (all require database access).
/// </summary>
public sealed class ApproveReserveCommandValidator : AbstractValidator<ApproveReserveCommand>
{
    public ApproveReserveCommandValidator()
    {
        RuleFor(x => x.ReserveHistoryId)
            .NotEmpty().WithMessage("ReserveHistoryId is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");
    }
}
