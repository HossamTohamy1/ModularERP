using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.InfrastructureMaster.Data;
using ModularERP.Common.Models;
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

                var serverConnectionString = GetServerConnectionString();

                using var serverConnection = new SqlConnection(serverConnectionString);
                await serverConnection.OpenAsync();

                var checkDbCommand = new SqlCommand($@"
            IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{GetDatabaseNameFromConnectionString(company.DatabaseName!)}')
                SELECT 1
            ELSE
                SELECT 0", serverConnection);

                var dbExists = (int)await checkDbCommand.ExecuteScalarAsync() == 1;

                if (dbExists)
                {
                    _logger.LogInformation("Database {DatabaseName} already exists, skipping creation", company.DatabaseName);
                    return true;
                }

                var createDbCommand = new SqlCommand($@"
            CREATE DATABASE [{GetDatabaseNameFromConnectionString(company.DatabaseName!)}]", serverConnection);

                await createDbCommand.ExecuteNonQueryAsync();

                await serverConnection.CloseAsync();

                var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new FinanceDbContext(optionsBuilder.Options);

                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    throw new InvalidOperationException($"Cannot connect to database {company.DatabaseName}");
                }

                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                    _logger.LogInformation("Applied {Count} migrations to database {DatabaseName}",
                        pendingMigrations.Count(), company.DatabaseName);
                }

                _logger.LogInformation("Created tenant database for company {CompanyId}: {DatabaseName}", companyId, company.DatabaseName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tenant database for company {CompanyId}", companyId);
                return false;
            }
        }
        private string GetServerConnectionString()
        {
            var template = _configuration.GetConnectionString("TenantTemplate")!;
            return template.Replace("Initial Catalog={DatabaseName};", "");
        }

        private string GetDatabaseNameFromConnectionString(string databaseName)
        {
            return databaseName.Replace("ModularERP_Tenant_", "ModularERP_Tenant_");
        }

        public async Task<bool> DropTenantDatabaseAsync(Guid companyId)
        {
            try
            {
                var company = await GetCompanyAsync(companyId);
                if (company == null) return false;

                var serverConnectionString = GetServerConnectionString();

                using var connection = new SqlConnection(serverConnectionString);
                await connection.OpenAsync();

                var dbName = GetDatabaseNameFromConnectionString(company.DatabaseName!);

                var killConnectionsCommand = new SqlCommand($@"
            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{dbName}];", connection);

                await killConnectionsCommand.ExecuteNonQueryAsync();

                _logger.LogInformation("Dropped tenant database: {DatabaseName}", company.DatabaseName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drop tenant database for company {CompanyId}", companyId);
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
