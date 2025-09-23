using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Services;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.InfrastructureMaster.Data
{
    public class TenantDbContextFactory : ITenantDbContextFactory
    {
        private readonly ITenantService _tenantService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TenantDbContextFactory> _logger;

        public TenantDbContextFactory(
            ITenantService tenantService,
            IServiceProvider serviceProvider,
            ILogger<TenantDbContextFactory> logger)
        {
            _tenantService = tenantService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<FinanceDbContext> CreateDbContextAsync()
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new InvalidOperationException("No tenant context available");
            }

            return await CreateDbContextAsync(tenantId);
        }

        public async Task<FinanceDbContext> CreateDbContextAsync(string tenantId)
        {
            try
            {
                var connectionString = await _tenantService.GetConnectionStringAsync(tenantId);
                var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                var context = new FinanceDbContext(optionsBuilder.Options);

                // تأكد من وجود Company record
                await EnsureCompanyExistsAsync(context, tenantId);

                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create DbContext for tenant {TenantId}", tenantId);
                throw;
            }
        }

        private async Task EnsureCompanyExistsAsync(FinanceDbContext context, string tenantId)
        {
            if (Guid.TryParse(tenantId, out var companyId))
            {
                var companyExists = await context.Companies.AnyAsync(c => c.Id == companyId);
                if (!companyExists)
                {
                    var masterCompany = await _tenantService.GetTenantAsync(tenantId);
                    if (masterCompany != null)
                    {
                        var tenantCompany = new Company
                        {
                            Id = masterCompany.Id,
                            Name = masterCompany.Name,
                            CurrencyCode = masterCompany.CurrencyCode,
                            TenantId = masterCompany.Id, // نفس الـ ID
                            CreatedAt = masterCompany.CreatedAt
                        };

                        context.Companies.Add(tenantCompany);
                        await context.SaveChangesAsync();
                        _logger.LogInformation("Created company record for tenant {TenantId}", tenantId);
                    }
                }
            }
        }
    }
}
