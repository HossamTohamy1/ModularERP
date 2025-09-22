using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.InfrastructureMaster.Data
{
    public interface ITenantDbContextProvider
    {
        Task<FinanceDbContext> GetDbContextAsync();
        Task<T> ExecuteAsync<T>(Func<FinanceDbContext, Task<T>> operation);
    }
}

