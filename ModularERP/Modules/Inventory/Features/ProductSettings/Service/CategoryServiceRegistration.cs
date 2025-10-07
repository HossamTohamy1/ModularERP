using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ModularERP.Common.Behaviors;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.GlAccounts.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Modules.Inventory.Features.Products.Mapping;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Command_Custom;
using ModularERP.Modules.Inventory.Features.ProductSettings.Commends.Commends_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.DTO.DTO_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handelrs_UnitTemplates;
using ModularERP.Modules.Inventory.Features.ProductSettings.Handlers.Handler_CustomField;
using ModularERP.Modules.Inventory.Features.ProductSettings.Mapping;
using ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Barcode;
using ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Brand;
using ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_Category;
using ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_CustomField;
using ModularERP.Modules.Inventory.Features.ProductSettings.Validators.Validators_UnitTemplates;
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
                cfg.AddProfile<UnitTemplateMappingProfile>();
                cfg.AddProfile<CustomFieldMappingProfile>();
                cfg.AddProfile<BarcodeSettingsMappingProfile>();
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


            // Register Validators
            services.AddScoped<IValidator<CreateUnitTemplateDto>, CreateUnitTemplateValidator>();
            services.AddScoped<IValidator<UpdateUnitTemplateDto>, UpdateUnitTemplateValidator>();
            services.AddScoped<IValidator<CreateUnitConversionDto>, CreateUnitConversionValidator>();

            // Register Query Handlers
            services.AddScoped<GetAllUnitTemplatesHandler>();
            services.AddScoped<GetUnitTemplateByIdHandler>();

            // Register Command Handlers
            services.AddScoped<CreateUnitTemplateHandler>();
            services.AddScoped<UpdateUnitTemplateHandler>();
            services.AddScoped<DeleteUnitTemplateHandler>();
            services.AddScoped<AddUnitConversionHandler>();
            services.AddScoped<DeleteUnitConversionHandler>();

            
            // Services
            services.AddScoped<ICustomFieldService, CustomFieldService>();

            // Handlers
            services.AddScoped<CreateCustomFieldHandler>();
            services.AddScoped<UpdateCustomFieldHandler>();
            services.AddScoped<DeleteCustomFieldHandler>();
            services.AddScoped<GetAllCustomFieldsHandler>();
            services.AddScoped<GetCustomFieldByIdHandler>();
            //services.AddScoped<GetCustomFieldsByEntityHandler>();

            // Validators
            services.AddScoped<IValidator<CreateCustomFieldCommand>, CreateCustomFieldValidator>();
            services.AddScoped<IValidator<UpdateCustomFieldCommand>, UpdateCustomFieldValidator>();
            services.AddScoped<IValidator<DeleteCustomFieldCommand>, DeleteCustomFieldValidator>();

            // Register MediatR handlers
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<CreateBarcodeSettingsValidator>();

            // Add validation behavior for MediatR pipeline
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


            return services;
        }
    }
}

