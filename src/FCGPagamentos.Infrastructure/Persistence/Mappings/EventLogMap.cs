using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCGPagamentos.Infrastructure.Persistence.Mappings;
public class EventLogMap : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> b)
    {
        b.ToTable("events");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasColumnName("type").IsRequired();
        b.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        b.Property(x => x.OccurredAt).HasColumnName("occurred_at").HasDefaultValueSql("now()");
        b.HasIndex(x => x.Type);
    }
}
