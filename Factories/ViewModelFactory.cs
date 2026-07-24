using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Services;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Factories;

public interface IViewModelFactory
{
    public Task<AdminProductViewModel> GetAdminProductViewModel(int productId);
}

public class ViewModelFactory(IDynamicPriceService dynamicPriceService) : IViewModelFactory
{
    public async Task<AdminProductViewModel> GetAdminProductViewModel(int productId)
    {
        var metalTypes = await dynamicPriceService.GetMetalTypesAsync();
        var dynamicPriceInfo = await dynamicPriceService.GetProductDynamicPriceByProductIdAsync(productId);

        return new()
        {
            BasePrice = dynamicPriceInfo.BasePrice,
            Weight = dynamicPriceInfo.Weight,
            ProductId = productId,
            SelectedMetalType = dynamicPriceInfo.MetalTypeId,
            AvailableMetalTypes = metalTypes.ToSelectItemList(
                label: e => e.Name,
                value: e => e.Id.ToString()
            )
        };
    }
}
