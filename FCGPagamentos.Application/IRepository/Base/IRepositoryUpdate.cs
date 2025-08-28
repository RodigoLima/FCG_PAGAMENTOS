namespace FCGPagamentos.Application.IRepository.Base;
public interface IRepositoryUpdate<T>
{
  Task<bool> UpdateAsync(T t);
}
