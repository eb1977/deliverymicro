using DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public sealed class OrderAssignedDomainEventHandler : INotificationHandler<OrderAssignedDomainEvent>
{
    private readonly IOrderAssignedEventsProducer _producer;
    public OrderAssignedDomainEventHandler(IOrderAssignedEventsProducer orderAssignedEventsProducer)
    {
        _producer = orderAssignedEventsProducer;
    }
    public async Task Handle(OrderAssignedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _producer.Publish(notification, cancellationToken);
    }
}
