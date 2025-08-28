namespace FCGPagamentos.Application.IRepository.Base;
public interface IRepositoryGetById<T>
{
  Task<T?> GetByIdAsync(Guid id);
}
