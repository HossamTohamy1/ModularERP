using FluentValidation;
using MediatR;
using ModularERP.Common.Behaviors;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.GlAccounts.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Modules.Inventory.Features.Products.Mapping;
using ModularERP.Modules.Inventory.Features.Requisitions.Mapping;
using ModularERP.Modules.Inventory.Features.Warehouses.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.Products.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInventoryModule(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ItemGroupMappingProfile>();
                cfg.AddProfile<ProductMappingProfile>();
                cfg.AddProfile<RequisitionMappingProfile>();

            });

            // Register FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Register Validation Pipeline Behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
