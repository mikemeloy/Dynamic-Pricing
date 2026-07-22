using Nop.Core;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;

public class DynamicPricing : BaseEntity
{
    public decimal BasePrice { get; set; }
    public int ProductId { get; set; }
    public int MetalType { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
}