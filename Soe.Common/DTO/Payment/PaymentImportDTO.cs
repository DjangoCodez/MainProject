using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class PaymentImportRowsDto
    {
        public TermGroup_SysPaymentMethod PaymentIOType;
        public int PaymentMethodId { get; set; }
        public List<byte[]> Contents { get; set; }
        public String Base64String { get; set; }
        public string FileName { get; set; }
        public int BatchId { get; set; }
        public int PaymentImportId { get; set; }
        public ImportPaymentType ImportType { get; set; }
    }
}
