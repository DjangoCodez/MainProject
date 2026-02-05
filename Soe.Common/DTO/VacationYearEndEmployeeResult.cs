using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class VacationYearEndResultDTO
    {
        public ActionResult Result { get; set; }
        public List<VacationYearEndEmployeeResultDTO> EmployeeResults { get; set; }
    }

    public class VacationYearEndEmployeeResultDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNrAndName { get; set; }
        public string VacationGroupName { get; set; }
        public string Message { get; set; }
        public TermGroup_VacationYearEndStatus Status { get; set; }
        public string StatusName { get; set; }
    }

    public static class VacationYearEndEmployeeResultExtensions
    {
        public static List<VacationYearEndEmployeeResultDTO> Sort(this IEnumerable<VacationYearEndEmployeeResultDTO> l)
        {
            return l?.OrderByDescending(e => e.Status).ThenBy(e => e.EmployeeNrAndName).ToList() ?? new List<VacationYearEndEmployeeResultDTO>();
        }
    }
}
