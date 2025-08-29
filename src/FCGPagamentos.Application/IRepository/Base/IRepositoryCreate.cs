namespace FCGPagamentos.Application.IRepository.Base;
public interface IRepositoryCreate<T>
{
  Task<T> CreateAsync(T t);
}
