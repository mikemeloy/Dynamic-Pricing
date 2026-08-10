using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public int Priority => 0;

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.SaveDynamicPrice,
            pattern: "dynamicprice/save",
            defaults: new { controller = "dynamicprice", action = "save", area = AreaNames.ADMIN }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.SaveDynamicPriceConfigure,
            pattern: "dynamicprice/configure",
            defaults: new { controller = "dynamicprice", action = "SaveConfigure", area = AreaNames.ADMIN }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.GetDynamicPriceValues,
            pattern: "DynamicPrice/GetMetalValues",
            defaults: new { controller = "DynamicPricePublic", action = "GetMetalValues" }
        );
    }
}
