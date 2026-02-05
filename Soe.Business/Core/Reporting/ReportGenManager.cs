using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using SoftOne.Soe.Business.Core.RptGen;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SoftOne.Soe.Business.Core.Reporting
{
    public class ReportGenManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ReportGenManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region ReportDocument

        public ReportDocument LoadReportDocument(byte[] file, SoeReportTemplateType reportTemplateType)
        {
            ReportDocument reportDocument = null;
            string fileName = "";

            try
            {
                reportDocument = new ReportDocument();

                //Save uploaded file temporary in the temp directory on the server
                fileName = ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString() + Constants.SOE_SERVER_FILE_RPT_SUFFIX;
                File.WriteAllBytes(fileName, file);

                //Read default XML to DataSet
                DataSet ds = GetDefaultXmlDataSet(reportTemplateType);

                //Load temp-file to ReportDocument and set default XML datasource
                reportDocument.Load(fileName);
                reportDocument.ReportOptions.EnableSaveDataWithReport = false;
                reportDocument.DataSourceConnections.Clear();
                reportDocument.SetDataSource(ds);
                reportDocument.Refresh();
                reportDocument.VerifyDatabase();
                if (reportDocument.SummaryInfo != null && ds.Tables != null && ds.Tables[0] != null)
                {
                    reportDocument.SummaryInfo.ReportTitle = ds.Tables[0].TableName;
                    reportDocument.SummaryInfo.ReportAuthor = Constants.APPLICATION_NAME;
                }
                reportDocument.PrintOptions.PrinterName = "";
            }
            catch (Exception ex)
            {
                //Dispose
                DisposeReportDocument(ref reportDocument);
                reportDocument = null;

                string message = String.Format("LoadReportDocument failed {0}. {1}", fileName, SoeException.GetStackTrace());
                SoeGeneralException soeEx = new SoeGeneralException(message, ex, this.ToString());
                base.LogError(soeEx, this.log);
            }
            finally
            {
                //Delete temp-file
                if (!String.IsNullOrEmpty(fileName))
                    File.Delete(fileName);
            }

            return reportDocument;
        }

        public byte[] ExportReportDocument(ReportDocument reportDocument, ExportFormatType exportFormatType)
        {
            try
            {
                return ZipUtility.GetDataFromStream(reportDocument.ExportToStream(exportFormatType));
            }
            catch (Exception ex)
            {
                //Dispose
                DisposeReportDocument(ref reportDocument);

                string message = String.Format("ExportReportDocumentToStream failed  {0}", SoeException.GetStackTrace());
                SoeGeneralException soeEx = new SoeGeneralException(message, ex, this.ToString());
                base.LogError(soeEx, this.log);
                return null;
            }
        }

        public void DisposeReportDocument(CreateReportResult reportResult)
        {
            ReportDocument reportDocument = reportResult.ReportDocument;
            DisposeReportDocument(ref reportDocument);
        }

        public void DisposeReportDocument(ref ReportDocument reportDocument)
        {
            if (reportDocument == null)
                return;

            try
            {
                reportDocument.Close();
                reportDocument.Dispose();
                reportDocument = null;
                GC.Collect(); //NOSONAR
            }
            catch (Exception ex)
            {
                string message = String.Format("DisposeReportDocument failed {0}.", SoeException.GetStackTrace());
                SoeGeneralException soeEx = new SoeGeneralException(message, ex, this.ToString());
                base.LogError(soeEx, this.log);
            }
        }

        #endregion

        #region DataSet

        public DataSet CreateDataSet(XDocument document, SoeReportTemplateType reportTemplateType)
        {
            DataSet ds = new DataSet();

            string filePathXsd = GetXsdFilePath(reportTemplateType);

            string errorMessage;
            if (!ValidateDocument(document, reportTemplateType, out errorMessage))
                return null;

            ds.ReadXmlSchema(filePathXsd);

            try
            {
                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    ds.ReadXml(stream);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                LogError("ds.ReadXml(stream) from MemoryStream failed - trying XmlReader " + ex.Message.ToString());

                try
                {
                    XmlReader reader = document.CreateReader();
                    ds.ReadXml(reader);
                }
                catch (Exception ex2)
                {
                    LogError("ds.ReadXml(stream) from XMLreader also failed - trying XmlDocument " + ex2.Message.ToString());

                    try
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(document.Root.ToString());
                        ds.ReadXml(new XmlNodeReader(xmlDocument));
                    }
                    catch (Exception ex3)
                    {
                        LogError("ds.ReadXml(new XmlNodeReader(xmlDocument)); from XMLreader also failed - trying creating file " + ex3.Message.ToString());

                        try
                        {
                            string path = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + Guid.NewGuid().ToString() + "_ex3";
                            document.Save(path);
                            ds.ReadXml(path);
                            Thread.Sleep(500);
                            File.Delete(path);
                        }
                        catch (Exception ex4)
                        {
                            LogError("ds.ReadXml(path);r also failed - trying creating file " + ex4.Message.ToString());
                            return null;
                        }
                    }
                }
            }

            return ds;
        }

        #endregion

        #region XSD

        public static string GetXsdFilePath(SoeReportTemplateType reportTemplateType)
        {
            string filename = "";

            switch (reportTemplateType)
            {
                #region Economy

                case SoeReportTemplateType.VoucherList:
                    filename = "VoucherListSchema";
                    break;
                case SoeReportTemplateType.GeneralLedger:
                    filename = "GeneralLedgerSchema";
                    break;
                case SoeReportTemplateType.BalanceReport:
                    filename = "BalanceReportSchema";
                    break;
                case SoeReportTemplateType.ResultReport:
                    filename = "ResultReportSchema";
                    break;
                case SoeReportTemplateType.ResultReportV2:
                    filename = "ResultReportSchemaV2";
                    break;
                case SoeReportTemplateType.TaxAudit:
                    filename = "TaxAuditSchema";
                    break;
                case SoeReportTemplateType.TaxAudit_FI:
                    filename = "TaxAuditSchema_FI";
                    break;
                case SoeReportTemplateType.SruReport:
                    filename = "SruReportSchema";
                    break;
                case SoeReportTemplateType.ReadSoftScanningSupplierInvoice:
                    filename = "ReadSoftScanningSchema";
                    break;
                case SoeReportTemplateType.FinvoiceEdiSupplierInvoice:
                    filename = "FinvoiceEdiSchema";
                    break;
                case SoeReportTemplateType.PeriodAccountingRegulationsReport:
                    filename = "PeriodAccountingRegulationsReportSchema";
                    break;
                case SoeReportTemplateType.PeriodAccountingForecastReport:
                    filename = "PeriodAccountingForecastReportSchema";
                    break;
                case SoeReportTemplateType.FixedAssets:
                    filename = "FixedAssetsReportSchema";
                    break;

                #endregion

                #region Ledger

                case SoeReportTemplateType.SupplierBalanceList:
                    filename = "SupplierBalanceListSchema";
                    break;
                case SoeReportTemplateType.CustomerBalanceList:
                    filename = "CustomerBalanceListSchema";
                    break;
                case SoeReportTemplateType.CustomerInvoiceJournal:
                    filename = "CustomerInvoiceJournalSchema";
                    break;
                case SoeReportTemplateType.SupplierInvoiceJournal:
                    filename = "SupplierInvoiceJournalSchema";
                    break;
                case SoeReportTemplateType.CustomerPaymentJournal:
                    filename = "CustomerPaymentJournalSchema";
                    break;
                case SoeReportTemplateType.SupplierPaymentJournal:
                    filename = "SupplierPaymentJournalSchema";
                    break;
                case SoeReportTemplateType.SupplierListReport:
                    filename = "SupplierReportSchema";
                    break;
                case SoeReportTemplateType.SEPAPaymentImportReport:
                    filename = "SEPAPaymentImportReportSchema";
                    break;
                case SoeReportTemplateType.InterestRateCalculation:
                    filename = "CustomerInterestRateCalculationSchema";
                    break;
                #endregion

                #region Billing

                case SoeReportTemplateType.BillingContract:
                case SoeReportTemplateType.BillingOffer:
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                case SoeReportTemplateType.OriginStatisticsReport:
                case SoeReportTemplateType.BillingOrderOverview:
                    filename = "BillingInvoiceSchema";
                    break;
                case SoeReportTemplateType.BillingStatisticsReport:
                    filename = "BillingStatisticsSchema";
                    break;
                case SoeReportTemplateType.ExpenseReport:
                    filename = "ExpensesReportSchema";
                    break;
                case SoeReportTemplateType.HousholdTaxDeduction:
                    filename = "HouseholdTaxDeductionReportSchema";
                    break;
                case SoeReportTemplateType.ProjectStatisticsReport:
                    filename = "ProjectStatisticsSchema";
                    break;
                case SoeReportTemplateType.TimeProjectReport:
                    filename = "TimeProjectReportSchema";
                    break;
                case SoeReportTemplateType.ProjectTimeReport:
                    filename = "ProjectTimeReportSchema";
                    break;
                case SoeReportTemplateType.SymbrioEdiSupplierInvoice:
                    filename = "SymbrioEdiSchema";
                    break;
                case SoeReportTemplateType.OrderChecklistReport:
                    filename = "OrderChecklistsSchema";
                    break;
                case SoeReportTemplateType.ProjectTransactionsReport:
                    filename = "ProjectTransactionsReportSchema";
                    break;

                case SoeReportTemplateType.StockSaldoListReport:
                    filename = "StockProductSchema";
                    break;
                case SoeReportTemplateType.StockTransactionListReport:
                    filename = "StockTransactionSchema";
                    break;
                case SoeReportTemplateType.StockInventoryReport:
                    filename = "StockInventorySchema";
                    break;
                case SoeReportTemplateType.CustomerListReport:
                    filename = "CustomerReportSchema";
                    break;
                case SoeReportTemplateType.ProductListReport:
                    filename = "ProductReportSchema";
                    break;
                case SoeReportTemplateType.PurchaseOrder:
                    filename = "PurchaseOrderSchema";
                    break;
                case SoeReportTemplateType.OrderContractChange:
                    filename = "OrderContractChangeSchema";
                    break;

                #endregion

                #region Time

                case SoeReportTemplateType.TimeMonthlyReport:
                    filename = "TimeMonthlyReportSchema";
                    break;
                case SoeReportTemplateType.TimePayrollTransactionReport:
                    filename = "TimePayrollTransactionReportSchema";
                    break;
                case SoeReportTemplateType.TimePayrollTransactionSmallReport:
                    filename = "TimePayrollTransactionSmallReportSchema";
                    break;
                case SoeReportTemplateType.TimeEmployeeSchedule:
                case SoeReportTemplateType.TimeEmployeeTemplateSchedule:
                case SoeReportTemplateType.TimeScheduleCopyReport:
                    filename = "TimeEmployeeScheduleSchema";
                    break;
                case SoeReportTemplateType.TimeCategorySchedule:
                    filename = "TimeCategoryScheduleSchema";
                    break;
                case SoeReportTemplateType.TimeAccumulatorReport:
                    filename = "TimeAccumulatorReportSchema";
                    break;
                case SoeReportTemplateType.TimeAccumulatorDetailedReport:
                    filename = "TimeAccumulatorDetailedReportSchema";
                    break;
                case SoeReportTemplateType.TimeCategoryStatistics:
                    filename = "TimeCategoryStatisticsSchema";
                    break;
                case SoeReportTemplateType.TimeStampEntryReport:
                    filename = "TimeStampEntryReportSchema";
                    break;
                case SoeReportTemplateType.TimeSalarySpecificationReport:
                    filename = "TimeSalarySpecificationReportSchema";
                    break;
                case SoeReportTemplateType.TimeSalaryControlInfoReport:
                    filename = "TimeSalaryControlInfoReportSchema";
                    break;
                case SoeReportTemplateType.EmployeeListReport:
                    filename = "EmployeeReportSchema";
                    break;
                case SoeReportTemplateType.TimeEmploymentContract:
                    filename = "TimeEmploymentContractSchema";
                    break;
                case SoeReportTemplateType.PayrollSlip:
                    filename = "PayrollSlipSchema";
                    break;
                case SoeReportTemplateType.TimeScheduleBlockHistory:
                    filename = "TimeScheduleBlockHistorySchema";
                    break;
                case SoeReportTemplateType.ConstructionEmployeesReport:
                    filename = "ConstructionEmployeesReportSchema";
                    break;
                case SoeReportTemplateType.PayrollTransactionStatisticsReport:
                    filename = "PayrollTransactionStatisticSchema";
                    break;
                case SoeReportTemplateType.PayrollAccountingReport:
                case SoeReportTemplateType.PayrollVacationAccountingReport:
                    filename = "PayrollAccountingSchema";
                    break;
                case SoeReportTemplateType.PayrollProductReport:
                    filename = "PayrollProductReportSchema";
                    break;
                case SoeReportTemplateType.EmployeeVacationInformationReport:
                    filename = "EmployeeVacationInformationReportSchema";
                    break;
                case SoeReportTemplateType.EmployeeVacationDebtReport:
                    filename = "EmployeeVacationDebtReportSchema";
                    break;
                case SoeReportTemplateType.KU10Report:
                    filename = "KU10ReportSchema";
                    break;
                case SoeReportTemplateType.SKDReport:
                    filename = "SKDReportSchema";
                    break;
                case SoeReportTemplateType.CertificateOfEmploymentReport:
                    filename = "CertificateOfEmploymentReportSchema";
                    break;
                case SoeReportTemplateType.CollectumReport:
                    filename = "CollectumReportSchema";
                    break;
                case SoeReportTemplateType.EmployeeTimePeriodReport:
                    filename = "EmployeeTimePeriodReportSchema";
                    break;
                case SoeReportTemplateType.PayrollPeriodWarningCheck:
                    filename = "PayrollPeriodWarningCheckSchema";
                    break;
                case SoeReportTemplateType.SCB_SLPReport:
                    filename = "SCB_SN_ReportSchema";
                    break;
                case SoeReportTemplateType.SCB_KSPReport:
                    filename = "SCB_KSP_ReportSchema";
                    break;
                case SoeReportTemplateType.SCB_KLPReport:
                    filename = "SCB_KLP_ReportSchema";
                    break;
                case SoeReportTemplateType.SCB_KSJUReport:
                    filename = "SCB_KSJU_ReportSchema";
                    break;
                case SoeReportTemplateType.SNReport:
                    filename = "SCB_SN_ReportSchema";
                    break;
                case SoeReportTemplateType.KPADirektReport:
                    filename = "KPADirektReportSchema";
                    break;
                case SoeReportTemplateType.Bygglosen:
                    filename = "BygglosenReportSchema";
                    break;
                case SoeReportTemplateType.KPAReport:
                    filename = "KPAReportSchema";
                    break;
                case SoeReportTemplateType.ForaReport:
                    filename = "ForaReportSchema";
                    break;
                case SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport:
                    filename = "TimeScheduleTasksAndDeliverysSchema";
                    break;
                case SoeReportTemplateType.AgdEmployeeReport:
                    filename = "AgdEmployeeReportSchema";
                    break;
                case SoeReportTemplateType.TimeEmployeeScheduleSmallReport:
                    filename = "EmployeeScheduleDataSmallReportSchema";
                    break;
                case SoeReportTemplateType.TimeAbsenceReport:
                    filename = "TimeAbsenceReportSchema";
                    break;
                case SoeReportTemplateType.TimeEmployeeLineSchedule:
                    filename = "TimeEmployeeLineScheduleSchema";
                    break;
                case SoeReportTemplateType.RoleReport:
                    filename = "RoleReportSchema";
                    break;
                case SoeReportTemplateType.Kronofogden:
                    filename = "KronofogdenReportSchema";
                    break;
                case SoeReportTemplateType.TimeEmploymentDynamicContract:
                    filename = "EmploymentDynamicContractReportSchema";
                    break;
                case SoeReportTemplateType.FolksamGTP:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.IFMetall:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.SkandiaPension:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.ForaMonthlyReport:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.SEF:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.AgiAbsence:
                    filename = "Generic";
                    break;
                #endregion

                #region Import

                case SoeReportTemplateType.IOVoucher:
                    filename = "IOVoucherSchema";
                    break;
                case SoeReportTemplateType.IOCustomerInvoice:
                    filename = "IOCustomerInvoiceSchema";
                    break;
                //case SoeReportTemplateType.IOSupplierInvoice:
                //    filename = "IOSupplierInvoiceSchema";
                //    break;

                #endregion

                #region Wizard

                case SoeReportTemplateType.UserListReport:
                    filename = "UserReportSchema";
                    break;

                    #endregion
            }

            string path = UrlUtil.GetFilePath(ConfigSettings.SOE_SERVER_DIR_REPORT_PHYSICAL, filename, "xsd");
            if (!File.Exists(path))
                path = UrlUtil.GetFilePath(ConfigSettings.SOE_SERVER_DIR_REPORT_CONFIGFILE, filename, "xsd");

            return path;
        }

        public string GetXsdFileString(SoeReportTemplateType reportTemplateType)
        {
            try
            {
                return File.ReadAllText(GetXsdFilePath(reportTemplateType));
            }
            catch (Exception ex)
            {
                string message = String.Format("GetSchema {0}.", SoeException.GetStackTrace());
                SoeGeneralException soeEx = new SoeGeneralException(message, ex, this.ToString());
                base.LogError(soeEx, this.log);
                return string.Empty;
            }
        }

        #endregion

        #region XML

        public string GetDefaultXmlFilePath(SoeReportTemplateType reportTemplateType)
        {
            string filename = "";

            switch (reportTemplateType)
            {
                #region Economy

                case SoeReportTemplateType.VoucherList:
                    filename = "VoucherListDefaultXml";
                    break;
                case SoeReportTemplateType.GeneralLedger:
                    filename = "GeneralLedgerDefaultXml";
                    break;
                case SoeReportTemplateType.BalanceReport:
                    filename = "BalanceReportDefaultXml";
                    break;
                case SoeReportTemplateType.ResultReport:
                    filename = "ResultReportDefaultXml";
                    break;
                case SoeReportTemplateType.ResultReportV2:
                    filename = "ResultReportDefaultV2Xml";
                    break;
                case SoeReportTemplateType.TaxAudit:
                    filename = "TaxAuditDefaultXml";
                    break;
                case SoeReportTemplateType.TaxAudit_FI:
                    filename = "TaxAuditDefaultXml_FI";
                    break;
                case SoeReportTemplateType.SruReport:
                    filename = "SruReportDefaultXml";
                    break;
                case SoeReportTemplateType.ReadSoftScanningSupplierInvoice:
                    filename = "ReadSoftScanningDefaultXml";
                    break;
                case SoeReportTemplateType.FinvoiceEdiSupplierInvoice:
                    filename = "FinvoiceEdiDefaultXml";
                    break;
                case SoeReportTemplateType.SupplierListReport:
                    filename = "SupplierReportDefaultXml";
                    break;
                case SoeReportTemplateType.PeriodAccountingRegulationsReport:
                    filename = "PeriodAccountingRegulationsReportDefaultXml";
                    break;
                case SoeReportTemplateType.PeriodAccountingForecastReport:
                    filename = "PeriodAccountingForecastReportDefaultXml";
                    break;
                case SoeReportTemplateType.FixedAssets:
                    filename = "FixedAssetsReportDefaultXml";
                    break;

                #endregion

                #region Ledger

                case SoeReportTemplateType.SupplierBalanceList:
                    filename = "SupplierBalanceListDefaultXml";
                    break;
                case SoeReportTemplateType.CustomerBalanceList:
                    filename = "CustomerBalanceListDefaultXml";
                    break;
                case SoeReportTemplateType.CustomerInvoiceJournal:
                    filename = "CustomerInvoiceJournalDefaultXml";
                    break;
                case SoeReportTemplateType.SupplierInvoiceJournal:
                    filename = "SupplierInvoiceJournalDefaultXml";
                    break;
                case SoeReportTemplateType.CustomerPaymentJournal:
                    filename = "CustomerPaymentJournalDefaultXml";
                    break;
                case SoeReportTemplateType.SupplierPaymentJournal:
                    filename = "SupplierPaymentJournalDefaultXml";
                    break;
                case SoeReportTemplateType.SEPAPaymentImportReport:
                    filename = "SEPAPaymentImportReportDefaultXml";
                    break;
                case SoeReportTemplateType.InterestRateCalculation:
                    filename = "CustomerInterestRateCalculationDefaultXml";
                    break;
                #endregion

                #region Billing

                case SoeReportTemplateType.BillingContract:
                case SoeReportTemplateType.BillingOffer:
                case SoeReportTemplateType.BillingOrder:
                case SoeReportTemplateType.BillingInvoice:
                case SoeReportTemplateType.BillingInvoiceInterest:
                case SoeReportTemplateType.BillingInvoiceReminder:
                case SoeReportTemplateType.OriginStatisticsReport:
                case SoeReportTemplateType.BillingOrderOverview:
                    filename = "BillingInvoiceDefaultXml";
                    break;
                case SoeReportTemplateType.OrderContractChange:
                    filename = "OrderContractChangeDefaultXml";
                    break;
                case SoeReportTemplateType.BillingStatisticsReport:
                    filename = "BillingStatisticsDefaultXml";
                    break;
                case SoeReportTemplateType.HousholdTaxDeduction:
                    filename = "HouseholdTaxDeductionReportDefaultXml";
                    break;
                case SoeReportTemplateType.ProjectStatisticsReport:
                    filename = "ProjectStatisticsDefaultXml";
                    break;
                case SoeReportTemplateType.TimeProjectReport:
                    filename = "TimeProjectReportDefaultXml";
                    break;
                case SoeReportTemplateType.ProjectTimeReport:
                    filename = "ProjectTimeReportDefaultXml";
                    break;
                case SoeReportTemplateType.ExpenseReport:
                    filename = "ExpensesReportDefaultXml";
                    break;
                case SoeReportTemplateType.SymbrioEdiSupplierInvoice:
                    filename = "SymbrioEdiDefaultXml";
                    break;
                case SoeReportTemplateType.OrderChecklistReport:
                    filename = "OrderChecklistsDefaultXml";
                    break;
                case SoeReportTemplateType.ProjectTransactionsReport:
                    filename = "ProjectTransactionsReportDefaultXml";
                    break;
                case SoeReportTemplateType.CustomerListReport:
                    filename = "CustomerReportDefaultXml";
                    break;
                case SoeReportTemplateType.ProductListReport:
                    filename = "ProductReportDefaultXml";
                    break;
                case SoeReportTemplateType.StockSaldoListReport:
                    filename = "StockProductDefaultXml";
                    break;

                case SoeReportTemplateType.StockTransactionListReport:
                    filename = "StockTransactionDefaultXml";
                    break;
                case SoeReportTemplateType.StockInventoryReport:
                    filename = "StockInventoryDefaultXml";
                    break;
                case SoeReportTemplateType.PurchaseOrder:
                    filename = "PurchaseOrderDefaultXml";
                    break;

                #endregion

                #region Time

                case SoeReportTemplateType.TimeMonthlyReport:
                    filename = "TimeMonthlyReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimePayrollTransactionReport:
                    filename = "TimePayrollTransactionReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimePayrollTransactionSmallReport:
                    filename = "TimePayrollTransactionSmallReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeEmployeeSchedule:
                case SoeReportTemplateType.TimeEmployeeTemplateSchedule:
                case SoeReportTemplateType.TimeScheduleCopyReport:
                    filename = "TimeEmployeeScheduleDefaultXml";
                    break;
                case SoeReportTemplateType.TimeCategorySchedule:
                    filename = "TimeCategoryScheduleDefaultXml";
                    break;
                case SoeReportTemplateType.TimeAccumulatorReport:
                    filename = "TimeAccumulatorReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeAccumulatorDetailedReport:
                    filename = "TimeAccumulatorDetailedReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeCategoryStatistics:
                    filename = "TimeCategoryStatisticsDefaultXml";
                    break;
                case SoeReportTemplateType.TimeStampEntryReport:
                    filename = "TimeStampEntryReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeSalarySpecificationReport:
                    filename = "TimeSalarySpecificationReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeSalaryControlInfoReport:
                    filename = "TimeSalaryControlInfoReportDefaultXml";
                    break;
                case SoeReportTemplateType.EmployeeListReport:
                    filename = "EmployeeReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeEmploymentContract:
                    filename = "TimeEmploymentContractDefaultXml";
                    break;
                case SoeReportTemplateType.PayrollSlip:
                    filename = "PayrollSlipDefaultXml";
                    break;
                case SoeReportTemplateType.TimeScheduleBlockHistory:
                    filename = "TimeScheduleBlockHistoryDefaultXml";
                    break;
                case SoeReportTemplateType.IOVoucher:
                    filename = "IOVoucherDefaultXml";
                    break;
                case SoeReportTemplateType.IOCustomerInvoice:
                    filename = "IOCustomerInvoiceDefaultXml";
                    break;
                //case SoeReportTemplateType.IOSupplierInvoice:
                //    filename = "IOSupplierInvoiceDefaultXml";
                //    break;
                case SoeReportTemplateType.ConstructionEmployeesReport:
                    filename = "ConstructionEmployeesReportDefaultxml";
                    break;
                case SoeReportTemplateType.PayrollAccountingReport:
                case SoeReportTemplateType.PayrollVacationAccountingReport:
                    filename = "PayrollAccountingDefaultXml";
                    break;
                case SoeReportTemplateType.PayrollTransactionStatisticsReport:
                    filename = "PayrollTransactionStatisticsDefaultXml";
                    break;
                case SoeReportTemplateType.PayrollProductReport:
                    filename = "PayrollProductReportDefaultXml";
                    break;
                case SoeReportTemplateType.EmployeeVacationInformationReport:
                    filename = "EmployeeVacationInformationReportDefaultXml";
                    break;
                case SoeReportTemplateType.EmployeeVacationDebtReport:
                    filename = "EmployeeVacationDebtReportDefaultXml";
                    break;
                case SoeReportTemplateType.KU10Report:
                    filename = "KU10ReportDefaultXml";
                    break;
                case SoeReportTemplateType.SKDReport:
                    filename = "SKDReportDefaultXml";
                    break;
                case SoeReportTemplateType.CertificateOfEmploymentReport:
                    filename = "CertificateOfEmploymentReportDefaultXml";
                    break;
                case SoeReportTemplateType.CollectumReport:
                    filename = "CollectumReportDefaultXml";
                    break;
                case SoeReportTemplateType.EmployeeTimePeriodReport:
                    filename = "EmployeeTimePeriodReportDefaultXml";
                    break;
                case SoeReportTemplateType.PayrollPeriodWarningCheck:
                    filename = "PayrollPeriodWarningCheckDefaultXml";
                    break;
                case SoeReportTemplateType.SCB_SLPReport:
                    filename = "SCB_SN_ReportDefaultXml";
                    break;
                case SoeReportTemplateType.SCB_KSPReport:
                    filename = "SCB_KSP_ReportDefaultXml";
                    break;
                case SoeReportTemplateType.SCB_KLPReport:
                    filename = "SCB_KLP_ReportDefaultXml";
                    break;
                case SoeReportTemplateType.SCB_KSJUReport:
                    filename = "SCB_KSJU_ReportDefaultXml";
                    break;
                case SoeReportTemplateType.SNReport:
                    filename = "SCB_SN_ReportDefaultXml";
                    break;
                case SoeReportTemplateType.KPADirektReport:
                    filename = "KPADirektReportDefaultXml";
                    break;
                case SoeReportTemplateType.Bygglosen:
                    filename = "BygglosenReportDefaultXml";
                    break;
                case SoeReportTemplateType.Kronofogden:
                    filename = "KronofogdenReportDefaultXml";
                    break;
                case SoeReportTemplateType.KPAReport:
                    filename = "KPAReportDefaultXml";
                    break;
                case SoeReportTemplateType.ForaReport:
                    filename = "ForaReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport:
                    filename = "TimeScheduleTasksAndDeliverysDefaultXml";
                    break;
                case SoeReportTemplateType.AgdEmployeeReport:
                    filename = "AgdEmployeeReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeEmployeeScheduleSmallReport:
                    filename = "EmployeeScheduleDataSmallReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeAbsenceReport:
                    filename = "TimeAbsenceReportDefaultXml";
                    break;
                case SoeReportTemplateType.TimeEmployeeLineSchedule:
                    filename = "TimeEmployeeLineScheduleDefaultXml";
                    break;
                case SoeReportTemplateType.TimeEmploymentDynamicContract:
                    filename = "EmploymentDynamicContractReportDefaultXml";
                    break;
                case SoeReportTemplateType.RoleReport:
                    filename = "RoleReportDefaultXml";
                    break;
                case SoeReportTemplateType.FolksamGTP:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.IFMetall:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.SkandiaPension:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.ForaMonthlyReport:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.SEF:
                    filename = "Generic";
                    break;
                case SoeReportTemplateType.AgiAbsence:
                    filename = "Generic";
                    break;
                #endregion

                #region Wizard

                case SoeReportTemplateType.UserListReport:
                    filename = "UserReportDefaultXml";
                    break;

                    #endregion
            }

            string path = UrlUtil.GetFilePath(ConfigSettings.SOE_SERVER_DIR_REPORT_PHYSICAL, filename, "xml");
            if (!File.Exists(path))
                path = UrlUtil.GetFilePath(ConfigSettings.SOE_SERVER_DIR_REPORT_CONFIGFILE, filename, "xml");

            return path;
        }

        public DataSet GetDefaultXmlDataSet(SoeReportTemplateType reportTemplateType)
        {
            //Read default XML to DataSet
            DataSet ds = new DataSet();
            string defaultXmlFilePath = GetDefaultXmlFilePath(reportTemplateType);
            ds.ReadXml(defaultXmlFilePath);

            return ds;
        }

        public XDocument GetDefaultXDocument(SoeReportTemplateType reportTemplateType)
        {
            return XDocument.Load(GetDefaultXmlFilePath(reportTemplateType));
        }

        #endregion

        #region Types and Formats

        public ExportFormatType GetCrystalExportFormatType(SoeExportFormat exportFormat)
        {
            ExportFormatType exportFormatType = ExportFormatType.Xml;

            switch (exportFormat)
            {
                #region Data

                case SoeExportFormat.Pdf:
                case SoeExportFormat.MergedPDF:
                    exportFormatType = ExportFormatType.PortableDocFormat;
                    break;
                case SoeExportFormat.Xml:
                    exportFormatType = ExportFormatType.Xml;
                    break;
                case SoeExportFormat.Excel:
                    exportFormatType = ExportFormatType.ExcelRecord;
                    break;
                case SoeExportFormat.ExcelXlsx:
                    exportFormatType = ExportFormatType.ExcelWorkbook;
                    break;
                case SoeExportFormat.Word:
                    exportFormatType = ExportFormatType.WordForWindows;
                    break;
                case SoeExportFormat.RichText:
                    exportFormatType = ExportFormatType.RichText;
                    break;
                case SoeExportFormat.EditableRTF:
                    exportFormatType = ExportFormatType.EditableRTF;
                    break;
                case SoeExportFormat.Text:
                    exportFormatType = ExportFormatType.Text;
                    break;
                case SoeExportFormat.TabSeperatedText:
                    exportFormatType = ExportFormatType.TabSeperatedText;
                    break;
                case SoeExportFormat.CharacterSeparatedValues:
                    exportFormatType = ExportFormatType.CharacterSeparatedValues;
                    break;

                    #endregion
            }

            return exportFormatType;
        }

        public SoeExportFormat GetReportExportFormat(TermGroup_ReportExportType reportExportType, TermGroup_ReportExportFileType exportFileType = TermGroup_ReportExportFileType.Unknown)
        {
            SoeExportFormat exportFormat = SoeExportFormat.Xml;

            switch (reportExportType)
            {
                case TermGroup_ReportExportType.Pdf:
                    exportFormat = SoeExportFormat.Pdf;
                    break;
                case TermGroup_ReportExportType.Xml:
                    exportFormat = SoeExportFormat.Xml;
                    break;
                case TermGroup_ReportExportType.MatrixExcel:
                    exportFormat = SoeExportFormat.ExcelXlsx;
                    break;
                case TermGroup_ReportExportType.Excel:
                    exportFormat = SoeExportFormat.Excel;
                    break;
                case TermGroup_ReportExportType.Word:
                    exportFormat = SoeExportFormat.Word;
                    break;
                case TermGroup_ReportExportType.RichText:
                    exportFormat = SoeExportFormat.RichText;
                    break;
                case TermGroup_ReportExportType.EditableRTF:
                    exportFormat = SoeExportFormat.EditableRTF;
                    break;
                case TermGroup_ReportExportType.MatrixText:
                case TermGroup_ReportExportType.Text:
                    exportFormat = SoeExportFormat.Text;
                    break;
                case TermGroup_ReportExportType.TabSeperatedText:
                    exportFormat = SoeExportFormat.TabSeperatedText;
                    break;
                case TermGroup_ReportExportType.CharacterSeparatedValues:
                    exportFormat = SoeExportFormat.CharacterSeparatedValues;
                    break;
                case TermGroup_ReportExportType.File:
                    switch (exportFileType)
                    {
                        case TermGroup_ReportExportFileType.Payroll_SIE_Accounting:
                        case TermGroup_ReportExportFileType.Payroll_SIE_VacationAccounting:
                            exportFormat = SoeExportFormat.Payroll_SIE_Accounting;
                            break;
                        case TermGroup_ReportExportFileType.Payroll_SCB_Statistics:
                            exportFormat = SoeExportFormat.Payroll_SCB_Statistics;
                            break;
                        case TermGroup_ReportExportFileType.Payroll_SN_Statistics:
                            exportFormat = SoeExportFormat.Payroll_SN_Statistics;
                            break;
                        case TermGroup_ReportExportFileType.Payroll_Visma_Accounting:
                        case TermGroup_ReportExportFileType.Payroll_Visma_VacationAccounting:
                            exportFormat = SoeExportFormat.Payroll_Visma_Accounting;
                            break;
                        case TermGroup_ReportExportFileType.Payroll_EKS_Accounting:
                        case TermGroup_ReportExportFileType.Payroll_EKS_VacationAccounting:
                            exportFormat = SoeExportFormat.Payroll_EKS_Accounting;
                            break;
                        case TermGroup_ReportExportFileType.Payroll_SAP_Accounting:
                        case TermGroup_ReportExportFileType.Payroll_SAP_VacationAccounting:
                            exportFormat = SoeExportFormat.Payroll_SAP_Accounting;
                            break;
                        case TermGroup_ReportExportFileType.KU10:
                            exportFormat = SoeExportFormat.KU10;
                            break;
                        case TermGroup_ReportExportFileType.eSKD:
                            exportFormat = SoeExportFormat.eSKD;
                            break;
                        case TermGroup_ReportExportFileType.QlikViewType1:
                            exportFormat = SoeExportFormat.QlikViewType1;
                            break;
                        case TermGroup_ReportExportFileType.Collectum:
                            exportFormat = SoeExportFormat.Collectum;
                            break;
                        case TermGroup_ReportExportFileType.KPA:
                            exportFormat = SoeExportFormat.Text;
                            break;
                        case TermGroup_ReportExportFileType.Fora:
                            exportFormat = SoeExportFormat.Text;
                            break;
                        case TermGroup_ReportExportFileType.ForaMonthly:
                            exportFormat = SoeExportFormat.Json;
                            break;
                        case TermGroup_ReportExportFileType.SCB_KSJU:
                            exportFormat = SoeExportFormat.Text;
                            break;
                        case TermGroup_ReportExportFileType.AGD:
                            exportFormat = SoeExportFormat.EmployerDeclarationIndividual;
                            break;
                        case TermGroup_ReportExportFileType.KPADirekt:
                            exportFormat = SoeExportFormat.KPADirekt;
                            break;
                        case TermGroup_ReportExportFileType.SCB_KLP:
                            exportFormat = SoeExportFormat.SCB_KLP;
                            break;
                        case TermGroup_ReportExportFileType.Kronofogden:
                            exportFormat = SoeExportFormat.CharacterSeparatedValues;
                            break;
                        case TermGroup_ReportExportFileType.FolksamGTP:
                            exportFormat = SoeExportFormat.Text;
                            break;
                        case TermGroup_ReportExportFileType.IFMetall:
                            exportFormat = SoeExportFormat.Text;
                            break;
                        case TermGroup_ReportExportFileType.SkandiaPension:
                            exportFormat = SoeExportFormat.SkandiaPension;
                            break;
                        case TermGroup_ReportExportFileType.SEF:
                            exportFormat = SoeExportFormat.Text;
                            break;
                        case TermGroup_ReportExportFileType.AGD_Franvarouppgift:
                            exportFormat = SoeExportFormat.AGD_Franvarouppgift;
                            break;
                    }

                    break;
            }

            return exportFormat;
        }

        public SoeExportFormat ConvertToSoeExportFormat(string fileType)
        {
            switch (fileType)
            {
                #region Data

                case Constants.SOE_SERVER_FILE_PDF_SUFFIX:
                    return SoeExportFormat.Pdf;
                case Constants.SOE_SERVER_FILE_XML_SUFFIX:
                    return SoeExportFormat.Xml;
                case Constants.SOE_SERVER_FILE_EXCEL_SUFFIX:
                case Constants.SOE_SERVER_FILE_EXCEL2007_SUFFIX:
                    return SoeExportFormat.Excel;
                case Constants.SOE_SERVER_FILE_WORD_SUFFIX:
                case Constants.SOE_SERVER_FILE_WORD2007_SUFFIX:
                    return SoeExportFormat.Word;
                case Constants.SOE_SERVER_FILE_ZIP_SUFFIX:
                    return SoeExportFormat.Zip;
                case Constants.SOE_SERVER_FILE_RTF_SUFFIX:
                    return SoeExportFormat.RichText;
                case Constants.SOE_SERVER_FILE_TEXT_SUFFIX:
                    return SoeExportFormat.Text;
                case Constants.SOE_SERVER_FILE_TABBSEPARATED_SUFFIX:
                    return SoeExportFormat.TabSeperatedText;
                case Constants.SOE_SERVER_FILE_CHARACTERSEPARATED_SUFFIX:
                    return SoeExportFormat.CharacterSeparatedValues;

                #endregion

                #region Images

                case "gif":
                    return SoeExportFormat.Gif;
                case "png":
                    return SoeExportFormat.Png;
                case "jpeg":
                    return SoeExportFormat.Jpeg;

                #endregion

                #region Video

                case "wmf":
                    return SoeExportFormat.Wmf;
                case "avi":
                    return SoeExportFormat.Avi;
                case "mpeg":
                    return SoeExportFormat.Mpeg;

                #endregion

                default:
                    return SoeExportFormat.Unknown;
            }
        }

        public TermGroup_ReportPrintoutDeliveryType GetReportDeliveryType(bool? email = null, bool? xeMail = null, bool? generate = null)
        {
            if (email == true)
                return TermGroup_ReportPrintoutDeliveryType.Email;
            else if (xeMail == true)
                return Common.Util.TermGroup_ReportPrintoutDeliveryType.XEMail;
            else if (generate == true)
                return Common.Util.TermGroup_ReportPrintoutDeliveryType.Generate;
            else
                return Common.Util.TermGroup_ReportPrintoutDeliveryType.Instant;
        }

        public string GetReportFileFype(SoeExportFormat exportFormat)
        {
            string fileType = "";

            switch (exportFormat)
            {
                #region Data

                case SoeExportFormat.Pdf:
                case SoeExportFormat.MergedPDF:
                    fileType = Constants.SOE_SERVER_FILE_PDF_SUFFIX;
                    break;
                case SoeExportFormat.Xml:
                    fileType = Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    break;
                case SoeExportFormat.Excel:
                    fileType = Constants.SOE_SERVER_FILE_EXCEL_SUFFIX;
                    break;
                case SoeExportFormat.ExcelXlsx:
                    fileType = Constants.SOE_SERVER_FILE_EXCEL2007_SUFFIX;
                    break;
                case SoeExportFormat.Word:
                    fileType = Constants.SOE_SERVER_FILE_WORD_SUFFIX;
                    break;
                case SoeExportFormat.RichText:
                    fileType = Constants.SOE_SERVER_FILE_RTF_SUFFIX;
                    break;
                case SoeExportFormat.EditableRTF:
                    fileType = Constants.SOE_SERVER_FILE_RTF_SUFFIX;
                    break;
                case SoeExportFormat.Text:
                    fileType = Constants.SOE_SERVER_FILE_TEXT_SUFFIX;
                    break;
                case SoeExportFormat.TabSeperatedText:
                    fileType = Constants.SOE_SERVER_FILE_TABBSEPARATED_SUFFIX;
                    break;
                case SoeExportFormat.CharacterSeparatedValues:
                    fileType = Constants.SOE_SERVER_FILE_CHARACTERSEPARATED_SUFFIX;
                    break;
                case SoeExportFormat.Zip:
                    fileType = Constants.SOE_SERVER_FILE_ZIP_SUFFIX;
                    break;
                case SoeExportFormat.Payroll_SIE_Accounting:
                    fileType = Constants.SOE_SERVER_FILE_SI_SUFFIX;
                    break;
                case SoeExportFormat.Payroll_Visma_Accounting:
                    fileType = Constants.SOE_SERVER_FILE_VISMA_SUFFIX;
                    break;
                case SoeExportFormat.Payroll_EKS_Accounting:
                    fileType = Constants.SOE_SERVER_FILE_EKS_SUFFIX;
                    break;
                case SoeExportFormat.Payroll_SCB_Statistics:
                    fileType = Constants.SOE_SERVER_FILE_TEXT_SUFFIX;
                    break;
                case SoeExportFormat.Payroll_SN_Statistics:
                    fileType = Constants.SOE_SERVER_FILE_TEXT_SUFFIX;
                    break;
                case SoeExportFormat.KU10:
                    fileType = Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    break;
                case SoeExportFormat.eSKD:
                    fileType = Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    break;
                case SoeExportFormat.QlikViewType1:
                    fileType = Constants.SOE_SERVER_FILE_TEXT_SUFFIX;
                    break;
                case SoeExportFormat.Collectum:
                    fileType = Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    break;
                case SoeExportFormat.EmployerDeclarationIndividual:
                    fileType = Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    break;
                case SoeExportFormat.KPADirekt:
                    fileType = Constants.SOE_SERVER_FILE_TEXT_SUFFIX;
                    break;
                case SoeExportFormat.SCB_KLP:
                    fileType = Constants.SOE_SERVER_FILE_TEXT_SUFFIX;
                    break;
                case SoeExportFormat.SkandiaPension:
                    fileType = Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    break;
                case SoeExportFormat.Json:
                    fileType = Constants.SOE_SERVER_FILE_JSON_SUFFIX;
                    break;
                case SoeExportFormat.AGD_Franvarouppgift:
                    fileType = Constants.SOE_SERVER_FILE_XML_SUFFIX;
                    break;

                #endregion

                #region Images

                case SoeExportFormat.Gif:
                    fileType = Constants.SOE_SERVER_FILE_GIF_SUFFIX;
                    break;
                case SoeExportFormat.Png:
                    fileType = Constants.SOE_SERVER_FILE_PNG_SUFFIX;
                    break;
                case SoeExportFormat.Jpeg:
                    fileType = Constants.SOE_SERVER_FILE_JPEG_SUFFIX;
                    break;

                #endregion

                #region Video

                case SoeExportFormat.Wmf:
                    fileType = Constants.SOE_SERVER_FILE_WMF_SUFFIX;
                    break;
                case SoeExportFormat.Avi:
                    fileType = Constants.SOE_SERVER_FILE_AVI_SUFFIX;
                    break;
                case SoeExportFormat.Mpeg:
                    fileType = Constants.SOE_SERVER_FILE_MPEG_SUFFIX;
                    break;

                #endregion

                default:
                    fileType = "";
                    break;
            }

            return fileType;
        }

        public void GetResponseContentType(SoeExportFormat exportFormat, out string contentType, out string fileType, out string imageSrc)
        {
            fileType = GetReportFileFype(exportFormat);
            imageSrc = "/img/mimetypes/";

            switch (exportFormat)
            {
                #region Data

                case SoeExportFormat.Pdf:
                case SoeExportFormat.MergedPDF:
                    contentType = "application/pdf";
                    imageSrc += "pdf.gif";
                    break;
                case SoeExportFormat.Xml:
                    contentType = "application/xml";
                    imageSrc += "xml.gif";
                    break;
                case SoeExportFormat.Excel:
                    contentType = "application/excel"; //application/vnd.ms-excel
                    imageSrc += "xls.gif";
                    break;
                case SoeExportFormat.ExcelXlsx:
                    contentType = "application/excel"; //application/vnd.ms-excel
                    imageSrc += "xls.gif";
                    break;
                case SoeExportFormat.Word:
                    contentType = "application/msword";
                    imageSrc += "doc.gif";
                    break;
                case SoeExportFormat.RichText:
                case SoeExportFormat.EditableRTF:
                case SoeExportFormat.Text:
                case SoeExportFormat.TabSeperatedText:
                case SoeExportFormat.CharacterSeparatedValues:
                case SoeExportFormat.Zip:
                    contentType = "application/x-compressed";
                    imageSrc += "blank.gif";
                    break;

                #endregion

                #region Images

                case SoeExportFormat.Gif:
                    contentType = "image/gif";
                    imageSrc += "blank.gif";
                    break;
                case SoeExportFormat.Png:
                    contentType = "image/png";
                    imageSrc += "blank.gif";
                    break;
                case SoeExportFormat.Jpeg:
                    contentType = "image/jpeg";
                    break;

                #endregion

                #region Video

                case SoeExportFormat.Wmf:
                    contentType = "image/x-wmf";
                    imageSrc += "blank.gif";
                    break;
                case SoeExportFormat.Avi:
                    contentType = "video/avi";
                    imageSrc += "blank.gif";
                    break;
                case SoeExportFormat.Mpeg:
                    contentType = "video/mpeg";
                    imageSrc += "blank.gif";
                    break;

                #endregion

                default:
                    contentType = "application/unknown";
                    imageSrc += "blank.gif";
                    break;
            }
        }

        #endregion

        #region Validation

        public bool ValidateReportTemplate(int sysReportTemplateTypeId, byte[] templateData, SoeReportType reportType)
        {
            SysReportTemplateType sysReportTemplateType = ReportManager.GetSysReportTemplateType(sysReportTemplateTypeId);
            if (sysReportTemplateType == null)
                return false;

            return ValidateReportTemplateCrGen((SoeReportTemplateType)sysReportTemplateType.SysReportTermId, templateData, reportType);
        }

        public bool ValidateReportTemplateCrGen(SoeReportTemplateType reportTemplateType, byte[] templateData, SoeReportType reportType)
        {
            ReportPrintoutDTO dto = new ReportPrintoutDTO();
            dto.ExportType = TermGroup_ReportExportType.Pdf;

            DataSet dataSet = GetDefaultXmlDataSet(reportTemplateType);

            List<RptGenRequestPicturesDTO> crGenRequestPicturesDTO = new List<RptGenRequestPicturesDTO>();
            RptGenConnector crgen = RptGenConnector.GetConnector(parameterObject, reportType);
            RptGenResultDTO crGenResult = crgen.GenerateReport(dto.ExportType, templateData, null, dataSet, crGenRequestPicturesDTO, GetCulture(GetLangId()), GetXsdFileString(reportTemplateType), $"dto reportid:{dto.ReportId} actorcompanyid: {dto.ActorCompanyId}");
            if (crGenResult != null)
            {
                dto.Data = crGenResult.GeneratedReport;
                if (dto.Data != null && crGenResult.Success)
                    return true;

                LogError(crGenResult.ErrorMessage);
            }

            return false;
        }

        public bool ValidateDocument(XDocument document, SoeReportTemplateType reportTemplateType, out string errorMessage)
        {
            string filePathXsd = GetXsdFilePath(reportTemplateType);
            return ValidateDocument(document, filePathXsd, out errorMessage);
        }

        public bool ValidateDocument(XDocument document, string filePathXsd, out string errorMessage)
        {
            Exception error = null;
            errorMessage = "";

            try
            {
                string file = File.ReadAllText(filePathXsd);

                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", XmlReader.Create(new StringReader(file)));
                document.Validate(schemas, (o, e) =>
                {
                    error = new SoeGeneralException(e.Message, this.ToString());
                });
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                if (error != null)
                {
                    errorMessage = error.Message;
                    base.LogError(error, this.log);
                }
            }

            return error == null;
        }

        public bool ValidateXDocument(XDocument document, List<String> xsdFilePaths)
        {
            bool errors = false;
            string message = "";
            XmlSchemaSet schemas = new XmlSchemaSet();

            foreach (var xsd in xsdFilePaths)
            {
                String fileContent = File.ReadAllText(xsd);
                schemas.Add(null, XmlReader.Create(new StringReader(fileContent)));
            }

            document.Validate(schemas, (o, e) =>
            {
                message = e.Message;
                errors = true;
            });

            if (errors)
            {
                var sge = new SoeGeneralException(message, this.ToString());
                base.LogError(sge, this.log);
                return false;
            }
            return true;
        }

        #endregion
    }
}
