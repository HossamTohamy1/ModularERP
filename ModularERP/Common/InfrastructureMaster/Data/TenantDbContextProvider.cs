using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.InfrastructureMaster.Data
{
    public class TenantDbContextProvider : ITenantDbContextProvider
    {
        private readonly ITenantDbContextFactory _contextFactory;

        public TenantDbContextProvider(ITenantDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<FinanceDbContext> GetDbContextAsync()
        {
            return await _contextFactory.CreateDbContextAsync();
        }

        public async Task<T> ExecuteAsync<T>(Func<FinanceDbContext, Task<T>> operation)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await operation(context);
        }
    }
}
