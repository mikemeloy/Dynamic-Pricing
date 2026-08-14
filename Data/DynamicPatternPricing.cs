using Nop.Core;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;

public class DynamicPatternPricing : BaseEntity
{
    /// <summary>
    /// FK Pattern
    /// </summary>
    public int PatternId { get; set; }
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