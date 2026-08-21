using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Repositories;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;

public interface IDynamicPriceTierPriceService
{
    public Task AddTimedTierPriceAsync(int cartItemId, int customerId, decimal price, int productId, int quantity, DateTime endDateUtc, int storeId = 0);
    /// <summary>
    /// Removes any temporary roles created by dynamic pricing
    /// </summary> 
    public Task DynamicPriceRoleCleanupAsync();
}

public class DynamicPriceTierPriceService(ICustomerService customerService, IProductService productService, IDynamicPricingRepository dynamicPricingRepository, IDynamicShoppingCartRepository shoppingCartRepo) : IDynamicPriceTierPriceService
{
    public async Task AddTimedTierPriceAsync(int cartItemId, int customerId, decimal price, int productId, int quantity, DateTime endDateUtc, int storeId = 0)
    {
        var existingTierPrice = await dynamicPricingRepository.GetTierPricingByCartIdAsync(cartItemId);

        if (existingTierPrice.IsNotNull() && existingTierPrice.IsExpired())
        {
            existingTierPrice.Price = price;
            existingTierPrice.EndDateTimeUtc = endDateUtc;

            await productService.UpdateTierPriceAsync(existingTierPrice);
            return;
        }

        var role = new CustomerRole() { Active = true, Name = $"{Guid.NewGuid()}", SystemName = $"{Guid.NewGuid()}" };
        await customerService.InsertCustomerRoleAsync(role);

        await customerService.AddCustomerRoleMappingAsync(new()
        {
            CustomerId = customerId,
            CustomerRoleId = role.Id
        });

        await productService.InsertTierPriceAsync(new()
        {
            CustomerRoleId = role.Id,
            Price = price,
            ProductId = productId,
            Quantity = quantity,
            StartDateTimeUtc = DateTime.UtcNow,
            EndDateTimeUtc = endDateUtc,
            StoreId = storeId
        });

        await dynamicPricingRepository.InsertDynamicPriceRoleMappingAsync(new()
        {
            RoleId = role.Id,
            CustomerId = customerId,
            CartItemId = cartItemId
        });
    }

    public async Task DynamicPriceRoleCleanupAsync()
    {
        foreach (var mapping in await dynamicPricingRepository.GetExpiredDynamicPriceRolesAsync())
        {
            var role = await customerService.GetCustomerRoleByIdAsync(mapping.RoleId);
            if (role.IsNull())
            {
                continue;
            }

            var customer = await customerService.GetCustomerByIdAsync(mapping.CustomerId);

            await customerService.RemoveCustomerRoleMappingAsync(customer, role);
            await customerService.DeleteCustomerRoleAsync(role);
            await dynamicPricingRepository.DeleteDynamicPriceRoleMappingAsync(role.Id);
            await dynamicPricingRepository.DeleteTierPricingByRoleIdAsync(role.Id);
        }
    }
}