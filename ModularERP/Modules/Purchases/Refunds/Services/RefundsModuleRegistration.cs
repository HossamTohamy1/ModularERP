using FluentValidation;
using MediatR;
using ModularERP.Common.Behaviors;
using ModularERP.Modules.Purchases.Goods_Receipt.Mapping;
using ModularERP.Modules.Purchases.Refunds.Mapping;
using ModularERP.Modules.Purchases.Refunds.Validators.Validators__RefundItem;
using System.Reflection;

namespace ModularERP.Modules.Purchases.Refunds.Services
{
    public static class RefundsModuleRegistration
    {
        public static IServiceCollection AddRefundsModule(this IServiceCollection services)
        {
            // Register MediatR Handlers
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper Profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<RefundMappingProfile>();
                cfg.AddProfile<RefundPostingMappingProfile>();
                cfg.AddProfile<RefundPostingMappingProfile>();
            });
            // Register FluentValidation Validators
            services.AddValidatorsFromAssemblyContaining<CreateRefundCommandValidator>();

            // Add MediatR Pipeline Behavior for Validation
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}

