using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class VatCodeCopyItem
    {
        public int VatCodeId { get; set; }
        public int AccountId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Percent { get; set; }
        public int? PurchaseVATAccountId { get; set; }
    }

}
