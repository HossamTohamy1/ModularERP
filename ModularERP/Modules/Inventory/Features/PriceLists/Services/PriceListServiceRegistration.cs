using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Mapping;
using ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceListItems;
using ModularERP.Modules.Inventory.Features.ProductSettings.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.PriceLists.Services
{
    public static class PriceListServiceRegistration
    {
        public static IServiceCollection AddPriceListServices(this IServiceCollection services)
        {
            // Register MediatR handlers from this assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {

                cfg.AddProfile<PriceListMappingProfile>();
                cfg.AddProfile<PriceListItemMappingProfile>();
                cfg.AddProfile<PriceListRuleMappingProfile>();

            });
            // Register FluentValidation validators
            services.AddScoped<IValidator<CreatePriceListDto>, CreatePriceListValidator>();
            services.AddScoped<IValidator<UpdatePriceListDto>, UpdatePriceListValidator>();
            services.AddScoped<IValidator<ClonePriceListDto>, ClonePriceListValidator>();
            services.AddScoped<IValidator<PriceListFilterDto>, PriceListFilterValidator>();


            // Register FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreatePriceListItemValidator>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());



            return services;
        }
    }
} 
