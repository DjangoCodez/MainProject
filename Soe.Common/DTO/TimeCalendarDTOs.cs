using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeCalendarPeriodDTO
    {
        #region Public properties

        // Date
        public DateTime Date { get; set; }
        public string DayDescription { get; set; }

        // Number of transactions
        public int Type1 { get; set; }
        public int Type2 { get; set; }
        public int Type3 { get; set; }
        public int Type4 { get; set; }

        // ToolTips
        public string Type1ToolTip { get; set; }
        public string Type2ToolTip { get; set; }
        public string Type3ToolTip { get; set; }
        public string Type4ToolTip { get; set; }

        public List<TimeCalendarPeriodPayrollProductDTO> PayrollProducts { get; set; }

        #endregion

        #region Constructors

        public TimeCalendarPeriodDTO()
        {
            this.PayrollProducts = new List<TimeCalendarPeriodPayrollProductDTO>();
        }

        public TimeCalendarPeriodDTO(DateTime date)
        {
            this.Date = date;
            this.PayrollProducts = new List<TimeCalendarPeriodPayrollProductDTO>();
        }

        #endregion
    }

    public class TimeCalendarPeriodPayrollProductDTO
    {
        public int PayrollProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public decimal Amount { get; set; }
    }

    public class TimeCalendarSummaryDTO
    {
        public int PayrollProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int Occations { get; set; }
        public int Days { get; set; }
    }
}
