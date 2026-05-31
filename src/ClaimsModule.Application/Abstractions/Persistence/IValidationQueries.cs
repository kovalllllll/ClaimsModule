using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Application.Abstractions.Persistence;

public interface IValidationQueries
{
    Task<bool> ClaimExistsAsync(Guid claimId, Guid organisationId, CancellationToken cancellationToken = default);

    Task<bool> ClaimHasLinkedPolicyAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<bool> CauseOfLossCodeIsActiveAsync(string code, CancellationToken cancellationToken = default);

    Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken = default);

    Task<bool> ReserveComponentExistsAsync(
        Guid claimId,
        Guid organisationId,
        ReserveComponentType component,
        CancellationToken cancellationToken = default);

    Task<bool> ReserveComponentExistsByIdAsync(
        Guid componentId,
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingApprovalForComponentTypeAsync(
        Guid claimId,
        ReserveComponentType component,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingApprovalForComponentIdAsync(
        Guid componentId,
        CancellationToken cancellationToken = default);

    Task<bool> ReserveIdempotencyKeyExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task<bool> FailedGlPostingExistsAsync(
        Guid historyId,
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);
}
