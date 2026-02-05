using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class StaffingStatisticsInterval
    {
        #region Properties

        private DateTime interval;
        public DateTime Interval
        {
            get
            {
                return this.interval;
            }
        }
        private List<StaffingStatisticsIntervalRow> rows;
        public List<StaffingStatisticsIntervalRow> Rows
        {
            get
            {
                return this.rows;
            }
        }
        public int? EmployeeId { get; set; }

        #endregion

        #region Ctor

        public StaffingStatisticsInterval(DateTime interval)
        {
            this.interval = interval;
            this.rows = new List<StaffingStatisticsIntervalRow>();
        }

        #endregion

        #region Public methods

        public void AddNeedValue(int key, int value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetNeedValue(value);
        }

        public decimal GetNeedValue(int key)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetNeedValue();
        }

        public decimal SumNeedRow()
        {
            decimal sum = 0;
            foreach (var row in GetIntervalRows())
            {
                sum += row.GetNeedValue();
            }
            return sum;
        }

        public void AddFrequencyValue(int key, decimal value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetFrequencyValue(value);
        }

        public decimal GetFrequencyValue(int key)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetFrequencyValue();
        }

        public void AddRowFrequencyValue(int key, decimal value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetRowFrequencyValue(value);
        }

        public decimal GetRowFrequencyValue(int key)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetRowFrequencyValue();
        }

        public decimal SumFrequencyRow()
        {
            decimal sum = 0;
            foreach (var row in GetIntervalRows())
            {
                sum += row.GetFrequencyValue();
            }
            return sum;
        }

        public void AddBudgetValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetBudgetValue(calculationType, value);
        }

        public decimal GetBudgetValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetBudgetValue(calculationType);
        }

        public decimal SumBudgetRow(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            decimal sum = 0;
            foreach (var row in GetIntervalRows())
            {
                sum += row.GetBudgetValue(calculationType);
            }
            return sum;
        }

        public void AddForecastValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetForecastValue(calculationType, value);
        }

        public decimal GetForecastValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetForecastValue(calculationType);
        }

        public decimal SumForecastRow(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            decimal sum = 0;
            foreach (var row in GetIntervalRows())
            {
                sum += row.GetForecastValue(calculationType);
            }
            return sum;
        }

        public void AddTemplateValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetTemplateValue(calculationType, value);
        }

        public void AddTemplateForEmployeePostValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetTemplateForEmployeePostValue(calculationType, value);
        }

        public decimal GetTemplateValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetTemplateValue(calculationType);
        }

        public void AddScheduleValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value, bool addScheduleAndTime = true)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetScheduleValue(calculationType, value);

            if (addScheduleAndTime && this.interval >= DateTime.Today)
                row.SetScheduleAndTimeValue(calculationType, value);
        }

        public decimal GetScheduleValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetScheduleValue(calculationType);
        }

        public void AddTimeValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value, bool addScheduleAndTime = true, decimal? calculatedValue = null)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetTimeValue(calculationType, value);

            if (addScheduleAndTime && this.interval < DateTime.Today)
                row.SetScheduleAndTimeValue(calculationType, value);
            else if (this.Interval >= DateTime.Today && calculatedValue.HasValue)
            {
                row.SetScheduleValue(calculationType, calculatedValue.Value);
                row.SetScheduleAndTimeValue(calculationType, calculatedValue.Value);
            }
        }

        public void AddTimeAndSchedulValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            row.SetScheduleAndTimeValue(calculationType, value);
        }

        public decimal GetTimeValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetTimeValue(calculationType);
        }

        public decimal GetScheduleAndTimeValue(int key, TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            StaffingStatisticsIntervalRow row = GetIntervalRow(key);
            return row.GetScheduleAndTimeValue(calculationType);
        }

        #endregion

        #region Help-methods

        private List<StaffingStatisticsIntervalRow> GetIntervalRows()
        {
            return this.rows;
        }

        private StaffingStatisticsIntervalRow GetIntervalRow(int key)
        {
            StaffingStatisticsIntervalRow row = this.rows.FirstOrDefault(i => i.Key == key);
            if (row == null)
            {
                row = new StaffingStatisticsIntervalRow(key);
                this.rows.Add(row);
            }
            return row;
        }

        #endregion
    }

    public class StaffingStatisticsIntervalRow
    {
        #region Properties

        public int Key { get; }

        private int? _need;
        public int? Need
        {
            get
            {
                return this._need;
            }
        }
        private decimal? _frequency;
        public decimal? NeedFrequency
        {
            get
            {
                return this._frequency;
            }
        }

        private decimal? _rowFrequency;
        public decimal? NeedRowFrequency
        {
            get
            {
                return this._rowFrequency;
            }
        }

        private StaffingStatisticsIntervalValue _forecast;
        public StaffingStatisticsIntervalValue Forecast
        {
            get
            {
                return this._forecast;
            }
        }
        private StaffingStatisticsIntervalValue _budget;
        public StaffingStatisticsIntervalValue Budget
        {
            get
            {
                return this._budget;
            }
        }
        private StaffingStatisticsIntervalValue _templateSchedule;
        public StaffingStatisticsIntervalValue TemplateSchedule
        {
            get
            {
                return this._templateSchedule;
            }
        }
        private StaffingStatisticsIntervalValue _templateScheduleForEmployeePost;
        public StaffingStatisticsIntervalValue TemplateScheduleForEmployeePost
        {
            get
            {
                return this._templateScheduleForEmployeePost;
            }
        }
        private StaffingStatisticsIntervalValue _schedule;
        public StaffingStatisticsIntervalValue Schedule
        {
            get
            {
                return this._schedule;
            }
        }
        private StaffingStatisticsIntervalValue _time;
        public StaffingStatisticsIntervalValue Time
        {
            get
            {
                return this._time;
            }
        }

        private StaffingStatisticsIntervalValue _scheduleAndTime;
        public StaffingStatisticsIntervalValue ScheduleAndTime
        {
            get
            {
                return this._scheduleAndTime;
            }
        }

        public TermGroup_TimeSchedulePlanningFollowUpCalculationType TargetCalculationType { get; set; }
        public TermGroup_TimeSchedulePlanningFollowUpCalculationType ModifiedCalculationType { get; set; }

        #endregion

        #region Ctor

        public StaffingStatisticsIntervalRow(int key)
        {
            this.Key = key;
        }

        #endregion

        #region Public methods

        public void SetNeedValue(int value)
        {
            this._need = value;
        }

        public decimal GetNeedValue()
        {
            return this._need.HasValue ? this._need.Value : 0;
        }

        public void SetFrequencyValue(decimal value)
        {
            this._frequency = value;
        }

        public decimal GetFrequencyValue()
        {
            return this._frequency.HasValue ? this._frequency.Value : 0;
        }

        public void SetRowFrequencyValue(decimal value)
        {
            this._rowFrequency = value;
        }

        public decimal GetRowFrequencyValue()
        {
            return this._rowFrequency.HasValue ? this._rowFrequency.Value : 0;
        }

        public void SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            if (this._budget == null)
                this._budget = new StaffingStatisticsIntervalValue();
            this._budget.SetValue(calculationType, value);
        }

        public decimal GetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            if (this._budget == null)
                return 0;
            return this._budget.GetValue(calculationType);
        }

        public void SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            if (this._forecast == null)
                this._forecast = new StaffingStatisticsIntervalValue();
            this._forecast.SetValue(calculationType, value);
        }

        public decimal GetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            if (this._forecast == null)
                return 0;
            return this._forecast.GetValue(calculationType);
        }

        public void SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            if (this._templateSchedule == null)
                this._templateSchedule = new StaffingStatisticsIntervalValue();
            this._templateSchedule.SetValue(calculationType, value);
        }

        public decimal GetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            if (this._templateSchedule == null)
                return 0;
            return this._templateSchedule.GetValue(calculationType);
        }

        public void SetTemplateForEmployeePostValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            if (this._templateScheduleForEmployeePost == null)
                this._templateScheduleForEmployeePost = new StaffingStatisticsIntervalValue();
            this._templateScheduleForEmployeePost.SetValue(calculationType, value);
        }

        public decimal GetTemplateForEmployeePostValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            if (this._templateScheduleForEmployeePost == null)
                return 0;
            return this._templateScheduleForEmployeePost.GetValue(calculationType);
        }


        public void SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            if (this._schedule == null)
                this._schedule = new StaffingStatisticsIntervalValue();
            this._schedule.SetValue(calculationType, value);
        }

        public decimal GetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            if (this._schedule == null)
                return 0;
            return this._schedule.GetValue(calculationType);
        }

        public void SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            if (this._time == null)
                this._time = new StaffingStatisticsIntervalValue();
            this._time.SetValue(calculationType, value);
        }

        public decimal GetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            if (this._time == null)
                return 0;
            return this._time.GetValue(calculationType);
        }

        public void SetScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            if (this._scheduleAndTime == null)
                this._scheduleAndTime = new StaffingStatisticsIntervalValue();
            this._scheduleAndTime.SetValue(calculationType, value);
        }

        public decimal GetScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            if (this._scheduleAndTime == null)
                return 0;
            return this._scheduleAndTime.GetValue(calculationType);
        }

        #endregion
    }

    public class StaffingStatisticsIntervalRowDTO
    {
        public int Key { get; set; }

        public StaffingStatisticsIntervalValueDTO Forecast { get; set; }
        public StaffingStatisticsIntervalValueDTO Budget { get; set; }
        public StaffingStatisticsIntervalValueDTO TemplateSchedule { get; set; }
        public StaffingStatisticsIntervalValueDTO Schedule { get; set; }
        public StaffingStatisticsIntervalValueDTO Time { get; set; }

        public TermGroup_TimeSchedulePlanningFollowUpCalculationType TargetCalculationType { get; set; }
        public TermGroup_TimeSchedulePlanningFollowUpCalculationType ModifiedCalculationType { get; set; }

        public StaffingStatisticsIntervalRow ConvertToStaffingStatisticsIntervalRow()
        {
            StaffingStatisticsIntervalRow row = new StaffingStatisticsIntervalRow(this.Key);

            row.SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.Forecast.Sales);
            row.SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.Forecast.Hours);
            row.SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.Forecast.PersonelCost);
            row.SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, this.Forecast.SalaryPercent);
            row.SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, this.Forecast.LPAT);
            row.SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, this.Forecast.FPAT);
            row.SetForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.Forecast.BPAT);

            row.SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.Budget.Sales);
            row.SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.Budget.Hours);
            row.SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.Budget.PersonelCost);
            row.SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, this.Budget.SalaryPercent);
            row.SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, this.Budget.LPAT);
            row.SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, this.Budget.FPAT);
            row.SetBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.Budget.BPAT);

            row.SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.TemplateSchedule.Sales);
            row.SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.TemplateSchedule.Hours);
            row.SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.TemplateSchedule.PersonelCost);
            row.SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, this.TemplateSchedule.SalaryPercent);
            row.SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, this.TemplateSchedule.LPAT);
            row.SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, this.TemplateSchedule.FPAT);
            row.SetTemplateValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.TemplateSchedule.BPAT);

            row.SetTemplateForEmployeePostValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.TemplateSchedule.Hours);

            row.SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.Schedule.Sales);
            row.SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.Schedule.Hours);
            row.SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.Schedule.PersonelCost);
            row.SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, this.Schedule.SalaryPercent);
            row.SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, this.Schedule.LPAT);
            row.SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, this.Schedule.FPAT);
            row.SetScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.Schedule.BPAT);

            row.SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales, this.Time.Sales);
            row.SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours, this.Time.Hours);
            row.SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost, this.Time.PersonelCost);
            row.SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent, this.Time.SalaryPercent);
            row.SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT, this.Time.LPAT);
            row.SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT, this.Time.FPAT);
            row.SetTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT, this.Time.BPAT);

            row.TargetCalculationType = this.TargetCalculationType;
            row.ModifiedCalculationType = this.ModifiedCalculationType;

            return row;
        }
    }

    public class StaffingStatisticsIntervalValue
    {
        #region Properties

        private decimal _sales;
        public decimal Sales
        {
            get
            {
                return this._sales;
            }
        }
        public void SetSales(decimal value)
        {
            this._sales = value;
        }

        private decimal _hours;
        public decimal Hours
        {
            get
            {
                return this._hours;
            }
        }
        public void SetHours(decimal value)
        {
            this._hours = value;
        }

        private decimal _personelCost;
        public decimal PersonelCost
        {
            get
            {
                return this._personelCost;
            }
        }
        public void SetPersonelCost(decimal value)
        {
            this._personelCost = value;
        }

        private decimal _salaryPercent;
        public decimal SalaryPercent
        {
            get
            {
                return this._salaryPercent;
            }
        }
        public void SetSalaryPercent(decimal value)
        {
            this._salaryPercent = value;
        }

        private decimal _lpat;
        public decimal LPAT
        {
            get
            {
                return this._lpat;
            }
        }
        public void SetLPAT(decimal value)
        {
            this._lpat = value;
        }

        private decimal _fpat;
        public decimal FPAT
        {
            get
            {
                return this._fpat;
            }
        }
        public void SetFPAT(decimal value)
        {
            this._fpat = value;
        }

        private decimal _bpat;
        public decimal BPAT
        {
            get
            {
                return this._bpat;
            }
        }
        public void SetBPAT(decimal value)
        {
            this._bpat = value;
        }

        #endregion

        #region Ctor

        public StaffingStatisticsIntervalValue()
        {

        }

        #endregion

        #region Public methods

        public void SetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType, decimal value)
        {
            switch (calculationType)
            {
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales:
                    this._sales += value;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours:
                    value = decimal.Round(value, 0);
                    this._hours += value;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost:
                    this._personelCost += value;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent:
                    this._salaryPercent += value;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT:
                    this._lpat += value;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT:
                    this._fpat += value;
                    break;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT:
                    this._bpat += value;
                    break;
            }
        }

        public decimal GetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType calculationType)
        {
            switch (calculationType)
            {
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales:
                    return this._sales;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours:
                    return this._hours;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.ActualHours:
                    return this.Hours > 0 ? Decimal.Divide(this._hours, 60) : 0;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost:
                    return this._personelCost;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent:
                    return this._salaryPercent;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT:
                    return this._lpat;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT:
                    return this._fpat;
                case TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT:
                    return this._bpat;
            }
            return 0;
        }

        #endregion
    }

    public class StaffingStatisticsIntervalValueDTO
    {
        public decimal Sales { get; set; }
        public decimal Hours { get; set; }
        public decimal PersonelCost { get; set; }
        public decimal SalaryPercent { get; set; }
        public decimal LPAT { get; set; }
        public decimal FPAT { get; set; }
        public decimal BPAT { get; set; }
    }
}
