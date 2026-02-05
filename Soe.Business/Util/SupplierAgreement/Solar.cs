using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Solar : ISupplierAgreement, IFileValidator<ISupplierAgreement>
    {
        public List<SolarDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Solar; } }

        public Solar()
        {
            supplierAgreements = new List<SolarDiscount>();
        }

        #region Public methods

        public ActionResult Read(Stream stream)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_LATIN1);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(65);
                try
                {
                    supplierAgreements.Add(new SolarDiscount(line));
                }
                catch (Exception ex) //hide error
                {
                    ex.ToString(); //prevent compiler warning
                }
            }
            return result;
        }
        public GenericProvider ToGeneric()
        {
            var result = new GenericProvider();
            //var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.Solar); 
            foreach (SolarDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        public ActionResult ValidateFile(Stream stream)
        {
            var result = SolarCSV.ValidateSolarCSVFile(stream);
            // Invert result
            result.Success = !result.Success;
            return result;
        }

        public ISupplierAgreement GetSecondaryProvider()
        {
            return new SolarCSV(SoeSupplierAgreementProvider.Solar);
        }
        #endregion
    }

    public class SolarDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }
        public string Type { get; set; }

        #endregion

        #region Constructors

        public SolarDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public SolarDiscount(string item)
        {
            MaterialClass = item.Substring(8, 4);
            Name = item.Substring(27, 31);
            Discount = ToDecimal(item.Substring(59, 4));
            Type = item.Substring(4, 1);
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            int asInt = Convert.ToInt32(item);
            return Convert.ToDecimal(asInt / 10M);
            //return Convert.ToDecimal(asInt);
        }

        #endregion
    }
}
