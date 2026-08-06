using Nop.Core.Domain.Orders;
using Nop.Data;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;

public interface IDynamicShoppingCartRepository
{
    public Task<IEnumerable<ShoppingCartItem>> GetAllCartItemsAsync();
}

public class DynamicShoppingCartRepository(IRepository<ShoppingCartItem> cartItemRepo) : IDynamicShoppingCartRepository
{
    public async Task<IEnumerable<ShoppingCartItem>> GetAllCartItemsAsync()
    {
        var query = from ci in cartItemRepo.Table
                    where ci.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart
                    select ci;

        return await query.ToListAsync();

    }
}
