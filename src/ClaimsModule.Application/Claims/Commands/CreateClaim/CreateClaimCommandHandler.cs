using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using MediatR;

namespace ClaimsModule.Application.Claims.Commands.CreateClaim;

public sealed class CreateClaimCommandHandler(
    IClaimRepository claims,
    IReserveRepository reserves,
    IApiIdempotencyRepository idempotency,
    IUnitOfWork unitOfWork,
    IClaimNumberGenerator claimNumberGenerator,
    IAuditLogService auditLog,
    ICurrentUserService currentUser,
    ISystemClock clock,
    IValidationWarningCollector warnings)
    : IRequestHandler<CreateClaimCommand, CreateClaimResult>
{
    private const decimal AutoApprovalThreshold = 10_000m;

    public async Task<CreateClaimResult> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await idempotency.FindAsync(
                request.OrganisationId, "CreateClaim", request.IdempotencyKey, cancellationToken);

            if (existing is not null)
            {
                var existingClaim = await claims.GetByIdAsync(existing.ResourceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Claim {existing.ResourceId} not found.");

                return new CreateClaimResult(
                    ClaimId: existing.ResourceId,
                    ClaimNumber: existingClaim.ClaimNumber.Value,
                    Warnings: []);
            }
        }

        var year = clock.UtcNow.Year;
        var seq = await claimNumberGenerator.AllocateNextSequenceAsync(
            request.OrganisationId, year, cancellationToken);
        var claimNumber = ClaimNumber.Parse($"CLM-{year}-{seq:D7}");

        var claim = Claim.Create(
            organisationId: request.OrganisationId,
            claimNumber: claimNumber,
            policyId: request.PolicyId,
            policyNumber: request.PolicyNumber,
            clientName: request.ClientName,
            severity: request.Severity ?? ClaimSeverity.Standard,
            reportedDate: clock.UtcNow);

        await claims.AddAsync(claim, cancellationToken);

        var lossEvent = LossEvent.Create(
            claimId: claim.Id,
            organisationId: request.OrganisationId,
            lossDate: request.LossDate,
            lossDescription: request.LossDescription,
            lossLocation: request.LossLocation,
            causeOfLossCode: request.CauseOfLossCode,
            estimatedLossAmount: request.EstimatedLossAmount.HasValue
                ? new Money(request.EstimatedLossAmount.Value)
                : null,
            reportDate: clock.UtcNow,
            policeReportNumber: request.PoliceReportNumber);

        await claims.AddLossEventAsync(lossEvent, cancellationToken);

        foreach (var p in request.Parties)
        {
            var party = ClaimParty.Create(
                claimId: claim.Id,
                organisationId: request.OrganisationId,
                partyRole: p.PartyRole,
                partyType: p.PartyType,
                firstName: p.FirstName,
                lastName: p.LastName,
                companyName: p.CompanyName,
                email: p.Email,
                phone: p.Phone,
                notes: p.Notes);
            await claims.AddPartyAsync(party, cancellationToken);
        }

        foreach (var ro in request.RiskObjects)
        {
            var riskObject = ClaimRiskObject.Create(
                claimId: claim.Id,
                organisationId: request.OrganisationId,
                assetType: ro.AssetType,
                assetDescription: ro.AssetDescription,
                damageDescription: ro.DamageDescription,
                isPrimary: ro.IsPrimary,
                assetReference: ro.AssetReference);
            await claims.AddRiskObjectAsync(riskObject, cancellationToken);
        }

        ClaimReserveComponent? component = null;
        ReserveHistory? history = null;
        bool autoApprove = false;

        if (request.InitialReserve is not null && request.PolicyId.HasValue)
        {
            var input = request.InitialReserve;
            var newAmount = new Money(input.Amount);
            var now = clock.UtcNow;
            autoApprove = Math.Abs(input.Amount) <= AutoApprovalThreshold;

            component = ClaimReserveComponent.Create(
                claimId: claim.Id,
                organisationId: request.OrganisationId,
                component: input.ComponentType);

            history = ReserveHistory.Create(
                reserveComponentId: component.Id,
                claimId: claim.Id,
                organisationId: request.OrganisationId,
                transactionType: ReserveTransactionType.Add,
                amount: newAmount,
                previousBalance: Money.Zero,
                newBalance: autoApprove ? newAmount : Money.Zero,
                approvalStatus: autoApprove
                    ? ReserveApprovalStatus.AutoApproved
                    : ReserveApprovalStatus.PendingApproval,
                changeReason: input.ChangeReason,
                idempotencyKey: IdempotencyKey.ForReserveChange(component.Id, 1),
                changeSequence: 1,
                submittedByUserId: currentUser.UserId,
                createdAt: now);

            if (autoApprove)
            {
                history.AutoApprove(currentUser.UserId!.Value, now);
                component.Approve(history.Id, newAmount);
            }

            await reserves.AddComponentAsync(component, cancellationToken);
            await reserves.AddHistoryAsync(history, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (component is not null && history is not null)
        {
            var reserveEventType = autoApprove
                ? AuditEventType.ReserveAutoApproved
                : AuditEventType.ReserveCreated;

            await auditLog.WriteAsync(
                claimId: claim.Id,
                eventType: reserveEventType,
                description: $"Initial reserve {request.InitialReserve!.ComponentType} " +
                             $"{(autoApprove ? "auto-approved" : "submitted for approval")} " +
                             $"for {history.Amount}.",
                correlationId: request.CorrelationId,
                relatedEntityId: history.Id,
                relatedEntityType: nameof(ReserveHistory),
                ct: cancellationToken);
        }

        if (request.RiskObjects.Count == 0)
        {
            await auditLog.WriteAsync(
                claimId: claim.Id,
                eventType: AuditEventType.ValidationIssueAdded,
                description: "[Warning] RiskObjects: No risk objects linked to this claim.",
                correlationId: request.CorrelationId,
                newValue: AuditJsonValues.ValidationIssue(
                    "No risk objects linked to this claim.",
                    "Warning"),
                ct: cancellationToken);
        }

        foreach (var w in warnings.Warnings)
        {
            await auditLog.WriteAsync(
                claimId: claim.Id,
                eventType: AuditEventType.ValidationIssueAdded,
                description: $"[Warning] {w.PropertyName}: {w.Message}",
                correlationId: request.CorrelationId,
                newValue: AuditJsonValues.ValidationIssue(w.Message, "Warning"),
                ct: cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            await idempotency.AddAsync(new ApiIdempotencyRecord
            {
                Id = EntityId.New(),
                OrganisationId = request.OrganisationId,
                Key = request.IdempotencyKey,
                Operation = "CreateClaim",
                ResourceId = claim.Id,
                CreatedAt = clock.UtcNow
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new CreateClaimResult(
            ClaimId: claim.Id,
            ClaimNumber: claimNumber.Value,
            Warnings: warnings.Warnings.Select(w => w.Message).ToList());
    }
}
