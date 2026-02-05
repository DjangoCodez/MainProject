using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public enum RexelAgreementColumnPositions
    {
        /// <summary>
        /// Rabattgrupp
        /// </summary>
        MaterialClass = 0,
        /// <summary>
        /// Rabatt 1
        /// </summary>
        Discount = 1,
        /// <summary>
        /// Rabatt 2
        /// </summary>
        Discount2 = 2,
    }

    public class Rexel : CSVProviderBase //ISupplierAgreement
    {
        public override SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.Rexel; } }
        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string[] columns)
        {
            var code = columns[(int)RexelAgreementColumnPositions.MaterialClass].Trim();

            return (true, new GenericSupplierAgreement()
            {
                Name = string.Empty,
                CodeType = code.Length == 7 ? SoeSupplierAgreemntCodeType.Product :  SoeSupplierAgreemntCodeType.MaterialCode,
                Code = code,
                Discount = Convert.ToDecimal(Convert.ToInt32(columns[(int)RexelAgreementColumnPositions.Discount].Trim()) / 10m),
            });
        }

        protected override string WholesellerName
        {
            //Not used
            get { return string.Empty; }
        }
    }
}
