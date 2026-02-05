using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftOne.Soe.Business.Core.Tests;
using System.Threading;
using SoftOne.Soe.Common.DTO.SoftOneLogger;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Core.Logger.Tests
{
    [TestClass()]
    public class LoggerManagerTests : TestBase
    {
        [TestMethod()]
        public async Task LogTest()
        {
           var dto1 = GetEmployeeDTO(-1);

            LoggerManager lm = new LoggerManager(GetParameterObject(17, 25));
            await lm.CreatePersonalDataLog(dto1, Guid.NewGuid(), TermGroup_PersonalDataActionType.Read);

            Assert.IsTrue(true);
        }

        [TestMethod()]
        public async Task LogTestList()
        {
            LoggerManager lm = new LoggerManager(GetParameterObject(17, 25));
            int countBatches = 100;

            while (countBatches > 0)
            {
                var dtos = new List<EmployeeDTO>();
                int count = 100;
                while (count > 0)
                {
                    dtos.Add(GetEmployeeDTO(-1 * (countBatches +  count)));
                    count--;
                }

                await lm.CreatePersonalDataLog(dtos, Guid.NewGuid(), TermGroup_PersonalDataActionType.Read);
                countBatches--;
            }

            Thread.Sleep(50000);
            Assert.IsTrue(true);
        }

        private EmployeeDTO GetEmployeeDTO(int employeeId)
        {
            EmployeeDTO dto = new EmployeeDTO();
            dto.SocialSec = "3294203948";
            dto.EmployeeId = employeeId;
            dto.EmploymentDate = null;
            dto.FirstName = null;
            dto.EmployeeTaxSE = new EmployeeTaxSEDTO();
            dto.EmployeeTaxSE.EmployeeId = employeeId;
            dto.Employments = new List<EmploymentDTO>();

            EmploymentDTO employment1 = new EmploymentDTO()
            {
                EmploymentId = 1,
                EmployeeId = employeeId,
                Comment = "Anställning 1",
            };
            dto.Employments.Add(employment1);

            EmploymentDTO employment2 = new EmploymentDTO()
            {
                EmploymentId = 2,
                EmployeeId = employeeId,
                Comment = "Anställning 2",
                PriceTypes = new List<EmploymentPriceTypeDTO>(),
            };
            employment2.PriceTypes.Add(new EmploymentPriceTypeDTO()
            {
                EmploymentId = employment2.EmploymentId,
            });
            dto.Employments.Add(employment2);

            return dto;
        }
    }
}