using FluentValidation;
using ModularERP.Modules.Purchases.Goods_Receipt.DTO.DTO_GRN;
using ModularERP.Modules.Purchases.Goods_Receipt.Mapping;
using ModularERP.Modules.Purchases.Goods_Receipt.Validators.Validators_GRN;
using ModularERP.Modules.Purchases.Purchase_Order_Management.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Purchases.Goods_Receipt.Services
{
    public static class GoodsReceiptServiceExtensions
    {
        public static IServiceCollection AddGoodsReceiptServices(this IServiceCollection services)
        {
            // Register MediatR handlers from this assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper profiles

            // Register AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<GRNMappingProfile>();
            });
            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<CreateGRNValidator>();

            // Register validators explicitly
            services.AddScoped<IValidator<CreateGRNDto>, CreateGRNValidator>();
            services.AddScoped<IValidator<UpdateGRNDto>, UpdateGRNValidator>();

            return services;
        }
    }
}