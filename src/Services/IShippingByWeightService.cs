using Nop.Core;
using Nop.Plugin.Shipping.ByWeightExtended.Domain;
using System.Collections;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.ByWeightExtended.Services
{
    public partial interface IShippingByWeightExtendedService
    {
        void DeleteShippingByWeightExtendedRecord(ShippingByWeightExtendedRecord ShippingByWeightExtendedRecord);

        IPagedList<ShippingByWeightExtendedRecord> GetAll(int pageIndex = 0, int pageSize = int.MaxValue);

        ShippingByWeightExtendedRecord FindRecord(int shippingMethodId,
            int storeId, int warehouseId, 
            int countryId, int stateProvinceId, string zip, decimal weight);

        ShippingByWeightExtendedRecord GetById(int ShippingByWeightExtendedRecordId);

        void InsertShippingByWeightExtendedRecord(ShippingByWeightExtendedRecord ShippingByWeightExtendedRecord);

        void UpdateShippingByWeightExtendedRecord(ShippingByWeightExtendedRecord ShippingByWeightExtendedRecord);



        /// <summary>
        /// Will remove any existing record and add the new ones
        /// </summary>
        void UpdateProductMapToShippingMethodRecords(IEnumerable<ProductMapToShippingMethodRecord> ProductMapToShippingMethodRecords);

        /// <summary>
        /// List shipping methods for a product
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        IEnumerable<ProductMapToShippingMethodRecord> GetProductMapToShippingMethodForProduct(int productId);

        /// <summary>
        /// used inthe shipping calculation
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shippingMethodId"></param>
        /// <returns></returns>
        bool ProductHasFreeShipping(int productId, int shippingMethodId);

        // list All products For a shipping method (for admin screens)

        // For a product list methods that have free shipping (for use on product display page)

    }
}
