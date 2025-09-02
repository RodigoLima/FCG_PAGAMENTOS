using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCGPagamentos.Infrastructure.Persistence.Mappings;

public class PaymentMap : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments");
        b.HasKey(x => x.Id);

        b.Property(x => x.OrderId).IsRequired().HasMaxLength(100);
        b.Property(x => x.CorrelationId).IsRequired().HasMaxLength(100);
        b.Property(x => x.Method).HasConversion<int>().IsRequired();

        // Mapeamento do ValueObject Money
        b.OwnsOne(x => x.Value, mv =>
        {
            mv.Property(p => p.Amount)   // aqui é o builder 'mv', não Payment
              .HasColumnName("amount")
              .HasColumnType("numeric(14,2)")
              .IsRequired();

            mv.Property(p => p.Currency) // idem
              .HasColumnName("currency")
              .HasMaxLength(3)
              .IsRequired();
        });

        b.Property(x => x.Status)
            .HasConversion<int>()
            .HasDefaultValue(PaymentStatus.Pending);

        b.Property(x => x.ProcessedAt).HasColumnName("processed_at");

        // Mapeamento dos timestamps
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(x => x.OrderId);
        b.HasIndex(x => x.CorrelationId);
    }
}
