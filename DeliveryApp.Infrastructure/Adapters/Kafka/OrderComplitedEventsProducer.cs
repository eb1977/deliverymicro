using Confluent.Kafka;
using DeliveryApp.Core;
using DeliveryApp.Core.Domain.Model.OrderAggegate.DomainEvents;
using DeliveryApp.Core.Ports;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Queues.Basket.Events;
using Queues.Order.Events;
using System.Text;

namespace DeliveryApp.Infrastructure.Adapters.Kafka;

public class OrderComplitedEventsProducer : IOrderComplitedEventsProducer
{
    private readonly ProducerConfig _config;
    private readonly string _topicName;

    public OrderComplitedEventsProducer(IOptions<Settings> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.MessageBrokerHost))
            throw new ArgumentException(nameof(options.Value.MessageBrokerHost));
        if (string.IsNullOrWhiteSpace(options.Value.OrderEventsTopic))
            throw new ArgumentException(nameof(options.Value.OrderEventsTopic));

        _config = new ProducerConfig
        {
            BootstrapServers = options.Value.MessageBrokerHost
        };

        _topicName = options.Value.OrderEventsTopic;
    }

    public async Task Publish(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Создаем Integration Event
        var integrationEvent = new OrderCompletedIntegrationEvent()
        { 
            OrderId = notification.Order.Id.ToString()
        };

        // Отправляем Integration Event
        await Produce(
            key: notification.EventId.ToString(),
            integrationEvent,
            notification,
            cancellationToken);

    }

    private async Task Produce<TIntegrationEvent, TDomainEvent>(
        string key,
        TIntegrationEvent integrationEvent,
        TDomainEvent domainEvent,
        CancellationToken cancellationToken)
        where TIntegrationEvent : IMessage
    {
        var message = new Message<string, byte[]>
        {
            Key = key,
            Value = integrationEvent.ToByteArray(),
            Headers = new Headers
            {
                { "event-id", Encoding.UTF8.GetBytes(key) },
                { "event-type", Encoding.UTF8.GetBytes(domainEvent!.GetType().Name) },
                { "occurred-at", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) },
                { "content-type", "application/x-protobuf"u8.ToArray() },
                { "debug-json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainEvent)) }
            }
        };

        using var producer = new ProducerBuilder<string, byte[]>(_config).Build();
        await producer.ProduceAsync(_topicName, message, cancellationToken);
    }
}
