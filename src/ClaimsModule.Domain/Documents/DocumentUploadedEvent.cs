using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Documents;

public sealed record DocumentUploadedEvent(
    Guid ClaimId,
    Guid ClaimDocumentId) : DomainEvent;
