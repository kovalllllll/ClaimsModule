using ClaimsModule.Domain.Common;

namespace ClaimsModule.Application.Abstractions.Persistence;

public interface IApiIdempotencyRepository
{
    Task<ApiIdempotencyRecord?> FindAsync(
        Guid organisationId,
        string operation,
        string key,
        CancellationToken cancellationToken = default);

    Task AddAsync(ApiIdempotencyRecord record, CancellationToken cancellationToken = default);
}
