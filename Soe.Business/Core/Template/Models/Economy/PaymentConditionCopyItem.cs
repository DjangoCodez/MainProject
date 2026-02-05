using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class PaymentConditionCopyItem
    {
        public int PaymentConditionId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Days { get; set; }
    }
}
