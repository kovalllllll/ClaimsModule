using ClaimsModule.Domain.Common;
using MediatR;

namespace ClaimsModule.Application.Common.Models;

public sealed class DomainEventNotification<TEvent>(TEvent @event) : INotification
    where TEvent : IDomainEvent
{
    public TEvent Event { get; } = @event;
}
