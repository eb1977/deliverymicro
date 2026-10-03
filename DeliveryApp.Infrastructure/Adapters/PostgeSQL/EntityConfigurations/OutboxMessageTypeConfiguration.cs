using DeliveryApp.Infrastructure.Adapters.PostgeSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.PostgeSQL.EntityConfigurations;

internal class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable("OutboxMessages");
        entityTypeBuilder.HasKey(entity => entity.Id);

        // Id
        entityTypeBuilder
            .Property(entity => entity.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .IsRequired();

        // Type
        entityTypeBuilder
            .Property(entity => entity.Type)
            .HasColumnName("type")
            .IsRequired();

        // Payload
        entityTypeBuilder
            .Property(entity => entity.Payload)
            .HasColumnName("payload")
            .IsRequired();

        // OccurredOnUtc
        entityTypeBuilder
            .Property(entity => entity.OccurredOnUtc)
            .HasColumnName("occurred_on_utc")
            .IsRequired();

        // ProcessedOnUtc
        entityTypeBuilder
            .Property(entity => entity.ProcessedOnUtc)
            .HasColumnName("processed_on_utc")
            .IsRequired(false);
    }
}