using Nop.Web.Framework.Mvc.ModelBinding;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;

public class ConfigureViewModel
{
    public string Version { get; set; } = "0.0.0";
    public string? SaveRoute { get; set; } = PluginDefaults.SaveDynamicPriceConfigure;
    [NopResourceDisplayName("admin.dynamic.price.configure.label.weight.conversion")]
    public decimal WeightConversion { get; set; }
    [NopResourceDisplayName("admin.dynamic.price.configure.api.key")]
    public string ApiKey { get; set; } = "";
    [NopResourceDisplayName("admin.dynamic.price.configure.api.end.point")]
    public string ApiEndpoint { get; set; } = "";
    [NopResourceDisplayName("admin.dynamic.price.configure.cart.lock")]
    public int CartPriceLock { get; set; }
}