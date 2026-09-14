using Ddd;

namespace DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;

public class OrderCompletedDomainEvent : DomainEvent
{
    public OrderCompletedDomainEvent(Order order)
    {
        Order = order;
    }

    public Order Order { get; }
}
