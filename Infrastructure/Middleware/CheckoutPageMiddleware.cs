using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Nop.Services.Logging;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Infrastructure.Middleware;

public class CheckoutPageMiddleware(ILogger logger, RequestDelegate next, IDynamicPriceService dynamicPriceService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (context.Request.Path.StartsWithSegments(new PathString("/checkout")) || context.Request.Path.StartsWithSegments(new PathString("/simplecheckout")))
            {
                await dynamicPriceService.UpdateDynamicallyPriceCartItemsAsync();
            }
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(InvokeAsync), ex);
        }


        await next(context);
    }
}

public static class DynamicPriceMiddlewareExtensions
{
    public static IApplicationBuilder UseCheckoutPageMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CheckoutPageMiddleware>();
    }
}