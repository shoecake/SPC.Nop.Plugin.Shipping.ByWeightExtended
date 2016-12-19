using Nop.Plugin.Shipping.ByWeightExtended.Controllers;
using Nop.Plugin.Shipping.ByWeightExtended.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Shipping.ByWeightExtended.Events
{
    class ProductAdminConsumer : IConsumer<AdminTabStripCreated>
    {
        private readonly IProductService _productService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IShippingByWeightExtendedService _ShippingByWeightExtendedService;

        public ProductAdminConsumer(IProductService productService, ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService, IPermissionService permissionService, IShippingByWeightExtendedService ShippingByWeightExtendedService)
        {
            this._productService = productService;
            this._localizationService = localizationService;
            this._genericAttributeService = genericAttributeService;
            this._permissionService = permissionService;
            this._ShippingByWeightExtendedService = ShippingByWeightExtendedService;
        }

        public void HandleEvent(AdminTabStripCreated eventMessage)
        {
            if (eventMessage.TabStripName == "product-edit")
            {

                ProductAdminController controller = new ProductAdminController(_productService, _genericAttributeService, _permissionService, _ShippingByWeightExtendedService);
                int productId = Convert.ToInt32(HttpContext.Current.Request.RequestContext.RouteData.Values["id"]);
                var actionName = "GetShippingList";
                var controllerName = "ProductAdmin";
                var routeValues = new RouteValueDictionary()
                                  {
                                      {"Namespaces", "Nop.Plugin.Shipping.ByWeightExtended.Controllers"},
                                      {"area", null},
                                      {"id", productId}
                                  };
                var urlHelper = new UrlHelper(eventMessage.Helper.ViewContext.RequestContext).Action(actionName, controllerName, routeValues);

                StringBuilder sb = new StringBuilder();
                sb.Append("<script>");
                sb.Append("$(document).ready(function() {");
                sb.Append("$('#product-edit > .nav-tabs').append('<li><a data-tab-name=\"spc-shipping-options\" data-toggle=\"tab\" href=\"#spc-shipping-options\">Shipping Options</a></li>');");
                sb.Append("$('#product-edit > .tab-content').append('<div class=\"tab-pane\" id=\"spc-shipping-options\"> Loading...</div>');});");
                sb.Append("$(document).one('click', 'a[data-tab-name=spc-shipping-options]', function() {$(this).tab('show');");
                sb.Append("$.ajax({async: true,cache: false,type: \"GET\",url:\"" + urlHelper + "\"}).done(function(data) {$('#spc-shipping-options').html(data);}); });");
                sb.Append("</script>");
                eventMessage.BlocksToRender.Add(new MvcHtmlString(sb.ToString()));

                //int id = Convert.ToInt32(System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["ID"]);
                //string urlHelper = new UrlHelper(eventMessage.Helper.ViewContext.RequestContext).Action("GetShippingList", "ProductAdmin",
                //new RouteValueDictionary() { { "area", "" }, { "Id", id } });
                //string script = string.Format(@"<script> $(document).ready(function(){{$('#{0}')
                //            .data('kendoTabStrip').append([{{text:""<b>{1}</b>"", encoded: false, contentUrl:'{2}'}}]);}}); </script>",
                //           eventMessage.TabStripName, "Shipping Options", urlHelper);
                //eventMessage.BlocksToRender.Add(new MvcHtmlString(script));

                // on product save, add/delete a productSpecification, so SevenSpikes ribbons can be used

                // free shipping should be by Royal Mail/Airmail etc an dnot by primary country

                // only show once product has been saved
            }
        }
    }
}

