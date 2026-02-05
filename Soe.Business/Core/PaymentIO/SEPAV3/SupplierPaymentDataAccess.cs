using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    /// <summary>
    /// Default implementation of ISupplierPaymentDataAccess.
    /// Wraps existing PaymentManager and SettingManager calls.
    /// </summary>
    public class SupplierPaymentDataAccess : ISupplierPaymentDataAccess
    {
        private readonly CompEntities _entities;
        private readonly PaymentManager _paymentManager;
        private readonly SettingManager _settingManager;

        public SupplierPaymentDataAccess(CompEntities entities, PaymentManager paymentManager, SettingManager settingManager)
        {
            _entities = entities;
            _paymentManager = paymentManager;
            _settingManager = settingManager;
        }

        public List<PaymentRowInvoiceDTO> GetPaymentRowsWithSupplierInvoice(int paymentId, int actorCompanyId)
        {
            return _paymentManager.GetPaymentRowsWithSupplierInvoice(_entities, paymentId, actorCompanyId);
        }

        public List<PaymentRowInvoiceDTO> GetPaymentRowsWithSupplierInvoice(int paymentId, int actorCompanyId, int? actorId, TermGroup_BillingType billingType)
        {
            return _paymentManager.GetPaymentRowsWithSupplierInvoice(_entities, paymentId, actorCompanyId, actorId, billingType);
        }

        public PaymentRowInvoiceDTO GetPaymentRowWithSupplierInvoice(int paymentRowId, int actorCompanyId)
        {
            return _paymentManager.GetPaymentRowWithSupplierInvoice(_entities, paymentRowId, actorCompanyId);
        }

        public bool GetAutoTransferAutogiroSetting()
        {
            return _settingManager.GetCompanyBoolSetting(CompanySettingType.SupplierInvoiceAutoTransferAutogiroPaymentsToVoucher);
        }
    }
}
