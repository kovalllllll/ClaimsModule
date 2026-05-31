using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Common;
using ClaimsModule.Domain.Audit;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public sealed class ClaimRepository(ClaimsDbContext db) : IClaimRepository
{
    public Task<Claim?> GetByIdWithPartiesReadOnlyAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.Claims
            .Include(c => c.Parties)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == claimId && c.OrganisationId == organisationId,
                cancellationToken);

    public Task<Claim?> GetByIdWithPartiesForUpdateAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.Claims
            .Include(c => c.Parties)
            .FirstOrDefaultAsync(
                c => c.Id == claimId && c.OrganisationId == organisationId,
                cancellationToken);

    public Task<Claim?> GetDetailByIdAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.Claims
            .Include(c => c.LossEvents)
            .Include(c => c.Parties)
            .Include(c => c.RiskObjects)
            .Include(c => c.Documents)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == claimId && c.OrganisationId == organisationId,
                cancellationToken);

    public Task<Claim?> GetByIdForUpdateAsync(Guid claimId, CancellationToken cancellationToken = default)
        => db.Claims.FirstOrDefaultAsync(c => c.Id == claimId, cancellationToken);

    public Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default)
        => db.Claims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == claimId, cancellationToken);

    public Task<bool> ExistsAsync(Guid claimId, Guid organisationId, CancellationToken cancellationToken = default)
        => db.Claims.AnyAsync(
            c => c.Id == claimId && c.OrganisationId == organisationId,
            cancellationToken);

    public async Task<ClaimListPage> ListAsync(
        ClaimListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = db.Claims
            .AsNoTracking()
            .Where(c => c.OrganisationId == criteria.OrganisationId);

        if (criteria.Statuses is { Count: > 0 })
            query = query.Where(c => criteria.Statuses.Contains(c.Status));
        else if (criteria.Status.HasValue)
            query = query.Where(c => c.Status == criteria.Status.Value);

        if (!string.IsNullOrWhiteSpace(criteria.AssignedHandlerSearch))
        {
            var handlerIds = MockUserNames.FindIdsByNameSearch(criteria.AssignedHandlerSearch);
            if (handlerIds.Count == 0)
                return new ClaimListPage([], 0);

            query = query.Where(c =>
                c.AssignedHandlerId.HasValue && handlerIds.Contains(c.AssignedHandlerId.Value));
        }

        if (criteria.DateFrom.HasValue)
        {
            var from = criteria.DateFrom.Value;
            query = query.Where(c => c.LossEvents.Any(le => le.LossDate >= from));
        }

        if (criteria.DateTo.HasValue)
        {
            var to = criteria.DateTo.Value;
            query = query.Where(c => c.LossEvents.Any(le => le.LossDate <= to));
        }

        if (criteria.AssignedHandlerId.HasValue)
            query = query.Where(c => c.AssignedHandlerId == criteria.AssignedHandlerId.Value);

        if (criteria.PolicyId.HasValue)
            query = query.Where(c => c.PolicyId == criteria.PolicyId.Value);

        if (!string.IsNullOrWhiteSpace(criteria.CauseOfLossCode))
        {
            var code = criteria.CauseOfLossCode;
            query = query.Where(c => c.LossEvents.Any(le => le.CauseOfLossCode == code));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var term = criteria.Search.Trim();
            var candidates = await query
                .Select(c => new { c.Id, c.ClaimNumber, c.ClientName, c.PolicyNumber })
                .ToListAsync(cancellationToken);

            var matchingIds = candidates
                .Where(c =>
                    c.ClaimNumber.Value.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (c.ClientName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.PolicyNumber?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                .Select(c => c.Id)
                .ToList();

            query = query.Where(c => matchingIds.Contains(c.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            return new ClaimListPage([], 0);

        var claimsPage = await query
            .Include(c => c.LossEvents)
            .OrderByDescending(c => c.ReportedDate)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new ClaimListPage(claimsPage, totalCount);
    }

    public Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
        => db.Claims.AddAsync(claim, cancellationToken).AsTask();

    public Task AddLossEventAsync(LossEvent lossEvent, CancellationToken cancellationToken = default)
        => db.LossEvents.AddAsync(lossEvent, cancellationToken).AsTask();

    public Task AddPartyAsync(ClaimParty party, CancellationToken cancellationToken = default)
        => db.ClaimParties.AddAsync(party, cancellationToken).AsTask();

    public Task AddRiskObjectAsync(ClaimRiskObject riskObject, CancellationToken cancellationToken = default)
        => db.ClaimRiskObjects.AddAsync(riskObject, cancellationToken).AsTask();

    public Task<ClaimParty?> GetPartyAsync(
        Guid partyId,
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.ClaimParties.FirstOrDefaultAsync(
            p => p.Id == partyId && p.ClaimId == claimId && p.OrganisationId == organisationId,
            cancellationToken);

    public async Task<IReadOnlyList<ClaimParty>> GetPartiesForClaimAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => await db.ClaimParties
            .AsNoTracking()
            .Where(p => p.ClaimId == claimId && p.OrganisationId == organisationId)
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.PartyRole)
            .ToListAsync(cancellationToken);

    public Task<int> CountActiveClaimantsAsync(Guid claimId, CancellationToken cancellationToken = default)
        => db.ClaimParties.CountAsync(
            p => p.ClaimId == claimId && p.IsActive && p.PartyRole == PartyRole.Claimant,
            cancellationToken);

    public async Task<IReadOnlyList<string>> GetValidationIssueDescriptionsAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
        => await db.ClaimAuditLog
            .Where(a => a.ClaimId == claimId && a.EventType == AuditEventType.ValidationIssueAdded)
            .Select(a => a.Description)
            .ToListAsync(cancellationToken);

    public Task<bool> HasUnresolvedCriticalValidationIssuesAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
        => db.ClaimAuditLog.AnyAsync(
            a => a.ClaimId == claimId &&
                 a.EventType == AuditEventType.ValidationIssueAdded &&
                 !a.Description.StartsWith("[Warning]") &&
                 !a.Description.StartsWith("[Info]"),
            cancellationToken);

    public async Task<IReadOnlyList<ClaimAuditLog>> GetRecentAuditAsync(
        Guid claimId,
        int take,
        CancellationToken cancellationToken = default)
        => await db.ClaimAuditLog
            .Where(a => a.ClaimId == claimId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<ClaimAuditLog> Items, int TotalCount)> GetAuditPagedAsync(
        Guid claimId,
        Guid organisationId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = db.ClaimAuditLog
            .Where(a => a.ClaimId == claimId && a.OrganisationId == organisationId)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task AddAuditEntryAsync(ClaimAuditLog entry, CancellationToken cancellationToken = default)
        => db.ClaimAuditLog.AddAsync(entry, cancellationToken).AsTask();

    public Task<bool> HasAuditEntryAsync(
        Guid claimId,
        AuditEventType eventType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
        => db.ClaimAuditLog.AnyAsync(
            a => a.ClaimId == claimId
                 && a.EventType == eventType
                 && a.RelatedEntityId == relatedEntityId,
            cancellationToken);

    public async Task<Guid?> GetOrganisationIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var orgId = await db.Claims
            .Where(c => c.Id == claimId)
            .Select(c => (Guid?)c.OrganisationId)
            .FirstOrDefaultAsync(cancellationToken);
        return orgId;
    }

    public async Task<IReadOnlyList<Claim>> GetStaleOpenClaimsForSlaAsync(
        DateTimeOffset updatedBefore,
        CancellationToken cancellationToken = default)
        => await db.Claims
            .AsNoTracking()
            .Where(c => (c.Status == ClaimStatus.Draft || c.Status == ClaimStatus.Open)
                        && (c.UpdatedAt ?? c.CreatedAt) < updatedBefore)
            .ToListAsync(cancellationToken);

    public Task<bool> HasRecentSlaBreachAuditAsync(
        Guid claimId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
        => db.ClaimAuditLog.AnyAsync(
            a => a.ClaimId == claimId
                 && a.EventType == AuditEventType.SlaBreachDetected
                 && a.CreatedAt >= since,
            cancellationToken);
}
