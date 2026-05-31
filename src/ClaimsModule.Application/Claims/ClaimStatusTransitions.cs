using ClaimsModule.Domain.Enums;

namespace ClaimsModule.Application.Claims;

/// <summary>
/// Single source of truth for the claim status state machine.
/// Used by both the transition command handler (enforcement) and the
/// detail query handler (valid-next-statuses for frontend).
/// </summary>
internal static class ClaimStatusTransitions
{
    private static readonly IReadOnlyDictionary<ClaimStatus, IReadOnlySet<ClaimStatus>> ValidNext =
        new Dictionary<ClaimStatus, IReadOnlySet<ClaimStatus>>
        {
            [ClaimStatus.Draft]              = new HashSet<ClaimStatus> { ClaimStatus.Open, ClaimStatus.Withdrawn },
            [ClaimStatus.Open]               = new HashSet<ClaimStatus> { ClaimStatus.UnderInvestigation, ClaimStatus.PendingPayment, ClaimStatus.Closed, ClaimStatus.Withdrawn },
            [ClaimStatus.UnderInvestigation] = new HashSet<ClaimStatus> { ClaimStatus.Open, ClaimStatus.PendingPayment, ClaimStatus.Closed, ClaimStatus.Withdrawn },
            [ClaimStatus.PendingPayment]     = new HashSet<ClaimStatus> { ClaimStatus.Closed },
            [ClaimStatus.Closed]             = new HashSet<ClaimStatus> { ClaimStatus.Reopened },
            [ClaimStatus.Reopened]           = new HashSet<ClaimStatus> { ClaimStatus.Open },
            [ClaimStatus.Withdrawn]          = new HashSet<ClaimStatus>()
        };

    internal static bool IsValid(ClaimStatus from, ClaimStatus to)
        => ValidNext.TryGetValue(from, out var allowed) && allowed.Contains(to);

    internal static IReadOnlyList<string> NextStatusNames(ClaimStatus current)
        => ValidNext.TryGetValue(current, out var set)
            ? set.Select(s => s.ToString()).ToList()
            : [];

    internal static string NextStatusesDisplay(ClaimStatus current)
        => ValidNext.TryGetValue(current, out var set) && set.Count > 0
            ? string.Join(", ", set.Select(s => s.ToString()))
            : "none";

    internal static IReadOnlySet<ClaimStatus>? GetAllowed(ClaimStatus current)
        => ValidNext.TryGetValue(current, out var set) ? set : null;

    internal static IReadOnlyList<(ClaimStatus Status, IReadOnlyList<string> AllowedNext)> AllStatusTransitions()
        => ValidNext
            .Select(kvp => (kvp.Key, (IReadOnlyList<string>)kvp.Value.Select(s => s.ToString()).ToList()))
            .ToList();
}
