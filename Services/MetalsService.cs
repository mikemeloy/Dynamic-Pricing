using System.Text.Json;
using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.External;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;

public interface IMetalsService
{
    public Task<Dictionary<string, decimal>> GetCurrentMetalPricesAsync();
}

public class MetalsService(IDynamicPriceService dynamicPriceService) : IMetalsService
{
    /// <summary>
    /// Returns a dictionary mapping Api  material Symbol to DynamicPricingMetalType.ApiSymbol
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, decimal>> GetCurrentMetalPricesAsync()
    {
        var metalTypes = await dynamicPriceService.GetMetalTypeSymbolsAsync();
        var apiKey = "bad6c749effd9d1d8937845988089594";

        if (!metalTypes.Any())
        {
            return new();
        }

#if !DEBUG
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.metalpriceapi.com/v1/latest?api_key={apiKey}&base=USD&currencies={string.Join(',', metalTypes)}");
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
#else        
        var rand = new Random();
        var goldValue = rand.Next(0, 8000);
        var silverValue = rand.Next(0, 700);

        var content = $"{{\"success\":true,\"base\":\"USD\",\"timestamp\":1784764799,\"rates\":{{\"USDXAG\":{silverValue},\"USDXAU\":{goldValue},\"XAG\":0.0169528822,\"XAU\":0.0002444298}}}}";
#endif


        var apiResponse = JsonSerializer.Deserialize<PreciousMetalsApiResponse>(content);

        if (apiResponse.IsNull())
        {
            return new();
        }

        return (from symbol in metalTypes
                select new
                {
                    Key = symbol,
                    Value = apiResponse.Rates[symbol]?.GetValue<decimal>() ?? 0
                }).ToDictionary(k => k.Key, v => v.Value);
    }
}