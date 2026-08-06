using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

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
    public Task SaveSettingsAsync(decimal conversion, string apiKey, string endpoint, int cartPriceLockInSeconds);
    public Task<ScheduleTask> GetDynamicPriceScheduledTaskAsync();
}

public class DynamicPriceService(IScheduleTaskService scheduleTaskService, IStoreContext storeContext, ISettingService settingService, ILogger logger, IDynamicShoppingCartRepository shoppingCartRepository, IDynamicPricingRepository dynamicPricingRepository, IDiscountService discountService, ICustomerService customerService, IProductService productService) : IDynamicPriceService
{
    public async Task<DynamicPricing> GetProductDynamicPriceByProductIdAsync(int productId)
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

    public async Task SaveDynamicPricingAsync(DynamicPricing pricing)
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
        var cartItems = await shoppingCartRepository.GetAllCartItemsAsync();
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
            var cartItem = cartItems.FirstOrDefault(ci => ci.ProductId == product.Id);

            await logger.LogDebugAsync($"Product {product.Name} (Id: {product.Id}) OldPrice: {oldPrice} NewPrice:{newPrice} @ {DateTime.UtcNow:G}");

            product.Price = newPrice;

            await dynamicPricingRepository.UpdateProductAsync(product: product);

            if (cartItem.IsNotNull())
            {
                await InsertCartItemDiscountAsync(cartItem, newPrice, oldPrice, settings.CartPriceLock);
            }
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
    /// <summary>
    /// Applies a temporary discount to a cart item if the dynamic price increases within a time specified by a the setting CartPriceLock
    /// </summary>
    public async Task InsertCartItemDiscountAsync(ShoppingCartItem cartItem, decimal newPrice, decimal oldPrice, int cartPriceLock)
    {
        var secondsInCart = (DateTime.UtcNow - cartItem.CreatedOnUtc).TotalSeconds;

        if (oldPrice > newPrice || secondsInCart > cartPriceLock)
        {
            return;
        }

        var discountAmount = newPrice - oldPrice;

        var discount = ObjectModelFactory.CreateDiscount(
            discountAmount: discountAmount,
            endDateUtc: cartItem.CreatedOnUtc.AddSeconds(cartPriceLock - secondsInCart),
            adminComment: "Inserted via dynamic pricing",
            name: $"discount product id: {cartItem.ProductId} for customer {cartItem.CustomerId} for {discountAmount}"
        );

        await discountService.InsertDiscountAsync(discount);
        await productService.InsertDiscountProductMappingAsync(new() { EntityId = cartItem.ProductId, DiscountId = discount.Id });
        await customerService.ApplyDiscountCouponCodeAsync(
            customer: await customerService.GetCustomerByIdAsync(cartItem.CustomerId),
            couponCode: discount.CouponCode
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
                SilverSymbol = settings.SilverSymbol
            });
        }
        catch (Exception ex)
        {
            await logger.ErrorAsync(nameof(SaveSettingsAsync), ex);
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
}