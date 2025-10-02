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

        private IQueryable<T> ApplyTenantFilter(IQueryable<T> query)
        {
            if (string.IsNullOrEmpty(_tenantId))
            {
                throw new InvalidOperationException("Tenant ID is required but not found");
            }

            // Check if entity has TenantId property through navigation
            var entityType = typeof(T);

            // For TaxProfileComponent, we need to filter through TaxProfile or TaxComponent
            if (entityType.Name == "TaxProfileComponent")
            {
                if (Guid.TryParse(_tenantId, out var tenantId))
                {
                    // Filter through TaxProfile's TenantId
                    return query.Where(e =>
                        EF.Property<Guid>(EF.Property<object>(e, "TaxProfile"), "TenantId") == tenantId);
                }
            }

            return query;
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
            return ApplyTenantFilter(query);
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> expression)
        {
            return GetAll().Where(expression);
        }

        public async Task<T?> FindAsync(params object[] keyValues)
        {
            var entity = await _dbSet.FindAsync(keyValues);

            if (entity != null)
            {
                // Verify tenant access
                var query = _dbSet.Where(e => e.Equals(entity));
                query = ApplyTenantFilter(query);
                return await query.FirstOrDefaultAsync();
            }

            return null;
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.Where(predicate);
            query = ApplyTenantFilter(query);
            return await query.AnyAsync();
        }
    
        public async Task Delete(T entity)
        {
          _context.Entry(entity).State = EntityState.Deleted;
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
