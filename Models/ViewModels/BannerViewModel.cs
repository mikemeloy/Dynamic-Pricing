namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;

public class BannerViewModel
{
    public required int CartPriceLock { get; set; }
    public int SecondsSinceLastPriceUpdate { get; set; }
    public required string Version { get; set; }
    public decimal GoldPrice { get; set; }
    public decimal SilverPrice { get; set; }
    public decimal GoldDelta { get; set; }
    public decimal SilverDelta { get; set; }
    public required string GoldSymbol { get; set; }
    public required string SilverSymbol { get; set; }
}