namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;

public class DynamicPricingRequestModel
{
    public decimal BasePrice { get; set; }
    public int MetalType { get; set; }
    public int ProductId { get; set; }
    public decimal Weight { get; set; }
    public decimal PriceModifier { get; set; }
    public int PriceModifierType { get; set; }
}
