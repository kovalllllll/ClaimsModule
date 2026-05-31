using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public sealed class ApiIdempotencyRepository(ClaimsDbContext db) : IApiIdempotencyRepository
{
    public Task<ApiIdempotencyRecord?> FindAsync(
        Guid organisationId,
        string operation,
        string key,
        CancellationToken cancellationToken = default)
        => db.ApiIdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.OrganisationId == organisationId
                     && r.Operation == operation
                     && r.Key == key,
                cancellationToken);

    public Task AddAsync(ApiIdempotencyRecord record, CancellationToken cancellationToken = default)
        => db.ApiIdempotencyRecords.AddAsync(record, cancellationToken).AsTask();
}
