using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Data
{
    public partial class Currency : ICreatedModified
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int SysTermId { get; set; }
        public string Description
        {
            get { return StringUtility.Concat(this.Code, this.Name, false); }
        }
        public bool DefineRateManually
        {
            get
            {
                if (this.IntervalType > 0)
                {
                    if (this.IntervalType == (int)TermGroup_CurrencyIntervalType.Manually)
                        this.UseSysRate = 0;
                    else
                        this.UseSysRate = 1;
                }
                return this.UseSysRate == 0;
            }
        }
    }

    public static partial class EntityExtensions
    {
        
    }
}
