using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.ValueObjects;

namespace ClaimsModule.Domain.Reserves;

public sealed class ClaimReserveComponent : AuditableAggregateRoot
{
    private readonly List<ReserveHistory> _history = new();

    public Guid ClaimId { get; private set; }
    public ReserveComponentType Component { get; private set; }
    public Money CurrentAmount { get; private set; } = Money.Zero;
    public ReserveComponentStatus Status { get; private set; } = ReserveComponentStatus.Active;
    public string? Notes { get; private set; }

    public byte[] RowVer { get; private set; } = null!;

    public IReadOnlyCollection<ReserveHistory> History => _history.AsReadOnly();

    private ClaimReserveComponent() { }

    public static ClaimReserveComponent Create(Guid claimId, Guid organisationId, ReserveComponentType component)
        => new()
        {
            Id = EntityId.New(),
            ClaimId = claimId,
            OrganisationId = organisationId,
            Component = component,
            CurrentAmount = Money.Zero,
            Status = ReserveComponentStatus.Active,
            RowVer = new byte[8]
        };

    public void UpdateCurrentAmount(Money amount) => CurrentAmount = amount;

    public void Approve(Guid reserveHistoryId, Money approvedDelta)
    {
        // Event-sourcing pattern: CurrentAmount is the running sum of all approved deltas.
        CurrentAmount = CurrentAmount.Add(approvedDelta);
        RaiseDomainEvent(new ReserveApprovedEvent(ClaimId, Id, reserveHistoryId));
    }

    public void Reject(Guid reserveHistoryId, string rejectionReason)
        => RaiseDomainEvent(new ReserveRejectedEvent(ClaimId, Id, reserveHistoryId, rejectionReason));
}
