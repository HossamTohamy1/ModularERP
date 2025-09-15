using ModularERP.Common.Middleware;
using Microsoft.Extensions.DependencyInjection;
using ModularERP.Shared.Interfaces;
using ModularERP.SharedKernel.Repository;

namespace ModularERP.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<GlobalErrorHandlerMiddleware>();
            services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));

            return services;
        }
    }
}