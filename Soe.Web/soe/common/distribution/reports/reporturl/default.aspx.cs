using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SoftOne.Soe.Web.soe.common.distribution.reports.reporturl
{
    public partial class _default : PageBase
    {

        #region Constants

        private const string EMPLOYEE = "emp";
        private const string TIMEPERIOD = "per";
        private const string PRELIMINARY = "prel";

        #endregion

        #region Variables

        private ReportDataManager rdm;
        private ReportManager rm;
        private SettingManager sm;
        private InvoiceManager im;
        private TermManager tm;
        private Feature FeatureDownload = Feature.None;
        private string guid;
        private Dictionary<string, string> parametersDict;
        private int reportId;
        private int? reportUrlId = null;
        private int exportTypeId;
        private int exportFileTypeId;
        private bool isEmailFromAngular = false;

        private int sysReportTemplateTypeId;
        private SoeReportTemplateType reportTemplateType;
        private Report report;
        private Selection selection;
        private List<EvaluatedSelection> esc;
        private ReportSelectionDTO reportItem;

        #endregion

        #region Properties

        private bool IsAuthorized
        {
            get
            {
                bool isAuthorized = true;
                if (this.report != null)
                    isAuthorized = rm.HasReportRolePermission(this.reportId, RoleId);
                return isAuthorized;
            }
        }

        public bool IsTimeProjectReport
        {
            get
            {
                return reportTemplateType == SoeReportTemplateType.TimeProjectReport && reportItem is BillingInvoiceTimeProjectReportDTO && (reportItem as BillingInvoiceTimeProjectReportDTO).UploadToInexchange;
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
                        FeatureDownload = Feature.Economy_Distribution_Reports_Selection_Download;
                        break;
                    case Feature.Billing_Distribution_Reports_Selection:
                        FeatureDownload = Feature.Billing_Distribution_Reports_Selection_Download;
                        break;
                    case Feature.Time_Distribution_Reports_Selection:
                        FeatureDownload = Feature.Time_Distribution_Reports_Selection_Download;
                        break;
                }
            }
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            isEmailFromAngular = (this.Url.Contains("email=True") && this.Url.Contains("angular=True"));

            if (isEmailFromAngular)
            {
                this.MasterPageFile = "";
            }

        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rdm = new ReportDataManager(ParameterObject);
            rm = new ReportManager(ParameterObject);
            sm = new SettingManager(ParameterObject);
            tm = new TermManager(ParameterObject);
            im = new InvoiceManager(ParameterObject);

            //Mandatory parameters
            if (!Int32.TryParse(QS["report"], out this.reportId))
            {
                ShowError(GetText(5971, "Rapport kunde inte skrivas ut"), GetText(5972, "Felaktigt anrop"));
                return;
            }
            if (!Int32.TryParse(QS["templatetype"], out sysReportTemplateTypeId))
            {
                ShowError(GetText(5971, "Rapport kunde inte skrivas ut"), GetText(5972, "Felaktigt anrop"));
                return;
            }
            if (String.IsNullOrEmpty(QS["guid"]))
            {
                ShowError(GetText(5971, "Rapport kunde inte skrivas ut"), GetText(5972, "Felaktigt anrop"));
                return;
            }
            if (!HasRolePermission(FeatureDownload, Permission.Modify) || !IsAuthorized)
            {
                ShowError(GetText(5971, "Rapport kunde inte skrivas ut"), GetText(5973, "Behörighet saknas"));
                return;
            }

            this.reportTemplateType = (SoeReportTemplateType)sysReportTemplateTypeId;
            this.guid = QS["guid"];

            this.report = rm.GetReport(this.reportId, SoeCompany.ActorCompanyId);
            if (this.report != null)
            {
                var compTerm = tm.GetCompTerm(CompTermsRecordType.ReportName, this.report.ReportId, this.GetLanguageId());
                if (compTerm != null && !String.IsNullOrEmpty(compTerm.Name))
                    report.Name = compTerm.Name;
            }
            else
            {
                ShowError(GetText(5971, "Rapport kunde inte skrivas ut"), GetText(1336, "Rapport hittades inte"));
                return;
            }

            //ExportType 
            Int32.TryParse(QS["exporttype"], out this.exportTypeId);
            if (this.exportTypeId <= 0) //Prio 1: Take passed ExportType by QS (set earlier)
            {
                if (this.report != null && this.report.ExportType != (int)TermGroup_ReportExportType.Unknown)
                    this.exportTypeId = this.report.ExportType; //Prio 2: Take ExportType on Report
                else
                    this.exportTypeId = (int)TermGroup_ReportExportType.Pdf; //Prio 3: Take default ExportType PDF
            }

            //ExportFileType 
            Int32.TryParse(QS["exportfiletype"], out this.exportFileTypeId);
            if (this.exportFileTypeId <= 0)
            {
                this.exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown;
            }

            #endregion

            #region Print

            Print();

            #endregion
        }

        #region Action-methods

        protected override void Print()
        {
            #region Parse selection

            bool upload = false;
            int reportPrintoutId = 0;
            Dictionary<int, int> exportFileReportPrintoutIdsDict = new Dictionary<int, int>();

            ParseSelection();

            if (this.reportItem == null)
            {
                ShowError(GetText(5971, "Rapport kunde inte skrivas ut"), GetText(5974, "Urval kunde inte tolkas"));
                return;
            }

            #endregion

            #region Print

            string culture = Thread.CurrentThread.CurrentCulture.Name;
            var channel = UseCrystalService() ? GetCrystalServiceChannel() : null;

            #region TimeProjectReport

            if (this.IsTimeProjectReport)
            {
                upload = true;

                foreach (int invoiceId in ((BillingInvoiceTimeProjectReportDTO)reportItem).InvoiceIds)
                {
                    EvaluatedSelection es = this.esc[0];
                    es.SB_InvoiceIds = new List<int>() { invoiceId };
                    es.ReportNamePostfix = "TPR" + "_" + im.GetInvoiceNr(invoiceId) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");

                    if (channel != null)
                    {
                        reportPrintoutId = channel.PrintReport(es, SoeCompany.ActorCompanyId, UserId, culture);
                    }
                    else if (UseWebApiInternal())
                    {
                        ReportConnector connector = new ReportConnector();
                        reportPrintoutId = connector.PrintReport(es, SoeCompany.ActorCompanyId, UserId, culture);
                    }
                    else
                    {
                        reportPrintoutId = rdm.PrintReportId(es);
                    }

                    if (reportPrintoutId > 0)
                        exportFileReportPrintoutIdsDict.Add(reportPrintoutId, invoiceId);
                }
            }

            #endregion

            if (UseCrystalService())
            {
                #region Print from Crystal service

                try
                {
                    if (channel != null)
                    {
                        if (this.esc.Count > 1)
                            reportPrintoutId = channel.PrintReportPackageData(this.esc, SoeCompany.ActorCompanyId, UserId, culture);
                        else
                            reportPrintoutId = channel.PrintReport(this.esc[0], SoeCompany.ActorCompanyId, UserId, culture);
                    }
                }
                catch (Exception ex)
                {
                    SysLogManager.LogError<_default>(ex);
                }

                #endregion
            }
            else if (this.reportTemplateType != SoeReportTemplateType.PayrollSlip && this.reportTemplateType != SoeReportTemplateType.TimeSalarySpecificationReport && UseWebApiInternal())
            {
                try
                {
                    var connector = new ReportConnector();
                    if (this.esc.Count > 1)
                        reportPrintoutId = connector.PrintReportPackageData(this.esc, SoeCompany.ActorCompanyId, UserId, culture);
                    else
                        reportPrintoutId = connector.PrintReport(this.esc[0], SoeCompany.ActorCompanyId, UserId, culture);
                }
                catch (Exception ex)
                {
                    SysLogManager.LogError<_default>(ex);
                }
            }
            else
            {
                #region Print from Web

                if (this.esc.Count > 1)
                    reportPrintoutId = rdm.PrintReportPackageId(this.esc);
                else
                    reportPrintoutId = rdm.PrintReportId(this.esc[0]);

                #endregion
            }

            #region TimeProjectReport

            if (this.IsTimeProjectReport && exportFileReportPrintoutIdsDict.Count > 0)
            {
                int eInvoiceFormat = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingEInvoiceFormat, 0, SoeCompany.ActorCompanyId, 0);
                if (eInvoiceFormat == (int)TermGroup_EInvoiceFormat.Svefaktura || eInvoiceFormat == (int)TermGroup_EInvoiceFormat.SvefakturaTidbok)
                {
                    #region FTP
                    //Get ftp-settings primarily from company's settings, secondarily from application's settings
                    int companySettingTypeInExchangeFtpUsername = (int)(this.ReleaseMode ? CompanySettingType.InExchangeFtpUsername : CompanySettingType.InExchangeFtpUsernameTest);
                    UserCompanySetting settingInExchangeFtpUsername = sm.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpUsername, UserId, SoeCompany.ActorCompanyId, 0);
                    if (settingInExchangeFtpUsername == null || settingInExchangeFtpUsername.StrData == string.Empty)
                        settingInExchangeFtpUsername = sm.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpUsername, UserId, 2, 0);

                    int companySettingTypeInExchangeFtpPassword = (int)(this.ReleaseMode ? CompanySettingType.InExchangeFtpPassword : CompanySettingType.InExchangeFtpPasswordTest);
                    UserCompanySetting settingInExchangeFtpPassword = sm.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpPassword, UserId, SoeCompany.ActorCompanyId, 0);
                    if (settingInExchangeFtpPassword == null || settingInExchangeFtpPassword.StrData == string.Empty)
                        settingInExchangeFtpPassword = sm.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpPassword, UserId, 2, 0);

                    int companySettingTypeInExchangeFtpAddress = (int)(this.ReleaseMode ? CompanySettingType.InExchangeFtpAddress : CompanySettingType.InExchangeFtpAddressTest);
                    UserCompanySetting settingExchangeFtpAddress = sm.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpAddress, UserId, SoeCompany.ActorCompanyId, 0);
                    if (settingExchangeFtpAddress == null || settingExchangeFtpAddress.StrData == string.Empty)
                        settingExchangeFtpAddress = sm.GetUserCompanySetting(SettingMainType.Company, companySettingTypeInExchangeFtpAddress, UserId, 2, 0);

                    if (settingInExchangeFtpUsername != null && settingInExchangeFtpPassword != null && settingExchangeFtpAddress != null)
                    {
                        foreach (var pair in exportFileReportPrintoutIdsDict)
                        {
                            int exportFileReportPrintoutId = pair.Key;
                            int invoiceId = pair.Value;

                            ReportPrintout exportFileReportPrintout = rm.GetReportPrintout(exportFileReportPrintoutId, SoeCompany.ActorCompanyId);
                            if (exportFileReportPrintout != null && exportFileReportPrintout.Data != null && exportFileReportPrintout.Data.Length > 0)
                            {
                                string invoiceNr = im.GetInvoiceNr(invoiceId);
                                if (!String.IsNullOrEmpty(invoiceNr))
                                {
                                    //Send it with ftp
                                    Uri tprUri = new Uri(settingExchangeFtpAddress.StrData.ToString() + "/attachment/" + invoiceNr.ToString() + invoiceId.ToString() + ".pdf");
                                    FtpUtility.UploadData(tprUri, exportFileReportPrintout.Data, settingInExchangeFtpUsername.StrData, settingInExchangeFtpPassword.StrData);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }

            #endregion

            ReportPrintout reportPrintout = rm.GetReportPrintout(reportPrintoutId, SoeCompany.ActorCompanyId);
            if (!upload && rm.DoShowReportPrintoutErrorMessage(reportPrintout))
            {
                ShowError(GetText(5971, "Rapport kunde inte skrivas ut"), ParseResult(reportPrintout));
                return;
            }

            #endregion

            #region Download

            if (isEmailFromAngular)
            {
                Response.Clear();
                Response.Write(GetText(8653, "Skickat"));
            }
            else
            {
                Session[Constants.SESSION_DOWNLOAD_REPORT_ITEM + this.guid] = reportPrintout;
                Response.Redirect($"../selection/download/?c={this.SoeCompany.ActorCompanyId}&r={this.RoleId}&email={Boolean.FalseString}&guid={this.guid}&upload={upload}");
            }

            #endregion
        }

        #endregion

        #region Help-methods

        private void ParseSelection()
        {
            this.selection = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName, report: report.ToDTO(), isMainReport: true, exportType: this.exportTypeId, exportFileType: this.exportFileTypeId);
            if (this.esc == null)
                this.esc = new List<EvaluatedSelection>();

            CheckReportUrl();

            switch (this.reportTemplateType)
            {
                #region Economy

                case SoeReportTemplateType.GeneralLedger:
                case SoeReportTemplateType.VoucherList:
                case SoeReportTemplateType.SupplierBalanceList:
                case SoeReportTemplateType.CustomerBalanceList:
                case SoeReportTemplateType.IOCustomerInvoice:
                //case SoeReportTemplateType.IOSupplierInvoice:
                case SoeReportTemplateType.IOVoucher:
                case SoeReportTemplateType.SEPAPaymentImportReport:
                case SoeReportTemplateType.PeriodAccountingRegulationsReport:
                case SoeReportTemplateType.PeriodAccountingForecastReport:
                case SoeReportTemplateType.InterestRateCalculation:
                case SoeReportTemplateType.SupplierInvoiceJournal:
                    ParseSelectionEconomyReports();
                    break;

                #endregion

                #region Billing

                case SoeReportTemplateType.HousholdTaxDeduction:
                case SoeReportTemplateType.HouseholdTaxDeductionFile:
                case SoeReportTemplateType.BillingContract:
                case SoeReportTemplateType.BillingOffer:
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                case SoeReportTemplateType.OrderChecklistReport:
                case SoeReportTemplateType.ProjectTransactionsReport:
                case SoeReportTemplateType.ProjectStatisticsReport:
                case SoeReportTemplateType.TimeProjectReport:
                case SoeReportTemplateType.ProductListReport:
                case SoeReportTemplateType.PurchaseOrder:
                case SoeReportTemplateType.ExpenseReport:
                case SoeReportTemplateType.StockInventoryReport:
                    ParseSelectionBillingReports();
                    break;

                #endregion

                #region Time

                case SoeReportTemplateType.TimeMonthlyReport:
                case SoeReportTemplateType.TimePayrollTransactionReport:
                case SoeReportTemplateType.TimeEmployeeSchedule:
                case SoeReportTemplateType.TimeEmployeeTemplateSchedule:
                case SoeReportTemplateType.TimeCategorySchedule:
                case SoeReportTemplateType.TimeAccumulatorReport:
                case SoeReportTemplateType.TimeCategoryStatistics:
                case SoeReportTemplateType.TimeStampEntryReport:
                case SoeReportTemplateType.TimeEmploymentDynamicContract:
                case SoeReportTemplateType.TimeSalarySpecificationReport:
                case SoeReportTemplateType.TimeSalaryControlInfoReport:
                case SoeReportTemplateType.EmployeeListReport:
                case SoeReportTemplateType.UserListReport:
                case SoeReportTemplateType.TimeEmploymentContract:
                case SoeReportTemplateType.PayrollSlip:
                case SoeReportTemplateType.TimeScheduleBlockHistory:
                case SoeReportTemplateType.ConstructionEmployeesReport:
                case SoeReportTemplateType.PayrollAccountingReport:
                case SoeReportTemplateType.PayrollVacationAccountingReport:
                case SoeReportTemplateType.PayrollTransactionStatisticsReport:
                case SoeReportTemplateType.EmployeeVacationInformationReport:
                case SoeReportTemplateType.EmployeeVacationDebtReport:
                case SoeReportTemplateType.KU10Report:
                case SoeReportTemplateType.SKDReport:
                case SoeReportTemplateType.SCB_SLPReport:
                case SoeReportTemplateType.SCB_KLPReport:
                case SoeReportTemplateType.SCB_KSPReport:
                case SoeReportTemplateType.SCB_KSJUReport:
                case SoeReportTemplateType.SNReport:
                case SoeReportTemplateType.KPAReport:
                case SoeReportTemplateType.ForaReport:
                case SoeReportTemplateType.EmployeeTimePeriodReport:
                case SoeReportTemplateType.CollectumReport:
                case SoeReportTemplateType.PayrollPeriodWarningCheck:
                case SoeReportTemplateType.CertificateOfEmploymentReport:
                case SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport:
                case SoeReportTemplateType.AgdEmployeeReport:
                case SoeReportTemplateType.TimePayrollTransactionSmallReport:
                case SoeReportTemplateType.TimeEmployeeScheduleSmallReport:
                case SoeReportTemplateType.TimeAbsenceReport:
                case SoeReportTemplateType.RoleReport:
                case SoeReportTemplateType.ForaMonthlyReport:
                    ParseSelectionTimeReports();
                    break;

                #endregion

                #region Payroll

                case SoeReportTemplateType.PayrollProductReport:
                    ParseSelectionSalaryReports();
                    break;

                    #endregion
            }

            this.selection.Evaluate(this.reportItem, reportUrlId);
            this.esc.Add(selection.Evaluated);
        }

        private void ParseSelectionBillingReports()
        {
            switch (this.reportTemplateType)
            {
                case SoeReportTemplateType.HousholdTaxDeduction:
                    this.reportItem = new HouseholdTaxDeductionReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, false, this.parametersDict);
                    break;
                case SoeReportTemplateType.HouseholdTaxDeductionFile:
                    this.reportItem = new HouseholdTaxDeductionReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, true, this.parametersDict);
                    break;
                case SoeReportTemplateType.BillingContract:
                case SoeReportTemplateType.BillingOffer:
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                    this.reportItem = new BillingInvoiceReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    #region TimeProjectReport

                    var billingInvoiceReportDto = this.reportItem as BillingInvoiceReportDTO;
                    if (billingInvoiceReportDto != null && billingInvoiceReportDto.IncludeProjectReport)
                    {
                        Report timeProjectReport = rm.GetBillingInvoiceProjectTimeProjectReport(this.reportTemplateType, SoeCompany.ActorCompanyId, RoleId);
                        if (timeProjectReport != null)
                        {
                            BillingInvoiceTimeProjectReportDTO timeProjectReportDto = new BillingInvoiceTimeProjectReportDTO(SoeCompany.ActorCompanyId, timeProjectReport.ReportId, (int)this.reportTemplateType, billingInvoiceReportDto);
                            Selection timeProjectReportSelection = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName, timeProjectReport.ToDTO(), exportType: this.exportTypeId);
                            timeProjectReportSelection.Evaluate(timeProjectReportDto, this.reportUrlId);
                            this.esc.Add(timeProjectReportSelection.Evaluated);
                        }
                    }

                    #endregion
                    break;
                case SoeReportTemplateType.TimeProjectReport:
                    this.reportItem = new BillingInvoiceTimeProjectReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.ExpenseReport:
                    this.reportItem = new BillingInvoiceTimeProjectReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.OrderChecklistReport:
                    this.reportItem = new OrderChecklistReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.ProjectTransactionsReport:
                    this.reportItem = new ProjectTransactionsReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.ProjectStatisticsReport:
                    this.reportItem = new ProjectStatisticsReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.ProductListReport:
                    this.reportItem = new ProductListReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.PurchaseOrder:
                    this.reportItem = new PurchaseOrderReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.StockInventoryReport:
                    this.reportItem = new StockInventoryReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
            }
        }

        private void ParseSelectionEconomyReports()
        {
            switch (this.reportTemplateType)
            {
                case SoeReportTemplateType.CustomerBalanceList:
                case SoeReportTemplateType.InterestRateCalculation:
                    this.reportItem = new BalanceListReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.GeneralLedger:
                    this.reportItem = new GeneralLedgerReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.SupplierInvoiceJournal:
                    this.reportItem = new BalanceListReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict, true);
                    break;
                case SoeReportTemplateType.SupplierBalanceList:
                    this.reportItem = new BalanceListReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.VoucherList:
                    this.reportItem = new VoucherListReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.IOCustomerInvoice:
                    this.reportItem = new CustomerInvoiceIOReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.IOVoucher:
                    this.reportItem = new VoucherIOReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.SEPAPaymentImportReport:
                    this.reportItem = new SEPAPaymentImportReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                    //case SoeReportTemplateType.PeriodAccountingRegulationsReport:
                    //    this.reportItem = new PeriodAccountingRegulationsReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    //    break;
            }
        }

        private void ParseSelectionTimeReports()
        {
            switch (this.reportTemplateType)
            {
                case SoeReportTemplateType.KU10Report:
                    this.reportItem = new TimeKU10ReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.AgdEmployeeReport:
                    this.reportItem = new TimeAgdEmployeeReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.TimeSalarySpecificationReport:
                    this.reportItem = new TimeSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.TimeSalaryControlInfoReport:
                    this.reportItem = new TimeSalaryControlInfoReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.TimeCategorySchedule:
                    this.reportItem = new TimeCategoryReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.TimeEmploymentContract:
                    this.reportItem = new TimeEmploymentReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.UserListReport:
                    this.reportItem = new TimeUsersListReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.PayrollSlip:
                    #region PayrollSlip

                    List<int> employeeIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(this.parametersDict, EMPLOYEE));
                    List<int> timePeriodIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(this.parametersDict, TIMEPERIOD));
                    bool isPreliminary = StringUtility.TryGetBoolValue(this.parametersDict, PRELIMINARY);

                    GeneralManager gm = new GeneralManager(null);

                    foreach (var employeeId in employeeIds)
                    {
                        foreach (var timePeriodId in timePeriodIds)
                        {
                            if (!isPreliminary)
                            {
                                //if not preliminary we must have a datastorage
                                var dataStorage = gm.GetDataStorage(SoeDataStorageRecordType.PayrollSlipXML, timePeriodId, employeeId, ParameterObject.ActorCompanyId);
                                if (dataStorage == null || dataStorage.XML == null)
                                    continue;
                            }

                            if (this.reportItem == null)
                            {
                                this.reportItem = new TimePayrollSlipReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, employeeId, timePeriodId, this.parametersDict);
                            }
                            else
                            {
                                var newReportItem = new TimePayrollSlipReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, employeeId, timePeriodId, this.parametersDict);
                                Selection newReportSelection = new Selection(SoeCompany.ActorCompanyId, UserId, RoleId, SoeUser.LoginName, report.ToDTO(), exportType: this.exportTypeId);
                                newReportSelection.Evaluate(newReportItem, this.reportUrlId);
                                this.esc.Add(newReportSelection.Evaluated);
                            }
                        }
                    }
                    #endregion
                    break;
                case SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport:
                    this.reportItem = new TimeScheduleTasksAndDeliverysReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                default:
                    this.reportItem = new TimeEmployeeReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
            }
        }

        private void ParseSelectionSalaryReports()
        {
            switch (this.reportTemplateType)
            {
                case SoeReportTemplateType.PayrollProductReport:
                    {
                        this.reportItem = new PayrollProductReportDTO(SoeCompany.ActorCompanyId, report.ReportId, sysReportTemplateTypeId, this.parametersDict);
                    }
                    break;
            }
        }

        private void CheckReportUrl()
        {
            this.parametersDict = QS.ConvertToDict();
            if (this.parametersDict != null && this.parametersDict.ContainsKey(ReportSelectionDTO.GUID))
            {
                ReportUrl reportUrl = null;

                //Try 10 times, wait 1 second between each try
                int trys = 10;
                int currentTry = 1;
                while (currentTry <= trys && reportUrl == null)
                {
                    if (currentTry > 1)
                        System.Threading.Thread.Sleep(1000);

                    reportUrl = rm.GetReportUrl(this.parametersDict[TimeReportDTO.GUID], SoeCompany.ActorCompanyId);
                    if (reportUrl != null)
                        this.reportUrlId = reportUrl.ReportUrlId;

                    currentTry++;
                }

                if (reportUrl != null && !String.IsNullOrEmpty(reportUrl.Url))
                    this.parametersDict = UrlUtil.GetQS(reportUrl.Url);
            }
        }

        private void ShowError(string headerMessage, string detailMessage)
        {
            RedirectToReportError(headerMessage, detailMessage);
        }

        public string ParseResult(ReportPrintout reportPrintout)
        {
            string result = "";
            if (reportPrintout != null)
            {
                switch (reportPrintout.ResultMessage)
                {
                    #region Core

                    case (int)SoeReportDataResultMessage.DocumentNotCreated:
                        result = GetText(1942, "Rapporturval gav ingen data");
                        break;
                    case (int)SoeReportDataResultMessage.EmptyInput:
                    case (int)SoeReportDataResultMessage.ReportFailed:
                        result = GetText(1576, "Rapportkörning misslyckades, se logg");
                        break;
                    case (int)SoeReportDataResultMessage.ReportTemplateDataNotFound:
                        result = GetText(5977, "Rapportmall hittades inte");
                        break;

                    #endregion

                    #region Economy

                    case (int)SoeReportDataResultMessage.BalanceReportHasNoGroupsOrHeaders:
                    case (int)SoeReportDataResultMessage.ResultReportHasNoGroupsOrHeaders:
                        result = GetText(1400, "Rapport har inga grupper eller rubriker");
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
                        result = GetText(5976, "Behörighet saknas");
                        break;

                        #endregion

                        #region Import

                        #endregion
                }
            }
            return result;
        }

        #endregion
    }
}