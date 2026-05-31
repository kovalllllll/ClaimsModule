using FluentValidation;

namespace ClaimsModule.Application.Reserves.Commands.RetractReserve;

public sealed class RetractReserveCommandValidator : AbstractValidator<RetractReserveCommand>
{
    public RetractReserveCommandValidator()
    {
        RuleFor(x => x.ReserveHistoryId)
            .NotEmpty().WithMessage("ReserveHistoryId is required.");

        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");
    }
}
