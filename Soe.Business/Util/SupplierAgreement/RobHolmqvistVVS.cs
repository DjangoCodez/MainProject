using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public class RobHolmqvistVVS : RowBasedSupplierAgreementBase
    {
        public override SoeSupplierAgreementProvider Provider { get { return SoeSupplierAgreementProvider.RobHolmqvistVVS; } }
        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeSupplierAgreementProvider), SoeSupplierAgreementProvider.RobHolmqvistVVS); }
        }

        protected override (bool success, GenericSupplierAgreement agreement) ToGenericSupplierAgreement(string line)
        {
            /*
            Fältnamn	        Från	Till	Fältlängd
            Radrabatt-kod	    1	    10	    10
            Beskrivning	        11	    40	    30
            Rabatt heltal	    41	    42	    2
            Punkt	            43	    43	    1
            Rabatt decimal	    44	    45	    2
            Artikelnummer	    46	    66	    20
            Nettopris heltal	66	    72	    7
            Punkt	            73	    73	    1
            Nettopris decimal	74	    75	    2
            */

            var agreement = new GenericSupplierAgreement();

            var productNr = line.Substring(45, 20);
            if (string.IsNullOrWhiteSpace(productNr))
            {
                agreement.Code = line.Substring(0, 10).Trim();
                agreement.CodeType = Common.Util.SoeSupplierAgreemntCodeType.MaterialCode;
            }
            else
            {
                // Rob holmkvist use netprice when discount is on productNr. 
                // In order to use this we have to fetch the product and calculate the discount in percentage.
                // For now just return null to move on to next product
                return (true, null);

                //agreement.Code = productNr.Trim();
                //agreement.CodeType = Common.Util.SoeSupplierAgreemntCodeType.Product;
            }

            agreement.Name = line.Substring(10, 30).Trim();
            agreement.Discount = Convert.ToDecimal(line.Substring(40, 5).Replace('.', ','));

            return (true, agreement);
        }
    }
}
