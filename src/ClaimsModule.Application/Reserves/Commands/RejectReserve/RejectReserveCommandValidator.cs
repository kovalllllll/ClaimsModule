using FluentValidation;

namespace ClaimsModule.Application.Reserves.Commands.RejectReserve;

public sealed class RejectReserveCommandValidator : AbstractValidator<RejectReserveCommand>
{
    public RejectReserveCommandValidator()
    {
        RuleFor(x => x.ReserveHistoryId)
            .NotEmpty().WithMessage("ReserveHistoryId is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");

        // Rejection reason is required — reviewers need to understand why it was rejected (BR-R-04).
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required.");
    }
}
