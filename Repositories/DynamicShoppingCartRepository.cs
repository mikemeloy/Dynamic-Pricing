using Nop.Core.Domain.Orders;
using Nop.Data;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;

public interface IDynamicShoppingCartRepository
{
    public Task<IEnumerable<ShoppingCartItem>> GetCartItemsByProductId(int productId);
    public Task UpdateCartItem(ShoppingCartItem shoppingCartItem);
}

public class DynamicShoppingCartRepository(IRepository<ShoppingCartItem> cartItemRepo) : IDynamicShoppingCartRepository
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
}
