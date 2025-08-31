using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCGPagamentos.Infrastructure.Persistence.Mappings;

public class EventStoreMap : IEntityTypeConfiguration<EventStore>
{
    public void Configure(EntityTypeBuilder<EventStore> builder)
    {
        builder.ToTable("EventStore");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.EventId).IsRequired();
        builder.Property(e => e.Type).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Payload).IsRequired();
        builder.Property(e => e.OccurredAt).IsRequired();
        builder.Property(e => e.Version).IsRequired();
        builder.Property(e => e.AggregateId).IsRequired().HasMaxLength(50);

        // Índices para performance
        builder.HasIndex(e => e.AggregateId);
        builder.HasIndex(e => e.Version);
        builder.HasIndex(e => e.OccurredAt);
    }
}
