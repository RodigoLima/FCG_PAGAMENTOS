using FCGPagamentos.Application.Payments.DTOs.Requests;
using FCGPagamentos.Application.Payments.DTOs.Responses;
using FCGPagamentos.Domain.Entites;

namespace FCGPagamentos.Application.UseCases.Events;
public interface IEventUseCase
{
  Task<Event?> GetByIdAsync(long Id);
  Task<EventResponse> CreateAsync(EventCreateRequest payment);
}
