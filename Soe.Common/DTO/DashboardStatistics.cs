using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class DashboardStatisticType
    {
        public string Key { get; set; }
        public DashboardStatisticsType DashboardStatisticsType { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
    }

    public class DashboardStatisticsDTO
    {
        public DashboardStatisticsDTO()
        {
            DashboardStatisticRows = new List<DashboardStatisticRowDTO>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public DashboardStatisticsType DashboardStatisticsType { get; set; }
        public List<DashboardStatisticRowDTO> DashboardStatisticRows { get; set; }
        public TermGroup_PerformanceTestInterval interval { get; set; }
    }

    public class DashboardStatisticRowDTO
    {
        public DashboardStatisticRowDTO(string name, DashboardStatisticsRowType dashboardStatisticsRowType, List<DashboardStatisticPeriodDTO> dashboardStatisticPeriods)
        {
            Name = name;
            DashboardStatisticsRowType = dashboardStatisticsRowType;
            DashboardStatisticPeriods = dashboardStatisticPeriods;
        }

        public string Name { get; set; }
        public DashboardStatisticsRowType DashboardStatisticsRowType { get; set; }
        public List<DashboardStatisticPeriodDTO> DashboardStatisticPeriods { get; set; }
    }

    public class DashboardStatisticPeriodDTO
    {
        public DashboardStatisticPeriodDTO(DashboardStatisticsRowType dashboardStatisticsPeriodRowType, DateTime from, DateTime to, decimal value)
        {
            DashboardStatisticsPeriodRowType = dashboardStatisticsPeriodRowType;
            From = from;
            To = to;
            Value = value;
        }

        public DashboardStatisticsRowType DashboardStatisticsPeriodRowType { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal Value { get; set; }
    }
}
