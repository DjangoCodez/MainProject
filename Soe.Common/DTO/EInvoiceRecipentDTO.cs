using SoftOne.Soe.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
  public class EInvoiceRecipientSearchResultDTO
    {
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string OrgNo { get; set; }
        public string VatNo { get; set; }
        public string GLN { get; set; }
    }

    [TSInclude]
    public class EInvoiceRecipientSearchDTO
    {
        public string Name { set; get; }
        public string GLN { set; get; }
        public string OrgNo { set; get; }
        public string VatNo { set; get; }
        public bool ReceiveElectronicInvoiceCapability { set; get; }
    }
}
