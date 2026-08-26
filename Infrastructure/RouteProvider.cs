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
            name: PluginDefaults.SaveProductDynamicPrice,
            pattern: "dynamicprice/saveproduct",
            defaults: new { controller = "dynamicprice", action = "SaveProduct", area = AreaNames.ADMIN }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.SaveDynamicPricePatternList,
            pattern: "dynamicprice/savepattern",
            defaults: new { controller = "dynamicprice", action = "SetPatternAsDynamicallyPriced", area = AreaNames.ADMIN }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.SaveDynamicPriceConfigure,
            pattern: "dynamicprice/configure",
            defaults: new { controller = "dynamicprice", action = "SaveConfigure", area = AreaNames.ADMIN }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.Import,
            pattern: "dynamicprice/import",
            defaults: new { controller = "dynamicprice", action = "import", area = AreaNames.ADMIN }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.Export,
            pattern: "dynamicprice/export",
            defaults: new { controller = "dynamicprice", action = "export", area = AreaNames.ADMIN }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: PluginDefaults.GetDynamicPriceValues,
            pattern: "DynamicPrice/GetMetalValues",
            defaults: new { controller = "DynamicPricePublic", action = "GetMetalValues" }
        );

        endpointRouteBuilder.MapControllerRoute(
           name: PluginDefaults.CreateCartLocks,
           pattern: "DynamicPrice/CreateCartLocks",
           defaults: new { controller = "DynamicPricePublic", action = "CreateCartLocks" }
       );
    }
}
