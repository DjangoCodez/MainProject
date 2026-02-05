using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Template.Models.Core
{
    public class CompanySettingCopyItem
    {
        public int UserCompanySettingId { get; set; }
        public int? ActorCompanyId { get; set; }
        public CompanySettingType SettingTypeId { get; set; }
        public SettingDataType DataTypeId { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public decimal? DecimalData { get; set; }
    }
}

