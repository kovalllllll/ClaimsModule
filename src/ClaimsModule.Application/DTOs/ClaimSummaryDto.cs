namespace ClaimsModule.Application.DTOs;

public sealed class ClaimSummaryDto
{
    public Guid Id { get; init; }
    public string ClaimNumber { get; init; } = string.Empty;
    public string? ClientName { get; init; }
    public string? PolicyNumber { get; init; }
    public DateTimeOffset? LossDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Severity { get; init; }
    public decimal TotalReserves { get; init; }
    public DateTimeOffset ReportedDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid? AssignedHandlerId { get; init; }
    public string? AssignedHandlerName { get; init; }
    public string? CauseOfLossCode { get; init; }
    public string? CauseOfLossName { get; init; }
}
