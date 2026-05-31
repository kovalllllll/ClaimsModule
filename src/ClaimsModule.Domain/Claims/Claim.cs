using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Documents;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Parties;
using ClaimsModule.Domain.ValueObjects;

namespace ClaimsModule.Domain.Claims;

public sealed class Claim : AuditableAggregateRoot
{
    private readonly List<LossEvent> _lossEvents = new();
    private readonly List<ClaimParty> _parties = new();
    private readonly List<ClaimRiskObject> _riskObjects = new();
    private readonly List<ClaimDocument> _documents = new();

    public ClaimNumber ClaimNumber { get; private set; } = null!;
    public Guid? PolicyId { get; private set; }
    public string? PolicyNumber { get; private set; }
    public string? ClientName { get; private set; }
    public ClaimStatus Status { get; private set; } = ClaimStatus.Draft;
    public ClaimSeverity? Severity { get; private set; }
    public DateTimeOffset ReportedDate { get; private set; }
    public Guid? AssignedHandlerId { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public string? ClosureReason { get; private set; }
    public string? Notes { get; private set; }
    public bool ManagerOverrideFlag { get; private set; }

    public byte[] RowVer { get; private set; } = null!;

    public IReadOnlyCollection<LossEvent> LossEvents => _lossEvents.AsReadOnly();
    public IReadOnlyCollection<ClaimParty> Parties => _parties.AsReadOnly();
    public IReadOnlyCollection<ClaimRiskObject> RiskObjects => _riskObjects.AsReadOnly();
    public IReadOnlyCollection<ClaimDocument> Documents => _documents.AsReadOnly();

    private Claim() { }

    public static Claim Create(
        Guid organisationId,
        ClaimNumber claimNumber,
        Guid? policyId,
        string? policyNumber,
        string? clientName,
        ClaimSeverity? severity,
        DateTimeOffset reportedDate)
    {
        var claim = new Claim
        {
            Id = EntityId.New(),
            OrganisationId = organisationId,
            ClaimNumber = claimNumber,
            PolicyId = policyId,
            PolicyNumber = policyNumber,
            ClientName = clientName,
            Status = ClaimStatus.Draft,
            Severity = severity,
            ReportedDate = reportedDate,
            RowVer = new byte[8]
        };
        claim.RaiseDomainEvent(new ClaimCreatedEvent(claim.Id));
        return claim;
    }

    public void TransitionStatus(ClaimStatus targetStatus, string? reason = null)
    {
        var fromStatus = Status;
        Status = targetStatus;

        if (targetStatus == ClaimStatus.Closed)
        {
            ClosedAt = DateTimeOffset.UtcNow;
            ClosureReason = reason;
        }

        if (targetStatus == ClaimStatus.Withdrawn)
            Notes = reason;

        RaiseDomainEvent(new ClaimStatusChangedEvent(Id, fromStatus, targetStatus, reason));
    }

    public void SetManagerOverrideFlag(bool value) => ManagerOverrideFlag = value;

    public void AssignHandler(Guid handlerId) => AssignedHandlerId = handlerId;

    public void LinkPolicy(Guid policyId, string policyNumber, string? clientName)
    {
        PolicyId = policyId;
        PolicyNumber = policyNumber;
        ClientName = clientName;
    }

    public void UpdateNotes(string? notes) => Notes = notes;
}
