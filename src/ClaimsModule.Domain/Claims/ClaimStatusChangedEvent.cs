using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Domain.Claims;

public sealed record ClaimStatusChangedEvent(
    Guid ClaimId,
    ClaimStatus FromStatus,
    ClaimStatus ToStatus,
    string? Reason = null) : DomainEvent;
