using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Shared.Interfaces;
using System.Linq.Expressions;

namespace ModularERP.Shared.Repository
{
    public class JoinTableRepository<T> : IJoinTableRepository<T> where T : class
    {
        private readonly FinanceDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _tenantId;

        public JoinTableRepository(FinanceDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _httpContextAccessor = httpContextAccessor;
            _tenantId = GetTenantId();
        }

        private string? GetTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader) == true)
            {
                return tenantHeader.FirstOrDefault();
            }

            var tenantClaim = httpContext?.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim))
            {
                return tenantClaim;
            }

            var host = httpContext?.Request.Host.Host;
            if (!string.IsNullOrEmpty(host) && host.Contains('.'))
            {
                var subdomain = host.Split('.')[0];
                if (subdomain != "www" && subdomain != "api")
                {
                    return subdomain;
                }
            }

            return null;
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public IQueryable<T> GetAll()
        {
            var query = _dbSet.AsQueryable();

            var entityType = typeof(T);
            var isDeletedProperty = entityType.GetProperty("IsDeleted");

            if (isDeletedProperty != null)
            {
                query = query.Where(e => !EF.Property<bool>(e, "IsDeleted"));
            }

            return query;
        }

        public async Task<T?> GetByIDWithTracking(Guid id)
        {
            var query = _dbSet.AsTracking();

            var isDeletedProp = typeof(T).GetProperty("IsDeleted");
            var idProp = typeof(T).GetProperty("Id");

            if (idProp == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property.");

            var entities = await query.ToListAsync();

            return entities
                .Where(e =>
                {
                    var entityId = (Guid)idProp.GetValue(e)!;
                    var isDeleted = (bool?)(isDeletedProp?.GetValue(e) ?? false);
                    return entityId == id && isDeleted == false;
                })
                .FirstOrDefault();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> expression)
        {
            var query = _dbSet.AsQueryable();

            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                query = query.Where(e => !EF.Property<bool>(e, "IsDeleted"));
            }

            return query.Where(expression);
        }

        public async Task<T?> FindAsync(params object[] keyValues)
        {
            var entity = await _dbSet.FindAsync(keyValues);

            if (entity != null)
            {
                var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
                if (isDeletedProperty != null)
                {
                    var isDeleted = (bool?)isDeletedProperty.GetValue(entity);
                    if (isDeleted == true)
                    {
                        return null;
                    }
                }
            }

            return entity;
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await GetAll().Where(predicate).AnyAsync();
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}