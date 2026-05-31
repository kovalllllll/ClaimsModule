using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using ClaimsModule.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public sealed class ReserveRepository(ClaimsDbContext db) : IReserveRepository
{
    public Task<ClaimReserveComponent?> GetComponentByTypeAsync(
        Guid claimId,
        Guid organisationId,
        ReserveComponentType component,
        CancellationToken cancellationToken = default)
        => db.ClaimReserveComponents.FirstOrDefaultAsync(
            rc => rc.ClaimId == claimId
                  && rc.Component == component
                  && rc.OrganisationId == organisationId,
            cancellationToken);

    public Task<ClaimReserveComponent?> GetComponentByIdAsync(
        Guid componentId,
        CancellationToken cancellationToken = default)
        => db.ClaimReserveComponents.FirstOrDefaultAsync(c => c.Id == componentId, cancellationToken);

    public async Task<IReadOnlyList<ClaimReserveComponent>> GetComponentsByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
        => await db.ClaimReserveComponents
            .Where(rc => rc.ClaimId == claimId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ClaimReserveComponent>> GetComponentsWithHistoryAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
        => await db.ClaimReserveComponents
            .Include(rc => rc.History)
            .Where(rc => rc.ClaimId == claimId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ClaimReserveComponent>> GetComponentsForClaimIdsAsync(
        IReadOnlyList<Guid> claimIds,
        CancellationToken cancellationToken = default)
        => claimIds.Count == 0
            ? []
            : await db.ClaimReserveComponents
                .Where(rc => claimIds.Contains(rc.ClaimId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

    public async Task<int> GetNextChangeSequenceAsync(
        Guid componentId,
        CancellationToken cancellationToken = default)
    {
        var max = await db.ReserveHistory
            .Where(h => h.ReserveComponentId == componentId)
            .MaxAsync(h => (int?)h.ChangeSequence, cancellationToken);

        return (max ?? 0) + 1;
    }

    public Task AddComponentAsync(ClaimReserveComponent component, CancellationToken cancellationToken = default)
        => db.ClaimReserveComponents.AddAsync(component, cancellationToken).AsTask();

    public Task AddHistoryAsync(ReserveHistory history, CancellationToken cancellationToken = default)
        => db.ReserveHistory.AddAsync(history, cancellationToken).AsTask();

    public Task<ReserveHistory?> GetHistoryByIdOnlyAsync(
        Guid historyId,
        CancellationToken cancellationToken = default)
        => db.ReserveHistory.FirstOrDefaultAsync(h => h.Id == historyId, cancellationToken);

    public Task<ReserveHistory?> GetHistoryByIdAsync(
        Guid historyId,
        Guid claimId,
        CancellationToken cancellationToken = default)
        => db.ReserveHistory.FirstOrDefaultAsync(
            h => h.Id == historyId && h.ClaimId == claimId,
            cancellationToken);

    public Task<bool> HasPendingApprovalAsync(Guid claimId, CancellationToken cancellationToken = default)
        => db.ReserveHistory.AnyAsync(
            r => r.ClaimId == claimId && r.ApprovalStatus == ReserveApprovalStatus.PendingApproval,
            cancellationToken);

    public async Task<bool> HasOutstandingReservesAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var components = await db.ClaimReserveComponents
            .Where(rc => rc.ClaimId == claimId)
            .ToListAsync(cancellationToken);

        return components.Any(rc => rc.CurrentAmount.Amount > 0);
    }

    public Task<bool> HasApprovedReserveAsync(Guid claimId, CancellationToken cancellationToken = default)
        => db.ReserveHistory.AnyAsync(
            r => r.ClaimId == claimId &&
                 (r.ApprovalStatus == ReserveApprovalStatus.Approved ||
                  r.ApprovalStatus == ReserveApprovalStatus.AutoApproved),
            cancellationToken);

    public Task<bool> IsPostedForIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var parsed = IdempotencyKey.Parse(idempotencyKey);
        return db.ReserveHistory.AnyAsync(
            h => h.IdempotencyKey == parsed
                 && h.PostingStatus == ReservePostingStatus.Posted,
            cancellationToken);
    }
}
