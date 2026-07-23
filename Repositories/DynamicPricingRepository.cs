using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Common;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;

public interface IDynamicPricingRepository
{
    public Task InsertDynamicPricingAsync(DynamicPricing dynamicPrice);
    public Task UpdateDynamicPricingAsync(DynamicPricing dynamicPrice);
    public Task<IList<DynamicPricingMetalType>> GetMetalTypesAsync();
    public Task<DynamicPricing?> GetDynamicPricingByProductIdAsync(int productId);
    public Task InsertMetalTypeAsync(DynamicPricingMetalType metalType);
    public Task UpdateMetalTypeAsync(DynamicPricingMetalType pricingMetalType);
    public Task DeleteMetalTypeAsync(int metalTypeId);
    public Task<IList<DynamicPricing>> GetAllDynamicPricingItemsAsync();
    public Task<List<DynamicPricedProduct>> GetProductsByMetalTypeAssociationAsync();
    public Task<decimal> GetProductPriceProductIdAsync(int productId);
}

public class DynamicPricingRepository(IRepository<DynamicPricingMetalType> metalTypeRepo, IRepository<DynamicPricing> dynamicPriceRepo, IRepository<Product> productRepo) : IDynamicPricingRepository
{
    public async Task InsertDynamicPricingAsync(DynamicPricing dynamicPrice)
    {
        dynamicPrice.UpdatedOnUtc = dynamicPrice.CreatedOnUtc = DateTime.UtcNow;
        await dynamicPriceRepo.InsertAsync(dynamicPrice);
    }

    public async Task UpdateDynamicPricingAsync(DynamicPricing dynamicPrice)
    {
        dynamicPrice.UpdatedOnUtc = DateTime.UtcNow;
        await dynamicPriceRepo.UpdateAsync(dynamicPrice);
    }

    public async Task<IList<DynamicPricingMetalType>> GetMetalTypesAsync()
    {
        return await metalTypeRepo.GetAllAsync(async metalTypes =>
             from mt in metalTypes
             where !mt.Deleted
             select mt
        );
    }

    public async Task<IList<DynamicPricing>> GetAllDynamicPricingItemsAsync()
    {
        return await dynamicPriceRepo.GetAllAsync(async dynamicPrices =>
       {
           return from dp in dynamicPrices
                  select dp;
       });
    }

    public async Task<DynamicPricing?> GetDynamicPricingByProductIdAsync(int productId)
    {
        return (await dynamicPriceRepo.GetAllAsync(async dynamicPrices =>
        {
            return from dp in dynamicPrices
                   where dp.ProductId == productId
                   select dp;
        })).FirstOrDefault();
    }

    public async Task<DynamicPricingMetalType?> GetMetalTypeByIdAsync(int metalTypeId)
    {
        return (await metalTypeRepo.GetAllAsync(async metalTypes =>
         {
             return from dp in metalTypes
                    where dp.Id == metalTypeId
                    select dp;
         })).FirstOrDefault();
    }

    public async Task InsertMetalTypeAsync(DynamicPricingMetalType metalType)
    {
        await metalTypeRepo.InsertAsync(metalType);
    }

    public async Task DeleteMetalTypeAsync(int metalTypeId)
    {
        var existing = await GetMetalTypeByIdAsync(metalTypeId);

        if (existing.IsNull())
        {
            return;
        }

        await metalTypeRepo.DeleteAsync(existing);
    }

    public async Task UpdateMetalTypeAsync(DynamicPricingMetalType pricingMetalType)
    {
        await metalTypeRepo.UpdateAsync(pricingMetalType);
    }

    public async Task<List<DynamicPricedProduct>> GetProductsByMetalTypeAssociationAsync()
    {
        return (from p in productRepo.Table
                join d in dynamicPriceRepo.Table on p.Id equals d.ProductId
                join mt in metalTypeRepo.Table on d.MetalTypeId equals mt.Id
                select new DynamicPricedProduct()
                {
                    MetalSymbol = mt.ApiSymbol,
                    BasePrice = d.BasePrice,
                    Product = p
                }).ToList();
    }

    public Task<decimal> GetProductPriceProductIdAsync(int productId)
    {
        return (from p in productRepo.Table
                where p.Id == productId
                select p.Price).FirstOrDefaultAsync();
    }
}