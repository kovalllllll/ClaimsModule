using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Common.Audit;

public static class AuditEventTypeFormatter
{
    private static readonly IReadOnlyDictionary<AuditEventType, string> ToSpecification =
        new Dictionary<AuditEventType, string>
        {
            [AuditEventType.ClaimCreated] = "CLAIM_CREATED",
            [AuditEventType.StatusChanged] = "STATUS_CHANGED",
            [AuditEventType.PartyAdded] = "PARTY_ADDED",
            [AuditEventType.PartyRemoved] = "PARTY_REMOVED",
            [AuditEventType.ReserveCreated] = "RESERVE_CREATED",
            [AuditEventType.ReserveAutoApproved] = "RESERVE_AUTO_APPROVED",
            [AuditEventType.ReserveApproved] = "RESERVE_APPROVED",
            [AuditEventType.ReserveRejected] = "RESERVE_REJECTED",
            [AuditEventType.ReserveRetracted] = "RESERVE_RETRACTED",
            [AuditEventType.GlPostingSimulated] = "GL_POSTING_SIMULATED",
            [AuditEventType.GlPostingFailed] = "GL_POSTING_FAILED",
            [AuditEventType.DocumentUploaded] = "DOCUMENT_UPLOADED",
            [AuditEventType.ClaimClosed] = "CLAIM_CLOSED",
            [AuditEventType.ClaimReopened] = "CLAIM_REOPENED",
            [AuditEventType.SlaBreachDetected] = "SLA_BREACH_DETECTED",
            [AuditEventType.ValidationIssueAdded] = "VALIDATION_ISSUE_ADDED"
        };

    private static readonly IReadOnlyDictionary<string, AuditEventType> FromSpecification =
        ToSpecification.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.OrdinalIgnoreCase);

    public static string ToSpecificationString(AuditEventType eventType) =>
        ToSpecification[eventType];

    public static AuditEventType Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (FromSpecification.TryGetValue(value, out var mapped))
            return mapped;

        if (Enum.TryParse<AuditEventType>(value, ignoreCase: true, out var parsed))
            return parsed;

        throw new FormatException($"Unknown audit event type '{value}'.");
    }
}
