using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getEmployeesByNumber: JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string employeeNr = QS["enr"];
            if (!string.IsNullOrEmpty(employeeNr))
            {
                List<Employee> Employees = EmployeeManager.GetAllEmployeesByNumber(SoeCompany.ActorCompanyId, employeeNr, 20);

                Queue q = new Queue();
                
                if (Employees != null)
                {
                    foreach (var employee in Employees)
                    {
                        q.Enqueue(new
                        {
                            Found = true,
                            ActorCustomerId = employee.EmployeeId,
                            EmployeeNr = employee.EmployeeNr,
                            EmployeeName = employee.Name,
                        });
                    }
                }

                ResponseObject = q;
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }
    }
}
