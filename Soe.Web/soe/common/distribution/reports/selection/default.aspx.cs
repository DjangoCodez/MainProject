using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SoftOne.Soe.Web.soe.common.distribution.reports.selection
{
    public partial class _default : PageBase
    {
        #region Variables

        private ReportDataManager rdm;
        private ReportManager rm;
        private TermManager tm;


        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }
        private Feature FeatureDownload = Feature.None;
        private Feature FeatureSysTemplateDownload = Feature.None;
        private Feature FeatureTemplateDownload = Feature.None;

        private List<Report> reportsToPrint;
        protected Report report;
        private ReportPackage reportPackage;
        private SysReportTemplateType sysReportTemplateType;
        private SoeSelectionType selectionType;
        private SoeReportTemplateType reportTemplateType;
        private List<EvaluatedSelection> evaluatedSelections;
        private int reportId;
        private int reportPackageId;
        private int exportTypeId;
        private bool email;
        private string guid;

        #endregion

        #region Properties

        private bool IsAuthorized
        {
            get
            {
                bool isAuthorized = true;

                if (this.report != null)
                    isAuthorized = rm.HasReportRolePermission(this.reportId, RoleId);
                else if (this.reportPackage != null)
                    isAuthorized = rm.HasReportRolePermission(this.reportPackage.GetActiveReportIds(), RoleId, SoeCompany.ActorCompanyId);

                return isAuthorized;
            }
        }

        #endregion

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_Reports_Selection:
                        EnableEconomy = true;
                        FeatureDownload = Feature.Economy_Distribution_Reports_Selection_Download;
                        FeatureSysTemplateDownload = Feature.Economy_Distribution_SysTemplates_Edit_Download;
                        FeatureTemplateDownload = Feature.Economy_Distribution_Templates_Edit_Download;
                        break;
                    case Feature.Billing_Distribution_Reports_Selection:
                        EnableBilling = true;
                        FeatureDownload = Feature.Billing_Distribution_Reports_Selection_Download;
                        FeatureSysTemplateDownload = Feature.Billing_Distribution_SysTemplates_Edit_Download;
                        FeatureTemplateDownload = Feature.Billing_Distribution_Templates_Edit_Download;
                        break;
                    case Feature.Time_Distribution_Reports_Selection:
                        EnableTime = true;
                        FeatureDownload = Feature.Time_Distribution_Reports_Selection_Download;
                        FeatureSysTemplateDownload = Feature.Time_Distribution_SysTemplates_Edit_Download;
                        FeatureTemplateDownload = Feature.Time_Distribution_Templates_Edit_Download;
                        break;
                }
            }
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            //Add scripts and style sheets
            Scripts.Add("/soe/common/distribution/reports/selection/default.js");

            if (Master.Master is BaseMaster masterpage)
            {
                this.Form1.ClientIDMode = System.Web.UI.ClientIDMode.Predictable;
                masterpage.BodyTag.Attributes.Add("onkeydown", "masterKeyDown(event, '" + this.Form1.ClientID + "')");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rdm = new ReportDataManager(ParameterObject);
            rm = new ReportManager(ParameterObject);
            tm = new TermManager(ParameterObject);

            //Mandatory parameters
            if (Int32.TryParse(QS["report"], out this.reportId))
            {
                this.report = rm.GetReport(this.reportId, SoeCompany.ActorCompanyId);
                if (this.report == null)
                {
                    Form1.MessageWarning = GetText(1336, "Rapport hittades inte");
                    Form1.DisableSave = true;
                    Form1.EnableRunReport = false;
                    SetupEnvironment();
                    return;
                }

                this.sysReportTemplateType = rm.GetSysReportTemplateType(this.report, SoeCompany.ActorCompanyId);
                if (this.sysReportTemplateType == null)
                {
                    Form1.MessageWarning = GetText(1352, "Rapportmall hittades inte");
                    Form1.DisableSave = true;
                    Form1.EnableRunReport = false;
                    SetupEnvironment();
                    return;
                }

                this.selectionType = (SoeSelectionType)this.sysReportTemplateType.SelectionType;
                this.reportTemplateType = (SoeReportTemplateType)this.sysReportTemplateType.SysReportTemplateTypeId;
            }
            else if (Int32.TryParse(QS["package"], out this.reportPackageId))
            {
                this.reportPackage = rm.GetReportPackage(this.reportPackageId, SoeCompany.ActorCompanyId, true, true);
                if (this.reportPackage == null)
                {
                    Form1.MessageWarning = GetText(1427, "Rapportpaket hittades inte");
                    Form1.DisableSave = true;
                    Form1.EnableRunReport = false;
                    return;
                }

                this.selectionType = SoeSelectionType.None;
            }
            else
            {
                RedirectToModuleRoot();
                return;
            }

            //Optional
            if (!String.IsNullOrEmpty(QS["email"]))
                this.email = Boolean.Parse(QS["email"]);
            if (!String.IsNullOrEmpty(QS["guid"]))
                this.guid = QS["guid"];
            else
                this.guid = Guid.NewGuid().ToString();

            if (!HasRolePermission(FeatureDownload, Permission.Modify))
                Form1.EnableRunReport = false;

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath + "?report=" + this.reportId, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            bool condition = this.report != null && !this.report.Original;
            PostOptionalParameterCheck(Form1, this.report, condition);

            if (this.report != null)
            {
                var compTerm = tm.GetCompTerm(CompTermsRecordType.ReportName, this.report.ReportId, this.GetLanguageId());
                if (compTerm != null && !string.IsNullOrEmpty(compTerm.Name))
                    report.Name = compTerm.Name;
            }

            Form1.Title = this.report != null ? this.report.Name : "";

            if (this.email)
                Mode = SoeFormMode.RunReport;

            //Cannot update a original Report
            if (Form1.Mode == SoeFormMode.Update && this.report != null && this.report.Original)
            {
                Mode = SoeFormMode.Register;
                Form1.Mode = Mode;
            }

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.ReportPermissionMissing);

            #endregion

            #region Setup

            SetupParameters();
            SetupEnvironment();

            #endregion

            #region Actions

            bool printReport = Mode == SoeFormMode.RunReport;
            bool back = Mode == SoeFormMode.Back;
            bool saveReport = !printReport && !back && Form1.IsPosted;

            if (printReport)
            {
                ClearSoeFormObject();
                Print();
            }
            else if (back)
            {
                ClearSoeFormObject();
                Back();
            }
            else if (saveReport)
            {
                Save();
            }

            #endregion

            #region Populate

            Populate();

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                //Prereq
                if (MessageFromSelf == "EVALUATE_FAILED")
                    Form1.MessageWarning = GetText(1450, "Felaktigt urval");
                else if (MessageFromSelf == "PACKAGE_HAS_NO_REPORTS")
                    Form1.MessageWarning = GetText(1843, "Rapportpaket innehåller inga rapporter");
                else if (MessageFromSelf == "EXPORTTYPE_MANDATORY")
                {
                    Form1.MessageWarning = GetText(1429, "Exporttyp måste anges");
                    Form1.ActiveTab = 2;
                }

                //Run
                else if (MessageFromSelf == "NO_DATA")
                    Form1.MessageError = GetText(1942, "Rapporturval gav ingen data");
                else if (MessageFromSelf == "RUN_FAILED")
                    Form1.MessageError = GetText(1576, "Rapportkörning misslyckades, se logg");
                else if (MessageFromSelf == "REPORT_HAS_NO_GROUPS_OR_HEADERS")
                    Form1.MessageError = GetText(1400, "Rapport har inga grupper eller rubriker");
                else if (MessageFromSelf == "REPORTTEMPLATE_NOT_FOUND")
                    Form1.MessageError = GetText(5977, "Rapportmall hittades inte");
                else if (MessageFromSelf == "REPORT_NOT_AUTHORIZED")
                    Form1.MessageError = GetText(5976, "Behörighet saknas");

                //ReportSelection
                else if (MessageFromSelf == "SAVED")
                    Form1.MessageSuccess = GetText(1447, "Urval sparat");
                else if (MessageFromSelf == "SAVE_FAILED")
                    Form1.MessageError = GetText(1451, "Urval kunde inte sparas");
                else if (MessageFromSelf == "REPORTSELECTIONTEXT_MANDATORY")
                    Form1.MessageWarning = GetText(1446, "Urvalsnamn måste anges");

                //Other
                else
                {
                    //Evaluation from UserControls failed (sends specific message)
                    Form1.MessageWarning = MessageFromSelf;
                }
            }

            #endregion

            #region Navigation

            if (this.report != null)
            {
                if (this.report.Standard)
                {
                    SysReportTemplate sysReportTemplate = rm.GetSysReportTemplate(this.report.ReportTemplateId);
                    if (sysReportTemplate != null)
                    {
                        Form1.AddLink(GetText(1887, "Ladda ner systemrapportmall"), "/soe/economy/distribution/systemplates/edit/download/?systemplate=" + sysReportTemplate.SysReportTemplateId,
                            FeatureSysTemplateDownload, Permission.Modify);
                    }
                }
                else
                {
                    Form1.AddLink(GetText(1886, "Ladda ner rapportmall"), "/soe/economy/distribution/templates/edit/download/?template=" + this.report.ReportTemplateId,
                        FeatureTemplateDownload, Permission.Modify);
                }
            }

            #endregion
        }

        #region Action-methods

        /// <summary>
        /// Evaluates input.
        /// RedirectToSelf with message if failed.
        /// </summary>
        private void Evaluate()
        {
            #region Init

            if (this.evaluatedSelections == null)
                this.evaluatedSelections = new List<EvaluatedSelection>();

            EvaluateExportType();
            EvaluateReports();

            #endregion

            #region Evaluate

            bool evaluated = false;

            if (this.report != null)
            {
                #region Report

                Selection selection = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName, report: report.ToDTO(), isMainReport: true, exportType: this.exportTypeId);

                if (rm.IsAccountingReport((int)this.selectionType))
                {
                    #region Accounting

                    //SelectionStd
                    SelectionStd.F = F;
                    selection.SelectionStd = new SelectionStd();
                    if (SelectionStd.Evaluate(selection.SelectionStd, selection.Evaluated))
                    {
                        //SelectionVoucher
                        SelectionVoucher.F = F;
                        selection.SelectionVoucher = new SelectionVoucher();
                        if (SelectionVoucher.Evaluate(selection.SelectionVoucher, selection.Evaluated))
                        {
                            //SelectionAccount
                            SelectionAccount.F = F;
                            selection.SelectionAccount = new SelectionAccount();
                            evaluated = SelectionAccount.Evaluate(selection.SelectionAccount, selection.Evaluated);
                        }

                        SelectionFixedAssets.F = F;
                        selection.SelectionFixedAssets = new SelectionFixedAssets();
                        if (SelectionFixedAssets.Evaluate(selection.SelectionFixedAssets, selection.Evaluated))
                        {
                            evaluated = SelectionFixedAssets.Evaluate(selection.SelectionFixedAssets, selection.Evaluated);
                        }
                    }

                    #endregion
                }
                else if (rm.IsLedgerReport((int)this.selectionType))
                {
                    #region Ledger

                    SelectionLedger.F = F;
                    selection.SelectionLedger = new SelectionLedger();
                    evaluated = SelectionLedger.Evaluate(selection.SelectionLedger, selection.Evaluated);

                    #endregion
                }
                else if (rm.IsBillingReport(this.report.Module, (int)this.selectionType, this.sysReportTemplateType.SysReportTemplateTypeId))
                {
                    #region Billing

                    SelectionBilling.F = F;
                    selection.SelectionBilling = new SelectionBilling();
                    SelectionBilling.Evaluate(selection.SelectionBilling, selection.Evaluated);

                    SelectionAccount.F = F;
                    selection.SelectionAccount = new SelectionAccount();
                    evaluated = SelectionAccount.Evaluate(selection.SelectionAccount, selection.Evaluated);

                    #region TimeProjectReport

                    if (evaluated && selection.SelectionBilling.IncludeProjectReport)
                    {
                        Report timeProjectReport = rm.GetBillingInvoiceProjectTimeProjectReport(this.reportTemplateType, SoeCompany.ActorCompanyId, RoleId);
                        if (timeProjectReport != null)
                        {
                            Selection timeProjectReportSelection = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName, timeProjectReport.ToDTO(), exportType: this.exportTypeId);
                            timeProjectReportSelection.SelectionBilling = new SelectionBilling();
                            evaluated = SelectionBilling.Evaluate(timeProjectReportSelection.SelectionBilling, timeProjectReportSelection.Evaluated);
                            this.evaluatedSelections.Add(timeProjectReportSelection.Evaluated);
                        }
                    }

                    #endregion

                    #endregion
                }

                this.evaluatedSelections.Add(selection.Evaluated);

                #endregion
            }
            else if (this.reportPackage != null)
            {
                #region ReportPackage

                if (this.reportsToPrint.Count == 0)
                    RedirectToSelf("PACKAGE_HAS_NO_REPORTS", true);

                foreach (Report reportToPrint in this.reportsToPrint)
                {
                    #region Report

                    //Common
                    Selection selection = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName, reportToPrint.ToDTO(), this.reportPackage.ToDTO(), exportType: this.exportTypeId);

                    //SelectionStd
                    SelectionStd.F = Request.Form;
                    selection.SelectionStd = new SelectionStd();
                    if (SelectionStd.Evaluate(selection.SelectionStd, selection.Evaluated))
                    {
                        //SelectionVoucher
                        selection.SelectionVoucher = rm.GetReportSelectionVoucher(reportToPrint.ReportId, SoeCompany.ActorCompanyId);
                        SelectionVoucher.SetEvaluated(selection.SelectionVoucher, selection.Evaluated);

                        //SelectionAccount
                        selection.SelectionAccount = rm.GetReportSelectionAccount(reportToPrint.ReportId, SoeCompany.ActorCompanyId);
                        SelectionAccount.SetEvaluated(selection.SelectionAccount, selection.Evaluated);

                        //SeletionLedger
                        SelectionLedger.SetDefaultValues(selection.Evaluated);
                        selection.SelectionLedger = rm.GetReportSelectionLedger(reportToPrint.ReportId, SoeCompany.ActorCompanyId);
                        SelectionLedger.SetEvaluated(selection.SelectionLedger, selection.Evaluated);




                        //TODO: SelectionBilling

                        this.evaluatedSelections.Add(selection.Evaluated);
                        evaluated = true;
                    }

                    #endregion
                }

                if (this.evaluatedSelections.Count == 0)
                    RedirectToSelf("PACKAGE_HAS_NO_REPORTS", true);

                #endregion
            }

            //Set default message if empty
            if (!evaluated)
            {
                string message = Form1.Message;
                if (String.IsNullOrEmpty(message))
                    message = "EVALUATE_FAILED";
                RedirectToSelf(message, true);
            }

            #endregion
        }

        protected void Back()
        {
            string sectionUrl = UrlUtil.GetSectionUrl(Module, Constants.SOE_SECTION_DISTRIBUTION);
            string reportsUrl = sectionUrl + "reports/";
            Response.Redirect(reportsUrl);
        }


        /// <summary>
        /// Saves ReportSelection.
        /// RedirectToSelf with success or error message.
        /// </summary>
        protected override void Save()
        {
            base.Save();

            #region Evaluate

            Evaluate();

            #endregion

            #region Save

            int? savedReportId = null;

            //Can only save one ReportSelection, not selections for a ReportPackage
            EvaluatedSelection es = this.evaluatedSelections[0];
            if (es != null)
            {
                //ReportSelectionText is mandatory
                es.ReportSelectionText = F["ReportSelectionText"];
                if (String.IsNullOrEmpty(es.ReportSelectionText))
                    RedirectToSelf("REPORTSELECTIONTEXT_MANDATORY", true);

                var result = rm.SaveReportSelection(es);
                if (result.Success)
                    savedReportId = result.IntegerValue;
            }

            if (savedReportId.HasValue)
            {
                //Redirect to self (with new reportId if it was saved from a original)
                this.reportId = savedReportId.Value;
                string postBackUrlQs = "&report=" + this.reportId;
                RedirectToSelf("SAVED", postBackUrlQs);
            }
            else
            {
                string message = Form1.Message;
                if (String.IsNullOrEmpty(message))
                    message = "SAVE_FAILED";
                RedirectToSelf(message, true);
            }

            #endregion
        }

        /// <summary>
        /// Prints report.
        /// RedirectToSelf with message if failed.
        /// Redirect to download if succeeded.
        /// </summary>
        protected override void Print()
        {
            #region Evaluate

            Evaluate();

            #endregion

            #region Print

            int reportPrintoutId = 0;

            if (UseCrystalService())
            {
                #region Print from Crystal service

                try
                {
                    string culture = Thread.CurrentThread.CurrentCulture.Name;
                    var channel = GetCrystalServiceChannel();
                    if (this.evaluatedSelections.Count > 1)
                        reportPrintoutId = channel.PrintReportPackageData(this.evaluatedSelections, SoeCompany.ActorCompanyId, UserId, culture);
                    else
                        reportPrintoutId = channel.PrintReport(this.evaluatedSelections[0], SoeCompany.ActorCompanyId, UserId, culture);
                }
                catch (Exception ex)
                {
                    SysLogManager.LogError<_default>(ex);
                }

                #endregion
            }
            else if (UseWebApiInternal())
            {
                try
                {
                    string culture = Thread.CurrentThread.CurrentCulture.Name;
                    var connector = new ReportConnector();
                    if (this.evaluatedSelections.Count > 1)
                        reportPrintoutId = connector.PrintReportPackageData(this.evaluatedSelections, SoeCompany.ActorCompanyId, UserId, culture);
                    else
                        reportPrintoutId = connector.PrintReport(this.evaluatedSelections[0], SoeCompany.ActorCompanyId, UserId, culture);
                }
                catch (Exception ex)
                {
                    SysLogManager.LogError<_default>(ex);
                }
            }
            else
            {
                #region Print from Web

                if (this.evaluatedSelections.Count > 1)
                    reportPrintoutId = rdm.PrintReportPackageId(this.evaluatedSelections);
                else
                    reportPrintoutId = rdm.PrintReportId(this.evaluatedSelections[0]);

                #endregion
            }

            ReportPrintout reportPrintout = rm.GetReportPrintout(reportPrintoutId, SoeCompany.ActorCompanyId);
            if (rm.DoShowReportPrintoutErrorMessage(reportPrintout))
            {
                RedirectToSelf(reportPrintout != null ? ParseResult(reportPrintout.ResultMessage) : "Errormessage missing", true);
                return;
            }

            #endregion

            #region Download

            Session[Constants.SESSION_DOWNLOAD_REPORT_ITEM + this.guid] = reportPrintout;
            Response.Redirect($"download/?c={SoeCompany.ActorCompanyId}&r={RoleId}&email={this.email}&guid={this.guid}");

            #endregion
        }

        #endregion

        #region Help-methods

        private void SetupParameters()
        {
            if (this.report != null)
            {
                #region Report

                //Set UserControl parameters
                if (rm.IsAccountingReport((int)this.selectionType))
                {
                    SelectionStd.SoeForm = Form1;
                    SelectionVoucher.SoeForm = Form1;
                    SelectionAccount.SoeForm = Form1;
                    SelectionFixedAssets.SoeForm = Form1;
                }
                else if (rm.IsLedgerReport((int)this.selectionType))
                {
                    SelectionLedger.SoeForm = Form1;
                    SelectionLedger.SelectionType = this.selectionType;
                    SelectionLedger.ReportTemplateType = this.reportTemplateType;
                }
                else if (rm.IsBillingReport(this.report.Module, (int)this.selectionType, this.sysReportTemplateType.SysReportTemplateTypeId))
                {
                    SelectionBilling.SoeForm = Form1;
                    SelectionAccount.SoeForm = Form1;
                }

                #endregion
            }
            else if (this.reportPackage != null)
            {
                #region ReportPackage

                SelectionStd.SoeForm = Form1;

                #endregion
            }
        }

        private void SetupEnvironment()
        {
            ParseSelectionType();

            if (this.report != null)
            {
                #region Report

                LoadReportSelection();

                #endregion
            }
            else if (this.reportPackage != null)
            {
                #region ReportPackage

                SelectionStd.Visible = true;

                #endregion
            }

            //Not implemented
            ReportExport.Visible = false;
        }

        private void Populate()
        {
            //Populate (after ReportSelection has been set on UserControls)
            bool repopulate = Mode == SoeFormMode.Repopulate;
            int accountYearIdFrom;
            int accountYearIdTo;

            ExportType.ConnectDataSource(GetGrpText(TermGroup.ReportExportType));
            ExportType.OnChange = "showHideExportType();";

            if (this.report != null)
            {
                #region Report

                ExportType.Value = (this.report.ExportType != (int)TermGroup_ReportExportType.Unknown ? this.report.ExportType : (int)TermGroup_ReportExportType.Pdf).ToString();

                if (rm.IsAccountingReport((int)this.selectionType))
                {
                    //Bug: 
                    //PageBase.Scripts is null in method Populate() 
                    //in a UserControl that lives in a Page that is navigated to from Server.Transfer
                    Scripts.Add("/UserControls/DistributionSelectionStd.js");
                    Scripts.Add("/UserControls/DistributionSelectionBilling.js");

                    SelectionStd.Populate(repopulate, this.reportTemplateType, this.report.IncludeBudget);
                    SelectionStd.GetSelectedAccountYearId(repopulate, out accountYearIdFrom, out accountYearIdTo);
                    SelectionVoucher.Populate(repopulate, accountYearIdFrom, accountYearIdTo);
                    SelectionAccount.Populate(repopulate);
                    SelectionFixedAssets.Populate(repopulate);
                }
                else if (rm.IsLedgerReport((int)this.selectionType))
                {
                    SelectionLedger.Populate(repopulate);
                }
                else if (rm.IsBillingReport(this.report.Module, (int)this.selectionType, this.sysReportTemplateType.SysReportTemplateTypeId))
                {
                    //Bug: 
                    //PageBase.Scripts is null in method Populate() 
                    //in a UserControl that lives in a Page that is navigated to from Server.Transfer
                    Scripts.Add("/UserControls/DistributionSelectionStd.js");
                    Scripts.Add("/UserControls/DistributionSelectionBilling.js");

                    SelectionBilling.Populate(repopulate, this.reportTemplateType);
                    SelectionAccount.Populate(repopulate);
                }

                #endregion
            }
            else if (this.reportPackage != null)
            {
                #region ReportPackage

                //Add scripts and style sheets
                Scripts.Add("/UserControls/DistributionSelectionStd.js");

                ExportType.Value = ((int)TermGroup_ReportExportType.Pdf).ToString();

                SelectionStd.Populate(repopulate, this.reportTemplateType);
                SelectionStd.GetSelectedAccountYearId(repopulate, out accountYearIdFrom, out accountYearIdTo);

                #endregion
            }
        }

        private void EvaluateReports()
        {
            if (this.report != null)
                this.reportsToPrint = new List<Report>() { this.report };
            else if (this.reportPackage != null)
                this.reportsToPrint = this.reportPackage.GetActiveReports();
            else
                RedirectToSelf("EVALUATE_FAILED", true);
        }

        private void EvaluateExportType()
        {
            //Must be passed
            if (!Int32.TryParse(F["ExportType"], out this.exportTypeId))
                RedirectToSelf("EXPORTTYPE_MANDATORY", true);
        }

        public string ParseResult(int resultMessage)
        {
            string result = "";

            switch (resultMessage)
            {
                #region Core

                case (int)SoeReportDataResultMessage.DocumentNotCreated:
                    result = "NO_DATA";
                    break;
                case (int)SoeReportDataResultMessage.EmptyInput:
                case (int)SoeReportDataResultMessage.ReportFailed:
                    result = "RUN_FAILED";
                    break;
                case (int)SoeReportDataResultMessage.ReportTemplateDataNotFound:
                    result = "REPORTTEMPLATE_NOT_FOUND";
                    break;

                #endregion

                #region Economy

                case (int)SoeReportDataResultMessage.BalanceReportHasNoGroupsOrHeaders:
                case (int)SoeReportDataResultMessage.ResultReportHasNoGroupsOrHeaders:
                    result = "REPORT_HAS_NO_GROUPS_OR_HEADERS";
                    break;

                #endregion

                #region Ledger

                #endregion

                #region Billing

                case (int)SoeReportDataResultMessage.EdiEntryNotFound:
                case (int)SoeReportDataResultMessage.EdiEntryCouldNotParseXML:
                case (int)SoeReportDataResultMessage.EdiEntryCouldNotSavePDF:
                    //NA
                    break;

                #endregion

                #region Time

                case (int)SoeReportDataResultMessage.ReportsNotAuthorized:
                    result = "REPORT_NOT_AUTHORIZED";
                    break;

                    #endregion

                    #region Import

                    #endregion
            }

            return result;
        }

        #region SelectionType

        private void ParseSelectionType()
        {
            switch (this.selectionType)
            {
                #region Accounting

                case SoeSelectionType.Accounting:
                    ShowSections(true, false, false, false);
                    SelectionStd.Visible = true;
                    SelectionVoucher.Visible = true;
                    SelectionAccount.Visible = true;
                    SelectionFixedAssets.Visible = false;
                    break;
                case SoeSelectionType.Accounting_ExcludeVoucher:
                    ShowSections(true, false, false, false);
                    SelectionStd.Visible = true;
                    SelectionVoucher.Visible = false;
                    SelectionAccount.Visible = true;
                    SelectionFixedAssets.Visible = false;
                    break;
                case SoeSelectionType.Accounting_ExcludeVoucherAndDate:
                    ShowSections(true, false, false, false);
                    SelectionStd.DisableDateSelection = true;
                    SelectionStd.Visible = true;
                    SelectionVoucher.Visible = false;
                    SelectionAccount.Visible = true;
                    SelectionFixedAssets.Visible = false;
                    break;
                case SoeSelectionType.Accounting_ExcludeVoucherAccountAndDate:
                    ShowSections(true, false, false, false);
                    SelectionStd.DisableDateSelection = true;
                    SelectionStd.Visible = true;
                    SelectionVoucher.Visible = false;
                    SelectionAccount.Visible = false;
                    SelectionFixedAssets.Visible = false;
                    break;
                case SoeSelectionType.Accounting_FixedAssets:
                    ShowSections(true, false, false, false);
                    SelectionStd.Visible = true;
                    SelectionVoucher.Visible = false;
                    SelectionAccount.Visible = false;
                    SelectionFixedAssets.Visible = true;
                    break;

                #endregion

                #region Ledger

                case SoeSelectionType.Ledger_Supplier:
                case SoeSelectionType.Ledger_Customer:
                    ShowSections(false, true, false, false);
                    SelectionStd.Visible = false;
                    SelectionFixedAssets.Visible = false;
                    break;
                case SoeSelectionType.Ledger_Supplier_ExcludeAllButNr:
                case SoeSelectionType.Ledger_Customer_ExcludeAllButNr:
                    ShowSections(false, true, false, false);
                    SelectionStd.Visible = false;
                    SelectionLedger.DisableAllButNr = true;
                    SelectionFixedAssets.Visible = false;
                    break;

                #endregion

                #region Billing

                case SoeSelectionType.Billing_Invoice:
                    ShowSections(true, false, true, false);
                    SelectionStd.Visible = false;
                    SelectionVoucher.Visible = false;
                    SelectionFixedAssets.Visible = false;

                    switch (reportTemplateType)
                    {
                        case SoeReportTemplateType.BillingOffer:
                        case SoeReportTemplateType.BillingContract:
                        case SoeReportTemplateType.BillingInvoice:
                        case SoeReportTemplateType.BillingInvoiceInterest:
                        case SoeReportTemplateType.BillingInvoiceReminder:
                            SelectionAccount.Visible = false;
                            break;
                        case SoeReportTemplateType.BillingOrder:
                        case SoeReportTemplateType.BillingOrderOverview:
                        default:
                            SelectionAccount.Visible = true;
                            break;
                    }

                    break;
                case SoeSelectionType.Billing_HousholdTaxDeduction:
                case SoeSelectionType.Billing_Stock:
                    ShowSections(false, false, true, false);
                    SelectionStd.Visible = false;
                    SelectionFixedAssets.Visible = false;
                    break;

                #endregion

                #region Time

                case SoeSelectionType.Time_Report:
                    ShowSections(false, false, false, false);
                    SelectionStd.Visible = false;
                    DivReportSelectionText.Visible = false;
                    SelectionFixedAssets.Visible = false;
                    break;

                #endregion

                #region None/Default

                case SoeSelectionType.None:
                default:
                    ShowSections(false, false, false, false);
                    DivReportSelectionText.Visible = false;
                    Form1.DisableSave = true;
                    SelectionFixedAssets.Visible = false;

                    break;

                    #endregion
            }
        }

        private void ShowSections(bool accountingVisible, bool ledgerVisible, bool billingVisible, bool timeVisible)
        {
            //Show/Hide div's
            DivSelectionAccounting.Visible = accountingVisible;
            DivSelectionLedger.Visible = ledgerVisible;
            DivSelectionBilling.Visible = billingVisible;
            DivSelectionTime.Visible = timeVisible;
        }

        #endregion

        #region ReportSelection

        private void LoadReportSelection()
        {
            //Get saved ReportSelection
            ReportSelection reportSelection = rm.GetReportSelection(this.reportId, SoeCompany.ActorCompanyId);
            if (reportSelection != null)
            {
                ReportSelectionText.Value = reportSelection.ReportSelectionText;

                if (rm.IsAccountingReport((int)this.selectionType))
                {
                    SelectionStd.ReportSelection = reportSelection;
                    SelectionVoucher.ReportSelection = reportSelection;
                    SelectionAccount.ReportSelection = reportSelection;
                    SelectionFixedAssets.ReportSelection = reportSelection;
                }
                else if (rm.IsLedgerReport((int)this.selectionType))
                {
                    SelectionLedger.ReportSelection = reportSelection;
                    SelectionLedger.SelectionType = this.selectionType;
                    SelectionLedger.ReportTemplateType = this.reportTemplateType;
                }
                else if (rm.IsBillingReport(this.report.Module, (int)this.selectionType, this.sysReportTemplateType.SysReportTemplateTypeId))
                {
                    SelectionBilling.ReportSelection = reportSelection;
                    SelectionAccount.ReportSelection = reportSelection;
                }
                else if (rm.IsTimeReport(this.report.Module, (int)this.selectionType))
                {
                    //Silveright GUI
                }
            }
        }

        #endregion

        #endregion
    }
}
