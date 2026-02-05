using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.soe.time.import.salary.imported
{
    public partial class _default : PageBase
    {
        #region Variables

        private GeneralManager gm;

        protected string subtitle;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Import_Salary;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            gm = new GeneralManager(ParameterObject);

            #endregion

            #region Actions

            int delete;
            if (Int32.TryParse(QS["delete"], out delete) && delete == 1)
            {
                int dataStorageId;
                if (Int32.TryParse(QS["datastorage"], out dataStorageId) && dataStorageId > 0)
                {
                    bool deleted = DeleteDataStorage(dataStorageId);
                    if (deleted)
                        Response.Redirect(Request.Url.AbsolutePath);
                }
            }

            #endregion

            #region Populate

            SoeGrid1.Title = GetText(5596, "Lönespecifikationer");
            SoeGrid1.DataSource = gm.GetTimeSalaryImportsByCompany(SoeCompany.ActorCompanyId);
            SoeGrid1.RowDataBound += SoeGrid1_RowDataBound;
            SoeGrid1.DataBind();

            #endregion
        }

        #region Events

        private void SoeGrid1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var timeSalaryImport = ((e.Row.DataItem) as DataStorage);
            if (timeSalaryImport != null)
            {
                PlaceHolder phSalarySpecification = (PlaceHolder)e.Row.FindControl("phSalarySpecification");
                if (phSalarySpecification != null)
                {
                    string url = "";
                    if (timeSalaryImport.Type == (int)SoeDataStorageRecordType.TimeSalaryExportEmployee)
                        url = GetTimeSalarySpecificationUrl(timeSalaryImport);
                    else if (timeSalaryImport.Type == (int)SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee)
                        url = GetTimeSalaryControlInfoUrl(timeSalaryImport);
                    else if (timeSalaryImport.Type == (int)SoeDataStorageRecordType.TimeSalaryExportSaumaPdf)
                        url = "#";

                    if (timeSalaryImport.Type == (int)SoeDataStorageRecordType.TimeSalaryExportSaumaPdf)
                    {
                        Link link = new Link()
                        {
                            Href = url,
                            Alt = GetText(5602, "Visa lönespec"),
                            ImageSrc = "/img/sauma.png",
                            Permission = Permission.Readonly,
                            Feature = Feature.Time_Time_AttestUser,
                        };
                        phSalarySpecification.Controls.Add(link);
                    }
                    else
                    {
                        Link link = new Link()
                        {
                            Href = url,
                            Alt = GetText(5602, "Visa lönespec"),
                            ImageSrc = "/img/pdf.png",
                            Permission = Permission.Readonly,
                            Feature = Feature.Time_Time_AttestUser,
                        };
                        phSalarySpecification.Controls.Add(link);
                    }                    
                }
            }
        }

        #endregion

        #region Help-methods

        private string GetTimeSalarySpecificationUrl(DataStorage timeSalaryImport)
        {
            if (timeSalaryImport == null || timeSalaryImport.TimePeriod == null)
                return "#";

            var reportItem = new TimeSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, 0, (int)SoeReportTemplateType.TimeSalarySpecificationReport, 
                timeSalaryImport.TimePeriod.StartDate, timeSalaryImport.TimePeriod.StopDate, timeSalaryImport.TimePeriod.TimePeriodId, timeSalaryImport.DataStorageId, webBaseUrl: true);

            return reportItem.ToString(true);
        }

        private string GetTimeSalaryControlInfoUrl(DataStorage timeSalaryImport)
        {
            if (timeSalaryImport == null || timeSalaryImport.TimePeriod == null)
                return "#";

            var reportItem = new TimeSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, 0, (int)SoeReportTemplateType.TimeSalaryControlInfoReport,
                timeSalaryImport.TimePeriod.StartDate, timeSalaryImport.TimePeriod.StopDate, timeSalaryImport.TimePeriod.TimePeriodId, timeSalaryImport.DataStorageId, webBaseUrl: true);

            return reportItem.ToString(true);
        }

        #endregion

        #region Action-methods

        private bool DeleteDataStorage(int dataStorageId)
        {
            if (dataStorageId > 0)
                return gm.DeleteDataStorage(dataStorageId, SoeCompany.ActorCompanyId).Success;
            return false;
        }

        #endregion
    }
}