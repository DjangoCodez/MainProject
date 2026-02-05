using AngleSharp.Common;
using Common.Util;
using CrystalDecisions.Shared;
using Microsoft.Azure.Amqp.Framing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.FlexForceService;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.ScheduledJobs;
using SoftOne.Soe.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Transactions;
using System.Web.Hosting;
using StringUtility = SoftOne.Soe.Common.Util.StringUtility;

namespace SoftOne.Soe.Business.Util
{
    public class StaffingStatisticsInput
    {
        #region Properties

        public TermGroup_TimeSchedulePlanningFollowUpCalculationType CalculationType { get; }
        public int ActorCompanyId { get; }
        public DateTime DateFrom { get; }
        public DateTime SelectionDateFrom { get; set; }
        public DateTime SelectionDateTo { get; set; }
        public DateTime DateTo { get; }
        public bool ForceWeekView { get; set; }
        public bool IsWeekView { get { return ForceWeekView || DateFrom.Date < DateTo.Date; } }
        public TimeSpan IntervalLength { get; }
        private int? ratioPerHour;
        public int RatioPerHour
        {
            get
            {
                if (!this.ratioPerHour.HasValue)
                {
                    if (this.IsWeekView)
                        this.ratioPerHour = 1;
                    else
                        this.ratioPerHour = (60 / (this.IntervalLength.Minutes > 0 ? this.IntervalLength.Minutes : 15)); //IntervalLength can only be 15,30,60
                }
                return this.ratioPerHour.Value;
            }
        }
        public int AccountId { get; set; }
        public int? ReportId { get; set; }
        public List<PayrollGroupSetting> PayrollGroupSettings;
        public List<PayrollGroupPriceTypeDTO> PayrollGroupPriceTypes;
        public List<EmploymentPriceTypeDTO> EmploymentPriceTypes;
        public List<AccountDTO> FilterAccounts { get; set; }
        public List<AccountDTO> Accounts { get; set; }

        public AccountDTO MainAccount
        {
            get
            {
                mainAccount = mainAccount ?? this.Accounts.FirstOrDefault(a => a.AccountId == this.AccountId);
                return mainAccount;
            }
        }

        private AccountDTO mainAccount { get; set; }

        public bool AllAccountsMustbeOnEntity { get; set; } = false;

        private bool? hasEmptyAccounts { get; set; }
        public bool HasEmptyAccounts
        {
            get
            {
                if (hasEmptyAccounts.HasValue)
                    return hasEmptyAccounts.Value;

                hasEmptyAccounts = this.Accounts.Any(a => a.AccountId < 0);

                return hasEmptyAccounts.Value;
            }
        }
        public bool HasEmptyAccountsOnDim(int dimId)
        {
            if (HasEmptyAccounts)
            {
                emptyAccounts = emptyAccounts ?? this.Accounts.Where(a => a.AccountId < 0).ToList();
                return emptyAccounts.Any(a => a.AccountDimId == dimId);
            }
            else
                return false;
        }
        private List<AccountDTO> emptyAccounts { get; set; }

        public List<AccountDTO> FrequencyFilterAccounts { get; }
        public List<EmployeeGroup> EmployeeGroups { get; set; }
        public List<PayrollPriceType> PayrollPriceTypes { get; set; }
        public List<PayrollGroup> PayrollGroups { get; set; }
        public List<PayrollProductSettingDTO> PayrollProductSettings { get; set; }

        public List<PayrollProductReportSetting> PayrollProductReportSettings { get; }
        public List<FixedPayrollRowDTO> FixedPayrollRows { get; internal set; }
        public EvaluatePayrollPriceFormulaInputDTO EvaluatePayrollPriceFormulaInputDTO { get; internal set; }

        public bool LoadNeed { get; }
        public bool LoadFrequency { get; }
        public bool LoadRowFrequency { get; }
        public bool LoadBudget { get; }
        public bool LoadForecast { get; }
        public bool LoadTemplate { get; }
        public bool LoadTemplateForEmployeePost { get; }
        public bool LoadSchedule { get; }
        public bool LoadTransactions { get; }
        public bool TryToGroupOnEmployee { get; set; }
        public List<int> EmployeeIds { get; set; }
        public bool CalculateNeed { get; }
        public bool CalculateFreqency { get; }
        public bool CalculateRowFreqency { get; }
        public bool CalculateBudget { get; }
        public bool CalculateForecast { get; }
        public bool CalculateTemplate { get; }
        public bool CalculateTemplateForEmployeePost { get; }
        public bool CalculateSchedule { get; }
        public bool CalculateTime { get; }
        public bool IncludeEmpTaxAndSuppCharge { get; set; }

        public bool CalculateSales
        {
            get
            {
                return this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All || this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales;
            }
        }
        public bool CalculateHours
        {
            get
            {
                return this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All || this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours;
            }
        }
        public bool CalculatePersonalCost
        {
            get
            {
                return this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All || this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost;
            }
        }
        public bool CalculateSalaryPercent
        {
            get
            {
                return this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All || this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent;
            }
        }
        public bool CalculateLPAT
        {
            get
            {
                return this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All || this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT;
            }
        }
        public bool CalculateFPAT
        {
            get
            {
                return this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All || this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT;
            }
        }
        public bool CalculateBPAT
        {
            get
            {
                return this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All || this.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT;
            }
        }

        public bool AddSummaryRow { get; }
        public Dictionary<int, List<TimePayrollTransactionDTO>> AllTransactionsDict { get; set; }

        #endregion

        #region Loadings

        private decimal templateCostPerHour;

        private List<EmployeeDTO> employees;
        private List<ShiftTypeDTO> shiftTypes;
        private List<StaffingNeedsRowPeriodDTO> needs;
        private List<StaffingNeedsFrequencyDTO> frequencys;
        private List<StaffingNeedsFrequencyDTO> budgetFrequencys;
        private List<StaffingNeedsFrequencyDTO> forecastFrequencys;
        private List<StaffingNeedsRowFrequencyDTO> rowFrequencys;
        private List<StaffingNeedsRowFrequencyDTO> budgetRowFrequencys;
        private List<StaffingNeedsRowFrequencyDTO> forecastRowFrequencys;
        private List<BudgetPeriodDTO> budgets;
        private List<BudgetPeriodDTO> forecasts = new List<BudgetPeriodDTO>();
        private List<SysPayrollPriceViewDTO> sysPayrollPriceViews;
        /// <summary> Key: Employee. Value: Time inteval</summary>
        private Dictionary<int, List<WorkIntervalDTO>> templateByEmployee;
        /// <summary> Key: Employee. Value: Time inteval</summary>
        private Dictionary<int, List<WorkIntervalDTO>> scheduleByEmployee;
        /// <summary> Key: Employee. Value: Time inteval</summary>
        private Dictionary<int, List<TimePayrollTransactionDTO>> transactionsByEmployee;
        /// <summary> Key: Employee. Value: Time inteval</summary>
        private Dictionary<int, List<TimePayrollTransactionDTO>> scheduletransactionsByEmployee;
        /// <summary> Key: EmployeePost. Value: Time inteval</summary>
        private Dictionary<int, List<WorkIntervalDTO>> templateForEmployeePostByEmployeePost;

        #endregion

        #region Ctor

        public StaffingStatisticsInput(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, int actorCompanyId, DateTime dateFrom, DateTime dateTo, DateTime selectionDateFrom, DateTime selectionDateTo, int intervalMinutes, int accountId, List<AccountDTO> accounts, List<AccountDTO> filterAccounts, List<AccountDTO> frequencyFilterAccounts, bool calculateNeed, bool calculateFreqency, bool calculateRowFreqency, bool calculateBudget, bool calculateForecast, bool calculateTemplate, bool calculateSchedule, bool calculateTime, bool addSummaryRow, bool calculateTemplateForEmployeePost, List<EmployeeGroup> employeeGroups = null, List<PayrollGroup> payrollGroups = null, List<PayrollPriceType> payrollPriceTypes = null, bool includeEmpTaxAndSuppCharge = false, bool trustBoolOverCalculationTypeAll = false, List<PayrollProductSettingDTO> payrollProductSettings = null, int? reportId = null, List<PayrollProductReportSetting> payrollProductReportSettings = null, bool noInterval = false, bool tryToGroupOnEmployee = false, List<int> employeeIds = null, bool forceWeekView = false)
        {
            this.CalculationType = calculationType;
            this.ActorCompanyId = actorCompanyId;
            this.DateFrom = CalendarUtility.GetBeginningOfDay(dateFrom);
            this.DateTo = CalendarUtility.GetEndOfDay(dateTo);
            this.SelectionDateFrom = CalendarUtility.GetBeginningOfDay(selectionDateFrom);
            this.SelectionDateTo = CalendarUtility.GetEndOfDay(selectionDateTo);
            this.ForceWeekView = forceWeekView;
            this.IntervalLength = noInterval ? (DateTo - dateFrom) : IsWeekView ? new TimeSpan(1, 0, 0, 0) : new TimeSpan(0, (intervalMinutes >= 15 ? intervalMinutes : 15), 0);
            this.AccountId = accountId;
            this.FilterAccounts = filterAccounts;
            this.Accounts = accounts;
            this.FrequencyFilterAccounts = frequencyFilterAccounts;
            this.PayrollGroups = payrollGroups;
            this.EmployeeGroups = employeeGroups;
            this.PayrollPriceTypes = payrollPriceTypes;
            this.PayrollProductSettings = payrollProductSettings ?? new List<PayrollProductSettingDTO>();
            this.ReportId = reportId;
            this.PayrollProductReportSettings = payrollProductReportSettings ?? new List<PayrollProductReportSetting>();
            this.LoadNeed = this.CalculateNeed = calculateNeed;
            this.LoadFrequency = this.CalculateFreqency = calculateFreqency;
            this.LoadRowFrequency = this.CalculateRowFreqency = calculateRowFreqency;
            this.LoadBudget = this.CalculateBudget = calculateBudget;
            this.LoadForecast = this.CalculateForecast = calculateForecast;
            this.LoadTemplate = this.CalculateTemplate = calculateTemplate;
            this.LoadTemplateForEmployeePost = this.CalculateTemplateForEmployeePost = calculateTemplateForEmployeePost;
            this.LoadSchedule = this.CalculateSchedule = calculateSchedule;
            this.LoadTransactions = this.CalculateTime = calculateTime;
            this.TryToGroupOnEmployee = tryToGroupOnEmployee;
            this.EmployeeIds = employeeIds;

            this.AddSummaryRow = addSummaryRow;

            switch (this.CalculationType)
            {
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.All:
                    if (!trustBoolOverCalculationTypeAll)
                    {
                        this.LoadNeed = true;
                        this.LoadFrequency = true;
                        this.LoadBudget = true;
                        this.LoadTemplate = true;
                        this.LoadSchedule = true;
                        this.LoadTransactions = true;
                    }
                    if (calculateTemplateForEmployeePost)
                        this.LoadTemplateForEmployeePost = true;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales:
                    this.LoadFrequency = true;
                    this.LoadBudget = true;
                    this.LoadTransactions = true;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours:
                    this.LoadNeed = true;
                    this.LoadBudget = true;
                    this.LoadSchedule = true;
                    this.LoadTransactions = true;
                    if (calculateTemplateForEmployeePost)
                        this.LoadTemplateForEmployeePost = true;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost:
                    this.LoadTransactions = true;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent:
                    this.LoadFrequency = true;
                    this.LoadBudget = true;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT:
                    if (CalculateSchedule)
                        this.LoadTemplate = true;
                    if (CalculateTime)
                        this.LoadSchedule = true;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT:
                    if (CalculateTemplate)
                        this.LoadBudget = true;
                    if (CalculateSchedule)
                        this.LoadBudget = true;
                    if (CalculateTime)
                        this.LoadFrequency = true;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT:
                    //NA
                    break;
            }

            this.IncludeEmpTaxAndSuppCharge = includeEmpTaxAndSuppCharge;
        }

        #endregion

        #region Public methods

        #region Interval

        public void GetIntervalDates(DateTime interval, bool useRealDate, out DateTime intervalFrom, out DateTime intervalTo)
        {
            intervalFrom = CalendarUtility.GetDateTime(useRealDate ? interval : CalendarUtility.DATETIME_DEFAULT, interval);
            intervalTo = intervalFrom.Add(this.IntervalLength).AddSeconds(-1);
        }

        #endregion

        #region TemplateCost

        public void SetTemplateCostPerHour(decimal templateCostPerHour)
        {
            this.templateCostPerHour = templateCostPerHour;
        }

        public decimal GetTemplateCostPerHour()
        {
            return this.templateCostPerHour;
        }

        #endregion

        #region ShiftType

        public void SetShiftTypes(List<ShiftTypeDTO> shiftTypes)
        {
            if (shiftTypes == null)
                return;
            this.shiftTypes = new List<ShiftTypeDTO>();
            this.shiftTypes.AddRange(shiftTypes);
        }

        public List<ShiftTypeDTO> GetShiftTypes()
        {
            return this.shiftTypes;
        }

        #endregion

        #region Need

        public void SetNeeds(List<StaffingNeedsHeadDTO> needs)
        {
            if (needs == null)
                return;
            this.needs = new List<StaffingNeedsRowPeriodDTO>();
            foreach (StaffingNeedsHeadDTO need in needs)
            {
                this.needs.AddRange(need.GetPeriods());
            }
            this.needs = FilterNeedByAccount(this.needs);
        }

        public List<StaffingNeedsRowPeriodDTO> GetNeeds()
        {
            return this.needs;
        }

        #endregion

        #region Budget

        public void SetBudgets(List<BudgetHeadDTO> budgets)
        {
            if (budgets == null)
                return;
            this.budgets = new List<BudgetPeriodDTO>();

            List<DistributionCodeHead> distributionCodeHeads = new List<DistributionCodeHead>();


            if (!this.IsWeekView)
            {
                BudgetManager vm = new BudgetManager(null);
                distributionCodeHeads = vm.GetDistributionCodes(this.ActorCompanyId, true);

                foreach (var head in distributionCodeHeads)
                    head.OpeningHoursReference.Load();
            }

            foreach (BudgetHeadDTO budgetHead in budgets)
            {
                foreach (BudgetRowDTO budgetRow in budgetHead.Rows)
                {
                    if (this.IsWeekView)
                    {
                        this.budgets.AddRange(budgetRow.Periods.Where(i => i.StartDate.HasValue && i.Type == (int)BudgetRowPeriodType.Day));
                    }
                    else
                    {
                        var periods = new List<BudgetPeriodDTO>();
                        if (budgetRow != null)
                        {
                            foreach (var period in budgetRow.Periods.Where(i => i.DistributionCodeHeadId.HasValue && i.StartDate.HasValue && i.StartDate >= this.DateFrom && i.StartDate <= CalendarUtility.GetEndOfDay(this.DateTo) && i.Type == (int)BudgetRowPeriodType.Day).OrderBy(o => o.PeriodNr))
                            {

                                var code = distributionCodeHeads.FirstOrDefault(f => f.DistributionCodeHeadId == period.DistributionCodeHeadId.Value);
                                if (code != null)
                                {
                                    decimal totalAmount = period.Amount;
                                    decimal totalQuantity = period.Quantity;
                                    DateTime startTime = CalendarUtility.MergeDateAndTime(period.StartDate.Value, code.OpeningHours.OpeningTime);
                                    DateTime stopTime = CalendarUtility.MergeDateAndTime(period.StartDate.Value, code.OpeningHours.ClosingTime);
                                    DateTime currentTime = startTime;
                                    int index = 0;
                                    int maxIndex = code.DistributionCodePeriod.Count - 1;

                                    while (currentTime <= stopTime)
                                    {
                                        if (index <= maxIndex)
                                        {
                                            var distributionCodePeriod = code.DistributionCodePeriod.ToArray()[index];

                                            if (distributionCodePeriod != null)
                                            {
                                                BudgetPeriodDTO periodDTO = new BudgetPeriodDTO()
                                                {
                                                    PeriodNr = index + 1,
                                                    StartDate = currentTime,
                                                    Quantity = decimal.Round(decimal.Multiply(totalQuantity, decimal.Divide(distributionCodePeriod.Percent, 100)), 2),
                                                    Amount = decimal.Round(decimal.Multiply(totalAmount, decimal.Divide(distributionCodePeriod.Percent, 100)), 2),
                                                    Type = (int)BudgetRowPeriodType.Hour,
                                                    BudgetRow = period.BudgetRow,
                                                };

                                                periods.Add(periodDTO);
                                            }
                                        }

                                        currentTime = currentTime.AddHours(1);
                                        index++;
                                    }
                                }
                            }
                        }
                        this.budgets.AddRange(periods);
                    }
                }
            }
            this.budgets = FilterBudgetByAccount(this.budgets);
        }

        public List<BudgetPeriodDTO> GetForecasts()
        {
            return this.forecasts;
        }

        public List<BudgetPeriodDTO> GetBudgets()
        {
            return this.budgets;
        }

        #endregion

        #region Frequency

        public void SetFrequencys(List<StaffingNeedsFrequencyDTO> frequencys)
        {
            if (frequencys == null)
                return;

            frequencys = frequencys.Where(w => w.FrequencyType == FrequencyType.Actual).ToList();

            this.frequencys = new List<StaffingNeedsFrequencyDTO>();
            foreach (var frequencysGroupedByTime in frequencys.GroupBy(i => i.CompareKey)) //only use latest
            {
                StaffingNeedsFrequencyDTO latestFrequency = frequencysGroupedByTime.OrderByDescending(o => o.StaffingNeedsFrequencyId).FirstOrDefault();
                if (latestFrequency != null)
                    this.frequencys.Add(latestFrequency);
            }
            this.frequencys = FilterFrequencyByAccount(this.frequencys);
        }

        public void SetRowFrequencys(List<StaffingNeedsRowFrequencyDTO> frequencys)
        {
            if (frequencys == null)
                return;
            this.rowFrequencys = new List<StaffingNeedsRowFrequencyDTO>();
            foreach (StaffingNeedsRowFrequencyDTO frequency in frequencys)
            {
                this.rowFrequencys.Add(frequency);
            }

            this.rowFrequencys = FilterRowFrequencyByAccount(this.rowFrequencys);
        }

        public List<StaffingNeedsFrequencyDTO> GetFrequencys()
        {
            return this.frequencys;
        }

        public List<StaffingNeedsRowFrequencyDTO> GetRowFrequencys()
        {
            return this.rowFrequencys;
        }

        #endregion

        #region 

        public void SetBudgetOrForecastFrequencys(List<StaffingNeedsFrequencyDTO> budgetFrequencys, FrequencyType frequencyType)
        {
            if (budgetFrequencys == null)
                return;

            budgetFrequencys = budgetFrequencys.Where(w => w.FrequencyType == frequencyType).ToList();

            if (frequencyType == FrequencyType.Budget)
            {
                this.budgetFrequencys = new List<StaffingNeedsFrequencyDTO>();
                foreach (var budgetFrequencysGroupedByTime in budgetFrequencys.GroupBy(i => i.CompareKey)) //only use latest
                {
                    StaffingNeedsFrequencyDTO latestBudgetFrequency = budgetFrequencysGroupedByTime.OrderByDescending(o => o.StaffingNeedsFrequencyId).FirstOrDefault();
                    if (latestBudgetFrequency != null)
                        this.budgetFrequencys.Add(latestBudgetFrequency);
                }
                this.budgetFrequencys = FilterFrequencyByAccount(this.budgetFrequencys);
            }
            else if (frequencyType == FrequencyType.Forecast)
            {
                this.forecastFrequencys = new List<StaffingNeedsFrequencyDTO>();
                foreach (var budgetFrequencysGroupedByTime in budgetFrequencys.GroupBy(i => i.CompareKey)) //only use latest
                {
                    StaffingNeedsFrequencyDTO latestBudgetFrequency = budgetFrequencysGroupedByTime.OrderByDescending(o => o.StaffingNeedsFrequencyId).FirstOrDefault();
                    if (latestBudgetFrequency != null)
                        this.forecastFrequencys.Add(latestBudgetFrequency);
                }
                this.forecastFrequencys = FilterFrequencyByAccount(this.forecastFrequencys);
            }
        }

        public void SetRowBudgetFrequencys(List<StaffingNeedsRowFrequencyDTO> budgetFrequencys)
        {
            if (budgetFrequencys == null)
                return;
            this.budgetRowFrequencys = new List<StaffingNeedsRowFrequencyDTO>();
            foreach (StaffingNeedsRowFrequencyDTO budgetFrequency in budgetFrequencys)
            {
                this.budgetRowFrequencys.Add(budgetFrequency);
            }

            this.budgetRowFrequencys = FilterRowFrequencyByAccount(this.budgetRowFrequencys);
        }

        public List<StaffingNeedsFrequencyDTO> GetBudgetFrequencys()
        {
            return this.budgetFrequencys;
        }

        public List<StaffingNeedsRowFrequencyDTO> GetRowBudgetFrequencys()
        {
            return this.budgetRowFrequencys;
        }

        public List<StaffingNeedsFrequencyDTO> GetForecastFrequencys()
        {
            return this.forecastFrequencys;
        }

        public List<StaffingNeedsRowFrequencyDTO> GetRowForecastFrequencys()
        {
            return this.forecastRowFrequencys;
        }

        public void SetRowForecastFrequencys(List<StaffingNeedsRowFrequencyDTO> forecastFrequencys)
        {
            if (forecastFrequencys == null)
                return;
            this.forecastRowFrequencys = new List<StaffingNeedsRowFrequencyDTO>();
            foreach (StaffingNeedsRowFrequencyDTO forecastFrequency in forecastFrequencys)
            {
                this.forecastRowFrequencys.Add(forecastFrequency);
            }

            this.forecastRowFrequencys = FilterRowFrequencyByAccount(this.forecastRowFrequencys);
        }

        #endregion

        #region Template

        public void SetTemplate(List<WorkIntervalDTO> template)
        {
            if (template == null)
                return;

            foreach (var groupById in template.GroupBy(i => i.Id))
            {
                SetTemplateForEmployee(groupById.Key, groupById.ToList());
            }
        }

        public void SetTemplate(Dictionary<int, List<WorkIntervalDTO>> template)
        {
            if (template == null)
                return;
            if (this.templateByEmployee == null)
                this.templateByEmployee = new Dictionary<int, List<WorkIntervalDTO>>();
            this.templateByEmployee.AddRange(template);
        }

        public void SetTemplateForEmployee(int employeeId, List<WorkIntervalDTO> template)
        {
            if (template == null)
                return;
            if (this.templateByEmployee == null)
                this.templateByEmployee = new Dictionary<int, List<WorkIntervalDTO>>();
            this.templateByEmployee.Add(employeeId, template);
        }

        public Dictionary<int, List<WorkIntervalDTO>> GetTemplate()
        {
            return this.templateByEmployee;
        }

        #endregion

        #region TemplateForEmployeePost

        public void SetTemplateForEmployeePost(List<WorkIntervalDTO> template)
        {
            if (template == null)
                return;

            foreach (var groupById in template.GroupBy(i => i.Id))
            {
                SetTemplateForEmployeePostForEmployee(groupById.Key, groupById.ToList());
            }
        }

        public void SetTemplateForEmployeePost(Dictionary<int, List<WorkIntervalDTO>> template)
        {
            if (template == null)
                return;
            if (this.templateForEmployeePostByEmployeePost == null)
                this.templateForEmployeePostByEmployeePost = new Dictionary<int, List<WorkIntervalDTO>>();
            this.templateForEmployeePostByEmployeePost.AddRange(template);
        }

        public void SetTemplateForEmployeePostForEmployee(int employeeId, List<WorkIntervalDTO> template)
        {
            if (template == null)
                return;
            if (this.templateForEmployeePostByEmployeePost == null)
                this.templateForEmployeePostByEmployeePost = new Dictionary<int, List<WorkIntervalDTO>>();
            this.templateForEmployeePostByEmployeePost.Add(employeeId, template);
        }

        public Dictionary<int, List<WorkIntervalDTO>> GetTemplateForEmployeePost()
        {
            return this.templateForEmployeePostByEmployeePost;
        }

        #endregion

        #region Schedule

        public void SetSchedule(List<WorkIntervalDTO> schedule)
        {
            if (schedule == null)
                return;
            foreach (var groupById in schedule.GroupBy(i => i.Id))
            {
                SetScheduleForEmployee(groupById.Key, groupById.ToList());
            }
        }

        public void SetSchedule(Dictionary<int, List<WorkIntervalDTO>> schedule)
        {
            if (schedule == null)
                return;
            if (this.scheduleByEmployee == null)
                this.scheduleByEmployee = new Dictionary<int, List<WorkIntervalDTO>>();
            this.scheduleByEmployee.AddRange(schedule);
        }

        public void SetScheduleForEmployee(int employeeId, List<WorkIntervalDTO> schedule)
        {
            if (schedule == null)
                return;
            if (this.scheduleByEmployee == null)
                this.scheduleByEmployee = new Dictionary<int, List<WorkIntervalDTO>>();
            this.scheduleByEmployee.Add(employeeId, schedule);
        }

        public Dictionary<int, List<WorkIntervalDTO>> GetSchedule()
        {
            return this.scheduleByEmployee;
        }

        #endregion

        #region Transactions

        public void SetTransactions(Dictionary<int, List<TimePayrollTransactionDTO>> transactions)
        {
            if (transactions == null)
                return;
            if (this.transactionsByEmployee == null)
                this.transactionsByEmployee = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            this.transactionsByEmployee.AddRange(transactions);
        }

        public void SetTransactionsForEmployee(int employeeId, List<TimePayrollTransactionDTO> transactions)
        {
            if (transactions == null)
                return;
            if (this.transactionsByEmployee == null)
                this.transactionsByEmployee = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            transactions = FilterTransactionsByAccount(transactions);
            transactions.ForEach(f => f.TempUsed = true);

            foreach (var transactionType in transactions.GroupBy(g => (g.ScheduleTransaction ?? false)))
            {
                if (transactionType.Key)
                    SetScheduleAbsenceTransactionsForEmployee(employeeId, transactionType.Where(W => W.ScheduleTransactionType == SoeTimePayrollScheduleTransactionType.Absence).ToList());
                else
                    SetPayrollTransactionsForEmployee(employeeId, transactionType.ToList());
            }
        }

        private void SetPayrollTransactionsForEmployee(int employeeId, List<TimePayrollTransactionDTO> transactions)
        {
            if (transactions == null)
                return;
            if (this.transactionsByEmployee == null)
                this.transactionsByEmployee = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            transactions = FilterTransactionsByAccount(transactions);
            transactions.ForEach(f => f.TempUsed = true);
            this.transactionsByEmployee.Add(employeeId, transactions);
        }

        private void SetScheduleAbsenceTransactionsForEmployee(int employeeId, List<TimePayrollTransactionDTO> transactions)
        {
            if (transactions == null)
                return;
            if (this.scheduletransactionsByEmployee == null)
                this.scheduletransactionsByEmployee = new Dictionary<int, List<TimePayrollTransactionDTO>>();
            transactions = FilterTransactionsByAccount(transactions);
            transactions.ForEach(f => f.TempUsed = true);
            this.scheduletransactionsByEmployee.Add(employeeId, transactions);
        }

        public Dictionary<int, List<TimePayrollTransactionDTO>> GetTransactions(bool includeAbsenceTransactions = false)
        {
            if (!includeAbsenceTransactions)
                return this.transactionsByEmployee;

            if (this.scheduletransactionsByEmployee == null)
            {
                this.scheduletransactionsByEmployee = new Dictionary<int, List<TimePayrollTransactionDTO>>();
                return this.transactionsByEmployee;
            }

            var combinedTransactions = this.scheduletransactionsByEmployee;

            if (this.transactionsByEmployee != null)
            {
                foreach (var employee in this.transactionsByEmployee)
                {
                    if (combinedTransactions.ContainsKey(employee.Key))
                    {
                        combinedTransactions[employee.Key] = combinedTransactions[employee.Key].Union(employee.Value).ToList();
                    }
                    else
                    {
                        combinedTransactions.Add(employee.Key, employee.Value);
                    }
                }
            }

            return combinedTransactions;
        }

        public List<TimePayrollTransactionDTO> GetTransactionsForEmployee(int employeeId, bool includeAbsenceTransactions = false)
        {
            var transactions = new List<TimePayrollTransactionDTO>();

            if (includeAbsenceTransactions && this.scheduletransactionsByEmployee != null)
            {
                this.scheduletransactionsByEmployee.TryGetValue(employeeId, out var absenceTransactions);
                if (absenceTransactions != null)
                    transactions.AddRange(absenceTransactions);
            }

            if (this.transactionsByEmployee != null && this.transactionsByEmployee.TryGetValue(employeeId, out List<TimePayrollTransactionDTO> payrollTransactions))
                transactions.AddRange(payrollTransactions);
            return transactions;
        }

        public void SetSettings(List<PayrollGroupSetting> payrollGroupSettingsInput, List<PayrollGroupPriceTypeDTO> payrollGroupPriceTypesInput, List<EmploymentPriceTypeDTO> employmentPriceTypesInput)
        {
            this.PayrollGroupSettings = payrollGroupSettingsInput;
            this.PayrollGroupPriceTypes = payrollGroupPriceTypesInput;
            this.EmploymentPriceTypes = employmentPriceTypesInput;
        }

        #endregion

        #region SysPayrollPriceViews

        public void SetSysPayrollPriceViews(List<SysPayrollPriceViewDTO> sysPayrollPriceViews)
        {
            this.sysPayrollPriceViews = sysPayrollPriceViews;
        }

        public List<SysPayrollPriceViewDTO> GetSysPayrollPriceViews()
        {
            return this.sysPayrollPriceViews;
        }

        #endregion

        #region Employee

        public void SetEmployees(List<EmployeeDTO> employees)
        {
            this.employees = employees;
            this.employeesDict = employees.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.FirstOrDefault());
        }

        private Dictionary<int, EmployeeDTO> employeesDict;

        public void SetEmployee(EmployeeDTO employee)
        {
            if (employee == null)
                return;
            if (this.employees == null)
                this.employees = new List<EmployeeDTO>();
            if (this.employees.Any(i => i.EmployeeId == employee.EmployeeId))
                return;
            this.employees.Add(employee);
        }

        public EmployeeDTO GetEmployee(int employeeId)
        {
            if (employeesDict != null && employeesDict.TryGetValue(employeeId, out EmployeeDTO employee) && employee != null)
                return employee;

            if (this.employees == null)
                return null;
            return this.employees.FirstOrDefault(i => i.EmployeeId == employeeId);
        }

        #endregion

        #region Filter by account

        public bool ValidAccordingToFilter(List<AccountInternalDTO> accountInternals)
        {
            if (FilterAccounts.IsNullOrEmpty())
                return true;

            if (accountInternals.IsNullOrEmpty())
                return true;

            return ValidAccordingToFilterCached(accountInternals.Select(s => s.AccountId).Distinct().OrderBy(o => o).ToList());
        }

        public bool ValidAccordingToFilter(List<AccountInternal> accountInternals)
        {
            if (FilterAccounts.IsNullOrEmpty())
                return true;

            if (accountInternals.IsNullOrEmpty())
                return true;

            return ValidAccordingToFilterCached(accountInternals.Select(s => s.AccountId).Distinct().OrderBy(o => o).ToList());
        }

        public bool ValidAccordingToFilter(List<int?> accountIds, bool useFrequencyFilterAccounts = false)
        {
            List<int> validInts = accountIds.Where(w => w.HasValue).Select(s => s.Value).ToList();
            return ValidAccordingToFilterCached(validInts, useFrequencyFilterAccounts);
        }
        public bool ValidAccordingToFilter(List<int> accountInternalIds, bool useFrequencyFilterAccounts = false)
        {
            var valid = true;

            if (FilterAccounts.IsNullOrEmpty())
                return true;

            if (accountInternalIds.IsNullOrEmpty())
                return false;

            var filterAccounts = FilterAccounts.ToList();

            if (useFrequencyFilterAccounts)
                filterAccounts = FrequencyFilterAccounts.Where(w => accountInternalIds.Contains(w.AccountId)).ToList();

            if (!filterAccounts.Any())
                valid = false;
            else
            {
                foreach (var accountsOnDim in filterAccounts.GroupBy(g => g.AccountDimId))
                {
                    if (valid)
                    {
                        var accountIds = accountsOnDim.Select(s => s.AccountId).Distinct().ToList();

                        if (accountInternalIds.Any(a => accountIds.Contains(a)))
                        {
                            valid = true;
                        }
                        else
                        {
                            if (!FilterAccounts.Any(a => a.AccountDimId == accountsOnDim.Key))
                            {
                                valid = true;
                            }
                            else
                            {
                                if (HasEmptyAccountsOnDim(accountsOnDim.Key) && MainAccount.AccountDimId == accountsOnDim.Key && this.AccountId < 0 && !AllAccountsMustbeOnEntity)
                                {
                                    valid = true;
                                }
                                else
                                {
                                    var internalAccounts = this.Accounts.Where(w => accountInternalIds.Contains(w.AccountId)).ToList();

                                    if (internalAccounts.Any(a => a.AccountDimId == accountsOnDim.Key))
                                        valid = false;
                                    if (valid && HasEmptyAccountsOnDim(accountsOnDim.Key))
                                        valid = false;

                                    if (AllAccountsMustbeOnEntity)
                                    {
                                        valid = false;

                                        if (!internalAccounts.Any(a => a.AccountDimId == accountsOnDim.Key) && HasEmptyAccountsOnDim(accountsOnDim.Key))
                                            valid = true;
                                    }

                                }
                            }
                        }
                    }
                }
            }

            return valid;
        }

        private ConcurrentDictionary<string, bool> _filterCache = new ConcurrentDictionary<string, bool>();

        public bool ValidAccordingToFilterCached(List<int> accountInternalIds, bool useFrequencyFilterAccounts = false, string key = null)
        {
            if (accountInternalIds == null && !FilterAccounts.Any())
                return true;
            else if (accountInternalIds == null && FilterAccounts.Any())
                return false;

            key = (useFrequencyFilterAccounts ? "freq_" : "") + ("all_" + this.AllAccountsMustbeOnEntity) + (!string.IsNullOrEmpty(key) ? key : accountInternalIds.Count == 0 ? "zero" : accountInternalIds.JoinToString("_"));
            if (_filterCache.TryGetValue(key, out bool cachedResult))
            {
                return cachedResult;
            }

            bool result = ValidAccordingToFilter(accountInternalIds, useFrequencyFilterAccounts);

            _filterCache.TryAdd(key, result);

            return result;
        }

        public List<StaffingNeedsRowPeriodDTO> FilterNeedByAccount(List<StaffingNeedsRowPeriodDTO> needs)
        {
            List<StaffingNeedsRowPeriodDTO> validPeriods = new List<StaffingNeedsRowPeriodDTO>();
            foreach (StaffingNeedsRowPeriodDTO need in needs)
            {
                ShiftTypeDTO shiftType = GetShiftTypes().FirstOrDefault(i => need.TimeSlot != null && need.ShiftTypeId.HasValue && i.ShiftTypeId == need.ShiftTypeId.Value);
                if (shiftType == null)
                    continue;

                bool valid = ValidAccordingToFilterCached(shiftType.AccountInternalIds);

                if (valid)
                    validPeriods.Add(need);
            }
            return validPeriods;
        }

        public List<StaffingNeedsRowFrequencyDTO> FilterRowFrequencyByAccount(List<StaffingNeedsRowFrequencyDTO> rowFrequencys)
        {
            List<StaffingNeedsRowFrequencyDTO> validFreqs = new List<StaffingNeedsRowFrequencyDTO>();
            foreach (StaffingNeedsRowFrequencyDTO freq in rowFrequencys)
            {
                ShiftTypeDTO shiftType = GetShiftTypes().FirstOrDefault(i => freq.ShiftTypeId.HasValue && i.ShiftTypeId == freq.ShiftTypeId.Value);
                if (shiftType == null)
                    continue;

                bool valid = ValidAccordingToFilterCached(shiftType.AccountInternalIds);

                if (valid)
                    validFreqs.Add(freq);
            }
            return validFreqs;
        }

        public List<StaffingNeedsFrequencyDTO> FilterFrequencyByAccount(List<StaffingNeedsFrequencyDTO> frequencys)
        {
            return frequencys.Where(w => ValidAccordingToFilter(new List<int?> { w.AccountId, w.ParentAccountId }, true)).ToList();
        }

        public List<TimeStampEntryDTO> FilterTimeStampEntriesByAccount(List<TimeStampEntryDTO> timeStampEntries)
        {
            return timeStampEntries.Where(w => ValidAccordingToFilter(w.GetAccountIds())).ToList();
        }

        public List<BudgetPeriodDTO> FilterBudgetByAccount(List<BudgetPeriodDTO> budgets)
        {
            return budgets.Where(budget => budget.BudgetRow != null &&
            ValidAccordingToFilter(new List<int?> { budget.BudgetRow.Dim1Id, budget.BudgetRow.Dim2Id, budget.BudgetRow.Dim3Id, budget.BudgetRow.Dim4Id, budget.BudgetRow.Dim5Id, budget.BudgetRow.Dim6Id })
            ).ToList();
        }

        public List<TimeSchedulePlanningDayDTO> FilterTemplateByAccount(List<TimeSchedulePlanningDayDTO> templates)
        {
            return templates.Where(template => template.AccountIds != null && this.ValidAccordingToFilterCached(template.GetAccountIdsIncludingAccountIdOnBlock())).ToList();
        }

        public List<TimePayrollTransactionDTO> FilterTransactionsByAccount(List<TimePayrollTransactionDTO> transactions)
        {
            var trans = transactions.Where(transaction => this.ValidAccordingToFilterCached(transaction.GetAccountInternalIds())).ToList();

            if (!trans.IsNullOrEmpty())
                return trans;

            return new List<TimePayrollTransactionDTO>();
        }

        public List<TimePayrollTransactionDTO> FilterTransactionsByAccount(Dictionary<string, List<TimePayrollTransactionDTO>> transactionsDict)
        {
            var validKeyValuePairs = transactionsDict.Where(transaction => this.ValidAccordingToFilterCached(GetAccountIdsFromKeyString(transaction.Key)));

            if (!validKeyValuePairs.IsNullOrEmpty())
                return validKeyValuePairs.SelectMany(sm => sm.Value).ToList();

            return new List<TimePayrollTransactionDTO>();
        }

        public List<TimeScheduleTemplateBlock> FilterTimeScheduleTemplateBlocksByAccount(Dictionary<string, List<TimeScheduleTemplateBlock>> timeScheduleTemplateBlockAccountingDict)
        {
            var validKeyValuePairs = timeScheduleTemplateBlockAccountingDict.Where(block => this.ValidAccordingToFilterCached(GetAccountIdsFromKeyString(block.Key)));
            var blocks = validKeyValuePairs.SelectMany(sm => sm.Value).ToList();

            if (!blocks.IsNullOrEmpty())
            {
                var employeeIds = blocks.Select(s => s.EmployeeId).Distinct().ToList();
                var valid = timeScheduleTemplateBlockAccountingDict.SelectMany(sm => sm.Value.Where(w => employeeIds.Contains(w.EmployeeId.Value) && w.IsBreak)).ToList();
                valid.AddRange(validKeyValuePairs.SelectMany(sm => sm.Value).ToList());
                return valid;
            }

            return new List<TimeScheduleTemplateBlock>();
        }

        public static List<int> GetAccountIdsFromKeyString(string key)
        {
            var asStrings = key.Split('_');
            var accountIds = new List<int>();
            foreach (var item in asStrings)
            {
                if (int.TryParse(item, out int value))
                    accountIds.Add(value);
            }

            return accountIds;
        }

        public List<TimeScheduleTemplateBlock> FilterTimeScheduleTemplateBlocksByAccount(List<TimeScheduleTemplateBlock> timeScheduleTemplateBlocks)
        {
            var blocks = timeScheduleTemplateBlocks.Where(block => block.IsBreak || (block.AccountInternal != null && this.ValidAccordingToFilterCached(block.GetAccountIdsIncludingAccountIdOnBlock()))).ToList();

            if (!blocks.IsNullOrEmpty())
                return blocks;

            return new List<TimeScheduleTemplateBlock>();
        }

        #endregion

        #endregion
    }

    public class StaffingStatisticsEngine : ManagerBase
    {
        #region Properties

        private StaffingStatisticsInput input;
        private List<StaffingStatisticsInterval> intervals;
        private StaffingStatisticsInterval currentInterval;

        #endregion

        #region Ctor

        public StaffingStatisticsEngine() : base(null) { }

        public StaffingStatisticsEngine(StaffingStatisticsInput input, ParameterObject parameterObject) : base(parameterObject)
        {
            this.input = input;
            this.intervals = new List<StaffingStatisticsInterval>();
        }

        #endregion

        public StaffingStatisticsEngine ReInitiate(StaffingStatisticsInput input)
        {
            this.input = input;
            intervals = new List<StaffingStatisticsInterval>();
            currentInterval = null;
            return this;
        }

        #region Public methods

        public void Calculate()
        {
            decimal staffingNeedRoundUp = SettingManager.GetDecimalSetting(SettingMainType.Company, (int)CompanySettingType.StaffingNeedRoundUp, 0, input.ActorCompanyId, 0);
            bool groupOnEmployee = true;
            if (this.input.EmployeeIds.IsNullOrEmpty())
            {
                groupOnEmployee = false;
                this.input.EmployeeIds = new List<int>() { 0 };
            }

            DateTime currrentDate = input.DateFrom;
            while (currrentDate <= input.DateTo)
            {
                foreach (var employeeId in this.input.EmployeeIds)
                {
                    this.currentInterval = new StaffingStatisticsInterval(currrentDate);

                    if (input.IntervalLength.Seconds == 59 && currrentDate.Add(TimeSpan.FromSeconds(1)) > input.DateTo)
                        continue;

                    if (input.CalculateNeed)
                        this.CalculateNeed();
                    if (input.CalculateFreqency)
                        this.CalculateFrequency();
                    if (input.CalculateRowFreqency)
                        this.CalculateRowFrequency(staffingNeedRoundUp);
                    if (input.CalculateBudget)
                        this.CalculateBudget(FrequencyType.Budget);
                    if (input.CalculateForecast)
                        this.CalculateBudget(FrequencyType.Forecast);
                    if (input.CalculateTemplate)
                        this.CalculateTemplate(groupOnEmployee ? employeeId : (int?)null);
                    if (input.CalculateTemplateForEmployeePost)
                        this.CalculateTemplateForEmployeePost();
                    if (input.CalculateSchedule)
                        this.CalculateSchedule(groupOnEmployee ? employeeId : (int?)null);
                    if (input.CalculateTime)
                        this.CalculateTime(groupOnEmployee ? employeeId : (int?)null);

                    currentInterval.EmployeeId = employeeId == 0 ? (int?)null : employeeId;

                    this.intervals.Add(this.currentInterval);
                }
                currrentDate = this.currentInterval.Interval.Add(input.IntervalLength);
            }

            if (input.AddSummaryRow) //TODO change
            {
                var intervalsBeforeToday = this.intervals.Where(w => w.Interval < DateTime.Today).ToList();
                var intervalsAfteroday = this.intervals.Where(w => w.Interval >= DateTime.Today).ToList();
            }

            if (input.AddSummaryRow)
            {
                // Important, add summary with date one day after specified interval.
                // Client side will use last date as summary.
                this.currentInterval = new StaffingStatisticsInterval(input.DateTo.AddDays(1));
                this.CalculateSummary();
                this.intervals.Add(this.currentInterval);
            }
        }

        public List<StaffingStatisticsInterval> GetIntervals()
        {
            return this.intervals;
        }

        public Dictionary<DateTime, List<StaffingStatisticsInterval>> GetIntervalsByDate()
        {
            Dictionary<DateTime, List<StaffingStatisticsInterval>> dict = new Dictionary<DateTime, List<StaffingStatisticsInterval>>();
            foreach (var intervalsGroupedByDate in this.intervals.GroupBy(i => i.Interval.Date))
            {
                if (dict.ContainsKey(intervalsGroupedByDate.Key))
                    dict.Add(intervalsGroupedByDate.Key, intervalsGroupedByDate.ToList());
            }
            return dict;
        }

        #endregion

        #region Calculation

        private void CalculateNeed()
        {
            List<StaffingNeedsRowPeriodDTO> needs = this.input.GetNeeds();
            if (needs == null)
                return;

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            needs = needs.Where(i => i.Date.HasValue && i.Date.Value == this.currentInterval.Interval.Date && !i.IsBreak).ToList();
            if (input.IsWeekView)
                needs = needs.Where(need => CalendarUtility.IsNewOverlappedByCurrent(CalendarUtility.MergeDateAndTime(need.Date.Value, need.TimeSlot.From, handleDefaultTimeAfterMidnight: true), CalendarUtility.MergeDateAndTime(need.Date.Value, need.TimeSlot.To, handleDefaultTimeAfterMidnight: true), intervalFrom, intervalTo)).ToList();
            else
                needs = needs.Where(need => CalendarUtility.IsNewOverlappedByCurrent(intervalFrom, intervalTo, CalendarUtility.MergeDateAndTime(need.Date.Value, need.TimeSlot.From, handleDefaultTimeAfterMidnight: true), CalendarUtility.MergeDateAndTime(need.Date.Value, need.TimeSlot.To, handleDefaultTimeAfterMidnight: true))).ToList();

            this.currentInterval.AddNeedValue(this.input.AccountId, GetFormulaOperandNeed(needs));
        }

        private void CalculateFrequency()
        {
            if (this.input.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return;

            List<StaffingNeedsFrequencyDTO> frequencys = this.input.GetFrequencys();
            if (frequencys == null)
                return;

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            this.currentInterval.AddFrequencyValue(this.input.AccountId, GetFormulaOperandFrequencyActualSales(intervalFrom, intervalTo, frequencys));
        }

        private void CalculateRowFrequency(decimal staffingNeedRoundUp)
        {
            List<StaffingNeedsRowFrequencyDTO> rowFrequencys = this.input.GetRowFrequencys();
            if (rowFrequencys == null)
                return;

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            rowFrequencys = rowFrequencys.Where(i => i.ActualStartTime.Date == this.currentInterval.Interval.Date).ToList();
            if (input.IsWeekView)
                rowFrequencys = rowFrequencys.Where(freq => CalendarUtility.IsNewOverlappedByCurrent(freq.ActualStartTime, freq.ActualStopTime, intervalFrom, intervalTo)).ToList();
            else
                rowFrequencys = rowFrequencys.Where(freq => CalendarUtility.IsNewOverlappedByCurrent(intervalFrom, intervalTo, freq.ActualStartTime, freq.ActualStopTime)).ToList();

            this.currentInterval.AddRowFrequencyValue(this.input.AccountId, GetFormulaOperandRowFrequencys(rowFrequencys, (int)this.input.IntervalLength.TotalMinutes));

            if ((this.input.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All && !this.input.IsWeekView) || this.input.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
            {
                List<StaffingNeedsRowFrequencyDTO> rowFrequencysRounded = new List<StaffingNeedsRowFrequencyDTO>();

                int index = -1;
                int count = rowFrequencys.Count();

                foreach (StaffingNeedsRowFrequencyDTO freq in rowFrequencys)
                {
                    index++;
                    StaffingNeedsRowFrequencyDTO clone = freq.CloneDTO();

                    decimal prevValue = 0;
                    decimal nextValue = 0;

                    StaffingNeedsRowFrequencyDTO prev = null;
                    StaffingNeedsRowFrequencyDTO next = null;

                    if (index > 0)
                        prev = rowFrequencys[index - 1];

                    if (index < count - 2)
                        next = rowFrequencys[index + 1];

                    if (prev != null)
                        prevValue = prev.Value;

                    if (next != null)
                        nextValue = next.Value;

                    var value = freq.Value;

                    if (prevValue != 0 && nextValue != 0)
                        value = Decimal.Divide((value + prevValue + nextValue), 3);
                    else if (prevValue != 0)
                        value = Decimal.Divide((prevValue + value), 2);
                    else if (nextValue != 0)
                        value = Decimal.Divide((nextValue + value), 2);

                    decimal decimalpart = value - Math.Truncate(value);
                    if (staffingNeedRoundUp < decimalpart)
                    {
                        clone.Value = clone.Value - decimalpart + 1;
                    }
                    else
                    {
                        clone.Value = Math.Truncate(clone.Value);
                    }
                    rowFrequencysRounded.Add(clone);
                }

                this.currentInterval.AddFrequencyValue(this.input.AccountId, GetFormulaOperandRowFrequencys(rowFrequencysRounded, (int)this.input.IntervalLength.TotalMinutes));
            }
        }

        private void CalculateBudgetFrequency()
        {
            if (this.input.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return;

            List<StaffingNeedsFrequencyDTO> frequencys = this.input.GetBudgetFrequencys();
            if (frequencys == null)
                return;

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, GetFormulaOperandFrequencyActualSales(intervalFrom, intervalTo, frequencys));
            this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandFrequencyMinutes(intervalFrom, intervalTo, frequencys));
            this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, GetFormulaOperandFrequencyCost(intervalFrom, intervalTo, frequencys));

            if (input.CalculateSalaryPercent)
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, GetFormulaOperandSalaryPercent(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
            if (input.CalculateLPAT)
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateFPAT)
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateBPAT)
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, GetFormulaOperandBPAT());
        }

        private void CalculateForecastFrequency()
        {
            if (this.input.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                return;

            List<StaffingNeedsFrequencyDTO> frequencys = this.input.GetForecastFrequencys();
            if (frequencys == null)
                return;

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, GetFormulaOperandFrequencyActualSales(intervalFrom, intervalTo, frequencys));
            this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandFrequencyMinutes(intervalFrom, intervalTo, frequencys));
            this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, GetFormulaOperandFrequencyCost(intervalFrom, intervalTo, frequencys));

            if (input.CalculateSalaryPercent)
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, GetFormulaOperandSalaryPercent(this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
            if (input.CalculateLPAT)
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateFPAT)
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateBPAT)
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, GetFormulaOperandBPAT());
        }

        private void CalculateBudgetRowFrequency(decimal staffingNeedRoundUp)
        {
            List<StaffingNeedsRowFrequencyDTO> rowFrequencys = this.input.GetRowBudgetFrequencys();
            if (rowFrequencys == null)
                return;

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            rowFrequencys = rowFrequencys.Where(i => i.ActualStartTime.Date == this.currentInterval.Interval.Date).ToList();
            if (input.IsWeekView)
                rowFrequencys = rowFrequencys.Where(freq => CalendarUtility.IsNewOverlappedByCurrent(freq.ActualStartTime, freq.ActualStopTime, intervalFrom, intervalTo)).ToList();
            else
                rowFrequencys = rowFrequencys.Where(freq => CalendarUtility.IsNewOverlappedByCurrent(intervalFrom, intervalTo, freq.ActualStartTime, freq.ActualStopTime)).ToList();

            this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, GetFormulaOperandRowFrequencys(rowFrequencys, (int)this.input.IntervalLength.TotalMinutes));

            if ((this.input.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.All && !this.input.IsWeekView) || this.input.CalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
            {
                List<StaffingNeedsRowFrequencyDTO> rowFrequencysRounded = new List<StaffingNeedsRowFrequencyDTO>();

                int index = -1;
                int count = rowFrequencys.Count();

                foreach (StaffingNeedsRowFrequencyDTO freq in rowFrequencys)
                {
                    index++;
                    StaffingNeedsRowFrequencyDTO clone = freq.CloneDTO();

                    decimal prevValue = 0;
                    decimal nextValue = 0;

                    StaffingNeedsRowFrequencyDTO prev = null;
                    StaffingNeedsRowFrequencyDTO next = null;

                    if (index > 0)
                        prev = rowFrequencys[index - 1];

                    if (index < count - 2)
                        next = rowFrequencys[index + 1];

                    if (prev != null)
                        prevValue = prev.Value;

                    if (next != null)
                        nextValue = next.Value;

                    var value = freq.Value;

                    if (prevValue != 0 && nextValue != 0)
                        value = Decimal.Divide((value + prevValue + nextValue), 3);
                    else if (prevValue != 0)
                        value = Decimal.Divide((prevValue + value), 2);
                    else if (nextValue != 0)
                        value = Decimal.Divide((nextValue + value), 2);

                    decimal decimalpart = value - Math.Truncate(value);
                    if (staffingNeedRoundUp < decimalpart)
                    {
                        clone.Value = clone.Value - decimalpart + 1;
                    }
                    else
                    {
                        clone.Value = Math.Truncate(clone.Value);
                    }
                    rowFrequencysRounded.Add(clone);
                }

                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandRowFrequencys(rowFrequencysRounded, (int)this.input.IntervalLength.TotalMinutes));
            }
        }

        private void CalculateBudget(FrequencyType frequencyType)
        {
            List<BudgetPeriodDTO> budgets = this.input.GetBudgets();
            if (budgets == null || !budgets.Any())
            {
                if (frequencyType == FrequencyType.Budget)
                {
                    this.CalculateBudgetFrequency();
                }
                else if (frequencyType == FrequencyType.Forecast)
                {
                    budgets = this.input.GetForecasts();
                    if (budgets == null || !budgets.Any())
                        this.CalculateForecastFrequency();
                }
            }
            else if (frequencyType == FrequencyType.Budget)
            {
                DateTime intervalFrom;
                DateTime intervalTo;
                input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

                if (input.IsWeekView || this.currentInterval.Interval.Minute == 0)
                {
                    if (input.IsWeekView)
                        budgets = budgets.Where(period => CalendarUtility.IsNewOverlappedByCurrent(period.StartDate.Value, period.GetStopTime().AddSeconds(-1), intervalFrom, intervalTo)).ToList();
                    else
                        budgets = budgets.Where(period => CalendarUtility.IsNewOverlappedByCurrent(intervalFrom, intervalTo, period.StartDate.Value, period.GetStopTime().AddSeconds(-1))).ToList();

                    if (input.CalculateHours)
                        this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandBudgetHours(budgets) * 60); // Minutes
                    if (input.CalculateSales)
                        this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, GetFormulaOperandBudgetSales(budgets));
                    if (input.CalculatePersonalCost)
                        this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, GetFormulaOperandBudgetPersonelCost(budgets));
                    if (input.CalculateSalaryPercent)
                        this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, GetFormulaOperandSalaryPercent(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
                    if (input.CalculateLPAT)
                        this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                    if (input.CalculateFPAT)
                        this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                    if (input.CalculateBPAT)
                        this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, GetFormulaOperandBPAT());
                }
                else if (!input.IsWeekView)
                {
                    StaffingStatisticsInterval previousInterval = this.GetPreviousInterval();
                    if (previousInterval != null)
                    {
                        foreach (StaffingStatisticsIntervalRow previousIntervalRow in previousInterval.Rows.Where(i => i.Budget != null).OrderBy(i => i.Key))
                        {
                            if (this.input.CalculateHours)
                                this.currentInterval.AddBudgetValue(previousIntervalRow.Key, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, previousIntervalRow.Budget.Hours);
                            if (this.input.CalculateSales)
                                this.currentInterval.AddBudgetValue(previousIntervalRow.Key, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, previousIntervalRow.Budget.Sales);
                            if (this.input.CalculatePersonalCost)
                                this.currentInterval.AddBudgetValue(previousIntervalRow.Key, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, previousIntervalRow.Budget.PersonelCost);
                            if (input.CalculateSalaryPercent)
                                this.currentInterval.AddBudgetValue(previousIntervalRow.Key, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, previousIntervalRow.Budget.SalaryPercent);
                            if (this.input.CalculateLPAT)
                                this.currentInterval.AddBudgetValue(previousIntervalRow.Key, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, previousIntervalRow.Budget.LPAT);
                            if (this.input.CalculateFPAT)
                                this.currentInterval.AddBudgetValue(previousIntervalRow.Key, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, previousIntervalRow.Budget.FPAT);
                            if (this.input.CalculateBPAT)
                                this.currentInterval.AddBudgetValue(previousIntervalRow.Key, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, previousIntervalRow.Budget.BPAT);
                        }
                    }
                }

            }
        }

        private void CalculateTemplate(int? employeeId)
        {
            Dictionary<int, List<WorkIntervalDTO>> templateByEmployee = this.input.GetTemplate();
            if (templateByEmployee == null)
                return;

            if (employeeId.HasValue)
                templateByEmployee = templateByEmployee.Any(a => a.Key == employeeId) ? templateByEmployee.Where(w => w.Key == employeeId.Value).ToDictionary(k => k.Key, v => v.Value ?? new List<WorkIntervalDTO>()) : new Dictionary<int, List<WorkIntervalDTO>>();

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            if (input.CalculateHours)
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandHours(intervalFrom, intervalTo, templateByEmployee));
            if (input.CalculateSales)
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, GetFormulaOperandSales());
            if (input.CalculatePersonalCost)
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, GetFormulaOperandPersonelCost(intervalFrom, intervalTo, templateByEmployee));
            if (input.CalculateSalaryPercent)
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, GetFormulaOperandSalaryPercent(this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
            if (input.CalculateLPAT)
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateFPAT)
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateBPAT)
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, GetFormulaOperandBPAT());
        }

        private void CalculateTemplateForEmployeePost()
        {
            Dictionary<int, List<WorkIntervalDTO>> templateByEmployeePost = this.input.GetTemplateForEmployeePost();
            if (templateByEmployeePost == null)
                return;

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);

            if (input.CalculateHours)
                this.currentInterval.AddTemplateForEmployeePostValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandHours(intervalFrom, intervalTo, templateByEmployeePost));
        }

        private void CalculateSchedule(int? employeeId)
        {
            Dictionary<int, List<WorkIntervalDTO>> scheduleByEmployee = this.input.GetSchedule();
            if (scheduleByEmployee == null)
                return;

            if (employeeId.HasValue)
                scheduleByEmployee = scheduleByEmployee.Any(a => a.Key == employeeId) ? scheduleByEmployee.Where(w => w.Key == employeeId.Value).ToDictionary(k => k.Key, v => v.Value ?? new List<WorkIntervalDTO>()) : new Dictionary<int, List<WorkIntervalDTO>>();

            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);


            if (input.CalculateHours)
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandHours(intervalFrom, intervalTo, scheduleByEmployee));
            if (input.CalculateSales)
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, GetFormulaOperandSales());
            if (input.CalculatePersonalCost)
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, GetFormulaOperandPersonelCost(intervalFrom, intervalTo, scheduleByEmployee));
            if (input.CalculateSalaryPercent)
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, GetFormulaOperandSalaryPercent(this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
            if (input.CalculateLPAT)
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateFPAT)
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateBPAT)
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, GetFormulaOperandBPAT());
        }

        private void CalculateTime(int? employeeId)
        {
            DateTime intervalFrom;
            DateTime intervalTo;
            input.GetIntervalDates(this.currentInterval.Interval, true, out intervalFrom, out intervalTo);
            Dictionary<int, List<WorkIntervalDTO>> scheduleByEmployee = this.input.GetSchedule();
            Dictionary<int, List<TimePayrollTransactionDTO>> transactionsByEmployee = this.input.GetTransactions(includeAbsenceTransactions: true);

            if (employeeId.HasValue)
            {
                scheduleByEmployee = scheduleByEmployee != null && scheduleByEmployee.Any(a => a.Key == employeeId) ? scheduleByEmployee.Where(w => w.Key == employeeId.Value).ToDictionary(k => k.Key, v => v.Value ?? new List<WorkIntervalDTO>()) : null;
                transactionsByEmployee = transactionsByEmployee != null && transactionsByEmployee.Any(a => a.Key == employeeId) ? transactionsByEmployee.Where(w => w.Key == employeeId.Value).ToDictionary(k => k.Key, v => v.Value ?? new List<TimePayrollTransactionDTO>()) : null;
            }

            if (transactionsByEmployee == null || !transactionsByEmployee.Any())
                return;

            Dictionary<int, TimePayrollTransactionDTO> transactionsWithTimeAndAbsenceCostDict = new Dictionary<int, TimePayrollTransactionDTO>();
            List<TimePayrollTransactionDTO> absenceWithAmount = new List<TimePayrollTransactionDTO>();
            int counter = 0;
            foreach (var transaction in transactionsByEmployee)
            {
                var employee = this.input.GetEmployee(transaction.Key);
                Dictionary<int, List<TimePayrollTransactionDTO>> dictOfOne = new Dictionary<int, List<TimePayrollTransactionDTO>>();
                dictOfOne.Add(transaction.Key, transaction.Value);
                var transactionsWithTimeAndAbsenceCostOnEmployee = AddTimePayrollTransactionsAbsenceWithCost(employee, dictOfOne, GetTimePayrollTransactionsActualTime(dictOfOne), this.input.PayrollProductSettings, this.input.PayrollGroups, intervalFrom.Date, intervalTo.Date);

                foreach (var transactionWithTimeAndAbsenceCostOnEmployee in transactionsWithTimeAndAbsenceCostOnEmployee)
                {
                    if (transactionWithTimeAndAbsenceCostOnEmployee.TimePayrollTransactionId == 0)
                        transactionsWithTimeAndAbsenceCostDict.Add(transactionWithTimeAndAbsenceCostOnEmployee.TimePayrollTransactionId + counter, transactionWithTimeAndAbsenceCostOnEmployee);
                    else if (!transactionsWithTimeAndAbsenceCostDict.ContainsKey(transactionWithTimeAndAbsenceCostOnEmployee.TimePayrollTransactionId))
                        transactionsWithTimeAndAbsenceCostDict.Add(transactionWithTimeAndAbsenceCostOnEmployee.TimePayrollTransactionId, transactionWithTimeAndAbsenceCostOnEmployee);
                    else
                        LogCollector.LogCollector.LogInfo($"CalculateTime() in StaffingStatisticsEngine Duplicate transaction actorcompanyid {this.input.ActorCompanyId}");
                    counter++;
                }

                absenceWithAmount.AddRange(transaction.Value.Where(w => w.IsAbsence() && w.Amount != 0));
            }

            List<TimePayrollTransactionDTO> transactionsWithTimeAndAbsenceCost = transactionsWithTimeAndAbsenceCostDict.Values.ToList();

            if (input.CalculateHours)
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, GetFormulaOperandTimeMinutes(intervalFrom, intervalTo, GetTimePayrollTransactionsActualTime(transactionsByEmployee).Where(t => !t.IsOBAddition()).ToList()));
            if (input.CalculateSales)
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, GetFormulaOperandSales());
            if (input.CalculatePersonalCost)
            {
                var value = GetFormulaOperandTimePersonalCost(this.input.ActorCompanyId, intervalFrom, intervalTo, transactionsWithTimeAndAbsenceCost, input.IncludeEmpTaxAndSuppCharge, absenceWithAmount, scheduleByEmployee);
                var scheduleValue = this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                var futureCost = value + scheduleValue;

                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, value, addScheduleAndTime: true, calculatedValue: (scheduleValue == 0 && DateTime.Today <= intervalFrom.Date ? futureCost : (decimal?)null));
            }
            if (input.CalculateSalaryPercent)
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, GetFormulaOperandSalaryPercent(this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
            if (input.CalculateLPAT)
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateFPAT)
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetFrequencyValue(this.input.AccountId), this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
            if (input.CalculateBPAT)
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, GetFormulaOperandBPAT());
        }

        public void ReCalculateKPIs(StaffingStatisticsIntervalRow row)
        {
            if (row.Forecast != null)
            {
                decimal sales = row.Forecast.Sales;
                decimal hours = Divide(row.Forecast.Hours, 60);   // Passed as minutes
                decimal personelCost = row.Forecast.PersonelCost;
                decimal salaryPercent = Divide(row.Forecast.SalaryPercent, 100);  // Passed as actual percent
                decimal lpat = row.Forecast.LPAT;
                decimal fpat = row.Forecast.FPAT;

                switch (row.ModifiedCalculationType)
                {
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Forecast.SetHours(GetFormulaOperandHours(sales, fpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Forecast.SetPersonelCost(GetFormulaOperandPersonelCost(salaryPercent, sales));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                            row.Forecast.SetSalaryPercent(GetFormulaOperandSalaryPercent(personelCost, sales));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                            row.Forecast.SetFPAT(GetFormulaOperandFPAT(sales, hours));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Forecast.SetSales(GetFormulaOperandSales(hours, fpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Forecast.SetPersonelCost(GetFormulaOperandPersonelCost2(hours, lpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                            row.Forecast.SetLPAT(GetFormulaOperandLPAT(personelCost, hours));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                            row.Forecast.SetFPAT(GetFormulaOperandFPAT(sales, hours));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Forecast.SetSales(GetFormulaOperandSales2(personelCost, salaryPercent));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Forecast.SetHours(GetFormulaOperandHours2(personelCost, lpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                            row.Forecast.SetSalaryPercent(GetFormulaOperandSalaryPercent(personelCost, sales));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                            row.Forecast.SetLPAT(GetFormulaOperandLPAT(personelCost, hours));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Forecast.SetSales(GetFormulaOperandSales2(personelCost, salaryPercent));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Forecast.SetPersonelCost(GetFormulaOperandPersonelCost(salaryPercent, sales));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Forecast.SetHours(GetFormulaOperandHours2(personelCost, lpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Forecast.SetPersonelCost(GetFormulaOperandPersonelCost2(hours, lpat));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Forecast.SetSales(GetFormulaOperandSales(hours, fpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Forecast.SetHours(GetFormulaOperandHours(sales, fpat));
                        break;
                }
            }

            if (row.Budget != null)
            {
                decimal sales = row.Budget.Sales;
                decimal hours = Divide(row.Budget.Hours, 60);   // Passed as minutes
                decimal personelCost = row.Budget.PersonelCost;
                decimal salaryPercent = Divide(row.Budget.SalaryPercent, 100);  // Passed as actual percent
                decimal lpat = row.Budget.LPAT;
                decimal fpat = row.Budget.FPAT;

                switch (row.ModifiedCalculationType)
                {
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Budget.SetHours(GetFormulaOperandHours(sales, fpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Budget.SetPersonelCost(GetFormulaOperandPersonelCost(salaryPercent, sales));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                            row.Budget.SetSalaryPercent(GetFormulaOperandSalaryPercent(personelCost, sales));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                            row.Budget.SetFPAT(GetFormulaOperandFPAT(sales, hours));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Budget.SetSales(GetFormulaOperandSales(hours, fpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Budget.SetPersonelCost(GetFormulaOperandPersonelCost2(hours, lpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                            row.Budget.SetLPAT(GetFormulaOperandLPAT(personelCost, hours));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT)
                            row.Budget.SetFPAT(GetFormulaOperandFPAT(sales, hours));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Budget.SetSales(GetFormulaOperandSales2(personelCost, salaryPercent));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Budget.SetHours(GetFormulaOperandHours2(personelCost, lpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent)
                            row.Budget.SetSalaryPercent(GetFormulaOperandSalaryPercent(personelCost, sales));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT)
                            row.Budget.SetLPAT(GetFormulaOperandLPAT(personelCost, hours));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Budget.SetSales(GetFormulaOperandSales2(personelCost, salaryPercent));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Budget.SetPersonelCost(GetFormulaOperandPersonelCost(salaryPercent, sales));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Budget.SetHours(GetFormulaOperandHours2(personelCost, lpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)
                            row.Budget.SetPersonelCost(GetFormulaOperandPersonelCost2(hours, lpat));
                        break;
                    case TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT:
                        if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
                            row.Budget.SetSales(GetFormulaOperandSales(hours, fpat));
                        else if (row.TargetCalculationType == TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
                            row.Budget.SetHours(GetFormulaOperandHours(sales, fpat));
                        break;
                }
            }
        }

        private void CalculateSummary()
        {
            if (input.CalculateHours)
            {
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.intervals.Sum(i => i.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)));
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.intervals.Sum(i => i.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)));
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.intervals.Sum(i => i.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)));
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.intervals.Sum(i => i.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)), false);
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.intervals.Sum(i => i.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)), false);
                this.currentInterval.AddTimeAndSchedulValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.intervals.Sum(i => i.GetScheduleAndTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)));
            }
            if (input.CalculateSales)
            {
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.intervals.Sum(i => i.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.intervals.Sum(i => i.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.intervals.Sum(i => i.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)));
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.intervals.Sum(i => i.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)), false);
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.intervals.Sum(i => i.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)), false);
            }
            if (input.CalculatePersonalCost)
            {
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.intervals.Sum(i => i.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)));
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.intervals.Sum(i => i.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)));
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.intervals.Sum(i => i.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)));
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.intervals.Sum(i => i.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)), false);
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.intervals.Sum(i => i.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)), false);
                this.currentInterval.AddTimeAndSchedulValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.intervals.Sum(i => i.GetScheduleAndTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost)));
            }
            if (input.CalculateSalaryPercent)
            {
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, Decimal.Divide(this.intervals.Sum(i => GetFormulaOperandSalaryPercent(i.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), i.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales))), this.intervals.Count));
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, Decimal.Divide(this.intervals.Sum(i => GetFormulaOperandSalaryPercent(i.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), i.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales))), this.intervals.Count));
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, Decimal.Divide(this.intervals.Sum(i => GetFormulaOperandSalaryPercent(i.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), i.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales))), this.intervals.Count));
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, Decimal.Divide(this.intervals.Sum(i => GetFormulaOperandSalaryPercent(i.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), i.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales))), this.intervals.Count), false);
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, Decimal.Divide(this.intervals.Sum(i => GetFormulaOperandSalaryPercent(i.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), i.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales))), this.intervals.Count), false);
            }
            if (input.CalculateLPAT)
            {
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)), false);
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, GetFormulaOperandLPAT(this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost), this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)), false);
            }
            if (input.CalculateFPAT)
            {
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)));
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)), false);
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, GetFormulaOperandFPAT(this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales), this.currentInterval.GetTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours)), false);
            }
            if (input.CalculateBPAT)
            {
                this.currentInterval.AddForecastValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.intervals.Sum(i => GetFormulaOperandBPAT()));
                this.currentInterval.AddBudgetValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.intervals.Sum(i => GetFormulaOperandBPAT()));
                this.currentInterval.AddTemplateValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.intervals.Sum(i => GetFormulaOperandBPAT()));
                this.currentInterval.AddScheduleValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.intervals.Sum(i => GetFormulaOperandBPAT()), false);
                this.currentInterval.AddTimeValue(this.input.AccountId, TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.intervals.Sum(i => GetFormulaOperandBPAT()), false);
            }
        }

        #endregion

        #region Formula operands

        #region Need

        /// <summary>
        /// Behov i antal personer per dag, men annars timmar per vecka...
        /// </summary>
        private int GetFormulaOperandNeed(List<StaffingNeedsRowPeriodDTO> needs)
        {
            return input.IsWeekView ? Convert.ToInt32(decimal.Divide(needs.Sum(i => i.TimeSlot.TimeSlotLength), 60)) : needs.Count;
        }


        #endregion

        #region Frequency

        /// <summary>
        /// Faktiskt försäljning (från import fil)
        /// </summary>
        private decimal GetFormulaOperandFrequencyActualSales(DateTime from, DateTime to, List<StaffingNeedsFrequencyDTO> frequencys)
        {
            decimal amount = 0;
            if (frequencys != null)
            {
                foreach (var frequency in frequencys.OrderBy(i => i.TimeFrom))
                {
                    TimeSpan time = CalendarUtility.GetNewTimeInCurrent(from, to, frequency.TimeFrom, frequency.TimeTo);

                    if (time.Seconds == 59)
                        time = time.Add(TimeSpan.FromSeconds(1));

                    if (time.TotalMinutes > 0)
                        amount += decimal.Multiply(frequency.AmountPerMinute, Convert.ToDecimal(time.TotalMinutes));
                }
            }
            return amount;
        }

        /// <summary>
        /// Minutes for forecast and budget
        /// </summary>
        private decimal GetFormulaOperandFrequencyMinutes(DateTime from, DateTime to, List<StaffingNeedsFrequencyDTO> frequencys)
        {
            decimal minutes = 0;
            if (frequencys != null)
            {
                foreach (var frequency in frequencys.OrderBy(i => i.TimeFrom))
                {
                    TimeSpan time = CalendarUtility.GetNewTimeInCurrent(from, to, frequency.TimeFrom, frequency.TimeTo);
                    if (time.TotalMinutes > 0)
                    {
                        var addMinutes = decimal.Multiply(frequency.MinutesPerMinute, Convert.ToDecimal(time.TotalMinutes));

                        if (addMinutes == 0)
                            addMinutes = decimal.Multiply(frequency.ItemsPerMinute, Convert.ToDecimal(time.TotalMinutes));

                        if (addMinutes != 0)
                            minutes += addMinutes;
                    }
                }
            }
            return minutes;
        }

        /// <summary>
        /// Minutes for forcast and budget
        /// </summary>
        private decimal GetFormulaOperandFrequencyCost(DateTime from, DateTime to, List<StaffingNeedsFrequencyDTO> frequencys)
        {
            decimal amount = 0;
            if (frequencys != null)
            {
                foreach (var frequency in frequencys.OrderBy(i => i.TimeFrom))
                {
                    TimeSpan time = CalendarUtility.GetNewTimeInCurrent(from, to, frequency.TimeFrom, frequency.TimeTo);
                    if (time.TotalMinutes > 0)
                        amount += decimal.Multiply(frequency.CostPerMinute, Convert.ToDecimal(time.TotalMinutes));
                }
            }
            return amount;
        }

        #endregion

        #region RowFrequency

        /// <summary>
        /// Behov i antal personer
        /// </summary>
        private decimal GetFormulaOperandRowFrequencys(List<StaffingNeedsRowFrequencyDTO> rowFrequencys, int interval)
        {
            if (rowFrequencys == null || rowFrequencys.Count == 0)
                return 0;

            int rowInterval = rowFrequencys[0].Interval;
            if (rowInterval == interval)
                return rowFrequencys.Sum(i => i.Value);
            else if (interval <= 60)    // Day view
                return rowFrequencys.Sum(i => i.Value) / (60 / interval);
            else                        // Week view
                return rowFrequencys.Sum(i => i.Value) / (60 / rowInterval);
        }

        #endregion

        #region Budget

        /// <summary>
        /// Budgeterade timmar
        /// </summary>
        private decimal GetFormulaOperandBudgetHours(List<BudgetPeriodDTO> budgets)
        {
            if (budgets == null || budgets.Count == 0)
                return 0;
            return Divide(budgets.GetQuantity(DistributionCodeBudgetType.SalesBudgetTime), input.RatioPerHour);
        }

        /// <summary>
        /// Budgeterad försäljning
        /// </summary>
        private decimal GetFormulaOperandBudgetSales(List<BudgetPeriodDTO> budgets)
        {
            if (budgets == null || budgets.Count == 0)
                return 0;
            return Divide(budgets.GetAmount(DistributionCodeBudgetType.SalesBudget), input.RatioPerHour);
        }

        /// <summary>
        /// Budgeterad lönekostnad
        /// </summary>
        private decimal GetFormulaOperandBudgetPersonelCost(List<BudgetPeriodDTO> budgets)
        {
            if (budgets == null || budgets.Count == 0)
                return 0;
            return budgets.GetAmount(DistributionCodeBudgetType.SalesBudgetSalaryCost);
        }

        /// <summary>
        /// Prognos timmar
        /// </summary>
        private decimal GetFormulaOperandForecastHours(List<BudgetPeriodDTO> forecasts)
        {
            if (forecasts == null || forecasts.Count == 0)
                return 0;
            return Divide(forecasts.GetQuantity(DistributionCodeBudgetType.SalesBudgetTime), input.RatioPerHour);
        }

        /// <summary>
        /// Prognos försäljning
        /// </summary>
        private decimal GetFormulaOperandForecastSales(List<BudgetPeriodDTO> forecasts)
        {
            if (forecasts == null || forecasts.Count == 0)
                return 0;
            return Divide(forecasts.GetAmount(DistributionCodeBudgetType.SalesBudget), input.RatioPerHour);
        }

        /// <summary>
        /// Prognos lönekostnad
        /// </summary>
        private decimal GetFormulaOperandForecastPersonelCost(List<BudgetPeriodDTO> forecasts)
        {
            if (forecasts == null || forecasts.Count == 0)
                return 0;
            return forecasts.GetAmount(DistributionCodeBudgetType.SalesBudgetSalaryCost);
        }

        /// <summary>
        /// Summering prognos per dag
        /// </summary>
        private decimal GetFormulaOperandForecastSumByDate(Dictionary<DateTime, List<StaffingStatisticsInterval>> intervalsByDate, DateTime date, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            decimal hours = 0;
            if (intervalsByDate != null && intervalsByDate.ContainsKey(date))
            {
                List<StaffingStatisticsInterval> intervals = intervalsByDate[this.currentInterval.Interval.Date];
                hours = intervals.Sum(i => i.SumForecastRow(calculationType));
            }
            return hours;
        }

        #endregion


        #region Time

        /// <summary>
        /// Utfall timmar. Bruttolön – Timlön, Tid – Arbetad schematid, Bruttolön – Mertid, Bruttolön – Övertidsersättning, Bruttolön – övertidstillägg, Tid – Nivå 4 (Mertid), Tid – Nivå 4 (Övertid)
        /// </summary>
        private decimal GetFormulaOperandTimeMinutes(DateTime from, DateTime to, List<TimePayrollTransactionDTO> transactions)
        {
            if (transactions.IsNullOrEmpty())
                return 0;

            decimal totalMinutes = 0;
            foreach (var groupOnTimeInterval in transactions.Where(i => i.Date.HasValue).GroupBy(g => $"{g.EmployeeId}#{g.Date}#{g.StartTime}#{g.StopTime}#{g.Quantity}")) // Making sure that we only count each interval once
            {
                TimePayrollTransactionDTO transaction = groupOnTimeInterval.First();
                TimeSpan time = CalendarUtility.GetNewTimeInCurrent(from, to, CalendarUtility.MergeDateAndTime(transaction.Date.Value, transaction.StartTime.HasValue ? transaction.StartTime.Value : from, handleDefaultTimeAfterMidnight: true), CalendarUtility.MergeDateAndTime(transaction.Date.Value, transaction.StopTime.HasValue ? transaction.StopTime.Value : to, handleDefaultTimeAfterMidnight: true));

                if (time.TotalMinutes == 0 && !transaction.StartTime.HasValue)
                    totalMinutes += transaction.Quantity;
                if (time.TotalMinutes > 0)
                    totalMinutes += (decimal)time.TotalMinutes;
            }
            return totalMinutes;
        }

        /// <summary>
        /// Utfall lönekostnad
        /// </summary>
        private decimal GetFormulaOperandTimePersonalCost(int actorCompanyId, DateTime from, DateTime to, List<TimePayrollTransactionDTO> transactions, bool includeEmpTaxAndSuppCharge, List<TimePayrollTransactionDTO> absenceWithAmount, Dictionary<int, List<WorkIntervalDTO>> scheduleWorkIntervals)
        {
            if (transactions == null || transactions.Count == 0)
                return 0;

            Dictionary<int, decimal> settingPayrollGroupMonthlyWorkTimeDict = new Dictionary<int, decimal>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var payrollGroups = this.input.PayrollGroups ?? base.GetPayrollGroupsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            var payrollGroupSettings = this.input.PayrollGroupSettings == null ? PayrollManager.GetPayrollGroupSettingsForCompany(actorCompanyId) : this.input.PayrollGroupSettings;
            var payrollGroupPriceTypes = this.input.PayrollGroupPriceTypes == null ? base.GetPayrollGroupPriceTypesForCompanyFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId)).ToDTOs(true).ToList() : this.input.PayrollGroupPriceTypes;
            var employmentPriceTypes = this.input.EmploymentPriceTypes == null ? EmployeeManager.GetEmploymentPriceTypesForCompany(entitiesReadOnly, actorCompanyId).ToDTOs(true, false).ToList() : this.input.EmploymentPriceTypes;
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var payrollGroupAccountStds = base.GetPayrollGroupAccountStdFromCache(entities, actorCompanyId);
            List<int> employeeIds = transactions.Select(s => s.EmployeeId).Distinct().ToList();
            bool usePayroll = base.UsePayroll(entities, actorCompanyId);
            var payrollProductSettingsWithDontIncludeInAbsenceCost = this.input.PayrollProductSettings.Where(a => a.DontIncludeInAbsenceCost).ToList();
            bool hasAnyDontIncludeInAbsenceCostSetting = payrollProductSettingsWithDontIncludeInAbsenceCost.Any();
            var payrollProductSettingsDoCalculateSupplementCharge = this.input.PayrollProductSettings.Where(a => a.CalculateSupplementCharge).ToList();
            bool hasCalculateSupplementChargeSetting = usePayroll && payrollProductSettingsDoCalculateSupplementCharge.Any();
            var employmentPriceTypesForCompanyDict = employmentPriceTypes.GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());
            var payrollProductReportSettings = input.PayrollProductReportSettings ?? (input.ReportId.HasValue ? base.GetPayrollProductReportSettingsForCompanyFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId)) : new List<PayrollProductReportSetting>());
            var intervalIsWholeMonth = CalendarUtility.GetBeginningOfMonth(input.SelectionDateFrom) == input.SelectionDateFrom && CalendarUtility.GetEndOfMonth(input.SelectionDateTo) == input.SelectionDateTo;
            DateTime endDate = to;

            decimal totalCost = 0;
            decimal totalEmploymentTax = 0;
            decimal totalEmploymentSupplementCharge = 0;
            if (transactions != null)
            {
                foreach (var transactionsByEmployee in transactions.GroupBy(i => i.EmployeeId))
                {
                    EmployeeDTO employee = this.input.GetEmployee(transactionsByEmployee.Key);
                    var dates = CalendarUtility.GetDates(from, to);
                    var filtered = transactionsByEmployee.Where(w => w.Date.HasValue && dates.Contains(w.Date.Value));
                    Dictionary<DateTime, decimal> sysPayrollPriceIntervalAmountDict = new Dictionary<DateTime, decimal>();
                    Dictionary<DateTime, SysPayrollPriceViewDTO> sysPayrollPriceViewDict = new Dictionary<DateTime, SysPayrollPriceViewDTO>();

                    if (filtered.IsNullOrEmpty())
                        continue;

                    DateTime? birthDate = CalendarUtility.GetBirthDateFromSecurityNumber(employee.SocialSec);

                    DateTime dateFrom = CalendarUtility.GetBeginningOfMonth(filtered.Min(m => m.Date.Value));
                    DateTime dateTo = CalendarUtility.GetEndOfDay(CalendarUtility.GetEndOfMonth(filtered.Max(m => m.Date.Value)));

                    decimal? devisor = null;
                    decimal deductionAmount = 0;
                    if (intervalIsWholeMonth)
                    {
                        var datesInterval = CalendarUtility.GetDates(input.SelectionDateFrom, input.SelectionDateTo);

                        if (absenceWithAmount.Any(a => a.EmployeeId == transactionsByEmployee.Key))
                        {
                            var transes = transactionsByEmployee.ToList();

                            foreach (var date in datesInterval)
                            {
                                if (CalendarUtility.GetBeginningOfMonth(date) == CalendarUtility.GetBeginningOfMonth(dateFrom))
                                {
                                    var absense = absenceWithAmount.Where(w => w.EmployeeId == transactionsByEmployee.Key && w.Date == date).ToList();
                                    var absenseCost = transactionsByEmployee.Where(w => w.IsAbsenceCost() && w.EmployeeId == transactionsByEmployee.Key && w.Date == date).ToList();

                                    foreach (var abs in absense)
                                    {
                                        using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                                        var product = base.GetPayrollProductFromCache(entitiesReadonly, CacheConfig.Company(actorCompanyId), abs.PayrollProductId);
                                        var employment = employee.GetEmployment(abs.Date);
                                        if (employment != null && employment.PayrollGroupId.HasValue)
                                        {
                                            int? payrollGroupId = employment.PayrollGroupId;
                                            var dontIncludeInAbsenceCost = payrollProductSettingsWithDontIncludeInAbsenceCost.GetSetting(payrollGroupId, abs.PayrollProductId)?.DontIncludeInAbsenceCost ?? false;

                                            if (!dontIncludeInAbsenceCost)
                                            {
                                                var matchingAbsenceCost = absenseCost.FirstOrDefault(f => f.StartTime == abs.StartTime && f.StopTime == abs.StopTime && f.Quantity == abs.Quantity);

                                                if (matchingAbsenceCost != null)
                                                {
                                                    var dontIncludeInAbsenceCostOnAbsenceCost = payrollProductSettingsWithDontIncludeInAbsenceCost.GetSetting(payrollGroupId, matchingAbsenceCost.PayrollProductId)?.DontIncludeInAbsenceCost ?? false;

                                                    if (dontIncludeInAbsenceCostOnAbsenceCost)
                                                        continue;

                                                }
                                                deductionAmount += abs.Amount;
                                            }
                                        }
                                    }
                                }
                            }
                            deductionAmount = -1 * deductionAmount;
                        }
                        List<TimePayrollTransactionDTO> workTimeTransactions = new List<TimePayrollTransactionDTO>();
                        if (input.AllTransactionsDict != null && input.AllTransactionsDict.TryGetValue(employee.EmployeeId, out List<TimePayrollTransactionDTO> value))
                        {
                            workTimeTransactions = value.Where(w => w.Date.HasValue && CalendarUtility.GetBeginningOfMonth(w.Date.Value) == CalendarUtility.GetBeginningOfMonth(dateFrom) && w.AttestStateId != 0 && w.Date >= dateFrom && w.Date <= dateTo && w.IsWorkTime()).ToList();
                            devisor = decimal.Divide(workTimeTransactions.Sum(s => s.Quantity), 60);

                            if (dateTo >= DateTime.Today && !workTimeTransactions.Any(a => a.Date >= DateTime.Today))
                            {
                                devisor = null;
                                deductionAmount = 0;
                            }
                        }
                        else if (!transactionsByEmployee.IsNullOrEmpty())
                        {
                            workTimeTransactions = transactionsByEmployee.Where(w => w.Date.HasValue && CalendarUtility.GetBeginningOfMonth(w.Date.Value) == CalendarUtility.GetBeginningOfMonth(dateFrom) && w.Date >= dateFrom && w.Date <= dateTo && w.IsWorkTime()).ToList();
                            devisor = decimal.Divide(workTimeTransactions.Sum(s => s.Quantity), 60);

                            if (dateTo >= DateTime.Today && !workTimeTransactions.Any(a => a.Date >= DateTime.Today))
                            {
                                devisor = null;
                                deductionAmount = 0;
                            }
                        }

                        if (devisor != null && devisor != 0 && scheduleWorkIntervals != null && scheduleWorkIntervals.TryGetValue(transactionsByEmployee.Key, out List<WorkIntervalDTO> employeeScheduleWorkIntervals))
                        {
                            if (employeeScheduleWorkIntervals != null && employeeScheduleWorkIntervals.Any())
                            {
                                var datesWithZeroSchedule = employeeScheduleWorkIntervals
                                                              .GroupBy(g => g.Date)
                                                              .Where(g => g.All(i => i.TotalMinutes == 0))
                                                              .Select(g => g.Key)
                                                              .Distinct()
                                                              .ToList();
                                foreach (var transDate in workTimeTransactions.GroupBy(g => g.Date.Value))
                                {
                                    if (datesWithZeroSchedule.Contains(transDate.Key))
                                    {
                                        devisor -= decimal.Divide(transDate.Sum(s => s.Quantity), 60);
                                    }
                                }
                            }
                        }
                    }

                    if (decimal.Round(devisor ?? 0, 4) == new decimal(0))
                        devisor = null;

                    var atleastOneWorkTimeTransactionsHasAmount = filtered.Where(w => w.IsWorkTime()).Any(a => a.Amount != 0);
                    var taxNextMonth = filtered.Any(w => w.IsHourlySalary());

                    var calculatedCostPerHourDict = PayrollManager.GetEmployeeHourlyPays(actorCompanyId, null, dateFrom, dateTo, payrollGroupSettings: payrollGroupSettings, payrollGroupPriceTypes: payrollGroupPriceTypes, employeeDTO: employee, employmentPriceTypesForCompanyDict: employmentPriceTypesForCompanyDict, devisor: devisor, fixedPayrollRows: input.FixedPayrollRows, iDTO: input.EvaluatePayrollPriceFormulaInputDTO, deductionAmount: deductionAmount);

                    foreach (TimePayrollTransactionDTO transaction in filtered)
                    {
                        decimal totalCostByEmployee = 0;
                        TimeSpan time = transaction.StartTime.HasValue ? CalendarUtility.GetNewTimeInCurrent(from, to, CalendarUtility.MergeDateAndTime(transaction.Date.Value, transaction.StartTime.Value, handleDefaultTimeAfterMidnight: true), CalendarUtility.MergeDateAndTime(transaction.Date.Value, transaction.StopTime.Value, handleDefaultTimeAfterMidnight: true)) : TimeSpan.FromMinutes((double)transaction.Quantity);

                        if (time.TotalMinutes > (double)transaction.Quantity || (input.IntervalLength.TotalMinutes > time.TotalMinutes && (transaction.IsOvertimeCompensation() || transaction.IsOverTimeAddition())))
                            time = TimeSpan.FromMinutes((double)transaction.Quantity);

                        if ((transaction.ScheduleTransaction == true || transaction.ForceTempAmountSupplementCharge) && (transaction.TempAmountEmploymentTax != 0 || transaction.TempAmountSupplementCharge != 0))
                        {
                            totalEmploymentSupplementCharge += transaction.TempAmountSupplementCharge;
                            totalEmploymentTax += transaction.TempAmountEmploymentTax;
                            totalCost += transaction.TempAmountEmploymentTax + transaction.TempAmountSupplementCharge;
                            continue;
                        }
                        else if (transaction.IsWorkForHourlyPay() || payrollProductReportSettings.IncludeInReport(input.ReportId, TermGroup_PayrollProductReportSettingType.StaffingStatistics_IsWorkTime, transaction.PayrollProductId, employee.GetEmployment(transaction.Date)?.PayrollGroupId))
                        {
                            if (time.TotalMinutes > 0 && transaction.Amount != 0 && transaction.TimeCodeTransaction != null && transaction.TimeCodeTransaction.Quantity != 0)
                                totalCostByEmployee += (transaction.Amount / transaction.TimeCodeTransaction.Quantity) * (decimal)time.TotalMinutes;
                            else if (time.TotalMinutes > 0 && transaction.Amount != 0 && transaction.TimeCodeTransactionId.HasValue && transaction.TimeCodeTransactionId != 0)
                                totalCostByEmployee += transaction.Amount;
                        }
                        else if (transaction.IsAbsenceCost() && transaction.Amount != 0)
                        {
                            bool dontIncludeInAbsenceCost = false;
                            bool doCalculateSupplementCharge = false;

                            EmploymentDTO employmentDTO = employee.GetEmployment(transaction.Date);
                            decimal deductEmploymentTaxAndSupplementCharge = 0;
                            if (hasAnyDontIncludeInAbsenceCostSetting && employmentDTO != null && payrollProductSettingsWithDontIncludeInAbsenceCost.Any(a => a.ProductId == transaction.PayrollProductId))
                            {
                                int? payrollGroupId = employmentDTO.PayrollGroupId;
                                dontIncludeInAbsenceCost = payrollProductSettingsWithDontIncludeInAbsenceCost.GetSetting(payrollGroupId, transaction.PayrollProductId)?.DontIncludeInAbsenceCost ?? false;
                                doCalculateSupplementCharge = hasCalculateSupplementChargeSetting ? payrollProductSettingsDoCalculateSupplementCharge.GetSetting(payrollGroupId, transaction.PayrollProductId)?.CalculateSupplementCharge ?? true : true;

                                if (dontIncludeInAbsenceCost && includeEmpTaxAndSuppCharge)
                                {
                                    var taxDate = transaction.Date ?? DateTime.Today;
                                    var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                    taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, taxDate) ?? taxDate : taxDate;
                                    var taxRate = PayrollManager.GetTaxRate(input.ActorCompanyId, taxDate, birthDate, (int)TermGroup_Languages.Swedish, PayrollManager.GetSysPayrollPriceInterval(input.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthDate.Value.Year, taxDate, (int)TermGroup_Languages.Swedish));
                                    var payrollPrice = PayrollManager.GetSysPayrollPriceInterval(input.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthDate.Value.Year, taxDate, (int)TermGroup_Languages.Swedish);
                                    decimal employmentTaxCost = PayrollManager.CalculateEmploymentTaxSimple(input.ActorCompanyId, taxDate, transaction.Amount, birthDate, taxRate: taxRate, payrollPrice);
                                    decimal supplementChargeCost = !usePayroll || !doCalculateSupplementCharge ? 0 : PayrollManager.CalculateSupplementChargeSE(input.ActorCompanyId, taxDate, transaction.Amount, payrollGroupId, birthDate, payrollPrice, payrollGroupAccountStds);
                                    deductEmploymentTaxAndSupplementCharge = employmentTaxCost + supplementChargeCost;
                                }
                            }
                            if (!dontIncludeInAbsenceCost)
                                totalCostByEmployee += (transaction.Amount - deductEmploymentTaxAndSupplementCharge);
                        }
                        else
                            continue;

                        decimal timefactor = 1;
                        if (atleastOneWorkTimeTransactionsHasAmount && transaction.IsWorkForHourlyPay() && totalCostByEmployee == 0 && base.UsePayroll(entitiesReadOnly, actorCompanyId))
                        {
                            continue; // If one transaction has amount and the other has not, we assume that everything has been calculated correctly and we should not add any more cost
                        }
                        else if (time.TotalMinutes > 0 && transaction.Amount == 0)
                        {
                            timefactor = transaction.Quantity != 0 ? decimal.Divide((decimal)time.TotalMinutes, transaction.Quantity) : 0;

                            decimal calculatedCostPerHour = 0;

                            if (transaction.Date.HasValue && calculatedCostPerHourDict.TryGetValue(transaction.Date.Value, out calculatedCostPerHour) && calculatedCostPerHour != 0)
                            {
                                decimal factor = 1;

                                if (transaction.IsOBAddition())
                                {
                                    if (transaction.IsOBAddition40())
                                        factor = new decimal(0.4);
                                    if (transaction.IsOBAddition50())
                                        factor = new decimal(0.5);
                                    if (transaction.IsOBAddition57())
                                        factor = new decimal(0.57);
                                    if (transaction.IsOBAddition70())
                                        factor = new decimal(0.7);
                                    if (transaction.IsOBAddition79())
                                        factor = new decimal(0.79);
                                    if (transaction.IsOBAddition100())
                                        factor = new decimal(1);
                                    if (transaction.IsOBAddition113())
                                        factor = new decimal(1.13);
                                }

                                if (transaction.IsOverTimeAddition())
                                {
                                    if (transaction.IsOvertimeAddition50())
                                        factor = new decimal(1.5);
                                    if (transaction.IsOvertimeAddition70())
                                        factor = new decimal(1.7);
                                    if (transaction.IsOvertimeAddition100())
                                        factor = new decimal(2);
                                }

                                if (transaction.IsOvertimeCompensation())
                                {
                                    if (transaction.IsOvertimeCompensation50())
                                        factor = new decimal(1.5);
                                    if (transaction.IsOvertimeCompensation70())
                                        factor = new decimal(1.7);
                                    if (transaction.IsOvertimeCompensation100())
                                        factor = new decimal(2);
                                }

                                var minutePay = decimal.Divide(decimal.Multiply(factor, calculatedCostPerHour), 60);
                                totalCostByEmployee += minutePay * (decimal)time.TotalMinutes;
                            }
                        }


                        if (includeEmpTaxAndSuppCharge && totalCostByEmployee != 0)
                        {
                            EmployeeDTO employeeDTO = input.GetEmployee(transactionsByEmployee.Key);
                            if (employeeDTO != null)
                            {
                                EmploymentDTO employmentDTO = employeeDTO.GetEmployment(from, to);
                                if (employmentDTO != null)
                                {

                                    int? payrollGroupId = employmentDTO.PayrollGroupId;
                                    var doCalculateSupplementCharge = hasCalculateSupplementChargeSetting ? payrollProductSettingsDoCalculateSupplementCharge.GetSetting(payrollGroupId, transaction.PayrollProductId)?.CalculateSupplementCharge ?? true : true;
                                    var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                    var taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, transaction.Date.Value) ?? transaction.Date.Value : transaction.Date.Value;
                                    birthDate = birthDate ?? DateTime.Today.AddYears(-40);
                                    var taxRate = PayrollManager.GetTaxRate(input.ActorCompanyId, taxDate, birthDate, (int)TermGroup_Languages.Swedish, PayrollManager.GetSysPayrollPriceInterval(input.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthDate.Value.Year, taxDate, (int)TermGroup_Languages.Swedish));
                                    var payrollPrice = PayrollManager.GetSysPayrollPriceInterval(input.ActorCompanyId, (int)TermGroup_SysPayrollPrice.SE_EmploymentTax, birthDate.Value.Year, taxDate, (int)TermGroup_Languages.Swedish);
                                    decimal employmentTaxCost = PayrollManager.CalculateEmploymentTaxSimple(input.ActorCompanyId, taxDate, totalCostByEmployee, birthDate, taxRate: taxRate, payrollPrice);
                                    decimal supplementChargeCost = !usePayroll || !doCalculateSupplementCharge ? 0 : PayrollManager.CalculateSupplementChargeSE(input.ActorCompanyId, taxDate, totalCostByEmployee, payrollGroupId, birthDate, payrollPrice, payrollGroupAccountStds);
                                    totalEmploymentTax += employmentTaxCost;
                                    totalEmploymentSupplementCharge += supplementChargeCost;
                                    totalCostByEmployee += (employmentTaxCost + supplementChargeCost);
                                }
                            }
                        }
                        if (timefactor != 1)
                            totalCostByEmployee = timefactor * totalCostByEmployee;

                        totalCost += totalCostByEmployee;
                    }
                }
            }
            var totalEmpTaxAndSuppCharge = totalEmploymentTax + totalEmploymentSupplementCharge;
            return totalCost;
        }

        #endregion

        #region Common

        private decimal Divide(decimal value1, decimal value2)
        {
            if (value1 == 0 || value2 == 0)
                return 0;
            else if (value2 == 1)
                return value1;
            else
                return Decimal.Divide(value1, value2);
        }

        private decimal Multiply(decimal value1, decimal value2)
        {
            if (value1 == 0 || value2 == 0)
                return 0;
            else if (value2 == 1)
                return value1;
            else
                return Decimal.Multiply(value1, value2);
        }

        /// <summary>
        /// Försäljning
        /// </summary>
        private decimal GetFormulaOperandSales()
        {
            return this.currentInterval.GetFrequencyValue(this.input.AccountId);
        }

        private decimal GetFormulaOperandSales(decimal hours, decimal fpat)
        {
            return Multiply(hours, fpat);
        }

        private decimal GetFormulaOperandSales2(decimal personelCost, decimal salaryPercent)
        {
            return Divide(personelCost, salaryPercent);
        }

        /// <summary>
        /// Schema timmar
        /// </summary>
        private decimal GetFormulaOperandHours(DateTime from, DateTime to, Dictionary<int, List<WorkIntervalDTO>> intervals)
        {
            if (intervals == null || intervals.Count == 0)
                return 0;
            return (decimal)GetFormulaOperandTime(from, to, intervals).TotalMinutes;
        }

        /// <summary>
        /// Timmar
        /// </summary>
        private TimeSpan GetFormulaOperandTime(DateTime from, DateTime to, Dictionary<int, List<WorkIntervalDTO>> employeeWorkIntervals)
        {
            TimeSpan totalTime = new TimeSpan();
            if (employeeWorkIntervals != null)
            {
                foreach (var pair in employeeWorkIntervals)
                {
                    foreach (WorkIntervalDTO workInterval in pair.Value)
                    {
                        if (workInterval.StartTime > to || workInterval.StopTime < from || workInterval.HasAbsence())
                            continue;

                        TimeSpan timeInInterval = CalendarUtility.GetNewTimeInCurrent(from, to, workInterval.StartTime, workInterval.StopTime);
                        if (timeInInterval.TotalMinutes > 0)
                        {
                            totalTime = totalTime.Add(timeInInterval);
                        }
                    }
                }
            }
            return totalTime;
        }

        /// <summary>
        /// Personalkostnad
        /// </summary>
        /// <returns></returns>
        private decimal GetFormulaOperandPersonelCost(DateTime from, DateTime to, Dictionary<int, List<WorkIntervalDTO>> workIntervalsByEmployee)
        {
            if (workIntervalsByEmployee.IsNullOrEmpty())
                return 0;

            decimal totalCost = 0;
            foreach (var pair in workIntervalsByEmployee)
            {
                foreach (WorkIntervalDTO workInterval in pair.Value)
                {
                    if (workInterval.GrossNetCost == null || workInterval.StartTime > to || workInterval.StopTime < from)
                        continue;

                    if (CalendarUtility.IsDatesOverlapping(from, to, workInterval.StartTime, workInterval.StopTime))
                        totalCost += workInterval.GrossNetCost.TotalCostIncEmpTaxAndSuppCharge;
                }
            }
            return totalCost;
        }

        private decimal GetFormulaOperandHours(decimal sales, decimal fpat)
        {
            return Multiply(Divide(sales, fpat), 60);
        }

        private decimal GetFormulaOperandHours2(decimal personelCost, decimal lpat)
        {
            return Multiply(Divide(personelCost, lpat), 60);
        }

        private decimal GetFormulaOperandPersonelCost(decimal salaryPercent, decimal sales)
        {
            return Multiply(salaryPercent, sales);
        }

        private decimal GetFormulaOperandPersonelCost2(decimal hours, decimal lpat)
        {
            return Multiply(hours, lpat);
        }

        /// <summary>
        /// Löneprocent
        /// </summary>
        private decimal GetFormulaOperandSalaryPercent(decimal cost, decimal sales)
        {
            if (cost <= 0 || sales <= 0)
                return 0;

            return Multiply(Divide(cost, sales), 100);
        }

        /// <summary>
        /// LPAT
        /// </summary>
        private decimal GetFormulaOperandLPAT(decimal cost, decimal hours)
        {
            return Divide(cost, hours);
        }

        /// <summary>
        /// FPAT
        /// </summary>
        private decimal GetFormulaOperandFPAT(decimal sales, decimal actualHours)
        {
            return Divide(sales, actualHours);
        }

        /// <summary>
        /// BPAT
        /// </summary>
        private decimal GetFormulaOperandBPAT()
        {
            // Not in version 1
            return 0;
        }

        #endregion

        #endregion

        #region Help-methods

        private StaffingStatisticsInterval GetPreviousInterval()
        {
            return intervals.OrderByDescending(i => i.Interval).FirstOrDefault();
        }

        private List<TimePayrollTransactionDTO> GetTimePayrollTransactionsActualTime(Dictionary<int, List<TimePayrollTransactionDTO>> transactionsByEmployee)
        {
            List<TimePayrollTransactionDTO> validTransactions = new List<TimePayrollTransactionDTO>();
            if (transactionsByEmployee != null)
            {
                foreach (var pair in transactionsByEmployee)
                {
                    List<TimePayrollTransactionDTO> transactions = pair.Value;
                    foreach (TimePayrollTransactionDTO transaction in transactions.Where(i => i.TimeCodeTransaction != null || i.TimeCodeTransactionId != null))
                    {
                        if (IsTransactionActualTime(transaction))
                            validTransactions.Add(transaction);
                    }
                }
            }
            return validTransactions;
        }

        private bool IsTransactionActualTime(TimePayrollTransactionDTO transaction)
        {
            if (transaction == null)
                return false;
            if (transaction.IsHourlySalary())
                return true;
            if (transaction.IsTimeScheduledTime())
                return true;
            if (transaction.IsAddedTime())
                return true;
            if (transaction.IsOvertimeCompensation())
                return true;
            if (transaction.IsOverTimeAddition())
                return true;
            if (transaction.IsTimeAccumulatorTimeOrAddedTime())
                return true;
            if (transaction.IsOBAddition())
                return true;

            if (!input.PayrollProductReportSettings.IsNullOrEmpty() && input.PayrollProductReportSettings.IncludeInReport(input.ReportId, TermGroup_PayrollProductReportSettingType.StaffingStatistics_IsWorkTime, transaction.PayrollProductId, input.GetEmployee(transaction.EmployeeId).GetEmployment(transaction.Date)?.PayrollGroupId))
                return true;

            return false;
        }

        private List<TimePayrollTransactionDTO> AddTimePayrollTransactionsAbsenceWithCost(EmployeeDTO employeeDTO, Dictionary<int, List<TimePayrollTransactionDTO>> transactionsByEmployee, List<TimePayrollTransactionDTO> timeTransactions, List<PayrollProductSettingDTO> payrollProductSettings, List<PayrollGroup> payrollGroups, DateTime selectionDateFrom, DateTime selectionDateTo)
        {
            List<TimePayrollTransactionDTO> validTransactions = new List<TimePayrollTransactionDTO>();
            validTransactions.AddRange(timeTransactions.Where(w => w.ScheduleTransaction == null || w.ScheduleTransaction == false));
            DateTime? birthDate = CalendarUtility.GetBirthDateFromSecurityNumber(employeeDTO.SocialSec);
            var payrollProductSettingsWithDontIncludeInAbsenceCost = payrollProductSettings.Where(a => a.DontIncludeInAbsenceCost).ToList();
            var payrollProductSettingsWithCalculateSupplementCharge = payrollProductSettings.Where(a => a.CalculateSupplementCharge).ToList();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (transactionsByEmployee != null)
            {
                foreach (var pair in transactionsByEmployee)
                {
                    List<TimePayrollTransactionDTO> transactions = pair.Value;
                    foreach (TimePayrollTransactionDTO transaction in transactions.Where(i => i.EmployeeId == pair.Key))
                    {
                        if (transaction.IsAbsenceCost() && transaction.Amount != 0 && (transaction.TimeCodeTransaction != null || transaction.TimeCodeTransactionId != null))
                        {
                            bool dontIncludeInAbsenceCost = false;
                            var hasSetting = payrollProductSettingsWithDontIncludeInAbsenceCost.Any(a => a.ProductId == transaction.PayrollProductId);
                            var hasCalculateSupplementCharge = !base.UsePayroll(entitiesReadOnly, ActorCompanyId) || payrollProductSettingsWithCalculateSupplementCharge.Any(a => a.ProductId == transaction.PayrollProductId);

                            EmploymentDTO employmentDTO = hasSetting ? employeeDTO.GetEmployment(transaction.Date) : null;
                            if (employmentDTO != null)
                            {
                                int? payrollGroupId = employmentDTO.PayrollGroupId;
                                dontIncludeInAbsenceCost = payrollProductSettings.GetSetting(payrollGroupId, transaction.PayrollProductId)?.DontIncludeInAbsenceCost ?? false;
                                if (input.IncludeEmpTaxAndSuppCharge)
                                {
                                    var calcSupplementCharge = hasCalculateSupplementCharge ? payrollProductSettings.GetSetting(payrollGroupId, transaction.PayrollProductId)?.CalculateSupplementCharge ?? true : true;
                                    var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                    var taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, transaction.Date.Value) ?? transaction.Date.Value : transaction.Date.Value;
                                    decimal employmentTaxCost = PayrollManager.CalculateEmploymentTaxSimple(input.ActorCompanyId, taxDate, transaction.Amount, birthDate);
                                    decimal supplementChargeCost = 0;

                                    if (calcSupplementCharge)

                                        supplementChargeCost = PayrollManager.CalculateSupplementChargeSE(input.ActorCompanyId, taxDate, transaction.Amount, payrollGroupId, birthDate, null, base.GetPayrollGroupAccountStdFromCache(entitiesReadOnly, employeeDTO.ActorCompanyId));

                                    transaction.TempAmountEmploymentTax = -1 * employmentTaxCost;
                                    transaction.TempAmountSupplementCharge = -1 * supplementChargeCost;
                                }
                            }
                            if (!dontIncludeInAbsenceCost && !validTransactions.Contains(transaction))
                                validTransactions.Add(transaction);
                        }
                        else if (input.IncludeEmpTaxAndSuppCharge && transaction.IsAbsence() && !transaction.ScheduleTransaction.HasValue && !transaction.IsAbsenceCost())
                        {
                            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                            var product = base.GetPayrollProductsFromCache(entities, CacheConfig.Company(ActorCompanyId)).FirstOrDefault(f => f.ProductId == transaction.PayrollProductId);
                            EmploymentDTO employmentDTO = employeeDTO.GetEmployment(transaction.Date);
                            if (product != null && employmentDTO != null)
                            {
                                int? payrollGroupId = employmentDTO.PayrollGroupId;
                                if (!payrollProductSettings.GetSetting(payrollGroupId, transaction.PayrollProductId)?.DontIncludeInAbsenceCost ?? false)
                                {
                                    var calculateSupplementCharge = payrollProductSettings.GetSetting(employmentDTO.PayrollGroupId, transaction.PayrollProductId).CalculateSupplementCharge;

                                    if (!calculateSupplementCharge)
                                    {
                                        var matchingTransaction = transactions.FirstOrDefault(f => f.IsAbsenceCost() && f.Date == transaction.Date && f.Quantity == transaction.Quantity && f.StartTime == transaction.StartTime && f.PayrollProductId != transaction.PayrollProductId);

                                        if (matchingTransaction == null)
                                        {
                                            var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                            var taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, transaction.Date.Value) ?? transaction.Date.Value : transaction.Date.Value;
                                                                   transaction.TempAmountSupplementCharge = -1 * PayrollManager.CalculateSupplementChargeSE(input.ActorCompanyId, taxDate, transaction.Amount, payrollGroupId, birthDate, null, base.GetPayrollGroupAccountStdFromCache(entitiesReadOnly, employeeDTO.ActorCompanyId));
                                            transaction.ForceTempAmountSupplementCharge = true;
                                            if (!validTransactions.Contains(transaction))
                                                validTransactions.Add(transaction);
                                        }
                                    }
                                }
                            }
                        }

                        // If this not a absence transaction but a schedule transaction, we need to check if supplmentcharge should be calculated on the schedule transaction based on settings of the absence transaction payroll product
                             if (transaction.Date >= selectionDateFrom && transaction.Date <= selectionDateTo && input.IncludeEmpTaxAndSuppCharge && base.UsePayroll(entitiesReadOnly, ActorCompanyId) && transaction.ScheduleTransaction == true && IsTransactionActualTime(transaction) && transaction.Amount != 0 && !transaction.IsAbsence())
                        {

                            //find matching absence transaction based on times
                            var matchingTransaction = transactions.FirstOrDefault(f => f.StartTime == transaction.StartTime && transaction.ScheduleTransaction == true && transaction != f && transaction.PayrollProductId != f.PayrollProductId &&
                            transaction.SysPayrollTypeLevel1 == f.SysPayrollTypeLevel1 &&
                            transaction.SysPayrollTypeLevel2 == f.SysPayrollTypeLevel2 &&
                            transaction.SysPayrollTypeLevel3 == f.SysPayrollTypeLevel3 &&
                            transaction.SysPayrollTypeLevel4 == f.SysPayrollTypeLevel4);

                            if (matchingTransaction == null)
                            {
                                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                                var products = base.GetPayrollProductsFromCache(entities, CacheConfig.Company(ActorCompanyId));
                                var product = products.FirstOrDefault(f => f.ProductId == transaction.PayrollProductId);

                                if (product != null)
                                    //find matching absence transaction based on times
                                    matchingTransaction = transactions.FirstOrDefault(f => f.StartTime == transaction.StartTime && transaction.ScheduleTransaction == true && transaction != f && transaction.PayrollProductId != f.PayrollProductId &&
                                    product.SysPayrollTypeLevel1 == f.SysPayrollTypeLevel1 &&
                                    product.SysPayrollTypeLevel2 == f.SysPayrollTypeLevel2 &&
                                    product.SysPayrollTypeLevel3 == f.SysPayrollTypeLevel3 &&
                                    product.SysPayrollTypeLevel4 == f.SysPayrollTypeLevel4);

                                if (matchingTransaction == null && product != null)
                                {
                                    var matchingProducts = products.Where(w => w != product && w.SysPayrollTypeLevel1 == product.SysPayrollTypeLevel1 && w.SysPayrollTypeLevel2 == product.SysPayrollTypeLevel2 && w.SysPayrollTypeLevel3 == product.SysPayrollTypeLevel3 && w.SysPayrollTypeLevel4 == product.SysPayrollTypeLevel4).ToList();

                                    if (!matchingProducts.IsNullOrEmpty())
                                        matchingTransaction = transactions.FirstOrDefault(f => f.StartTime == transaction.StartTime && transaction.ScheduleTransaction == true && matchingProducts.Select(s => s.ProductId).Contains(f.PayrollProductId));
                                }
                            }

                            // if there are only one type of absence on the same date, use that absence transaction
                            //if (matchingTransaction == null)
                            //{
                            //    var absencetransactionsOnDate = transactions.Where(w => w.Date == transaction.Date && w.IsAbsenceCost()).ToList();

                            //    if (absencetransactionsOnDate.GroupBy(g => g.SysPayrollTypeLevel3).Count() == 1)
                            //        matchingTransaction = absencetransactionsOnDate[0];
                            //}

                            //if (matchingTransaction == null) // if no matching absence transaction is found, find all absence transactions on the same date and check if they have the same payroll type (not optimal)
                            //    matchingTransaction = transactions.FirstOrDefault(f => f.EmployeeId == transaction.EmployeeId && f.Date == transaction.Date && f.IsAbsenceCost() && f.Quantity == transaction.Quantity);

                            if (matchingTransaction != null)
                            {
                                EmploymentDTO employmentDTO = employeeDTO.GetEmployment(matchingTransaction.Date);
                                if (employmentDTO != null)
                                {
                                    int? payrollGroupId = employmentDTO.PayrollGroupId;

                                    if (payrollGroupId.HasValue)// && !(payrollProductSettings.GetSetting(payrollGroupId, transaction.PayrollProductId)?.DontIncludeInAbsenceCost ?? false))
                                    {
                                        var calcSupplementChargeOnMatchingTransaction = payrollProductSettings.GetSetting(payrollGroupId, matchingTransaction.PayrollProductId)?.CalculateSupplementCharge ?? true;
                                        var calcSupplementChargeOnScheduleTransaction = payrollProductSettings.GetSetting(payrollGroupId, transaction.PayrollProductId)?.CalculateSupplementCharge ?? true;
                                        // if the transaction thtat is matched is not supplment charge generating, we should calculate supplment charge on the schedule transaction
                                        // this means that it is mostly paid vacation that is matched in this condition
                                        if (!calcSupplementChargeOnMatchingTransaction && calcSupplementChargeOnScheduleTransaction)
                                        {
                                            var payrollGroup = payrollGroupId.HasValue && payrollGroups != null ? payrollGroups.FirstOrDefault(f => f.PayrollGroupId == payrollGroupId.Value) : null;
                                            var taxDate = payrollGroupId.HasValue ? PayrollManager.GetPaymentDate(payrollGroup, transaction.Date.Value) ?? transaction.Date.Value : transaction.Date.Value;
                                            decimal supplementChargeCost = PayrollManager.CalculateSupplementChargeSE(input.ActorCompanyId, taxDate, transaction.Amount, payrollGroupId, birthDate, null, base.GetPayrollGroupAccountStdFromCache(entitiesReadOnly, employeeDTO.ActorCompanyId));
                                            transaction.TempAmountSupplementCharge = supplementChargeCost;
                                        }

                                    }
                                }
                                validTransactions.Add(transaction);
                            }
                        }
                    }
                }
            }
            return validTransactions;
        }

        #endregion
    }
}

