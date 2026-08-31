using System.ComponentModel.DataAnnotations;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Enums;

public enum DynamicPriceModifierType
{
    None,
    Percentage,
    [Display(Name = "Absolute Value")]
    CostPlus
}
