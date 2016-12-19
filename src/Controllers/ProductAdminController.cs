using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;
using Nop.Plugin.Shipping.ByWeightExtended.Models;
using System.Web.Mvc;
using Nop.Plugin.Shipping.ByWeightExtended.Services;

namespace Nop.Plugin.Shipping.ByWeightExtended.Controllers
{
    [AdminAuthorize]
    public class ProductAdminController : BasePluginController
    {
        private IProductService _productService;
        private IGenericAttributeService _genericAttributeService;
        private IPermissionService _permissionService;
        private readonly IShippingByWeightExtendedService _ShippingByWeightExtendedService;

        public ProductAdminController(IProductService productService, 
            IGenericAttributeService genericAttributeService,
            IPermissionService permissionService,
            IShippingByWeightExtendedService ShippingByWeightExtendedService
)
        {
            this._productService = productService;
            this._genericAttributeService = genericAttributeService;
            this._permissionService = permissionService;
            this._ShippingByWeightExtendedService = ShippingByWeightExtendedService;
        }

        
        public ActionResult GetShippingList(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return null;

            var model = new ProductShippingByProductModel();



            var records = _ShippingByWeightExtendedService.GetProductMapToShippingMethodForProduct(id);
            model.shippingMethods = records.Select(x =>
            {
                var m = new ProductShippingByProductMethodsModel
                {
                   shippingMethodId = x.shippingMethodId,
                   shippingMethodName = x.shippingMethodName,
                   freeShipping = x.freeShipping
                };
                return m;
            }).ToList();

            model.productId = id;

            return PartialView("~/Plugins/SPC.Shipping.ShippingByWeightExtended/Views/ProductAdmin/Configure.cshtml", model);
        }


        [HttpPost]
        public void Save(ProductShippingByProductModel model)
        {

            IEnumerable<Domain.ProductMapToShippingMethodRecord> records = model.shippingMethods.Select(x =>
            {
                var m = new Domain.ProductMapToShippingMethodRecord
                {
                    shippingMethodId = x.shippingMethodId,
                    productId = model.productId,
                    freeShipping = x.freeShipping
                };
                return m;
            }

                );

            _ShippingByWeightExtendedService.UpdateProductMapToShippingMethodRecords(records);

        }

    }
}
