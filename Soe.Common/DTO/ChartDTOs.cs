using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    #region Common

    public class DateChart
    {
        public List<DateChartData> Data { get; set; }
    }

    public class DateChartData
    {
        public DateTime Date { get; set; }
        public List<DateChartValue> Values { get; set; }

        public DateChartData(DateTime date)
        {
            Date = date;
        }
    }

    public class DateChartValue
    {
        public int Type { get; set; }
        public decimal Value { get; set; }

        public DateChartValue(int type, decimal value)
        {
            Type = type;
            Value = value;
        }
    }

    #endregion

    #region AverageSalaryCost

    public class AverageSalaryCostChartData
    {

    }

    #endregion

    #region EmployeeStatistics

    public class EmployeeStatisticsChartData
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Color { get; set; }
        public string ToolTip { get; set; }
    }

    public class EmployeeStatisticsAgentFeedData
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string ServiceGroup { get; set; }
        public DateTime Date { get; set; }
        public string ShiftTypeName { get; set; }

        public decimal AnsweredCalls { get; set; }
        public int NotAnsweredCalls { get; set; }
        public decimal LoggedOnTime { get; set; }
        public int SpeechTime { get; set; }
        public int ACWTime { get; set; }
        public int HoldTime { get; set; }
        public int ReadyTime { get; set; }
        public int Transferred { get; set; }
    }

    #endregion

    #region StaffingNeedsAnalysis

    public class StaffingNeedsAnalysisChartData
    {
        public TermGroup_TimeScheduleTemplateBlockShiftStatus ShiftStatus { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public decimal OrginalValue { get; set; }
        public decimal FrequencyValue { get; set; }
        public int? StaffingNeedsLocationGroupId { get; set; }
        public int MaxQuantity { get; set; }
    }

    #endregion

    #region StatisticsGauge

    public class StatisticsGaugeLoginsChartData
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
    }

    public class StatisticsGaugeUsersChartData
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class StatisticsGaugeCompaniesChartData
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    #endregion

    #region TimeStampStatistics

    public class TimeStampStatisticsChartData
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
    }

    #endregion
}
