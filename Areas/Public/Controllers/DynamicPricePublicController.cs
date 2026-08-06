using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Controllers;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Areas.Public.Controllers;

public class DynamicPricePublicController(IDynamicPriceService dynamicPriceService) : BasePluginController
{
    [HttpGet("DynamicPrice/GetMetalValues")]
    public async Task<IEnumerable<DynamicPricingMetalType>> GetMetalValuesAsync()
    {
        return await dynamicPriceService.GetMetalTypesAsync();
    }
}