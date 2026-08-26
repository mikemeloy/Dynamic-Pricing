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

        return new()
        {
            Version = await GetPluginVersionAsync(),
            SecondsSinceLastPriceUpdate = scheduledTask.LastSuccessUtc.DeltaInSeconds(),
            PriceUpdateInterval = scheduledTask.Seconds,
            Tokens = from t in values
                     where !string.IsNullOrWhiteSpace(t.ApiSymbol)
                     select new BannerTokens()
                     {
                         Symbol = t.ApiSymbol,
                         Delta = t.CurrentValue - t.PreviousValue,
                         Price = t.CurrentValue
                     }
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