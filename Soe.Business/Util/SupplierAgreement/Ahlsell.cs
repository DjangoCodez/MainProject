using SoftOne.Soe.Common.DTO.CustomerInvoice;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Ahlsell : ISupplierAgreementWithNetPrices
    {
        private readonly List<AhlsellDiscount> supplierAgreements;
        private readonly List<AhlsellDiscount> supplierNetPrices;

        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Ahlsell; } }
        public bool HasNetPrice { get { return true; } }
        public SoeWholeseller SysWholeSeller { get { return SoeWholeseller.Ahlsell; } }

        public Ahlsell()
        {
            supplierAgreements = new List<AhlsellDiscount>();
            supplierNetPrices = new List<AhlsellDiscount>();
        }

        public ActionResult Read(Stream stream)
        {
            var sr = new StreamReader(stream, Constants.ENCODING_LATIN1);

            //First line is special....
            sr.ReadLine();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(80);

                try
                {
                    var item = new AhlsellDiscount(line);

                    if (item.Type == SoeSupplierAgreemntCodeType.Product && item.NetPrice != 0)
                    {
                        supplierNetPrices.Add(item);
                    }
                    else
                    {
                        supplierAgreements.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    return new ActionResult(ex.Message + "\n" + line ?? "");
                }
            }

            return new ActionResult(true);
        }

        public GenericProvider ToGeneric()
        {
            var result = new GenericProvider();
            //var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.Ahlsell); 
            foreach (AhlsellDiscount post in supplierAgreements)
            {
                var agreement = new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount);
                if (post.Type == SoeSupplierAgreemntCodeType.Product)
                {
                    agreement.Code = post.ProductNr;
                    agreement.CodeType = SoeSupplierAgreemntCodeType.Product;
                }
                result.supplierAgreements.Add(agreement);
            }
            return result;
        }

        public List<WholsellerNetPriceRowDTO> ToNetPrices()
        {
            return supplierNetPrices.Select(x => new WholsellerNetPriceRowDTO
            {
                ProductNr = x.ProductNr,
                NetPrice = x.NetPrice
            }).ToList();
        }
    }

    public class AhlsellDiscount
    {
        #region Variables

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }
        public string ProductNr { get; set; }
        public decimal NetPrice { get; set; }
        public SoeSupplierAgreemntCodeType Type { get; set; }

        #endregion

        #region Ctor

        public AhlsellDiscount()
        {
            //Empty constructor for mocking in unittesting
        }

        public AhlsellDiscount(string item)
        {
            
            ProductNr = item.Substring(10, 20).TrimEnd();
            if (!string.IsNullOrEmpty(ProductNr))
            {
                NetPrice = ToDecimal(item.Substring(44, 9),100M);
                Type = SoeSupplierAgreemntCodeType.Product;
                Discount = ToDecimal(item.Substring(40, 4), 10M);
            }
            else
            {
                var discountString = item.Substring(36, 4);
                MaterialClass = item.Substring(30, 6).TrimEnd();
                if (!discountString.Contains("-"))
                {
                    Discount = ToDecimal(discountString, 10M);
                }
                
                Name = item.Substring(57, 30).TrimEnd();
                Type = SoeSupplierAgreemntCodeType.MaterialCode;
            }
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item, decimal divideWith)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            var asInt = Convert.ToInt32(item);
            return Convert.ToDecimal(asInt / divideWith);
        }
        
        #endregion
    }
}
