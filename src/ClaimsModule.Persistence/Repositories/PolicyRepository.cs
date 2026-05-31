using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Policies;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public sealed class PolicyRepository(ClaimsDbContext db) : IPolicyRepository
{
    public async Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchPagedAsync(
        Guid organisationId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = db.Policies
            .AsNoTracking()
            .Where(p => p.OrganisationId == organisationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.PolicyNumber.ToLower().Contains(s) ||
                p.ClientName.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.PolicyNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Policy?> GetByIdAsync(
        Guid policyId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == policyId && p.OrganisationId == organisationId,
                cancellationToken);

    public async Task<IReadOnlyList<CauseOfLossCode>> GetCauseOfLossCodesAsync(
        Guid organisationId,
        string? perilCategory,
        CancellationToken cancellationToken = default)
    {
        var query = db.CauseOfLossCodes
            .AsNoTracking()
            .Where(c => c.OrganisationId == organisationId && c.IsActive);

        if (!string.IsNullOrWhiteSpace(perilCategory)
            && Enum.TryParse<PerilCategory>(perilCategory, true, out var peril))
        {
            query = query.Where(c => c.PerilCategory == peril);
        }

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetCauseOfLossCodeNamesAsync(
        CancellationToken cancellationToken = default)
        => await db.CauseOfLossCodes
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, c => c.Name, cancellationToken);
}
