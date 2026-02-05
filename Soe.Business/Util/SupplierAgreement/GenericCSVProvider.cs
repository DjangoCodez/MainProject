using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class GenericSupplierAgreementColumnPositions
    {
        public int? Code { get; set; }
        public int? Name { get; set; }
        public int? Discount { get; set; }
    }

    public class GenericCSVProvider : RowBasedSupplierAgreementBase
    {
        private readonly string _wholesellerName;
        private readonly GenericSupplierAgreementColumnPositions _columnPositions;
        private readonly SoeSupplierAgreementProvider providerType;
        public override SoeSupplierAgreementProvider Provider { get { return providerType; } }

        public GenericCSVProvider(SoeSupplierAgreementProvider provider, GenericSupplierAgreementColumnPositions columnPositions, bool skipFirstRow, bool errorOnNoRows = false)
        {
            this.providerType = provider;
            _wholesellerName = Enum.GetName(typeof(SoeSupplierAgreementProvider), provider);
            _columnPositions = columnPositions;
            SkipRows = skipFirstRow ? 1 : 0;
            ErrorOnNoRows = errorOnNoRows;
        }

        protected override string WholesellerName
        {
            get { return _wholesellerName; }
        }

        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string line)
        {
            string[] columns = line.Split(';');
            var code = _columnPositions.Code != null ? columns[(int)_columnPositions.Code].Trim() : null;
            var discountStr = _columnPositions.Discount != null ? columns[(int)_columnPositions.Discount]?.Trim() : null;

            if (string.IsNullOrEmpty(discountStr))
            {
                return (false, null);
            }

            var agreement = new GenericSupplierAgreement
            {
                Name = _columnPositions.Name != null ? columns[(int)_columnPositions.Name].Trim() : "",
                Discount = discountStr != null ? ToDecimalInvariant(discountStr) : 0,
                Code = code,
                CodeType = SoeSupplierAgreemntCodeType.MaterialCode
            };

            return (true,agreement);
        }
    }
}
