using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Billing
{
    public class SupplierAgreementCopyItem
    {
        public int ActorCompanyId { get; set; }
        public int? CategoryId { get; set; }
        public int CodeType { get; set; }
        public string Code { get; set; }
        public DateTime? Date { get; set; }
        public decimal DiscountPercent { get; set; }
        public int PriceListOrigin { get; set; }
        public int? PriceListTypeId { get; set; }
        public int SysWholesellerId { get; set; }
    }
}
