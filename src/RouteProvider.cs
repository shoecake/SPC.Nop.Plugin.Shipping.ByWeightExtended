using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Shipping.ByWeightExtended
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.ByWeightExtended.SaveGeneralSettings",
                 "Plugins/ShippingByWeightExtended/SaveGeneralSettings",
                 new { controller = "ShippingByWeightExtended", action = "SaveGeneralSettings", },
                 new[] { "Nop.Plugin.Shipping.ByWeightExtended.Controllers" }
            );

            routes.MapRoute("Plugin.Shipping.ByWeightExtended.AddPopup",
                 "Plugins/ShippingByWeightExtended/AddPopup",
                 new { controller = "ShippingByWeightExtended", action = "AddPopup" },
                 new[] { "Nop.Plugin.Shipping.ByWeightExtended.Controllers" }
            );
            routes.MapRoute("Plugin.Shipping.ByWeightExtended.EditPopup",
                 "Plugins/ShippingByWeightExtended/EditPopup",
                 new { controller = "ShippingByWeightExtended", action = "EditPopup" },
                 new[] { "Nop.Plugin.Shipping.ByWeightExtended.Controllers" }
            );


            //var route = routes.MapRoute("Plugin.Shipping.ByWeightExtended.ProductAdmin",
            //      "Admin/ProductAdmin/GetShippingList",
            //      new { controller = "ProductAdmin", action = "GetShippingList" },
            //      new[] { "Nop.Plugin.Shipping.ByWeightExtended.Controllers" }
            // );

            //route.DataTokens.Add("area", "Admin");

        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }

    //public class PluginAdminAreaRegistration : AreaRegistration
    //{
    //    public override string AreaName
    //    {
    //        get
    //        {
    //            return "Admin";
    //        }
    //    }

    //    public override void RegisterArea(AreaRegistrationContext context)
    //    {
    //        //put overridden routes for admin area
    //        context.MapRoute("Plugin.Shipping.ByWeightExtended.ProductAdmin",
    //            "Admin/ProductAdmin/GetShippingList",
    //            new { controller = "ProductAdmin", action = "GetShippingList" },
    //            new[] { "Nop.Plugin.Shipping.ByWeightExtended.Controllers" });
    //    }
    //}

}
