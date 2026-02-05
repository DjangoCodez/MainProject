using SoftOne.Soe.Business.Core.Template.Models.Economy;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class ShiftTypeCopyItem
    {
        public int ShiftTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? TimeScheduleTemplateBlockType { get; set; }
        public string Color { get; set; }
        public int? ExternalId { get; set; }
        public string ExternalCode { get; set; }
        public int DefaultLength { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public string NeedsCode { get; set; }
        public bool HandlingMoney { get; set; }
        public int? AccountId { get; set; } 
        public List<AccountInternalCopyItem> AccountInternals { get; set; }
        public int? TimeScheduleTypeId { get; set; }
    }


}
