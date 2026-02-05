using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeAccumulatorItem
    {
        //In-params
        public int TimeAccumulatorId { get; set; }
        public string Name { get; set; }
        [TsIgnore]
        public TermGroup_TimeAccumulatorType Type { get; set; }
        [TsIgnore]
        public bool FinalSalary { get; set; }
        [TsIgnore]
        public int? TimeCodeId { get; set; }
        [TsIgnore]
        public TimePeriodDTO TimePeriod { get; set; }
        [TsIgnore]
        public TimePeriodHeadDTO TimeAccumulatorTimePeriodHead { get; set; }
        [TsIgnore]
        public int EmployeeId { get; }
        [TsIgnore]
        public DateTime StartDate { get; }
        [TsIgnore]
        public DateTime StopDate { get; }
        [TsIgnore]
        public bool IsMultipleDays { get; }
        [TsIgnore]
        public bool IsMoreThanOneWeek { get; }
        [TsIgnore]
        public bool IncludeBalanceYear { get; private set; }
        [TsIgnore]
        public bool CalculateDay { get; }
        [TsIgnore]
        public bool CalculatePeriod { get; }
        [TsIgnore]
        public bool CalculatePlanningPeriod { get; }
        [TsIgnore]
        public bool CalculateYear { get; }
        [TsIgnore]
        public bool CalculateAccToday { get; }
        [TsIgnore]
        public bool CalculateRange { get; }

        //AccToday
        public DateTime? AccTodayStartDate { get; set; }
        public DateTime? AccTodayStopDate { get; set; }

        //Period
        public string TimePeriodName { get; set; }
        public string TimePeriodDatesText { get; set; }
        public bool HasTimePeriod { get; set; }

        //PlanningPeriod
        public string PlanningPeriodName { get; set; }
        public string PlanningPeriodDatesText { get; set; }
        public bool HasPlanningPeriod { get; set; }
        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }

        //Sums
        public decimal SumToday { get; set; }
        public decimal SumPeriod { get; set; }
        [TsIgnore]
        public decimal SumRange { get; set; }
        [TsIgnore]
        public decimal? SumRangeValue { get; set; }
        public decimal SumPlanningPeriod { get; set; }
        public decimal SumAccToday { get; set; }
        public decimal? SumAccTodayValue { get; set; }
        public decimal SumYear { get; set; }
        public decimal SumYearWithIB { get; set; }
        public bool SumTodayIsQuantity { get; set; }
        public bool SumPeriodIsQuantity { get; set; }
        public bool SumPlanningPeriodIsQuantity { get; set; }
        public bool SumAccTodayIsQuantity { get; set; }
        public bool SumYearIsQuantity { get; set; }

        //TimeAccumulatorEmployeeGroupRule
        [TsIgnore]
        public TermGroup_AccumulatorTimePeriodType EmployeeGroupRuleType { get; set; }
        [TsIgnore]
        public int? EmployeeGroupRuleMinMinutes { get; set; }
        [TsIgnore]
        public int? EmployeeGroupRuleMinMinutesWarning { get; set; }
        [TsIgnore]
        public int? EmployeeGroupRuleMaxMinutes { get; set; }
        [TsIgnore]
        public int? EmployeeGroupRuleMaxMinutesWarning { get; set; }
        public string EmployeeGroupRuleBoundaries { get; set; }
        public List<TimeAccumulatorRuleItem> EmployeeGroupRules { get; set; }

        // TimeCodeTransaction
        [TsIgnore]
        public TimeAccumulatorEmployeeCalculation<TimeAccumulatorCalculationTimeCodeRow> TimeCodeCalculation { get; set; }
        public decimal SumTimeCodeToday { get; set; }
        public decimal SumTimeCodePeriod { get; set; }
        public decimal SumTimeCodePlanningPeriod { get; set; }
        public decimal SumTimeCodeYear { get; set; }
        public decimal SumTimeCodeAccToday { get; set; }
        [TsIgnore]
        public decimal SumTimeCodeRange { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumTimeCodeTodayByTimeCode { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumTimeCodePeriodByTimeCode { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumTimeCodePlanningPeriodByTimeCode { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumTimeCodeYearByTimeCode { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumTimeCodeAccTodayByTimeCode { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumTimeCodeRangeByTimeCode { get; set; }
        public void UpdateTimeCode(TermGroup_AccumulatorTimePeriodType periodType, TimeAccumulatorCalculationTimeCodeResult result)
        {
            switch (periodType)
            {
                case TermGroup_AccumulatorTimePeriodType.Day:
                    this.SumTimeCodeToday = result.Sum;
                    this.SumTimeCodeTodayByTimeCode = result.SumByTimeCode;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Period:
                    this.SumTimeCodePeriod = result.Sum;
                    this.SumTimeCodePeriodByTimeCode = result.SumByTimeCode;
                    break;
                case TermGroup_AccumulatorTimePeriodType.PlanningPeriod:
                    this.SumTimeCodePlanningPeriod = result.Sum;
                    this.SumTimeCodePlanningPeriodByTimeCode = result.SumByTimeCode;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Year:
                    this.SumTimeCodeYear = result.Sum;
                    this.SumTimeCodeYearByTimeCode = result.SumByTimeCode;
                    break;
                case TermGroup_AccumulatorTimePeriodType.AccToday:
                    this.SumTimeCodeAccToday = result.Sum;
                    this.SumTimeCodeAccTodayByTimeCode = result.SumByTimeCode;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Range:
                    this.SumTimeCodeRange = result.Sum;
                    this.SumTimeCodeRangeByTimeCode = result.SumByTimeCode;
                    break;
            }
        }

        // TimePayrollTransaction
        [TsIgnore]
        public TimeAccumulatorEmployeeCalculation<TimeAccumulatorCalculationPayrollProductRow> TimePayrollCalculation { get; set; }
        public decimal SumPayrollToday { get; set; }
        public decimal SumPayrollPeriod { get; set; }
        public decimal SumPayrollPlanningPeriod { get; set; }
        public decimal SumPayrollYear { get; set; }
        public decimal SumPayrollAccToday { get; set; }
        [TsIgnore]
        public decimal SumPayrollRange { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumPayrollTodayByPayrollProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumPayrollPeriodByPayrollProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumPayrollPlanningPeriodByPayrollProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumPayrollYearByPayrollProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumPayrollAccTodayByPayrollProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumPayrollRangeByPayrollProduct { get; set; }
        public void UpdatePayrollProduct(TermGroup_AccumulatorTimePeriodType periodType, TimeAccumulatorCalculationPayrollProductResult result)
        {
            switch (periodType)
            {
                case TermGroup_AccumulatorTimePeriodType.Day:
                    this.SumPayrollToday = result.Sum;
                    this.SumPayrollTodayByPayrollProduct = result.SumByPayrollProduct;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Period:
                    this.SumPayrollPeriod = result.Sum;
                    this.SumPayrollPeriodByPayrollProduct = result.SumByPayrollProduct;
                    break;
                case TermGroup_AccumulatorTimePeriodType.PlanningPeriod:
                    this.SumPayrollPlanningPeriod = result.Sum;
                    this.SumPayrollPlanningPeriodByPayrollProduct = result.SumByPayrollProduct;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Year:
                    this.SumPayrollYear = result.Sum;
                    this.SumPayrollYearByPayrollProduct = result.SumByPayrollProduct;
                    break;
                case TermGroup_AccumulatorTimePeriodType.AccToday:
                    this.SumPayrollAccToday = result.Sum;
                    this.SumPayrollAccTodayByPayrollProduct = result.SumByPayrollProduct;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Range:
                    this.SumPayrollRange = result.Sum;
                    this.SumPayrollRangeByPayrollProduct = result.SumByPayrollProduct;
                    break;
            }
        }

        //Time InvoiceTransaction
        [TsIgnore]
        public TimeAccumulatorEmployeeCalculation<TimeAccumulatorCalculationInvoiceProductRow> TimeInvoiceCalculation { get; set; }
        public decimal SumInvoiceToday { get; set; }
        public decimal SumInvoicePeriod { get; set; }
        public decimal SumInvoicePlanningPeriod { get; set; }
        public decimal SumInvoiceYear { get; set; }
        public decimal SumInvoiceAccToday { get; set; }
        [TsIgnore]
        public decimal SumInvoiceRange { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumInvoiceTodayByInvoiceProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumInvoicePeriodByInvoiceProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumInvoicePlanningPeriodByInvoiceProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumInvoiceYearByInvoiceProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumInvoiceAccTodayByInvoiceProduct { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> SumInvoiceRangeByInvoiceProduct { get; set; }
        public void UpdateInvoiceProduct(TermGroup_AccumulatorTimePeriodType periodType, TimeAccumulatorCalculationInvoiceProductResult result)
        {
            switch (periodType)
            {
                case TermGroup_AccumulatorTimePeriodType.Day:
                    this.SumInvoiceToday = result.Sum;
                    this.SumInvoiceTodayByInvoiceProduct = result.SumByInvoiceProductId;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Period:
                    this.SumInvoicePeriod = result.Sum;
                    this.SumInvoicePeriodByInvoiceProduct = result.SumByInvoiceProductId;
                    break;
                case TermGroup_AccumulatorTimePeriodType.PlanningPeriod:
                    this.SumInvoicePlanningPeriod = result.Sum;
                    this.SumInvoicePlanningPeriodByInvoiceProduct = result.SumByInvoiceProductId;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Year:
                    this.SumInvoiceYear = result.Sum;
                    this.SumInvoiceYearByInvoiceProduct = result.SumByInvoiceProductId;
                    break;
                case TermGroup_AccumulatorTimePeriodType.AccToday:
                    this.SumInvoiceAccToday = result.Sum;
                    this.SumInvoiceAccTodayByInvoiceProduct = result.SumByInvoiceProductId;
                    break;
                case TermGroup_AccumulatorTimePeriodType.Range:
                    this.SumInvoiceRange = result.Sum;
                    this.SumInvoiceRangeByInvoiceProduct = result.SumByInvoiceProductId;
                    break;
            }
        }

        // TimeAccumulatorBalance
        public decimal TimeAccumulatorBalanceYear { get; set; }

        //Factors
        [TsIgnore]
        public Dictionary<int, decimal> TimeCodeIds { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> InvoiceProductIds { get; set; }
        [TsIgnore]
        public Dictionary<int, decimal> PayrollProductIds { get; set; }

        public TimeAccumulatorItem()
        {
            this.InvoiceProductIds = new Dictionary<int, decimal>();
            this.PayrollProductIds = new Dictionary<int, decimal>();
            this.TimeCodeIds = new Dictionary<int, decimal>();
        }

        public TimeAccumulatorItem(TimeAccumulatorDTO timeAccumulator, TimePeriodDTO timePeriod, TimePeriodHeadDTO timeAccumulatorTimePeriodHead, int employeeId, DateTime startDate, DateTime stopDate, bool calculateDay, bool calculatePeriod, bool calculatePlanningPeriod, bool calculateYear, bool calculateAccToday, bool calculateRange, bool includeBalanceYear) : this()
        {
            this.TimeAccumulatorId = timeAccumulator.TimeAccumulatorId;
            this.Name = timeAccumulator.Name;
            this.Type = timeAccumulator.Type;
            this.FinalSalary = timeAccumulator.FinalSalary;
            this.TimeCodeId = timeAccumulator.TimeCodeId;
            this.TimePeriod = timePeriod;
            this.TimeAccumulatorTimePeriodHead = timeAccumulatorTimePeriodHead;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.EmployeeId = employeeId;
            this.IsMultipleDays = stopDate.Date > startDate.Date;
            this.IsMoreThanOneWeek = stopDate.Subtract(startDate).TotalDays > 7;
            this.CalculateDay = calculateDay;
            this.CalculatePeriod = calculatePeriod;
            this.CalculatePlanningPeriod = calculatePlanningPeriod;
            this.CalculateYear = calculateYear;
            this.CalculateAccToday = calculateAccToday;
            this.CalculateRange = calculateRange;
            this.IncludeBalanceYear = includeBalanceYear;
        }

        public bool DoCalculateDay(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType)
        {
            periodType = TermGroup_AccumulatorTimePeriodType.Day;
            InitDates(out calculationStartDate, out calculationStopDate);
            if (!this.CalculateDay || this.IsMultipleDays)
                return false;

            calculationStartDate = CalendarUtility.GetBeginningOfDay(this.StartDate);
            calculationStopDate = CalendarUtility.GetEndOfDay(this.StartDate);

            return IsValid(calculationStartDate, calculationStopDate);
        }

        public bool DoCalculatePeriod(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType, out string name, out string datesText)
        {
            periodType = TermGroup_AccumulatorTimePeriodType.Period;
            InitDates(out calculationStartDate, out calculationStopDate);
            name = "";
            datesText = "";
            if (!this.CalculatePeriod)
                return false;

            if (this.IsMoreThanOneWeek && this.TimePeriod != null)
            {
                calculationStartDate = CalendarUtility.GetBeginningOfDay(this.TimePeriod.StartDate);
                calculationStopDate = CalendarUtility.GetEndOfDay(this.TimePeriod.StopDate);
                name = this.TimePeriod.Name;
                datesText = $"{TimePeriod.StartDate.ToShortDateString()} - {this.TimePeriod.StopDate.ToShortDateString()}";
            }
            else
            {
                calculationStartDate = CalendarUtility.GetBeginningOfDay(this.StartDate);
                calculationStopDate = CalendarUtility.GetEndOfDay(this.StopDate);
            }


            return IsValid(calculationStartDate, calculationStopDate);
        }

        public bool DoCalculatePlanningPeriod(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType, out TimePeriodDTO timePeriod)
        {
            periodType = TermGroup_AccumulatorTimePeriodType.PlanningPeriod;
            InitDates(out calculationStartDate, out calculationStopDate);
            timePeriod = null;
            if (!this.CalculatePlanningPeriod)
                return false;
            if (this.TimeAccumulatorTimePeriodHead == null || this.TimeAccumulatorTimePeriodHead.TimePeriodType != TermGroup_TimePeriodType.RuleWorkTime)
                return false;

            timePeriod = this.TimeAccumulatorTimePeriodHead.TimePeriods.GetTimePeriod(this.StartDate);
            if (timePeriod == null)
                return false;

            calculationStartDate = CalendarUtility.GetBeginningOfDay(timePeriod.StartDate);
            calculationStopDate = CalendarUtility.GetEndOfDay(timePeriod.StopDate);

            return IsValid(calculationStartDate, calculationStopDate);
        }

        public bool DoCalculateAccToday(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType)
        {
            periodType = TermGroup_AccumulatorTimePeriodType.AccToday;
            InitDates(out calculationStartDate, out calculationStopDate);
            if (!this.CalculateAccToday)
                return false;

            calculationStartDate = CalendarUtility.GetFirstDateOfYear(this.StartDate);
            calculationStopDate = CalendarUtility.GetEndOfDay(this.StopDate);
            return true;
        }

        public bool DoCalculateYear(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType, DateTime? overrideStopDate = null)
        {
            periodType = TermGroup_AccumulatorTimePeriodType.Year;
            InitDates(out calculationStartDate, out calculationStopDate);
            if (!this.CalculateYear)
                return false;

            calculationStartDate = CalendarUtility.GetBeginningOfDay(CalendarUtility.GetFirstDateOfYear(this.StartDate));

            if (overrideStopDate.HasValue && overrideStopDate.Value.Date < CalendarUtility.GetEndOfDay(CalendarUtility.GetLastDateOfYear(this.StartDate)))
                calculationStopDate = CalendarUtility.GetEndOfDay(overrideStopDate.Value.Date);
            else
                calculationStopDate = CalendarUtility.GetEndOfDay(CalendarUtility.GetLastDateOfYear(this.StartDate));

            return IsValid(calculationStartDate, calculationStopDate);
        }

        public bool DoCalculateRange(out DateTime calculationStartDate, out DateTime calculationStopDate, out TermGroup_AccumulatorTimePeriodType periodType)
        {
            periodType = TermGroup_AccumulatorTimePeriodType.Range;
            InitDates(out calculationStartDate, out calculationStopDate);
            if (!this.CalculateRange)
                return false;

            calculationStartDate = CalendarUtility.GetBeginningOfDay(this.StartDate);
            calculationStopDate = CalendarUtility.GetEndOfDay(this.StopDate);

            return IsValid(calculationStartDate, calculationStopDate);
        }

        public bool DoCalculuateBalanceYear(out DateTime calculationStartDate)
        {
            calculationStartDate = CalendarUtility.DATETIME_DEFAULT;

            if (!this.CalculateYear && !this.CalculateAccToday)
                return false;
            if (this.Type != TermGroup_TimeAccumulatorType.Rolling)
                return false;

            if (this.CalculateAccToday)
                this.IncludeBalanceYear = true;
            if (this.IncludeBalanceYear)
                calculationStartDate = CalendarUtility.GetBeginningOfDay(CalendarUtility.GetFirstDateOfYear(this.StartDate));

            return IsValid(calculationStartDate);
        }

        #region Help-methods

        private void InitDates(out DateTime calculationStartDate, out DateTime calculationStopDate)
        {
            calculationStartDate = CalendarUtility.DATETIME_DEFAULT;
            calculationStopDate = CalendarUtility.DATETIME_DEFAULT;
        }

        private bool IsValid(params DateTime[] dates)
        {
            foreach (DateTime date in dates)
            {
                if (date == CalendarUtility.DATETIME_DEFAULT)
                    return false;
            }
            return true;
        }

        #endregion
    }

    public class TimeAccumulatorRuleItem
    {
        public TermGroup_AccumulatorTimePeriodType PeriodType { get; set; }
        public SoeTimeAccumulatorComparison Comparison { get; set; }
        public int ValueMinutes { get; set; }
        public int WarningMinutes { get; set; }
        public int DiffMinutes { get; set; }
        public TimeSpan DiffValue { get; set; }
        public string Diff { get; set; }
        public bool ShowWarning
        {
            get
            {
                return this.Comparison == SoeTimeAccumulatorComparison.LessThanMinWarning || this.Comparison == SoeTimeAccumulatorComparison.MoreThanMaxWarning;
            }
        }
        public bool ShowError
        {
            get
            {
                return this.Comparison == SoeTimeAccumulatorComparison.LessThanMin || this.Comparison == SoeTimeAccumulatorComparison.MoreThanMax;
            }
        }
        [TsIgnore]
        public bool NoRulesDefined { get; set; }

        public TimeAccumulatorRuleItem()
        {
            this.PeriodType = TermGroup_AccumulatorTimePeriodType.Unknown;
            this.Comparison = SoeTimeAccumulatorComparison.OK;
            this.NoRulesDefined = true;
        }

        public TimeAccumulatorRuleItem(TermGroup_AccumulatorTimePeriodType periodType, bool noRulesDefined = false)
        {
            this.PeriodType = periodType;
            this.Comparison = SoeTimeAccumulatorComparison.OK;
            this.DiffValue = new TimeSpan();
            this.Diff = string.Empty;
            this.NoRulesDefined = noRulesDefined;
        }

        public TimeAccumulatorRuleItem(TermGroup_AccumulatorTimePeriodType periodType, SoeTimeAccumulatorComparison comparison, decimal value, int warning)
        {
            this.PeriodType = periodType;
            this.Comparison = comparison;
            this.ValueMinutes = (int)value;
            this.WarningMinutes = warning;
            this.DiffMinutes = this.ValueMinutes - this.WarningMinutes;
            this.DiffValue = CalendarUtility.MinutesToTimeSpan(this.DiffMinutes);
            this.Diff = CalendarUtility.FormatTimeSpan(this.DiffMinutes, prefix: comparison == SoeTimeAccumulatorComparison.MoreThanMax || comparison == SoeTimeAccumulatorComparison.MoreThanMaxWarning ? "+" : "");
        }
    }

    public class TimeAccumulatorEmployeeCalculation<TRow> where TRow : TimeAccumulatorCalculationRow
    {
        public int EmployeeId { get; private set; }
        public List<DateTime> DatesCalculated { get; private set; }
        public List<DateRangeDTO> DateRanges { get; private set; }
        public List<TRow> Rows { get; private set; }

        public TimeAccumulatorEmployeeCalculation(int employeeId)
        {
            this.EmployeeId = employeeId;
            this.DatesCalculated = new List<DateTime>();
            this.DateRanges = new List<DateRangeDTO>();
            this.Rows = new List<TRow>();
        }

        public bool ContainsDateRange(DateTime startDate, DateTime stopDate, out List<DateRangeDTO> missingDateRanges)
        {
            missingDateRanges = new List<DateRangeDTO>();

            if (this.DateRanges.IsNullOrEmpty())
            {
                missingDateRanges.Add(new DateRangeDTO(startDate, stopDate));
            }
            else
            {
                DateTime minDate = this.DateRanges.Min(i => i.Start);
                if (startDate < minDate)
                    missingDateRanges.Add(new DateRangeDTO(startDate, CalendarUtility.GetEndOfDay(minDate.AddDays(-1))));
                DateTime maxDate = this.DateRanges.Max(i => i.Stop);
                if (stopDate > maxDate)
                    missingDateRanges.Add(new DateRangeDTO(CalendarUtility.GetBeginningOfDay(maxDate.AddDays(1)), stopDate));
            }

            return !missingDateRanges.Any();
        }

        public void AddDateIntervals(List<DateRangeDTO> dateRanges)
        {
            if (dateRanges.IsNullOrEmpty())
                return;

            foreach (DateRangeDTO dateRange in dateRanges)
            {
                if (!this.DateRanges.Any(r => r.Start == dateRange.Start && r.Stop == dateRange.Stop))
                    this.DateRanges.Add(dateRange);
            }

            this.DateRanges = this.DateRanges.OrderBy(i => i.Start).ThenBy(i => i.Stop).ToList();
        }

        public bool TryAddRow(TRow row)
        {
            if (row == null)
                return false;

            this.Rows.Add(row);
            this.DatesCalculated.Add(row.Date);
            return true;
        }

        public List<TRow> GetRows(DateTime startDate, DateTime stopDate)
        {
            return this.Rows?.Where(i => i.Date >= startDate && i.Date <= stopDate).ToList() ?? new List<TRow>();
        }

        public bool ContainsTransaction(int transactionId)
        {
            return this.Rows?.Any(i => i.GetTransactionId() == transactionId) ?? false;
        }

        public T GetResult<T>(DateTime startDate, DateTime stopDate) where T : TimeAccumulatorCalculationResult, new()
        {
            T result = new T();

            List<TRow> validRows = GetRows(startDate, stopDate);
            foreach (var row in validRows)
            {
                result.Accumulate(row.GetId(), row.Sum);
            }

            return result;
        }
    }

    #region Row classes

    public abstract class TimeAccumulatorCalculationRow
    {
        public int TimeAccumulatorId { get; }
        public DateTime Date { get; }
        public decimal Quantity { get; }
        public decimal Factor { get; }

        private decimal? sum;
        public decimal Sum
        {
            get
            {
                if (!sum.HasValue)
                    sum = this.Quantity * this.Factor;
                return sum.Value;
            }
        }

        protected TimeAccumulatorCalculationRow(int timeAccumulatorId, DateTime date, decimal quantity, decimal factor)
        {
            this.TimeAccumulatorId = TimeAccumulatorId;
            this.Date = date;
            this.Quantity = quantity;
            this.Factor = factor;
        }

        public abstract int GetId();
        public abstract int GetTransactionId();
    }

    public class TimeAccumulatorCalculationTimeCodeRow : TimeAccumulatorCalculationRow
    {
        public int TimeCodeId { get; }
        public int TimeCodeTransactionId { get; }
        public TermGroup_TimeCodeRegistrationType RegistrationType { get; set; }

        public TimeAccumulatorCalculationTimeCodeRow(int timeAccumulatorId, DateTime date, decimal quantity, decimal factor, int timeCodeId, int timeCodeTransactionId, int registrationType) : base(timeAccumulatorId, date, quantity, factor)
        {
            this.TimeCodeId = timeCodeId;
            this.TimeCodeTransactionId = timeCodeTransactionId;
            this.RegistrationType = (TermGroup_TimeCodeRegistrationType)registrationType;
        }

        public override int GetId()
        {
            return this.TimeCodeId;
        }

        public override int GetTransactionId()
        {
            return this.TimeCodeTransactionId;
        }
    }

    public class TimeAccumulatorCalculationPayrollProductRow : TimeAccumulatorCalculationRow
    {
        public int PayrollProductId { get; }
        public int TimePayrollTransactionId { get; }

        public TimeAccumulatorCalculationPayrollProductRow(int timeAccumulatorId, DateTime date, decimal quantity, decimal factor, int payrollProductId, int timePayrollTransactionId) : base(timeAccumulatorId, date, quantity, factor)
        {
            this.PayrollProductId = payrollProductId;
            this.TimePayrollTransactionId = timePayrollTransactionId;
        }

        public override int GetId()
        {
            return this.PayrollProductId;
        }

        public override int GetTransactionId()
        {
            return this.TimePayrollTransactionId;
        }
    }

    public class TimeAccumulatorCalculationInvoiceProductRow : TimeAccumulatorCalculationRow
    {
        public int InvoiceProductId { get; }
        public int TimeInvoiceTransactionId { get; }

        public TimeAccumulatorCalculationInvoiceProductRow(int timeAccumulatorId, DateTime date, decimal quantity, decimal factor, int invoiceProductId, int timeInvoiceTransactionId) : base(timeAccumulatorId, date, quantity, factor)
        {
            this.InvoiceProductId = invoiceProductId;
            this.TimeInvoiceTransactionId = timeInvoiceTransactionId;
        }

        public override int GetId()
        {
            return this.InvoiceProductId;
        }

        public override int GetTransactionId()
        {
            return this.TimeInvoiceTransactionId;
        }
    }

    #endregion

    #region Result classes

    public abstract class TimeAccumulatorCalculationResult
    {
        protected Dictionary<int, decimal> SumById { get; private set; }
        public decimal Sum { get; private set; }

        protected TimeAccumulatorCalculationResult()
        {
            this.SumById = new Dictionary<int, decimal>();
        }

        public virtual void Accumulate(int id, decimal sum)
        {
            this.Sum += sum;

            if (this.SumById.ContainsKey(id))
                this.SumById[id] += sum;
            else
                this.SumById.Add(id, sum);
        }
    }

    public class TimeAccumulatorCalculationTimeCodeResult : TimeAccumulatorCalculationResult
    {
        public bool HasOneTimeCodeRegistrationType { get; set; }
        public Dictionary<int, decimal> SumByTimeCode
        {
            get
            {
                return base.SumById;
            }
        }

        public override void Accumulate(int id, decimal sum)
        {
            base.Accumulate(id, sum);
        }
    }

    public class TimeAccumulatorCalculationPayrollProductResult : TimeAccumulatorCalculationResult
    {
        public Dictionary<int, decimal> SumByPayrollProduct
        {
            get
            {
                return base.SumById;
            }
        }

        public override void Accumulate(int id, decimal sum)
        {
            base.Accumulate(id, sum);
        }
    }

    public class TimeAccumulatorCalculationInvoiceProductResult : TimeAccumulatorCalculationResult
    {
        public Dictionary<int, decimal> SumByInvoiceProductId
        {
            get
            {
                return base.SumById;
            }
        }

        public override void Accumulate(int id, decimal sum)
        {
            base.Accumulate(id, sum);
        }
    }

    #endregion
}
