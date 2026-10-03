namespace DeliveryApp.Infrastructure.Adapters.PostgeSQL.Entities;

/// <summary>
/// OutboxMessages
/// </summary>
public class OutboxMessage
{
    /// <summary>
    ///     Уникальный идентификатор сообщения
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Тип сообщения
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Тело сообщения (полезная информация)
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    ///     Дата создания
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    ///     Дата публикации
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }
}
