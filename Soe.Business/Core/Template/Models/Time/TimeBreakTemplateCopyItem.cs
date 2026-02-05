using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeBreakTemplateCopyItem
    {
        public int TimeBreakTemplateId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool UseMaxWorkTimeBetweenBreaks { get; set; }
        public int ShiftLength { get; set; }
        public DateTime? ShiftStartFromTime { get; set; }
        public int MinTimeBetweenBreaks { get; set; }
        public string DayOfWeeks { get; set; }

        public List<ShiftTypeCopyItem> ShiftTypes { get; set; } = new List<ShiftTypeCopyItem>();
        public List<TimeBreakTemplateRowCopyItem> TimeBreakTemplateRows { get; set; } = new List<TimeBreakTemplateRowCopyItem>();
    }

    public class TimeBreakTemplateRowCopyItem
    {
        public int Type { get; set; }
        public int MinTimeAfterStart { get; set; }
        public int MinTimeBeforeEnd { get; set; }
        public int? TimeCodeBreakGroupId { get; set; }
    }

}
