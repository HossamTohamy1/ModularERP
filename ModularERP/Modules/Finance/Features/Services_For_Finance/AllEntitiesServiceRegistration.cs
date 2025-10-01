using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.Companys.Handlers;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.GlAccounts.Handlers;
using ModularERP.Modules.Finance.Features.GlAccounts.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.Taxs.Handlers;
using ModularERP.Modules.Finance.Features.Taxs.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Handlers;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Modules.Inventory.Features.Products.Mapping;

using ModularERP.Modules.Inventory.Features.Warehouses.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Finance.Features.Services_For_Finance
{
    public static class CategoryServiceRegistration
    {
        public static IServiceCollection AddAllEntitiesSettingsServices(this IServiceCollection services)
        {
            // Register MediatR for this assembly
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateTreasuryHandler).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(CreateCompanyHandler).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(CreateGlAccountHandler).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(CreateTaxCommandHandler).Assembly);
            });

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<TreasuryMappingProfile>();
                cfg.AddProfile<BankAccountMappingProfile>();
                cfg.AddProfile<ExpenseVoucherMappingProfile>();
                cfg.AddProfile<IncomeVoucherMappingProfile>();
                cfg.AddProfile<GlAccountMappingProfile>();
                cfg.AddProfile<TaxProfile>();
                cfg.AddProfile<WarehouseProfile>();
                cfg.AddProfile<ProductMappingProfile>();
            });

            // Register HttpContextAccessor for file uploads
            services.AddHttpContextAccessor();

            return services;
        }
    }
}