using FluentValidation;
using ModularERP.Modules.Inventory.Features.Products.Mapping;
using ModularERP.Modules.Inventory.Features.Requisitions.Mapping;
using ModularERP.Modules.Inventory.Features.Stocktaking.Mapping;
using ModularERP.Modules.Inventory.Features.Stocktaking.Validators.Validators_StockTaking_Header;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.Stocktaking.Services
{
    public static class StocktakingServiceRegistration
    {
        public static IServiceCollection AddStocktakingServices(this IServiceCollection services)
        {
            // Register MediatR handlers from this assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<CreateStocktakingValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateStocktakingValidator>();

            // Register AutoMapper profiles
            // Register AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<StocktakingMappingProfile>();
                cfg.AddProfile<StocktakingLineMappingProfile>();
                cfg.AddProfile<VarianceSummaryMappingProfile>();
                cfg.AddProfile<StocktakingImportExportMappingProfile>();
                cfg.AddProfile<StockSnapshotMappingProfile>();
                    

            });

            return services;
        }
    }
}