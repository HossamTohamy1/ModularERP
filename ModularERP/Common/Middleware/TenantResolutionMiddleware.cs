using ModularERP.Common.Services;
using System.Text.Json;

namespace ModularERP.Common.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            try
            {
                var tenantId = tenantService.GetCurrentTenantId();

                _logger.LogInformation("Tenant ID resolved: {TenantId}", tenantId ?? "null");

                if (IsPublicEndpoint(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                if (string.IsNullOrEmpty(tenantId))
                {
                    await HandleTenantError(context, "Tenant ID is required", 400);
                    return;
                }

                var isValidTenant = await tenantService.ValidateTenantAsync(tenantId);
                if (!isValidTenant)
                {
                    await HandleTenantError(context, "Invalid or inactive tenant", 403);
                    return;
                }

                context.Items["TenantId"] = tenantId;

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in tenant resolution middleware");
                await HandleTenantError(context, "Internal server error", 500);
            }
        }

        private bool IsPublicEndpoint(PathString path)
        {
            var publicPaths = new[]
            {
                "/api/auth",
                "/api/health",
                "/api/ping",
                "/swagger",
                "/api/tenant/register"
            };

            return publicPaths.Any(publicPath =>
                path.Value?.StartsWith(publicPath, StringComparison.OrdinalIgnoreCase) == true);
        }

        private async Task HandleTenantError(HttpContext context, string message, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = message,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }

    public static class TenantResolutionMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantResolutionMiddleware>();
        }
    }

}
