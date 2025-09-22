using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.Services.Data
{
    public interface ITenantDbContextFactory
    {
        Task<FinanceDbContext> CreateDbContextAsync();
        Task<FinanceDbContext> CreateDbContextAsync(string tenantId);
    }
}
