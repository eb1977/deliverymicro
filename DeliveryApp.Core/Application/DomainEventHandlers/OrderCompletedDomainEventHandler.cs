using DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public class OrderCompletedDomainEventHandler : INotificationHandler<OrderCompletedDomainEvent>
{
    private readonly IOrderComplitedEventsProducer _producer;
    public OrderCompletedDomainEventHandler(IOrderComplitedEventsProducer orderCompletedEventsProducer)
    {
        _producer = orderCompletedEventsProducer;
    }
    public async Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _producer.Publish(notification, cancellationToken);
    }
}
