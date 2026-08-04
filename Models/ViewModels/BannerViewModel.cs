namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;

public class BannerViewModel
{
    public required string Version { get; set; }
    public decimal GoldPrice { get; set; }
    public decimal SilverPrice { get; set; }
    public decimal GoldDelta { get; set; }
    public decimal SilverDelta { get; set; }
}