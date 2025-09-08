using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManager.Shared.Data;
using TaskManager.Shared.Interfaces;

namespace TaskManager.Shared.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<T> _set;

        public GenericRepository(AppDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _set.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public Task GetBasicIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            // try to find by PK named Id (common pattern). If entity has different PK, override in derived repo.
            return await _set.FindAsync(id) as T;
        }

        public Task GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> q = _set;
            if (filter != null) q = q.Where(filter);
            return await q.ToListAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _set.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}
