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
                        .LogTo(message => Debug.WriteLine(message), LogLevel.Information)
                        .EnableSensitiveDataLogging(true)
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

                builder.Services.AddCommonServices();

                builder.Services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(CreateTreasuryHandler).Assembly);
                });

                builder.Services.AddAutoMapper(cfg =>
                {
                    cfg.AddProfile<TreasuryMappingProfile>();
                });

                builder.Services.AddScoped<IValidator<CreateTreasuryCommand>, CreateTreasuryCommandValidator>();
                builder.Services.AddScoped<IValidator<UpdateTreasuryCommand>, UpdateTreasuryCommandValidator>();
                builder.Services.AddScoped<IValidator<DeleteTreasuryCommand>, DeleteTreasuryCommandValidator>();
                builder.Services.AddScoped<IValidator<CreateTreasuryDto>, CreateTreasuryDtoValidator>();
                builder.Services.AddScoped<IValidator<UpdateTreasuryDto>, UpdateTreasuryDtoValidator>();

                builder.Services.AddScoped<IGeneralRepository<Treasury>, GeneralRepository<Treasury>>();


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