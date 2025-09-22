using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.InfrastructureMaster.Data
{
    public interface ITenantDbContextFactory
    {
        Task<FinanceDbContext> CreateDbContextAsync();
        Task<FinanceDbContext> CreateDbContextAsync(string tenantId);
    }
}
