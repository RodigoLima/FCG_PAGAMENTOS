using FCGPagamentos.Application.Domain.Payments.Messages;
using FCGPagamentos.Application.IRepository.Base;
using FCGPagamentos.Application.Payments.DTOs.Requests;
using FCGPagamentos.Application.Payments.DTOs.Responses;
using FCGPagamentos.Domain.Entites;

namespace FCGPagamentos.Application.UseCases.Payments;
public class PaymentsUseCase : IPaymentsUseCase
{
  private readonly IRepositoryBase<Payment> _paymentRepository;

  public PaymentsUseCase(IRepositoryBase<Payment> paymentRepository)
  {
    _paymentRepository = paymentRepository;
  }
  public async Task<PaymentResponse> CreateAsync(PaymentCreateRequest payment)
  {
    var newPayment = new Payment
    {
      Amount = payment.Amount,
      UserId = payment.UserId,
      GameId = payment.GameId,
    };

    var createdGame = await _paymentRepository.CreateAsync(newPayment);

    return new PaymentResponse
    {
      Id = createdGame.Id,
      Amount = createdGame.Amount,
      MessageStatus = createdGame.MessageStatus,
      GameId = createdGame.GameId,
      UserId = createdGame.UserId,
      Status = createdGame.Status
    };
  }

  public async Task<Payment?> GetByIdAsync(Guid Id)
  {
    var game = await _paymentRepository.GetByIdAsync(Id);
    if (game == null)
      throw new Exception(PaymentMessages.General.NotFound);
      //throw new NotFoundException(GameMessages.General.NotFound);

    return game;
  }
  public async Task<bool> UpdateAsync(Guid Id, PaymentUpdateRequest payment)
  {
    var existingGame = await GetByIdAsync(Id);
    if (existingGame == null) return false;

    existingGame!.Amount = payment.Amount;
    existingGame!.Status = payment.Status ?? existingGame.Status;
    existingGame!.MessageStatus = payment.MessageStatus ?? existingGame.MessageStatus;

    return await _paymentRepository.UpdateAsync(existingGame);
  }
}
