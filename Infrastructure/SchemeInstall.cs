using FluentMigrator;
using i7MEDIA.Plugin.Misc.Dynamic.Pricing.Data;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;

namespace i7MEDIA.Plugin.Misc.Dynamic.Pricing.Infrastructure;

[NopMigration("2026/07/26 00:00:00", "Widgets.Dynamic.Price base schema", MigrationProcessType.Installation)]
public class SchemeInstall : Migration
{
    private readonly string _dynamicProductPricing = NameCompatibilityManager.GetTableName(typeof(DynamicProductPricing));
    private readonly string _metalTypes = NameCompatibilityManager.GetTableName(typeof(DynamicPricingMetalType));
    private readonly string _dynamicPriceRoles = NameCompatibilityManager.GetTableName(typeof(DynamicPriceRoleMapping));

    public override void Up()
    {
        if (!Schema.Table(_dynamicProductPricing).Exists())
        {
            Create.TableFor<DynamicProductPricing>();

            //Create.Index("idx_dynamic_price_product_id")
            //.OnTable(nameof(DynamicPricing))
            //.OnColumn(nameof(DynamicPricing.ProductId));

            //Create.Index("idx_dynamic_price_product_id")
            //.OnTable(nameof(DynamicPricing))
            //.OnColumn(nameof(DynamicPricing.MetalTypeId));

        }

        if (!Schema.Table(_metalTypes).Exists())
        {
            Create.TableFor<DynamicPricingMetalType>();
        }

        if (!Schema.Table(_dynamicPriceRoles).Exists())
        {
            Create.TableFor<DynamicPriceRoleMapping>();
        }
    }

    public override void Down()
    {
#if DEBUG
        if (Schema.Table(_dynamicProductPricing).Exists())
        {
            Delete.Table(_dynamicProductPricing);
        }
#endif
    }
}
