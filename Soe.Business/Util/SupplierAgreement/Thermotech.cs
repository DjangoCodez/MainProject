using SoftOne.Soe.Common.Util;
using System.Data;
using System;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class ThermotechExcel : ExcelProviderBase
    {
        public override SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Thermotech; } }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSupplierAgreementProvider), SoeSupplierAgreementProvider.Thermotech); }
        }

        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(DataRow row)
        {
            decimal discount = 0;

            var productnr = row[0].ToString();

            //skipp articles price rows...
            if (!string.IsNullOrEmpty(productnr))
                return (true, null);

            var code = row[5].ToString();
            if (string.IsNullOrEmpty(code))
                return (true, null);

            if (!decimal.TryParse(row[6].ToString(), out discount))
                return (false, null);

            var agreement = new GenericSupplierAgreement()
            {
                Code = code,
                CodeType = SoeSupplierAgreemntCodeType.MaterialCode,
                Discount = discount*100,
            };

            return (true, agreement);
        }
    }



}
