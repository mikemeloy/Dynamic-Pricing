using System.Net;
using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Areas.Admin.Controllers;

public class DynamicPriceController(IDynamicPriceService dynamicPriceService, IViewModelFactory viewModelFactory) : BasePluginController
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public async Task<IActionResult> SaveAsync(DynamicPricingRequestModel requestObject)
    {
        if (requestObject.IsNull())
        {
            return StatusCode((int)HttpStatusCode.BadRequest);
        }

        await dynamicPriceService.SaveDynamicPricingAsync(
            dynamicPricing: requestObject.ToDynamicPriceEntity()
        );

        return StatusCode((int)HttpStatusCode.OK);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Configure()
    {
        var model = viewModelFactory.GetAdminConfigureViewModel();

        return View("~/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Admin/Views/Configure.cshtml", model);
    }
}