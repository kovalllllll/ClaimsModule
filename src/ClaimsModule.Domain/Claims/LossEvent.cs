using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.ValueObjects;

namespace ClaimsModule.Domain.Claims;

public sealed class LossEvent : AuditableEntity
{
    public Guid ClaimId { get; private set; }
    public DateTimeOffset LossDate { get; private set; }
    public string LossDescription { get; private set; } = string.Empty;
    public string? LossLocation { get; private set; }
    public string CauseOfLossCode { get; private set; } = string.Empty;
    public Money? EstimatedLossAmount { get; private set; }
    public DateTimeOffset ReportDate { get; private set; }
    public string? PoliceReportNumber { get; private set; }

    private LossEvent() { }

    public static LossEvent Create(
        Guid claimId,
        Guid organisationId,
        DateTimeOffset lossDate,
        string lossDescription,
        string? lossLocation,
        string causeOfLossCode,
        Money? estimatedLossAmount,
        DateTimeOffset reportDate,
        string? policeReportNumber = null)
        => new()
        {
            Id = EntityId.New(),
            ClaimId = claimId,
            OrganisationId = organisationId,
            LossDate = lossDate,
            LossDescription = lossDescription,
            LossLocation = lossLocation,
            CauseOfLossCode = causeOfLossCode,
            EstimatedLossAmount = estimatedLossAmount,
            ReportDate = reportDate,
            PoliceReportNumber = policeReportNumber
        };
}
