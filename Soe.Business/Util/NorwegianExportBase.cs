using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util
{
    public abstract class NorwegianExportBase
    {
        #region Enums
        public enum VoucherTaxCode
        {
            P25 = 1,
            P0 = 9,
            P15 = 11,
            P8 = 21,
        }
        #endregion

        #region Fields
        protected Company company;
        protected string errorMessage = string.Empty;
        #endregion

        #region Constructor

        public NorwegianExportBase(Company company)
        {
            this.company = company;
        }

        #endregion Constructor

        #region Public Methods

        public bool AddForExport(CustomerInvoice invoice, Customer customer, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<PaymentInformationRowDTO> paymentInformations, string kidNr, int? paymentSeqNr = null)
        {
            return Populate(invoice, customer, customerBillingAddress, customerDeliveryAddress, customerContactEcoms, paymentInformations, kidNr, paymentSeqNr);
        }

        #endregion

        protected abstract bool Populate(CustomerInvoice invoice, Customer customer, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<PaymentInformationRowDTO> paymentInformations, string kidNr, int? paymentSeqNr = null);

        #region InnerClasses

        #endregion

        //[Obsolete("TODO change this when the new VAT system is implemented", false)]
        internal static bool TryGetTaxCodeFromVatRate(decimal vatRate, out VoucherTaxCode taxCode)
        {
            // TODO change this when the new VAT system is implemented
            taxCode = VoucherTaxCode.P0;
            if (vatRate == 0)
                taxCode = VoucherTaxCode.P0;
            else if (vatRate == 8)
                taxCode = VoucherTaxCode.P8;
            else if (vatRate == 15)
                taxCode = VoucherTaxCode.P15;
            else if (vatRate == 25)
                taxCode = VoucherTaxCode.P25;
            else
                return false;

            return true;
        }
    }
}
