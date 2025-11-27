using FluentValidation;
using ModularERP.Modules.Purchases.Invoicing.Mapping;
using ModularERP.Modules.Purchases.Invoicing.Services;
using ModularERP.Modules.Purchases.Payment.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Purchases.Payment.Services
{
    public static class ServicePaymentCollectionExtensions
    {
        public static IServiceCollection AddPaymentServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<PaymentTermMappingProfile>();
                cfg.AddProfile<PaymentMethodMappingProfile>();

            });
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();

            return services;
        }
    }
}
