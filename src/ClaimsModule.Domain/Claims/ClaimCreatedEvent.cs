using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Claims;

public sealed record ClaimCreatedEvent(Guid ClaimId) : DomainEvent;
