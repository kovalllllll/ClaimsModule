using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Reserves;

public sealed record ReserveRejectedEvent(
    Guid ClaimId,
    Guid ReserveComponentId,
    Guid ReserveHistoryId,
    string RejectionReason) : DomainEvent;
