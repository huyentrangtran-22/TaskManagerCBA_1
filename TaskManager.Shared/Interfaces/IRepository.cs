using System.Linq.Expressions;

namespace TaskManager.Shared.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>>? filter = null);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task GetBasicIdAsync(int id);
        Task GetByIdAsync(int id);
    }
}
