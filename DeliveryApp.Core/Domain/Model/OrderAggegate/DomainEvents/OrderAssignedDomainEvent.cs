using Ddd;

namespace DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;

public sealed class OrderAssignedDomainEvent : DomainEvent
{
    public OrderAssignedDomainEvent(Order order)
    {
        Order = order;
    }

    public Order Order { get; }
}
