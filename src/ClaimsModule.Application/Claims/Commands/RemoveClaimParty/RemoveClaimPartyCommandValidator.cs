using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.RemoveClaimParty;

public sealed class RemoveClaimPartyCommandValidator : AbstractValidator<RemoveClaimPartyCommand>
{
    public RemoveClaimPartyCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.PartyId)
            .NotEmpty().WithMessage("PartyId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");
    }
}
