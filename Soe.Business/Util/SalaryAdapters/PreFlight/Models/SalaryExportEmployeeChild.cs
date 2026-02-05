using SoftOne.Soe.Common.Util.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models
{
    public class SalaryExportEmployeeChild
    {
        public int EmployeeChildId { get; set; }
        public int EmployeeId { get; set; }
        public string Name
        {
            get
            {
                return $"{this.FirstName} {this.LastName}";
            }
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
