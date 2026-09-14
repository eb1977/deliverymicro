#nullable disable

using Ddd;
using DeliveryApp.Core.Application.EventHandlers.OrderAssigned;
using DeliveryApp.Core.Application.EventHandlers.OrderCompleted;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.OrderAggegate;


/// <summary>
/// Заказ на доставку
/// </summary>
public class Order : Aggregate<Guid>
{
    #region Свойства

    public Location Location { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Created;
    public Volume Volume { get; private set; }

    #endregion  

    #region Constructor

    [ExcludeFromCodeCoverage]
    public Order(Guid id, Location location, Volume volume)
    {
        Id = id;
        Location = location ?? throw new ArgumentNullException(nameof(location), "Место доставки заказа не может быть пустым");
        Volume = volume ?? throw new ArgumentNullException(nameof(volume), "Объем заказа не может быть пустым");
    }

    [ExcludeFromCodeCoverage]
    private Order()
    {
    }

    #endregion

    #region functions

    /// <summary>
    /// Сменяет статус заказа на Назначен.
    /// </summary>
    public void Assign()
    {
        if (Status != OrderStatus.Created)
        {
            throw new OrderInvalidStatusException("Статус заказа может быть изменен на 'Назначенный' только из 'Созданного'", OrderStatus.Assigned);
        }

        Status = OrderStatus.Assigned;
        RaiseDomainEvent (new OrderAssignedDomainEvent(this));
    }

    /// <summary>
    /// Сменяет статус заказа на Завершен.
    /// </summary>
    /// <exception cref="OrderInvalidStatusException"></exception>
    public void Complete()
    {
        if (Status != OrderStatus.Assigned)
        {
            throw new OrderInvalidStatusException("Статус заказа может быть изменен на 'Завершенный' только из 'Назначенный'", OrderStatus.Completed);
        }

        Status = OrderStatus.Completed;
        RaiseDomainEvent(new OrderCompletedDomainEvent(this));
    }

    #endregion

    public class OrderInvalidStatusException : Exception
    {
        public OrderInvalidStatusException(string message, OrderStatus invalideStatus) : base(message)
        {
            Invalidestatus = invalideStatus;
        }

        public OrderStatus Invalidestatus { get; }
    }
}
