using Nop.Core;
using Nop.Core.Domain.Common;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;

public class DynamicPricingMetalType : BaseEntity, ISoftDeletedEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal CurrentValue { get; set; }
    public bool Deleted { get; set; }
}