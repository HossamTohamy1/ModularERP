using FluentValidation;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.Treasuries.Handlers;
using ModularERP.Modules.Finance.Features.Treasuries.Queries;
using ModularERP.Modules.Finance.Features.Treasuries.Validators;

namespace ModularERP.Modules.Finance.Features.Treasuries.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWalletPermissions(this IServiceCollection services)
        {
            // Register handlers
            services.AddScoped<IRequestHandler<AddWalletPermissionCommand, ResponseViewModel<bool>>, AddWalletPermissionHandler>();
            services.AddScoped<IRequestHandler<UpdateWalletPermissionCommand, ResponseViewModel<bool>>, UpdateWalletPermissionHandler>();
            services.AddScoped<IRequestHandler<DeleteWalletPermissionCommand, ResponseViewModel<bool>>, DeleteWalletPermissionHandler>();
            services.AddScoped<IRequestHandler<GetWalletPermissionsQuery, ResponseViewModel<WalletPermissionDto>>, GetWalletPermissionsHandler>();
            services.AddScoped<IRequestHandler<GetUserWalletsQuery, ResponseViewModel<List<WalletPermissionDto>>>, GetUserWalletsHandler>();
            services.AddScoped<IRequestHandler<GetAllWalletsQuery, ResponseViewModel<List<WalletPermissionDto>>>, GetAllWalletsHandler>();

            // Register validators
            services.AddScoped<IValidator<AddWalletPermissionCommand>, AddWalletPermissionCommandValidator>();
            services.AddScoped<IValidator<UpdateWalletPermissionCommand>, UpdateWalletPermissionCommandValidator>();
            services.AddScoped<IValidator<DeleteWalletPermissionCommand>, DeleteWalletPermissionCommandValidator>();
            services.AddScoped<IValidator<GetWalletPermissionsQuery>, GetWalletPermissionsQueryValidator>();
            services.AddScoped<IValidator<GetUserWalletsQuery>, GetUserWalletsQueryValidator>();
            services.AddScoped<IValidator<GetAllWalletsQuery>, GetAllWalletsQueryValidator>();

            return services;
        }
    }
}
