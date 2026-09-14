using Ddd;
using MediatR;

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
        await _dbContext.SaveChangesAsync(cancellationToken);
        await PublishDomainEventsAsync();
        return true;
    }

    private async Task PublishDomainEventsAsync()
    {
        // Получаем агрегаты, у которых есть доменные события
        var domainEntities = _dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.GetDomainEvents().Any())
            .ToList();

        // Извлекаем все события
        var domainEvents = domainEntities
            .SelectMany(e => e.Entity.GetDomainEvents())
            .ToList();

        // Очищаем их после извлечения
        domainEntities.ForEach(e => e.Entity.ClearDomainEvents());

        // Публикуем через MediatR
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}