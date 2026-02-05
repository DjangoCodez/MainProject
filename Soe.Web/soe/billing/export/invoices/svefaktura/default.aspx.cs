using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Util;

namespace SoftOne.Soe.Web.soe.billing.export.invoices.svefaktura
{
    public partial class _default : PageBase
    {
        #region Variables

        int dataStorageTypeId = 0;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Export_Invoices_Svefaktura;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init
            
            Int32.TryParse(QS["storageType"], out dataStorageTypeId);

            #endregion

            //Optional paramters
            int dataStorageId = 0;
            Int32.TryParse(QS["dataStorageId"], out dataStorageId);                        

            if (dataStorageId > 0 && dataStorageTypeId > 0)
            {
                ReportManager rm = new ReportManager(ParameterObject);
                DataStorage storage =  rm.GetDataStorage(dataStorageId);
                if(storage != null && dataStorageTypeId == (int)SoeDataStorageRecordType.BillingInvoiceXML)
                    ExportUtil.DownloadFile(storage.Description, storage.Data, false);
            }                        
        }        
    }
}
