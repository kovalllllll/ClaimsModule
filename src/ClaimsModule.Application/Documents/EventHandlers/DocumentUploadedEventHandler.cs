using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Documents;
using ClaimsModule.Domain.Enums;
using MediatR;

namespace ClaimsModule.Application.Documents.EventHandlers;

public sealed class DocumentUploadedEventHandler(IAuditLogService auditLog)
    : INotificationHandler<DomainEventNotification<DocumentUploadedEvent>>
{
    public async Task Handle(
        DomainEventNotification<DocumentUploadedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.Event;
        await auditLog.WriteAsync(
            claimId: evt.ClaimId,
            eventType: AuditEventType.DocumentUploaded,
            description: "Document uploaded via domain event.",
            relatedEntityId: evt.ClaimDocumentId,
            relatedEntityType: nameof(ClaimDocument),
            ct: cancellationToken);
    }
}
