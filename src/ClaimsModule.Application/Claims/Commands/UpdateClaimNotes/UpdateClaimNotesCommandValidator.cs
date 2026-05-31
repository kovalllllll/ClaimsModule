using ClaimsModule.Application.Abstractions.Persistence;
using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.UpdateClaimNotes;

public sealed class UpdateClaimNotesCommandValidator : AbstractValidator<UpdateClaimNotesCommand>
{
    public UpdateClaimNotesCommandValidator(IValidationQueries validationQueries)
    {
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.OrganisationId).NotEmpty();
        RuleFor(x => x.Notes)
            .MaximumLength(8000)
            .When(x => x.Notes is not null);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await validationQueries.ClaimExistsAsync(cmd.ClaimId, cmd.OrganisationId, ct))
            .WithMessage("Claim not found.");
    }
}
