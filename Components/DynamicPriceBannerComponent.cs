using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Components;

public class DynamicPriceBannerComponent(IDynamicPriceViewModelFactory viewModelFactory) : NopViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var model = await viewModelFactory.GetBannerViewModelAsync();
        return View("~/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Public/Views/DynamicPriceBanner.cshtml", model);
    }
}