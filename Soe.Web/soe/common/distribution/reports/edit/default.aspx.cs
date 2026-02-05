using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace SoftOne.Soe.Web.soe.common.distribution.reports.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private ReportManager rm;
        private CompanyManager cm;

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        public bool EnableSorting { get; set; }
        private SoeModule TargetSoeModule = SoeModule.None;
        private Feature FeatureEdit = Feature.None;
        private Feature FeatureReportGroupMapping = Feature.None;
        private Feature FeatureSelection = Feature.None;
        private Feature FeatureSysTemplateDownload = Feature.None;
        private Feature FeatureTemplateDownload = Feature.None;
        private Feature FeatureReportRolePermission = Feature.None;

        protected Report report;
        protected int reportId;
        protected SysReportTemplateType sysReportTemplateType;
        protected SortedDictionary<int, string> reportGroupAndSortingTypes;

        #endregion

        private bool IsAuthorized
        {
            get
            {
                if (report == null)
                    return true;
                return rm.HasReportRolePermission(report.ReportId, this.RoleId);
            }
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/soe/common/distribution/reports/edit/default.js");
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_Reports_Edit:
                        EnableEconomy = true;
                        TargetSoeModule = SoeModule.Economy;
                        FeatureEdit = Feature.Economy_Distribution_Reports_Edit;
                        FeatureReportGroupMapping = Feature.Economy_Distribution_Reports_ReportGroupMapping;
                        FeatureSelection = Feature.Economy_Distribution_Reports_Selection;
                        FeatureSysTemplateDownload = Feature.Economy_Distribution_SysTemplates_Edit_Download;
                        FeatureTemplateDownload = Feature.Economy_Distribution_Templates_Edit_Download;
                        FeatureReportRolePermission = Feature.Economy_Distribution_Reports_ReportRolePermission;
                        EnableSorting = false;  // No sorting for Economy reports
                        break;
                    case Feature.Billing_Distribution_Reports_Edit:
                        EnableBilling = true;
                        TargetSoeModule = SoeModule.Billing;
                        FeatureEdit = Feature.Billing_Distribution_Reports_Edit;
                        FeatureReportGroupMapping = Feature.Billing_Distribution_Reports_ReportGroupMapping;
                        FeatureSelection = Feature.Billing_Distribution_Reports_Selection;
                        FeatureSysTemplateDownload = Feature.Billing_Distribution_SysTemplates_Edit_Download;
                        FeatureTemplateDownload = Feature.Billing_Distribution_Templates_Edit_Download;
                        FeatureReportRolePermission = Feature.Billing_Distribution_Reports_ReportRolePermission;
                        EnableSorting = false;  // No sorting for Economy reports
                        break;
                    case Feature.Time_Distribution_Reports_Edit:
                        EnableTime = true;
                        TargetSoeModule = SoeModule.Time;
                        FeatureEdit = Feature.Time_Distribution_Reports_Edit;
                        FeatureReportGroupMapping = Feature.Time_Distribution_Reports_ReportGroupMapping;
                        FeatureSelection = Feature.Time_Distribution_Reports_Selection;
                        FeatureSysTemplateDownload = Feature.Time_Distribution_SysTemplates_Edit_Download;
                        FeatureTemplateDownload = Feature.Time_Distribution_Templates_Edit_Download;
                        FeatureReportRolePermission = Feature.Time_Distribution_Reports_ReportRolePermission;
                        EnableSorting = true;  // Sorting for Time reports
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rm = new ReportManager(ParameterObject);
            cm = new CompanyManager(ParameterObject);

            //Mandatory parameters

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath, Request.Url.PathAndQuery);
            
            //Optional parameters
            if (Int32.TryParse(QS["report"], out reportId))
            {
                if (Mode == SoeFormMode.Prev || Mode == SoeFormMode.Next)
                {
                    report = rm.GetPrevNextReport(reportId, (int)TargetSoeModule, SoeCompany.ActorCompanyId, Mode);
                    ClearSoeFormObject();
                    if (report != null)
                        Response.Redirect(Request.Url.AbsolutePath + "?report=" + report.ReportId);
                    else
                        Response.Redirect(Request.Url.AbsolutePath + "?report=" + reportId);
                }
                else
                {
                    report = rm.GetReport(reportId, SoeCompany.ActorCompanyId, loadSysReportTemplateType: true);
                    if (report == null)
                    {
                        Form1.MessageWarning = GetText(1336, "Rapport hittades inte");
                        Mode = SoeFormMode.Register;
                    }
                }
            }

            //Mode
            string editModeTabHeaderText = GetText(1323, "Redigera rapport");
            string registerModeTabHeaderText = GetText(1556, "Registrera rapport");
            PostOptionalParameterCheck(Form1, report, true, editModeTabHeaderText, registerModeTabHeaderText);

            Form1.Title = report != null ? report.Name : "";

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.ReportPermissionMissing);

            #endregion

            #region UserControls

            if (report != null)
            {
                Translations.Visible = HasRolePermission(Feature.Common_Language, Permission.Modify);
                if (Translations.Visible)
                    Translations.InitControl(CompTermsRecordType.ReportName, report.ReportId);
            }

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region Populate

            ReportTemplate.ConnectDataSource(rm.GetReportTemplatesForModuleDict(SoeCompany.ActorCompanyId, (int)TargetSoeModule, true));
            SysReportTemplate.ConnectDataSource(rm.GetSysReportTemplatesForModuleDict((int)TargetSoeModule, true, SoeCompany.ActorCompanyId).OrderBy(r => r.Value));
            ExportType.ConnectDataSource(GetGrpText(TermGroup.ReportExportType));
            ReportExportFileType.ConnectDataSource(GetGrpText(TermGroup.ReportExportFileType));

            // Get sorting orders
            if (EnableSorting && report?.SysReportTemplateTypeId != null)
            {
                bool skipSelections = false;

                if (report.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimePayrollTransactionReport || report.SysReportTemplateTypeId == (int)SoeReportTemplateType.TimePayrollTransactionSmallReport)
                {
                    reportGroupAndSortingTypes = GetGrpTextSorted(TermGroup.ReportGroupAndSortingTypes, true, true, (int)TermGroup_ReportGroupAndSortingTypes.Unknown, (int)TermGroup_ReportGroupAndSortingTypes.PayrollTransactionDate);
                }
                else if (report.SysReportTemplateTypeId == (int)SoeReportTemplateType.EmployeeListReport)
                {
                    reportGroupAndSortingTypes = GetGrpTextSorted(TermGroup.ReportGroupAndSortingTypes, true, true, (int)TermGroup_ReportGroupAndSortingTypes.Unknown, (int)TermGroup_ReportGroupAndSortingTypes.Unknown);
                    reportGroupAndSortingTypes.AddRange(GetGrpTextSorted(TermGroup.ReportGroupAndSortingTypes, true, true, (int)TermGroup_ReportGroupAndSortingTypes.EmployeeCategoryName, (int)TermGroup_ReportGroupAndSortingTypes.EmployeeGender));
                }
                else
                {
                    // If SysReportTemplateType is not EmployeeListReport or TimePayrollTransaction report, selections are not generated. 
                    // For the future usage you may need to add additional report types above, but there is no need to show DivSorting section 
                    // for report types that has no sorting ability
                    skipSelections = true;
                }

                if (!skipSelections)
                {
                    GroupByLevel1.ConnectDataSource(reportGroupAndSortingTypes);
                    GroupByLevel2.ConnectDataSource(reportGroupAndSortingTypes);
                    GroupByLevel3.ConnectDataSource(reportGroupAndSortingTypes);
                    GroupByLevel4.ConnectDataSource(reportGroupAndSortingTypes);
                    SortByLevel1.ConnectDataSource(reportGroupAndSortingTypes);
                    SortByLevel2.ConnectDataSource(reportGroupAndSortingTypes);
                    SortByLevel3.ConnectDataSource(reportGroupAndSortingTypes);
                    SortByLevel4.ConnectDataSource(reportGroupAndSortingTypes);

                    DivSorting.Visible = true;
                }
                else
                {
                    DivSorting.Visible = false;
                }
            }
            else
            {
                DivSorting.Visible = false;
            }

            #endregion

            #region Set data

            if (report != null)
            {
                ReportNr.Value = report.ReportNr.ToString();
                Name.Value = report.Name;
                Description.Value = report.Description;
                ExportType.Value = (report.ExportType != (int)TermGroup_ReportExportType.Unknown ? report.ExportType : (int)TermGroup_ReportExportType.Pdf).ToString();
                ReportExportFileType.Value = report.FileType.ToString();
                ReportExportFileType.Visible = report.ExportType == (int)TermGroup_ReportExportType.File;
                IncludeAllHistoricalData.Value = report.IncludeAllHistoricalData.ToString();
                IncludeAllHistoricalData.Visible = true;
                IncludeBudget.Value = report.IncludeBudget.ToString();
                IncludeBudget.Visible = report.SysReportTemplateTypeId.HasValue && (report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.ResultReport || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.ResultReportV2);
                GetDetailedInformation.Value = report.GetDetailedInformation.ToString();
                GetDetailedInformation.Visible = report.SysReportTemplateTypeId.HasValue && (report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.ResultReport || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.BalanceReport || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.PayrollAccountingReport || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.TimeEmployeeSchedule || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.ResultReportV2 || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.BillingInvoice || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.BillingInvoiceInterest || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.BillingInvoiceReminder);
                NumberOfYearsBackPreviousData.Value = report.NoOfYearsBackinPreviousYear.ToString() != string.Empty ? report.NoOfYearsBackinPreviousYear.ToString() : "0";
                NumberOfYearsBackPreviousData.Visible = report.SysReportTemplateTypeId.HasValue && (report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.ResultReport || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.ResultReportV2);
                ShowInAccountingReports.Value = report.ShowInAccountingReports.ToString();
                ShowInAccountingReports.Visible = report.SysReportTemplateTypeId.HasValue && (report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.ResultReport || report.SysReportTemplateTypeId.Value == (int)SoeReportTemplateType.BalanceReport);
                GroupByLevel1.Value = report.GroupByLevel1.ToString();
                GroupByLevel2.Value = report.GroupByLevel2.ToString();
                GroupByLevel3.Value = report.GroupByLevel3.ToString();
                GroupByLevel4.Value = report.GroupByLevel4.ToString();
                SortByLevel1.Value = report.SortByLevel1.ToString();
                SortByLevel2.Value = report.SortByLevel2.ToString();
                SortByLevel3.Value = report.SortByLevel3.ToString();
                SortByLevel4.Value = report.SortByLevel4.ToString();
                IsSortAscending.Value = report.IsSortAscending.ToString();
                Special.Value = report.Special;

                if (report.Standard)
                    SysReportTemplate.Value = report.ReportTemplateId.ToString();
                else
                    ReportTemplate.Value = report.ReportTemplateId.ToString();

                #region DivImportReportContent

                sysReportTemplateType = rm.GetSysReportTemplateType(report, SoeCompany.ActorCompanyId);
                if (sysReportTemplateType != null)
                {
                    //Only import Report that contains Groups
                    if (sysReportTemplateType.GroupMapping)
                    {
                        //Get Companies in own license
                        List<Company> companies = cm.GetCompaniesByLicense(SoeCompany.LicenseNr, false).ToList<Company>();

                        //Get template Companies
                        IEnumerable<Company> globalTemplateCompanies = cm.GetGlobalTemplateCompanies();
                        foreach (Company globalCompany in globalTemplateCompanies)
                        {
                            globalCompany.Name = globalCompany.Name + "(" + GetText(2138, "Global") + ")";
                            companies.Add(globalCompany);
                        }

                        //Insert dummy Company
                        Company dummyComp = new Company()
                        {
                            ActorCompanyId = 0,
                            Name = "",
                        };
                        companies.Insert(0, dummyComp);

                        ImportCompany.ConnectDataSource(companies, "Name", "ActorCompanyId");

                        DivImportReportContent.Visible = true;
                        ImportCompany.OnClick = "companySelected(" + report.ReportId + "," + sysReportTemplateType.SysReportTemplateTypeId + ")";
                    }
                }

                #endregion
            }
            else
            {
                ExportType.Value = ((int)TermGroup_ReportExportType.Pdf).ToString();

                IncludeAllHistoricalData.Value = Boolean.FalseString;
                IncludeAllHistoricalData.Visible = true;
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess += GetText(1337, "Rapport sparad");
                else if (MessageFromSelf == "NOTSAVED")
                    Form1.MessageError += GetText(1338, "Rapport kunde inte sparas");
                else if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess += GetText(1440, "Rapport uppdaterad");
                else if (MessageFromSelf == "UPDATED_WITHIMPORTERRORS")
                    Form1.MessageWarning += GetText(1636, "Rapport uppdaterad. Notera: Fel inträffade vid import av rapportinnehåll, se logg");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError += GetText(1441, "Rapport kunde inte uppdateras");
                else if (MessageFromSelf == "DELETED")
                    Form1.MessageSuccess = GetText(1972, "Rapport borttagen");
                else if (MessageFromSelf == "NOTDELETED_HASREPORTPACKAGES")
                    Form1.MessageError = GetText(1443, "Rapport kunde inte tas bort") + ". " + GetText(11551, "Rapporten är kopplad till rapportpaket");
                else if (MessageFromSelf == "NOTDELETED_HASPAYROLLGROUPS")
                    Form1.MessageError = GetText(1443, "Rapport kunde inte tas bort") + ". " + GetText(11552, "Rapporten är kopplad till löneavtal");
                else if (MessageFromSelf == "NOTDELETED_HASCHECKLISTS")
                    Form1.MessageError = GetText(1443, "Rapport kunde inte tas bort") + ". " + GetText(11553, "Rapporten är kopplad till checklistor");
                else if (MessageFromSelf == "NOTDELETED")
                    Form1.MessageError = GetText(1443, "Rapport kunde inte tas bort");
                else if (MessageFromSelf == "EXIST")
                    Form1.MessageInformation = GetText(1471, "Rapport finns redan") + ", " + GetText(1936, "angivet rapportnummer används i någon modul");
                else if (MessageFromSelf == "NOTEMPLATE")
                    Form1.MessageWarning = GetText(1368, "Rapportmall måste anges");
                else if (MessageFromSelf == "FILENOTFOUND")
                    Form1.MessageWarning = GetText(1179, "Filen hittades inte");
                else if (MessageFromSelf == "FILEERROR")
                    Form1.MessageError += GetText(1339, "Felaktig fil, kunde inte valideras");
                else if (MessageFromSelf == "IMPORT_ERROR_TEMPLATETYPE")
                    Form1.MessageWarning += GetText(1625, "Import av rapportinnehåll kan inte göras samtidigt som rapportmall ändras");
                else if (MessageFromSelf == "IMPORT_ERROR_IMPORTFROMSELF")
                    Form1.MessageWarning += GetText(1627, "Kan inte importera rapportinnehåll från samma rapport");
                else if(MessageFromSelf == "NOTDELETED_HASCUSTOMERS")
                    Form1.MessageError = GetText(1443, "Rapport kunde inte tas bort") + ". " + GetText(7883, "Rapporten är kopplad till kunder");
            }

            #endregion

            if (!ClientScript.IsStartupScriptRegistered("Page_start"))
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(),
                    "alert", "Page_start();", true);
            }

            #region Navigation

            if (report != null)
            {
                Form1.SetRegLink(GetText(1330, "Registrera rapport"), "",
                    FeatureEdit, Permission.Modify);

                if (sysReportTemplateType != null)
                {
                    if (sysReportTemplateType.GroupMapping)
                    {
                        Form1.AddLink(GetText(2176, "Koppla rapport till rapportgrupper"), "../reportgroupmapping/?report=" + report.ReportId,
                            FeatureReportGroupMapping, Permission.Readonly);
                    }
                }

                if (report.SysReportTemplateTypeSelectionType.HasValue && report.SysReportTemplateTypeSelectionType.Value > 0)
                {
                    Form1.AddLink(report.Original ? GetText(1334, "Skapa urval") : GetText(1335, "Redigera urval"), "../selection/?report=" + report.ReportId,
                        FeatureSelection, Permission.Readonly);
                }

                if (report.Standard)
                {
                    SysReportTemplate sysReportTemplate = rm.GetSysReportTemplate(report.ReportTemplateId);
                    if (sysReportTemplate != null)
                    {
                        Form1.AddLink(GetText(1887, "Ladda ner systemrapportmall"), "../../systemplates/edit/download/?systemplate=" + sysReportTemplate.SysReportTemplateId,
                            FeatureSysTemplateDownload, Permission.Modify);
                    }
                }
                else
                {
                    Form1.AddLink(GetText(1886, "Ladda ner rapportmall"), "../../templates/edit/download/?template=" + report.ReportTemplateId,
                        FeatureTemplateDownload, Permission.Modify);
                }

                Form1.AddLink(GetText(5668, "Koppla mot roller"), "../permissions/?report=" + report.ReportId,
                    FeatureReportRolePermission, Permission.Readonly);
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            int reportNr = Convert.ToInt32(F["ReportNr"]);
            string name = F["Name"];
            string description = F["Description"];
            int exportType = StringUtility.GetInt(F["ExportType"], (int)TermGroup_ReportExportType.Pdf);
            int fileType = StringUtility.GetInt(F["ReportExportFileType"], 0);
            int groupByLevel1 = StringUtility.GetInt(F["GroupByLevel1"], 0);
            int groupByLevel2 = StringUtility.GetInt(F["GroupByLevel2"], 0);
            int groupByLevel3 = StringUtility.GetInt(F["GroupByLevel3"], 0);
            int groupByLevel4 = StringUtility.GetInt(F["GroupByLevel4"], 0);
            int sortByLevel1 = StringUtility.GetInt(F["SortByLevel1"], 0);
            int sortByLevel2 = StringUtility.GetInt(F["SortByLevel2"], 0);
            int sortByLevel3 = StringUtility.GetInt(F["SortByLevel3"], 0);
            int sortByLevel4 = StringUtility.GetInt(F["SortByLevel4"], 0);
            bool isSortAscending = StringUtility.GetBool(F["IsSortAscending"]);
            string special = F["Special"];
            bool includeAllHistoricalData = StringUtility.GetBool(F["IncludeAllHistoricalData"]);
            bool includeBudget = StringUtility.GetBool(F["includeBudget"]);
            bool getDetailedInformation = StringUtility.GetBool(F["GetDetailedInformation"]);
            bool showInAccountingReports = StringUtility.GetBool(F["ShowInAccountingReports"]);
            bool newGroupsAndHeaders = StringUtility.GetBool(F["NewGroupsAndHeaders"]);
            int noOfYearsBackinPreviousYear = F["NumberOfYearsBackPreviousData"] != string.Empty ? Convert.ToInt32(F["NumberOfYearsBackPreviousData"]) : 0;

            bool standard = false;
            if (Int32.TryParse(F["SysReportTemplate"], out int reportTemplateId) && reportTemplateId > 0)
            {
                standard = true;
            }
            else
            {
                if (!Int32.TryParse(F["ReportTemplate"], out reportTemplateId) || reportTemplateId == 0)
                    RedirectToSelf("NOTEMPLATE", true);
            }

            if (report == null)
            {
                #region Add

                if (rm.ReportExist(SoeCompany.ActorCompanyId, reportNr))
                    RedirectToSelf("EXIST", true);

                report = new Report()
                {
                    ReportNr = reportNr,
                    Name = name,
                    Description = description,
                    ExportType = exportType,
                    FileType = fileType,
                    IncludeAllHistoricalData = includeAllHistoricalData,
                    Standard = standard,
                    Original = true,
                    Module = (int)TargetSoeModule,
                    IncludeBudget = includeBudget,
                    GetDetailedInformation = getDetailedInformation,
                    NoOfYearsBackinPreviousYear = noOfYearsBackinPreviousYear,
                    ShowInAccountingReports = showInAccountingReports,

                    //Set FK
                    ReportTemplateId = reportTemplateId,
                    GroupByLevel1 = groupByLevel1,
                    GroupByLevel2 = groupByLevel2,
                    GroupByLevel3 = groupByLevel3,
                    GroupByLevel4 = groupByLevel4,
                    SortByLevel1 = sortByLevel1,
                    SortByLevel2 = sortByLevel2,
                    SortByLevel3 = sortByLevel3,
                    SortByLevel4 = sortByLevel4,
                    IsSortAscending = isSortAscending,
                    Special = special,
                };

                if (rm.AddReport(report, SoeCompany.ActorCompanyId).Success)
                {
                    string postBackUrlQs = "&report=" + report.ReportId;
                    RedirectToSelf("SAVED", postBackUrlQs, true);
                }
                else
                {
                    RedirectToSelf("NOTSAVED");
                }

                #endregion
            }
            else
            {
                #region Update

                //Validation: ReportNr not already exist
                if (report.ReportNr != reportNr)
                {
                    if (rm.ReportExist(SoeCompany.ActorCompanyId, reportNr))
                        RedirectToSelf("EXIST", true);
                }

                bool importReportContent = false;
                int importReportId = 0;
                if (Int32.TryParse(F["ImportCompany"], out int importCompanyId) && Int32.TryParse(F["ImportReport"], out importReportId))
                {
                    if (importCompanyId > 0 && importReportId > 0)
                    {
                        //Validate that User not import Report content from self
                        if (importReportId == report.ReportId)
                            RedirectToSelf("IMPORT_ERROR_IMPORTFROMSELF", true);

                        //Validate that User not changed TemplateType and trying to import at the same time. Wich would cause errors.
                        if (report.Standard)
                        {
                            if (report.ReportTemplateId != Convert.ToInt32(F["SysReportTemplate"]))
                                RedirectToSelf("IMPORT_ERROR_TEMPLATETYPE", true);
                        }
                        else
                        {
                            if (report.ReportTemplateId != Convert.ToInt32(F["ReportTemplate"]))
                                RedirectToSelf("IMPORT_ERROR_TEMPLATETYPE", true);
                        }

                        importReportContent = true;
                    }
                }

                report.ReportNr = reportNr;
                report.Name = name;
                report.Description = description;
                report.ExportType = exportType;
                report.FileType = fileType;
                report.IncludeAllHistoricalData = includeAllHistoricalData;
                report.IncludeBudget = includeBudget;
                report.NoOfYearsBackinPreviousYear = noOfYearsBackinPreviousYear;
                report.Standard = standard;
                report.GetDetailedInformation = getDetailedInformation;
                report.ShowInAccountingReports = showInAccountingReports;
                report.GroupByLevel1 = groupByLevel1;
                report.GroupByLevel2 = groupByLevel2;
                report.GroupByLevel3 = groupByLevel3;
                report.GroupByLevel4 = groupByLevel4;
                report.SortByLevel1 = sortByLevel1;
                report.SortByLevel2 = sortByLevel2;
                report.SortByLevel3 = sortByLevel3;
                report.SortByLevel4 = sortByLevel4;
                report.IsSortAscending = isSortAscending;
                report.Special = special;

                //Set FK
                report.ReportTemplateId = reportTemplateId;

                if (rm.UpdateReport(report, SoeCompany.ActorCompanyId).Success)
                {
                    Translations.SaveTranslations();

                    if (importReportContent)
                    {
                        if (newGroupsAndHeaders)
                        {
                            if (rm.ImportReportHeadersAndReportGroupsFromReportCreateNew(importReportId, report.ReportId, importCompanyId, SoeCompany.ActorCompanyId))
                                RedirectToSelf("UPDATED");
                            else
                                RedirectToSelf("UPDATED_WITHIMPORTERRORS");
                        }
                        else
                        {
                            if (rm.ImportReportHeadersAndReportGroupsFromReportReuseExisting(importReportId, report.ReportId, importCompanyId, SoeCompany.ActorCompanyId))
                                RedirectToSelf("UPDATED");
                            else
                                RedirectToSelf("UPDATED_WITHIMPORTERRORS");
                        }
                    }
                    else
                    {
                        RedirectToSelf("UPDATED");
                    }
                }

                RedirectToSelf("NOTUPDATED", true);

                #endregion
            }
        }

        protected override void Delete()
        {
            var result = rm.DeleteReport(report, SoeCompany.ActorCompanyId);
            if (result.Success)
                RedirectToSelf("DELETED", false, true);
            else
            {
                if (result.ErrorNumber == (int)ActionResultDelete.ReportHasReportPackages)
                    RedirectToSelf("NOTDELETED_HASREPORTPACKAGES", true);
                if (result.ErrorNumber == (int)ActionResultDelete.ReportHasReportPayrollGroups)
                    RedirectToSelf("NOTDELETED_HASPAYROLLGROUPS", true);
                if (result.ErrorNumber == (int)ActionResultDelete.ReportHasReportChecklists)
                    RedirectToSelf("NOTDELETED_HASCHECKLISTS", true);
                if (result.ErrorNumber == (int)ActionResultDelete.ReportHasCustomers)
                    RedirectToSelf("NOTDELETED_HASCUSTOMERS", true);
                else
                    RedirectToSelf("NOTDELETED", true);
            }
        }

        #endregion

    }
}
