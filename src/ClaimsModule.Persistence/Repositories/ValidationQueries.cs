using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public sealed class ValidationQueries(ClaimsDbContext db) : IValidationQueries
{
    public Task<bool> ClaimExistsAsync(Guid claimId, Guid organisationId, CancellationToken cancellationToken = default)
        => db.Claims.AnyAsync(
            c => c.Id == claimId && c.OrganisationId == organisationId,
            cancellationToken);

    public Task<bool> ClaimHasLinkedPolicyAsync(Guid claimId, CancellationToken cancellationToken = default)
        => db.Claims.AnyAsync(c => c.Id == claimId && c.PolicyId != null, cancellationToken);

    public Task<bool> CauseOfLossCodeIsActiveAsync(string code, CancellationToken cancellationToken = default)
        => db.CauseOfLossCodes.AnyAsync(c => c.Code == code && c.IsActive, cancellationToken);

    public Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        => db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken);

    public Task<bool> ReserveComponentExistsAsync(
        Guid claimId,
        Guid organisationId,
        ReserveComponentType component,
        CancellationToken cancellationToken = default)
        => db.ClaimReserveComponents.AnyAsync(
            rc => rc.ClaimId == claimId
                  && rc.Component == component
                  && rc.OrganisationId == organisationId,
            cancellationToken);

    public Task<bool> ReserveComponentExistsByIdAsync(
        Guid componentId,
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.ClaimReserveComponents.AnyAsync(
            rc => rc.Id == componentId
                  && rc.ClaimId == claimId
                  && rc.OrganisationId == organisationId,
            cancellationToken);

    public async Task<bool> HasPendingApprovalForComponentTypeAsync(
        Guid claimId,
        ReserveComponentType component,
        CancellationToken cancellationToken = default)
    {
        var componentId = await db.ClaimReserveComponents
            .Where(rc => rc.ClaimId == claimId && rc.Component == component)
            .Select(rc => (Guid?)rc.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (componentId is null) return false;

        return await HasPendingApprovalForComponentIdAsync(componentId.Value, cancellationToken);
    }

    public Task<bool> HasPendingApprovalForComponentIdAsync(
        Guid componentId,
        CancellationToken cancellationToken = default)
        => db.ReserveHistory.AnyAsync(
            h => h.ReserveComponentId == componentId
                 && h.ApprovalStatus == ReserveApprovalStatus.PendingApproval,
            cancellationToken);

    public Task<bool> ReserveIdempotencyKeyExistsAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var parsed = IdempotencyKey.Parse(idempotencyKey);
        return db.ReserveHistory.AnyAsync(r => r.IdempotencyKey == parsed, cancellationToken);
    }

    public Task<bool> FailedGlPostingExistsAsync(
        Guid historyId,
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.ReserveHistory.AnyAsync(
            h => h.Id == historyId
                 && h.ClaimId == claimId
                 && h.OrganisationId == organisationId
                 && h.PostingStatus == ReservePostingStatus.Failed,
            cancellationToken);
}
