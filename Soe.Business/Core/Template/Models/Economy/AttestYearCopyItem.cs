using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class AccountYearCopyItem
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TermGroup_AccountStatus Status { get; set; } 
        public List<AccountPeriodCopyItem> AccountPeriodCopyItems { get; set; } = new List<AccountPeriodCopyItem>();
        public List<VoucherSerieCopyItem> VoucherSeriesCopyItems { get; set; } = new List<VoucherSerieCopyItem>();
        public int AccountYearId { get; internal set; }
    }

    public class AccountPeriodCopyItem
    {
        public int PeriodNr { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public TermGroup_AccountStatus Status { get; set; }
        public int AccountPeriodId { get; internal set; }
    }

    public class VoucherSerieCopyItem
    {
        public string VoucherSeriesTypeName { get; set; }
        public DateTime? VoucherDateLatest { get; set; }
        public long? VoucherNrLatest { get; set; }
        public int? Status { get; set; }
    }
}
