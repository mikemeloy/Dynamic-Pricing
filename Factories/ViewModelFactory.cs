using i7MEDIA.Plugin.Misc.Core.Extentions;
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
            SelectedMetalType = dynamicPriceInfo.MetalTypeId,
            AvailableMetalTypes = metalTypes.ToSelectItemList(
                label: e => e.Name,
                value: e => e.Id.ToString()
            )
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

        var gold = values.FirstOrDefault(x => x.ApiSymbol == settings.GoldSymbol);
        var silver = values.FirstOrDefault(s => s.ApiSymbol == settings.SilverSymbol);

        return new()
        {
            Version = await GetPluginVersionAsync(),
            GoldPrice = gold.GetValueOrDefault(g => g.CurrentValue, 0.0m),
            GoldDelta = gold.GetValueOrDefault(g => g.CurrentValue, 0.0m) - gold.GetValueOrDefault(g => g.PreviousValue, 0.0m),
            GoldSymbol = gold.GetValueOrDefault(g => g.ApiSymbol, ""),
            SilverPrice = silver.GetValueOrDefault(s => s.CurrentValue, 0.0m),
            SilverDelta = silver.GetValueOrDefault(s => s.CurrentValue, 0.0m) - silver.GetValueOrDefault(s => s.PreviousValue, 0.0m),
            SilverSymbol = silver.GetValueOrDefault(s => s.ApiSymbol, ""),
            CartPriceLock = settings.CartPriceLock,
            SecondsSinceLastPriceUpdate = scheduledTask.LastSuccessUtc.DeltaInSeconds()
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