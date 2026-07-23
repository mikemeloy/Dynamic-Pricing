using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Nop.Services.ScheduleTasks;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.ScheduledTasks;

public class UpdatePreciousMetalPrices(IDynamicPriceService dynamicPriceService, IMetalsService metalsService) : IScheduleTask
{
    public async Task ExecuteAsync()
    {

        var dicMetalValues = await metalsService.GetCurrentMetalPricesAsync();

        await dynamicPriceService.UpdateMetalPrices(dicMetalValues);
        await dynamicPriceService.UpdateProductPricesByMetalType();

    }
}