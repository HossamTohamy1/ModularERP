using Microsoft.EntityFrameworkCore;
using Serilog;
using ModularERP.Common.Extensions;
using ModularERP.Common.Middleware;
using ModularERP.Common.Services;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Finance.Features.Treasuries.Handlers;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Validators;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Service;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Validators;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Service.Interface;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Validators;
using ModularERP.Shared.Interfaces;
using ModularERP.SharedKernel.Repository;
using FluentValidation;
using ModularERP.Common.Enum.Finance_Enum;
using ModularERP.Common.Models;
using ModularERP.Common.InfrastructureMaster.Data;
using ModularERP.Modules.Finance.Features.Companys.Handlers;
using ModularERP.Modules.Finance.Features.Companys.Services;
using ModularERP.Modules.Finance.Features.GlAccounts.Handlers;
using ModularERP.Modules.Finance.Features.GlAccounts.Mapping;
using ModularERP.Modules.Finance.Features.GlAccounts.Service;
using Microsoft.AspNetCore.Identity;
using ModularERP.Modules.Finance.Finance.Infrastructure.Seeds;

using ModularERP.Modules.Inventory.Features.Warehouses.Mapping;
using ModularERP.Modules.Inventory.Features.Products.Mapping;
using ModularERP.Modules.Inventory.Features.ProductSettings.Service;
using ModularERP.Modules.Finance.Features.Services_For_Finance;
using ModularERP.Modules.Inventory.Features.TaxManagement.Services;
using ModularERP.Modules.Inventory.Features.TaxManagement.Models;
using ModularERP.Shared.Repository;
using ModularERP.Modules.Inventory.Features.Products.Services;
using ModularERP.Modules.Inventory.Features.Services.Models;

namespace ModularERP
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host...");

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

                // ---------------------------
                // 🟢 Services Configuration
                // ---------------------------
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddMemoryCache();

                builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                       .AddEntityFrameworkStores<FinanceDbContext>()
                       .AddDefaultTokenProviders();

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.CustomSchemaIds(type => type.FullName!.Replace("+", "."));
                });

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });

                // ---------------------------
                //  MasterDbContext
                // ---------------------------
                builder.Services.AddDbContext<MasterDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterConnection")));

                builder.Services.AddScoped<IMasterDbService, MasterDbService>();
                builder.Services.AddScoped<ITenantService, TenantService>();
                builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
                builder.Services.AddScoped<ITenantDbContextProvider, TenantDbContextProvider>();

                // ---------------------------
                // FinanceDbContext (Dynamic Tenant DB)
                // ---------------------------
                builder.Services.AddTransient<FinanceDbContext>(provider =>
                {
                    var tenantService = provider.GetRequiredService<ITenantService>();
                    var tenantId = tenantService.GetCurrentTenantId();

                    var optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();

                    if (string.IsNullOrEmpty(tenantId))
                    {
                        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
                        optionsBuilder.UseSqlServer(connectionString);
                    }
                    else
                    {
                        var tenantConnectionString = tenantService.GetConnectionString(tenantId);
                        optionsBuilder.UseSqlServer(tenantConnectionString);
                    }

                    return new FinanceDbContext(optionsBuilder.Options);
                });

                // ---------------------------
                // 🟢 Repositories & Common
                // ---------------------------
                builder.Services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));
                builder.Services.AddCommonServices();
                builder.Services.AddCompanySerivces();
                builder.Services.AddGlAccountServices();
                builder.Services.AddProductSettingsServices();
                builder.Services.AddAllEntitiesSettingsServices();
                builder.Services.AddTaxManagementServices();
                builder.Services.AddInventoryModule();


                // ---------------------------
                // 🟢 Domain Services
                // ---------------------------
                builder.Services.AddScoped<IGeneralRepository<Treasury>, GeneralRepository<Treasury>>();
                builder.Services.AddScoped<IGeneralRepository<BankAccount>, GeneralRepository<BankAccount>>();
                builder.Services.AddScoped<IExpenseVoucherService, ExpenseVoucherService>();
                builder.Services.AddScoped<IIncomeVoucherService, IncomeVoucherService>();
                builder.Services.AddScoped<IJoinTableRepository<TaxProfileComponent>, JoinTableRepository<TaxProfileComponent>>();
                builder.Services.AddScoped(typeof(IJoinTableRepository<>), typeof(JoinTableRepository<>));

                // ---------------------------
                // 🟢 Build App
                // ---------------------------
                var app = builder.Build();

                // 🟢 Ensure Master DB created
                await EnsureMasterDatabaseAsync(app.Services);

                // ---------------------------
                // 🟢 Seed AspNetUsers for Tenant
                // ---------------------------
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
                    dbContext.Database.Migrate(); // تأكد من عمل migrate للـ tenant DB
                    dbContext.SeedUsers();        // عمل seed للمستخدمين
                }

                // ---------------------------
                // 🟢 Middleware
                // ---------------------------
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseCors("AllowAll");
                app.UseAuthorization();

                app.UseMiddleware<GlobalErrorHandlerMiddleware>();
                //              app.UseMiddleware<TenantResolutionMiddleware>();

                // ✅ Tenant Validation Middleware
                app.Use(async (context, next) =>
                {
                    var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                    var publicPaths = new[]
                    {
                        "/api/tenants/create",
                        "/api/tenants/validate",
                        "/swagger",
                        "/api/health"
                    };

                    if (publicPaths.Any(path => context.Request.Path.Value?.StartsWith(path, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        await next();
                        return;
                    }

                    var tenantId = tenantService.GetCurrentTenantId();

                    if (string.IsNullOrEmpty(tenantId))
                    {
                        context.Response.StatusCode = 400;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            error = "Tenant ID is required",
                            timestamp = DateTime.UtcNow,
                            path = context.Request.Path.Value
                        };

                        var json = System.Text.Json.JsonSerializer.Serialize(response);
                        await context.Response.WriteAsync(json);
                        return;
                    }

                    logger.LogInformation("Processing request for tenant: {TenantId}", tenantId);
                    context.Items["TenantId"] = tenantId;

                    await next();
                });

                app.MapControllers();
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // ---------------------------
        // 🟢 Ensure Master DB Method
        // ---------------------------
        static async Task EnsureMasterDatabaseAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                await masterContext.Database.EnsureCreatedAsync();
                await masterContext.Database.MigrateAsync();

                if (!await masterContext.MasterCompanies.AnyAsync())
                {
                    var defaultCompany = new MasterCompany
                    {
                        Id = Guid.NewGuid(),
                        Name = "Default Company",
                        CurrencyCode = "EGP",
                        DatabaseName = "ModularERP_Default",
                        Status = CompanyStatus.Active
                    };

                    masterContext.MasterCompanies.Add(defaultCompany);
                    await masterContext.SaveChangesAsync();

                    logger.LogInformation("Created default company: {CompanyId}", defaultCompany.Id);

                    var masterDbService = scope.ServiceProvider.GetRequiredService<IMasterDbService>();
                    await masterDbService.CreateTenantDatabaseAsync(defaultCompany.Id);
                }

                logger.LogInformation("Master database initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize master database");
                throw;
            }
        }
    }
}
