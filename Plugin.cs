using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Core.WidgetZones;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Components;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;

namespace i7MEDIA.Plugin.Misc.Dyanmic.Pricing;

public class Plugin(ILocalizationService localizationService, IDynamicPriceService dynamicPriceService, IScheduleTaskService scheduleTaskService, IWebHelper webHelper) : BasePlugin, IWidgetPlugin
{
    public bool HideInWidgetList => false;

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
        return $"{webHelper.GetStoreLocation()}Admin/DynamicPrice/Configure";
    }
    //Gold, Silver, Platinum, Palladium and Copper
    public override async Task InstallSampleDataAsync()
    {
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "", Name = "None", Description = "Product will not be subject to dynamic pricing" });
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "XAU", Name = "Gold", CurrentValue = 0m, Description = "(Au), chemical element, a dense lustrous yellow precious metal of Group 11 (Ib), Period 6, of the periodic table of the elements. Gold has several qualities that have made it exceptionally valuable throughout history. It is attractive in colour and brightness, durable to the point of virtual indestructibility, highly malleable, and usually found in nature in a comparatively pure form. The history of gold is unequaled by that of any other metal because of its perceived value from earliest times." });
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "XAG", Name = "Silver", CurrentValue = 0m, Description = "(Ag), chemical element, a white lustrous metal valued for its decorative beauty and electrical conductivity. Silver is located in Group 11 (Ib) and Period 5 of the periodic table, between copper (Period 4) and gold (Period 6), and its physical and chemical properties are intermediate between those two metals." });
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "XPD", Name = "Palladium", CurrentValue = 0m, Description = "(Pd), chemical element, the least dense and lowest-melting of the platinum metals of Groups 8–10 (VIIIb), Periods 5 and 6, of the periodic table, used especially as a catalyst (a substance that speeds up chemical reactions without changing their products) and in alloys" });
        await dynamicPriceService.InsertMetalTypeAsync(new() { ApiSymbol = "XCU", Name = "Copper", CurrentValue = 0m, Description = "(Cu), chemical element, a reddish, extremely ductile metal of Group 11 (Ib) of the periodic table that is an unusually good conductor of electricity and heat" });
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> {
            AdminWidgetZones.ProductDetailsBlock,
            PublicWidgetZones.BodyStartHtmlTagAfter,
            PatternWidgetZones.PatternListButtons
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == AdminWidgetZones.ProductDetailsBlock)
        {
            return typeof(DynamicPricingProductComponent);
        }

        if (widgetZone == PublicWidgetZones.BodyStartHtmlTagAfter)
        {
            return typeof(DynamicPriceBannerComponent);
        }

        if (widgetZone == PatternWidgetZones.PatternListButtons)
        {
            return typeof(DynamicPricingPatternComponent);
        }

        return typeof(DynamicPricingProductComponent);
    }

    private async Task AddOrUpdateLocaleResourceAsync()
    {
        await localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Admin.dynamic.Price.Section.Label"] = "Dynamic Pricing",
            ["admin.dynamic.price.label.base.price"] = "Base Price",
            ["admin.dynamic.price.label.metal.type"] = "Metal Type",
            ["Admin.dynamic.Price.Save"] = "Save",
            ["admin.dynamic.price.label.metal.weight"] = "Weight (oz t)",
            ["admin.dynamic.price.configure.label.weight.conversion"] = "Conversion",
            ["admin.dynamic.price.configure.api.key"] = "API key",
            ["admin.dynamic.price.configure.api.end.point"] = "End Point",
            ["admin.dynamic.price.configure.cart.lock"] = "Cart Lock",
            ["admin.dynamic.price.banner.label.timer"] = "Time Left:",
            ["Admin.Dynamic.Price.Label.Price.Modifier.Type"] = "Modifier Type",
            ["admin.dynamic.price.label.price.modifier"] = "Modifier Value",
            ["Admin.Dynamic.Price.Pattern.Button.Save"] = "Add dynamic pricing",
            ["Admin.Dynamic.Price.Pattern.Button.Warning"] = "This product is not setup for dynamic pricing, please select a metal type and add the weight of the product in oz (T) or check \"Exclude from pricing\" to remove this message",
            ["Admin.Dynamic.Price.Product.Warn"] = "This product is not setup for dynamic pricing!!",
            ["public.dynamic.price.banner.confirm"] = "Your bag prices may have changed and the total displayed below may no longer be valid, Press Ok to refresh the page",
            ["i7media.plugin.misc.patterns.products.fields.isdynamicallypriced"] = "Is dynamically priced?"
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
