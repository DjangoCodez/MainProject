using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Linq;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.economy.export.invoices.sop
{
    public partial class _default : PageBase
    {
        #region Variables

        public int dataStorageTypeId = 0;

        private ReportManager rm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);
        }

        private void EnableModuleSpecifics()
        {
            Int32.TryParse(QS["storageType"], out dataStorageTypeId);

            switch (dataStorageTypeId)
            {
                case (int)SoeDataStorageRecordType.SOPCustomerInvoiceExport:
                    Feature = Feature.Economy_Export_Invoices_SOP;
                    break;
                case (int)SoeDataStorageRecordType.DiRegnskapCustomerInvoiceExport:
                    Feature = Feature.Economy_Export_Invoices_DIRegnskap;
                    break;
                case (int)SoeDataStorageRecordType.UniMicroCustomerInvoiceExport:
                    Feature = Feature.Economy_Export_Invoices_UniMicro;
                    break;
                case (int)SoeDataStorageRecordType.DnBNorCustomerInvoiceExport:
                    Feature = Feature.Economy_Export_Invoices_DnBNor;
                    break;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init
            
            rm = new ReportManager(ParameterObject);

            //Optional paramters
            int dataStorageId = 0;
            Int32.TryParse(QS["dataStorageId"], out dataStorageId);

            #endregion

            if (dataStorageId > 0 && dataStorageTypeId > 0)
            {
                DataStorage storage = rm.GetDataStorage(dataStorageId);
                if (storage != null)
                {
                    if (dataStorageTypeId == (int)SoeDataStorageRecordType.SOPCustomerInvoiceExport)
                    {
                        ExportUtil.DownloadFile(storage.Description, storage.Data, false);
                    }
                    else if (dataStorageTypeId == (int)SoeDataStorageRecordType.DiRegnskapCustomerInvoiceExport)
                    {
                        ExportUtil.DownloadFile(storage.Description, storage.XML, true);
                    }
                    else if (dataStorageTypeId == (int)SoeDataStorageRecordType.DnBNorCustomerInvoiceExport)
                    {
                        ExportUtil.DownloadFile(storage.Description, storage.XML, true);
                    }
                    else if (dataStorageTypeId == (int)SoeDataStorageRecordType.UniMicroCustomerInvoiceExport)
                    {
                        ExportUtil.DownloadFile(storage.Description, storage.Data, false);
                    }
                }
            }           
        }        
    }
}
