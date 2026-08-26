using i7MEDIA.Plugin.Misc.Core.Extentions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;

public interface IDynamicShoppingCartRepository
{
    public Task<IEnumerable<ShoppingCartItem>> GetCartItemsByProductId(int productId);
    public Task UpdateCartItem(ShoppingCartItem shoppingCartItem);
    public Task<int> GetCartPriceLockByCustomerAsync(Customer customer);
}

public class DynamicShoppingCartRepository(IRepository<ShoppingCartItem> cartItemRepo, IRepository<TierPrice> tierPriceRepo, IRepository<CustomerCustomerRoleMapping> mappingRepo, IRepository<CustomerRole> roleRepo) : IDynamicShoppingCartRepository
{
    public async Task<IEnumerable<ShoppingCartItem>> GetCartItemsByProductId(int productId)
    {
        var query = from ci in cartItemRepo.Table
                    where ci.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart
                    where ci.ProductId == productId
                    select ci;

        return await query.ToListAsync();
    }

    public async Task UpdateCartItem(ShoppingCartItem shoppingCartItem)
    {
        await cartItemRepo.UpdateAsync(shoppingCartItem);
    }

    public async Task<int> GetCartPriceLockByCustomerAsync(Customer customer)
    {
        if (customer.IsNull())
        {
            return 0;
        }

        var query = await (from t in tierPriceRepo.Table
                           join r in roleRepo.Table on t.CustomerRoleId equals r.Id
                           join cart in cartItemRepo.Table on t.ProductId equals cart.ProductId
                           join map in mappingRepo.Table on r.Id equals map.CustomerRoleId
                           where map.CustomerId == customer.Id && cart.CustomerId == customer.Id
                           select t.EndDateTimeUtc).FirstOrDefaultAsync();

        if (query.IsNull())
        {
            return 0;
        }

        var remainingCartLock = (int)(query - DateTime.UtcNow).Value.TotalSeconds;

        return Math.Max(0, remainingCartLock);
    }
}