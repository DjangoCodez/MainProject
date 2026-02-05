using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.ExportFiles;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.ExportFiles.Tests
{
    [TestClass()]
    public class SCBStatisticsFilesTests
    {
        [TestMethod()]
        public void CreateSCB_SLPPayrollStatisticsFileHeadDTOTest()
        {
            TimeTransactionManager ttm = new TimeTransactionManager(null);
            EmployeeManager em = new EmployeeManager(null);

            using (SOECompEntities entities = new SOECompEntities())
            {

                int actorCompanyId = 701609;
                var employees = em.GetAllEmployees(entities, actorCompanyId, loadEmployment: true);
                employees = employees.OrderBy(e => e.EmployeeId).Take(20).ToList();
                TimePeriodManager tpm = new TimePeriodManager(null);
                var timePeriods = tpm.GetTimePeriods(entities, TermGroup_TimePeriodType.Payroll, actorCompanyId).Where(p => p.StartDate > new DateTime(2017, 12, 31)).ToList();

               // var transFull = ttm.GetTimePayrollStatisticsDTOs(entities, actorCompanyId, employees, timePeriods.Select(t => t.TimePeriodId).ToList());
                CreateReportResult r = new CreateReportResult();
                r.EvaluatedSelection = new Soe.Common.DTO.EvaluatedSelection();
                r.EvaluatedSelection.ST_TimePeriodIds = timePeriods.Select(t => t.TimePeriodId).ToList();
                r.EvaluatedSelection.ST_EmployeeIds = employees.Select(s => s.EmployeeId).ToList();
                r.EvaluatedSelection.ActorCompanyId = 701609;
                r.EvaluatedSelection.DateTo = DateTime.Now;
                SCBStatisticsFiles f = new SCBStatisticsFiles(GetParameterObject(701609, 22), r);

                TimeReportDataManager tu = new TimeReportDataManager(GetParameterObject(701609, 22));
                tu.Create_SCB_SN_ReportData(r,false);
            }

        }

        public ParameterObject GetParameterObject(int actorCompanyId, int userId)
        {
            Company company = null;
            if (actorCompanyId > 0)
                company = new CompanyManager(null).GetCompany(actorCompanyId);

            User user = null;
            if (userId > 0)
                user = new UserManager(null).GetUser(userId);
            else
                user = new User() { LoginName = this.ToString() };

            return new ParameterObject()
            {
                SoeCompany = company.ToCompanyDTO(),
                SoeUser = user.ToDTO(),
                Thread = "test",
            };
        }
    }
}