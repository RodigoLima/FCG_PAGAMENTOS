using FCGPagamentos.Application.IRepository.Base;
using FCGPagamentos.Domain.Entites;

namespace FCGPagamentos.Application.Repository;
public interface IEventRepository : 
  IRepositoryGetById<Event>,
  IRepositoryCreate<Event>
{
}