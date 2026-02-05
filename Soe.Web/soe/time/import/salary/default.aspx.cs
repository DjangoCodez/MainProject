using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.IO;
using System.Web;
using System.Xml.Linq;

namespace SoftOne.Soe.Web.soe.time.import.salary
{
    public partial class _default : PageBase
    {
        #region Variables

        private SettingManager sm;
        private TimePeriodManager tpm;
        private TimeSalaryManager tsm;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Import_Salary;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            sm = new SettingManager(ParameterObject);
            tpm = new TimePeriodManager(ParameterObject);
            tsm = new TimeSalaryManager(ParameterObject);

            #endregion

            #region Populate

            int defaultTimePeriodHeadId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.TimeDefaultTimePeriodHead, 0, SoeCompany.ActorCompanyId, 0);
            SalaryTimePeriod.ConnectDataSource(tpm.GetTimePeriodsDict(defaultTimePeriodHeadId, true, SoeCompany.ActorCompanyId));

            ImportType.ConnectDataSource(tsm.GetTimeSalaryImportTypes());

            #endregion

            #region Set data

            //Default values
            if (Mode != SoeFormMode.Repopulate)
            {
                TimePeriod timePeriod = tpm.GetTimePeriod(DateTime.Now, defaultTimePeriodHeadId, SoeCompany.ActorCompanyId);
                if (timePeriod != null)
                    SalaryTimePeriod.Value = timePeriod.TimePeriodId.ToString();
            }

            #endregion

            #region Actions

            if (Request.Form["action"] == "upload")
            {
                int timePeriodId = Convert.ToInt32(F["SalaryTimePeriod"]);
                int importType = Convert.ToInt32(F["ImportType"]);
             
                if (timePeriodId > 0)
                {
                    int fileNo = 0;
                    foreach (String fileName in Request.Files)
                    {
                        HttpPostedFile file = Request.Files[fileNo];
                        if (file != null && file.ContentLength > 0)
                        {
                            if (Import(file, timePeriodId, importType))
                                Form1.MessageSuccess = GetText(5566, "Import av lönespecifikation klar");
                            else
                                Form1.MessageError = GetText(5567, "Import av lönespecifikation misslyckades");
                        }
                        else
                        {
                            Form1.MessageWarning = GetText(5568, "Lönespecifikationen hittades inte");
                        }

                        fileNo++;
                    }
                }
                else
                {
                    Form1.MessageWarning = GetText(5565, "Du måste ange löneperiod");
                }
            }

            #endregion
        }

        #region Action-methods

        private bool Import(HttpPostedFile file, int timePeriodId, int importType)
        {
            bool success = false;

            if (importType == (int)SoeDataStorageRecordType.TimeSalaryExportEmployee)
            {
                #region TimeSalaryExportEmployee

                try
                {
                    XDocument xdoc = XDocument.Load(file.InputStream);
                    if (xdoc != null)
                        success = tsm.ImportTimeSalary(xdoc, timePeriodId, SoeCompany.ActorCompanyId).Success;
                }
                finally
                {
                    file.InputStream.Close();
                }

                #endregion
            }
            else if (importType == (int)SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee)
            {
                #region TimeSalaryExportControlInfoEmployee

                try
                {
                    XDocument xdoc = XDocument.Load(file.InputStream);
                    if (xdoc != null)
                        success = tsm.ImportTimeSalaryControlInfo(xdoc, timePeriodId, SoeCompany.ActorCompanyId).Success;
                }
                finally
                {
                    file.InputStream.Close();
                }

                #endregion
            }
            else if (importType == (int)SoeDataStorageRecordType.TimeKU10ExportEmployee)
            {
                #region SoeControlInfoEmployee

                try
                {
                    XDocument xdoc = XDocument.Load(file.InputStream);
                    if (xdoc != null)
                        success = tsm.ImportKU10(xdoc, timePeriodId, SoeCompany.ActorCompanyId).Success;
                }
                finally
                {
                    file.InputStream.Close();
                }

                #endregion
            }
            else if (importType == (int)SoeDataStorageRecordType.TimeSalaryExportSaumaPdf)
            {
                #region TimeSalaryExportSaumaPdf

                try
                {
                    byte[] data = new byte[file.ContentLength];
                    if (data != null)
                    {
                        file.InputStream.Read(data, 0, file.ContentLength);
                        success = tsm.ImportTimeSalarySaumaSpecification(data, Path.GetFileNameWithoutExtension(file.FileName), timePeriodId, SoeCompany.ActorCompanyId).Success;
                    }
                }
                finally
                {
                    file.InputStream.Close();
                }

                #endregion
            }

            return success;
        }

        #endregion
   
    }


}