namespace FCGPagamentos.Application.IRepository.Base;
public interface IRepositoryBase<T> :
  IRepositoryGetById<T>,
  IRepositoryCreate<T>,
  IRepositoryUpdate<T>
{
  //Task<IEnumerable<T>> GetAllAsync();
  //Task<bool> UpdateAsync(T t);
  //Task<bool> DeleteAsync(int id);
}
