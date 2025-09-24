using FluentValidation;
using ModularERP.Modules.Finance.Features.GlAccounts.DTO;
using ModularERP.Modules.Finance.Features.GlAccounts.Validators;

namespace ModularERP.Modules.Finance.Features.GlAccounts.Service
{
    public static class GlAccountServiceExtensions
    {
        public static IServiceCollection AddGlAccountServices(this IServiceCollection services)
        {
            // Register Validators
            services.AddScoped<IValidator<CreateGlAccountDto>, CreateGlAccountValidator>();
            services.AddScoped<IValidator<UpdateGlAccountDto>, UpdateGlAccountValidator>();

            // Register MediatR handlers (automatically registered by MediatR)
            // No need to manually register handlers as MediatR will discover them

            return services;
        }
    }
}
