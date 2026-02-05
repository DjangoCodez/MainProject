using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public abstract class CSVProviderBase : RowBasedSupplierAgreementBase
    {
        protected virtual int FirstColumnIndex { get { return 0; } }

        protected override (bool success,GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string line)
        {
            if (FirstColumnIndex > 0)
                line = line.AddLeft(FirstColumnIndex, ';');

            var columns = line.Split(';');
            return this.ToGenericSupplierAgreement(columns);
        }

        protected abstract (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string[] columns);
    }
}
