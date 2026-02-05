using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class JohnFredrik : RowBasedSupplierAgreementBase
    {
        public override SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.JohnFredrik; } }
        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSupplierAgreementProvider), SoeSupplierAgreementProvider.JohnFredrik); }
        }

        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string line)
        {
            /*
             * RabateCode = 10, 4
             * Name = 25, 42
             * Rabate = 67, 4
             */

            return (true, new GenericSupplierAgreement(line.Substring(10, 4), line.Substring(25, 40).Trim(), decimal.Parse(line.Substring(67, 4).Replace('.', ','))));
        }
    }
}
