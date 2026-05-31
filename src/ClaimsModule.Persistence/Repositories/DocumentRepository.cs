using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Repositories;

public sealed class DocumentRepository(ClaimsDbContext db) : IDocumentRepository
{
    public async Task<IReadOnlyList<ClaimDocument>> GetByClaimIdAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
    {
        return await db.ClaimDocuments
            .Where(d => d.ClaimId == claimId && d.OrganisationId == organisationId)
            .OrderByDescending(d => d.UploadedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<ClaimDocument?> GetByIdAsync(
        Guid claimId,
        Guid documentId,
        Guid organisationId,
        CancellationToken cancellationToken = default)
        => db.ClaimDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                d => d.Id == documentId && d.ClaimId == claimId && d.OrganisationId == organisationId,
                cancellationToken);

    public Task AddAsync(ClaimDocument document, CancellationToken cancellationToken = default)
        => db.ClaimDocuments.AddAsync(document, cancellationToken).AsTask();
}
