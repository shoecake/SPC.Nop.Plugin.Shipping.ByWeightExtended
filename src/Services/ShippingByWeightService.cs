using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Shipping.ByWeightExtended.Domain;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.ByWeightExtended.Services
{
    public partial class ShippingByWeightExtendedService : IShippingByWeightExtendedService

    {
        #region Constants
        private const string SHIPPINGBYWEIGHT_ALL_KEY = "Nop.shippingbyweightextended.all-{0}-{1}";
        private const string SHIPPINGBYWEIGHT_FREESHIP_KEY = "Nop.shippingbyweightextended.freeship-{0}-{1}";
        private const string SHIPPINGBYWEIGHT_PATTERN_KEY = "Nop.shippingbyweightextended.";
        #endregion

        #region Fields

        private readonly IRepository<ShippingByWeightExtendedRecord> _sbwRepository;
        private readonly IRepository<ProductMapToShippingMethodRecord> _pMapRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IShippingService _shippingService;
        #endregion

        #region Ctor

      public ShippingByWeightExtendedService(ICacheManager cacheManager,
            IRepository<ShippingByWeightExtendedRecord> sbwRepository, 
            IRepository<ProductMapToShippingMethodRecord> pMapRepository,
            IShippingService shippingService
          )
        {
            this._cacheManager = cacheManager;
            this._sbwRepository = sbwRepository;
            this._pMapRepository = pMapRepository;
            this._shippingService = shippingService;
        }

        #endregion

        #region Methods

        public virtual void DeleteShippingByWeightExtendedRecord(ShippingByWeightExtendedRecord ShippingByWeightExtendedRecord)
        {
            if (ShippingByWeightExtendedRecord == null)
                throw new ArgumentNullException("ShippingByWeightExtendedRecord");

            _sbwRepository.Delete(ShippingByWeightExtendedRecord);

            _cacheManager.RemoveByPattern(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        public virtual IPagedList<ShippingByWeightExtendedRecord> GetAll(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = string.Format(SHIPPINGBYWEIGHT_ALL_KEY, pageIndex, pageSize);
            return _cacheManager.Get(key, () =>
            {
                var query = from sbw in _sbwRepository.Table
                            orderby sbw.StoreId, sbw.CountryId, sbw.StateProvinceId, sbw.Zip, sbw.ShippingMethodId, sbw.From
                            select sbw;

                var records = new PagedList<ShippingByWeightExtendedRecord>(query, pageIndex, pageSize);
                return records;
            });
        }

        public virtual ShippingByWeightExtendedRecord FindRecord(int shippingMethodId,
            int storeId, int warehouseId, 
            int countryId, int stateProvinceId, string zip, decimal weight)
        {
            if (zip == null)
                zip = string.Empty;
            zip = zip.Trim();

            //filter by weight and shipping method
            var existingRates = GetAll()
                .Where(sbw => sbw.ShippingMethodId == shippingMethodId && weight >= sbw.From && weight <= sbw.To)
                .ToList();

            //filter by store
            var matchedByStore = new List<ShippingByWeightExtendedRecord>();
            foreach (var sbw in existingRates)
                if (storeId == sbw.StoreId)
                    matchedByStore.Add(sbw);
            if (!matchedByStore.Any())
                foreach (var sbw in existingRates)
                    if (sbw.StoreId == 0)
                        matchedByStore.Add(sbw);

            //filter by warehouse
            var matchedByWarehouse = new List<ShippingByWeightExtendedRecord>();
            foreach (var sbw in matchedByStore)
                if (warehouseId == sbw.WarehouseId)
                    matchedByWarehouse.Add(sbw);
            if (!matchedByWarehouse.Any())
                foreach (var sbw in matchedByStore)
                    if (sbw.WarehouseId == 0)
                        matchedByWarehouse.Add(sbw);

            //filter by country
            var matchedByCountry = new List<ShippingByWeightExtendedRecord>();
            foreach (var sbw in matchedByWarehouse)
                if (countryId == sbw.CountryId)
                    matchedByCountry.Add(sbw);
            if (!matchedByCountry.Any())
                foreach (var sbw in matchedByWarehouse)
                    if (sbw.CountryId == 0)
                        matchedByCountry.Add(sbw);

            //filter by state/province
            var matchedByStateProvince = new List<ShippingByWeightExtendedRecord>();
            foreach (var sbw in matchedByCountry)
                if (stateProvinceId == sbw.StateProvinceId)
                    matchedByStateProvince.Add(sbw);
            if (!matchedByStateProvince.Any())
                foreach (var sbw in matchedByCountry)
                    if (sbw.StateProvinceId == 0)
                        matchedByStateProvince.Add(sbw);


            //filter by zip
            var matchedByZip = new List<ShippingByWeightExtendedRecord>();
            foreach (var sbw in matchedByStateProvince)
                if ((String.IsNullOrEmpty(zip) && String.IsNullOrEmpty(sbw.Zip)) ||
                    (zip.Equals(sbw.Zip, StringComparison.InvariantCultureIgnoreCase)))
                    matchedByZip.Add(sbw);

            if (!matchedByZip.Any())
                foreach (var taxRate in matchedByStateProvince)
                    if (String.IsNullOrWhiteSpace(taxRate.Zip))
                        matchedByZip.Add(taxRate);

            return matchedByZip.FirstOrDefault();
        }

        public virtual ShippingByWeightExtendedRecord GetById(int ShippingByWeightExtendedRecordId)
        {
            if (ShippingByWeightExtendedRecordId == 0)
                return null;

            return _sbwRepository.GetById(ShippingByWeightExtendedRecordId);
        }

        public virtual void InsertShippingByWeightExtendedRecord(ShippingByWeightExtendedRecord ShippingByWeightExtendedRecord)
        {
            if (ShippingByWeightExtendedRecord == null)
                throw new ArgumentNullException("ShippingByWeightExtendedRecord");

            _sbwRepository.Insert(ShippingByWeightExtendedRecord);

            _cacheManager.RemoveByPattern(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        public virtual void UpdateShippingByWeightExtendedRecord(ShippingByWeightExtendedRecord ShippingByWeightExtendedRecord)
        {
            if (ShippingByWeightExtendedRecord == null)
                throw new ArgumentNullException("ShippingByWeightExtendedRecord");

            _sbwRepository.Update(ShippingByWeightExtendedRecord);

            _cacheManager.RemoveByPattern(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }





        public void UpdateProductMapToShippingMethodRecords(IEnumerable<ProductMapToShippingMethodRecord> ProductMapToShippingMethodRecords)
        {
            if (ProductMapToShippingMethodRecords == null)
                throw new ArgumentNullException("ProductMapToShippingMethodRecords");

            foreach(var item in ProductMapToShippingMethodRecords)
            {
                // delete any existing record
                var query = from pMap in _pMapRepository.Table
                            where pMap.productId == item.productId & pMap.shippingMethodId == item.shippingMethodId
                            select pMap;

                if(query.Count() > 0)
                {
                    _pMapRepository.Delete(query);
                }

                _pMapRepository.Insert(item);
            }

            
            _cacheManager.RemoveByPattern(SHIPPINGBYWEIGHT_PATTERN_KEY);
        }

        public IEnumerable<ProductMapToShippingMethodRecord> GetProductMapToShippingMethodForProduct(int productId)
        {
            // if no record, then add one

            var result = new List<ProductMapToShippingMethodRecord>();

            var query = from pMap in _pMapRepository.Table
                        where pMap.productId == productId
                        select pMap;

            var src = query.ToDictionary(x=>x.shippingMethodId);
            var shipMethods = _shippingService.GetAllShippingMethods();

            foreach(var item in shipMethods)
            {
                if(src.ContainsKey(item.Id))
                {
                    src[item.Id].shippingMethodName = item.Name;
                    result.Add(src[item.Id]);
                }
                else
                {
                    var newItem = new ProductMapToShippingMethodRecord()
                    {
                        shippingMethodId = item.Id,
                        shippingMethodName = item.Name,
                        productId = productId,
                        freeShipping = false
                    };
                    result.Add(newItem);
                }
            }
            return result;
        }

        public bool ProductHasFreeShipping(int productId, int shippingMethodId)
        {
            string key = string.Format(SHIPPINGBYWEIGHT_FREESHIP_KEY, productId, shippingMethodId);
            return _cacheManager.Get(key, () =>
            {
                var query = from pMap in _pMapRepository.Table
                            where pMap.productId == productId & pMap.shippingMethodId == shippingMethodId
                            select pMap;

                if (query.Count() == 0)
                {
                    return false;
                }
                else
                {
                    return query.First().freeShipping;
                }

            });
            
        }

        #endregion
    }
}
