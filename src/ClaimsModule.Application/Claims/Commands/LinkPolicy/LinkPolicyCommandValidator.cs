using ClaimsModule.Application.Abstractions.Persistence;
using FluentValidation;

namespace ClaimsModule.Application.Claims.Commands.LinkPolicy;

public sealed class LinkPolicyCommandValidator : AbstractValidator<LinkPolicyCommand>
{
    public LinkPolicyCommandValidator(IValidationQueries validationQueries)
    {
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.OrganisationId).NotEmpty();
        RuleFor(x => x.PolicyId).NotEmpty().WithMessage("PolicyId is required.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await validationQueries.ClaimExistsAsync(cmd.ClaimId, cmd.OrganisationId, ct))
            .WithName("ClaimId")
            .WithMessage("Claim not found.");

        RuleFor(x => x.PolicyId)
            .MustAsync(async (policyId, ct) =>
                await validationQueries.GetPolicyByIdAsync(policyId, ct) is not null)
            .WithMessage("Policy not found.");
    }
}
