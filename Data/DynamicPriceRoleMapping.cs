using Nop.Core;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;

public class DynamicPriceRoleMapping : BaseEntity
{
    public int RoleId { get; set; }
    public int CustomerId { get; set; }
    public int CartItemId { get; set; }
}