using ModularERP.Common.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;

namespace ModularERP.Common.Services
{
    public interface ITenantService
    {
        string? GetCurrentTenantId();
        Task<bool> ValidateTenantAsync(string tenantId);
        Task<MasterCompany?> GetTenantAsync(string tenantId);
        string GetConnectionString(string tenantId);
        Task<string> GetConnectionStringAsync(string tenantId);
    }
}
