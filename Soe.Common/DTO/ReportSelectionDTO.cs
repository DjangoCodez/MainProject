using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SoftOne.Soe.Common.DTO
{
    /// <summary>
    /// Container class for Selection
    /// Other classes in this file has a dependent UserControl that fills it's properties.
    /// When the UserControl's values is validated and evaluated its values are copied to EvaluatedSelection object.
    /// The EvaluatedSelection object its used to get data to Reports and saved ReportSelections.
    /// </summary>
    public class Selection
    {
        #region Evaluated selection

        private readonly EvaluatedSelection es;
        public EvaluatedSelection Evaluated
        {
            get
            {
                return es;
            }
        }

        #endregion

        #region Selection in code (QS)

        public ReportSelectionDTO ReportSelectionDTO { get; set; }

        #endregion

        #region Selection in Web

        public SelectionStd SelectionStd { get; set; }
        public SelectionAccount SelectionAccount { get; set; }
        public SelectionVoucher SelectionVoucher { get; set; }
        public SelectionFixedAssets SelectionFixedAssets { get; set; }
        public SelectionUser SelectionUser { get; set; }
        public SelectionLedger SelectionLedger { get; set; }
        public SelectionBilling SelectionBilling { get; set; }

        #endregion

        #region Ctor

        public Selection(int actorCompanyId, int userId, int roleId, string loginName, ReportDTO report = null, ReportPackageDTO reportPackage = null, bool isMainReport = false, int exportType = 0, int exportFileType = 0)
        {
            es = new EvaluatedSelection()
            {
                ActorCompanyId = actorCompanyId,
                UserId = userId,
                RoleId = roleId,
                LoginName = loginName,
                ExportType = (TermGroup_ReportExportType)exportType,
            };

            if (es.ExportType == TermGroup_ReportExportType.File)
            {
                if (exportFileType != 0)
                    es.ExportFileType = (TermGroup_ReportExportFileType)exportFileType;
                else if (report != null && report.ExportFileType != 0)
                    es.ExportFileType = report.ExportFileType;

            }
            if (report != null)
            {
                es.ReportId = report.ReportId;
                es.ReportTemplateId = report.ReportTemplateId;
                es.ReportNr = report.ReportNr;
                es.ReportName = report.Name;
                es.ReportDescription = report.Description;
                es.IsReportStandard = report.Standard;
                es.IncludeAllHistoricalData = report.IncludeAllHistoricalData;
                es.IncludeBudget = report.IncludeBudget;
                es.NoOfYearsBackinPreviousYear = report.NoOfYearsBackinPreviousYear;
                es.GetDetailedInformation = report.DetailedInformation;
                es.IsMainReport = isMainReport;
                es.GroupByLevel1 = report.GroupByLevel1;
                es.GroupByLevel2 = report.GroupByLevel2;
                es.GroupByLevel3 = report.GroupByLevel3;
                es.GroupByLevel4 = report.GroupByLevel4;
                es.SortByLevel1 = report.SortByLevel1;
                es.SortByLevel2 = report.SortByLevel2;
                es.SortByLevel3 = report.SortByLevel3;
                es.SortByLevel4 = report.SortByLevel4;
                es.IsSortAscending = report.IsSortAscending;
                es.Special = report.Special;
                es.ShowInAccountingReports = report.ShowInAccountingReports;
                es.NrOfDecimals = report.NrOfDecimals;
                es.ShowRowsByAccount = report.ShowRowsByAccount;
                if (report.ReportSelectionInt != null)
                {
                    foreach (var reportSelectionInt in report.ReportSelectionInt)
                    {
                        if (reportSelectionInt.ReportSelectionType == SoeSelectionData.Int_BudgetId)
                        {
                            es.SSTD_BudgetId = reportSelectionInt.SelectFrom;
                            break;
                        }
                    }
                }
            }
            if (reportPackage != null)
            {
                es.ReportPackageId = reportPackage.ReportPackageId;
                es.ReportPackageName = reportPackage.Name;
                es.ReportPackageDescription = reportPackage.Description;
            }
        }

        #endregion

        #region Evaluate

        public bool Evaluate(ReportSelectionDTO dto, int? reportUrlId)
        {
            if (!es.Evaluate(dto))
                return false;

            this.ReportSelectionDTO = dto;
            this.Evaluated.ReportUrlId = reportUrlId;
            return true;
        }

        #endregion
    }

    #region Evaluated selection

    /// <summary>
    /// Contains the evaluated data. Used by Business to get correct data for Reports
    /// </summary>
    public class EvaluatedSelection
    {
        #region Ctor

        public EvaluatedSelection()
        {
            OnlyActiveAccounts = null; //For now (maybe a choice in gui in the future)
        }

        #endregion

        #region General

        //Identifier
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string LoginName { get; set; }

        //Settings
        public bool Preview { get; set; }
        public bool? OnlyActiveAccounts { get; set; }
        public bool IgnoreSchema { get; set; }
        public string ReportNamePostfix { get; set; }
        public string FilePath { get; set; }
        public TermGroup_ReportExportType ExportType { get; set; }
        public TermGroup_ReportExportFileType ExportFileType { get; set; }

        //Dates
        private DateTime dateFrom;
        public DateTime DateFrom
        {
            get
            {
                //Read only if HasDate is true and its valid
                if (HasDateInterval && CalendarUtility.IsDateTimeSqlServerValid(dateFrom))
                    return CalendarUtility.GetBeginningOfDay(dateFrom);
                return CalendarUtility.DATETIME_DEFAULT;
            }
            set
            {
                dateFrom = value;
            }
        }
        private DateTime dateTo;
        public DateTime DateTo
        {
            get
            {
                //Read only if HasDate is true and its valid
                if (HasDateInterval && CalendarUtility.IsDateTimeSqlServerValid(dateTo))
                    return CalendarUtility.GetEndOfDay(dateTo);
                return CalendarUtility.DATETIME_DEFAULT;
            }
            set
            {
                dateTo = CalendarUtility.GetEndOfDay(value);
            }
        }
        public bool HasDateInterval { get; set; }
        public DateTime UntilDateFrom
        {
            get
            {
                return DateFrom.Date.AddDays(-1);
            }
        }

        //Report
        public int ReportId { get; set; }
        public int ReportTemplateId { get; set; }
        public int ReportNr { get; set; }
        public string ReportName { get; set; }
        public string ReportDescription { get; set; }
        public bool IsReportStandard { get; set; }
        public bool IncludeAllHistoricalData { get; set; }
        public bool GetDetailedInformation { get; set; }
        public bool IncludeBudget { get; set; }
        public bool MergePdfs { get; set; }
        public bool IsSortAscending { get; set; }
        public string Special { get; set; }
        public bool IsMainReport { get; set; }
        public int? ReportUrlId { get; set; }
        public int NoOfYearsBackinPreviousYear { get; set; }
        public int? InvoiceDistributionId { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel1 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel2 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel3 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes SortByLevel4 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel1 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel2 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel3 { get; set; }
        public TermGroup_ReportGroupAndSortingTypes GroupByLevel4 { get; set; }
        public bool ShowInAccountingReports { get; set; }
        public int? NrOfDecimals { get; set; }
        public bool ShowRowsByAccount { get; set; }

        //ReportPackage
        public int ReportPackageId { get; set; }
        public string ReportPackageName { get; set; }
        public string ReportPackageDescription { get; set; }

        //ReportSelection
        public string ReportSelectionText { get; set; }
        public string Selection { get; set; }

        //ReportTemplate
        public SoeReportTemplateType ReportTemplateType { get; set; }
        public byte[] Template { get; set; }
        public SoeReportType ReportType { get; set; }

        //Email
        public bool? Email { get; set; }
        public int? EmailTemplateId { get; set; }
        public List<int> EmailRecipients { get; set; }
        public string SingleRecipients { get; set; }
        public string EmailFileName { get; set; }
        public EmailTemplateDTO EmailTemplate { get; set; }
        public List<KeyValuePair<string, byte[]>> EmailAttachments { get; set; }
        public List<KeyValuePair<string, byte[]>> InvoiceAttachments { get; set; }

        //Misc
        public decimal CalculatedGross { get; set; }

        #endregion

        #region SelectionStd

        public bool SSTD_IsEvaluated { get; set; }

        public bool SSTD_IsSameYear { get; set; }
        public int SSTD_AccountYearId { get; set; }
        public int SSTD_PreviousAccountYearId { get; set; }

        public string SSTD_AccountYearFromText { get; set; }
        public string SSTD_AccountYearToText { get; set; }
        public string SSTD_LongAccountYearFromText { get; set; }
        public string SSTD_LongAccountYearToText { get; set; }
        public string SSTD_PreviousAccountYearFromText { get; set; }
        public string SSTD_PreviousAccountYearToText { get; set; }
        public bool SSTD_HasAccountYearText { get; set; }

        public string SSTD_AccountPeriodFromText { get; set; }
        public string SSTD_AccountPeriodToText { get; set; }
        public bool SSTD_HasAccountPeriodText { get; set; }

        public bool SSTD_OnlyFrom { get; set; }
        public bool SSTD_CreateVatVoucher { get; set; }

        public int? SSTD_BudgetId { get; set; }
        public bool SSTD_IncludeYearEndVouchers { get; set; }
        public bool SSTD_IncludeExternalVouchers { get; set; }
        public bool SSTD_ProjectReport { get; set; }

        public int SSTD_AccountDimId { get; set; }
        public bool SSTD_IncludeMissingAccountDim { get; set; }
        public bool SSTD_SeparateAccountDim { get; set; }

        #endregion

        #region SelectionVoucher

        public bool SV_IsEvaluated { get; set; }

        private int sv_voucherNrFrom;
        public int SV_VoucherNrFrom
        {
            get
            {
                //Read only if HasVoucherNrInterval is true
                if (SV_HasVoucherNrInterval)
                    return sv_voucherNrFrom;
                return -1;
            }
            set
            {
                sv_voucherNrFrom = value;
            }
        }

        private int sv_voucherNrTo;
        public int SV_VoucherNrTo
        {
            get
            {
                //Read only if HasVoucherNrInterval is true
                if (SV_HasVoucherNrInterval)
                    return sv_voucherNrTo;
                return -1;
            }
            set
            {
                sv_voucherNrTo = value;
            }
        }

        public bool SV_HasVoucherSeriesTypeNrInterval { get; set; }
        private int voucherSeriesTypeNrFrom;
        public int SV_VoucherSeriesTypeNrFrom
        {
            get
            {
                //Read only if HasVoucherSeriesTypeNrInterval is true
                if (SV_HasVoucherSeriesTypeNrInterval)
                    return voucherSeriesTypeNrFrom;
                return -1;
            }
            set
            {
                voucherSeriesTypeNrFrom = value;
            }
        }

        public bool SV_HasVoucherNrInterval { get; set; }
        private int voucherSeriesTypeNrTo;
        public int SV_VoucherSeriesTypeNrTo
        {
            //Read only if HasVoucherSeriesTypeNrInterval is true
            get
            {
                if (SV_HasVoucherSeriesTypeNrInterval)
                    return voucherSeriesTypeNrTo;
                return -1;
            }
            set
            {
                voucherSeriesTypeNrTo = value;
            }
        }

        public bool SV_HasVoucherHeadIds { get; set; }
        public List<int> SV_VoucherHeadIds { get; set; }
        public bool SV_IsAccountingOrder { get; set; }

        public bool SV_HasVoucherInterval { get; set; } = false;
		private List<VoucherIntervalDTO> voucherIntervals;
		public List<VoucherIntervalDTO> SV_VoucherIntervals
		{
			get
			{
				//Read only if HasVoucherInterval is true
				if (SV_HasVoucherInterval)
					return voucherIntervals;
				return null;
			}
			set
			{
				voucherIntervals = value;
			}
		}

		#endregion

        #region SelectionFixedAssets

        public bool SFA_IsEvaluated { get; set; }
        public bool SFA_HasInventoryInterval { get; set; }
        public string SFA_InventoryFrom { get; set; }
        public string SFA_InventoryTo { get; set; }
        public bool SFA_HasCategoryInterval { get; set; }
        public string SFA_CategoryFrom { get; set; }
        public string SFA_CategoryTo { get; set; }
        public int SFA_PrognoseType { get; set; } //TermGroup_PrognoseInterval

        #endregion

        #region SelectionAccount

        public bool SA_IsEvaluated { get; set; }

        public bool SA_HasAccountInterval { get; set; }
        private List<AccountIntervalDTO> accountIntervals;
        public List<AccountIntervalDTO> SA_AccountIntervals
        {
            get
            {
                //Read only if HasAccountInterval is true
                if (SA_HasAccountInterval)
                    return accountIntervals;
                return null;
            }
            set
            {
                accountIntervals = value;
            }
        }

        #endregion

        #region SelectionUser

        public bool SU_IsEvaluated { get; set; }
        public bool SU_HasUser { get; set; }

        private int userId;
        public int SU_UserId
        {
            get
            {
                //Read only if HasUser is true and its valid
                if (SU_HasUser)
                    return userId;
                return -1;
            }
            set
            {
                userId = value;
            }
        }

        #endregion

        #region SelectionLedger

        public bool SL_IsEvaluated { get; set; }
        public int SL_SortOrder { get; set; } //TermGroup_ReportLedgerSortOrder
        public int SL_DateRegard { get; set; } //TermGroup_ReportLedgerDateRegard
        public int SL_InvoiceSelection { get; set; } //TermGroup_ReportLedgerInvoiceSelection
        public bool SL_ShowVoucher { get; set; }
        public bool SL_ShowPendingPaymentsInReport { get; set; }
        public bool SL_ShowPreliminaryInvoices { get; set; }
        public bool SL_IncludeCashSalesInvoices { get; set; }
        
        public bool SL_HasActorNrInterval { get; set; }
        public string SL_ActorNrFrom { get; set; }
        public string SL_ActorNrTo { get; set; }

        public bool SL_HasInvoiceSeqNrInterval { get; set; }
        public int SL_InvoiceSeqNrFrom { get; set; }
        public int SL_InvoiceSeqNrTo { get; set; }

        public bool SL_HasInvoiceIds { get; set; }
        public List<int> SL_InvoiceIds { get; set; }
        public List<int> SL_PaymentRowIds { get; set; }

        #endregion

        #region SelectionBilling

        public bool SB_IsEvaluated { get; set; }
        public int SB_SortOrder { get; set; } //TermGroup_ReportBillingInvoiceSortOrder

        public string SB_CustomerNrFrom { get; set; }
        public string SB_CustomerNrTo { get; set; }
        public bool SB_HasCustomerNrInterval { get; set; }

        public string SB_InvoiceNrFrom { get; set; }
        public string SB_InvoiceNrTo { get; set; }
        public bool SB_HasInvoiceNrInterval { get; set; }

        public bool SB_HasInvoiceIds { get; set; }
        public List<int> SB_InvoiceIds { get; set; }

        public bool SB_HasProjectIds { get; set; }
        public List<int> SB_ProjectIds { get; set; }
        public bool SB_HasPurchaseIds { get; set; }
        public List<int> SB_PurchaseIds { get; set; }

        public int? SB_ChecklistHeadRecordId { get; set; }

        public bool SB_InvoiceCopy { get; set; }
        public bool SB_InvoiceReminder { get; set; }
        public bool SB_DisableInvoiceCopies { get; set; }
        public bool SB_IncludeDrafts { get; set; }
        public bool SB_ShowNotPrinted { get; set; }
        public bool SB_ShowCopy { get; set; }
        public int SB_CustomerGroupId { get; set; }
        public int SB_ReportLanguageId { get; set; }
        public bool SB_IncludeClosedOrder { get; set; }
        public string SB_ProductNrFrom { get; set; }
        public string SB_ProductNrTo { get; set; }
        public bool SB_HasProductIds { get; set; }
        public List<int> SB_ProductIds { get; set; }
        public int SB_StockLocationIdFrom { get; set; }
        public int SB_StockLocationIdTo { get; set; }
        public int SB_StockShelfIdFrom { get; set; }
        public int SB_StockShelfIdTo { get; set; }
        public int SB_StockInventoryId { get; set; }
        public DateTime SB_PaymentDateFrom { get; set; }
        public DateTime SB_PaymentDateTo { get; set; }
        public bool SB_HasPaymentDateInterval { get; set; }
        public string SB_PeriodFrom { get; set; }
        public string SB_PeriodTo { get; set; }


        #region ProjectReport

        public string SB_ProjectNrFrom { get; set; }
        public string SB_ProjectNrTo { get; set; }
        public bool SB_HasProjectNrInterval { get; set; }
        public String SB_EmployeeNrFrom { get; set; }
        public String SB_EmployeeNrTo { get; set; }
        public bool SB_HasEmployeeNrInterval { get; set; }
        public bool SB_HasProjectIdList { get; set; }
        public List<int> SB_ProjectIdList { get; set; }
        public bool SB_IncludeOnlyInvoiced { get; set; }
        public bool SB_IncludeProjectReport { get; set; }
        public bool SB_IncludeProjectReport2 { get; set; }

        #endregion

        #region HouseholdTaxDeduction

        public int SB_HTDCompanyId { get; set; }
        public int SB_HTDSeqNbr { get; set; }
        public bool SB_HTDHasCustomerInvoiceRowIds { get; set; }
        public List<int> SB_HTDCustomerInvoiceRowIds { get; set; }
        public bool SB_HTDShowFile { get; set; }
        public bool SB_HTDUseGreen { get; set; }

        #endregion

        #endregion

        #region SelectionProject

        public bool SP_IsEvaluated { get; set; }

        public List<int> SP_ProjectIds { get; set; }
        public string SP_OfferNrFrom { get; set; }
        public string SP_OfferNrTo { get; set; }
        public string SP_OrderNrFrom { get; set; }
        public string SP_OrderNrTo { get; set; }
        public string SP_InvoiceNrFrom { get; set; }
        public string SP_InvoiceNrTo { get; set; }
        public string SP_EmployeeNrFrom { get; set; }
        public string SP_EmployeeNrTo { get; set; }
        public string SP_PayrollProductNrFrom { get; set; }
        public string SP_PayrollProductNrTo { get; set; }
        public string SP_InvoiceProductNrFrom { get; set; }
        public string SP_InvoiceProductNrTo { get; set; }
        public string SP_Dim2From { get; set; }
        public string SP_Dim2To { get; set; }
        public string SP_Dim3From { get; set; }
        public string SP_Dim3To { get; set; }
        public string SP_Dim4From { get; set; }
        public string SP_Dim4To { get; set; }
        public string SP_Dim5From { get; set; }
        public string SP_Dim5To { get; set; }
        public string SP_Dim6From { get; set; }
        public string SP_Dim6To { get; set; }
        public DateTime? SP_PayrollTransactionDateFrom { get; set; }
        public DateTime? SP_PayrollTransactionDateTo { get; set; }
        public DateTime? SP_InvoiceTransactionDateFrom { get; set; }
        public DateTime? SP_InvoiceTransactionDateTo { get; set; }
        public bool SP_IncludeChildProjects { get; set; }

        #endregion

        #region SelectionTime

        public bool ST_IsEvaluated { get; set; }

        public List<int> ST_PayrollProductIds { get; set; }
        public List<int> ST_ShiftTypeIds { get; set; }
        public List<int> ST_VacationGroupIds { get; set; }
        public List<int> ST_CategoryIds { get; set; }
        public List<int> ST_EmployeeIds { get; set; }
        public List<int> ST_EmployeePostIds { get; set; }
        public List<int> ST_TimePeriodIds { get; set; }
        public int ST_Year { get; set; }
        public bool ST_KU10RemovePrevSubmittedData { get; set; }
        public bool ST_ShowAllEmployees { get; set; }
        public int ST_EmployeeId
        {
            get
            {
                return ST_HasOneEmployee ? ST_EmployeeIds.First() : 0;
            }
        }
        public bool ST_HasOneEmployee
        {
            get
            {
                return ST_EmployeeIds != null && ST_EmployeeIds.Count == 1;
            }
        }
        public int ST_EmploymentId { get; set; }
        public DateTime? ST_ChangesForDate { get; set; }
        public List<DateTime> ST_SubstituteDates { get; set; }
        public int? ST_TimePeriodId { get; set; }
        public int? ST_AccumulatorId { get; set; }
        public int? ST_DataStorageId { get; set; }
        public bool ST_IncludePreliminary { get; set; }
        public bool ST_IncludePayrollStartvalues { get; set; }
        public bool ST_IncludeInactiveEmployees { get; set; }
        public bool ST_ShowOnlyTotals { get; set; }
        public bool ST_PrintedFromSchedulePlanning { get; set; }
        public bool ST_OnlyActiveEmployees
        {
            get
            {
                return !ST_IncludeInactiveEmployees;
            }
        }
        public bool? ST_ActiveEmployees
        {
            get
            {
                return ST_IncludeInactiveEmployees ? (bool?)null : true;
            }
        }
        public bool ST_Preliminary { get; set; }
        public string ST_EmployeeXml { get; set; }

        #endregion

        #region SelectionImport

        public List<int> SI_CustomerInvoiceHeadIOIds { get; set; }
        public List<int> SI_VoucherHeadIOIds { get; set; }

        #endregion

        #region SelectionSalary(PayrollProduct)

        public List<int> SS_PayrollProductIds { get; set; }

        #endregion

        #region Evaluate

        public bool Evaluate(ReportSelectionDTO dto)
        {
            if (dto == null)
                return false;

            bool evaluated = false;

            #region Billing

            if (dto is BillingReportDTO)
            {
                if (dto is BillingInvoiceReportDTO billingInvoice)
                    evaluated = this.Evaluate(billingInvoice);
                else if (dto is BillingInvoiceTimeProjectReportDTO timeProject)
                    evaluated = this.Evaluate(timeProject);
                else if (dto is HouseholdTaxDeductionReportDTO householdTaxDeduction)
                    evaluated = this.Evaluate(householdTaxDeduction);
                else if (dto is OrderChecklistReportDTO checklist)
                    evaluated = this.Evaluate(checklist);
                else if (dto is ProjectTransactionsReportDTO projectTransactions)
                    evaluated = this.Evaluate(projectTransactions);
                else if (dto is ProjectStatisticsReportDTO projectStatistics)
                    evaluated = this.Evaluate(projectStatistics);
                else if (dto is ProductListReportDTO productList)
                    evaluated = this.Evaluate(productList);
                else if (dto is PurchaseOrderReportDTO purchaseOrder)
                    evaluated = this.Evaluate(purchaseOrder);
                else if (dto is StockInventoryReportDTO stockInventory)
                    evaluated = this.Evaluate(stockInventory);
            }

            #endregion

            #region Economy

            if (dto is EconomyReportDTO)
            {
                if (dto is BalanceListReportDTO balanceList)
                    evaluated = this.Evaluate(balanceList);
                if (dto is GeneralLedgerReportDTO generalLedger)
                    evaluated = this.Evaluate(generalLedger);
                else if (dto is VoucherListReportDTO voucherList)
                    evaluated = this.Evaluate(voucherList);
                if (dto is CustomerInvoiceIOReportDTO customerInvoice)
                    evaluated = this.Evaluate(customerInvoice);
                if (dto is VoucherIOReportDTO voucherIO)
                    evaluated = this.Evaluate(voucherIO);
            }

            #endregion

            #region Time

            if (dto is TimeReportDTO)
            {
                if (dto is TimeKU10ReportDTO ku10) //must be evaluated before TimeEmployeeReportDTO because it extends TimeEmployeeReportDTO
                    evaluated = this.Evaluate(ku10);
                else if (dto is TimeEmployeeReportDTO employee)
                    evaluated = this.Evaluate(employee);
                else if (dto is TimeEmploymentReportDTO employment)
                    evaluated = this.Evaluate(employment);
                else if (dto is TimeCategoryReportDTO category)
                    evaluated = this.Evaluate(category);
                else if (dto is TimeSalarySpecificationReportDTO timeSalary)
                    evaluated = this.Evaluate(timeSalary);
                else if (dto is TimeSalaryControlInfoReportDTO timeSalaryControl)
                    evaluated = this.Evaluate(timeSalaryControl);
                else if (dto is TimeUsersListReportDTO user)
                    evaluated = this.Evaluate(user);
                else if (dto is TimePayrollSlipReportDTO payrollSlip)
                    evaluated = this.Evaluate(payrollSlip);
                else if (dto is TimeScheduleTasksAndDeliverysReportDTO taskAndDelivery)
                    evaluated = this.Evaluate(taskAndDelivery);
            }

            #endregion

            #region PayrollProduct

            if (dto is PayrollProductReportDTO payrollProduct)
                evaluated = this.Evaluate(payrollProduct);

            #endregion

            #region Common

            this.Email = dto.Email;
            this.EmailTemplateId = dto.EmailTemplateId;
            this.EmailFileName = dto.EmailFileName;
            this.EmailRecipients = dto.EmailRecipients;
            this.SingleRecipients = dto.SingleRecipient;

            #endregion

            return evaluated;
        }

        #region Billing

        private bool Evaluate(BillingReportDTO dto)
        {
            if (dto == null)
                return false;

            this.SB_SortOrder = (int)TermGroup_ReportBillingInvoiceSortOrder.InvoiceNr;
            this.SB_IsEvaluated = true;

            return true;
        }

        private bool Evaluate(BillingInvoiceReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SB_InvoiceIds = dto.InvoiceIds;
            if (dto.InvoiceIds != null && dto.InvoiceIds.Count > 0)
                this.SB_HasInvoiceIds = true;
            this.SB_InvoiceNrFrom = dto.InvoiceNr;
            this.SB_InvoiceNrTo = dto.InvoiceNr;
            if (!String.IsNullOrEmpty(dto.InvoiceNr))
                this.SB_HasInvoiceNrInterval = true;
            this.SB_InvoiceCopy = dto.InvoiceCopy;
            this.SB_InvoiceReminder = dto.InvoiceReminder;
            this.SB_IncludeDrafts = dto.IncludeDrafts;
            this.SB_IncludeProjectReport = dto.IncludeProjectReport;
            this.SB_IncludeOnlyInvoiced = dto.IncludeOnlyInvoiced;
            this.SB_ReportLanguageId = dto.ReportLanguageId;
            this.SB_ShowNotPrinted = true;
            this.SB_ShowCopy = true;
            this.SB_IncludeClosedOrder = true;

            if (dto.DateFrom.HasValue && dto.DateFrom != CalendarUtility.DATETIME_DEFAULT)
            {
                this.HasDateInterval = true;
                this.DateFrom = dto.DateFrom.Value;
            }

            if (dto.DateTo.HasValue && dto.DateTo != CalendarUtility.DATETIME_DEFAULT)
            {
                this.HasDateInterval = true;
                this.DateTo = dto.DateTo.Value;
            }

            return true;
        }

        private bool Evaluate(BillingInvoiceTimeProjectReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingInvoiceReportDTO))
                return false;

            return true;
        }

        private bool Evaluate(HouseholdTaxDeductionReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SB_HTDCompanyId = dto.HTDCompanyId;
            this.SB_HTDSeqNbr = dto.HTDSeqNbr;
            this.SB_HTDCustomerInvoiceRowIds = dto.HTDCustomerInvoiceRowIds;
            this.SB_HTDHasCustomerInvoiceRowIds = true;
            this.SB_HTDShowFile = dto.ShowFile;
            this.SB_HTDUseGreen = dto.UseGreen;
            this.SB_IsEvaluated = true;

            return true;
        }

        private bool Evaluate(OrderChecklistReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SB_InvoiceIds = new List<int> { dto.InvoiceId };
            this.SB_ChecklistHeadRecordId = dto.ChecklistHeadRecordId;
            this.SB_HasInvoiceIds = true;

            return true;
        }

        private bool Evaluate(ProductListReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SB_ProductIds = dto.ProductIds;
            this.SB_HasProductIds = true;

            return true;
        }

        private bool Evaluate(ProjectTransactionsReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SP_ProjectIds = dto.ProjectIds;
            this.SP_OfferNrFrom = dto.OfferNrFrom;
            this.SP_OfferNrTo = dto.OfferNrTo;
            this.SP_OrderNrFrom = dto.OrderNrFrom;
            this.SP_OrderNrTo = dto.OrderNrTo;
            this.SP_InvoiceNrFrom = dto.InvoiceNrFrom;
            this.SP_InvoiceNrTo = dto.InvoiceNrTo;
            this.SP_EmployeeNrFrom = dto.EmployeeNrFrom;
            this.SP_EmployeeNrTo = dto.EmployeeNrTo;
            this.SP_PayrollProductNrFrom = dto.PayrollProductNrFrom;
            this.SP_PayrollProductNrTo = dto.PayrollProductNrTo;
            this.SP_InvoiceProductNrFrom = dto.InvoiceProductNrFrom;
            this.SP_InvoiceProductNrTo = dto.InvoiceProductNrTo;
            this.SP_PayrollTransactionDateFrom = dto.PayrollTransactionDateFrom;
            this.SP_PayrollTransactionDateTo = dto.PayrollTransactionDateTo;
            this.SP_InvoiceTransactionDateFrom = dto.InvoiceTransactionDateFrom;
            this.SP_InvoiceTransactionDateTo = dto.InvoiceTransactionDateTo;
            this.SP_IncludeChildProjects = dto.IncludeChildProjects;
            this.SP_Dim2From = dto.Dim2From;
            this.SP_Dim2To = dto.Dim2To;
            this.SP_Dim3From = dto.Dim3From;
            this.SP_Dim3To = dto.Dim3To;
            this.SP_Dim4From = dto.Dim4From;
            this.SP_Dim4To = dto.Dim4To;
            this.SP_Dim5From = dto.Dim5From;
            this.SP_Dim5To = dto.Dim5To;
            this.SP_Dim6From = dto.Dim6From;
            this.SP_Dim6To = dto.Dim6To;
            this.SP_IsEvaluated = true;
            if (this.SP_InvoiceTransactionDateTo.HasValue)
                this.SP_InvoiceTransactionDateTo = this.SP_InvoiceTransactionDateTo.Value.AddMinutes(60 * 24 - 1);
            if (this.SP_PayrollTransactionDateTo.HasValue)
                this.SP_PayrollTransactionDateTo = this.SP_PayrollTransactionDateTo.Value.AddMinutes(60 * 24 - 1);

            #region Internal accounts

            this.SA_AccountIntervals = new List<AccountIntervalDTO>();

            if (dto.Dim2From != String.Empty || dto.Dim2To != String.Empty)
            {
                AccountIntervalDTO accountInterval = new AccountIntervalDTO()
                {
                    AccountDimId = dto.Dim2Id,
                    AccountNrFrom = dto.Dim2From,
                    AccountNrTo = dto.Dim2To,
                };
                this.SA_HasAccountInterval = true;
                this.SA_AccountIntervals.Add(accountInterval);
            }
            if (dto.Dim3From != String.Empty || dto.Dim3To != String.Empty)
            {
                AccountIntervalDTO accountInterval = new AccountIntervalDTO()
                {
                    AccountDimId = dto.Dim3Id,
                    AccountNrFrom = dto.Dim3From,
                    AccountNrTo = dto.Dim3To,
                };
                this.SA_HasAccountInterval = true;
                this.SA_AccountIntervals.Add(accountInterval);
            }
            if (dto.Dim4From != String.Empty || dto.Dim4To != String.Empty)
            {
                AccountIntervalDTO accountInterval = new AccountIntervalDTO()
                {
                    AccountDimId = dto.Dim4Id,
                    AccountNrFrom = dto.Dim4From,
                    AccountNrTo = dto.Dim4To,
                };
                this.SA_HasAccountInterval = true;
                this.SA_AccountIntervals.Add(accountInterval);
            }
            if (dto.Dim5From != String.Empty || dto.Dim5To != String.Empty)
            {
                AccountIntervalDTO accountInterval = new AccountIntervalDTO()
                {
                    AccountDimId = dto.Dim5Id,
                    AccountNrFrom = dto.Dim5From,
                    AccountNrTo = dto.Dim5To,
                };
                this.SA_HasAccountInterval = true;
                this.SA_AccountIntervals.Add(accountInterval);
            }
            if (dto.Dim6From != String.Empty || dto.Dim6To != String.Empty)
            {
                AccountIntervalDTO accountInterval = new AccountIntervalDTO()
                {
                    AccountDimId = dto.Dim6Id,
                    AccountNrFrom = dto.Dim6From,
                    AccountNrTo = dto.Dim6To,
                };
                this.SA_HasAccountInterval = true;
                this.SA_AccountIntervals.Add(accountInterval);
            }

            #endregion

            return true;
        }

        private bool Evaluate(ProjectStatisticsReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SP_ProjectIds = dto.ProjectIds;
            this.SP_OfferNrFrom = dto.ProjectNrFrom;
            this.SP_OfferNrTo = dto.ProjectNrTo;
            this.SP_OrderNrFrom = dto.EmployeeNrFrom;
            this.SP_OrderNrTo = dto.EmployeeNrTo;
            this.SP_InvoiceNrFrom = dto.CustomerNrFrom;
            this.SP_InvoiceNrTo = dto.CustomerNrTo;
            this.SP_EmployeeNrFrom = dto.EmployeeNrFrom;
            this.SP_EmployeeNrTo = dto.EmployeeNrTo;
            this.SP_PayrollTransactionDateFrom = dto.TransactionDateFrom;
            this.SP_PayrollTransactionDateTo = dto.TransactionDateTo;
            this.SP_InvoiceTransactionDateFrom = dto.TransactionDateFrom;
            this.SP_InvoiceTransactionDateTo = dto.TransactionDateTo;
            this.SP_IncludeChildProjects = dto.IncludeChildProjects;
            this.SP_IsEvaluated = true;
            if (this.SP_InvoiceTransactionDateTo.HasValue)
                this.SP_InvoiceTransactionDateTo = this.SP_InvoiceTransactionDateTo.Value.AddMinutes(60 * 24 - 1);
            if (this.SP_PayrollTransactionDateTo.HasValue)
                this.SP_PayrollTransactionDateTo = this.SP_PayrollTransactionDateTo.Value.AddMinutes(60 * 24 - 1);

            #region Internal accounts

            this.SA_AccountIntervals = new List<AccountIntervalDTO>();

            #endregion

            return true;
        }

        private bool Evaluate(PurchaseOrderReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SB_PurchaseIds = dto.PurchaseIds;
            this.SB_HasPurchaseIds = true;
            this.SB_ReportLanguageId = dto.ReportLanguageId;

            return true;
        }

        private bool Evaluate(StockInventoryReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as BillingReportDTO))
                return false;

            this.SB_StockInventoryId = dto.StockInventoryIds.FirstOrDefault();
            this.SB_ReportLanguageId = dto.ReportLanguageId;

            return true;
        }

        #endregion

        #region Economy

        private bool Evaluate(EconomyReportDTO dto)
        {
            if (dto == null)
                return false;

            return true;
        }

        private bool Evaluate(BalanceListReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as EconomyReportDTO))
                return false;

            this.SL_InvoiceIds = dto.InvoiceIds;
            this.SL_PaymentRowIds = dto.PaymentRowIds;
            this.SL_ShowPreliminaryInvoices = dto.ShowPreliminaryInvoices;
            this.SL_IncludeCashSalesInvoices = dto.IncludeCashSalesInvoices;

            this.SL_HasInvoiceIds = true;
            this.SL_ShowVoucher = false;
            this.SL_SortOrder = (int)SoeReportSortOrder.ActorName;
            this.SL_DateRegard = (int)TermGroup_ReportLedgerDateRegard.InvoiceDate;
            this.SL_InvoiceSelection = (int)TermGroup_ReportLedgerInvoiceSelection.All;
            this.SL_IsEvaluated = true;

            return true;
        }

        private bool Evaluate(GeneralLedgerReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as EconomyReportDTO))
                return false;

            AccountIntervalDTO accountInterval = new AccountIntervalDTO()
            {
                AccountDimId = dto.AccountDimId,
                AccountNrFrom = dto.AccountNr,
                AccountNrTo = dto.AccountNr,
            };
            this.SA_HasAccountInterval = true;
            this.SA_AccountIntervals = new List<AccountIntervalDTO> { accountInterval };
            this.SA_IsEvaluated = true;

            return true;
        }

        private bool Evaluate(VoucherListReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as EconomyReportDTO))
                return false;

            this.SV_IsAccountingOrder = dto.IsAccountingOrder;
            this.SV_VoucherHeadIds = dto.VoucherHeadIds;
            this.SV_HasVoucherHeadIds = true;
            this.SV_IsEvaluated = true;

            return true;
        }

        #endregion

        #region Time

        private bool Evaluate(TimeReportDTO dto)
        {
            if (dto == null)
                return false;

            this.HasDateInterval = true;
            this.DateFrom = dto.StartDate;
            this.DateTo = dto.StopDate;
            this.ST_TimePeriodId = dto.TimePeriodId;
            this.ST_IncludeInactiveEmployees = dto.IncludeInactiveEmployees;
            this.ST_ShowOnlyTotals = dto.ShowOnlyTotals;
            this.ST_IsEvaluated = true;
            return true;
        }

        public bool Evaluate(TimeEmployeeReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeReportDTO))
                return false;

            this.ST_PayrollProductIds = dto.PayrollProductIds;
            this.ST_ShiftTypeIds = dto.ShiftTypeIds;
            this.ST_VacationGroupIds = dto.VacationGroupIds;
            this.ST_EmployeeIds = dto.EmployeeIds;
            this.ST_EmployeePostIds = dto.EmployeePostIds;
            this.ST_ShowAllEmployees = dto.ShowAllEmployees;
            this.ST_IncludePreliminary = dto.IncludePreliminary;
            this.ST_AccumulatorId = dto.TimeAccumulatorId.ToNullable();
            this.ST_TimePeriodIds = dto.TimePeriodIds;
            this.ST_IncludePayrollStartvalues = dto.IncludePayrollStartvalues;

            return true;
        }

        public bool Evaluate(TimeKU10ReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeEmployeeReportDTO))
                return false;

            this.ST_KU10RemovePrevSubmittedData = dto.KU10RemovePrevSubmittedData;
            this.ST_DataStorageId = dto.DataStorageId;

            return true;
        }

        public bool Evaluate(TimePayrollSlipReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeReportDTO))
                return false;

            this.ST_TimePeriodIds = dto.TimePeriodIds;
            this.ST_EmployeeIds = dto.EmployeeIds;
            this.ST_Preliminary = dto.Preliminary;

            return true;
        }

        public bool Evaluate(TimeEmploymentReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeReportDTO))
                return false;

            this.ST_EmployeeIds = dto.EmployeeIds;
            this.ST_EmploymentId = dto.EmploymentId;
            this.ST_ChangesForDate = dto.ChangesForDate;
            this.ST_SubstituteDates = dto.SubstituteDates;
            this.ST_PrintedFromSchedulePlanning = dto.PrintedFromSchedulePlanning;
            return true;
        }

        public bool Evaluate(TimeCategoryReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeReportDTO))
                return false;

            this.ST_CategoryIds = dto.CategoryIds;

            return true;
        }

        public bool Evaluate(TimeUsersListReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeReportDTO))
                return false;

            this.ST_EmployeeXml = dto.Xml;

            return true;
        }

        public bool Evaluate(TimeSalarySpecificationReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeReportDTO))
                return false;

            this.ST_DataStorageId = dto.DataStorageId;

            return true;
        }

        public bool Evaluate(TimeSalaryControlInfoReportDTO dto)
        {
            if (dto == null)
                return false;
            if (!Evaluate(dto as TimeReportDTO))
                return false;

            this.ST_DataStorageId = dto.DataStorageId;

            return true;
        }

        #endregion

        #region PayrollProduct

        public bool Evaluate(PayrollProductReportDTO dto)
        {
            if (dto == null)
                return false;
            this.SS_PayrollProductIds = dto.PayrollProductIds;
            return true;
        }

        #endregion

        #region IO

        public bool Evaluate(CustomerInvoiceIOReportDTO dto)
        {
            if (dto == null)
                return false;

            this.SI_CustomerInvoiceHeadIOIds = dto.CustomerInvoiceHeadIOIds;
            return true;
        }

        public bool Evaluate(VoucherIOReportDTO dto)
        {
            if (dto == null)
                return false;

            this.SI_VoucherHeadIOIds = dto.VoucherHeadIOIds;
            return true;
        }

        #endregion

        #endregion
    }

    #endregion

    #region Selection in code (QS)

    public abstract class ReportSelectionDTO
    {
        #region Constants

        public const string GUID = "guid";
        private const string COMPANY = "c";
        private const string REPORT = "report";
        private const string REPORTTEMPLATETYPE = "templatetype";
        private const string EMAIL = "email";
        private const string EMAILTEMPLATEID = "emailtemplateid";
        private const string EMAILFILENAME = "emailfilename";
        private const string EMAILRECIPIENTS = "emailrecipients";
        private const string EXPORTTYPE = "exporttype";
        private const string EXPORTFILETYPE = "exportfiletype";

        #endregion

        #region Variables

        protected string baseUrl;
        protected int actorCompanyId;
        protected int sysReportTemplateTypeId;       
        public int ReportTemplateTypeId
        {
            get 
            { 
                return sysReportTemplateTypeId; 
            }
        }
        protected int reportId;
        public int ReportId
        {
            get 
            { 
                return reportId; 
            }
        }
        protected int exportTypeId;
        public int ExportType
        {
            get
            {
                return this.exportTypeId;
            }
        }
        protected int exportFileTypeId;
        public int ExportFileType
        {
            get
            {
                return this.exportFileTypeId; //NOSONAR
            }
        }
        protected bool? email;
        public bool? Email
        {
            get
            {
                return this.email;
            }
        }
        protected int? emailTemplateId;
        public int? EmailTemplateId
        {
            get
            {
                return this.emailTemplateId;
            }
        }
        protected string emailFileName;
        public string EmailFileName
        {
            get
            {
                return this.emailFileName;
            }
        }
        protected int? reportLangId;
        public int? ReportLangId
        {
            get
            {
                return this.reportLangId;
            }
        }
        protected List<int> emailRecipients;
        public List<int> EmailRecipients
        {
            get
            {
                return this.emailRecipients;
            }
        }
        protected string reportGuid;
        public string ReportGuid
        {
            get
            {
                return this.reportGuid;
            }
        }
        protected string singleRecipient;
        public string SingleRecipient
        {
            get
            {
                return this.singleRecipient;
            }
        }

        #endregion

        #region Ctor

        protected ReportSelectionDTO(string baseUrl, int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, int exportFileTypeId, bool? email, int? emailTemplateId, string emailFileName, List<int> emailRecipients)
        {
            this.baseUrl = baseUrl;
            this.actorCompanyId = actorCompanyId;
            this.reportId = reportId;
            this.sysReportTemplateTypeId = sysReportTemplateTypeId;
            this.exportTypeId = exportTypeId;
            this.exportFileTypeId = exportFileTypeId;
            this.email = email;
            this.emailTemplateId = emailTemplateId;
            this.emailFileName = emailFileName;
            this.emailRecipients = emailRecipients;
            this.reportGuid = Guid.NewGuid().ToString();
        }

        protected ReportSelectionDTO(string baseUrl, int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, bool? email, int? emailTemplateId, string emailFileName, List<int> emailRecipients, string singleRecipient = "")
        {
            this.baseUrl = baseUrl;
            this.actorCompanyId = actorCompanyId;
            this.reportId = reportId;
            this.sysReportTemplateTypeId = sysReportTemplateTypeId;
            this.exportTypeId = exportTypeId;
            this.email = email;
            this.emailTemplateId = emailTemplateId;
            this.emailFileName = emailFileName;
            this.emailRecipients = emailRecipients;
            this.reportGuid = Guid.NewGuid().ToString();
            this.singleRecipient = singleRecipient;
        }

        protected ReportSelectionDTO(string baseUrl, int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict)
        {
            this.baseUrl = baseUrl;
            this.actorCompanyId = actorCompanyId;
            this.reportId = reportId;
            this.sysReportTemplateTypeId = sysReportTemplateTypeId;
            this.exportTypeId = StringUtility.TryGetIntValue(dict, EXPORTTYPE);
            this.email = StringUtility.TryGetBoolValue(dict, EMAIL);
            this.emailTemplateId = StringUtility.TryGetIntValue(dict, EMAILTEMPLATEID);
            this.emailFileName = StringUtility.TryGetStringValue(dict, EMAILFILENAME);
            this.exportFileTypeId = StringUtility.TryGetIntValue(dict, EXPORTFILETYPE);
            this.emailRecipients = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, EMAILRECIPIENTS));
            this.reportGuid = Guid.NewGuid().ToString();
        }

        #endregion

        #region Protected methods

        protected string GetUrlParameter(string name, List<int> values, bool useInterval = false)
        {
            if (values == null)
                return String.Empty;

            string url = GetUrlParameter(name);
            url += StringUtility.GetCommaSeparatedString(values, useInterval: useInterval);

            return url;
        }

        protected string GetUrlParameter(string name, List<DateTime> values)
        {
            if (values == null)
                return String.Empty;

            string url = GetUrlParameter(name);
            url += StringUtility.GetCommaSeparatedString<DateTime>(values);

            return url;
        }

        protected string GetUrlParameter(string name, int? value)
        {
            string url = "";
            if (value.HasValue)
                url = GetUrlParameter(name, value.Value);
            return url;
        }

        protected string GetUrlParameter(string name, int value, bool firstParam = false)
        {
            return GetUrlParameter(name, value.ToString(), firstParam);
        }

        protected string GetUrlParameter(string name, bool? value)
        {
            string url = "";
            if (value.HasValue)
                url = GetUrlParameter(name, value.Value);
            return url;
        }

        protected string GetUrlParameter(string name, bool value)
        {
            return GetUrlParameter(name, value.ToString());
        }

        protected string GetUrlParameter(string name, DateTime? value)
        {
            string url = "";
            if (value.HasValue)
                url = GetUrlParameter(name, value.Value);
            return url;
        }

        protected string GetUrlParameter(string name, DateTime value)
        {
            return GetUrlParameter(name, value.ToString("yyyyMMdd"));
        }

        protected string GetUrlParameter(string name)
        {
            string delimeter = "&";
            return String.Concat(delimeter, name, "=");
        }

        protected string GetUrlParameter(string name, string value, bool firstParam = false)
        {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            string delimeter = firstParam ? "?" : "&";
            return String.Concat(delimeter, name, "=", value);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public virtual string ToString(bool includeBaseUrl)
        {
            string url = includeBaseUrl ? baseUrl : "";

            url = AddBaseParameters(url);

            return url;
        }

        public virtual string ToShortString(bool includeBaseUrl)
        {
            string url = includeBaseUrl ? baseUrl : "";

            url = AddBaseParameters(url);
            url = AddGuidUrl(url, this.reportGuid);

            return url;
        }

        public string GetBaseUrl()
        {
            return baseUrl;
        }

        #endregion

        #region Private methods

        private string AddBaseParameters(string url)
        {
            url += GetUrlParameter(COMPANY, this.actorCompanyId, true);
            url += GetUrlParameter(REPORT, this.reportId);
            url += GetUrlParameter(REPORTTEMPLATETYPE, this.sysReportTemplateTypeId);
            url += GetUrlParameter(EXPORTTYPE, this.exportTypeId);
            url += GetUrlParameter(EXPORTFILETYPE, this.exportFileTypeId);
            url += GetUrlParameter(EMAIL, this.Email);
            url += GetUrlParameter(EMAILTEMPLATEID, this.EmailTemplateId);
            url += GetUrlParameter(EMAILFILENAME, this.EmailFileName);
            url += GetUrlParameter(EMAILRECIPIENTS);
            url += StringUtility.GetCommaSeparatedString(this.EmailRecipients, useInterval: false);
            return url;
        }

        private string AddGuidUrl(string url, string guid)
        {
            url += GetUrlParameter(GUID, guid);
            return url;
        }

        #endregion
    }

    #region Pre-generated reports

    public abstract class PreGenereatedReportDTO : ReportSelectionDTO
    {
        #region Constants

        private static readonly string BaseUrl = "/ajax/downloadReport.aspx";

        #endregion

        #region Variables

        #endregion

        #region Ctor

        protected PreGenereatedReportDTO(int actorCompanyId, int sysReportTemplateTypeId, int exportTypeId) :
            base(BaseUrl, actorCompanyId, 0, sysReportTemplateTypeId, exportTypeId, null, null, null, null)
        {

        }

        protected PreGenereatedReportDTO(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(BaseUrl, actorCompanyId, 0, sysReportTemplateTypeId, dict)
        {

        }

        #endregion
    }

    public sealed class EdiSupplierInvoiceReportItem : PreGenereatedReportDTO
    {
        #region Constants

        private readonly string EDIENTRYID = "edientryid";

        #endregion

        #region Variables

        public int EdiEntryId { get; set; }

        #endregion

        #region Ctor

        public EdiSupplierInvoiceReportItem(int actorCompanyId, int sysReportTemplateTypeId, int ediEntryId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {
            this.EdiEntryId = ediEntryId;
        }

        public EdiSupplierInvoiceReportItem(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {
            this.EdiEntryId = StringUtility.TryGetIntValue(dict, EDIENTRYID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(EDIENTRYID, this.EdiEntryId);

            return url;
        }

        #endregion
    }

    public sealed class EdiScanningSupplierInvoiceImageDTO : PreGenereatedReportDTO
    {
        #region Constants

        private readonly string SCANNINGENTRYID = "scanningentryid";
        private readonly string EDIENTRYID = "edientryid";

        #endregion

        #region Variables

        public int EdiEntryId { get; set; }
        public int ScanningEntryId { get; set; }

        #endregion

        #region Ctor

        public EdiScanningSupplierInvoiceImageDTO(int actorCompanyId, int sysReportTemplateTypeId, int ediEntryId, int scanningEntryId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {
            this.EdiEntryId = ediEntryId;
            this.ScanningEntryId = scanningEntryId;
        }

        public EdiScanningSupplierInvoiceImageDTO(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {
            this.ScanningEntryId = StringUtility.TryGetIntValue(dict, SCANNINGENTRYID);
            this.EdiEntryId = StringUtility.TryGetIntValue(dict, EDIENTRYID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(SCANNINGENTRYID, this.ScanningEntryId);
            url += GetUrlParameter(EDIENTRYID, this.EdiEntryId);

            return url;
        }

        #endregion
    }

    public sealed class FinvoiceReportDTO : PreGenereatedReportDTO
    {
        #region Constants

        private readonly string INVOICEID = "invoiceid";
        private readonly string INVOICENR = "invoicenr";
        private readonly string ORIGINAL = "original";

        #endregion

        #region Variables

        public int InvoiceId { get; set; }
        public String InvoiceNr { get; set; }
        public bool Original { get; set; }

        #endregion

        #region Ctor

        public FinvoiceReportDTO(int actorCompanyId, int sysReportTemplateTypeId, int invoiceId, string invoiceNr, bool original = true, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {
            this.InvoiceId = invoiceId;
            this.InvoiceNr = invoiceNr;
            this.Original = original;
        }

        public FinvoiceReportDTO(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {
            this.InvoiceId = StringUtility.TryGetIntValue(dict, INVOICEID);
            this.InvoiceNr = StringUtility.TryGetStringValue(dict, INVOICENR);
            this.Original = StringUtility.TryGetBoolValue(dict, ORIGINAL);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(INVOICEID, this.InvoiceId);
            url += GetUrlParameter(INVOICENR, this.InvoiceNr);
            url += GetUrlParameter(ORIGINAL, this.Original);

            return url;
        }

        #endregion
    }

    public sealed class EfhInvoiceReportDTO : PreGenereatedReportDTO
    {
        #region Constants

        private readonly string INVOICEID = "invoiceid";
        private readonly string INVOICENR = "invoicenr";

        #endregion

        #region Variables

        public int InvoiceId { get; set; }
        public String InvoiceNr { get; set; }

        #endregion

        #region Ctor

        public EfhInvoiceReportDTO(int actorCompanyId, int sysReportTemplateTypeId, int invoiceId, string invoiceNr, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {
            this.InvoiceId = invoiceId;
            this.InvoiceNr = invoiceNr;
        }

        public EfhInvoiceReportDTO(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {
            this.InvoiceId = StringUtility.TryGetIntValue(dict, INVOICEID);
            this.InvoiceNr = StringUtility.TryGetStringValue(dict, INVOICENR);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(INVOICEID, this.InvoiceId);
            url += GetUrlParameter(INVOICENR, this.InvoiceNr);

            return url;
        }

        #endregion
    }

    public sealed class SvefakturaReportDTO : PreGenereatedReportDTO
    {
        #region Constants

        private readonly string INVOICEID = "invoiceid";
        private readonly string INVOICENR = "invoicenr";

        #endregion

        #region Variables

        public int InvoiceId { get; set; }
        public String InvoiceNr { get; set; }

        #endregion

        #region Ctor

        public SvefakturaReportDTO(int actorCompanyId, int sysReportTemplateTypeId, int invoiceId, string invoiceNr, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {
            this.InvoiceId = invoiceId;
            this.InvoiceNr = invoiceNr;
        }

        public SvefakturaReportDTO(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {
            this.InvoiceId = StringUtility.TryGetIntValue(dict, INVOICEID);
            this.InvoiceNr = StringUtility.TryGetStringValue(dict, INVOICENR);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(INVOICEID, this.InvoiceId);
            url += GetUrlParameter(INVOICENR, this.InvoiceNr);

            return url;
        }

        #endregion
    }

    public sealed class CsrReportDTO : PreGenereatedReportDTO
    {
        #region Constants

        #endregion

        #region Variables


        #endregion

        #region Ctor

        public CsrReportDTO(int actorCompanyId, int sysReportTemplateTypeId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {

        }

        public CsrReportDTO(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {

        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);


            return url;
        }

        #endregion
    }

    public sealed class TimeSaumaSalarySpecificationReportDTO : PreGenereatedReportDTO
    {
        #region Constants

        private readonly string DATASTORAGEID = "datastorageid";

        #endregion

        #region Variables

        public int DataStorageId { get; set; }

        #endregion

        #region Ctor

        public TimeSaumaSalarySpecificationReportDTO(int actorCompanyId, int sysReportTemplateTypeId, int dataStorageId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {
            this.DataStorageId = dataStorageId;
        }

        public TimeSaumaSalarySpecificationReportDTO(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {
            this.DataStorageId = StringUtility.TryGetIntValue(dict, DATASTORAGEID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(DATASTORAGEID, this.DataStorageId);

            return url;
        }

        #endregion
    }

    public sealed class EmployeeVacationDebtReport : PreGenereatedReportDTO
    {
        #region Constants

        private readonly string DATASTORAGEID = "datastorageid";

        #endregion

        #region Variables

        public int DataStorageId { get; set; }

        #endregion

        #region Ctor

        public EmployeeVacationDebtReport(int actorCompanyId, int sysReportTemplateTypeId, int dataStorageId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown) :
            base(actorCompanyId, sysReportTemplateTypeId, exportTypeId)
        {
            this.DataStorageId = dataStorageId;
        }

        public EmployeeVacationDebtReport(int actorCompanyId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, sysReportTemplateTypeId, dict)
        {
            this.DataStorageId = StringUtility.TryGetIntValue(dict, DATASTORAGEID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(DATASTORAGEID, this.DataStorageId);

            return url;
        }

        #endregion
    }

    #endregion

    #region Billing reports

    public abstract class BillingReportDTO : ReportSelectionDTO
    {
        #region Constants

        public static readonly string WebBaseUrl = "/ajax/printReport.aspx";
        public static readonly string BaseUrl = "/soe/common/distribution/reports/reporturl/";

        #endregion

        #region Ctor

        protected BillingReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, bool? email, int? emailTemplateId, string emailFileName, List<int> emailRecipients, bool webBaseUrl = false, string singleRecipient = "") :
            base((webBaseUrl ? WebBaseUrl : BaseUrl), actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, email, emailTemplateId, emailFileName, emailRecipients, singleRecipient)
        {

        }

        protected BillingReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(BaseUrl, actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {

        }

        #endregion
    }

    public class BillingInvoiceReportDTO : BillingReportDTO
    {
        #region Constants

        private const string INVOICEID = "invoiceid";
        private const string INVOICENR = "invoicenr";
        private const string INVOICECOPY = "invoicecopy";
        private const string REMINDER = "reminder";
        private const string DISABLECOPIES = "disablecopies";
        private const string INCLUDEPROJECT = "incproject";
        private const string INCLUDEONLYINVOICED = "inconlyinvoiced";
        private const string INCLUDEDRAFTS = "incdrafts";
        private const string REPORTLANGUAGEID = "reportlangid";
        private const string DATEFROM = "datefr";
        private const string DATETO = "dateto";

        #endregion

        #region Variables

        public List<int> InvoiceIds { get; set; }
        public string InvoiceNr { get; set; }
        public bool InvoiceCopy { get; set; }
        public bool InvoiceReminder { get; set; }
        public bool DisableInvoiceCopies { get; set; }
        public bool IncludeProjectReport { get; set; }
        public bool IncludeOnlyInvoiced { get; set; }
        public bool IncludeDrafts { get; set; }
        public int ReportLanguageId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }


        #endregion

        #region Ctor

        public BillingInvoiceReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int invoiceId, string invoiceNr, bool invoiceCopy, bool invoiceReminder, bool disableInvoiceCopies, bool includeProjectReport, bool includeOnlyInvoiced, int reportLanguageId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool? email = null, int? emailTemplateId = null, string emailFileName = null, List<int> emailRecipients = null, bool webBaseUrl = false, string singleRecipient = "") :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, email, emailTemplateId, emailFileName, emailRecipients, webBaseUrl, singleRecipient)
        {
            this.InvoiceIds = new List<int>() { invoiceId };
            this.InvoiceNr = invoiceNr;
            this.InvoiceCopy = invoiceCopy;
            this.InvoiceReminder = invoiceReminder;
            this.DisableInvoiceCopies = disableInvoiceCopies;
            this.IncludeProjectReport = includeProjectReport;
            this.IncludeOnlyInvoiced = includeOnlyInvoiced;
            this.IncludeDrafts = true;
            this.ReportLanguageId = reportLanguageId;
        }

        public BillingInvoiceReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> invoiceIds, bool invoiceCopy, bool invoiceReminder, bool disableInvoiceCopies = false, bool includeProjectReport = true, bool includeOnlyInvoiced = true, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool? email = null, int? emailTemplateId = null, string emailFileName = null, List<int> emailRecipients = null, bool webBaseUrl = false, DateTime? dateFrom = null, DateTime? dateTo = null) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, email, emailTemplateId, emailFileName, emailRecipients, webBaseUrl)
        {
            this.InvoiceIds = invoiceIds;
            this.InvoiceCopy = invoiceCopy;
            this.InvoiceReminder = invoiceReminder;
            this.DisableInvoiceCopies = disableInvoiceCopies;
            this.IncludeProjectReport = includeProjectReport;
            this.IncludeOnlyInvoiced = includeOnlyInvoiced;
            this.IncludeDrafts = true;
            this.ReportLanguageId = 0;
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
        }

        public BillingInvoiceReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.InvoiceIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, INVOICEID));
            this.InvoiceNr = StringUtility.TryGetStringValue(dict, INVOICENR);
            this.InvoiceCopy = StringUtility.TryGetBoolValue(dict, INVOICECOPY);
            this.InvoiceReminder = StringUtility.TryGetBoolValue(dict, REMINDER);
            this.DisableInvoiceCopies = StringUtility.TryGetBoolValue(dict, DISABLECOPIES);
            this.IncludeProjectReport = StringUtility.TryGetBoolValue(dict, INCLUDEPROJECT);
            this.IncludeOnlyInvoiced = StringUtility.TryGetBoolValue(dict, INCLUDEONLYINVOICED, true);
            this.IncludeDrafts = StringUtility.TryGetBoolValue(dict, INCLUDEDRAFTS);
            this.ReportLanguageId = StringUtility.TryGetIntValue(dict, REPORTLANGUAGEID);
            this.DateFrom = StringUtility.TryGetDateValue(dict, DATEFROM);
            this.DateTo = StringUtility.TryGetDateValue(dict, DATETO);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToShortString(bool includeBaseUrl)
        {
            string url = base.ToShortString(includeBaseUrl);
            url += GetUrlParameter(REPORTLANGUAGEID, this.ReportLanguageId);
            url += GetUrlParameter(DATEFROM, this.DateFrom);
            url += GetUrlParameter(DATETO, this.DateTo);
            return url;

        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(INVOICEID);
            url += StringUtility.GetCommaSeparatedString(this.InvoiceIds, useInterval: false);
            if (!String.IsNullOrEmpty(this.InvoiceNr))
                url += GetUrlParameter(INVOICENR, this.InvoiceNr); //setting this also makes the selection faster
            url += GetUrlParameter(INVOICECOPY, this.InvoiceCopy);
            url += GetUrlParameter(REMINDER, this.InvoiceReminder);
            url += GetUrlParameter(INCLUDEPROJECT, this.IncludeProjectReport);
            url += GetUrlParameter(INCLUDEONLYINVOICED, this.IncludeOnlyInvoiced);
            url += GetUrlParameter(INCLUDEDRAFTS, this.IncludeDrafts);
            url += GetUrlParameter(REPORTLANGUAGEID, this.ReportLanguageId);
            url += GetUrlParameter(DATEFROM, this.DateFrom);
            url += GetUrlParameter(DATETO, this.DateTo);

            return url;
        }


        #endregion
    }

    public sealed class BillingInvoiceTimeProjectReportDTO : BillingInvoiceReportDTO
    {
        #region Constants

        private const string UPLOAD = "upload";

        #endregion

        #region Variables

        public bool UploadToInexchange { get; private set; }

        #endregion

        #region Ctor

        public BillingInvoiceTimeProjectReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.UploadToInexchange = StringUtility.TryGetBoolValue(dict, UPLOAD);
        }

        public BillingInvoiceTimeProjectReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, BillingInvoiceReportDTO dto, bool webBaseUrl = false, bool uploadToInexchange = false, DateTime? dateFrom = null, DateTime? dateTo = null) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dto.InvoiceIds, dto.InvoiceCopy, dto.InvoiceReminder, dto.DisableInvoiceCopies, dto.IncludeProjectReport, dto.IncludeOnlyInvoiced, dto.ExportType, null, null, null, null, webBaseUrl, dateFrom, dateTo)
        {
            this.UploadToInexchange = uploadToInexchange;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(UPLOAD, this.UploadToInexchange);

            return url;
        }

        #endregion
    }

    public sealed class BillingInvoiceExpenseReportDTO : BillingInvoiceReportDTO
    {
        #region Constants

        private const string UPLOAD = "upload";

        #endregion

        #region Variables

        public bool UploadToInexchange { get; private set; }

        #endregion

        #region Ctor

        public BillingInvoiceExpenseReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.UploadToInexchange = StringUtility.TryGetBoolValue(dict, UPLOAD);
        }

        public BillingInvoiceExpenseReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, BillingInvoiceReportDTO dto, bool webBaseUrl = false, bool uploadToInexchange = false, DateTime? dateFrom = null, DateTime? dateTo = null) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dto.InvoiceIds, dto.InvoiceCopy, dto.InvoiceReminder, dto.DisableInvoiceCopies, dto.IncludeProjectReport, dto.IncludeOnlyInvoiced, dto.ExportType, null, null, null, null, webBaseUrl, dateFrom, dateTo)
        {
            this.UploadToInexchange = uploadToInexchange;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(UPLOAD, this.UploadToInexchange);

            return url;
        }

        #endregion
    }
    public sealed class PurchaseOrderReportDTO : BillingReportDTO
    {
        #region Constants

        private const string PURCHASEID = "purchaseid";
        private const string REPORTLANGUAGEID = "reportlangid";

        #endregion

        #region Variables

        public int PurchaseId { get; set; }
        public int ActorCompanyId { get; set; }
        public List<int> PurchaseIds { get; set; }
        public int ReportLanguageId { get; set; }


        #endregion

        #region Ctor

        public PurchaseOrderReportDTO(int actorCompanyId, List<int> purchaseIds, int reportId, int languageId, int sysReportTemplateTypeId) : 
            base(actorCompanyId, reportId, sysReportTemplateTypeId, (int)TermGroup_ReportExportType.Unknown, null, null, null, null, false, null)
        {
            this.PurchaseIds = purchaseIds;
            this.ReportLanguageId = languageId;
            this.ActorCompanyId = actorCompanyId;
        }

        public PurchaseOrderReportDTO(int actorCompanyId, List<int> purchaseIds, int reportId, int languageId, int sysReportTemplateTypeId, int reportExportType, string fileName, List<int> emailRecipients, int emailTemplateId, string singleRecipient = "") : 
            base(actorCompanyId, reportId, sysReportTemplateTypeId, reportExportType, true, emailTemplateId, fileName, emailRecipients, false, singleRecipient)
        {
            this.PurchaseIds = purchaseIds;
            this.ReportLanguageId = languageId;
            this.ActorCompanyId = actorCompanyId;
        }

        public PurchaseOrderReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
        base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.PurchaseIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, PURCHASEID));
            this.ReportLanguageId = StringUtility.TryGetIntValue(dict, REPORTLANGUAGEID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToShortString(bool includeBaseUrl)
        {
            string url = base.ToShortString(includeBaseUrl);
            url += GetUrlParameter(REPORTLANGUAGEID, this.ReportLanguageId);
            return url;

        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(PURCHASEID);
            url += StringUtility.GetCommaSeparatedString(this.PurchaseIds, useInterval: false);
            url += GetUrlParameter(REPORTLANGUAGEID, this.ReportLanguageId);

            return url;
        }
        #endregion

    }
    public sealed class StockInventoryReportDTO : BillingReportDTO
    {
        #region Constants

        private const string STOCKINVENTORYID = "stockinventoryid";
        private const string REPORTLANGUAGEID = "reportlangid";

        #endregion

        #region Variables

        public int StockInventoryId { get; set; }
        public int ActorCompanyId { get; set; }
        public List<int> StockInventoryIds { get; set; }
        public int ReportLanguageId { get; set; }


        #endregion

        #region Ctor

        public StockInventoryReportDTO(int actorCompanyId, List<int> stockInventoryIds, int reportId, int languageId, int sysReportTemplateTypeId) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, (int)TermGroup_ReportExportType.Unknown, null, null, null, null, false, null)
        {
            this.StockInventoryIds = stockInventoryIds;
            this.ReportLanguageId = languageId;
            this.ActorCompanyId = actorCompanyId;
        }

        public StockInventoryReportDTO(int actorCompanyId, List<int> stockInventoryIds, int reportId, int languageId, int sysReportTemplateTypeId, int reportExportType, string fileName, List<int> emailRecipients, int emailTemplateId, string singleRecipient = "") :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, reportExportType, true, emailTemplateId, fileName, emailRecipients, false, singleRecipient)
        {
            this.StockInventoryIds = stockInventoryIds;
            this.ReportLanguageId = languageId;
            this.ActorCompanyId = actorCompanyId;
        }

        public StockInventoryReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
        base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.StockInventoryIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, STOCKINVENTORYID));
            this.ReportLanguageId = StringUtility.TryGetIntValue(dict, REPORTLANGUAGEID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToShortString(bool includeBaseUrl)
        {
            string url = base.ToShortString(includeBaseUrl);
            url += GetUrlParameter(REPORTLANGUAGEID, this.ReportLanguageId);
            return url;

        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(STOCKINVENTORYID);
            url += StringUtility.GetCommaSeparatedString(this.StockInventoryIds, useInterval: false);
            url += GetUrlParameter(REPORTLANGUAGEID, this.ReportLanguageId);

            return url;
        }
        #endregion

    }

    public sealed class HouseholdTaxDeductionReportDTO : BillingReportDTO
    {
        #region Constants

        private const string HTDCOMPANY = "company";
        private const string HTDSEQNR = "seqnbr";
        private const string INVOICEROWID = "invoicerowid";
        private const string USEGREEN = "usg";

        #endregion

        #region Variables

        public int HTDCompanyId { get; set; }
        public int HTDSeqNbr { get; set; }
        public List<int> HTDCustomerInvoiceRowIds { get; set; }
        public bool ShowFile { get; set; }
        public bool UseGreen { get; set; }

        #endregion

        #region Ctor

        public HouseholdTaxDeductionReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int htdCompanyId, int htdSeqNbr, List<int> customerInvoiceRowIds, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false, bool useGreen = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null, webBaseUrl)
        {
            this.HTDCompanyId = htdCompanyId;
            this.HTDSeqNbr = htdSeqNbr;
            this.HTDCustomerInvoiceRowIds = customerInvoiceRowIds;
            this.ShowFile = false;
            this.UseGreen = useGreen;
        }

        public HouseholdTaxDeductionReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, bool file, Dictionary<string, string> dict, bool useGreen = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.HTDCompanyId = StringUtility.TryGetIntValue(dict, HTDCOMPANY);
            this.HTDSeqNbr = StringUtility.TryGetIntValue(dict, HTDSEQNR);
            this.HTDCustomerInvoiceRowIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, INVOICEROWID));
            this.ShowFile = file;
            this.UseGreen = StringUtility.TryGetBoolValue(dict, USEGREEN);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(HTDCOMPANY, this.HTDCompanyId);
            url += GetUrlParameter(HTDSEQNR, this.HTDSeqNbr);
            url += GetUrlParameter(USEGREEN, this.UseGreen);
            url += GetUrlParameter(INVOICEROWID);
            url += StringUtility.GetCommaSeparatedString(this.HTDCustomerInvoiceRowIds, useInterval: false);

            return url;
        }

        #endregion
    }

    public sealed class OrderChecklistReportDTO : BillingReportDTO
    {
        #region Constants

        private const string INVOICEID = "invoiceid";
        private const string CHECKLISTHEADRECORDID = "checklistheadrecordid";

        #endregion

        #region Variables

        public int InvoiceId { get; set; }
        public int ChecklistHeadRecordId { get; set; }

        #endregion

        #region Ctor

        public OrderChecklistReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int invoiceId, int checklistHeadRecordId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null, webBaseUrl)
        {
            this.InvoiceId = invoiceId;
            this.ChecklistHeadRecordId = checklistHeadRecordId;
        }

        public OrderChecklistReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.InvoiceId = StringUtility.TryGetIntValue(dict, INVOICEID);
            this.ChecklistHeadRecordId = StringUtility.TryGetIntValue(dict, CHECKLISTHEADRECORDID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(INVOICEID, this.InvoiceId);
            url += GetUrlParameter(CHECKLISTHEADRECORDID, this.ChecklistHeadRecordId);

            return url;
        }

        #endregion
    }

    public sealed class ProjectTransactionsReportDTO : BillingReportDTO
    {
        #region Constants

        private const string PROJECTID = "projectid";
        private const string OFFERNRFROM = "offnrfrom";
        private const string OFFERNRTO = "offnrto";
        private const string ORDERNRFROM = "ordnrfrom";
        private const string ORDERNRTO = "ordnrto";
        private const string INVOICENRFROM = "invnrfrom";
        private const string INVOICENRTO = "invnrto";
        private const string EMPLOYEENRFROM = "empnrfrom";
        private const string EMPLOYEENRTO = "empnrto";
        private const string PAYROLLPRODUCTNRFROM = "pprodnrfrom";
        private const string PAYROLLPRODUCTNRTO = "pprodnrto";
        private const string INVOICEPRODUCTNRFROM = "iprodnrfrom";
        private const string INVOICEPRODUCTNRTO = "iprodnrto";
        private const string PAYROLLTRANSACTIONDATEFROM = "ptransdatefrom";
        private const string PAYROLLTRANSACTIONDATETO = "ptransdateto";
        private const string INVOICERANSACTIONDATEFROM = "itransdatefrom";
        private const string INVOICETRANSACTIONDATETO = "itransdateto";
        private const string INCLUDECHILDPROJECTS = "childproj";
        private const string DIM2ID = "d2id";
        private const string DIM2FROM = "d2from";
        private const string DIM2TO = "d2to";
        private const string DIM3ID = "d3id";
        private const string DIM3FROM = "d3from";
        private const string DIM3TO = "d3to";
        private const string DIM4ID = "d4id";
        private const string DIM4FROM = "d4from";
        private const string DIM4TO = "d4to";
        private const string DIM5ID = "d5id";
        private const string DIM5FROM = "d5from";
        private const string DIM5TO = "d5to";
        private const string DIM6ID = "d6id";
        private const string DIM6FROM = "d6from";
        private const string DIM6TO = "d6to";

        #endregion

        #region Variables

        public List<int> ProjectIds { get; set; }
        public string OfferNrFrom { get; set; }
        public string OfferNrTo { get; set; }
        public string OrderNrFrom { get; set; }
        public string OrderNrTo { get; set; }
        public string InvoiceNrFrom { get; set; }
        public string InvoiceNrTo { get; set; }
        public string EmployeeNrFrom { get; set; }
        public string EmployeeNrTo { get; set; }
        public string PayrollProductNrFrom { get; set; }
        public string PayrollProductNrTo { get; set; }
        public string InvoiceProductNrFrom { get; set; }
        public string InvoiceProductNrTo { get; set; }
        public int Dim2Id { get; set; }
        public string Dim2From { get; set; }
        public string Dim2To { get; set; }
        public int Dim3Id { get; set; }
        public string Dim3From { get; set; }
        public string Dim3To { get; set; }
        public int Dim4Id { get; set; }
        public string Dim4From { get; set; }
        public string Dim4To { get; set; }
        public int Dim5Id { get; set; }
        public string Dim5From { get; set; }
        public string Dim5To { get; set; }
        public int Dim6Id { get; set; }
        public string Dim6From { get; set; }
        public string Dim6To { get; set; }
        public DateTime? PayrollTransactionDateFrom { get; set; }
        public DateTime? PayrollTransactionDateTo { get; set; }
        public DateTime? InvoiceTransactionDateFrom { get; set; }
        public DateTime? InvoiceTransactionDateTo { get; set; }
        public bool IncludeChildProjects { get; set; }

        #endregion

        #region Ctor

        public ProjectTransactionsReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId,
                                            int projectId, DateTime? dateFrom, DateTime? dateTo, bool inclChildProjects,
                                            int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null, webBaseUrl)
        {
            this.ProjectIds = new List<int> { projectId };
            this.InvoiceTransactionDateFrom = dateFrom;
            this.PayrollTransactionDateFrom = dateFrom;
            this.InvoiceTransactionDateTo = dateTo;
            this.PayrollTransactionDateTo = dateTo;
            this.IncludeChildProjects = inclChildProjects;
        }

        public ProjectTransactionsReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> projectIds,
                                            string offerNrFrom, string offerNrTo, string orderNrFrom, string orderNrTo, string invoiceNrFrom, string invoiceNrTo, string employeeNrFrom, string employeeNrTo,
                                            string payrollProductNrFrom, string payrollProductNrTo, string invoiceProductNrFrom, string invoiceProductNrTo,
                                            DateTime? payrollTransactionDateFrom, DateTime? payrollTransactionDateTo, DateTime? invoiceTransactionDateFrom, DateTime? invoiceTransactionDateTo, bool inclChildProjects,
                                            int dim2id, string dim2from, string dim2to, int dim3id, string dim3from, string dim3to, int dim4id, string dim4from, string dim4to, int dim5id, string dim5from, string dim5to,
                                            int dim6id, string dim6from, string dim6to, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null, webBaseUrl)
        {
            this.ProjectIds = projectIds;
            this.OfferNrFrom = offerNrFrom;
            this.OfferNrTo = offerNrTo;
            this.OrderNrFrom = orderNrFrom;
            this.OrderNrTo = orderNrTo;
            this.InvoiceNrFrom = invoiceNrFrom;
            this.InvoiceNrTo = invoiceNrTo;
            this.EmployeeNrFrom = employeeNrFrom;
            this.EmployeeNrTo = employeeNrTo;
            this.PayrollProductNrFrom = payrollProductNrFrom;
            this.PayrollProductNrTo = payrollProductNrTo;
            this.InvoiceProductNrFrom = invoiceProductNrFrom;
            this.InvoiceProductNrTo = invoiceProductNrTo;
            this.PayrollTransactionDateFrom = payrollTransactionDateFrom;
            this.PayrollTransactionDateTo = payrollTransactionDateTo;
            this.InvoiceTransactionDateFrom = invoiceTransactionDateFrom;
            this.InvoiceTransactionDateTo = invoiceTransactionDateTo;
            this.IncludeChildProjects = inclChildProjects;
            this.Dim2Id = dim2id;
            this.Dim2From = dim2from;
            this.Dim2To = dim2to;
            this.Dim3Id = dim3id;
            this.Dim3From = dim3from;
            this.Dim3To = dim3to;
            this.Dim4Id = dim4id;
            this.Dim4From = dim4from;
            this.Dim4To = dim4to;
            this.Dim5Id = dim5id;
            this.Dim5From = dim5from;
            this.Dim5To = dim5to;
            this.Dim6Id = dim6id;
            this.Dim6From = dim6from;
            this.Dim6To = dim6to;
        }

        public ProjectTransactionsReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.ProjectIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, PROJECTID));
            this.OfferNrTo = StringUtility.TryGetStringValue(dict, OFFERNRFROM);
            this.OfferNrTo = StringUtility.TryGetStringValue(dict, OFFERNRTO);
            this.OrderNrFrom = StringUtility.TryGetStringValue(dict, ORDERNRFROM);
            this.OrderNrTo = StringUtility.TryGetStringValue(dict, ORDERNRTO);
            this.InvoiceNrFrom = StringUtility.TryGetStringValue(dict, INVOICENRFROM);
            this.InvoiceNrTo = StringUtility.TryGetStringValue(dict, INVOICENRTO);
            this.EmployeeNrFrom = StringUtility.TryGetStringValue(dict, EMPLOYEENRFROM);
            this.EmployeeNrTo = StringUtility.TryGetStringValue(dict, EMPLOYEENRTO);
            this.PayrollProductNrFrom = StringUtility.TryGetStringValue(dict, PAYROLLPRODUCTNRFROM);
            this.PayrollProductNrTo = StringUtility.TryGetStringValue(dict, PAYROLLPRODUCTNRTO);
            this.InvoiceProductNrFrom = StringUtility.TryGetStringValue(dict, INVOICEPRODUCTNRFROM);
            this.InvoiceProductNrTo = StringUtility.TryGetStringValue(dict, INVOICEPRODUCTNRTO);
            this.PayrollTransactionDateFrom = CalendarUtility.GetNullableDateTime(StringUtility.TryGetStringValue(dict, PAYROLLTRANSACTIONDATEFROM), "yyyyMMdd");
            this.PayrollTransactionDateTo = CalendarUtility.GetNullableDateTime(StringUtility.TryGetStringValue(dict, PAYROLLTRANSACTIONDATETO), "yyyyMMdd");
            this.InvoiceTransactionDateFrom = CalendarUtility.GetNullableDateTime(StringUtility.TryGetStringValue(dict, INVOICERANSACTIONDATEFROM), "yyyyMMdd");
            this.InvoiceTransactionDateTo = CalendarUtility.GetNullableDateTime(StringUtility.TryGetStringValue(dict, INVOICETRANSACTIONDATETO), "yyyyMMdd");
            this.IncludeChildProjects = StringUtility.TryGetBoolValue(dict, INCLUDECHILDPROJECTS);
            this.Dim2Id = StringUtility.TryGetIntValue(dict, DIM2ID);
            this.Dim2From = StringUtility.TryGetStringValue(dict, DIM2FROM);
            this.Dim2To = StringUtility.TryGetStringValue(dict, DIM2TO);
            this.Dim3Id = StringUtility.TryGetIntValue(dict, DIM3ID);
            this.Dim3From = StringUtility.TryGetStringValue(dict, DIM3FROM);
            this.Dim3To = StringUtility.TryGetStringValue(dict, DIM3TO);
            this.Dim4Id = StringUtility.TryGetIntValue(dict, DIM4ID);
            this.Dim4From = StringUtility.TryGetStringValue(dict, DIM4FROM);
            this.Dim4To = StringUtility.TryGetStringValue(dict, DIM4TO);
            this.Dim5Id = StringUtility.TryGetIntValue(dict, DIM5ID);
            this.Dim5From = StringUtility.TryGetStringValue(dict, DIM5FROM);
            this.Dim5To = StringUtility.TryGetStringValue(dict, DIM5TO);
            this.Dim6Id = StringUtility.TryGetIntValue(dict, DIM6ID);
            this.Dim6From = StringUtility.TryGetStringValue(dict, DIM6FROM);
            this.Dim6To = StringUtility.TryGetStringValue(dict, DIM6TO);
        }

        #endregion

        #region Public methods

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(PROJECTID, this.ProjectIds);
            url += GetUrlParameter(OFFERNRFROM, this.OfferNrFrom);
            url += GetUrlParameter(OFFERNRTO, this.OfferNrTo);
            url += GetUrlParameter(ORDERNRFROM, this.OrderNrFrom);
            url += GetUrlParameter(ORDERNRTO, this.OrderNrTo);
            url += GetUrlParameter(INVOICENRFROM, this.InvoiceNrFrom);
            url += GetUrlParameter(INVOICENRTO, this.InvoiceNrTo);
            url += GetUrlParameter(EMPLOYEENRFROM, this.EmployeeNrFrom);
            url += GetUrlParameter(EMPLOYEENRTO, this.EmployeeNrTo);
            url += GetUrlParameter(PAYROLLPRODUCTNRFROM, this.PayrollProductNrFrom);
            url += GetUrlParameter(PAYROLLPRODUCTNRTO, this.PayrollProductNrTo);
            url += GetUrlParameter(INVOICEPRODUCTNRFROM, this.InvoiceProductNrFrom);
            url += GetUrlParameter(INVOICEPRODUCTNRTO, this.InvoiceProductNrTo);
            url += GetUrlParameter(PAYROLLTRANSACTIONDATEFROM, this.PayrollTransactionDateFrom);
            url += GetUrlParameter(PAYROLLTRANSACTIONDATETO, this.PayrollTransactionDateTo);
            url += GetUrlParameter(INVOICERANSACTIONDATEFROM, this.InvoiceTransactionDateFrom);
            url += GetUrlParameter(INVOICETRANSACTIONDATETO, this.InvoiceTransactionDateTo);
            url += GetUrlParameter(INCLUDECHILDPROJECTS, this.IncludeChildProjects);
            url += GetUrlParameter(DIM2ID, this.Dim2Id);
            url += GetUrlParameter(DIM2FROM, this.Dim2From);
            url += GetUrlParameter(DIM2TO, this.Dim2To);
            url += GetUrlParameter(DIM3ID, this.Dim3Id);
            url += GetUrlParameter(DIM3FROM, this.Dim3From);
            url += GetUrlParameter(DIM3TO, this.Dim3To);
            url += GetUrlParameter(DIM4ID, this.Dim4Id);
            url += GetUrlParameter(DIM4FROM, this.Dim4From);
            url += GetUrlParameter(DIM4TO, this.Dim4To);
            url += GetUrlParameter(DIM5ID, this.Dim5Id);
            url += GetUrlParameter(DIM5FROM, this.Dim5From);
            url += GetUrlParameter(DIM5TO, this.Dim5To);
            url += GetUrlParameter(DIM6ID, this.Dim6Id);
            url += GetUrlParameter(DIM6FROM, this.Dim6From);
            url += GetUrlParameter(DIM6TO, this.Dim6To);

            return url;
        }

        #endregion
    }

    public sealed class ProjectStatisticsReportDTO : BillingReportDTO
    {
        #region Constants

        private const string PROJECTID = "projectid";
        private const string PROJECTNRFROM = "projnrfrom";
        private const string PROJECTNRTO = "projnrto";
        private const string CUSTOMERNRFROM = "custnrfrom";
        private const string CUSTOMERNRTO = "custnrto";
        private const string EMPLOYEENRFROM = "empnrfrom";
        private const string EMPLOYEENRTO = "empnrto";
        private const string TRANSACTIONDATEFROM = "transdatefrom";
        private const string TRANSACTIONDATETO = "transdateto";
        private const string INCLUDECHILDPROJECTS = "childproj";

        #endregion

        #region Variables

        public List<int> ProjectIds { get; set; }
        public string ProjectNrFrom { get; set; }
        public string ProjectNrTo { get; set; }
        public string CustomerNrFrom { get; set; }
        public string CustomerNrTo { get; set; }
        public string EmployeeNrFrom { get; set; }
        public string EmployeeNrTo { get; set; }
        public DateTime? TransactionDateFrom { get; set; }
        public DateTime? TransactionDateTo { get; set; }
        public bool IncludeChildProjects { get; set; }

        #endregion

        #region Ctor

        public ProjectStatisticsReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId,
                                            int projectId, DateTime? transactionDateFrom, DateTime? transactionDateTo, bool inclChildProjects,
                                            int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null, webBaseUrl)
        {
            this.ProjectIds = new List<int> { projectId };
            this.ProjectNrFrom = String.Empty;
            this.ProjectNrTo = String.Empty;
            this.CustomerNrFrom = String.Empty;
            this.CustomerNrTo = String.Empty;
            this.EmployeeNrFrom = String.Empty;
            this.EmployeeNrTo = String.Empty;
            this.TransactionDateFrom = transactionDateFrom;
            this.TransactionDateTo = transactionDateTo;
            this.IncludeChildProjects = inclChildProjects;
        }

        public ProjectStatisticsReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId,
                                            string projectNrFrom, string projectNrTo, string customerNrFrom, string customerNrTo, string employeeNrFrom, string employeeNrTo,
                                            DateTime? transactionDateFrom, DateTime? transactionDateTo, bool inclChildProjects,
                                            int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null, webBaseUrl)
        {
            this.ProjectIds = null;
            this.ProjectNrFrom = projectNrFrom;
            this.ProjectNrTo = projectNrTo;
            this.CustomerNrFrom = customerNrFrom;
            this.CustomerNrTo = customerNrTo;
            this.EmployeeNrFrom = employeeNrFrom;
            this.EmployeeNrTo = employeeNrTo;
            this.TransactionDateFrom = transactionDateFrom;
            this.TransactionDateTo = transactionDateTo;
            this.IncludeChildProjects = inclChildProjects;
        }

        public ProjectStatisticsReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.ProjectIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, PROJECTID));
            this.ProjectNrFrom = StringUtility.TryGetStringValue(dict, PROJECTNRFROM);
            this.ProjectNrTo = StringUtility.TryGetStringValue(dict, PROJECTNRTO);
            this.CustomerNrTo = StringUtility.TryGetStringValue(dict, CUSTOMERNRFROM);
            this.CustomerNrTo = StringUtility.TryGetStringValue(dict, CUSTOMERNRTO);
            this.EmployeeNrFrom = StringUtility.TryGetStringValue(dict, EMPLOYEENRFROM);
            this.EmployeeNrTo = StringUtility.TryGetStringValue(dict, EMPLOYEENRTO);
            this.TransactionDateFrom = CalendarUtility.GetNullableDateTime(StringUtility.TryGetStringValue(dict, TRANSACTIONDATEFROM), "yyyyMMdd");
            this.TransactionDateTo = CalendarUtility.GetNullableDateTime(StringUtility.TryGetStringValue(dict, TRANSACTIONDATETO), "yyyyMMdd");
            this.IncludeChildProjects = StringUtility.TryGetBoolValue(dict, INCLUDECHILDPROJECTS);
        }

        #endregion

        #region Public methods

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(PROJECTID, this.ProjectIds);
            url += GetUrlParameter(PROJECTNRFROM, this.ProjectNrFrom);
            url += GetUrlParameter(PROJECTNRTO, this.ProjectNrTo);
            url += GetUrlParameter(CUSTOMERNRFROM, this.CustomerNrFrom);
            url += GetUrlParameter(CUSTOMERNRTO, this.CustomerNrTo);
            url += GetUrlParameter(EMPLOYEENRFROM, this.EmployeeNrFrom);
            url += GetUrlParameter(EMPLOYEENRTO, this.EmployeeNrTo);
            url += GetUrlParameter(TRANSACTIONDATEFROM, this.TransactionDateFrom);
            url += GetUrlParameter(TRANSACTIONDATETO, this.TransactionDateTo);
            url += GetUrlParameter(INCLUDECHILDPROJECTS, this.IncludeChildProjects);

            return url;
        }

        #endregion
    }

    #endregion

    #region Economy reports

    public abstract class EconomyReportDTO : ReportSelectionDTO
    {
        #region Variables

        public static readonly string WebBaseUrl = "/ajax/printReport.aspx";
        public static readonly string BaseUrl = "/soe/common/distribution/reports/reporturl/";

        #endregion

        #region Ctor

        protected EconomyReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, bool webBaseUrl = false) :
            base((webBaseUrl ? WebBaseUrl : BaseUrl), actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null)
        {

        }

        protected EconomyReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(BaseUrl, actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {

        }

        #endregion

        #region Public methods

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            return url;
        }

        #endregion
    }

    public sealed class BalanceListReportDTO : EconomyReportDTO
    {
        #region Constants

        public static readonly string INVOICEID = "invoiceid";
        public static readonly string PAYMENTROWID = "prid";

        #endregion

        #region Variables

        public bool ShowPreliminaryInvoices { get; set; }
        public bool IncludeCashSalesInvoices { get; set; }
        
        public List<int> InvoiceIds { get; set; }
        public List<int> PaymentRowIds { get; set; }

        #endregion

        #region Ctor

        public BalanceListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> invoiceIds, List<int> paymentRowIds, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false, bool showPreliminaryInvoices = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, webBaseUrl)
        {
            this.InvoiceIds = invoiceIds;
            this.PaymentRowIds = paymentRowIds;
            this.ShowPreliminaryInvoices = showPreliminaryInvoices;
        }

        public BalanceListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict, bool showPreliminaryInvoices = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.InvoiceIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, INVOICEID));
            this.PaymentRowIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, PAYMENTROWID));
            this.ShowPreliminaryInvoices = showPreliminaryInvoices;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(INVOICEID);
            url += StringUtility.GetCommaSeparatedString(this.InvoiceIds, useInterval: false);
            if (this.PaymentRowIds.Count > 0)
            {
                url += GetUrlParameter(PAYMENTROWID);
                url += StringUtility.GetCommaSeparatedString(this.PaymentRowIds, useInterval: false);
            }

            return url;
        }

        #endregion
    }

    public sealed class GeneralLedgerReportDTO : EconomyReportDTO
    {
        #region Constants

        public static readonly string ACCOUNTID = "accountid";
        public static readonly string ACCOUNTNR = "accountnr";
        public static readonly string ACCOUNTDIM = "dim";

        #endregion

        #region Variables

        public int AccountId { get; set; }
        public string AccountNr { get; set; }
        public int AccountDimId { get; set; }

        #endregion

        #region Ctor

        public GeneralLedgerReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int accountId, string accountNr, int accountDimId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, webBaseUrl)
        {
            this.AccountId = accountId;
            this.AccountNr = accountNr;
            this.AccountDimId = accountDimId;
        }

        public GeneralLedgerReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.AccountId = StringUtility.TryGetIntValue(dict, ACCOUNTID);
            this.AccountNr = StringUtility.TryGetStringValue(dict, ACCOUNTNR);
            this.AccountDimId = StringUtility.TryGetIntValue(dict, ACCOUNTDIM);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(ACCOUNTID, this.AccountId);
            url += GetUrlParameter(ACCOUNTNR, this.AccountNr);
            url += GetUrlParameter(ACCOUNTDIM, this.AccountDimId);

            return url;
        }

        #endregion
    }

    public sealed class VoucherListReportDTO : EconomyReportDTO
    {
        #region Constants

        private const string VOUCHERHEADID = "voucherheadid";
        private const string ISACCOUNTINGORDER = "isaccountingorder";

        #endregion

        #region Variables

        public List<int> VoucherHeadIds { get; set; }
        public bool IsAccountingOrder { get; set; }

        #endregion

        #region Ctor

        public VoucherListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int voucherHeadId, bool isAccountingOrder, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, webBaseUrl)
        {
            this.VoucherHeadIds = new List<int>() { voucherHeadId };
            this.IsAccountingOrder = isAccountingOrder;
        }

        public VoucherListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> voucherHeadIds, bool isAccountingOrder, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, webBaseUrl)
        {
            this.VoucherHeadIds = voucherHeadIds;
            this.IsAccountingOrder = isAccountingOrder;
        }

        public VoucherListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.VoucherHeadIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, VOUCHERHEADID));
            this.IsAccountingOrder = StringUtility.TryGetBoolValue(dict, ISACCOUNTINGORDER);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(VOUCHERHEADID);
            url += StringUtility.GetCommaSeparatedString(this.VoucherHeadIds, useInterval: false);
            url += GetUrlParameter(ISACCOUNTINGORDER, this.IsAccountingOrder);

            return url;
        }

        public string ToStringSort(bool includeBaseUrl, bool doNotSort)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(VOUCHERHEADID);
            url += StringUtility.GetCommaSeparatedString(this.VoucherHeadIds, useInterval: false, doNotSort: doNotSort);
            url += GetUrlParameter(ISACCOUNTINGORDER, this.IsAccountingOrder);

            return url;
        }
        #endregion
    }

    public sealed class CustomerInvoiceIOReportDTO : EconomyReportDTO
    {
        #region Constants

        private const string CUSOMTERINVOICEHEADIOID = "customerInvoiceHeadIOId";

        #endregion

        #region Variables

        public List<int> CustomerInvoiceHeadIOIds { get; set; }

        #endregion

        #region Ctor

        public CustomerInvoiceIOReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> customerInvoiceHeadIOIds, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, webBaseUrl)
        {
            this.CustomerInvoiceHeadIOIds = customerInvoiceHeadIOIds;
        }

        public CustomerInvoiceIOReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.CustomerInvoiceHeadIOIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, CUSOMTERINVOICEHEADIOID));
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(CUSOMTERINVOICEHEADIOID);
            url += StringUtility.GetCommaSeparatedString(this.CustomerInvoiceHeadIOIds, useInterval: false);

            return url;
        }

        #endregion
    }

    public sealed class ProductListReportDTO : BillingReportDTO
    {
        #region Constants

        private const string PRODUCTID = "productId";

        #endregion

        #region Variables

        public List<int> ProductIds { get; set; }

        #endregion

        #region Ctor        

        public ProductListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> productIds, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null, webBaseUrl)
        {
            this.ProductIds = productIds;
        }

        public ProductListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.ProductIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, PRODUCTID));
        }

        #endregion

        #region Public methods

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(PRODUCTID, this.ProductIds);

            return url;
        }

        #endregion
    }

    public sealed class VoucherIOReportDTO : EconomyReportDTO
    {
        #region Constants

        private const string VOUCHERHEADOIID = "voucherHeadIOId";

        #endregion

        #region Variables

        public List<int> VoucherHeadIOIds { get; set; }

        #endregion

        #region Ctor

        public VoucherIOReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> voucherHeadIOIds, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, webBaseUrl)
        {
            this.VoucherHeadIOIds = voucherHeadIOIds;
        }

        public VoucherIOReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.VoucherHeadIOIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, VOUCHERHEADOIID));
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(VOUCHERHEADOIID);
            url += StringUtility.GetCommaSeparatedString(this.VoucherHeadIOIds, useInterval: false);

            return url;
        }

        #endregion
    }

    public sealed class SEPAPaymentImportReportDTO : EconomyReportDTO
    {
        #region Constants

        private const string DATASTORAGEID = "datastorageid";

        #endregion

        #region Variables

        public List<int> DataStorageIds { get; set; }

        #endregion

        #region Ctor

        public SEPAPaymentImportReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, List<int> dataStorageIds, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, webBaseUrl)
        {
            this.DataStorageIds = dataStorageIds;
        }

        public SEPAPaymentImportReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.DataStorageIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, DATASTORAGEID));
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(DATASTORAGEID);
            url += StringUtility.GetCommaSeparatedString(this.DataStorageIds, useInterval: false);

            return url;
        }

        #endregion
    }

    #endregion

    #region Time reports

    public abstract class TimeReportDTO : ReportSelectionDTO
    {
        #region Constants

        public static readonly string WebBaseUrl = "/ajax/printReport.aspx";
        public static readonly string BaseUrl = "/soe/common/distribution/reports/reporturl/";
        private const string STARTDATE = "start";
        private const string STOPDATE = "stop";
        private const string TIMEPERIODID = "tpid";
        private const string INCLUDEINACTIVEEMPLOYEES = "inac";
        private const string SHOWONLYTOTALS = "onlytotals";

        #endregion

        #region Variables

        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int TimePeriodId { get; set; }
        public bool IncludeInactiveEmployees { get; set; }
        public bool ShowOnlyTotals { get; set; }

        #endregion

        #region Ctor

        protected TimeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, bool includeInactiveEmployees, bool showOnlyTotals, int exportTypeId, int exportFileTypeId, bool webBaseUrl = false) :
            base((webBaseUrl ? WebBaseUrl : BaseUrl), actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, exportFileTypeId, null, null, null, null)
        {
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.TimePeriodId = timePeriodId;
            this.IncludeInactiveEmployees = includeInactiveEmployees;
            this.ShowOnlyTotals = showOnlyTotals;
            this.exportFileTypeId = exportFileTypeId;
        }

        protected TimeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, bool includeInactiveEmployees, bool showOnlyTotals, int exportTypeId, bool webBaseUrl = false) :
            base((webBaseUrl ? WebBaseUrl : BaseUrl), actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null)
        {
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.TimePeriodId = timePeriodId;
            this.IncludeInactiveEmployees = includeInactiveEmployees;
            this.ShowOnlyTotals = showOnlyTotals;
        }

        protected TimeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(BaseUrl, actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.StartDate = CalendarUtility.GetDateTime(StringUtility.TryGetStringValue(dict, STARTDATE), "yyyyMMdd");
            this.StopDate = CalendarUtility.GetDateTime(StringUtility.TryGetStringValue(dict, STOPDATE), "yyyyMMdd");
            this.TimePeriodId = StringUtility.TryGetIntValue(dict, TIMEPERIODID);
            this.IncludeInactiveEmployees = StringUtility.TryGetBoolValue(dict, INCLUDEINACTIVEEMPLOYEES);
            this.ShowOnlyTotals = StringUtility.TryGetBoolValue(dict, SHOWONLYTOTALS);
        }

        #endregion

        #region Public methods

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(STARTDATE, this.StartDate);
            url += GetUrlParameter(STOPDATE, this.StopDate);
            url += GetUrlParameter(TIMEPERIODID, this.TimePeriodId);
            url += GetUrlParameter(INCLUDEINACTIVEEMPLOYEES, this.IncludeInactiveEmployees);
            url += GetUrlParameter(SHOWONLYTOTALS, this.ShowOnlyTotals);

            return url;
        }

        #endregion
    }

    public class TimeEmployeeReportDTO : TimeReportDTO
    {
        #region Constants

        private const string SHIFTTYPE = "shift";
        private const string EMPLOYEE = "emp";
        private const string EMPLOYEEPOST = "empp";
        private const string VACATIONGROUP = "vacgrp";
        private const string PAYROLLPRODUCT = "prod";
        private const string TIMEACCUMULATORID = "acc";
        private const string INCLUDEPRELIMINARY = "prel";
        private const string TIMEPERIOD = "per";
        private const string INCLUDEPAYROLLSTARTVALUE = "startv";

        #endregion

        #region Variables

        public List<int> ShiftTypeIds { get; set; }
        public List<int> VacationGroupIds { get; set; }
        public List<int> PayrollProductIds { get; set; }
        public List<int> EmployeeIds { get; set; }

        public List<int> EmployeePostIds { get; set; }
        public bool ShowAllEmployees { get; set; }
        public bool IncludePreliminary { get; set; }
        public int? TimeAccumulatorId { get; set; }
        public List<int> TimePeriodIds { get; set; }
        public bool IncludePayrollStartvalues { get; set; }

        #endregion

        #region Ctor

        public TimeEmployeeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int employeeId, int timePeriodId = 0, bool includeInactiveEmployees = false, bool showOnlyTotals = false, bool includePayrollStartvalues = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, includeInactiveEmployees, showOnlyTotals, exportTypeId, exportFileTypeId, webBaseUrl)
        {
            this.EmployeeIds = new List<int>() { employeeId };
        }

        public TimeEmployeeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, List<int> shiftTypeIds, List<int> employeeIds, List<int> payrollProductIds, List<int> timePeriodIds, int? accumulatorId, bool includePreliminary, bool includeInactiveEmployees = false, bool showOnlyTotals = false, bool includePayrollStartvalues = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false, List<int> employeePostIds = null) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, includeInactiveEmployees, showOnlyTotals, exportTypeId, exportFileTypeId, webBaseUrl)
        {
            this.ShiftTypeIds = shiftTypeIds;
            this.EmployeeIds = employeeIds;
            this.EmployeePostIds = employeePostIds;
            this.PayrollProductIds = payrollProductIds;
            this.TimeAccumulatorId = accumulatorId;
            this.TimePeriodIds = timePeriodIds;
            this.IncludePayrollStartvalues = includePayrollStartvalues;
        }
        public TimeEmployeeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, List<int> vacationGroupIds = null, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, 0, false, false, exportTypeId, exportFileTypeId, webBaseUrl)
        {
            this.VacationGroupIds = vacationGroupIds;
        }

        public TimeEmployeeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.ShiftTypeIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, SHIFTTYPE));
            this.EmployeeIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, EMPLOYEE));
            this.EmployeePostIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, EMPLOYEEPOST));
            this.VacationGroupIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, VACATIONGROUP));
            this.PayrollProductIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, PAYROLLPRODUCT));
            this.TimeAccumulatorId = StringUtility.TryGetIntValue(dict, TIMEACCUMULATORID);
            this.TimePeriodIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, TIMEPERIOD));
            this.IncludePayrollStartvalues = StringUtility.TryGetBoolValue(dict, INCLUDEPAYROLLSTARTVALUE);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(SHIFTTYPE, this.ShiftTypeIds);
            url += GetUrlParameter(EMPLOYEE, this.EmployeeIds, useInterval: true);
            url += GetUrlParameter(EMPLOYEEPOST, this.EmployeePostIds, useInterval: true);
            url += GetUrlParameter(PAYROLLPRODUCT, this.PayrollProductIds, useInterval: true);
            url += GetUrlParameter(TIMEACCUMULATORID, this.TimeAccumulatorId);
            url += GetUrlParameter(INCLUDEPRELIMINARY, this.IncludePreliminary.ToString());
            url += GetUrlParameter(TIMEPERIOD, this.TimePeriodIds);
            url += GetUrlParameter(INCLUDEPAYROLLSTARTVALUE, this.IncludePayrollStartvalues.ToString());

            return url;
        }

        #endregion
    }

    public class TimeKU10ReportDTO : TimeEmployeeReportDTO
    {
        #region Constants

        private const string KU10REMOVEPREVSUBMITTEDDATA = "KU10RPSD";
        private const string DATASTORAGEID = "dsid";

        #endregion

        #region Variables

        public bool KU10RemovePrevSubmittedData { get; set; }
        public int DataStorageId { get; set; }

        #endregion

        #region Ctor

        public TimeKU10ReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, List<int> shiftTypeIds, List<int> employeeIds, List<int> payrollProductIds, List<int> timePeriodIds, int? accumulatorId, bool includePreliminary, bool ku10RemovePrevSubmittedData, bool includeInactiveEmployees = false, bool showOnlyTotals = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, shiftTypeIds, employeeIds, payrollProductIds, timePeriodIds, accumulatorId, includePreliminary, includeInactiveEmployees, showOnlyTotals, false, exportTypeId, exportFileTypeId, webBaseUrl)
        {
            this.KU10RemovePrevSubmittedData = ku10RemovePrevSubmittedData;
            this.DataStorageId = 0;
        }

        public TimeKU10ReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, int dataStorageId, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, webBaseUrl: webBaseUrl)
        {
            this.KU10RemovePrevSubmittedData = false;
            this.DataStorageId = dataStorageId;
        }

        public TimeKU10ReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.KU10RemovePrevSubmittedData = StringUtility.TryGetBoolValue(dict, KU10REMOVEPREVSUBMITTEDDATA);
            this.DataStorageId = StringUtility.TryGetIntValue(dict, DATASTORAGEID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(KU10REMOVEPREVSUBMITTEDDATA, this.KU10RemovePrevSubmittedData);
            url += GetUrlParameter(DATASTORAGEID, this.DataStorageId);

            return url;
        }

        #endregion
    }

    public sealed class TimeAgdEmployeeReportDTO : TimeKU10ReportDTO
    {
        #region Constants

        #endregion

        #region Variables

        #endregion

        #region Ctor

        public TimeAgdEmployeeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, List<int> shiftTypeIds, List<int> employeeIds, List<int> payrollProductIds, List<int> timePeriodIds, int? accumulatorId, bool includePreliminary, bool ku10RemovePrevSubmittedData, bool includeInactiveEmployees = false, bool showOnlyTotals = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, shiftTypeIds, employeeIds, payrollProductIds, timePeriodIds, accumulatorId, includePreliminary, ku10RemovePrevSubmittedData, includeInactiveEmployees, showOnlyTotals, exportTypeId, exportFileTypeId, webBaseUrl)
        {
        }

        public TimeAgdEmployeeReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
        }

        #endregion

        #region Public methods

        #endregion
    }

    public sealed class TimePayrollSlipReportDTO : TimeReportDTO
    {
        #region Constants

        private const string EMPLOYEE = "emp";
        private const string TIMEPERIOD = "per";
        private const string PRELIMINARY = "prel";

        #endregion

        #region Variables

        public List<int> TimePeriodIds { get; set; }
        public List<int> EmployeeIds { get; set; }
        public bool Preliminary { get; set; }

        #endregion

        #region Ctor

        public TimePayrollSlipReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int employeeId, int timePeriodId, bool includeInactiveEmployees = false, bool showOnlyTotals = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false, bool preliminary = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, includeInactiveEmployees, showOnlyTotals, exportTypeId, exportFileTypeId, webBaseUrl)
        {
            this.EmployeeIds = new List<int>() { employeeId };
            this.TimePeriodIds = new List<int>() { timePeriodId };
            this.Preliminary = preliminary;
        }

        public TimePayrollSlipReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, List<int> employeeIds, List<int> timePeriodIds, bool includePreliminary, bool includeInactiveEmployees = false, bool showOnlyTotals = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false, bool preliminary = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, includeInactiveEmployees, showOnlyTotals, exportTypeId, exportFileTypeId, webBaseUrl)
        {
            this.EmployeeIds = employeeIds;
            this.TimePeriodIds = timePeriodIds;
            this.Preliminary = preliminary;
        }

        public TimePayrollSlipReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int employeeId, int timePeriodId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.EmployeeIds = new List<int>() { employeeId };
            this.TimePeriodIds = new List<int>() { timePeriodId };
            this.Preliminary = StringUtility.TryGetBoolValue(dict, PRELIMINARY);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(EMPLOYEE, this.EmployeeIds);
            url += GetUrlParameter(TIMEPERIOD, this.TimePeriodIds);
            url += GetUrlParameter(PRELIMINARY, this.Preliminary);

            return url;
        }

        #endregion
    }

    public sealed class TimeEmploymentReportDTO : TimeReportDTO
    {
        #region Constants

        private const string EMPLOYEE = "emp";
        private const string EMPLOYMENT = "empl";
        private const string DATE = "date";
        private const string SUBSTITUTEDATES = "subdates";
        private const string PRINTEDFROMSCHEDULEPLANNING = "pfsp";

        #endregion

        #region Variables

        public List<int> EmployeeIds { get; set; }
        public int EmploymentId { get; set; }
        public DateTime? ChangesForDate { get; set; }
        public List<DateTime> SubstituteDates { get; set; }
        public bool PrintedFromSchedulePlanning { get; set; }

        #endregion

        #region Ctor

        public TimeEmploymentReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, DateTime? changesForDate, List<DateTime> substituteDates, int employeeId, int employmentId, bool printedFromSchedulePlanning, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, 0, false, false, exportTypeId, webBaseUrl)
        {
            this.EmployeeIds = new List<int>() { employeeId };
            this.EmploymentId = employmentId;
            this.ChangesForDate = changesForDate;
            this.SubstituteDates = substituteDates;
            this.PrintedFromSchedulePlanning = printedFromSchedulePlanning;
        }

        public TimeEmploymentReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, DateTime? changesForDate, List<DateTime> substituteDates, List<int> employeeIds, bool printedFromSchedulePlanning, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, 0, false, false, exportTypeId, webBaseUrl)
        {
            this.EmployeeIds = employeeIds;
            this.ChangesForDate = changesForDate;
            this.SubstituteDates = substituteDates;
            this.PrintedFromSchedulePlanning = printedFromSchedulePlanning;
        }

        public TimeEmploymentReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.EmployeeIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, EMPLOYEE));
            this.EmploymentId = StringUtility.TryGetIntValue(dict, EMPLOYMENT);
            this.ChangesForDate = StringUtility.TryGetDateValue(dict, DATE);
            this.SubstituteDates = StringUtility.SplitDateList(StringUtility.TryGetStringValue(dict, SUBSTITUTEDATES));
            this.PrintedFromSchedulePlanning = StringUtility.TryGetBoolValue(dict, PRINTEDFROMSCHEDULEPLANNING);

        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(EMPLOYEE, this.EmployeeIds);
            url += GetUrlParameter(EMPLOYMENT, this.EmploymentId);
            url += GetUrlParameter(DATE, this.ChangesForDate);
            url += GetUrlParameter(SUBSTITUTEDATES, this.SubstituteDates);
            url += GetUrlParameter(PRINTEDFROMSCHEDULEPLANNING, this.PrintedFromSchedulePlanning);

            return url;
        }

        #endregion
    }

    public class TimeScheduleTasksAndDeliverysReportDTO : TimeReportDTO
    {
        #region Constants

        private const string TIMESCHEDULETASKS = "tasks";
        private const string TIMEINCOMINGDELIVERYHEADS = "deliveries";
        private const string ISDAYVIEW = "isday";

        #endregion

        #region Variables

        public List<int> TimeScheduleTaskIds { get; set; }
        public List<int> TimeScheduleDeliveryHeadIds { get; set; }
        public bool IsDayView { get; set; }

        #endregion

        #region Ctor

        public TimeScheduleTasksAndDeliverysReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, List<int> timeScheduleTaskIds, List<int> timeScheduleDeliveryHeadIds, bool isDayView, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, int exportFileTypeId = (int)TermGroup_ReportExportFileType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, 0, false, false, exportTypeId, webBaseUrl)
        {
            this.TimeScheduleTaskIds = timeScheduleTaskIds;
            this.TimeScheduleDeliveryHeadIds = timeScheduleDeliveryHeadIds;
            this.IsDayView = isDayView;
        }

        public TimeScheduleTasksAndDeliverysReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.TimeScheduleTaskIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, TIMESCHEDULETASKS));
            this.TimeScheduleDeliveryHeadIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, TIMEINCOMINGDELIVERYHEADS));
            this.IsDayView = StringUtility.TryGetBoolValue(dict, ISDAYVIEW);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(TIMESCHEDULETASKS, this.TimeScheduleTaskIds);
            url += GetUrlParameter(TIMEINCOMINGDELIVERYHEADS, this.TimeScheduleDeliveryHeadIds, useInterval: true);
            url += GetUrlParameter(ISDAYVIEW, this.IsDayView);

            return url;
        }

        #endregion
    }

    public sealed class TimeCategoryReportDTO : TimeReportDTO
    {
        #region Constants

        private const string CATEGORYID = "cid";

        #endregion

        #region Variables

        public List<int> CategoryIds { get; set; }

        #endregion

        #region Ctor

        public TimeCategoryReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, int categoryId, bool includeInactiveEmployees = false, bool showOnlyTotals = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, includeInactiveEmployees, showOnlyTotals, exportTypeId, webBaseUrl)
        {
            this.CategoryIds = new List<int>() { categoryId };
        }

        public TimeCategoryReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, List<int> categoryIds, bool includeInactiveEmployees = false, bool showOnlyTotals = false, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, includeInactiveEmployees, showOnlyTotals, exportTypeId, webBaseUrl)
        {
            this.CategoryIds = categoryIds;
        }

        public TimeCategoryReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.CategoryIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, CATEGORYID));
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(CATEGORYID, this.CategoryIds);

            return url;
        }

        #endregion
    }

    public sealed class TimeUsersListReportDTO : TimeReportDTO
    {
        #region Constants

        public static readonly string ROOT_TAG = "employees";
        public static readonly string USER_TAG = "user";
        public static readonly string EMPLOYEENR_TAG = "enr";
        public static readonly string FIRSTNAME_TAG = "fn";
        public static readonly string LASTNAME_TAG = "ln";
        public static readonly string LOGINNAME_TAG = "login";
        public static readonly string EMAIL_TAG = "email";
        public static readonly string XML = "xml";

        #endregion

        #region Variables

        public string Xml { get; set; }

        #endregion

        #region Ctor

        public TimeUsersListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, string xml, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, CalendarUtility.DATETIME_DEFAULT, CalendarUtility.DATETIME_DEFAULT, 0, false, false, exportTypeId, webBaseUrl)
        {
            this.Xml = XmlConvert.EncodeName(xml);
        }

        public TimeUsersListReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.Xml = XmlConvert.DecodeName(StringUtility.TryGetStringValue(dict, XML));
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);
            url += GetUrlParameter(XML, this.Xml);
            return url;
        }

        #endregion
    }

    public sealed class TimeSalarySpecificationReportDTO : TimeReportDTO
    {
        #region Constants

        private const string DATASTORAGEID = "dsid";

        #endregion

        #region Variables

        public int DataStorageId { get; set; }

        #endregion

        #region Ctor

        public TimeSalarySpecificationReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, int dataStorageId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, false, false, exportTypeId, webBaseUrl)
        {
            this.DataStorageId = dataStorageId;
        }

        public TimeSalarySpecificationReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.DataStorageId = StringUtility.TryGetIntValue(dict, DATASTORAGEID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(DATASTORAGEID, this.DataStorageId);

            return url;
        }

        #endregion
    }

    public sealed class TimeSalaryControlInfoReportDTO : TimeReportDTO
    {
        #region Constants

        private const string DATASTORAGEID = "dsid";

        #endregion

        #region Variables

        public int DataStorageId { get; set; }

        #endregion

        #region Ctor

        public TimeSalaryControlInfoReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, DateTime startDate, DateTime stopDate, int timePeriodId, int dataStorageId, int exportTypeId = (int)TermGroup_ReportExportType.Unknown, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, startDate, stopDate, timePeriodId, false, false, exportTypeId, webBaseUrl)
        {
            this.DataStorageId = dataStorageId;
        }

        public TimeSalaryControlInfoReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.DataStorageId = StringUtility.TryGetIntValue(dict, DATASTORAGEID);
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return this.ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);

            url += GetUrlParameter(DATASTORAGEID, this.DataStorageId);

            return url;
        }

        #endregion
    }

    #endregion

    #endregion

    #region PayrollProduct reports

    public abstract class PayrollProductReportBaseDTO : ReportSelectionDTO
    {
        #region Constants

        public static readonly string WebBaseUrl = "/ajax/printReport.aspx";
        public static readonly string BaseUrl = "/soe/common/distribution/reports/reporturl/";

        #endregion

        #region Variables


        #endregion

        #region Ctor

        protected PayrollProductReportBaseDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, bool webBaseUrl = false) :
            base((webBaseUrl ? WebBaseUrl : BaseUrl), actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null)
        {

        }

        protected PayrollProductReportBaseDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, int payrollProductId, bool webBaseUrl = false) :
            base((webBaseUrl ? WebBaseUrl : BaseUrl), actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, null, null, null, null)
        {

        }

        protected PayrollProductReportBaseDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(BaseUrl, actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
        }

        #endregion

        #region Public methods


        #endregion
    }

    public sealed class PayrollProductReportDTO : PayrollProductReportBaseDTO
    {
        #region Variables

        private const string PAYROLLPRODUCT = "payrollproductid";
        public List<int> PayrollProductIds { get; set; }

        #endregion

        #region Ctor

        public PayrollProductReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, Dictionary<string, string> dict) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, dict)
        {
            this.PayrollProductIds = StringUtility.SplitNumericList(StringUtility.TryGetStringValue(dict, PAYROLLPRODUCT));
        }

        public PayrollProductReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, List<int> payrollProductIds, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, false)
        {
            this.PayrollProductIds = payrollProductIds;
        }

        public PayrollProductReportDTO(int actorCompanyId, int reportId, int sysReportTemplateTypeId, int exportTypeId, int payrollProductId, PayrollProductDTO product, bool webBaseUrl = false) :
            base(actorCompanyId, reportId, sysReportTemplateTypeId, exportTypeId, payrollProductId, false)
        {
            this.PayrollProductIds = new List<int>() { payrollProductId };
        }

        #endregion


        #region Public methods

        public override string ToString()
        {
            return ToString(true);
        }

        public override string ToString(bool includeBaseUrl)
        {
            string url = base.ToString(includeBaseUrl);
            url += GetUrlParameter(PAYROLLPRODUCT);
            url += StringUtility.GetCommaSeparatedString(this.PayrollProductIds, useInterval: false);
            return url;
        }

        #endregion
    }

    #endregion

    #region Selection in Web

    /// <summary>
    /// Filled by DistributionSelectionStd UserControl with values from GUI
    /// </summary>
    public class SelectionStd
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int AccountYearFromId { get; set; }
        public int AccountYearToId { get; set; }
        public DateTime? AccountYearFromDate { get; set; }
        public DateTime? AccountYearToDate { get; set; }

        public int AccountPeriodFromId { get; set; }
        public int AccountPeriodToId { get; set; }
        public DateTime? AccountPeriodFromDate { get; set; }
        public DateTime? AccountPeriodToDate { get; set; }

        public int? BudgetId { get; set; }

        public bool CreateVatVoucher { get; set; }
        public bool IncludeYearEndVouchers { get; set; }
        public bool IncludeExternalVouchers { get; set; }
        public bool ProjectReport { get; set; }

        public int AccountDimId { get; set; }
        public bool IncludeMissingAccountDim { get; set; }
        public bool SeparateAccountDim { get; set; }
    }

    /// <summary>
    /// Filled by DistributionSelectionVoucher UserControl with values from GUI
    /// </summary>
    public class SelectionVoucher
    {
        public int? VoucherSeriesTypeNrFrom { get; set; }
        public int? VoucherSeriesTypeNrTo { get; set; }
        public int? VoucherNrFrom { get; set; }
        public int? VoucherNrTo { get; set; }
        public List<int> VoucherHeadIds { get; set; }
        public bool IsAccountingOrder { get; set; }
    }

    public class SelectionFixedAssets
    {
        public string InventoryFrom { get; set; }
        public string InventoryTo { get; set; }
        public string CategoryFrom { get; set; }
        public string CategoryTo { get; set; }
        public int PrognoseType { get; set; }
    }

    /// <summary>
    /// Filled by DistributionSelectionAccount UserControl with values from GUI
    /// </summary>
    public class SelectionAccount
    {
        private List<AccountIntervalDTO> accountIntervals;
        public List<AccountIntervalDTO> AccountIntervals
        {
            get
            {
                if (accountIntervals == null)
                {
                    accountIntervals = new List<AccountIntervalDTO>();
                }
                return accountIntervals;
            }
        }

        public void AddAccountInterval(AccountIntervalDTO accountInterval)
        {
            if (accountIntervals == null)
            {
                accountIntervals = new List<AccountIntervalDTO>();
            }

            accountIntervals.Add(accountInterval);
        }
    }

    /// <summary>
    /// Filled by DistributionSelectionUser UserControl with values from GUI
    /// </summary>
    public class SelectionUser
    {
        public int? UserId { get; set; }
    }

    /// <summary>
    /// Filled by DistributionSelectionLedger UserControl with values from GUI
    /// </summary>
    public class SelectionLedger
    {
        public int SortOrder { get; set; } //TermGroup_ReportLedgerSortOrder
        public int DateRegard { get; set; } //TermGroup_ReportLedgerDateRegard
        public int InvoiceSelection { get; set; } //TermGroup_ReportLedgerInvoiceSelection
        public bool ShowVoucher { get; set; }
        public bool ShowPendingPaymentsInReport { get; set; }
        public string ActorNrFrom { get; set; }
        public string ActorNrTo { get; set; }
        public int? InvoiceSeqNrFrom { get; set; }
        public int? InvoiceSeqNrTo { get; set; }
        public List<int> InvoiceIds { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool ShowPreliminaryInvoices { get; set; }
        public bool IncludeCashSalesInvoices { get; set; }
     }

    /// <summary>
    /// Filled by DistributionSelectionBilling UserControl with values from GUI
    /// </summary>
    public class SelectionBilling
    {
        public int SortOrder { get; set; }
        public string CustomerNrFrom { get; set; }
        public string CustomerNrTo { get; set; }
        public string InvoiceNrFrom { get; set; }
        public string InvoiceNrTo { get; set; }
        public List<int> InvoiceIds { get; set; }
        public List<int> ProjectIds { get; set; }
        public int? ChecklistHeadRecordId { get; set; }
        public string InvoiceCacheKey { get; set; }
        public bool InvoiceCopy { get; set; }
        public bool InvoiceReminder { get; set; }
        public bool IncludeProjectReport { get; set; } //used for OrderInvoiceEdit
        public bool IncludeProjectReport2 { get; set; } //used for selection view
        public bool IncludeOnlyInvoiced { get; set; }
        public bool DisableInvoiceCopies { get; set; }
        public bool ShowNotPrinted { get; set; }
        public bool ShowCopy { get; set; }
        public bool IncludeClosedOrder { get; set; }
        public int? BudgetId { get; set; }
        public int? CustomerGroupId { get; set; }
        public int ReportLanguageId { get; set; }
        public string ProductNrFrom { get; set; }
        public string ProductNrTo { get; set; }
        public int StockLocationIdFrom { get; set; }
        public int StockLocationIdTo { get; set; }
        public int StockShelfIdFrom { get; set; }
        public int StockShelfIdTo { get; set; }
        public int StockInventoryId { get; set; }

        #region ProjectStats

        public string ProjectNrFrom { get; set; }
        public string ProjectNrTo { get; set; }
        public String EmployeeNrFrom { get; set; }
        public String EmployeeNrTo { get; set; }
        public List<int> ProjectIdList { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string PeriodFrom { get; set; }
        public string PeriodTo{ get; set; }

        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
        #endregion

        #region HouseholdTaxDeduction

        public int HTDCompanyId { get; set; }
        public int HTDSeqNbr { get; set; }
        public List<int> HTDCustomerInvoiceRowIds { get; set; }

        #endregion
    }

    #endregion
}
