namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;

public class ConfigureViewModel
{
    public string Version { get; set; } = "0.0.0";
    public string? SaveRoute { get; set; } = PluginDefaults.SaveDynamicPriceConfigure;
}
