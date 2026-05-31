using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Audit;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.EventHandlers;

public sealed class ClaimStatusChangedEventHandler(IAuditLogService auditLog)
    : INotificationHandler<DomainEventNotification<ClaimStatusChangedEvent>>
{
    public async Task Handle(DomainEventNotification<ClaimStatusChangedEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.Event;
        var eventType = evt.ToStatus switch
        {
            ClaimStatus.Closed   => AuditEventType.ClaimClosed,
            ClaimStatus.Reopened => AuditEventType.ClaimReopened,
            _                    => AuditEventType.StatusChanged
        };

        var newValue = eventType is AuditEventType.ClaimClosed or AuditEventType.ClaimReopened
            ? AuditJsonValues.Reason(evt.Reason)
            : AuditJsonValues.Status(evt.ToStatus.ToString());

        await auditLog.WriteAsync(
            claimId: evt.ClaimId,
            eventType: eventType,
            description: $"Status changed from {evt.FromStatus} to {evt.ToStatus}.",
            oldValue: AuditJsonValues.Status(evt.FromStatus.ToString()),
            newValue: newValue,
            ct: cancellationToken);
    }
}
