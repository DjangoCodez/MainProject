using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    /// <summary>
    /// Interface for payment data access read operations.
    /// Enables unit testing of ConvertSupplierStreamToEntity with mocked dependencies.
    /// </summary>
    public interface ISupplierPaymentDataAccess
    {
        /// <summary>
        /// Gets payment rows with supplier invoice information for a given payment.
        /// </summary>
        List<PaymentRowInvoiceDTO> GetPaymentRowsWithSupplierInvoice(int paymentId, int actorCompanyId);

        /// <summary>
        /// Gets payment rows with supplier invoice information, filtered by actor and billing type.
        /// </summary>
        List<PaymentRowInvoiceDTO> GetPaymentRowsWithSupplierInvoice(int paymentId, int actorCompanyId, int? actorId, TermGroup_BillingType billingType);

        /// <summary>
        /// Gets a single payment row with supplier invoice information.
        /// </summary>
        PaymentRowInvoiceDTO GetPaymentRowWithSupplierInvoice(int paymentRowId, int actorCompanyId);

        /// <summary>
        /// Gets the company setting for auto-transferring autogiro payments.
        /// </summary>
        bool GetAutoTransferAutogiroSetting();
    }
}
