using ModularERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ModularERP.SharedKernel.Repository
{
    public class GeneralRepository<T> : IGeneralRepository<T> where T : BaseEntity
    {
        private readonly FinanceDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _tenantId;

        public GeneralRepository(FinanceDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _httpContextAccessor = httpContextAccessor;
            _tenantId = GetTenantId();
        }

        private string? GetTenantId()
        {
            // استخراج TenantId من الـ HTTP Context
            var httpContext = _httpContextAccessor.HttpContext;

            // طريقة 1: من الـ Headers
            if (httpContext?.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader) == true)
            {
                return tenantHeader.FirstOrDefault();
            }

            // طريقة 2: من الـ Claims في JWT Token
            var tenantClaim = httpContext?.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim))
            {
                return tenantClaim;
            }

            // طريقة 3: من الـ Subdomain
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

            // تطبيق فلتر الـ TenantId بس، مش الـ CompanyId
            if (typeof(T).GetProperty("TenantId") != null)
            {
                if (Guid.TryParse(_tenantId, out var tenantId))
                {
                    return query.Where(e => EF.Property<Guid>(e, "TenantId") == tenantId);
                }
            }

            return query;
        }

        public async Task AddAsync(T entity)
        {
            // تعيين TenantId فقط للكيانات اللي محتاجة
            if (typeof(T).GetProperty("TenantId") != null && !string.IsNullOrEmpty(_tenantId))
            {
                if (Guid.TryParse(_tenantId, out var tenantId))
                {
                    // تأكد إن الـ TenantId مش متعين من قبل
                    var currentTenantId = (Guid?)entity.GetType().GetProperty("TenantId")?.GetValue(entity);
                    if (currentTenantId == Guid.Empty || currentTenantId == null)
                    {
                        entity.GetType().GetProperty("TenantId")?.SetValue(entity, tenantId);
                    }
                }
            }

            // 👈 هنا مش هنعمل override للـ CompanyId خالص
            // الـ CompanyId هيجي من الـ Handler أو الـ Client مباشرة

            await _dbSet.AddAsync(entity);
        }

        public IQueryable<T> GetAll()
        {
            var query = _dbSet.Where(x => !x.IsDeleted);
            return ApplyTenantFilter(query);
        }

        public async Task<T?> GetByID(Guid id)
        {
            var query = _dbSet.Where(c => c.Id == id && !c.IsDeleted);
            query = ApplyTenantFilter(query);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<T?> GetByIDWithTracking(Guid id)
        {
            var query = _dbSet.AsTracking().Where(c => c.Id == id && !c.IsDeleted);
            query = ApplyTenantFilter(query);
            return await query.FirstOrDefaultAsync();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> expression)
        {
            return GetAll().Where(expression);
        }

        // للحصول على البيانات بـ CompanyId معينة داخل نفس الـ Tenant
        public IQueryable<T> GetByCompanyId(Guid companyId)
        {
            var query = GetAll();

            // إذا الـ Entity عندها CompanyId، فلترها
            if (typeof(T).GetProperty("CompanyId") != null)
            {
                query = query.Where(e => EF.Property<Guid>(e, "CompanyId") == companyId);
            }

            return query;
        }

        public async Task Update(T entity)
        {
            // التأكد من أن الكيان ينتمي لنفس الـ Tenant (مش الـ Company)
            var existingEntity = await GetByIDWithTracking(entity.Id);
            if (existingEntity == null)
            {
                throw new UnauthorizedAccessException("Entity not found or does not belong to current tenant");
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
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

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _dbSet.Where(predicate);
            query = ApplyTenantFilter(query);
            return await query.AnyAsync();
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            // تعيين TenantId لجميع الكيانات (مش CompanyId)
            foreach (var entity in entities)
            {
                if (typeof(T).GetProperty("TenantId") != null && !string.IsNullOrEmpty(_tenantId))
                {
                    if (Guid.TryParse(_tenantId, out var tenantId))
                    {
                        var currentTenantId = (Guid?)entity.GetType().GetProperty("TenantId")?.GetValue(entity);
                        if (currentTenantId == Guid.Empty || currentTenantId == null)
                        {
                            entity.GetType().GetProperty("TenantId")?.SetValue(entity, tenantId);
                        }
                    }
                }
            }

            await _dbSet.AddRangeAsync(entities);
        }
    }
}