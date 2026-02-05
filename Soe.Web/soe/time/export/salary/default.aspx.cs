using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.export.salary
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Export_Salary;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            int timeSalaryExportId = 0;
            int.TryParse(QS["timeSalaryExportId"], out timeSalaryExportId);

            if (timeSalaryExportId > 0)
            {
                TimeSalaryManager tsm = new TimeSalaryManager(ParameterObject);

                TimeSalaryExport timeSalaryExport = tsm.GetTimeSalaryExport(timeSalaryExportId, SoeCompany.ActorCompanyId, false);
                if (timeSalaryExport != null)
                {
                    bool fileOne = true;
                    bool.TryParse(QS["first"], out fileOne);
                    string clientName = QS["clientname"];
                    clientName = clientName.Replace(" ", "_");
                    string extension = string.Empty;

                    if (!fileOne)
                    {
                        //decide the extension for file 2, should a column like the extension for file 1...
                        switch (timeSalaryExport.ExportTarget)
                        {
                            case (int)SoeTimeSalaryExportTarget.Hogia214006:
                            case (int)SoeTimeSalaryExportTarget.Hogia214007:
                            case (int)SoeTimeSalaryExportTarget.Flex:
                                extension = "sch";
                                break;
                            default:
                                extension = timeSalaryExport.Extension;
                                break;
                        }

                    }
                    else
                    {
                        extension = timeSalaryExport.Extension;
                    }

                    clientName += "." + extension;

                    byte[] data = fileOne ? timeSalaryExport.File1 : timeSalaryExport.File2;
                    if (data.Length < 1)
                    {
                        System.Text.Encoding enc = System.Text.Encoding.Unicode;
                        data = enc.GetBytes(" ");
                    }

                    //transfer file                    
                    switch (timeSalaryExport.ExportTarget)
                    {
                        case (int)SoeTimeSalaryExportTarget.Hogia214006:
                        case (int)SoeTimeSalaryExportTarget.Hogia214007:
                        case (int)SoeTimeSalaryExportTarget.Flex:
                        case (int)SoeTimeSalaryExportTarget.Spcs:
                        case (int)SoeTimeSalaryExportTarget.AditroL1:
                        case (int)SoeTimeSalaryExportTarget.HuldtOgLillevik:
                        case (int)SoeTimeSalaryExportTarget.PAxml2_1:
                        case (int)SoeTimeSalaryExportTarget.Pol:
                        case (int)SoeTimeSalaryExportTarget.SDWorx:
                        case (int)SoeTimeSalaryExportTarget.Lessor:
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
