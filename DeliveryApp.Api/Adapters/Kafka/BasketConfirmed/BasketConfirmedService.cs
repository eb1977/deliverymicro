#nullable disable
using Confluent.Kafka;
using DeliveryApp.Core;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrderCommand;
using MediatR;
using Microsoft.Extensions.Options;
using Queues.Basket.Events;

namespace DeliveryApp.Api.Adapters.Kafka.BasketConfirmed;


public class BasketConfirmedService : BackgroundService
{
    private readonly IConsumer<Ignore, byte[]> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _topic;

    public BasketConfirmedService(IServiceScopeFactory scopeFactory, IOptions<Settings> settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Value.MessageBrokerHost))
            throw new ArgumentException(nameof(settings.Value.MessageBrokerHost));
        if (string.IsNullOrWhiteSpace(settings.Value.BasketEventsTopic))
            throw new ArgumentException(nameof(settings.Value.BasketEventsTopic));

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = settings.Value.MessageBrokerHost,
            GroupId = "DeliveryConsumerGroup",
            EnableAutoOffsetStore = false,
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _consumer = new ConsumerBuilder<Ignore, byte[]>(consumerConfig).Build();
        _topic = settings.Value.BasketEventsTopic;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF) continue;

                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                BasketConfirmedIntegrationEvent evt;
                try
                {
                   evt = BasketConfirmedIntegrationEvent.Parser.ParseFrom(consumeResult.Message.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Deserialization error: {ex.Message}");
                    continue;
                }

                var command = new CreateOrderCommand(Guid.NewGuid(),
                    evt.Address.Country, evt.Address.City, evt.Address.Street, evt.Address.House, evt.Address.Apartment, evt.Volume);
                var result = await mediator.Send(command, cancellationToken);
                if (!result.Ok)
                {
                    Console.WriteLine("Ошибка");
                    continue;
                }

                _consumer.StoreOffset(consumeResult);
            }
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
    
