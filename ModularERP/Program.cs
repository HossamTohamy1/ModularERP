using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularERP.Modules.Finance.Finance.Infrastructure.Data;
using Serilog;
using System.Diagnostics;
using ModularERP.Common.Extensions;
using ModularERP.Common.Middleware;
using ModularERP.Modules.Finance.Features.Treasuries.Handlers;
using ModularERP.Modules.Finance.Features.Treasuries.Mapping;
using ModularERP.Modules.Finance.Features.Treasuries.Validators;
using ModularERP.Modules.Finance.Features.Treasuries.Models;
using ModularERP.Modules.Finance.Features.Companys.Models;
using ModularERP.Modules.Finance.Features.Currencies.Models;
using ModularERP.Shared.Interfaces;
using ModularERP.SharedKernel.Repository;
using FluentValidation;
using MediatR;
using System.Reflection;
using ModularERP.Modules.Finance.Features.Treasuries.Commands;
using ModularERP.Modules.Finance.Features.Treasuries.DTO;
using ModularERP.Modules.Finance.Features.BankAccounts.Models;
using ModularERP.Modules.Finance.Features.BankAccounts.Mapping;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Service;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.DTO;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Validators;
using ModularERP.Modules.Finance.Features.ExpensesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Mapping;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Service.Interface;
using ModularERP.Modules.Finance.Features.IncomesVoucher.DTO;
using ModularERP.Modules.Finance.Features.IncomesVoucher.Validators;

namespace ModularERP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host...");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                // Add services to the container.
                builder.Services.AddControllers();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });
                builder.Services.AddDbContext<FinanceDbContext>(options =>
                {
                    options
                        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                        .EnableSensitiveDataLogging() 
                        .LogTo(Console.WriteLine,
                               new[] { DbLoggerCategory.Database.Command.Name },
                               LogLevel.Information);
                });


                //builder.Services.AddDbContext<FinanceDbContext>(options =>
                //{
                //    options
                //        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                //        //.LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                //        .EnableSensitiveDataLogging(true)
                //        .LogTo(Console.WriteLine,
                //        new[] { DbLoggerCategory.Database.Command.Name },
                //        LogLevel.Information);
                //    //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                //});

                builder.Services.AddCommonServices();

                builder.Services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(CreateTreasuryHandler).Assembly);
                });

                builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<TreasuryMappingProfile>();
                });
                builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<BankAccountMappingProfile>();
                });
                                builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<ExpenseVoucherMappingProfile>();
                });
                                        builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<IncomeVoucherMappingProfile>();
                });




                builder.Services.AddScoped<IValidator<CreateTreasuryCommand>, CreateTreasuryCommandValidator>();
                builder.Services.AddScoped<IValidator<UpdateTreasuryCommand>, UpdateTreasuryCommandValidator>();
                builder.Services.AddScoped<IValidator<DeleteTreasuryCommand>, DeleteTreasuryCommandValidator>();
                builder.Services.AddScoped<IValidator<CreateTreasuryDto>, CreateTreasuryDtoValidator>();
                builder.Services.AddScoped<IValidator<UpdateTreasuryDto>, UpdateTreasuryDtoValidator>();

                builder.Services.AddScoped<IValidator<CreateIncomeVoucherDto>, CreateIncomeVoucherValidator>();

                builder.Services.AddScoped<IGeneralRepository<Treasury>, GeneralRepository<Treasury>>();
                builder.Services.AddScoped<IGeneralRepository<BankAccount>, GeneralRepository<BankAccount>>();
                builder.Services.AddScoped<IExpenseVoucherService, ExpenseVoucherService>();
                builder.Services.AddScoped<IIncomeVoucherService, IncomeVoucherService>();

                builder.Services.AddTransient<IValidator<CreateExpenseVoucherDto>, CreateExpenseVoucherValidator>();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    // Avoid schema ID conflicts by using full type name
                    c.CustomSchemaIds(type => type.FullName!.Replace("+", "."));
                });

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                // 🔥 Enable CORS middleware
                app.UseCors("AllowAll");

                app.UseAuthorization();

                // Global error handler middleware
                app.UseMiddleware<GlobalErrorHandlerMiddleware>();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}