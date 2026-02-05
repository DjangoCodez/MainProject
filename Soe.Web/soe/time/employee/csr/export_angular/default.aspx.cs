using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.employee.csr.export_angular
{
    public partial class _default : PageBase
    {

        public int dataStorageTypeId = 0;

        private ReportManager rm;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Employee_Csr_Export;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            rm = new ReportManager(ParameterObject);

            //Optional paramters
            int dataStorageId = 0;
            Int32.TryParse(QS["dataStorageId"], out dataStorageId);

            if (dataStorageId > 0)
            {
                DataStorage storage = rm.GetDataStorage(dataStorageId);
                if (storage != null)
                {
                    ExportUtil.DownloadFile(storage.Description, storage.Data, false);
                }
            }
        }
    }
}