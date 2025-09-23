using FluentValidation;
using ModularERP.Common.Middleware;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.Companys.Commands;
using ModularERP.Modules.Finance.Features.Companys.DTO;
using ModularERP.Modules.Finance.Features.Companys.Mapping;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Companys.Validators;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Shared.Interfaces;
using ModularERP.SharedKernel.Repository;

namespace ModularERP.Modules.Finance.Features.Companys.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCompanySerivces(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<TreasuryMappingProfile>();
                cfg.AddProfile<BankAccountMappingProfile>();
                cfg.AddProfile<ExpenseVoucherMappingProfile>();
                cfg.AddProfile<IncomeVoucherMappingProfile>();
                cfg.AddProfile<CompanyMappingProfile>();
            });

            services.AddScoped<IValidator<CreateCompanyCommand>, CreateCompanyCommandValidator>();
            services.AddScoped<IValidator<UpdateCompanyCommand>, UpdateCompanyCommandValidator>();
            services.AddScoped<IValidator<DeleteCompanyCommand>, DeleteCompanyCommandValidator>();
            services.AddScoped<IValidator<CreateCompanyDto>, CreateCompanyDtoValidator>();
            services.AddScoped<IValidator<UpdateCompanyDto>, UpdateCompanyDtoValidator>();

            // Add to Domain Services section (in the existing domain services section):
            services.AddScoped<IGeneralRepository<Company>, GeneralRepository<Company>>();
            // ✅ Return the IServiceCollection for chaining
            return services;
        }
    }
}
