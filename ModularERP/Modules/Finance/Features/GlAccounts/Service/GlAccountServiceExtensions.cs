using FluentValidation;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Commands;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Handlers;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Validators;
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


            services.AddScoped<IValidator<UpdateExpenseVoucherDto>, UpdateExpenseVoucherValidator>();
            services.AddScoped<IRequestHandler<UpdateExpenseVoucherCommand, ResponseViewModel<ExpenseVoucherResponseDto>>, UpdateExpenseVoucherHandler>();
            services.AddScoped<IRequestHandler<DeleteExpenseVoucherCommand, ResponseViewModel<string>>, DeleteExpenseVoucherHandler>();

            // Register MediatR handlers (automatically registered by MediatR)
            // No need to manually register handlers as MediatR will discover them

            return services;
        }
    }
}
