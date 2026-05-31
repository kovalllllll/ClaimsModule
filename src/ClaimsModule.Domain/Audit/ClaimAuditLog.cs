using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Domain.Audit;

public sealed class ClaimAuditLog : Entity, ITenantScoped
{
    public Guid OrganisationId { get; private set; }
    public Guid ClaimId { get; private set; }
    public AuditEventType EventType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public Guid? UserModified { get; private set; }

    private ClaimAuditLog() { }

    public static ClaimAuditLog Create(
        Guid claimId,
        Guid organisationId,
        AuditEventType eventType,
        string description,
        Guid? createdByUserId,
        DateTimeOffset createdAt,
        Guid? correlationId = null,
        string? oldValue = null,
        string? newValue = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
        => new()
        {
            Id = EntityId.New(),
            ClaimId = claimId,
            OrganisationId = organisationId,
            EventType = eventType,
            Description = description,
            CreatedByUserId = createdByUserId,
            CreatedAt = createdAt,
            CorrelationId = correlationId,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        };
}
