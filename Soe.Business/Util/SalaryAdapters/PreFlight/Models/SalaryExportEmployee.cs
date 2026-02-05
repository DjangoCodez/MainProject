using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models
{
    public class SalaryExportEmployee
    {
        public string EmployeeExternalCode { get; set; }
        public string EmployeeNr { get; set; }
        public int EmployeeId { get; set; }
        public List<SalaryExportSchedule> SalarySchedules { get; set; } = new List<SalaryExportSchedule>();
        public List<SalaryExportTransaction> SalaryTransactions { get; set; } = new List<SalaryExportTransaction>();
        public List<SalaryExportEmployeeChild> EmployeeChildren { get; set; } = new List<SalaryExportEmployeeChild>();
    }
}
