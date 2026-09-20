using Ddd;

namespace DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;

public class OrderCompletedDomainEvent : DomainEvent
{
    public OrderCompletedDomainEvent(Guid id, OrderStatus status)
    {
        Id = id;
        Status = status;
    }

    public Guid Id { get; }

    public OrderStatus Status { get; }
}
