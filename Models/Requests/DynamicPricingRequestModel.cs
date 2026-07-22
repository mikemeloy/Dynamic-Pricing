namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;

public class DynamicPricingRequestModel
{
    public decimal BasePrice { get; internal set; }
    public int MetalType { get; internal set; }
    public int ProductId { get; internal set; }
}
