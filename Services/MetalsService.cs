using System.Text.Json;
using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.External;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Services.Configuration;
using Nop.Services.Logging;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;

public interface IMetalsService
{
    public Task<Dictionary<string, decimal>?> GetCurrentMetalPricesAsync();
}

public class MetalsService(IStoreContext storeContext, ISettingService settingService, ILogger logger, IDynamicPriceService dynamicPriceService) : IMetalsService
{
    /// <summary>
    /// Returns a dictionary mapping Api  material Symbol to DynamicPricingMetalType.ApiSymbol
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, decimal>?> GetCurrentMetalPricesAsync()
    {
        var metalTypes = await dynamicPriceService.GetMetalTypeSymbolsAsync();

        if (!metalTypes.Any())
        {
            return null;
        }

        try
        {
#if !DEBUG
            var settings = await GetSettingsAsync<DynamicPriceSettings>();
            var client = new HttpClient();
            var url = $"{settings.ApiEndpoint}?api_key={settings.ApiKey}&base=USD&currencies={string.Join(',', metalTypes)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
#else
            var rand = new Random();
            var goldValue = rand.Next(7000, 8000);
            var silverValue = rand.Next(500, 700);
            var platinumValue = rand.Next(1000, 1200);

            var content = $"{{\"success\":true,\"base\":\"USD\",\"timestamp\":1784764799,\"rates\":{{\"USDXPT\":{platinumValue},\"USDXAG\":{goldValue},\"USDXAU\":{silverValue},\"XAG\":0.0169528822,\"XAU\":0.0002444298}}}}";
#endif
            var apiResponse = JsonSerializer.Deserialize<PreciousMetalsApiResponse>(content);

            if (apiResponse.IsNull())
            {
                return null;
            }

            return (from symbol in metalTypes
                    select new
                    {
                        Key = symbol,
                        Value = apiResponse.Rates[$"{apiResponse.Base}{symbol}"]?.GetValue<decimal>() ?? 0
                    }).ToDictionary(k => k.Key, v => v.Value);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(GetCurrentMetalPricesAsync), ex);
            return null;
        }
    }

    public async Task<T> GetSettingsAsync<T>() where T : ISettings, new()
    {
        var storeScope = await storeContext.GetActiveStoreScopeConfigurationAsync();
        var setting = await settingService.LoadSettingAsync<T>(storeScope);

        return setting;
    }
}