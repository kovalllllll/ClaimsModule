using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using FluentValidation;

namespace ClaimsModule.Application.Reserves.Commands.RetryGlPosting;

public sealed class RetryGlPostingCommandValidator : AbstractValidator<RetryGlPostingCommand>
{
    public RetryGlPostingCommandValidator(IValidationQueries validationQueries, ICurrentUserService currentUser)
    {
        RuleFor(x => x.ReserveHistoryId).NotEmpty();
        RuleFor(x => x.ClaimId).NotEmpty();
        RuleFor(x => x.OrganisationId).NotEmpty();

        RuleFor(x => x)
            .Must(_ => currentUser.Role is "supervisor" or "manager")
            .WithMessage("Only supervisor or manager roles may retry GL posting.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                await validationQueries.FailedGlPostingExistsAsync(
                    cmd.ReserveHistoryId, cmd.ClaimId, cmd.OrganisationId, ct))
            .WithMessage("GL posting retry is only allowed when posting status is Failed.");
    }
}
