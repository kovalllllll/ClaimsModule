using ClaimsModule.Domain.Audit;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;

namespace ClaimsModule.Application.Abstractions.Persistence;

public sealed record ClaimListCriteria(
    Guid OrganisationId,
    Domain.Enums.ClaimStatus? Status = null,
    IReadOnlyList<Domain.Enums.ClaimStatus>? Statuses = null,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null,
    Guid? AssignedHandlerId = null,
    string? AssignedHandlerSearch = null,
    string? CauseOfLossCode = null,
    Guid? PolicyId = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record ClaimListPage(IReadOnlyList<Claim> Claims, int TotalCount);

public interface IClaimRepository
{
    Task<Claim?> GetByIdWithPartiesReadOnlyAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<Claim?> GetByIdWithPartiesForUpdateAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<Claim?> GetDetailByIdAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<Claim?> GetByIdForUpdateAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid claimId, Guid organisationId, CancellationToken cancellationToken = default);

    Task<ClaimListPage> ListAsync(ClaimListCriteria criteria, CancellationToken cancellationToken = default);

    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);

    Task AddLossEventAsync(LossEvent lossEvent, CancellationToken cancellationToken = default);

    Task AddPartyAsync(ClaimParty party, CancellationToken cancellationToken = default);

    Task AddRiskObjectAsync(ClaimRiskObject riskObject, CancellationToken cancellationToken = default);

    Task<ClaimParty?> GetPartyAsync(
        Guid partyId,
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimParty>> GetPartiesForClaimAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveClaimantsAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetValidationIssueDescriptionsAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<bool> HasUnresolvedCriticalValidationIssuesAsync(
        Guid claimId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimAuditLog>> GetRecentAuditAsync(
        Guid claimId,
        int take,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ClaimAuditLog> Items, int TotalCount)> GetAuditPagedAsync(
        Guid claimId,
        Guid organisationId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAuditEntryAsync(ClaimAuditLog entry, CancellationToken cancellationToken = default);

    Task<bool> HasAuditEntryAsync(
        Guid claimId,
        AuditEventType eventType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetOrganisationIdAsync(Guid claimId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Claim>> GetStaleOpenClaimsForSlaAsync(
        DateTimeOffset updatedBefore,
        CancellationToken cancellationToken = default);

    Task<bool> HasRecentSlaBreachAuditAsync(
        Guid claimId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);
}
