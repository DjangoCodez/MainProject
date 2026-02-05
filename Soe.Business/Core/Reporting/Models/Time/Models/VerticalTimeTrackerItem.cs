using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class VerticalTimeTrackerItem
    {
        public VerticalTimeTrackerItem()
        {
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }

        public DateTime Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }

        public string TimeInterval
        {
            get
            {
                return StartTime.ToString("HH:mm") + " - " + StopTime.ToString("HH:mm");
            }
        }
        public decimal Time { get; set; }
        public decimal TimeCost { get; set; }
        public decimal Schedule { get; set; }
        public decimal ScheduleCost { get; set; }

        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNr { get; set; }
        public int EmployeeId { get; set; }
        public int TimeHeads { get; set; }
        public int ScheduleHeads { get; set; }

        public Guid? SeperateKey { get; set; }
        public string ScheduleBlockIds { get; set; } = string.Empty;

        public string GroupByProps()
        {
            var accounts = string.Join("|", (AccountAnalysisFields?.Select(s => s.AccountId).ToList() ?? new List<int>()));
            return $"{StartDate.Date}#{TimeInterval}#{EmployeeId}#{Time}#{Schedule}#{accounts}#{SeperateKey}";
        }

        public VerticalTimeTrackerItem Clone()
        {
            return new VerticalTimeTrackerItem
            {
                Date = this.Date,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                StartTime = this.StartTime,
                StopTime = this.StopTime,
                Time = this.Time,
                TimeCost = this.TimeCost,
                Schedule = this.Schedule,
                ScheduleCost = this.ScheduleCost,
                AccountAnalysisFields = AccountAnalysisFields != null ? this.AccountAnalysisFields.Select(s => s.Clone()).ToList() : null,
                EmployeeName = this.EmployeeName,
                EmployeeNr = this.EmployeeNr,
                EmployeeId = this.EmployeeId,
                TimeHeads = this.TimeHeads,
                ScheduleHeads = this.ScheduleHeads,
                SeperateKey = (Guid?)null
            };
        }
    }
}
