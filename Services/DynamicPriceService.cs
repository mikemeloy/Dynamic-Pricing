using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;
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
}

public class DynamicPriceService(ILogger logger, IDynamicPricingRepository dynamicPricingRepository) : IDynamicPriceService
{
    public async Task<DynamicPricing> GetProductDynamicPriceByProductIdAsync(int productId)
    {
        try
        {
            return await dynamicPricingRepository.GetDynamicPricingByProductIdAsync(productId) ??
                new()
                {
                    BasePrice = await dynamicPricingRepository.GetProductPriceProductIdAsync(productId)
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
        var products = await dynamicPricingRepository.GetProductsByMetalTypeAssociationAsync();

        foreach (var product in products)
        {
            var p = product.Product;
            var x = product.BasePrice;
            var y = product.MetalSymbol;
            var z = metalTypes.FirstOrDefault(mt => mt.ApiSymbol == y);
        }
    }
}