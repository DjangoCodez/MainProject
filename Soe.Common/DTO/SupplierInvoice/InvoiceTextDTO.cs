using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class InvoiceTextDTO
    {
        public int? InvoiceId { get; set; }
        public int? EdiEntryId { get; set; }
        public string Text { get; set; }
        public InvoiceTextType Type { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }
}
