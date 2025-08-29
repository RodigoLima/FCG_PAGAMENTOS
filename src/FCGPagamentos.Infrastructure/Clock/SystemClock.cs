using FCGPagamentos.Application.Abstractions;
namespace FCGPagamentos.Infrastructure.Clock;
public class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }
