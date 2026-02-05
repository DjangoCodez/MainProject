using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.soe.time.time.salary
{
    public partial class _default : PageBase
    {
        #region Variables

        private EmployeeManager em;
        private GeneralManager gm;
        private ReportManager rm;
        private SettingManager sm;
        private TimePeriodManager tpm;

        protected string subtitle;
        private int timePayrollSlipReportId;
        private int timeSalarySpecificationReportId;
        private int timeSalaryControlInfoReportId;
        private int timeKU10ReportId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Time_TimeSalarySpecification;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            em = new EmployeeManager(ParameterObject);
            gm = new GeneralManager(ParameterObject);
            rm = new ReportManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            tpm = new TimePeriodManager(ParameterObject);

            #endregion

            #region Populate

            try
            {
                bool usePayroll = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UsePayroll, 0, SoeCompany.ActorCompanyId, 0);

                //Include RoleId to check permissions
                timePayrollSlipReportId = usePayroll ? rm.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip, SoeCompany.ActorCompanyId, UserId, null) : 0;
                timeSalarySpecificationReportId = rm.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.TimeDefaultTimeSalarySpecificationReport, SoeReportTemplateType.TimeSalarySpecificationReport, SoeCompany.ActorCompanyId, UserId, RoleId);
                timeSalaryControlInfoReportId = rm.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.TimeDefaultTimeSalaryControlInfoReport, SoeReportTemplateType.TimeSalaryControlInfoReport, SoeCompany.ActorCompanyId, UserId, RoleId);
                timeKU10ReportId = rm.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.TimeDefaultKU10Report, SoeReportTemplateType.KU10Report, SoeCompany.ActorCompanyId, UserId, null);

                DivSubTitle.Visible = false;

                if (timePayrollSlipReportId > 0 || timeSalarySpecificationReportId > 0 || timeKU10ReportId > 0)
                {
                    Employee employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
                    if (employee != null)
                    {
                        var dataStorages = gm.GetTimeSalaryImportsByEmployee(employee.EmployeeId, SoeCompany.ActorCompanyId, true, false, false);
                        if (usePayroll)
                            dataStorages.AddRange(gm.GetTimePayrollSlipByEmployee(employee.EmployeeId, SoeCompany.ActorCompanyId, false, false));

                        dataStorages = dataStorages.OrderByDescending(o => o.TimePeriodStopDate).ToList();

                        SoeGrid1.Title = GetText(5596, "Lönespecifikationer");
                        SoeGrid1.DataSource = dataStorages;
                        SoeGrid1.RowDataBound += SoeGrid1_RowDataBound;
                        SoeGrid1.DataBind();
                    }
                    else
                    {
                        subtitle = GetText(5283, "Ingen anställd kopplad till inloggad användare");
                        DivSubTitle.Visible = true;
                        SoeGrid1.Visible = false;
                    }
                }
                else
                {
                    subtitle = GetText(5601, "Ingen standardrapport för lönesepec är upplagd på företaget");
                    DivSubTitle.Visible = true;
                    SoeGrid1.Visible = false;
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "Page_Load");
                subtitle = GetText(5601, "Ingen standardrapport för lönesepec är upplagd på företaget");
                DivSubTitle.Visible = true;
                SoeGrid1.Visible = false;
            }

            #endregion
        }

        #region Events

        private void SoeGrid1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var dataStorage = ((e.Row.DataItem) as DataStorageAllDTO);
            try
            {
                if (dataStorage != null)
                {
                    PlaceHolder phSalarySpecification = (PlaceHolder)e.Row.FindControl("phSalarySpecification");
                    if (phSalarySpecification != null)
                    {
                        if (dataStorage.Type == SoeDataStorageRecordType.PayrollSlipXML)
                        {
                            Link link = new Link()
                            {
                                Href = GetTimePayrollSlipUrl(dataStorage),
                                Alt = GetText(5602, "Visa lönespec"),
                                ImageSrc = "/img/pdf.png",
                                Permission = Permission.Readonly,
                                Feature = Feature.Time_Time_TimeSalarySpecification,
                            };
                            if (string.IsNullOrEmpty(link.Href))
                                link.Href = "#";
                            phSalarySpecification.Controls.Add(link);
                        }
                        else if (dataStorage.Type == SoeDataStorageRecordType.TimeSalaryExportSaumaPdf)
                        {
                            Link link = new Link()
                            {
                                Href = GetTimeSaumaSalarySpecificationUrl(dataStorage),
                                Alt = GetText(5602, "Visa lönespec"),
                                ImageSrc = "/img/sauma.png",
                                Permission = Permission.Readonly,
                                Feature = Feature.Time_Distribution_Reports_Selection,
                            };
                            if (string.IsNullOrEmpty(link.Href))
                                link.Href = "#";
                            phSalarySpecification.Controls.Add(link);
                        }
                        else if (dataStorage.Type == SoeDataStorageRecordType.TimeKU10ExportEmployee)
                        {
                            Link link = new Link()
                            {
                                Href = GetTimeKU10ReportUrl(dataStorage),
                                Alt = GetText(91895, "Visa kontrolluppgift"),
                                ImageSrc = "/img/pdf.png",
                                Permission = Permission.Readonly,
                                Feature = Feature.Time_Time_TimeSalarySpecification,
                            };
                            if (string.IsNullOrEmpty(link.Href))
                                link.Href = "#";
                            phSalarySpecification.Controls.Add(link);
                        }
                        else
                        {
                            Link link = new Link()
                            {
                                Href = GetTimeSalarySpecificationUrl(dataStorage),
                                Alt = GetText(5602, "Visa lönespec"),
                                ImageSrc = "/img/pdf.png",
                                Permission = Permission.Readonly,
                                Feature = Feature.Time_Time_TimeSalarySpecification,
                            };
                            if (string.IsNullOrEmpty(link.Href))
                                link.Href = "#";
                            phSalarySpecification.Controls.Add(link);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex, "SoeGrid1_RowDataBound");
            }
        }

        #endregion

        #region Help-methods

        private string GetTimePayrollSlipUrl(DataStorageAllDTO dataStorage)
        {
            if (dataStorage == null || !dataStorage.TimePeriodId.HasValue || !dataStorage.EmployeeId.HasValue)
                return "#";

            int reportId = timePayrollSlipReportId;
            int sysReportTemplateTypeId = (int)SoeReportTemplateType.TimeSalarySpecificationReport;

            if (reportId <= 0 || sysReportTemplateTypeId <= 0)
                return "#";

            TimePeriod timePeriod = tpm.GetTimePeriod(dataStorage.TimePeriodId.Value, SoeCompany.ActorCompanyId);
            if (timePeriod == null)
                return "#";

            return rm.GetPayrollSlipReportPrintUrl(SoeCompany.ActorCompanyId, timePeriod.TimePeriodId, dataStorage.EmployeeId.Value, reportId);
        }

        private string GetTimeSalarySpecificationUrl(DataStorageAllDTO dataStorage)
        {
            if (dataStorage == null || !dataStorage.TimePeriodId.HasValue)
                return "#";

            int reportId = 0;
            int sysReportTemplateTypeId = 0;

            if (dataStorage.Type == SoeDataStorageRecordType.TimeSalaryExportEmployee)
            {
                reportId = timeSalarySpecificationReportId;
                sysReportTemplateTypeId = (int)SoeReportTemplateType.TimeSalarySpecificationReport;
            }
            else if (dataStorage.Type == SoeDataStorageRecordType.TimeSalaryExportControlInfoEmployee)
            {
                reportId = timeSalaryControlInfoReportId;
                sysReportTemplateTypeId = (int)SoeReportTemplateType.TimeSalaryControlInfoReport;
            }

            if (reportId <= 0 || sysReportTemplateTypeId <= 0)
                return "#";

            var reportItem = new TimeSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, dataStorage.TimePeriodStartDate, dataStorage.TimePeriodStopDate, dataStorage.TimePeriodId.Value, dataStorage.DataStorageId, webBaseUrl: true);

            return reportItem.ToString(true);
        }

        private string GetTimeKU10ReportUrl(DataStorageAllDTO dataStorage)
        {
            if (dataStorage == null || !dataStorage.TimePeriodId.HasValue)
                return "#";

            int reportId = 0;
            int sysReportTemplateTypeId = 0;

            if (dataStorage.Type == SoeDataStorageRecordType.TimeKU10ExportEmployee)
            {
                reportId = timeKU10ReportId;
                sysReportTemplateTypeId = (int)SoeReportTemplateType.KU10Report;
            }

            if (reportId <= 0 || sysReportTemplateTypeId <= 0)
                return "#";

            var reportItem = new TimeKU10ReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, dataStorage.TimePeriodStartDate, dataStorage.TimePeriodStopDate, dataStorage.TimePeriodId.Value, dataStorage.DataStorageId, webBaseUrl: true);

            return reportItem.ToString(true);
        }

        private string GetTimeSaumaSalarySpecificationUrl(DataStorageAllDTO dataStorage)
        {
            if (dataStorage == null)
                return "#";

            int sysReportTemplateTypeId = 0;

            if (dataStorage.Type == SoeDataStorageRecordType.TimeSalaryExportSaumaPdf)
            {
                sysReportTemplateTypeId = (int)SoeReportTemplateType.TimeSaumaSalarySpecificationReport;
            }

            var reportItem = new TimeSaumaSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, sysReportTemplateTypeId, dataStorage.DataStorageId);

            return reportItem.ToString(true);
        }

        #endregion
    }
}