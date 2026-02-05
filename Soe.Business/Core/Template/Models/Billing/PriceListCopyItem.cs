using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Billing
{
    public class PriceListCopyItem
    {
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public int PriceListTypeId { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public decimal Quantity { get; set; }
        public int ProductId { get;  set; }
    }
}
