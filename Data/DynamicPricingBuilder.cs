using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;

public class DynamicPricingBuilder : NopEntityBuilder<DynamicPricing>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        //table
        //    .WithColumn(nameof(DynamicPricing.ProductId)).AsInt32().ForeignKey<Product>()
        //    .WithColumn(nameof(DynamicPricing.MetalTypeId)).AsInt32().ForeignKey<DynamicPricingMetalType>();
    }
}
