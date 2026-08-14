using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Components;

public class DynamicPricingPatternComponent(IDynamicPriceViewModelFactory viewModelFactory) : NopViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, BaseNopEntityModel additionalData)
    {
        var model = await viewModelFactory.GetAdminProductViewModel(productId: additionalData.Id);
        return View("~/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Admin/views/_pattern.dynamic.price.cshtml", model);
    }
}
