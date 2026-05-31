namespace ClaimsModule.Application.DTOs;

/// <summary>
/// Reserve tab payload for Claim Detail: per-component balances plus full append-only history.
/// </summary>
public sealed class ReserveDetailDto
{
    public Guid ClaimId { get; init; }
    public IReadOnlyList<ReserveComponentSummaryDto> Components { get; init; } = [];
    /// <summary>All transactions across components, newest first (timeline view).</summary>
    public IReadOnlyList<ReserveTransactionDto> Transactions { get; init; } = [];
    /// <summary>Sum of CurrentAmount across all components (approved/auto-approved only).</summary>
    public decimal TotalApprovedAmount { get; init; }
}
