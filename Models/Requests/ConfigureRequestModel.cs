namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.Requests;

public class ConfigureRequestModel
{
    public required string ApiEndpoint { get; set; }
    public required string ApiKey { get; set; }
    public decimal WeightConversion { get; set; }
    public int CartPriceLock { get; set; }
}