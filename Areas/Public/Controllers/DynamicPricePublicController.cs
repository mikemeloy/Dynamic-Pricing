using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Nop.Web.Framework.Controllers;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Areas.Public.Controllers;

public class DynamicPricePublicController(IDynamicPriceService dynamicPriceService) : BasePluginController
{
    //[HttpGet("DynamicPrice/GetMetalValues")]
    public async Task<IEnumerable<DynamicPricingMetalType>> GetMetalValuesAsync()
    {
        //this needs to return a list of current locks, so they can be displayed to the customer on the UI?
        return await dynamicPriceService.GetMetalTypesAsync();
    }
}