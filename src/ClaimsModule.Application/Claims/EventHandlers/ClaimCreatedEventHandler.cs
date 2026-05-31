using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Claims.EventHandlers;

public sealed class ClaimCreatedEventHandler(IAuditLogService auditLog)
    : INotificationHandler<DomainEventNotification<ClaimCreatedEvent>>
{
    public async Task Handle(DomainEventNotification<ClaimCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.Event;
        await auditLog.WriteAsync(
            claimId: evt.ClaimId,
            eventType: AuditEventType.ClaimCreated,
            description: "Claim created via domain event.",
            ct: cancellationToken);
    }
}
