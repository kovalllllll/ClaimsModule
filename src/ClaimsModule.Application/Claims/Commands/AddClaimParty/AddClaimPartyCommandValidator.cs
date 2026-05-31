using ClaimsModule.Domain.Enums;
using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.AddClaimParty;

public sealed class AddClaimPartyCommandValidator : AbstractValidator<AddClaimPartyCommand>
{
    public AddClaimPartyCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("ClaimId is required.");

        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("OrganisationId is required.");

        RuleFor(x => x.PartyRole)
            .IsInEnum().WithMessage("PartyRole is not a recognised value.");

        RuleFor(x => x.PartyType)
            .IsInEnum().WithMessage("PartyType is not a recognised value.");

        // Person: at least one of FirstName or LastName is required.
        When(x => x.PartyType == PartyType.Person, () =>
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.LastName))
                .WithMessage("FirstName or LastName is required for a Person party.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.FirstName))
                .WithMessage("FirstName or LastName is required for a Person party.");
        });

        // Company: CompanyName is required.
        When(x => x.PartyType == PartyType.Company, () =>
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("CompanyName is required for a Company party.");
        });

        // Email format when provided.
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email address is not valid.");
        });
    }
}
