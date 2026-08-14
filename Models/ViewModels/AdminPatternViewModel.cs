using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;

public class AdminPatternViewModel
{
    public required string Version { get; set; }
    public int PatternId { get; set; }

    [NopResourceDisplayName("Admin.Dynamic.Price.Label.Metal.Type")]
    public int SelectedMetalType { get; set; }

    [NopResourceDisplayName("Admin.Dynamic.Price.Label.Price.Modifier")]
    public decimal PriceModifier { get; set; }

    [NopResourceDisplayName("Admin.Dynamic.Price.Label.Price.Modifier.Type")]
    public int PriceModifierType { get; set; }

    public IList<SelectListItem> AvailableMetalTypes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> PriceModifierTypes { get; set; } = new List<SelectListItem>();
}