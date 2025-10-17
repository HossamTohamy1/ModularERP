using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ModularERP.Common.Behaviors;
using ModularERP.Modules.Inventory.Features.Products.Models;
using ModularERP.Modules.Inventory.Features.StockTransactions.Mapping;
using ModularERP.Modules.Inventory.Features.StockTransactions.Models;
using ModularERP.Modules.Inventory.Features.Warehouses.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.SharedKernel.Repository;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.StockTransactions.Services
{
    public static class DependencyInjectionForStockTranscation
    {
        public static IServiceCollection AddInventoryModule(this IServiceCollection services)
        {
            // Get the assembly that contains StockTransaction types
            var assembly = typeof(DependencyInjectionForStockTranscation).Assembly;

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper with explicit profile type
            services.AddScoped<IGeneralRepository<StockTransaction>, GeneralRepository<StockTransaction>>();
            services.AddScoped<IGeneralRepository<Product>, GeneralRepository<Product>>();
            services.AddScoped<IGeneralRepository<Warehouse>, GeneralRepository<Warehouse>>();
            services.AddScoped<IGeneralRepository<WarehouseStock>, GeneralRepository<WarehouseStock>>();

            // ✅ Register NEW repositories for Activity Logging and Stats
            services.AddScoped<IGeneralRepository<ProductStats>, GeneralRepository<ProductStats>>();
            services.AddScoped<IGeneralRepository<ActivityLog>, GeneralRepository<ActivityLog>>();
            services.AddScoped<IGeneralRepository<ProductTimeline>, GeneralRepository<ProductTimeline>>();

            // Register FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Register Validation Pipeline Behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}