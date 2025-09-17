using FluentValidation;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.BankAccounts.Commands;
using ModularERP.Modules.Finance.Features.BankAccounts.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Handlers;
using ModularERP.Modules.Finance.Features.BankAccounts.Queries;
using ModularERP.Modules.Finance.Features.BankAccounts.Validators;

namespace ModularERP.Modules.Finance.Features.BankAccounts.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBankAccounts(this IServiceCollection services)
        {
            // Register command handlers
            services.AddScoped<IRequestHandler<CreateBankAccountCommand, ResponseViewModel<BankAccountCreatedDto>>, CreateBankAccountHandler>();
            services.AddScoped<IRequestHandler<UpdateBankAccountCommand, ResponseViewModel<bool>>, UpdateBankAccountHandler>();
            services.AddScoped<IRequestHandler<DeleteBankAccountCommand, ResponseViewModel<bool>>, DeleteBankAccountHandler>();

            // Register query handlers
            services.AddScoped<IRequestHandler<GetAllBankAccountsQuery, ResponseViewModel<IEnumerable<BankAccountListDto>>>, GetAllBankAccountsHandler>();
            services.AddScoped<IRequestHandler<GetBankAccountByIdQuery, ResponseViewModel<BankAccountDto>>, GetBankAccountByIdHandler>();
            services.AddScoped<IRequestHandler<GetBankAccountsByCompanyQuery, ResponseViewModel<IEnumerable<BankAccountListDto>>>, GetBankAccountsByCompanyHandler>();
            services.AddScoped<IRequestHandler<GetBankAccountStatisticsQuery, ResponseViewModel<BankAccountStatisticsDto>>, GetBankAccountStatisticsHandler>();

            // Register command validators
            services.AddScoped<IValidator<CreateBankAccountCommand>, CreateBankAccountCommandValidator>();
            services.AddScoped<IValidator<UpdateBankAccountCommand>, UpdateBankAccountCommandValidator>();
            services.AddScoped<IValidator<DeleteBankAccountCommand>, DeleteBankAccountCommandValidator>();

            // Register query validators
            services.AddScoped<IValidator<GetAllBankAccountsQuery>, GetAllBankAccountsQueryValidator>();
            services.AddScoped<IValidator<GetBankAccountByIdQuery>, GetBankAccountByIdQueryValidator>();
            services.AddScoped<IValidator<GetBankAccountsByCompanyQuery>, GetBankAccountsByCompanyQueryValidator>();
            services.AddScoped<IValidator<GetBankAccountStatisticsQuery>, GetBankAccountStatisticsQueryValidator>();

            // Register DTO validators for direct use if needed
            services.AddScoped<IValidator<CreateBankAccountDto>, CreateBankAccountDtoValidator>();
            services.AddScoped<IValidator<UpdateBankAccountDto>, UpdateBankAccountDtoValidator>();

            return services;
        }
    }
}
