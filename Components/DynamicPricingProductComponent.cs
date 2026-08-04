using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Components;

public class DynamicPricingProductComponent(IDynamicPriceViewModelFactory viewModelFactory) : NopViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, ProductModel additionalData)
    {
        var model = await viewModelFactory.GetAdminProductViewModel(productId: additionalData.Id);

        return View("~/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Admin/views/_product.dynamic.price.cshtml", model);
    }
}