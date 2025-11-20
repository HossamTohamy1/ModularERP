using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.Mapping;
using ModularERP.Modules.Purchases.Invoicing.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Purchases.Invoicing.Services
{
    public static class ServiceInvoiceCollectionExtensions
    {
        public static IServiceCollection AddPurchaseInvoicingServices(this IServiceCollection services)
        {
            // Register MediatR handlers from this assembly
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<PurchaseInvoiceMappingProfile>();
                cfg.AddProfile<InvoiceMappingProfile>();
                cfg.AddProfile<InvoicePaymentsMappingProfile>();
            });
            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register services
            services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();

            return services;
        }
    }
}
