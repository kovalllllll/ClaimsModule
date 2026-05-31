using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Exceptions;
using FluentValidation.Results;
using ClaimsModule.Application.Common.Validation;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.TransitionClaimStatus;

public sealed class TransitionClaimStatusCommandHandler(
    IClaimRepository claims,
    IReserveRepository reserves,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IClaimClosureEvaluator closureEvaluator)
    : IRequestHandler<TransitionClaimStatusCommand, Unit>
{
    public async Task<Unit> Handle(TransitionClaimStatusCommand request, CancellationToken cancellationToken)
    {
        var claim = await claims.GetByIdWithPartiesForUpdateAsync(
                request.ClaimId, request.OrganisationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim {request.ClaimId} not found.");

        if (!string.IsNullOrWhiteSpace(request.RowVer))
        {
            var expected = Convert.FromBase64String(request.RowVer);
            if (!claim.RowVer.SequenceEqual(expected))
                throw new ConcurrencyException("The claim was modified by another user. Refresh and try again.");
        }

        if (!ClaimStatusTransitions.IsValid(claim.Status, request.TargetStatus))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    "TargetStatus",
                    $"Transition from {claim.Status} to {request.TargetStatus} is not permitted.")
            });
        }

        if (request.TargetStatus == ClaimStatus.Open)
        {
            if (await claims.HasUnresolvedCriticalValidationIssuesAsync(claim.Id, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "ValidationIssues",
                        "Claim has unresolved critical validation issues and cannot be opened.")
                });
            }

            var validationDescriptions = await claims.GetValidationIssueDescriptionsAsync(
                claim.Id, cancellationToken);
            var hasPolicyPeriodWarning = validationDescriptions.Any(d =>
                d.Contains("outside the policy effective period", StringComparison.OrdinalIgnoreCase));
            if (hasPolicyPeriodWarning && string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "Reason",
                        $"{ClaimValidationMessages.LossDateOutsidePolicyPeriod} " +
                        "Provide acknowledgment in the reason field before opening the claim.")
                });
            }

            var hasClaimant = claim.Parties.Any(p => p.IsActive && p.PartyRole == PartyRole.Claimant);
            if (!hasClaimant)
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "Parties",
                        "At least one Claimant party is required to open a claim.")
                });
        }

        if (request.TargetStatus == ClaimStatus.PendingPayment)
        {
            if (!await reserves.HasApprovedReserveAsync(request.ClaimId, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "Status",
                        "At least one approved reserve component is required to move to PendingPayment.")
                });
            }
        }

        if (request.TargetStatus == ClaimStatus.Closed)
        {
            var evaluation = await closureEvaluator.EvaluateAsync(
                claim.Id, request.OrganisationId, request.Reason, cancellationToken);

            if (!evaluation.CanClose)
            {
                var failures = evaluation.Conditions
                    .Where(c => !c.Passed)
                    .Select(c => new ValidationFailure(
                        "TargetStatus",
                        $"Claim cannot be closed — {c.Detail ?? c.Description} is not satisfied."))
                    .ToList();

                throw new StatusTransitionBlockedException(failures, evaluation.Conditions);
            }
        }

        if (request.TargetStatus == ClaimStatus.Reopened)
        {
            var role = currentUser.Role ?? string.Empty;
            if (!role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase)
                && !role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "Role",
                        "Only Supervisors or Managers can reopen a claim.")
                });
            }
        }

        claim.TransitionStatus(request.TargetStatus, request.Reason);
        if (request.TargetStatus == ClaimStatus.Reopened)
            claim.TransitionStatus(ClaimStatus.Open);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
