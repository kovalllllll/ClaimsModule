namespace ClaimsModule.Domain.Enums;

public enum AuditEventType
{
    ClaimCreated,
    StatusChanged,
    PartyAdded,
    PartyRemoved,
    ReserveCreated,
    ReserveAutoApproved,
    ReserveApproved,
    ReserveRejected,
    ReserveRetracted,
    GlPostingSimulated,
    GlPostingFailed,
    DocumentUploaded,
    ClaimClosed,
    ClaimReopened,
    SlaBreachDetected,
    ValidationIssueAdded
}
