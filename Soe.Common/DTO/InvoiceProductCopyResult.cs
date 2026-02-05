using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class InvoiceProductCopyResult : InvoiceProductPriceResult
    {
        public InvoiceProductDTO Product { get; set; }
        //public InvoiceProductRowItem RowItem { get; set; }

        public override bool ProductIsSupplementCharge
        {
            get { return Product.IsSupplementCharge; }
        }

        public InvoiceProductCopyResult() : base() { }

        public InvoiceProductCopyResult(ActionResultSelect errorNumber) : base(errorNumber) { }

        public InvoiceProductCopyResult(ActionResultSelect errorNumber, string errorMessage) : base(errorNumber, errorMessage) { }
    }
}
