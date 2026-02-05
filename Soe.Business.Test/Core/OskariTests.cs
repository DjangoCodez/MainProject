using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class OskariTests : TestBase
    {
        [TestMethod()]
        public void GetEmployeeInformationsTest_St1()
        {
            int actorCompanyId = 2267454;   // St1 Demo (2024-09-05 Christoffergren AB)
            int userId = 96532;             // solotes importer
            int roleId = 9544;              // solotes_importer_role

            ParameterObject parameterObject = GetParameterObject(actorCompanyId, userId, roleId);
            ImportExportManager iem = new ImportExportManager(parameterObject);
            ConfigurationSetupUtil.Init();

            var model = new FetchEmployeeInformation
            {
                DateFrom = new DateTime(1900, 1, 1),
                DateTo = new DateTime(9999, 12, 31),
                //EmployeeNrs = new List<string> { "0" },
                LoadContactInformation = true,
                LoadEmployments = true,
                LoadEmploymentAccounts = true,
                LoadEmploymentChanges = true,
                SetInitialValuesOnEmployment = true,
                LoadCalenderInfo = false,
                OnePerChangeCalenderDayInfo = true,
                LoadVacationInfo = true,
                LoadVacationInfoHistory = true,
                LoadHierarchyAccounts = true,
                LoadPositions = true,
                LoadReportSettings = true,
                LoadExtraFields = true,
                LoadSkills = true,
                LoadUser = true,
                LoadUserRoles = true,
                LoadSocialSec = true,
                EmployeesChangedOrAddedAfterUtc = new DateTime(1900, 1, 1),
                AddVirtualHierarchyAccounts = true,
                LoadExecutives = true,
            };

            var result = iem.GetEmployeeInformations(actorCompanyId, model);
            Assert.IsTrue(result != null);

            SaveResultToJsonFile(result, @"D:\TEMP\20241004\EmployeeInformations.json", true);
        }

        [TestMethod()]
        public void GetTimeScheduleInfoTest_Orkla()
        {
            ConfigurationSetupUtil.Init();
            int actorCompanyId = 752535; // Orkla
            int userId = 123678; // API USER
            int roleId = 10164; // API
            var importExportManager = new ImportExportManager(GetParameterObject(actorCompanyId, userId, roleId));

            DateTime fromDate = DateTime.Parse("2024-01-16");
            DateTime toDate = DateTime.Parse("2024-01-31");
            List<String> employeeNumbers = new List<string>();
            //List<String> employeeNumbers = new List<string> { "40519" };
            bool addAmounts = true;
            bool loadSchedule = false;
            bool loadPayroll = true;

            var dto = importExportManager.GetTimeScheduleInfo(actorCompanyId, fromDate, toDate, employeeNumbers, addAmounts, loadSchedule, loadPayroll);
            var json = JsonConvert.SerializeObject(dto);
            Assert.IsTrue(json != null);

            string filepath = @"D:\OneDrive\Orkla\JsonToHtmlReport\data";
            string filename = @"GetTimeScheduleInfoTest_Orkla";

            SaveResultToJsonFile(dto, filepath + @"\" + filename + $"_{addAmounts}_{loadSchedule}_{loadPayroll}.json", true);
        }

        private void SaveResultToJsonFile(object result, string jsonfile, bool writeIndented = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA),
                WriteIndented = writeIndented

            };

            string jsonresult = System.Text.Json.JsonSerializer.Serialize(result, options);
            File.WriteAllText(jsonfile, jsonresult);
        }
    }
}
