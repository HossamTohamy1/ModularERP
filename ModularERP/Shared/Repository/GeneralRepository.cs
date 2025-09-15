using ModularERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.SharedKernel.Repository
{
    public class GeneralRepository<T> : IGeneralRepository<T> where T : BaseEntity
    {
        private readonly FinanceDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GeneralRepository(FinanceDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.Where(x => !x.IsDeleted);
        }

    

        public async Task<T?> GetByID(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<T?> GetByIDWithTracking(Guid id)
        {
            return await _dbSet.AsTracking().FirstOrDefaultAsync(c => c.Id == id);
        }


        public IQueryable<T> Get(Expression<Func<T, bool>> expression)
        {
            return GetAll().Where(expression);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task Update(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public void UpdateInclude(T entity, params string[] modifiedParams)
        {
            if (!_dbSet.Any(x => x.Id.Equals(entity.Id)))
                return;

            var local = _dbSet.Local.FirstOrDefault(x => x.Id.Equals(entity.Id));
            EntityEntry entityEntry = local == null
                ? _context.Entry(entity)
                : _context.ChangeTracker.Entries<T>().FirstOrDefault(x => x.Entity.Id.Equals(entity.Id));

            if (entityEntry != null)
            {
                foreach (var prop in entityEntry.Properties)
                {
                    if (modifiedParams.Contains(prop.Metadata.Name))
                    {
                        prop.CurrentValue = entity.GetType().GetProperty(prop.Metadata.Name)?.GetValue(entity);
                        prop.IsModified = true;
                    }
                }
                _context.SaveChanges();
            }
        }


        public async Task Delete(Guid id)
        {
            var entity = await GetByIDWithTracking(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }


    }
}