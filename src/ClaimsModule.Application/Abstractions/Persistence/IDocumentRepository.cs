using ClaimsModule.Domain.Documents;

namespace ClaimsModule.Application.Abstractions.Persistence;

public interface IDocumentRepository
{
    Task<IReadOnlyList<ClaimDocument>> GetByClaimIdAsync(
        Guid claimId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<ClaimDocument?> GetByIdAsync(
        Guid claimId,
        Guid documentId,
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(ClaimDocument document, CancellationToken cancellationToken = default);
}
