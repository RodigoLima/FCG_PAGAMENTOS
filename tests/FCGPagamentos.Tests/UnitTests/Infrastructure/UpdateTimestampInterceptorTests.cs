using Microsoft.EntityFrameworkCore;
using FCGPagamentos.Domain.Entities;
using FCGPagamentos.Domain.ValueObjects;
using FCGPagamentos.Infrastructure.Persistence;
using FCGPagamentos.Infrastructure.Persistence.Interceptors;
using Xunit;

namespace FCGPagamentos.Tests.UnitTests.Infrastructure;

public class UpdateTimestampInterceptorTests
{
    [Fact]
    public void Should_Update_UpdatedAt_When_Entity_Is_Modified()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var interceptor = new UpdateTimestampInterceptor();
        var context = new AppDbContext(options);
        
        // Criar um pagamento
        var payment = new Payment(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            new Money(100.00m, "BRL"), 
            DateTime.UtcNow.AddHours(-1)
        );

        context.Payments.Add(payment);
        context.SaveChanges();

        // Verificar que UpdatedAt está null inicialmente
        Assert.Null(payment.UpdatedAt);

        // Act - Modificar a entidade
        payment.MarkProcessed(DateTime.UtcNow);
        context.SaveChanges();

        // Assert - UpdatedAt deve ser atualizado
        Assert.True(payment.UpdatedAt.HasValue);
        Assert.True(payment.UpdatedAt.Value > payment.CreatedAt);
    }

    [Fact]
    public void Should_Not_Update_UpdatedAt_When_Entity_Is_Added()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var interceptor = new UpdateTimestampInterceptor();
        var context = new AppDbContext(options);
        
        var payment = new Payment(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            new Money(100.00m, "BRL"), 
            DateTime.UtcNow
        );

        // Act - Adicionar nova entidade
        context.Payments.Add(payment);
        context.SaveChanges();

        // Assert - UpdatedAt deve permanecer null para novas entidades
        Assert.Null(payment.UpdatedAt);
        Assert.True(payment.CreatedAt > DateTime.MinValue);
    }
}
