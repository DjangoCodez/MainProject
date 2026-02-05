using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class VoucherTemplateRowCopyItem
    {
        public string Text { get; set; }
        public decimal Amount { get; set; }
        public int AccountId { get; set; }
        public decimal? Quantity { get; set; }
        public DateTime? Date { get; set; }
        public bool Merged { get; set; }
        public int State { get; set; }
        public int? AccountDistributionHeadId { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public int? RowNr { get; set; }

        public string AccountStdAccountName { get; set; }
        public string AccountStdAccountAccountNr { get; set; }
        public int? AccountDistributionHeadType { get; set; }
        public string AccountDistributionHeadName { get; set; }
    }
}
