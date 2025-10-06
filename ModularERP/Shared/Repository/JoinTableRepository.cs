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

        //private void SetTenantId(T entity)
        //{
        //    if (string.IsNullOrEmpty(_tenantId))
        //    {
        //        return;
        //    }

        //    var entityType = typeof(T);
        //    var tenantIdProperty = entityType.GetProperty("TenantId");

        //    if (tenantIdProperty != null && Guid.TryParse(_tenantId, out var tenantId))
        //    {
        //        var currentTenantId = (Guid?)tenantIdProperty.GetValue(entity);
        //        if (currentTenantId == Guid.Empty || currentTenantId == null)
        //        {
        //            tenantIdProperty.SetValue(entity, tenantId);
        //        }
        //    }
        //}

        public async Task AddAsync(T entity)
        {
            //SetTenantId(entity);
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
  
            await _dbSet.AddRangeAsync(entities);
        }

        public IQueryable<T> GetAll()
        {
            var query = _dbSet.AsQueryable();

            // ✅ إضافة فلتر soft delete لو موجود
            var entityType = typeof(T);
            var isDeletedProperty = entityType.GetProperty("IsDeleted");

            if (isDeletedProperty != null)
            {
                query = query.Where(e => !EF.Property<bool>(e, "IsDeleted"));
            }

            return query;
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> expression)
        {
            return GetAll().Where(expression);
        }

        public async Task<T?> FindAsync(params object[] keyValues)
        {
            var entity = await _dbSet.FindAsync(keyValues);

            // ✅ التحقق من soft delete
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

        // ✅ Soft Delete (إذا كان الـ entity فيه IsDeleted)
        public async Task Delete(T entity)
        {
            var entityType = typeof(T);
            var isDeletedProperty = entityType.GetProperty("IsDeleted");

            if (isDeletedProperty != null)
            {
                // Soft delete
                isDeletedProperty.SetValue(entity, true);
                _context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                // Hard delete
                _context.Entry(entity).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteRange(IEnumerable<T> entities)
        {
            var entityType = typeof(T);
            var isDeletedProperty = entityType.GetProperty("IsDeleted");

            if (isDeletedProperty != null)
            {
                // Soft delete
                foreach (var entity in entities)
                {
                    isDeletedProperty.SetValue(entity, true);
                    _context.Entry(entity).State = EntityState.Modified;
                }
            }
            else
            {
                // Hard delete
                _dbSet.RemoveRange(entities);
            }

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}