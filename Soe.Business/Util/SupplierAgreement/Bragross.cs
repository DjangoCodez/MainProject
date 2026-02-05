using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Bragross : ISupplierAgreement
    {
        public List<BragrossDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Bragross; } }
        public Bragross()
        {
            supplierAgreements = new List<BragrossDiscount>();
        }

        #region Public methods

        public ActionResult Read(Stream stream)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.IndexOf("Rabatt") > -1) continue;
                line = line.PadRight(30);
                try
                {
                    supplierAgreements.Add(new BragrossDiscount(line));
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
            foreach (BragrossDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class BragrossDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public BragrossDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public BragrossDiscount(string item)
        {
            MaterialClass = item.Substring(0, 6);
            Name = string.Empty;
            Discount = Convert.ToDecimal(item.Substring(54, 5));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            var asInt = Convert.ToInt32(item);

            if (asInt > 0)
              return Convert.ToDecimal(asInt);

            return 0;
        }

        #endregion
    }
}
