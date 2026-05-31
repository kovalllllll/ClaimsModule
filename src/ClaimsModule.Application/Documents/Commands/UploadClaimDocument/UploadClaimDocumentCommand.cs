using ClaimsModule.Application.Common.Interfaces;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Documents.Commands.UploadClaimDocument;

public sealed record UploadClaimDocumentCommand(
    Guid ClaimId,
    Guid OrganisationId,
    DocumentType DocumentType,
    string DocumentName,
    string ContentType,
    long FileSizeBytes,
    Stream FileContent,
    string? Notes = null,
    string? IdempotencyKey = null
) : ICommand<Guid>;
