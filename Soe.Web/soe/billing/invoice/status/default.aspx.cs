using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.CrGen;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.invoice.status
{
    public partial class _default : PageBase
    {
        #region Variables

        private ReportManager rm;       
        private AccountManager am;
        protected SoeModule TargetSoeModule = SoeModule.Billing;
        protected Feature FeatureEdit = Feature.Billing_Invoice_Status;
        public bool handleCustomerPayments = false;
        public int employeeId = 0;
        public int accountYearId;
        public bool accountYearIsOpen;
        public int invoiceId = 0;
        public string invoiceNr;
        public int customerId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Invoice_Status;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rm = new ReportManager(ParameterObject);           
            am = new AccountManager(ParameterObject);

            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            string classificationgroup = QS["classificationgroup"];
            if (classificationgroup != null && int.Parse(classificationgroup) == (int)SoeOriginStatusClassificationGroup.HandleCustomerPayments)
            {
                FeatureEdit = Feature.Economy_Customer_Payment;
                handleCustomerPayments = true;
            }

            //CustomerInvoice SOP Export / Finvoice File Download
            Int32.TryParse(QS["custInvExportBatchId"], out int exportSOPDataStorageId);
            if (exportSOPDataStorageId > 0)
            {
                DataStorage storage = rm.GetDataStorage(exportSOPDataStorageId);
                if (storage != null)
                {
                    if (storage.Data == null && storage.DataCompressed != null)
                        storage.Data = CompressionUtil.Decompress(storage.DataCompressed);

                    String exportSOPFileName = QS["fileName"];
                    ExportUtil.DownloadFile(exportSOPFileName, storage.Data, false);
                }
            }

            #endregion

            #region Legacy Navigation

            Int32.TryParse(QS["invoiceId"], out invoiceId);
            invoiceNr = QS["invoiceNr"] ?? "";
            customerId = string.IsNullOrEmpty(QS["customerId"]) ? 0 : Int32.Parse(QS["customerId"]);

            #endregion     
        }
    }
}
