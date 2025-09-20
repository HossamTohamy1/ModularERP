using System.Linq.Expressions;

namespace ModularERP.Shared.Interfaces
{
    public interface IGeneralRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        Task<T> GetByID(Guid id);
        Task<T> GetByIDWithTracking(Guid id);
        IQueryable<T> Get(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task Update(T entity);
        void UpdateInclude(T entity, params string[] modifiedParams);
        Task Delete(Guid id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        Task SaveChanges();
        Task AddRangeAsync(IEnumerable<T> entities); 


    }
}
