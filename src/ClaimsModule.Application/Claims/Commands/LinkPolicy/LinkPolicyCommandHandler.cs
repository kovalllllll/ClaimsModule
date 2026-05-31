using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.LinkPolicy;

public sealed class LinkPolicyCommandHandler(
    IClaimRepository claims,
    IValidationQueries validationQueries,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLog)
    : IRequestHandler<LinkPolicyCommand, Unit>
{
    public async Task<Unit> Handle(LinkPolicyCommand request, CancellationToken cancellationToken)
    {
        var claim = await claims.GetByIdWithPartiesForUpdateAsync(
                request.ClaimId, request.OrganisationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Claim {request.ClaimId} not found.");

        var policy = await validationQueries.GetPolicyByIdAsync(request.PolicyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Policy {request.PolicyId} not found.");

        claim.LinkPolicy(policy.Id, policy.PolicyNumber, policy.ClientName);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLog.WriteAsync(
            claimId: claim.Id,
            eventType: AuditEventType.ValidationIssueAdded,
            description: $"Policy {policy.PolicyNumber} linked to claim. Financial actions now permitted.",
            newValue: AuditJsonValues.ValidationIssue(
                $"Policy {policy.PolicyNumber} linked.",
                "Info"),
            ct: cancellationToken);

        // BR-C-02: check if any loss event's date falls outside the newly linked policy's effective period
        var primaryLoss = claim.LossEvents.FirstOrDefault();
        if (primaryLoss is not null)
        {
            var lossDateOnly = DateOnly.FromDateTime(primaryLoss.LossDate.UtcDateTime.Date);
            if (lossDateOnly < policy.EffectiveDate || lossDateOnly > policy.ExpirationDate)
            {
                await auditLog.WriteAsync(
                    claimId: claim.Id,
                    eventType: AuditEventType.ValidationIssueAdded,
                    description: "[Warning] Loss date is outside the linked policy's effective period.",
                    newValue: AuditJsonValues.ValidationIssue(
                        "Loss date is outside the linked policy's effective period.",
                        "Warning"),
                    ct: cancellationToken);
            }
        }

        return Unit.Value;
    }
}
