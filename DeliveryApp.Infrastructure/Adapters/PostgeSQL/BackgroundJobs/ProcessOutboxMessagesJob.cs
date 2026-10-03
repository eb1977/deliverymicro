using Ddd;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;
using JsonNet.ContractResolvers;


namespace DeliveryApp.Infrastructure.Adapters.PostgeSQL.BackgroundJobs;

public class ProcessOutboxMessagesJob : IJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var outboxMessages = await _dbContext
            .OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(o => o.OccurredOnUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);
        if (!outboxMessages.Any())
            return;

        foreach (var outboxMessage in outboxMessages)
        {
            // Настройки сериализатора
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver(),
                TypeNameHandling = TypeNameHandling.All
            };

            // Десериализуем запись из OutboxMessages в DomainEvent
            var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(outboxMessage.Payload, settings);

            // Отправляем
            await _mediator.Publish(domainEvent, context.CancellationToken);

            // Если предыдущий метод не вернул ошибку, значит отправка была успешной
            // Ставим дату отправки, это будет признаком, что сообщение отправлять больше не нужно 
            outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
        }

        // Сохраняем изменения
        await _dbContext.SaveChangesAsync();
    }
}
