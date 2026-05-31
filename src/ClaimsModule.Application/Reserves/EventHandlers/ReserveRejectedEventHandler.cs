using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Enums;
using ClaimsModule.Domain.Reserves;
using MediatR;

namespace ClaimsModule.Application.Reserves.EventHandlers;

public sealed class ReserveRejectedEventHandler(IAuditLogService auditLog)
    : INotificationHandler<DomainEventNotification<ReserveRejectedEvent>>
{
    public async Task Handle(DomainEventNotification<ReserveRejectedEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.Event;
        await auditLog.WriteAsync(
            claimId: evt.ClaimId,
            eventType: AuditEventType.ReserveRejected,
            description: $"Reserve rejected. Reason: {evt.RejectionReason}",
            oldValue: AuditJsonValues.RejectionReason(evt.RejectionReason),
            relatedEntityId: evt.ReserveHistoryId,
            relatedEntityType: "ReserveHistory",
            ct: cancellationToken);
    }
}
