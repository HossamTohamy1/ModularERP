using FluentValidation;
using MediatR;
using ModularERP.Common.ViewModel;
using ModularERP.Modules.Finance.Features.Taxs.Commands;
using ModularERP.Modules.Finance.Features.Taxs.DTO;
using ModularERP.Modules.Finance.Features.Taxs.Handlers;
using ModularERP.Modules.Finance.Features.Taxs.Queries;
using ModularERP.Modules.Finance.Features.Taxs.Validators;

namespace ModularERP.Modules.Finance.Features.Taxs.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTaxServices(this IServiceCollection services)
        {
            // Register Command Handlers
            services.AddScoped<IRequestHandler<CreateTaxCommand, ResponseViewModel<TaxResponseDto>>, CreateTaxCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateTaxCommand, ResponseViewModel<TaxResponseDto>>, UpdateTaxCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteTaxCommand, ResponseViewModel<bool>>, DeleteTaxCommandHandler>();
            services.AddScoped<IRequestHandler<ToggleTaxStatusCommand, ResponseViewModel<bool>>, ToggleTaxStatusCommandHandler>();

            // Register Query Handlers
            services.AddScoped<IRequestHandler<GetTaxByIdQuery, ResponseViewModel<TaxResponseDto>>, GetTaxByIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetAllTaxesQuery, ResponseViewModel<List<TaxListDto>>>, GetAllTaxesQueryHandler>();
            services.AddScoped<IRequestHandler<GetActiveTaxesQuery, ResponseViewModel<List<TaxListDto>>>, GetActiveTaxesQueryHandler>();
            services.AddScoped<IRequestHandler<SearchTaxesQuery, ResponseViewModel<List<TaxListDto>>>, SearchTaxesQueryHandler>();

            // Register Command Validators
            services.AddScoped<IValidator<CreateTaxCommand>, CreateTaxCommandValidator>();
            services.AddScoped<IValidator<UpdateTaxCommand>, UpdateTaxCommandValidator>();
            services.AddScoped<IValidator<DeleteTaxCommand>, DeleteTaxCommandValidator>();
            services.AddScoped<IValidator<ToggleTaxStatusCommand>, ToggleTaxStatusCommandValidator>();

            // Register Query Validators
            services.AddScoped<IValidator<GetTaxByIdQuery>, GetTaxByIdQueryValidator>();
            services.AddScoped<IValidator<GetAllTaxesQuery>, GetAllTaxesQueryValidator>();
            services.AddScoped<IValidator<GetActiveTaxesQuery>, GetActiveTaxesQueryValidator>();
            services.AddScoped<IValidator<SearchTaxesQuery>, SearchTaxesQueryValidator>();

            // Register DTO Validators
            services.AddScoped<IValidator<CreateTaxDto>, CreateTaxDtoValidator>();
            services.AddScoped<IValidator<UpdateTaxDto>, UpdateTaxDtoValidator>();

            return services;
        }
    }
}
