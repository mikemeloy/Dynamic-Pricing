using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Core.Helpers;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Enums;
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
    public Task<Product> GetProductByIdAsync(int productId);
    public Task UpdateProductAsync(Product product);
    public Task InsertDynamicPriceRoleMappingAsync(DynamicPriceRoleMapping roleMapping);
    public Task DeleteDynamicPriceRoleMappingAsync(DynamicPriceRoleMapping roleMapping);
    public Task<IList<DynamicPriceRoleMapping>> GetExpiredDynamicPriceRolesAsync();
    public Task<bool> GetDynamicPriceMappingByCartItemId(int cartItemId);
}

public class DynamicPricingRepository(IRepository<TierPrice> tierPriceRepo, IRepository<DynamicPriceRoleMapping> roleMappingRepository, IRepository<DynamicPricingMetalType> metalTypeRepo, IRepository<DynamicPricing> dynamicPriceRepo, IRepository<Product> productRepo) : IDynamicPricingRepository
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
                    Weight = d.Weight,
                    Product = p,
                    PriceModifier = d.PriceModifier,
                    PriceModifierTypeId = EnumHelper.ToEnum<DynamicPriceModifierType>(d.PriceModifierTypeId)
                }).ToList();
    }

    public Task<Product> GetProductByIdAsync(int productId)
    {
        return (from p in productRepo.Table
                where p.Id == productId
                select p).FirstOrDefaultAsync();
    }

    public async Task UpdateProductAsync(Product product)
    {
        await productRepo.UpdateAsync(product);
    }

    public async Task<IList<DynamicPriceRoleMapping>> GetExpiredDynamicPriceRolesAsync()
    {
        return await roleMappingRepository.GetAllAsync(async mapping =>
            from map in mapping
            join tier in tierPriceRepo.Table on map.RoleId equals tier.CustomerRoleId
            where tier.EndDateTimeUtc < DateTime.UtcNow
            select map
        );
    }

    public async Task InsertDynamicPriceRoleMappingAsync(DynamicPriceRoleMapping roleMapping)
    {
        await roleMappingRepository.InsertAsync(roleMapping);
    }

    public async Task DeleteDynamicPriceRoleMappingAsync(DynamicPriceRoleMapping roleMapping)
    {
        await roleMappingRepository.DeleteAsync(roleMapping);
    }

    public async Task<bool> GetDynamicPriceMappingByCartItemId(int cartItemId)
    {
        var result = await roleMappingRepository.GetAllAsync(async mapping =>
           from map in mapping
           where map.CartItemId == cartItemId
           select map
       );

        return result.Any();
    }
}