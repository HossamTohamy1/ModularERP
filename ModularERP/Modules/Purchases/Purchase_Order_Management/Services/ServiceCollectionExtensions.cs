using FluentValidation;
using MediatR;
using ModularERP.Common.Behaviors;
using ModularERP.Modules.Inventory.Features.ProductSettings.Mapping;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Purchases.Purchase_Order_Management.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPurchaseOrderModule(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper
            services.AddAutoMapper(cfg =>
            {

                cfg.AddProfile<PurchaseOrderMappingProfile>();


            });
            // Register FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Register MediatR Pipeline Behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }

}
