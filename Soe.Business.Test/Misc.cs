using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace Soe.Business.Test
{
    [TestClass]
    public class Misc
    {
        [TestMethod]
        public void GetAllEmployees()
        {
            EmployeeManager em = new EmployeeManager(null);
            var result = em.GetAllEmployees(7).ToDTOs();
            Assert.IsTrue(result != null);
        }

        [TestMethod]
        public void FixIncorrectEmploymentPercent()
        {
            EmployeeManager em = new EmployeeManager(null);
            StringBuilder sb = new StringBuilder();

            using (CompEntities entities = new CompEntities())
            {
                foreach (var item in entities.Company.Where(w => w.License.LicenseNr == "8000").ToList())
                {
                    var allEmployees = em.GetAllEmployees(entities, item.ActorCompanyId, true, true);

                    foreach (var employee in allEmployees)
                    {
                        bool employeeHasChanges = false;

                        foreach (var employment in employee.GetActiveEmployments())
                        {
                            var employeeGroup = employment.GetEmployeeGroup(employment.GetEndDate());

                            if (employeeGroup == null)
                                continue;

                            decimal percentOnEmployee = employment.GetPercent(employment.GetEndDate());
                            decimal percent = decimal.Round(decimal.Divide(employment.GetWorkTimeWeek(employment.GetEndDate()), employment.GetFullTimeWorkTimeWeek(employeeGroup, employment.GetEndDate())) * 100, 2);

                            if (percent != percentOnEmployee && decimal.Multiply(Math.Abs(decimal.Subtract(percent, percentOnEmployee)), 100) > 5)
                            {
                                if (employment.OriginalPercent == percentOnEmployee)
                                {
                                    sb.Append($"{item.Name} Employee {employee.EmployeeNrAndName} OrginalPercent {employment.OriginalPercent} CalculatePercent {percent} worktimeminutes {employment.GetWorkTimeWeek(employment.GetEndDate())}" + Environment.NewLine);
                                    employeeHasChanges = true;
                                    employment.OriginalPercent = percent;
                                    var created = employment.Created.HasValue ? employment.Created.Value : DateTime.Now;
                                    employment.Created = new DateTime(created.Year, created.Month, created.Day, created.Hour, created.Minute, 0, 999);
                                }

                                var changes = entities.EmploymentChange.Where(w => w.FieldType == (int)TermGroup_EmploymentChangeFieldType.Percent && w.EmploymentId == employment.EmploymentId).ToList();

                                if (!changes.IsNullOrEmpty())
                                {
                                    foreach (var change in changes)
                                    {
                                        if (change.ToValue == percentOnEmployee.ToString())
                                        {
                                            sb.Append($"{item.Name} Employee {employee.EmployeeNrAndName} change.ToValue {change.ToValue} CalculatePercent {percent} worktimeminutes {employment.GetWorkTimeWeek(employment.GetEndDate())}" + Environment.NewLine);
                                            change.FromValueName = change.ToValue;
                                            change.ToValue = percent.ToString();
                                            employeeHasChanges = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (employeeHasChanges)
                        {
                            var changes = entities.SaveChanges();

                            if (changes == 0)
                                throw new Exception("changes == 0");
                        }
                    }
                }

                string result = sb.ToString();
            }

            Assert.IsTrue(true);
        }

        public void FixEfProviderServicesProblem()
        {
            //The Entity Framework provider type 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'
            //for the 'System.Data.SqlClient' ADO.NET provider could not be loaded. 
            //Make sure the provider assembly is available to the running application. 
            //See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.

            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        [TestMethod]
        public void FizzBuzz()
        {
            List<string> list = new List<string>();

            for (int i = 0; i <= 100; i++)
            {
                bool isFizz = i % 3 == 0;
                bool isBuzz = i % 5 == 0;
                string value = string.Empty;

                if (isFizz)
                {
                    value = "Fizz";
                    if (isBuzz)
                        value += "Buzz";
                }
                else if (isBuzz)
                {
                    value = "Buzz";
                }
                else
                    value = i.ToString();

                list.Add(value);
            }

            Assert.IsTrue(true);

        }

        [TestMethod]
        public void GetAllEmployeeList()
        {
            EmployeeManager em = new EmployeeManager(null);
            var result = em.GetAllEmployees(7).ToDTOs();
            Assert.IsTrue(result != null);
        }

        [TestMethod]
        [Ignore]
        public void Fourit()
        {
            string sessionToken = @"=AzY{{YTY$$EWMwMWN[[0SZjRmYtYzN$$QTL{{//DO{{0SY{{EDZz//TN$$8VN\\FzM6YTM6MTMggDMtETMtYTMwIzX##MmZlRTOhZmMhlzNtAjNzEWL$$MjZ00CZxImYtUWY[[Y$$YlV$$N\\NWY[[QTY0QWZlFmYzYWO##gjM[[ADNzIDOhZTNwkDM££Y$$Y";
            string userId = "72";
            string companyApiKey = "7eecf7ae-bb1d-4f36-a360-79a2fa94efc2";
            string connectApiKey = "6573d15a-5875-4676-bdce-75c01a6a65c0";
            string URI = "http://52.166.139.254/4decisiondev/LoginSSO.aspx";
            string myParameters = $"SessionToken={sessionToken}&UserId={userId}&CompanyApiKey={companyApiKey}&ConnectApiKey={connectApiKey}";
            var data = new NameValueCollection();
            data["SessionID"] = System.Web.HttpUtility.UrlEncode(sessionToken);
            data["UserID"] = System.Web.HttpUtility.UrlEncode(userId);
            data["CompanyApiKey"] = System.Web.HttpUtility.UrlEncode(companyApiKey);
            data["ConnectApiKey"] = System.Web.HttpUtility.UrlEncode(connectApiKey);

            using (WebClient wc = new WebClient())
            {
                var response = wc.UploadValues(URI, "POST", data);
                string ss = Encoding.Default.GetString(response);
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string HtmlResult = wc.UploadString(URI, myParameters);
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void FixMissingPayrollSlips()
        {
            int timePeriodId = 22436;//19364;//19344;//22512;
            int actorCompanyId = 711071;//701609;
            int userId = 38858;
            int roleId = 0;
            EmployeeManager em = new EmployeeManager(null);
            ReportDataManager rdm = new ReportDataManager(null);
            GeneralManager gm = new GeneralManager(null);

            using (CompEntities entities = new CompEntities())
            {
                #region PayrollSlip

                var employees = em.GetAllEmployees(actorCompanyId).Where(e => e.State == 0);

                foreach (var employee in employees)
                {
                    if (employee.State == 0 && entities.EmployeeTimePeriod.Where(t => t.TimePeriodId == timePeriodId && t.EmployeeId == employee.EmployeeId && t.State == 0).Count() > 0)
                    {
                        if (entities.DataStorage.Where(s => s.ActorCompanyId == actorCompanyId && s.EmployeeId == employee.EmployeeId && s.TimePeriodId == timePeriodId && s.State == 0).Count() == 0)
                        {
                            EvaluatedSelection es = new EvaluatedSelection();
                            es.ReportTemplateType = SoeReportTemplateType.PayrollSlip;
                            es.ActorCompanyId = actorCompanyId;
                            es.ST_TimePeriodIds = new List<int>() { timePeriodId };
                            es.ST_EmployeeIds = new List<int>() { employee.EmployeeId };
                            es.UserId = userId;
                            es.RoleId = roleId;
                            //        XDocument xdoc = rdm.CreateReportXML(entities, es);
                            //        DataStorage dataStorage = gm.CreateDataStorage(entities, SoeDataStorageRecordType.PayrollSlipXML, xdoc.ToString(), null, timePeriodId, employee.EmployeeId, actorCompanyId);

                            //       entities.SaveChanges();
                        }
                    }
                }

                #endregion
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestFixedValue()
        {
            PayrollManager pm = new PayrollManager(null);
            ProductManager prm = new ProductManager(null);
            int actorCompanyId = 701609;

            using (CompEntities entities = new CompEntities())
            {
                var prod = prm.GetPayrollProductByNumber(entities, "41210", actorCompanyId);
                var ss = pm.IsAbsencePercentSameEntirePeriod(entities, actorCompanyId, 53701, new DateTime(2016, 6, 20), prod, new EvaluatePayrollPriceFormulaInputDTO());
            }

            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void AbsencePercentSameEntirePeriodTest()
        {
            PayrollManager pm = new PayrollManager(null);
            ProductManager prm = new ProductManager(null);
            int actorCompanyId = 701609;
            int employeeId = 107187;

            using (CompEntities entities = new CompEntities())
            {
                PayrollProduct payrollProduct = prm.GetPayrollProduct(entities, 1568231); //Tled
                bool value = pm.IsAbsencePercentSameEntirePeriod(entities, actorCompanyId, employeeId, new DateTime(2022, 06, 30), payrollProduct, new EvaluatePayrollPriceFormulaInputDTO());
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void GetTimeFromMinutes()
        {
            decimal amount = 129;
            decimal value = amount;
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);

            int hours = (int)value;
            int minutes;
            minutes = Convert.ToInt32((amount - (hours * 60)));
            minutes = Math.Abs(minutes);
            minutes = (int)Decimal.Divide(minutes * 100, 60);
            minutes = Math.Abs(minutes);

            //0 - 0,12”timmar” => 0 timmar
            //0,13 - 0,37 => 0,25 timmar
            //0,38 - 0,62 => 0,5 timmar
            //0,63 - 0,87 => 0,75 timmar
            //0,87 - 0,99 =>1,0 timmar

            if (minutes <= 12)
                minutes = 0;

            if (minutes > 12 && minutes <= 37)
                minutes = 25;

            if (minutes > 37 && minutes <= 62)
                minutes = 50;

            if (minutes > 62 && minutes <= 87)
                minutes = 75;

            if (minutes > 87)
            {
                hours++;
                minutes = 0;
            }

            //return hours.minutes i.e 8h3m -> 8.03
            var result = hours.ToString() + "." + (minutes.ToString().Length == 1 ? "0" + minutes.ToString() : minutes.ToString());
            Assert.IsTrue(result != null);
        }
    }
}



