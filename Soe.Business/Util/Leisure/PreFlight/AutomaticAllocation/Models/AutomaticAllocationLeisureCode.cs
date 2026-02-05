using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.Leisure.PreFlight.AutomaticAllocation.Models
{
    public class AutomaticAllocationLeisureCode
    {
        public int TimeLeisureCodeId { get; set; }
        public int ActorCompanyId { get; set; }
        public LeisureCodeType Type { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DayOfWeek PreferredDay { get; set; } = DayOfWeek.Sunday;
        public int RequiredDaysPerWeek { get; set; }
        public int? EmployeeGroupId { get; set; }
        public Decimal LeisureHour { get;  set; }
        public bool MoveableToOtherWeek { get { return Type == LeisureCodeType.X; } }

        public override string ToString()
        {
            return $"{Code} {Name} {Type} PD {PreferredDay} RDPW {RequiredDaysPerWeek} LH {LeisureHour}";
        }
    }

    public enum LeisureCodeType
    {
        None = 0,
        V = 1, // Weekly rest day
        X = 2, // Extra rest day
    }
}
