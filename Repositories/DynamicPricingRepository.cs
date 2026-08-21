using i7MEDIA.Plugin.Misc.Core.Data;
using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Core.Helpers;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Enums;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Common;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;

public interface IDynamicPricingRepository
{
    public Task InsertDynamicPricingAsync(DynamicProductPricing dynamicPrice);
    public Task UpdateDynamicPricingAsync(DynamicProductPricing dynamicPrice);
    public Task<IList<DynamicPricingMetalType>> GetMetalTypesAsync();
    public Task<DynamicProductPricing?> GetDynamicPricingByProductIdAsync(int productId);
    public Task InsertMetalTypeAsync(DynamicPricingMetalType metalType);
    public Task UpdateMetalTypeAsync(DynamicPricingMetalType pricingMetalType);
    public Task DeleteMetalTypeAsync(int metalTypeId);
    public Task<IList<DynamicProductPricing>> GetAllDynamicPricingItemsAsync();
    public Task<List<DynamicPricedProduct>> GetProductsByMetalTypeAssociationAsync();
    public Task<Product> GetProductByIdAsync(int productId);
    public Task UpdateProductAsync(Product product);
    public Task InsertDynamicPriceRoleMappingAsync(DynamicPriceRoleMapping roleMapping);
    public Task DeleteDynamicPriceRoleMappingAsync(int roleId);
    public Task<IList<DynamicPriceRoleMapping>> GetExpiredDynamicPriceRolesAsync();
    public Task<TierPrice> GetTierPricingByCartIdAsync(int cartItemId);
    public Task<IEnumerable<Product>> GetPatternProductsbyIdAsync(int patternId);
    public Task<IEnumerable<CartItemDetails>> GetCustomerDynamicallyPricedCartItemsAsync(int customerId);
    public Task DeleteTierPricingByRoleIdAsync(int id);
}

public class DynamicPricingRepository(IRepository<PatternProductMapping> productMappingRepository, IRepository<ShoppingCartItem> cartItemRepo, IRepository<TierPrice> tierPriceRepo, IRepository<DynamicPriceRoleMapping> roleMappingRepository, IRepository<DynamicPricingMetalType> metalTypeRepo, IRepository<DynamicProductPricing> dynamicPriceRepo, IRepository<Product> productRepo, IRepository<CustomerRole> customerRoleRepo) : IDynamicPricingRepository
{
    public async Task InsertDynamicPricingAsync(DynamicProductPricing dynamicPrice)
    {
        dynamicPrice.UpdatedOnUtc = dynamicPrice.CreatedOnUtc = DateTime.UtcNow;
        await dynamicPriceRepo.InsertAsync(dynamicPrice);
    }

    public async Task UpdateDynamicPricingAsync(DynamicProductPricing dynamicPrice)
    {
        dynamicPrice.UpdatedOnUtc = DateTime.UtcNow;
        await dynamicPriceRepo.UpdateAsync(dynamicPrice);
    }

    public async Task<IList<DynamicPricingMetalType>> GetMetalTypesAsync()
    {
        return await metalTypeRepo.GetAllAsync(async metalTypes =>
             from mt in metalTypes
             where !mt.Deleted
             orderby mt.Order
             select mt
        );
    }

    public async Task<IList<DynamicProductPricing>> GetAllDynamicPricingItemsAsync()
    {
        return await dynamicPriceRepo.GetAllAsync(async dynamicPrices =>
       {
           return from dp in dynamicPrices
                  select dp;
       });
    }

    public async Task<DynamicProductPricing?> GetDynamicPricingByProductIdAsync(int productId)
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
                where !p.Deleted && !d.Exclude
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

    public async Task<IEnumerable<CartItemDetails>> GetCustomerDynamicallyPricedCartItemsAsync(int customerId)
    {
        var query = from p in productRepo.Table
                    join dp in dynamicPriceRepo.Table on p.Id equals dp.ProductId
                    join cart in cartItemRepo.Table on p.Id equals cart.ProductId
                    where cart.CustomerId == customerId
                    select new CartItemDetails
                    {
                        ProductId = dp.ProductId,
                        CartItemId = cart.Id,
                        CustomerId = cart.CustomerId,
                        Price = p.Price,
                        Quantity = cart.Quantity
                    };

        return await query.ToListAsync();
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

    public async Task DeleteDynamicPriceRoleMappingAsync(int roleId)
    {
        var mapping = await (from m in roleMappingRepository.Table
                             where m.RoleId == roleId
                             select m).FirstOrDefaultAsync();

        if (mapping.IsNull())
        {
            return;
        }

        await roleMappingRepository.DeleteAsync(mapping);
    }

    public async Task<TierPrice> GetTierPricingByCartIdAsync(int cartItemId)
    {
        var query = from dpr in roleMappingRepository.Table
                    join cr in customerRoleRepo.Table on dpr.RoleId equals cr.Id
                    join tp in tierPriceRepo.Table on cr.Id equals tp.CustomerRoleId
                    where dpr.CartItemId == cartItemId
                    select tp;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetPatternProductsbyIdAsync(int patternId)
    {
        var query = from p in productRepo.Table
                    join map in productMappingRepository.Table on p.Id equals map.ProductId
                    where map.PatternId == patternId
                    select p;

        return await query.ToListAsync();
    }

    public async Task DeleteTierPricingByRoleIdAsync(int id)
    {
        var query = (from tp in tierPriceRepo.Table
                     where tp.CustomerRoleId == id
                     select tp).ToList();

        await tierPriceRepo.DeleteAsync(query);
    }
}