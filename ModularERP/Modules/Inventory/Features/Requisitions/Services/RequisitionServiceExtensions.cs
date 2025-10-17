using FluentValidation;
using ModularERP.Modules.Inventory.Features.ProductSettings.Mapping;
using ModularERP.Modules.Inventory.Features.Requisitions.Commends.Commends_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Requisitions.Mapping;
using ModularERP.Modules.Inventory.Features.Requisitions.Validators.Validators_Requisition_Workflow;
using ModularERP.Modules.Inventory.Features.Warehouses.Mapping;
using System.Reflection;

namespace ModularERP.Modules.Inventory.Features.Requisitions.Services
{
    public static class RequisitionServiceExtensions
    {
        public static IServiceCollection AddRequisitionWorkflowServices(this IServiceCollection services)
        {
            // Register MediatR handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddAutoMapper(cfg =>
             { 
                cfg.AddProfile<RequisitionItemMappingProfile>();
                cfg.AddProfile<WarehouseStockMappingProfile>();
                cfg.AddProfile<ApprovalWorkflowMappingProfile>();
            });

            // Register FluentValidation validators
            services.AddScoped<IValidator<SubmitRequisitionCommand>, SubmitRequisitionValidator>();
            services.AddScoped<IValidator<ApproveRequisitionCommand>, ApproveRequisitionValidator>();
            services.AddScoped<IValidator<RejectRequisitionCommand>, RejectRequisitionValidator>();
            services.AddScoped<IValidator<ConfirmRequisitionCommand>, ConfirmRequisitionValidator>();
            services.AddScoped<IValidator<CancelRequisitionCommand>, CancelRequisitionValidator>();
            services.AddScoped<IValidator<ReverseRequisitionCommand>, ReverseRequisitionValidator>();

            return services;
        }
    }
}