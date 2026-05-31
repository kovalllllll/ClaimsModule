namespace ClaimsModule.Application.DTOs;

public sealed class ClaimDetailDto
{
    // ── Claim overview ──────────────────────────────────────────────────────────
    public Guid Id { get; init; }
    public string ClaimNumber { get; init; } = string.Empty;
    public Guid? PolicyId { get; init; }
    public string? PolicyNumber { get; init; }
    public string? ClientName { get; init; }
    public string Status { get; init; } = string.Empty;

    /// <summary>Status names the claim can legally transition to from its current state.</summary>
    public IReadOnlyList<string> ValidNextStatuses { get; init; } = [];

    public string? Severity { get; init; }
    public DateTimeOffset ReportedDate { get; init; }
    public Guid? AssignedHandlerId { get; init; }
    public string? AssignedHandlerName { get; init; }
    public string? RowVer { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public string? ClosureReason { get; init; }
    public string? Notes { get; init; }
    public bool ManagerOverrideFlag { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    // ── Loss event ──────────────────────────────────────────────────────────────
    public IReadOnlyList<LossEventDto> LossEvents { get; init; } = [];

    // ── Parties ─────────────────────────────────────────────────────────────────
    public IReadOnlyList<ClaimPartyDto> Parties { get; init; } = [];

    // ── Risk objects ────────────────────────────────────────────────────────────
    public IReadOnlyList<ClaimRiskObjectDto> RiskObjects { get; init; } = [];

    // ── Reserves ────────────────────────────────────────────────────────────────
    /// <summary>Current balance per reserve component (summary level).</summary>
    public IReadOnlyList<ReserveComponentSummaryDto> ReserveComponents { get; init; } = [];

    /// <summary>All reserve history transactions across every component, newest first.</summary>
    public IReadOnlyList<ReserveTransactionDto> ReserveTransactions { get; init; } = [];

    public decimal TotalReserves { get; init; }

    // ── Documents ───────────────────────────────────────────────────────────────
    public IReadOnlyList<ClaimDocumentDto> Documents { get; init; } = [];

    // ── Audit ───────────────────────────────────────────────────────────────────
    /// <summary>Last 20 audit entries for this claim, newest first.</summary>
    public IReadOnlyList<AuditLogEntryDto> RecentAuditEntries { get; init; } = [];
}
