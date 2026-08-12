using Nop.Core;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;

public class DynamicPricing : BaseEntity
{
    public decimal BasePrice { get; set; }
    /// <summary>
    /// Weight of the precious metal content in troy ounce (oz t)
    /// </summary>
    public decimal Weight { get; set; }
    /// <summary>
    /// FK Product
    /// </summary>
    public int ProductId { get; set; }
    /// <summary>
    /// FK DynamicPricingMetalType
    /// </summary>
    public int MetalTypeId { get; set; }
    public decimal PriceModifier { get; set; }
    public int PriceModifierTypeId { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}