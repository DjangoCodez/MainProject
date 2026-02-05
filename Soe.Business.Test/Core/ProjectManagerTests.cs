using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ProjectManagerTests : TestBase
    {
        [TestMethod()]
        public void AddTimeRowBy2UsersWithSalary()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 7;
            int userId = 72;
            int roleId = 3;

            Task.Run(() =>
            {
                var projectManager = new ProjectManager(GetParameterObject(actorCompanyId, userId, roleId));

                var itemToSave = new ProjectTimeBlockSaveDTO
                {
                    ProjectTimeBlockId = 0,
                    CustomerInvoiceId = 41989,
                    ProjectId = 11058,
                    EmployeeId = 66,
                    Date = DateTime.Today,
                    From = new DateTime(1900, 1, 1, 7, 0, 0),
                    To = new DateTime(1900, 1, 1, 12, 0, 0),
                    InvoiceQuantity = 60,
                    ExternalNote = "Rad 1 " + DateTime.Now.ToString(),
                    TimeCodeId = 2,
                    TimeDeviationCauseId = 3,
                    TimePayrollQuantity = 60,
                    AutoGenTimeAndBreakForProject = true,
                };

                var saveResult1 = projectManager.SaveProjectTimeBlock(itemToSave);
                Debug.WriteLine(saveResult1.ErrorMessage);
                Assert.IsTrue(saveResult1.Success);
            }).ConfigureAwait(false);

            Task.Run(() =>
            {
                var projectManager = new ProjectManager(GetParameterObject(actorCompanyId,userId, roleId));

                var itemToSave = new ProjectTimeBlockSaveDTO
                {
                    ProjectTimeBlockId = 0,
                    CustomerInvoiceId = 41989,
                    ProjectId = 11058,
                    EmployeeId = 1,
                    Date = DateTime.Today,
                    From = new DateTime(1900, 1, 1, 7, 0, 0),
                    To = new DateTime(1900, 1, 1, 12, 0, 0),
                    InvoiceQuantity = 60,
                    ExternalNote = "Rad 2 " + DateTime.Now.ToString(),
                    TimeCodeId = 2,
                    TimeDeviationCauseId = 3,
                    TimePayrollQuantity = 60,
                    AutoGenTimeAndBreakForProject = true,
                };

                var saveResult2 = projectManager.SaveProjectTimeBlock(itemToSave);
                Debug.WriteLine(saveResult2.ErrorMessage);
                Assert.IsTrue(saveResult2.Success);
            }).ConfigureAwait(false);

            Thread.Sleep(300000);
        }
    }
}