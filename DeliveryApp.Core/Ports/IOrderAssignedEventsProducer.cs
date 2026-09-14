using DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;

namespace DeliveryApp.Core.Ports;

public interface IOrderAssignedEventsProducer
{
    Task Publish(OrderAssignedDomainEvent notification, CancellationToken cancellationToken);
}
