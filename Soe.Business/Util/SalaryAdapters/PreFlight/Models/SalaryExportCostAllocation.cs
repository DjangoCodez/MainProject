using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models
{
    public class SalaryExportCostAllocation
    {
        public string Costcenter { get; set; }
        public string Project { get; set; } 
        public string Department { get; set; }

        public string EmployeeCostCenter { get; set; }
        public string EmployeeProject { get; set; }
        public string EmployeeDepartment { get; set; }

        public SalaryExportCostAllocation Clone()
        {
            return new SalaryExportCostAllocation
            {
                Costcenter = Costcenter,
                Project = Project,
                Department = Department,
                EmployeeCostCenter = EmployeeCostCenter,
                EmployeeProject = EmployeeProject,
                EmployeeDepartment = EmployeeDepartment
            };
        }

        public override string ToString()
        {
            return $"Costcenter: {Costcenter}, Project: {Project}, Department: {Department}";//, EmployeeCostCenter: {EmployeeCostCenter}, EmployeeProject: {EmployeeProject}, EmployeeDepartment: {EmployeeDepartment}";
        }

    }
}
