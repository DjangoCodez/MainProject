using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ReportDataManagerTests : TestBase
    {
        [TestMethod()]
        public void UpdatePayrollSlipXMLtest()
        {
            ReportDataManager reportDataManager = new ReportDataManager(null);
            XDocument defaultxml = XDocument.Load(@"C:\temp\3.xml");
            string additionalLogInfo = "test";
            var doc = reportDataManager.TryUpdatePayrollSlipXML(defaultxml, additionalLogInfo);
            Assert.IsTrue(doc != null);
        }

        [TestClass()]
        public class AttestManagerTests
        {
            [TestMethod()]
            public void GetPayrollCalculationProductsTest()
            {
                TimeTreePayrollManager am = new TimeTreePayrollManager(null);
                am.GetPayrollCalculationProducts(291, 264, 239);

                Assert.Fail();
            }
        }

        [TestMethod()]
        public void TEMPPayrollSlipPrintoutsTest()
        {
            ReportDataManager reportDataManager = new ReportDataManager(null);
            var result = reportDataManager.TEMPPayrollSlipPrintouts();
            Assert.IsTrue(result != null);
        }

        [TestMethod()]
        public void StartMatrixGeneralTest()
        {
            MatrixResult matrixResult = new MatrixResult();
            matrixResult.MatrixDefinition = new MatrixDefinition();
            List<MatrixLayoutColumn> possibleColumns = new List<MatrixLayoutColumn>();
            Guid guid = Guid.NewGuid();
            possibleColumns.Add(new MatrixLayoutColumn(MatrixDataType.Date, guid.ToString(), "Date"));

            int rows = 2000;
            int row = 1;

            while (row <= rows)
            {
                List<MatrixField> fields = new List<MatrixField>();
                foreach (var column in possibleColumns)
                    fields.Add(new MatrixField(row, guid, DateTime.Now.AddDays(row), column.MatrixDataType));

                matrixResult.MatrixFields.AddRange(fields);
                row++;
            }

            ReportDataManager reportDataManager = new ReportDataManager(null);
            var result = reportDataManager.StartMatrixGeneric(matrixResult, 291, 193, 66, "testreport");
            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void CreateApiMatrixDataResultTest()
        {
            int reportId = 162338;
            int actorCompanyId = 2144720;
            int userId = 121473;
            int roleId = 9552;
            ApiMatrixDataSelection apiMatrixDataSelection = new ApiMatrixDataSelection();

            // Create a ApiMatrixDataSelection from "{"reportId":162338,"reportUserSelectionId":0,"apiMatrixDataSelectionDateRanges":[{"typeName":"string","key":"string","selectFrom":"2024-03-27T14:59:11.1060561+01:00","selectTo":"2024-03-27T14:59:11.1063536+01:00"}],"apiMatrixEmployeeSelections":{"typeName":"string","employeeIds":[],"employeeNumbers":[],"includeEnded":true,"includeHidden":false,"includeVacant":false},"apiMatrixColumnsSelection":{"typeName":"string","key":"string","apiMatrixColumnSelections":[{"field":"employeeNr"},{"field":"firstName"},{"field":"lastName"},{"field":"socialSec"},{"field":"email"},{"field":"cellPhone"},{"field":"firstEmploymentDate"},{"field":"endDate"},{"field":"employmentTypeName"},{"field":"workTimeWeekPercent"},{"field":"positionName"},{"field":"nearestExecutiveSocialSec"},{"field":"employmentDate"},{"field":"aGIPlaceOfEmploymentCity"},{"field":"aGIPlaceOfEmploymentAddress"},{"field":"payrollStatisticsPersonalCategory"},{"field":"aFACategory"},{"field":"accountInternalNrs5871"},{"field":"accountInternalNames5871"}]}}"

            apiMatrixDataSelection.ReportId = reportId;
            apiMatrixDataSelection.ReportUserSelectionId = 0;
            apiMatrixDataSelection.ApiMatrixDataSelectionDateRanges = new List<ApiMatrixDataSelectionDateRange>();
            apiMatrixDataSelection.ApiMatrixDataSelectionDateRanges.Add(new ApiMatrixDataSelectionDateRange()
            {
                TypeName = "string",
                Key = "string",
                SelectFrom = DateTime.Now,
                SelectTo = DateTime.Now
            });
            apiMatrixDataSelection.ApiMatrixEmployeeSelections = new ApiMatrixEmployeeSelection()
            {
                TypeName = "string",
                EmployeeIds = new List<int>(),
                EmployeeNumbers = new List<string>(),
                IncludeEnded = true,
                IncludeHidden = false,
                IncludeVacant = false
            };
            apiMatrixDataSelection.ApiMatrixColumnsSelection = new ApiMatrixColumnsSelection()
            {
                TypeName = "string",
                Key = "string",
                ApiMatrixColumnSelections = new List<ApiMatrixColumnSelection>()
                {
                    new ApiMatrixColumnSelection() { Field = "employeeNr" },
                    new ApiMatrixColumnSelection() { Field = "firstName" },
                    new ApiMatrixColumnSelection() { Field = "lastName" },
                    new ApiMatrixColumnSelection() { Field = "socialSec" },
                    new ApiMatrixColumnSelection() { Field = "email" },
                    new ApiMatrixColumnSelection() { Field = "cellPhone" },
                    new ApiMatrixColumnSelection() { Field = "firstEmploymentDate" },
                    new ApiMatrixColumnSelection() { Field = "endDate" },
                    new ApiMatrixColumnSelection() { Field = "employmentTypeName" },
                    new ApiMatrixColumnSelection() { Field = "workTimeWeekPercent" },
                    new ApiMatrixColumnSelection() { Field = "positionName" },
                    new ApiMatrixColumnSelection() { Field = "nearestExecutiveSocialSec" },
                    new ApiMatrixColumnSelection() { Field = "employmentDate" },
                    new ApiMatrixColumnSelection() { Field = "aGIPlaceOfEmploymentCity" },
                    new ApiMatrixColumnSelection() { Field = "aGIPlaceOfEmploymentAddress" },
                    new ApiMatrixColumnSelection() { Field = "payrollStatisticsPersonalCategory" },
                    new ApiMatrixColumnSelection() { Field = "aFACategory" },
                    new ApiMatrixColumnSelection() { Field = "accountInternalNrs5871" },
                    new ApiMatrixColumnSelection() { Field = "accountInternalNames5871" }
                }
            };


            var param = GetParameterObject(actorCompanyId,userId, roleId);
            ReportDataManager reportDataManager = new ReportDataManager(param);
            var result = reportDataManager.CreateApiMatrixDataResult(apiMatrixDataSelection, actorCompanyId, Guid.NewGuid());
            Assert.IsTrue(result != null);
        }
    }
}