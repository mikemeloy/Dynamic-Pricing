using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Components;

public class DynamicPricingProductComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, ProductModel additionalData)
    {
        return View("~/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Admin/views/_product.dynamic.price.cshtml", additionalData);
    }
}