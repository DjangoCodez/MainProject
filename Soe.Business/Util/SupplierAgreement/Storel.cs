using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Storel : CSVProviderBase//ISupplierAgreement
    {
        public override SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Storel; } }
        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string[] columns)
        {
            return (true, new GenericSupplierAgreement()
            {
                Name = String.Empty,
                CodeType = SoeSupplierAgreemntCodeType.MaterialCode,
                Code = columns[(int)RexelAgreementColumnPositions.MaterialClass].Trim(),
                Discount = Convert.ToDecimal(Convert.ToInt32(columns[(int)RexelAgreementColumnPositions.Discount].Trim()) / 10m),
            });
        }

        protected override string WholesellerName
        {
            //Not used
            get { return String.Empty; }
        }

        #region old
        /*public List<StorelDiscount> supplierAgreements;

        public Storel()
        {
            supplierAgreements = new List<StorelDiscount>();
        }

        #region Public methods

        public ActionResult Read(Stream stream)
        {
            ActionResult result = new ActionResult();
            StreamReader sr = new StreamReader(stream, Encoding.GetEncoding(Constants.FILE_ENCODING_LATIN1));
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                line = line.PadRight(25);
                try
                {
                    supplierAgreements.Add(new StorelDiscount(line));
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
            //var wholeSeller = Enum.GetName(typeof(SoeSysPriceListProvider), SoeSupplierAgreementProvider.Storel);
            foreach (StorelDiscount post in supplierAgreements)
            {
                result.supplierAgreements.Add(new GenericSupplierAgreement(post.MaterialClass, post.Name, post.Discount));
            }
            return result;
        }

        #endregion*/
        #endregion
    }

    public class StorelDiscount
    {
        #region Members

        public string MaterialClass { get; set; }
        public string Name { get; set; }
        public decimal Discount { get; set; }

        #endregion

        #region Constructors

        public StorelDiscount()
        {
            //Empty constructor to enable unit testing
        }

        public StorelDiscount(string item)
        {
            MaterialClass = item.Substring(0, 4);
            Name = string.Empty;
            Discount = ToDecimal(item.Substring(17, 5));
        }

        #endregion

        #region Help methods

        private decimal ToDecimal(string item)
        {
            item = item.Replace(".", "");
            item = item.Replace(",", "");
            var asInt = Convert.ToInt32(item);
            return Convert.ToDecimal(asInt / 100M);
        }

        #endregion
    }
}
