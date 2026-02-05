using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class SthlmElgross : ISupplierAgreement
    {
        public List<SthlmElgrossDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.SthlmElgross; } }

        public SthlmElgross()
        {
            supplierAgreements = new List<SthlmElgrossDiscount>();
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
                line = line.PadRight(30);
                try
                {
                    supplierAgreements.Add(new SthlmElgrossDiscount(line));
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
            //var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.SthlmElgross);
            foreach (SthlmElgrossDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class SthlmElgrossDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public SthlmElgrossDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public SthlmElgrossDiscount(string item)
        {
            MaterialClass = item.Substring(0, 4);
            Name = string.Empty;
            Discount = ToDecimal(item.Substring(11, 5));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            if (string.IsNullOrEmpty(item.Trim()))
                return 0.0M;
            return Convert.ToDecimal(item);
        }

        #endregion
    }
}
