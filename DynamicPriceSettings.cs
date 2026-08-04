namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing;

public class DynamicPriceSettings : Nop.Core.Configuration.ISettings
{
    /// <summary>
    /// Value used to convert weight of product to troy ounce
    /// </summary>
    public decimal WeightConversion { get; set; } = 14.58332955m;
    public string ApiEndpoint { get; set; } = "https://api.metalpriceapi.com/v1/latest";
    public string ApiKey { get; set; } = "bad6c749effd9d1d8937845988089594";
    public int CartPriceLock { get; set; } = 300;
    public string GoldSymbol { get; set; }
    public string SilverSymbol { get; set; }
}