using FCGPagamentos.Application.IRepository.Base;
using FCGPagamentos.Domain.Entites;

namespace FCGPagamentos.Application.Repository;
public interface IPaymentRepository : 
  IRepositoryGetById<Payment>,
  IRepositoryCreate<Payment>,
  IRepositoryUpdate<Payment>
{
}