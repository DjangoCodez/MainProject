using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO
{
    public class PaymentIOManager : ManagerBase
    {
        #region Variables

        #endregion

        #region Ctor

        protected PaymentIOManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Export

        /// <summary>
        /// A paymentExport-object, containing information about the export-file, is created and set to the Value-property in an ActionResult, which is returned
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="transaction"></param>
        /// <param name="fileName"></param>
        /// <param name="payment"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="customerNr"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        protected ActionResult CreatePaymentExport( string fileName, List<PaymentRow> paymentRows, TermGroup_SysPaymentMethod paymentMethod, string customerNr, string messageGuid, byte[] filedata, TermGroup_PaymentTransferStatus initialTransferStatus = TermGroup_PaymentTransferStatus.None)
        {
            var result = new ActionResult(true);

            var paymentExport = new PaymentExport
            {
                ExportDate = DateTime.Now,
                Filename = fileName,
                NumberOfPayments = paymentRows.Count,
                CustomerNr = customerNr,
                Type = (int)paymentMethod,
                Data = filedata,
                MsgId = (paymentMethod == TermGroup_SysPaymentMethod.ISO20022) || (paymentMethod == TermGroup_SysPaymentMethod.SEPA) ?  messageGuid: null,
                TransferStatus = (int)initialTransferStatus
            };
            SetCreatedProperties(paymentExport);

            result.Value = paymentExport;
            result.StringValue = messageGuid; //Save GUID and PaymentIOType to be able to open the file from disk later
            result.IntegerValue = (int)paymentMethod;

            return result;
        }

        #endregion
    }
}
