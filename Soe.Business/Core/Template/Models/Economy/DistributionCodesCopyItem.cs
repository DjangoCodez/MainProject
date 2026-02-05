using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class DistributionCodeHeadCopyItem
    {
        public int DistributionCodeHeadId { get;set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public int NoOfPeriods { get; set; }
        public int? SubType { get; set; }
        public int? OpeningHoursId { get; set; }
        public string OpeningHoursName { get; set; }
        public int? AccountDimId { get; set; }
        public int? AccountDimNr { get; set; }
        public string AccountDimName { get; set; }
        public DateTime? FromDate { get; set; }
        public int? ParentId { get; set; }

        public List<DistributionCodePeriodCopyItem> DistributionCodePeriodCopyItem { get; set; }
    }

    public class DistributionCodePeriodCopyItem
    {
        public int? ParentToDistributionCodeHeadId { get; set; }
        public decimal Percent { get; set; }
        public decimal OppositeBalance { get; set; }
        public string Comment { get; set; }
    }
}
