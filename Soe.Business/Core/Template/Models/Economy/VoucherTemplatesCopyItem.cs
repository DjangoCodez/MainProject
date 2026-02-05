using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class VoucherTemplatesCopyItem
    {
        public int VoucherHeadId { get; set; }
        public int AccountPeriodId { get; set; }
        public int VoucherSeriesId { get; set; }
        public long VoucherNr { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public int Status { get; set; }
        public bool TypeBalance { get; set; }
        public bool? VatVoucher { get; set; }
        public string Note { get; set; }
        public bool? CompanyGroupVoucher { get; set; }
        public int SourceType { get; set; }
        public bool Template { get; set; }
        public int ActorCompanyId { get; set; }

        public string VoucherSeriesTypeName { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public DateTime VoucherSeriesAccountYearFrom { get; set; }
        public DateTime VoucherSeriesAccountYearTo { get; set; }
        public int VoucherSeriesAccountYearStatus { get; set; }

        public int AccountPeriodPeriodNr { get; set; }
        public DateTime AccountPeriodFrom { get; set; }
        public DateTime AccountPeriodTo { get; set; }

        public List<VoucherTemplateRowCopyItem> VoucherTemplateRows { get; set; }
    }
}
