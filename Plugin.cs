using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Components;
using Nop.Services.Cms;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace i7MEDIA.Plugin.Misc.Dyanmic.Pricing;

public class Plugin() : BasePlugin, IWidgetPlugin
{
    public bool HideInWidgetList => throw new NotImplementedException();

    public override Task InstallAsync()
    {
        return base.InstallAsync();
    }

    public override Task UninstallAsync()
    {
        return base.UninstallAsync();
    }

    public override string GetConfigurationPageUrl()
    {
        return base.GetConfigurationPageUrl();
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> {
            AdminWidgetZones.ProductDetailsBlock
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
        {
            return typeof(DynamicPricingProductComponent);
        }

        return typeof(DynamicPricingProductComponent);
    }
}
