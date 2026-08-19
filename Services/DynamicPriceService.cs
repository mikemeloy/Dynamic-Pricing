using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;

public interface IDynamicPriceService
{
    public Task SaveDynamicPricingAsync(DynamicProductPricing dynamicPricing);
    public Task<IEnumerable<DynamicPricingMetalType>> GetMetalTypesAsync();
    public Task<DynamicProductPricing> GetProductDynamicPriceByProductIdAsync(int productId);
    public Task InsertMetalTypeAsync(DynamicPricingMetalType dynamicPricingMetalType);
    public Task DeleteMetalTypeAsync(int metalTypeId);
    public Task<IEnumerable<string>> GetMetalTypeSymbolsAsync();
    public Task UpdateMetalPrices(Dictionary<string, decimal> dicMetalValues);
    /// <summary>
    /// Updates the price of all items associated with a metal type based on [DynamicPricingMetalType] records
    /// </summary> 
    public Task UpdateProductPricesByMetalType();
    public Task<T> GetSettingsAsync<T>() where T : ISettings, new();
    public Task InsertInitialSettings();
    public Task SaveSettingsAsync(decimal conversion, string apiKey, string endpoint, int cartPriceLockInSeconds);
    public Task<ScheduleTask> GetDynamicPriceScheduledTaskAsync();
    public Task SetPatternProductsAsDyanamicallyPricedAsync(IEnumerable<int> patternIds);
    public Task SetPatternProductsAsDyanamicallyPricedAsync(int patternId);
    public Task UpdateDynamicallyPriceCartItemsAsync();
    public Task<int> GetCurrentCartLock();
}

public class DynamicPriceService(ILogger logger, IWorkContext workContext, IStoreContext storeContext, IDynamicPriceTierPriceService dynamicPricePriceService, IScheduleTaskService scheduleTaskService, ISettingService settingService, IDynamicPricingRepository dynamicPricingRepository, IDynamicShoppingCartRepository dynamicShoppingCartRepo) : IDynamicPriceService
{
    public async Task<DynamicProductPricing> GetProductDynamicPriceByProductIdAsync(int productId)
    {
        try
        {
            var product = await dynamicPricingRepository.GetProductByIdAsync(productId);

            if (product.IsNull())
            {
                return new();
            }

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

    public async Task SaveDynamicPricingAsync(DynamicProductPricing pricing)
    {
        try
        {
            var existing = await dynamicPricingRepository.GetDynamicPricingByProductIdAsync(pricing.ProductId);

            if (existing.IsNotNull())
            {
                existing.MetalTypeId = pricing.MetalTypeId;
                existing.BasePrice = pricing.BasePrice;
                existing.Weight = pricing.Weight;
                existing.UpdatedBy = -1;
                existing.PriceModifierTypeId = pricing.PriceModifierTypeId;
                existing.PriceModifier = pricing.PriceModifier;
                existing.Exclude = pricing.Exclude;

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
                             PreviousValue = metal.CurrentValue,
                             CurrentValue = sv.Value,
                             Deleted = metal.Deleted,
                         };

        foreach (var metalType in metalTypes)
        {
            await UpdateMetalTypeAsync(metalType);
        }
    }

    public async Task UpdateProductPricesByMetalType()
    {
        var metalTypes = await GetMetalTypesAsync();
        var productGrouping = await dynamicPricingRepository.GetProductsByMetalTypeAssociationAsync();
        var settings = await GetSettingsAsync<DynamicPriceSettings>();

        foreach (var productInfo in productGrouping)
        {
            var metalType = metalTypes.FirstOrDefault(mt => mt.ApiSymbol == productInfo.MetalSymbol);

            if (metalType.IsNull())
            {
                continue;
            }

            var product = productInfo.Product;
            var oldPrice = product.Price;
            var newPrice = productInfo.CalculatePrice(currentValue: metalType.CurrentValue);

            product.Price = newPrice;

            await logger.LogDebugAsync($"Product {product.Name} (Id: {product.Id}) OldPrice: {oldPrice} NewPrice:{newPrice} @ {DateTime.UtcNow:G}");
            await dynamicPricingRepository.UpdateProductAsync(product: product);
        }
    }

    public async Task<T> GetSettingsAsync<T>() where T : ISettings, new()
    {
        var storeScope = await storeContext.GetActiveStoreScopeConfigurationAsync();

        return await settingService.LoadSettingAsync<T>(storeScope);
    }

    public async Task InsertInitialSettings()
    {
        await settingService.SaveSettingAsync(
            settings: new DynamicPriceSettings()
        );
    }

    public async Task SaveSettingsAsync(decimal conversion, string apiKey, string endpoint, int cartPriceLockInSeconds)
    {
        try
        {
            var settings = await GetSettingsAsync<DynamicPriceSettings>();

            await settingService.SaveSettingAsync<DynamicPriceSettings>(new()
            {
                ApiEndpoint = endpoint,
                ApiKey = apiKey,
                WeightConversion = conversion,
                CartPriceLock = cartPriceLockInSeconds,
                GoldSymbol = settings.GoldSymbol,
                SilverSymbol = settings.SilverSymbol,
                PlatinumSymbol = settings.PlatinumSymbol
            });
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(SaveSettingsAsync), ex);
        }
    }

    public async Task SetPatternProductsAsDyanamicallyPricedAsync(IEnumerable<int> patternIds)
    {
        if (patternIds.IsNull())
        {
            return;
        }

        foreach (var patternId in patternIds)
        {
            await SetPatternProductsAsDyanamicallyPricedAsync(patternId);
        }
    }

    public async Task SetPatternProductsAsDyanamicallyPricedAsync(int patternId)
    {
        var products = await dynamicPricingRepository.GetPatternProductsbyIdAsync(patternId);

        foreach (var product in products)
        {
            var pricing = await GetProductDynamicPriceByProductIdAsync(product.Id);

            if (pricing.IsNull())
            {
                continue;
            }

            pricing.Exclude = false;

            await dynamicPricingRepository.UpdateDynamicPricingAsync(pricing);
        }
    }

    public async Task UpdateDynamicallyPriceCartItemsAsync()
    {
        var customer = await workContext.GetCurrentCustomerAsync();
        var cartItems = await dynamicPricingRepository.GetCustomerDynamicallyPricedCartItemsAsync(customer.Id);
        var settings = await GetSettingsAsync<DynamicPriceSettings>();

        foreach (var cartItem in cartItems)
        {
            var endDate = DateTime.UtcNow.AddSeconds(settings.CartPriceLock);

            await dynamicPricePriceService.AddTimedTierPriceAsync(
                cartItemId: cartItem.CartItemId,
                customerId: cartItem.CustomerId,
                productId: cartItem.ProductId,
                quantity: cartItem.Quantity,
                endDateUtc: endDate,
                price: cartItem.Price
            );
        }
    }

    public async Task<ScheduleTask> GetDynamicPriceScheduledTaskAsync()
    {
        try
        {
            return await scheduleTaskService.GetTaskByTypeAsync(PluginDefaults.ScheduledTaskType);

        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(GetDynamicPriceScheduledTaskAsync), ex);
        }

        return new();
    }

    private async Task UpdateMetalTypeAsync(DynamicPricingMetalType pricingMetalType)
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

    public async Task<int> GetCurrentCartLock()
    {
        try
        {
            var customer = await workContext.GetCurrentCustomerAsync();
            return await dynamicShoppingCartRepo.GetCartPriceLockByCustomerAsync(customer);

        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(GetCurrentCartLock), ex);
        }

        return 0;
    }
}