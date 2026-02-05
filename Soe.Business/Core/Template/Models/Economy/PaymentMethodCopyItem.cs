using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class PaymentMethodCopyItem
    {
        public int SysPaymentMethodId { get; set; }
        public int PaymentType { get; set; }
        public string Name { get; set; }
        public string CustomerNr { get; set; }
        public int PaymentInformationRowId { get; set; }
        public int AccountId { get; set; }
        public bool IsCustomerPayment { get; set; }
        public SoeOriginType SoeOriginType { get; internal set; }
    }


}
