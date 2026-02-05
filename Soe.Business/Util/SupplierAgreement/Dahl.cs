using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Dahl : ISupplierAgreement
    {
        public List<DahlDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Dahl; } }

        public Dahl()
        {
            supplierAgreements = new List<DahlDiscount>();
        }

        #region Public methods

        public ActionResult Read(Stream stream)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Constants.ENCODING_IBM437);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(50);
                try
                {
                    supplierAgreements.Add(new DahlDiscount(line));
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
            //var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.Dahl); 
            foreach (DahlDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class DahlDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public DahlDiscount()
        {
            //Empty constructor for unit testing
        }

        public DahlDiscount(string item)
        {
            MaterialClass = item.Substring(0, 6);
            Name = item.Substring(9, 30);
            Discount = ToDecimal(item.Substring(39, 4));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            var asInt = Convert.ToInt32(item);
            return Convert.ToDecimal(asInt / 10M);
        }

        #endregion
    }
}
