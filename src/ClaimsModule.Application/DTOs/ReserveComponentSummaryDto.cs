namespace ClaimsModule.Application.DTOs;

public sealed class ReserveComponentSummaryDto
{
    public Guid Id { get; init; }
    public string ComponentType { get; init; } = string.Empty;
    /// <summary>
    /// Running sum of approved/auto-approved deltas (domain projection of event-sourced history).
    /// PendingApproval transactions do not affect this balance.
    /// </summary>
    public decimal CurrentAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool HasPendingApproval { get; init; }
    public decimal? PendingAmount { get; init; }
    public string? Notes { get; init; }
}
