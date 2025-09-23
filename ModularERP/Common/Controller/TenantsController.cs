using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularERP.Common.Services;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;

namespace ModularERP.Common.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantsController : ControllerBase
    {
        private readonly IMasterDbService _masterDbService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantsController> _logger;

        public TenantsController(
            IMasterDbService masterDbService,
            ITenantService tenantService,
            ILogger<TenantsController> logger)
        {
            _masterDbService = masterDbService;
            _tenantService = tenantService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            try
            {
                var company = await _masterDbService.CreateCompanyAsync(request.Name, request.CurrencyCode);

                return Ok(new
                {
                    TenantId = company.Id,
                    Name = company.Name,
                    DatabaseName = company.DatabaseName,
                    Status = company.Status.ToString(),
                    CreatedAt = company.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tenant: {TenantName}", request.Name);
                return BadRequest(new { Error = "Failed to create tenant", Details = ex.Message });
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentTenant()
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { Error = "No tenant context available" });
            }

            var tenant = await _tenantService.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                return NotFound(new { Error = "Tenant not found" });
            }

            return Ok(new
            {
                TenantId = tenant.Id,
                Name = tenant.Name,
                CurrencyCode = tenant.CurrencyCode,
                Status = tenant.Status.ToString(),
                CreatedAt = tenant.CreatedAt
            });
        }

        [HttpGet("validate/{tenantId}")]
        public async Task<IActionResult> ValidateTenant(string tenantId)
        {
            var isValid = await _tenantService.ValidateTenantAsync(tenantId);
            return Ok(new { TenantId = tenantId, IsValid = isValid });
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest(new { Error = "No tenant context available" });
            }

            try
            {
                var connectionString = await _tenantService.GetConnectionStringAsync(tenantId);

                var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new FinanceDbContext(optionsBuilder.Options);
                var canConnect = await context.Database.CanConnectAsync();

                return Ok(new
                {
                    TenantId = tenantId,
                    CanConnect = canConnect,
                    DatabaseName = connectionString.Split(';')
                        .FirstOrDefault(x => x.Contains("Initial Catalog"))
                        ?.Split('=')[1]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test connection for tenant {TenantId}", tenantId);
                return BadRequest(new { Error = "Connection test failed", Details = ex.Message });
            }
        }
    }

    public class CreateTenantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = "EGP";
    }
}
