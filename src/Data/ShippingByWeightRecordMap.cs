using Nop.Data.Mapping;
using Nop.Plugin.Shipping.ByWeightExtended.Domain;

namespace Nop.Plugin.Shipping.ByWeightExtended.Data
{
    public partial class ShippingByWeightExtendedRecordMap : NopEntityTypeConfiguration<ShippingByWeightExtendedRecord>
    {
        public ShippingByWeightExtendedRecordMap()
        {
            this.ToTable("ShippingByWeightExtended");
            this.HasKey(x => x.Id);

         //   this.Property(x => x.Zip).HasMaxLength(400);


            // add relationship to product and shippingMethod - cascade deletes 
        }
    }
}