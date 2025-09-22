using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Common.Services.Data;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.Services
{
    public class MasterDbService : IMasterDbService
    {
        private readonly MasterDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MasterDbService> _logger;

        public MasterDbService(MasterDbContext context, IConfiguration configuration, ILogger<MasterDbService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<MasterCompany?> GetCompanyAsync(Guid companyId)
        {
            return await _context.MasterCompanies.FirstOrDefaultAsync(c => c.Id == companyId && c.Status == CompanyStatus.Active);
        }

        public async Task<MasterCompany?> GetCompanyByNameAsync(string companyName)
        {
            return await _context.MasterCompanies.FirstOrDefaultAsync(c => c.Name == companyName && c.Status == CompanyStatus.Active);
        }

        public async Task<bool> CompanyExistsAsync(Guid companyId)
        {
            return await _context.MasterCompanies.AnyAsync(c => c.Id == companyId && c.Status == CompanyStatus.Active);
        }

        public async Task<MasterCompany> CreateCompanyAsync(string name, string currencyCode = "EGP")
        {
            var company = new MasterCompany
            {
                Id = Guid.NewGuid(),
                Name = name,
                CurrencyCode = currencyCode,
                DatabaseName = $"ModularERP_Tenant_{Guid.NewGuid():N}",
                Status = CompanyStatus.Active
            };

            _context.MasterCompanies.Add(company);
            await _context.SaveChangesAsync();

            // إنشاء قاعدة البيانات الخاصة بالعميل
            await CreateTenantDatabaseAsync(company.Id);

            return company;
        }

        public async Task<bool> CreateTenantDatabaseAsync(Guid companyId)
        {
            try
            {
                var company = await GetCompanyAsync(companyId);
                if (company == null) return false;

                var connectionString = GetTenantConnectionString(company.DatabaseName!);

                // إنشاء قاعدة البيانات
                var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new FinanceDbContext(optionsBuilder.Options);
                await context.Database.EnsureCreatedAsync();

                // تشغيل Migrations
                await context.Database.MigrateAsync();

                _logger.LogInformation("Created tenant database for company {CompanyId}: {DatabaseName}", companyId, company.DatabaseName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tenant database for company {CompanyId}", companyId);
                return false;
            }
        }

        private string GetTenantConnectionString(string databaseName)
        {
            var template = _configuration.GetConnectionString("TenantTemplate")!;
            return template.Replace("{DatabaseName}", databaseName);
        }
    }
}
