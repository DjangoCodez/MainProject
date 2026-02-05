using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimePeriodHeadCopyItem
    {
        public int TimePeriodHeadId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TermGroup_TimePeriodType TimePeriodType { get; set; } // You may need to adjust the data type here

        public List<TimePeriodCopyItem> TimePeriods { get; set; } = new List<TimePeriodCopyItem>();

    }

    public class TimePeriodCopyItem
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime? PayrollStartDate { get; set; }
        public DateTime? PayrollStopDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int RowNr { get;  set; }
    }

}
