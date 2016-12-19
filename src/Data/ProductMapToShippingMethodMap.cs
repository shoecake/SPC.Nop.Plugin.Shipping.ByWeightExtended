using Nop.Data.Mapping;
using Nop.Plugin.Shipping.ByWeightExtended.Domain;

namespace Nop.Plugin.Shipping.ByWeightExtended.Data
{
    public partial class ProductMapToShippingMethodMap : NopEntityTypeConfiguration<ProductMapToShippingMethodRecord>
    {
        public ProductMapToShippingMethodMap()
        {
            this.ToTable("ShippingByWeightExtendedProductMapToShippingMethod");
            this.HasKey(x => x.Id);

         //   this.Property(x => x.Zip).HasMaxLength(400);
        }
    }
}