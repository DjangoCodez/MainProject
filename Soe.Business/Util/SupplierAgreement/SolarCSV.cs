using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO.CustomerInvoice;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class SolarCSV : CSVProviderBase, IFileValidator<IPriceListProvider>
    {
        private readonly SoeSupplierAgreementProvider providerType;
        private readonly List<WholsellerNetPriceRowDTO> supplierNetPrices = new List<WholsellerNetPriceRowDTO>();

        private enum SolarColumnPositions
        {
            Code = 0,
            DiscountGroup = 1,
            ArticleNr = 2,
            RSKNumber = 3,
            Description = 4,
            Discount = 5,
            AdditionalDiscount = 6,
            NetPrice = 7,
            MaterialDiscount = 8,
        }
        public override SoeSupplierAgreementProvider Provider { get { return providerType; } }
        public override bool HasNetPrice{ get { return true; } }
        public override SoeWholeseller SysWholeSeller { get { return SoeWholeseller.Solar; } }

        public SolarCSV(SoeSupplierAgreementProvider provider)
        {
            this.providerType = provider;
            //this.SysPriceListManager = new SysPriceListManager(null);
            int sysWholsellerId = 0;
            var wm = new WholeSellerManager(null);
            wm.TryGetSysWholesellerIdByName(this.WholesellerName, ref sysWholsellerId);
            //SysWholsellerId = sysWholsellerId;
        }

        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string[] columns)
        {
            var product = new GenericSupplierAgreement();

            switch (columns[(int)SolarColumnPositions.Code].ToUpper())
            {
                case "V":
                    product.CodeType = SoeSupplierAgreemntCodeType.MaterialCode;
                    product.Code = columns[(int)SolarColumnPositions.DiscountGroup];
                    product.Discount = Convert.ToDecimal(columns[(int)SolarColumnPositions.Discount]);
                    break;
                case "A":
                    var netPrice=ConvertToNetPrice(columns);
                    if (!netPrice)
                    {
                        product.CodeType = SoeSupplierAgreemntCodeType.Product;
                        if (this.providerType == SoeSupplierAgreementProvider.SolarVVS)
                            product.Code = columns[(int)SolarColumnPositions.RSKNumber];
                        else
                            product.Code = columns[(int)SolarColumnPositions.ArticleNr];

                        product.Discount = this.ToDecimal(columns[(int)SolarColumnPositions.MaterialDiscount]);
                    }
                    break;
                case "P":
                    // Imported in the new net price funktionality
                    ConvertToNetPrice(columns);
                    return (true, null);
                    /*
                    product.CodeType = SoeSupplierAgreemntCodeType.Product;

                    if (this.providerType == SoeSupplierAgreementProvider.SolarVVS)
                        product.Code = columns[(int)SolarColumnPositions.RSKNumber];
                    else
                        product.Code = columns[(int)SolarColumnPositions.ArticleNr];

                    var priceList = this.SysPriceListManager.GetSysPriceListByProductNumber(columns[(int)SolarColumnPositions.ArticleNr], this.SysWholsellerId);
                    var netPrice = this.ToDecimal(columns[(int)SolarColumnPositions.NetPrice]);
                    product.Discount = Math.Round(((priceList.GNP / netPrice) - 1), 4);
                    */
                default:
                    return (false,null);
            }

            if (product.Discount > 100)
            {
                product.Discount = 0;
            }

            return (true, product);
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSysPriceListProvider), SoeSysPriceListProvider.Solar); }
        }

        public static ActionResult ValidateSolarCSVFile(System.IO.Stream stream)
        {
            ActionResult result = new ActionResult(false);

            using (var sr = new StreamReader(stream, Constants.ENCODING_LATIN1))
            {
                // Remove heading
                sr.ReadLine();
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line) && !sr.EndOfStream)
                    line = sr.ReadLine();

                // Try and see if this is a csv file
                var columns = line.Split(';');
                if (columns.Length > 7)
                {
                    switch (columns[(int)SolarColumnPositions.Code].ToLower())
                    {
                        case "v":
                        case "a":
                        case "p":
                            // Now we can be pretty sure this is Solars new csv format, so return true
                            result.Success = true;
                            return result;
                        default:
                            result.ErrorMessage = "Column was not the expected value: " + (int)SolarColumnPositions.NetPrice;
                            break;
                    }
                }
                else
                {
                    result.ErrorMessage = "Number of columns missmatch, found " + columns.Length;
                }
            }

            return result;
        }

        public ActionResult ValidateFile(Stream stream)
        {
            return ValidateSolarCSVFile(stream);
        }

        public IPriceListProvider GetSecondaryProvider()
        {
            return null;
        }

        private bool ConvertToNetPrice(string[] columns)
        {
            if (columns.Length > 7)
            {
                var kod = columns[(int)SolarColumnPositions.Code];

                if (kod == "P" || kod == "A")
                {
                    var price = new WholsellerNetPriceRowDTO();

                    if (this.providerType == SoeSupplierAgreementProvider.SolarVVS)
                        price.ProductNr = columns[(int)SolarColumnPositions.RSKNumber];
                    else
                        price.ProductNr = columns[(int)SolarColumnPositions.ArticleNr];

                    price.NetPrice = this.ToDecimal(columns[(int)SolarColumnPositions.NetPrice]);

                    if (price.NetPrice > 0)
                    {
                        supplierNetPrices.Add(price);
                        return true;    
                    }
                }
            }

            return false;
        }

        public override List<WholsellerNetPriceRowDTO> ToNetPrices()
        {
            return supplierNetPrices;
        }
    }
}
