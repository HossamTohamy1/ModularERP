using Microsoft.EntityFrameworkCore;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.Services.Data
{
    public class TenantDbContextFactory: ITenantDbContextFactory
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

                return new FinanceDbContext(optionsBuilder.Options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create DbContext for tenant {TenantId}", tenantId);
                throw;
            }
        }
    }
}
