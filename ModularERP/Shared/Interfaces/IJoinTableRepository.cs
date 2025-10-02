using System.Linq.Expressions;

namespace ModularERP.Shared.Interfaces
{
    public interface IJoinTableRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        IQueryable<T> GetAll();

        IQueryable<T> Get(Expression<Func<T, bool>> expression);
        Task<T?> FindAsync(params object[] keyValues);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task Delete(T entity);
        Task DeleteRange(IEnumerable<T> entities);
        Task SaveChangesAsync();
    }
}
