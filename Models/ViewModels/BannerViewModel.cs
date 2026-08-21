namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;

public class BannerViewModel
{
    public required int CartPriceLock { get; set; }
    public required string Version { get; set; }
    public int SecondsSinceLastPriceUpdate { get; set; }
    public int PriceUpdateInterval { get; set; }
    public IEnumerable<BannerTokens> Tokens { get; set; } = Enumerable.Empty<BannerTokens>();
}

public class BannerTokens
{
    public decimal Price { get; set; }
    public decimal Delta { get; set; }
    public required string Symbol { get; set; }
}