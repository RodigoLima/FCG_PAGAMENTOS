using FCGPagamentos.Application.Payments.DTOs.Requests;
using FCGPagamentos.Application.Payments.DTOs.Responses;
using FCGPagamentos.Domain.Entites;

namespace FCGPagamentos.Application.UseCases.Payments;
public interface IPaymentsUseCase
{
  Task<Payment?> GetByIdAsync(Guid Id);
  Task<PaymentResponse> CreateAsync(PaymentCreateRequest payment);
  Task<bool> UpdateAsync(Guid Id, PaymentUpdateRequest payment);
}
