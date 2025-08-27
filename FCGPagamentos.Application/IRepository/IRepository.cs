
namespace FCGPagamentos.Application.Repository;
public interface IRepository<T>
{

  //Task<IEnumerable<T>> GetAllAsync();
  Task<T?> GetByIdAsync(Guid id);
  Task<T> CreateAsync(T t);
  Task<bool> UpdateAsync(T t);
  //Task<bool> UpdateAsync(T t);
  //Task<bool> DeleteAsync(int id);
}
