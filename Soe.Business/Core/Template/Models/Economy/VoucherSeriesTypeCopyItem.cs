using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Economy
{
    public class VoucherSeriesTypeCopyItem
    {
        public int VoucherSeriesTypeId { get; set; }
        public string Name { get; set; }
        public long StartNr { get; set; }
        public int VoucherSeriesTypeNr { get; set; }
        public bool Template { get; set; }
    }
}
