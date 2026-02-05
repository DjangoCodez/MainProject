using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.payroll.payment
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Payroll_Payment;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int timeSalaryPaymentExportId = 0;
            int.TryParse(QS["timeSalaryPaymentExportId"], out timeSalaryPaymentExportId);

            if (timeSalaryPaymentExportId > 0)
            {
                TimeSalaryManager tsm = new TimeSalaryManager(ParameterObject);

                TimeSalaryPaymentExport timeSalaryPaymentExport = tsm.GetTimeSalaryPaymentExport(timeSalaryPaymentExportId, SoeCompany.ActorCompanyId, false);
                if (timeSalaryPaymentExport != null)
                {                    
                    string clientName = QS["clientname"];
                    string extension = timeSalaryPaymentExport.Extension;
                    clientName += "." + extension;

                    byte[] data = timeSalaryPaymentExport.ExportFile;
                    if (data.Length < 1)
                    {
                        System.Text.Encoding enc = System.Text.Encoding.Unicode;
                        data = enc.GetBytes(" ");
                    }

                    //transfer file
                    switch ((TermGroup_TimeSalaryPaymentExportType)timeSalaryPaymentExport.ExportType)
                    {
                        case TermGroup_TimeSalaryPaymentExportType.SUS:
                            ExportUtil.DownloadFile(clientName, data, false);
                            break;                                                    
                        default:
                            ExportUtil.DownloadFile(clientName, data, true);
                            break;
                    }                    
                }
            }
        }
    }
}
