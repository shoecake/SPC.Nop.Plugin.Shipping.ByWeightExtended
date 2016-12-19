using System;
using System.Collections.Generic;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;


// used by ProductAdmin Views on the Product page
namespace Nop.Plugin.Shipping.ByWeightExtended.Models
{
    public class ProductShippingByProductModel : BaseNopEntityModel
    {
        // will be a grid of Shipping methods

        public int productId { get; set; }

        public List<ProductShippingByProductMethodsModel> shippingMethods { get; set; }
    }


    public class ProductShippingByProductMethodsModel
    {
        public int shippingMethodId { get; set; }

        public string shippingMethodName { get; set; }

        public bool freeShipping { get; set; }
    }

}
