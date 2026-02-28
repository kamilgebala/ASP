namespace AppCore.Repositories;

public interface IGenericRepository<T>
{
    Task<T?> FindById(int id);
    Task<IEnumerable<T>> FindAll();
    Task<T> Add(T entity);
    Task DeleteById(int id);
    Task<T> Update(T entity);
}