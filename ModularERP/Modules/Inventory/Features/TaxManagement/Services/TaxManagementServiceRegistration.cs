using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.GlAccounts.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Modules.Inventory.Features.Products.Mapping;
using ModularERP.Modules.Inventory.Features.TaxManagement.Mapping;
using ModularERP.Modules.Inventory.Features.TaxManagement.Validators;
using ModularERP.Modules.Inventory.Features.Warehouses.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.TaxManagement.Services
{
    public  static class TaxManagementServiceRegistration
    {
        public static IServiceCollection AddTaxManagementServices(this IServiceCollection services)
        {
            // Register MediatR handlers from this assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<TaxProfileMappingProfile>();
            });

            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<CreateTaxProfileValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateTaxComponentValidator>();

            return services;
        }
    }
}