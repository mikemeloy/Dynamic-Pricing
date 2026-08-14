using i7MEDIA.Plugin.Misc.Core.Extentions;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Enums;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Common;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;

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

    public static DynamicProductPricing ToDynamicPriceEntity(this DynamicPricingRequestModel source)
    {
        return new()
        {
            BasePrice = source.BasePrice,
            MetalTypeId = source.MetalType,
            ProductId = source.ProductId,
            Weight = source.Weight,
            Exclude = source.Exclude,
            PriceModifier = source.PriceModifier,
            PriceModifierTypeId = source.PriceModifierType
        };
    }

    public static decimal DoWeightConversion(this DynamicPriceSettings source, decimal weight)
    {
        return source.WeightConversion * weight;
    }

    public static decimal CalculatePrice(this DynamicPricedProduct source, decimal currentValue)
    {
        if (source.IsNull())
        {
            return 0m;
        }

        var basedOnWeightPrice = source.Weight * currentValue;
        var modifierBasedPrice = source.PriceModifierTypeId switch
        {
            DynamicPriceModifierType.None => basedOnWeightPrice,
            DynamicPriceModifierType.Percentage => (basedOnWeightPrice * (source.PriceModifier / 100)) + basedOnWeightPrice,
            DynamicPriceModifierType.CostPlus => basedOnWeightPrice + source.PriceModifier,
            _ => basedOnWeightPrice,
        };

        return Math.Max(source.BasePrice, modifierBasedPrice);
    }
    /// <summary>
    /// Gets the difference between the source date and now in seconds
    /// </summary> 
    public static int DeltaInSeconds(this DateTime? source)
    {
        if (source.IsNull())
        {
            return 0;
        }

        return source.Value.DeltaInSeconds();
    }
    /// <summary>
    /// Gets the difference between the source date and now in seconds
    /// </summary> 
    public static int DeltaInSeconds(this DateTime source) => (int)(DateTime.UtcNow - source).TotalSeconds;

    public static async Task LogDebugAsync(this ILogger source, string message) => await source.InsertLogAsync(LogLevel.Debug, message);
}