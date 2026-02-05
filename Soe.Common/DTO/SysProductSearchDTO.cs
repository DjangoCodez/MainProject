using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class SysProductSearchDTO
    {
        public int SysCountryId { get; set; }
        public string Token { get; set; }
        public int Fetchsize { get; set; }
        public string Search { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string ProductGroupIdentifier { get; set; }
        public string Text { get; set; }    
        public string EAN { get; set; }
        public List<int> SysPriceListHeadIds { get; set; }
        public ExternalProductType ExternalProductType { get; set; }
    }
}
