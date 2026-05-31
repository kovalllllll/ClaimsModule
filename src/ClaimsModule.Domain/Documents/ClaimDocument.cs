using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Domain.Documents;

public sealed class ClaimDocument : AuditableAggregateRoot
{
    public Guid ClaimId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DocumentName { get; private set; } = string.Empty;
    public string BlobPath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }
    public Guid? UploadedByUserId { get; private set; }
    public string? Notes { get; private set; }

    private ClaimDocument() { }

    public static ClaimDocument Create(
        Guid claimId,
        Guid organisationId,
        DocumentType documentType,
        string documentName,
        string blobPath,
        string contentType,
        long fileSizeBytes,
        DateTimeOffset uploadedAt,
        Guid? uploadedByUserId,
        string? notes = null)
    {
        var document = new ClaimDocument
        {
            Id = EntityId.New(),
            ClaimId = claimId,
            OrganisationId = organisationId,
            DocumentType = documentType,
            DocumentName = documentName,
            BlobPath = blobPath,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            UploadedAt = uploadedAt,
            UploadedByUserId = uploadedByUserId,
            Notes = notes,
            CreatedAt = uploadedAt
        };

        document.RaiseDomainEvent(new DocumentUploadedEvent(claimId, document.Id));
        return document;
    }
}
