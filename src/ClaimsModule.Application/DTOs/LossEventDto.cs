namespace ClaimsModule.Application.DTOs;

public sealed class LossEventDto
{
    public Guid Id { get; init; }
    public DateTimeOffset LossDate { get; init; }
    public string LossDescription { get; init; } = string.Empty;
    public string? LossLocation { get; init; }
    public string CauseOfLossCode { get; init; } = string.Empty;
    public decimal? EstimatedLossAmount { get; init; }
    public DateTimeOffset ReportDate { get; init; }
    public string? PoliceReportNumber { get; init; }
}
