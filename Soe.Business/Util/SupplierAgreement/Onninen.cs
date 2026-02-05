using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Onninen : ISupplierAgreement
    {
        public List<OnninenDiscount> supplierAgreements;
        public SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Onninen; } }

        public Onninen()
        {
            supplierAgreements = new List<OnninenDiscount>();
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
                var content = line.Split(";".ToCharArray());
                try
                {
                    var name = string.Empty;
                    var discount = "0";
                    var materialclass = string.Empty;

                    if (content.Length >= 2)
                        materialclass = content[1];

                    if (content.Length >= 3)
                        name = content[2];

                    if (content.Length >= 4)
                        discount = content[3];

                    supplierAgreements.Add(new OnninenDiscount(materialclass, name, discount));
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
            var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.Onninen);
            foreach (OnninenDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion
    }

    public class OnninenDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public OnninenDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public OnninenDiscount(string materialclass, string name, string discount)
        {
            MaterialClass = materialclass;
            Name = name;
            Discount = NumberUtility.ToDecimalSeparatorIndifferent(discount);
        }

        #endregion

        #region Help methods


        #endregion
    }
}
