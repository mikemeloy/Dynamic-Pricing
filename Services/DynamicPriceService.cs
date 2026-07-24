using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using NUglify.Helpers;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;

public interface IDynamicPriceService
{
    public Task SaveDynamicPricingAsync(DynamicPricing dynamicPricing);
    public Task<IEnumerable<DynamicPricingMetalType>> GetMetalTypesAsync();
    public Task<DynamicPricing> GetProductDynamicPriceByProductIdAsync(int productId);
    public Task InsertMetalTypeAsync(DynamicPricingMetalType dynamicPricingMetalType);
    public Task DeleteMetalTypeAsync(int metalTypeId);
    public Task<IEnumerable<string>> GetMetalTypeSymbolsAsync();
    public Task UpdateMetalPrices(Dictionary<string, decimal> dicMetalValues);
    public Task UpdateProductPricesByMetalType();
    public Task<T> GetSettingsAsync<T>() where T : ISettings, new();
    public Task InsertInitialSettings();
}

public class DynamicPriceService(IStoreContext storeContext, ISettingService settingService, ILogger logger, IDynamicPricingRepository dynamicPricingRepository) : IDynamicPriceService
{
    public async Task<DynamicPricing> GetProductDynamicPriceByProductIdAsync(int productId)
    {
        try
        {
            var product = await dynamicPricingRepository.GetProductByIdAsync(productId);
            var settings = await GetSettingsAsync<DynamicPriceSettings>();

            return await dynamicPricingRepository.GetDynamicPricingByProductIdAsync(productId) ??
                new()
                {
                    BasePrice = product.Price,
                    Weight = settings.DoWeightConversion(product.Weight)
                };
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(GetProductDynamicPriceByProductIdAsync), ex);
        }

        return new();
    }

    public async Task SaveDynamicPricingAsync(DynamicPricing pricing)
    {
        try
        {
            var existing = await dynamicPricingRepository.GetDynamicPricingByProductIdAsync(pricing.ProductId);

            if (existing.IsNotNull())
            {
                existing.MetalTypeId = pricing.MetalTypeId;
                existing.BasePrice = pricing.BasePrice;
                existing.UpdatedBy = -1;

                await dynamicPricingRepository.UpdateDynamicPricingAsync(existing);
                return;
            }

            await dynamicPricingRepository.InsertDynamicPricingAsync(pricing);
        }
        catch (Exception ex)
        {
            logger.Error(nameof(SaveDynamicPricingAsync), ex);
        }
    }

    public async Task<IEnumerable<DynamicPricingMetalType>> GetMetalTypesAsync()
    {
        try
        {
            return await dynamicPricingRepository.GetMetalTypesAsync();
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(GetMetalTypesAsync), ex);
        }

        return Enumerable.Empty<DynamicPricingMetalType>();
    }

    public async Task InsertMetalTypeAsync(DynamicPricingMetalType pricingMetalType)
    {
        try
        {
            await dynamicPricingRepository.InsertMetalTypeAsync(pricingMetalType);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(InsertMetalTypeAsync), ex);
        }
    }

    public async Task UpdateMetalTypeAsync(DynamicPricingMetalType pricingMetalType)
    {
        try
        {
            await logger.LogDebugAsync($"{pricingMetalType.ApiSymbol} price set at {pricingMetalType.CurrentValue} @ {DateTime.UtcNow:G} UTC");
            await dynamicPricingRepository.UpdateMetalTypeAsync(pricingMetalType);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(UpdateMetalTypeAsync), ex);
        }
    }

    public async Task DeleteMetalTypeAsync(int metalTypeId)
    {
        try
        {
            await dynamicPricingRepository.DeleteMetalTypeAsync(metalTypeId);
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(DeleteMetalTypeAsync), ex);
        }
    }

    public async Task<IEnumerable<string>> GetMetalTypeSymbolsAsync()
    {
        return from mt in await GetMetalTypesAsync()
               select mt.ApiSymbol;
    }

    public async Task UpdateMetalPrices(Dictionary<string, decimal> symbolValues)
    {
        var metalTypes = from metal in await GetMetalTypesAsync()
                         join sv in symbolValues on metal.ApiSymbol equals sv.Key
                         select new DynamicPricingMetalType()
                         {
                             Id = metal.Id,
                             Name = metal.Name,
                             Description = metal.Description,
                             ApiSymbol = metal.ApiSymbol,
                             CurrentValue = sv.Value,
                             Deleted = metal.Deleted,
                         };


        metalTypes.ForEach(async metalType => await UpdateMetalTypeAsync(metalType));
    }

    public async Task UpdateProductPricesByMetalType()
    {
        var metalTypes = await GetMetalTypesAsync();
        var productGrouping = await dynamicPricingRepository.GetProductsByMetalTypeAssociationAsync();

        foreach (var productInfo in productGrouping)
        {
            var product = productInfo.Product;
            var metalType = metalTypes.FirstOrDefault(mt => mt.ApiSymbol == productInfo.MetalSymbol);

            if (metalType.IsNull())
            {
                continue;
            }

            var newPrice = await CalculateProductTotalByConversionSettingsAsync(
                    basePrice: productInfo.BasePrice,
                    weight: productInfo.Weight,
                    currentValue: metalType.CurrentValue
                );

            await logger.LogDebugAsync($"Product {product.Name} (Id: {product.Id}) OldPrice: {product.Price} NewPrice:{newPrice} @ {DateTime.UtcNow:G}");

            product.Price = newPrice;

            await dynamicPricingRepository.UpdateProductAsync(product: product);
        }
    }

    public async Task<T> GetSettingsAsync<T>() where T : ISettings, new()
    {
        var storeScope = await storeContext.GetActiveStoreScopeConfigurationAsync();
        var setting = await settingService.LoadSettingAsync<T>(storeScope);

        return setting;
    }

    public async Task InsertInitialSettings()
    {
        var setting = new DynamicPriceSettings();

        await settingService.SaveSettingAsync(setting);
    }

    private async Task<decimal> CalculateProductTotalByConversionSettingsAsync(decimal basePrice, decimal weight, decimal currentValue)
    {

        var convertedWeight = weight;
        var totalValueByWeight = convertedWeight * currentValue;

        return Math.Max(basePrice, totalValueByWeight);
    }
}