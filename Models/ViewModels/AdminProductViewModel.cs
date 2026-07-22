using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Models.ViewModels;

public class AdminProductViewModel()
{
    public int ProductId { get; set; }
    [NopResourceDisplayName("admin.dynamic.price.label.base.price")]
    public decimal BasePrice { get; set; }
    [NopResourceDisplayName("Admin.Dynamic.Price.Label.Metal.Type")]
    public int SelectedMetalType { get; set; }
    public IList<SelectListItem> AvailableMetalTypes { get; set; } = new List<SelectListItem>();
}