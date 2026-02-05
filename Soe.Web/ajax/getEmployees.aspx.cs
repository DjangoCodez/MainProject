using SoftOne.Soe.Business.Core;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getEmployees : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Queue q = new Queue();
            int position = 0;

            var employees = EmployeeManager.GetAllEmployees(SoeCompany.ActorCompanyId, active: true, getHidden: false, getVacant: false);
            foreach (var employee in employees)
            {
                q.Enqueue(new
                {
                    Found = true,
                    Position = position,
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.Name,
                });

                position++;
            }

            ResponseObject = q;
        }
    }
}
