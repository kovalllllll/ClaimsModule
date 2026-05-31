using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Reserves;

public sealed record ReserveApprovedEvent(
    Guid ClaimId,
    Guid ReserveComponentId,
    Guid ReserveHistoryId) : DomainEvent;
