using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common;
using ClaimsModule.Application.DTOs;
using MediatR;

namespace ClaimsModule.Application.Documents.Queries.GetClaimDocuments;

public sealed class GetClaimDocumentsQueryHandler(IDocumentRepository documents, IStorageService storage)
    : IRequestHandler<GetClaimDocumentsQuery, IReadOnlyList<ClaimDocumentDto>>
{
    public async Task<IReadOnlyList<ClaimDocumentDto>> Handle(
        GetClaimDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var documents1 = await documents.GetByClaimIdAsync(
            request.ClaimId, request.OrganisationId, cancellationToken);

        var result = new List<ClaimDocumentDto>(documents1.Count);
        foreach (var doc in documents1)
        {
            var blobPath = StorageBlobPathNormalizer.ResolveReadPath(doc.BlobPath);
            var sasUrl = await storage.GetSasUrlAsync(blobPath, TimeSpan.FromHours(1), cancellationToken);
            result.Add(new ClaimDocumentDto
            {
                Id = doc.Id,
                DocumentType = doc.DocumentType.ToString(),
                DocumentName = doc.DocumentName,
                ContentType = doc.ContentType,
                FileSizeBytes = doc.FileSizeBytes,
                UploadedAt = doc.UploadedAt,
                UploadedByUserId = doc.UploadedByUserId,
                Notes = doc.Notes,
                SasUrl = sasUrl
            });
        }

        return result;
    }
}
