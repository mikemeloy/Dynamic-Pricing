using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Components;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;

namespace i7MEDIA.Plugin.Misc.Dyanmic.Pricing;

public class Plugin(ILocalizationService localizationService, IDynamicPriceService dynamicPriceService, IScheduleTaskService scheduleTaskService) : BasePlugin, IWidgetPlugin
{
    public bool HideInWidgetList => throw new NotImplementedException();

    public override async Task InstallAsync()
    {
        await AddOrUpdateLocaleResourceAsync();
        await InsertScheduledTaskAsync();
        await dynamicPriceService.InsertInitialSettings();
    }

    public override async Task UninstallAsync()
    {
        await DeleteScheduledTaskAsync();
    }

    public override string GetConfigurationPageUrl()
    {
        return base.GetConfigurationPageUrl();
    }

    public override async Task InstallSampleDataAsync()
    {
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "", Description = "Select to remove dynamic pricing", Name = "None" });
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "XAU", Description = "(Au), chemical element, a dense lustrous yellow precious metal of Group 11 (Ib), Period 6, of the periodic table of the elements. Gold has several qualities that have made it exceptionally valuable throughout history. It is attractive in colour and brightness, durable to the point of virtual indestructibility, highly malleable, and usually found in nature in a comparatively pure form. The history of gold is unequaled by that of any other metal because of its perceived value from earliest times.", Name = "Gold", CurrentValue = 1m });
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "XAG", Description = "(Ag), chemical element, a white lustrous metal valued for its decorative beauty and electrical conductivity. Silver is located in Group 11 (Ib) and Period 5 of the periodic table, between copper (Period 4) and gold (Period 6), and its physical and chemical properties are intermediate between those two metals.", Name = "Silver", CurrentValue = 1m });
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> {
            AdminWidgetZones.ProductDetailsBlock
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
        {
            return typeof(DynamicPricingProductComponent);
        }

        return typeof(DynamicPricingProductComponent);
    }

    private async Task AddOrUpdateLocaleResourceAsync()
    {
        await localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Admin.Dynamic.Price.Section.Label"] = "Dynamic Pricing",
            ["admin.dynamic.price.label.base.price"] = "Base Price",
            ["admin.dynamic.price.label.metal.type"] = "Metal Type",
            ["Admin.Dynamic.Price.Save"] = "Save",
            ["admin.dynamic.price.label.metal.weight"] = "Weight (oz t)"
        });
    }

    private async Task InsertScheduledTaskAsync()
    {
        await scheduleTaskService.InsertTaskAsync(new ScheduleTask
        {
            Name = "precious metals api",
            Seconds = 60,
            Type = PluginDefaults.ScheduledTaskName,
            Enabled = true,
            LastEnabledUtc = new(),
            StopOnError = false
        });
    }

    private async Task DeleteScheduledTaskAsync()
    {
        var scheduledTask = await scheduleTaskService.GetTaskByTypeAsync(PluginDefaults.ScheduledTaskName);

        if (scheduledTask.IsNull())
        {
            return;
        }

        await scheduleTaskService.DeleteTaskAsync(scheduledTask);
    }
}
