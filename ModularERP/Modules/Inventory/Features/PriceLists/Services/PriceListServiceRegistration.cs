using FluentValidation;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using ModularERP.Modules.Inventory.Features.PriceLists.Commends.Commends_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.DTO.DTO_PriceListItems;
using ModularERP.Modules.Inventory.Features.PriceLists.Mapping;
using ModularERP.Modules.Inventory.Features.PriceLists.Mappings;
using ModularERP.Modules.Inventory.Features.PriceLists.Models;
using ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_BulkDiscount;
using ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceCalculation;
using ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceList;
using ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceListAssignment;
using ModularERP.Modules.Inventory.Features.PriceLists.Validators.Validators_PriceListItems;
using ModularERP.Shared.Interfaces;
using ModularERP.SharedKernel.Repository;
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
                cfg.AddProfile<BulkDiscountMappingProfile>();
                cfg.AddProfile<PriceListAssignmentMappingProfile>();
                cfg.AddProfile<PriceCalculationMappingProfile>();

            });
            // Register FluentValidation validators
            services.AddScoped<IValidator<CreatePriceListDto>, CreatePriceListValidator>();
            services.AddScoped<IValidator<UpdatePriceListDto>, UpdatePriceListValidator>();
            services.AddScoped<IValidator<ClonePriceListDto>, ClonePriceListValidator>();
            services.AddScoped<IValidator<PriceListFilterDto>, PriceListFilterValidator>();
            services.AddScoped<IValidator<CreateBulkDiscountCommand>, CreateBulkDiscountCommandValidator>();
            services.AddScoped<IValidator<UpdateBulkDiscountCommand>, UpdateBulkDiscountCommandValidator>();
            services.AddScoped<IValidator<DeleteBulkDiscountCommand>, DeleteBulkDiscountCommandValidator>();
            services.AddValidatorsFromAssemblyContaining<CalculatePriceCommandValidator>();


            services.AddValidatorsFromAssemblyContaining<CreatePriceListAssignmentValidator>();
            services.AddValidatorsFromAssemblyContaining<CreatePriceListItemValidator>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddScoped<IGeneralRepository<PriceCalculationLog>>(sp =>
                new GeneralRepository<PriceCalculationLog>(
                    sp.GetRequiredService<FinanceDbContext>(),
                    sp.GetRequiredService<IHttpContextAccessor>()
                ));


            return services;
        }
    }
} 
