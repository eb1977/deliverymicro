using Ddd;

namespace DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;

public sealed class OrderAssignedDomainEvent : DomainEvent
{
    public OrderAssignedDomainEvent(Guid id, OrderStatus status)
    {
        Id = id;
        Status = status;
    }

    public Guid Id { get; }

    public OrderStatus Status { get; }
    
 }
