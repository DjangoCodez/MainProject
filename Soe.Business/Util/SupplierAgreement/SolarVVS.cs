using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class SolarVVS : ISupplierAgreement, IFileValidator<ISupplierAgreement>
    {
        public List<SolarVVSDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.SolarVVS; } }

        public SolarVVS()
        {
            supplierAgreements = new List<SolarVVSDiscount>();
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
                // Skip starting rows
                //if (!line.Substring(4, 1).Equals("V")) continue;
                line = line.PadRight(80);
                try
                {
                    supplierAgreements.Add(new SolarVVSDiscount(line));
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
            //var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.SolarVVS);
            foreach (SolarVVSDiscount post in supplierAgreements)
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
            return new SolarCSV(SoeSupplierAgreementProvider.SolarVVS);
        }
        #endregion
    }

    public class SolarVVSDiscount
    {
        #region Variables

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Ctor

        public SolarVVSDiscount()
        {
            //Empty constructor to mock object in unit test
        }

        public SolarVVSDiscount(string item)
        {
            //MaterialClass = item.Substring(27, 4); 
            MaterialClass = item.Substring(12, 4);
            Name = item.Substring(20, 35);
            Discount = ToDecimal(item.Substring(66, 4));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(",", "").Replace(".", "");
            var asInt = Convert.ToInt32(item);
            return Convert.ToDecimal(asInt / 10M);
        }
        #endregion
    }
}
