using ClaimsModule.Application.Abstractions.Persistence;
using ClaimsModule.Application.Abstractions.Services;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Domain.Reserves;
using MediatR;

namespace ClaimsModule.Application.Reserves.EventHandlers;

public sealed class ReserveApprovedEventHandler(IJobScheduler jobScheduler, IReserveRepository reserves)
    : INotificationHandler<DomainEventNotification<ReserveApprovedEvent>>
{
    public async Task Handle(
        DomainEventNotification<ReserveApprovedEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.Event;
        var history = await reserves.GetHistoryByIdOnlyAsync(evt.ReserveHistoryId, cancellationToken);
        if (history is null)
            return;

        jobScheduler.EnqueuePostGLReserveChange(
            history.Id,
            history.ClaimId,
            history.IdempotencyKey.Value);
    }
}
