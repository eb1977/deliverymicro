using DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;

namespace DeliveryApp.Core.Ports;

public interface IOrderComplitedEventsProducer
{
    Task Publish(OrderCompletedDomainEvent notification, CancellationToken cancellationToken);
}
