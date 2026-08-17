using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Infrastructure.Middleware;

public class CheckoutPageMiddleware(RequestDelegate next, IDynamicPriceService dynamicPriceService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(new PathString("/checkout")))
        {
            Console.WriteLine("correct!");
            await dynamicPriceService.UpdateDynamicallyPriceCartItemsAsync();
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