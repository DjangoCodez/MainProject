using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class PayrollPriceTypeCopyItem
    {
        public int PayrollPriceTypeId { get; set; }
        public TermGroup_SysPayrollPriceType Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ConditionEmployedMonths { get; set; }
        public int ConditionExperienceMonths { get; set; }
        public int ConditionAgeYears { get; set; }
        public List<PayrollPriceTypePeriodCopyItem> PayrollPriceTypePeriods { get; set; } = new List<PayrollPriceTypePeriodCopyItem>();

    }

    public class PayrollPriceTypePeriodCopyItem
    {
        public decimal Amount { get; set; }
        public DateTime? FromDate { get; set; }
    }

}
