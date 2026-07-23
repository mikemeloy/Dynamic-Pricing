using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Extensions;

public static class Extensions
{
    public static List<SelectListItem> ToSelectItemList<TSource>(this IEnumerable<TSource> enumerable, Func<TSource, string> label, Func<TSource, string> value)
    {
        return (from item in enumerable
                select new SelectListItem()
                {
                    Text = label(item),
                    Value = value(item)
                }).ToList();
    }

    public static DynamicPricing ToDynamicPriceEntity(this DynamicPricingRequestModel source)
    {
        return new()
        {
            BasePrice = source.BasePrice,
            MetalTypeId = source.MetalType,
            ProductId = source.ProductId
        };
    }
}
