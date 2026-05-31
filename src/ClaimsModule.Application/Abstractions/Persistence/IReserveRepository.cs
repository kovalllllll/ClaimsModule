using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Application.Abstractions.Persistence;

public interface IReserveRepository
{
    Task<ClaimReserveComponent?> GetComponentByTypeAsync(
        Guid claimId,
        Guid organisationId,
        ReserveComponentType component,
        CancellationToken cancellationToken = default);

    Task<ClaimReserveComponent?> GetComponentByIdAsync(
        Guid componentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimReserveComponent>> GetComponentsByClaimIdAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimReserveComponent>> GetComponentsWithHistoryAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimReserveComponent>> GetComponentsForClaimIdsAsync(
        IReadOnlyList<Guid> claimIds,
        CancellationToken cancellationToken = default);

    Task<int> GetNextChangeSequenceAsync(
        Guid componentId,
        CancellationToken cancellationToken = default);

    Task AddComponentAsync(ClaimReserveComponent component, CancellationToken cancellationToken = default);

    Task AddHistoryAsync(ReserveHistory history, CancellationToken cancellationToken = default);

    Task<ReserveHistory?> GetHistoryByIdOnlyAsync(
        Guid historyId,
        CancellationToken cancellationToken = default);

    Task<ReserveHistory?> GetHistoryByIdAsync(
        Guid historyId,
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingApprovalAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<bool> HasOutstandingReservesAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<bool> HasApprovedReserveAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<bool> IsPostedForIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
