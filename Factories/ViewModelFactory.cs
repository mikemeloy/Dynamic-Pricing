using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Core.Helpers;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Enums;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;
using Nop.Services.Plugins;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;

public interface IDynamicPriceViewModelFactory
{
    public Task<AdminProductViewModel> GetAdminProductViewModel(int productId);
    public Task<ConfigureViewModel> GetAdminConfigureViewModel();
    public Task<BannerViewModel> GetBannerViewModelAsync();
    public Task<AdminPatternListButtonViewModel> GetAdminPatternListButtonViewModelAsync();
}

public class ViewModelFactory(IPluginService pluginService, IDynamicPriceService dynamicPriceService) : IDynamicPriceViewModelFactory
{
    private string? _version = null;

    public async Task<AdminProductViewModel> GetAdminProductViewModel(int productId)
    {
        var metalTypes = await dynamicPriceService.GetMetalTypesAsync();
        var dynamicPriceInfo = await dynamicPriceService.GetProductDynamicPriceByProductIdAsync(productId);

        return new()
        {
            Version = await GetPluginVersionAsync(),
            BasePrice = dynamicPriceInfo.BasePrice,
            Weight = dynamicPriceInfo.Weight,
            ProductId = productId,
            Exclude = dynamicPriceInfo.Exclude,
            SelectedMetalType = dynamicPriceInfo.MetalTypeId,
            AvailableMetalTypes = metalTypes.ToSelectItemList(
                label: e => e.Name,
                value: e => e.Id.ToString()
            ),
            PriceModifier = dynamicPriceInfo.PriceModifier,
            PriceModifierType = dynamicPriceInfo.PriceModifierTypeId,
            PriceModifierTypes = EnumHelper.GetEnumSelectList<DynamicPriceModifierType>()
        };
    }

    public async Task<AdminPatternListButtonViewModel> GetAdminPatternListButtonViewModelAsync()
    {
        return new()
        {
            Version = await GetPluginVersionAsync()
        };
    }

    public async Task<ConfigureViewModel> GetAdminConfigureViewModel()
    {
        var settings = await dynamicPriceService.GetSettingsAsync<DynamicPriceSettings>();

        return new()
        {
            Version = await GetPluginVersionAsync(),
            SaveRoute = PluginDefaults.SaveDynamicPriceConfigure,
            ApiKey = settings.ApiKey,
            WeightConversion = settings.WeightConversion,
            ApiEndpoint = settings.ApiEndpoint,
            CartPriceLock = settings.CartPriceLock
        };
    }

    public async Task<BannerViewModel> GetBannerViewModelAsync()
    {
        var values = await dynamicPriceService.GetMetalTypesAsync();
        var settings = await dynamicPriceService.GetSettingsAsync<DynamicPriceSettings>();
        var scheduledTask = await dynamicPriceService.GetDynamicPriceScheduledTaskAsync();

        var gold = values.FirstOrDefault(g => g.ApiSymbol == settings.GoldSymbol);
        var silver = values.FirstOrDefault(s => s.ApiSymbol == settings.SilverSymbol);
        var platinum = values.FirstOrDefault(p => p.ApiSymbol == settings.PlatinumSymbol);

        return new()
        {
            Version = await GetPluginVersionAsync(),
            GoldPrice = gold.GetValueOrDefault(g => g.CurrentValue, 0.0m),
            GoldDelta = gold.GetValueOrDefault(g => g.CurrentValue, 0.0m) - gold.GetValueOrDefault(g => g.PreviousValue, 0.0m),
            GoldSymbol = gold.GetValueOrDefault(g => g.ApiSymbol, ""),
            SilverPrice = silver.GetValueOrDefault(s => s.CurrentValue, 0.0m),
            SilverDelta = silver.GetValueOrDefault(s => s.CurrentValue, 0.0m) - silver.GetValueOrDefault(s => s.PreviousValue, 0.0m),
            SilverSymbol = silver.GetValueOrDefault(s => s.ApiSymbol, ""),
            PlatinumPrice = platinum.GetValueOrDefault(p => p.CurrentValue, 0.0m),
            PlatinumDelta = platinum.GetValueOrDefault(p => p.CurrentValue, 0.0m) - platinum.GetValueOrDefault(p => p.PreviousValue, 0.0m),
            PlatinumSymbol = platinum.GetValueOrDefault(p => p.ApiSymbol, ""),
            CartPriceLock = settings.CartPriceLock,
            SecondsSinceLastPriceUpdate = scheduledTask.LastSuccessUtc.DeltaInSeconds(),
            PriceUpdateInterval = scheduledTask.Seconds
        };
    }

    private async Task<string> GetPluginVersionAsync()
    {
        if (_version.IsNotNull())
        {
            return _version;
        }

        var pluginDescriptor = await pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("i7MEDIA.Dynamic.Pricing", LoadPluginsMode.InstalledOnly);

        _version = pluginDescriptor.Version;

        return _version;
    }
}