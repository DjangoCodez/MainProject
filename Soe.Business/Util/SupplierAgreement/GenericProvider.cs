using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class GenericProvider
    {
        public List<GenericSupplierAgreement> supplierAgreements;

        public GenericProvider()
        {
            supplierAgreements = new List<GenericSupplierAgreement>();
        }
    }
    public class GenericSupplierAgreement
    {
        #region Members

        private string code = string.Empty;
        public string Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value?.Trim();
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
                name = value?.Trim();
            }
        }

        public decimal Discount { get; set; }
        public SoeSupplierAgreemntCodeType CodeType { get; set; }

        #endregion

        #region Constructors

        public GenericSupplierAgreement()
        {
        }

        public GenericSupplierAgreement(string materialClass, string name, decimal discount)
        {
            CodeType = SoeSupplierAgreemntCodeType.MaterialCode;
            Code = materialClass;
            Name = name;
            Discount = discount;
        }

        #endregion
    }
}
