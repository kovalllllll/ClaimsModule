using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Application.Abstractions.Persistence;

public interface IPolicyRepository
{
    Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchPagedAsync(
        Guid organisationId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Policy?> GetByIdAsync(
        Guid policyId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CauseOfLossCode>> GetCauseOfLossCodesAsync(
        Guid organisationId,
        string? perilCategory,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetCauseOfLossCodeNamesAsync(
        CancellationToken cancellationToken = default);
}
