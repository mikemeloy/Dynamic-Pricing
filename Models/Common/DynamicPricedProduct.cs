using Nop.Core.Domain.Catalog;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Common;

public class DynamicPricedProduct
{
    public required string MetalSymbol { get; set; }
    public required Product Product { get; set; }
    public decimal BasePrice { get; set; }
}