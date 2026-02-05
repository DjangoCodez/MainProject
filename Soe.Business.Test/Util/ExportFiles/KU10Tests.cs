using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.ExportFiles;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Linq;

namespace Soe.Business.Test.Util.ExportFiles
{
    [TestClass()]
    public class KU10Tests
    {
        [TestMethod()]
        public void Testku10()
        {
            using (CompEntities entities = new CompEntities())
            {
                EvaluatedSelection es = new EvaluatedSelection();
                es.ActorCompanyId = 30449;
                es.UserId = 854;
                es.ST_EmployeeIds = entities.Employee.Where(w => w.ActorCompanyId == es.ActorCompanyId && w.State == (int)SoeEntityState.Active).Select(s => s.EmployeeId).Take(2).ToList();
                es.DateFrom = new DateTime(2018, 1, 1);
                es.DateTo = new DateTime(2018, 12, 31);
                es.HasDateInterval = true;
                es.ST_TimePeriodIds = entities.TimePeriod.Where(w => w.TimePeriodHead.ActorCompanyId == es.ActorCompanyId && w.PaymentDate > es.DateFrom && w.PaymentDate < es.DateTo && w.State == (int)SoeEntityState.Active).Select(s => s.TimePeriodId).ToList();

                CreateReportResult reportResult = new CreateReportResult();
                reportResult.EvaluatedSelection = es;

                KU10 ku10 = new KU10(ParameterObject.Empty(), reportResult);
                var file = ku10.CreateKU10File(entities);
                Assert.IsTrue(file != null);
            }
        }
    }
}
