using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Nop.Services.ScheduleTasks;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.ScheduledTasks;

public class UpdatePreciousMetalPrices(IDynamicPriceTierPriceService dynamicPriceTierPriceService, IDynamicPriceService dynamicPriceService, IMetalsService metalsService) : IScheduleTask
{
    public async Task ExecuteAsync()
    {
        var dicMetalValues = await metalsService.GetCurrentMetalPricesAsync();

        if (dicMetalValues.IsNull())
        {
            return;
        }

        await dynamicPriceService.UpdateMetalPrices(dicMetalValues);
        await dynamicPriceService.UpdateProductPricesByMetalType();
        await dynamicPriceTierPriceService.DynamicPriceRoleCleanupAsync();
    }
}