using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class Elkedjan : RowBasedSupplierAgreementBase
    {
        public override SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Elkedjan; } }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSupplierAgreementProvider), SoeSupplierAgreementProvider.Elkedjan); }
        }

        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string line)
        {
            /*
            Fältnamn	            Från	Fältlängd
            Radrabatt-kod	        1	    4
            Rabatt heltal	        14	    2
            Rabattgruppsbenämning   17      33 
            */

            var agreement = new GenericSupplierAgreement()
            {
                Code = line.Substring(0, 4).Trim(),
                CodeType = SoeSupplierAgreemntCodeType.MaterialCode,
                Discount = Decimal.Parse(line.Substring(13, 2).Replace('.',',')),
                Name = line.SubstringToLengthOfString(16, 33).Trim(),
            };

            return (true, agreement);
        }
    }
}
