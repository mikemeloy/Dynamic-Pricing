using System.Net;
using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Areas.Admin.Controllers;

public class DynamicPriceController(IDynamicPriceImportFactory importFactory, IDynamicPriceService dynamicPriceService, IDynamicPriceViewModelFactory viewModelFactory) : BasePluginController
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> ConfigureAsync()
    {
        var model = await viewModelFactory.GetAdminConfigureViewModel();

        return View("~/Plugins/i7MEDIA.Plugin.Misc.Dynamic.Pricing/Areas/Admin/Views/Configure.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public async Task<IActionResult> SaveProductAsync(DynamicPricingRequestModel requestObject)
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
    [HttpPost]
    public async Task<IActionResult> SetPatternAsDynamicallyPricedAsync(IEnumerable<int> patternIds)
    {
        if (patternIds.IsNull())
        {
            return StatusCode((int)HttpStatusCode.BadRequest);
        }

        await dynamicPriceService.SetPatternProductsAsDyanamicallyPricedAsync(patternIds);

        return StatusCode((int)HttpStatusCode.OK);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost]
    public async Task<IActionResult> SaveConfigureAsync(ConfigureRequestModel model)
    {
        if (model.IsNull())
        {
            return BadRequest("Invalid Request");
        }

        await dynamicPriceService.SaveSettingsAsync(
             conversion: model.WeightConversion,
             apiKey: model.ApiKey,
             endpoint: model.ApiEndpoint,
             cartPriceLockInSeconds: model.CartPriceLock
         );

        return Ok();
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpGet()]
    public async Task<IActionResult> ExportAsync()
    {
        using var ms = new MemoryStream();

        await importFactory.ExportProductAsync(ms);

        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost()]
    public async Task<IActionResult> ImportAsync(IFormCollection form)
    {


        if (form.IsNull() || form.Files.IsNull())
        {
            return Ok();
        }

        var file = form.Files.First();

        await importFactory.ImportProductFromXSLTDataAsync(file);


        return Ok();
    }
}