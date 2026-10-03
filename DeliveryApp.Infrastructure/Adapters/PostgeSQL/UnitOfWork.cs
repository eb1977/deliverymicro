using Ddd;
using DeliveryApp.Infrastructure.Adapters.PostgeSQL.Entities;
using MediatR;
using Newtonsoft.Json;

namespace DeliveryApp.Infrastructure.Adapters.PostgeSQL;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SaveDomainEventsToOutboxAsync(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return true;
    }
       

    private async Task SaveDomainEventsToOutboxAsync(CancellationToken cancellationToken)
    {
        // Собираем доменные события из всех агрегатов, отслеживаемых контекстом
        var outboxMessages = _dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .SelectMany(aggregate =>
            {
                var domainEvents = aggregate.GetDomainEvents();
                aggregate.ClearDomainEvents(); // очищаем после извлечения
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.EventId,
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Payload = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    })
            })
            .ToList();

        if (outboxMessages.Count > 0)
            await _dbContext.OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);
    }
}