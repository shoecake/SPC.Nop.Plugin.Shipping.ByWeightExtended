using Nop.Core;

namespace Nop.Plugin.Shipping.ByWeightExtended.Domain
{
    public partial class ProductMapToShippingMethodRecord : BaseEntity
    {
        public int productId { get; set; }

        public int shippingMethodId { get; set; }

        public bool freeShipping { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string shippingMethodName { get; set; }
    }
}
