using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByWeightExtended.Models
{
    public class ShippingByWeightListModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Shipping.ByWeightExtended.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }
    }
}