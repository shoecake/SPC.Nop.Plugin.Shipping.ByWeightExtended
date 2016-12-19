using System;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.ByWeightExtended.Data;
using Nop.Plugin.Shipping.ByWeightExtended.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.ByWeightExtended
{
    public class ByWeightShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly IStoreContext _storeContext;
        private readonly IShippingByWeightExtendedService _ShippingByWeightExtendedService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ShippingByWeightExtendedSettings _shippingByWeightSettings;
        private readonly ShippingByWeightExtendedObjectContext _objectContext;
        private readonly ProductMapToShippingMethodObjectContext _prodMapObjectContext;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor
        public ByWeightShippingComputationMethod(IShippingService shippingService,
            IStoreContext storeContext,
            IShippingByWeightExtendedService ShippingByWeightExtendedService,
            IPriceCalculationService priceCalculationService,
            ShippingByWeightExtendedSettings shippingByWeightSettings,
            ShippingByWeightExtendedObjectContext objectContext,
            ProductMapToShippingMethodObjectContext prodMapobjectContext,
            ISettingService settingService)
        {
            this._shippingService = shippingService;
            this._storeContext = storeContext;
            this._ShippingByWeightExtendedService = ShippingByWeightExtendedService;
            this._priceCalculationService = priceCalculationService;
            this._shippingByWeightSettings = shippingByWeightSettings;
            this._objectContext = objectContext;
            this._prodMapObjectContext = prodMapobjectContext;
            this._settingService = settingService;
        }
        #endregion

        #region Utilities
        
        private decimal? GetRate(decimal subTotal, decimal weight, int shippingMethodId,
            int storeId, int warehouseId, int countryId, int stateProvinceId, string zip)
        {
            var ShippingByWeightExtendedRecord = _ShippingByWeightExtendedService.FindRecord(shippingMethodId,
                storeId, warehouseId, countryId, stateProvinceId, zip, weight);
            if (ShippingByWeightExtendedRecord == null)
            {
                if (_shippingByWeightSettings.LimitMethodsToCreated)
                    return null;
                
                return decimal.Zero;
            }

            //additional fixed cost
            decimal shippingTotal = ShippingByWeightExtendedRecord.AdditionalFixedCost;
            //charge amount per weight unit
            if (ShippingByWeightExtendedRecord.RatePerWeightUnit > decimal.Zero)
            {
                var weightRate = weight - ShippingByWeightExtendedRecord.LowerWeightLimit;
                if (weightRate < decimal.Zero)
                    weightRate = decimal.Zero;
                shippingTotal += ShippingByWeightExtendedRecord.RatePerWeightUnit * weightRate;
            }
            //percentage rate of subtotal
            if (ShippingByWeightExtendedRecord.PercentageRateOfSubtotal > decimal.Zero)
            {
                shippingTotal += Math.Round((decimal)((((float)subTotal) * ((float)ShippingByWeightExtendedRecord.PercentageRateOfSubtotal)) / 100f), 2);
            }

            if (shippingTotal < decimal.Zero)
                shippingTotal = decimal.Zero;
            return shippingTotal;
        }
        
        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException("getShippingOptionRequest");

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || !getShippingOptionRequest.Items.Any())
            {
                response.AddError("No shipment items");
                return response;
            }
            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            var storeId = getShippingOptionRequest.StoreId;
            if (storeId == 0)
                storeId = _storeContext.CurrentStore.Id;
            int countryId = getShippingOptionRequest.ShippingAddress.CountryId.HasValue ? getShippingOptionRequest.ShippingAddress.CountryId.Value : 0;
            int stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId.HasValue ? getShippingOptionRequest.ShippingAddress.StateProvinceId.Value : 0;
            int warehouseId = getShippingOptionRequest.WarehouseFrom != null ? getShippingOptionRequest.WarehouseFrom.Id : 0;
            string zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;
            
         

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);
            foreach (var shippingMethod in shippingMethods)
            {
                // SPC - Calc weight per shipping method
                decimal subTotal = decimal.Zero;
                decimal weightToSubtract = decimal.Zero;
                foreach (var packageItem in getShippingOptionRequest.Items)
                {
                    // this is the original code which is wrong, its cacluating a price total, and not the weight total.
                    // so the weight of the free item is still part of the calculation
                    // left here for backwards compatibility
                    if (packageItem.ShoppingCartItem.IsFreeShipping)
                        continue;

                    //TODO we should use getShippingOptionRequest.Items.GetQuantity() method to get subtotal
                    subTotal += _priceCalculationService.GetSubTotal(packageItem.ShoppingCartItem);


                    // SPC - add free ship by shipping method check
                    if (_ShippingByWeightExtendedService.ProductHasFreeShipping(packageItem.ShoppingCartItem.ProductId, shippingMethod.Id))
                    {
                        // product options have weight also?
                        weightToSubtract = weightToSubtract + (packageItem.ShoppingCartItem.Product.Weight * packageItem.ShoppingCartItem.Quantity);
                    }

                }
                decimal weight = _shippingService.GetTotalWeight(getShippingOptionRequest);
                weight = weight - weightToSubtract;

                decimal? rate = GetRate(subTotal, weight, shippingMethod.Id,
                    storeId, warehouseId, countryId, stateProvinceId, zip);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.GetLocalized(x => x.Name);
                    shippingOption.Description = shippingMethod.GetLocalized(x => x.Description);
                    shippingOption.Rate = rate.Value;
                    response.ShippingOptions.Add(shippingOption);
                }
            }


            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingByWeightExtended";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Shipping.ByWeightExtended.Controllers" }, { "area", null } };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new ShippingByWeightExtendedSettings
            {
                LimitMethodsToCreated = false,
            };
            _settingService.SaveSetting(settings);


            //database objects
            _objectContext.Install();
            _prodMapObjectContext.Install();

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Store", "Store");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Store.Hint", "If an asterisk is selected, then this shipping rate will apply to all stores.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Warehouse", "Warehouse");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Warehouse.Hint", "If an asterisk is selected, then this shipping rate will apply to all warehouses.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Country", "Country");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Country.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.StateProvince", "State / province");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.StateProvince.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers from the given country, regardless of the state.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Zip", "Zip");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Zip.Hint", "Zip / postal code. If zip is empty, then this shipping rate will apply to all customers from the given country or state, regardless of the zip code.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.ShippingMethod", "Shipping method");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.ShippingMethod.Hint", "The shipping method.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.From", "Order weight from");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.From.Hint", "Order weight from.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.To", "Order weight to");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.To.Hint", "Order weight to.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.AdditionalFixedCost", "Additional fixed cost");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.AdditionalFixedCost.Hint", "Specify an additional fixed cost per shopping cart for this option. Set to 0 if you don't want an additional fixed cost to be applied.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LowerWeightLimit", "Lower weight limit");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LowerWeightLimit.Hint", "Lower weight limit. This field can be used for \"per extra weight unit\" scenarios.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.PercentageRateOfSubtotal", "Charge percentage (of subtotal)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.PercentageRateOfSubtotal.Hint", "Charge percentage (of subtotal).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.RatePerWeightUnit", "Rate per weight unit");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.RatePerWeightUnit.Hint", "Rate per weight unit.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they've not configured here (zero shipping fee in this case).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.DataHtml", "Data");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.AddRecord", "Add record");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Formula", "Formula to calculate rates");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Formula.Value", "[additional fixed cost] + ([order total weight] - [lower weight limit]) * [rate per weight unit] + [order subtotal] * [charge percentage]");
            
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ShippingByWeightExtendedSettings>();

            //database objects
            _objectContext.Uninstall();
            _prodMapObjectContext.Uninstall();

            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Store");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Store.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Warehouse");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Warehouse.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Country");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Country.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.StateProvince");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.StateProvince.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Zip");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.Zip.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.ShippingMethod");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.ShippingMethod.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.From");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.From.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.To");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.To.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.AdditionalFixedCost");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.AdditionalFixedCost.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LowerWeightLimit");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LowerWeightLimit.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.PercentageRateOfSubtotal");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.PercentageRateOfSubtotal.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.RatePerWeightUnit");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.RatePerWeightUnit.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LimitMethodsToCreated");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.LimitMethodsToCreated.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Fields.DataHtml");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.AddRecord");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Formula");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByWeightExtended.Formula.Value");
            
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }


        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get
            {
                //uncomment a line below to return a general shipment tracker (finds an appropriate tracker by tracking number)
                //return new GeneralShipmentTracker(EngineContext.Current.Resolve<ITypeFinder>());
                return null; 
            }
        }

        #endregion
    }
}
