using SoftOne.Soe.Common.Util;
using System;
using System.Collections;
using System.Linq;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class GenericProvider
    {
        public GenericHeader header;
        public Hashtable products;
        public string WholeSellerName { get; set; }
        public TermGroup_Country SysCountry { get; private set; }

        public GenericProvider(string wholeSellerName, TermGroup_Country sysCountry = TermGroup_Country.SE)
        {
            this.SysCountry = sysCountry;
            this.WholeSellerName = wholeSellerName;
            products = new Hashtable();
            //products = new List<GenericProduct>();
        }
    }

    public class GenericHeader
    {
        #region Members
        public DateTime Date { get; set; }
        public int Version { get; set; }
        #endregion

        #region Constructors
        public GenericHeader(DateTime date)
        {
            Date = date;
        }
        public GenericHeader(DateTime date, int version)
        {
            Date = date;
            Version = version;
        }
        #endregion
    }

    public class GenericProduct
    {
        #region Members

        private string productId = string.Empty;
        public string ProductId
        {
            get
            {
                return productId;
            }
            set
            {
                productId = value.Trim();
            }
        }

        private string name = string.Empty;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value?.Trim() ?? "";
            }
        }

        public decimal Price { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal NetPrice { get; set; }

        public string PurchaseUnit { get; set; }
        public string SalesUnit { get; set; }
        public bool EnvironmentFee { get; set; }
        public bool Storage { get; set; }
        public string Manufacturer { get; set; }
        public string ExtendedInfo { get; set; }

        private string code = string.Empty;
        public string Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value == null ? "" : value.Trim();
            }
        }

        public string EAN { get; set; }
        public string ReplacesProduct { get; set; }
        public decimal PackageSizeMin { get; set; }
        public decimal PackageSize { get; set; }
        private string productLink;
        public string ProductLink
        {
            get
            {
                return this.productLink == null ? null : this.productLink.Length > 100 ? this.productLink.Take(100).ToString() : this.productLink;
            }
            set
            {
                this.productLink = value;
            }
        }
        public string WholesellerName { get; set; }
        public DateTime PriceChangeDate { get; set; }
        public SoeProductPriceStatus PriceStatus { get; set; }
        public SoeSysPriceListProviderType ProductType { get; set; }
        #endregion

        #region Constructors
        public GenericProduct()
        {
            PriceStatus = SoeProductPriceStatus.Undefined;
            ProductType = SoeSysPriceListProviderType.Unknown;
        }

        /// <summary>
        /// Used by: Eios
        /// </summary>
        /// <param name="name"></param>
        /// <param name="purchaseUnit"></param>
        /// <param name="discountGroup"></param>
        /// <param name="ean"></param>
        /// <param name="priceChangeDate"></param>
        /// <param name="wholeSellerName"></param>
        public GenericProduct(string name, decimal price, string purchaseUnit, string discountGroup, int productId, DateTime priceChangeDate, string wholeSellerName, SoeProductPriceStatus priceStatus)
        {
            Name = name;
            Price = price;
            PurchaseUnit = purchaseUnit;
            Code = discountGroup;
            ProductId = productId.ToString();
            PriceChangeDate = priceChangeDate;
            WholesellerName = wholeSellerName;
            PriceStatus = priceStatus;
        }

        /// <summary>
        ///  Used by: Ahlsell 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="name"></param>
        /// <param name="netPrice"></param>
        /// <param name="materialCode"></param>
        /// <param name="purchaseUnit"></param>
        /// <param name="environmentFee"></param>
        /// <param name="storage"></param>
        /// <param name="wholeSellerName"></param>
        public GenericProduct(string productId, string name, decimal price, string materialCode, string purchaseUnit, bool environmentFee, bool storage, string wholeSellerName, SoeProductPriceStatus priceStatus)
        {
            ProductId = productId;
            Name = name;
            Price = price;
            Code = materialCode;
            PurchaseUnit = purchaseUnit;
            EnvironmentFee = environmentFee;
            Storage = storage;
            WholesellerName = wholeSellerName;
            PriceStatus = priceStatus;
        }
        #endregion
    }
}
