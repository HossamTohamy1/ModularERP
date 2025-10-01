using FluentValidation;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.GlAccounts.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Modules.Inventory.Features.Products.Mapping;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Mapping;
using ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Category;
using ModularERP.Modules.Inventory.Features.Warehouses.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.ProductSettings.Service
{
    public static class CategoryServiceRegistration
    {
        public static IServiceCollection AddProductSettingsServices(this IServiceCollection services)
        {
            // Register MediatR for this assembly
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {

                cfg.AddProfile<CategoryMappingProfile>();
                cfg.AddProfile<BrandMappingProfile>();
            });

            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<CreateCategoryValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateCategoryValidator>();
            services.AddScoped<IValidator<CreateBrandDto>, CreateBrandDtoValidator>();
            services.AddScoped<IValidator<CreateBrandCommand>, CreateBrandCommandValidator>();
            services.AddScoped<IValidator<UpdateBrandDto>, UpdateBrandDtoValidator>();
            services.AddScoped<IValidator<UpdateBrandCommand>, UpdateBrandCommandValidator>();
            services.AddScoped<IValidator<DeleteBrandCommand>, DeleteBrandCommandValidator>();

            // Register HttpContextAccessor for file uploads
            services.AddHttpContextAccessor();

            return services;
        }
    }
}

