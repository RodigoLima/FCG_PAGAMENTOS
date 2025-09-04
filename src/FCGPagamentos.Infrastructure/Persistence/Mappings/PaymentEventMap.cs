using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCGPagamentos.Infrastructure.Persistence.Mappings;

public class PaymentEventMap : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> b)
    {
        b.ToTable("payment_events");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        b.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        b.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasConversion<int>()
            .IsRequired();

        b.Property(x => x.EventPayload)
            .HasColumnName("event_payload")
            .HasColumnType("jsonb")
            .IsRequired();

        b.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        // Mapeamento dos timestamps
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        // Índices
        b.HasIndex(x => x.PaymentId);
        b.HasIndex(x => x.EventType);
        b.HasIndex(x => x.OccurredAt);

        // Relacionamento com Payment
        b.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
