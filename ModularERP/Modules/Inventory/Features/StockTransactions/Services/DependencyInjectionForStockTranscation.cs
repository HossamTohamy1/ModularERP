using FluentValidation;
using MediatR;
using ModularERP.Common.Behaviors;
using ModularERP.Modules.Inventory.Features.Products.Mapping;
using ModularERP.Modules.Inventory.Features.StockTransactions.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Services
{
    public static class DependencyInjectionForStockTranscation

    {
        public static IServiceCollection AddInventoryModule(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<StockTransactionMappingProfile>();
            });

            // Register FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Register Validation Pipeline Behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }

}
