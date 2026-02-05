using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    public class Visma600Adapter : ISalaryAdapter
    {
        private const String FILE_VERSION = "1.00";
        private const String FILE_EXPORT_VERSION = "1.3";
        private const String FILE_TYPE = "SalaryData";
        //private const String VALUE_SIGN = "'";

        readonly private List<TransactionItem> allTransactionItems;
        readonly private List<TransactionItem> timeTransactionItems;
        readonly private List<TransactionItem> outlayTransactions;
        readonly private List<ScheduleItem> scheduleItems;
        readonly private Company company;
        readonly private List<PayrollProduct> payrollProducts;
        readonly private DateTime startDate;
        readonly private DateTime stopDate;
        readonly private List<int> employeeIds;

        #region Constructors

        public Visma600Adapter(Company company, List<PayrollProduct> payrollProducts, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<int> employeeIds, DateTime startDate, DateTime stopDate)
        {
            this.allTransactionItems = payrollTransactions;
            //this.timeTransactionItems = payrollTransactions.Where(p => p.PayrollType != TermGroup_PayrollType.Addition && p.PayrollType != TermGroup_PayrollType.Deduction).ToList();
            this.timeTransactionItems = payrollTransactions.Where(p => !p.IsAddition() && !p.IsDeduction()).ToList();
            //this.outlayTransactions = payrollTransactions.Where(p => p.PayrollType == TermGroup_PayrollType.Addition || p.PayrollType == TermGroup_PayrollType.Deduction).ToList();
            this.outlayTransactions = payrollTransactions.Where(p => p.IsAddition() || p.IsDeduction()).ToList();
            this.scheduleItems = scheduleItems;
            this.company = company;
            this.payrollProducts = payrollProducts;
            this.startDate = startDate;
            this.stopDate = stopDate;
            this.employeeIds = employeeIds;
        }

        #endregion

        public byte[] TransformSalary(XDocument baseXml)
        {
            XDocument doc = CreateDocumentDeclaration();
            doc.Add(CreateSalaryData());
            MemoryStream stream = new MemoryStream();
            doc.Save(stream);
            return stream.ToArray();
        }

        #region XML structure methods

        private XDocument CreateDocumentDeclaration()
        {
            XDocument doc = new XDocument(
                 new XDeclaration("1.0", Constants.ENCODING_LATIN1_NAME, "true"));

            return doc;
        }

        private XElement CreateSalaryData()
        {
            XElement salaryData = new XElement("SalaryData",
                  new XAttribute("ProgramName", GetString("SoftOne")),
                  new XAttribute("Version", GetString("")),
                  new XAttribute("ExportVersion", GetString(FILE_EXPORT_VERSION)),
                  new XAttribute("Created", GetString(DateTime.Now.ToShortDateString())),
                  new XAttribute("Type", GetString("SalaryData")),
                  new XAttribute("Language", GetString("Swedish")),
                  new XAttribute("CompanyName", GetString(company.Name)),
                  new XAttribute("OrgNo", GetString(company.OrgNr)),
                  new XAttribute("Imported", GetString("0")),
                  new XAttribute("BookDistributionProject", GetString("0")), //0 for now
                  new XAttribute("BookDistributionResultUnit", GetString("0"))); //0 for now

            salaryData.Add(CreateTimeCodes());
            salaryData.Add(CreateProjects());
            salaryData.Add(CreateResultUnits());
            salaryData.Add(CreateSalaryDataEmployee());
            return salaryData;
        }

        private XElement CreateTimeCodes()
        {
            XElement timeCodes = new XElement("TimeCodes");
            foreach (var payrollProduct in payrollProducts)
            {
                timeCodes.Add(CreateTimeCode(payrollProduct));
            }
            return timeCodes;
        }

        private XElement CreateTimeCode(PayrollProduct payrollProduct)
        {
            String absencetype = String.Empty;
            String regularworkingTime = String.Empty;

            if (payrollProduct.IsWork())
                absencetype = "0";
            else if (payrollProduct.IsAbsencePayrollExport())

                if (absencetype == "0")
                    regularworkingTime = "1";
                else if (payrollProduct.IsOvertimeCompensation() || payrollProduct.IsAddedTime())
                    regularworkingTime = "0";


            XElement timeCode = new XElement("TimeCode",
                  new XAttribute("Code", GetString(payrollProduct.ExternalNumberOrNumber)),
                  new XAttribute("TimeCodeName", GetString(payrollProduct.Name)),
                  new XAttribute("AbsenceType", GetString(absencetype)),
                  new XAttribute("RegularworkingTime", GetString(regularworkingTime)),
                  new XAttribute("ConversionFactorTime", GetString("")),
                  new XAttribute("Active", GetString("")));

            return timeCode;
        }

        private XElement CreateProjects()
        {
            XElement projects = new XElement("Projects");

            List<Tuple<String, String>> distinctProjects = SalaryExportUtil.GetDistinctAccounts(TermGroup_SieAccountDim.Project, allTransactionItems);
            foreach (var item in distinctProjects)
            {
                projects.Add(CreateProject(item.Item1, item.Item2));
            }
            return projects;
        }

        private XElement CreateProject(String accountNr, String accountName)
        {
            XElement project = new XElement("Project",
                  new XAttribute("Code", GetString(accountNr)),
                  new XAttribute("Description", GetString(accountName)));
            return project;
        }

        private XElement CreateResultUnits()
        {
            XElement resultUnits = new XElement("ResultUnits");
            List<Tuple<String, String>> distinctResultUnits = SalaryExportUtil.GetDistinctAccounts(TermGroup_SieAccountDim.CostCentre, allTransactionItems);

            foreach (var item in distinctResultUnits)
            {
                resultUnits.Add(CreateResultUnit(item.Item1, item.Item2));
            }
            return resultUnits;
        }

        private XElement CreateResultUnit(String accountNr, String accountName)
        {
            XElement resultUnit = new XElement("ResultUnit",
                new XAttribute("Code", GetString(accountNr)),
                new XAttribute("Description", GetString(accountName)));
            return resultUnit;
        }

        private XElement CreateSalaryDataEmployee()
        {
            XElement salaryDataEmployee = new XElement("SalaryDataEmployee",
                new XAttribute("FromDate", GetString(startDate)),
                new XAttribute("ToDate", GetString(stopDate)));

            foreach (var employeeId in employeeIds)
            {
                var employee = CreateEmployee(employeeId);
                if (employee != null)
                    salaryDataEmployee.Add(employee);
            }
            return salaryDataEmployee;
        }

        private XElement CreateEmployee(int employeeId)
        {
            List<ScheduleItem> scheduleItemsForEmployee = new List<ScheduleItem>();
            scheduleItemsForEmployee = scheduleItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();
            List<TransactionItem> transactionItemsForEmployee = new List<TransactionItem>();
            transactionItemsForEmployee = timeTransactionItems.Where(s => s.EmployeeId == employeeId.ToString()).ToList();

            //Get Timeaccumlator
            //EmployeeManager em = new EmployeeManager(null);
            //Employee employeeEntity = em.GetEmployee(employeeId);
            List<TimeAccumulatorItem> timeAccumlators = new List<TimeAccumulatorItem>();

            //int employeeGroupId = employeeEntity.GetEmployeeGroup(startDate).EmployeeGroupId;

            //if (employeeGroupId != null)
            //{
            //    TimeAccumulatorManager tam = new TimeAccumulatorManager(null);
            //    timeAccumlators = tam.GetTimeAccumulatorItems(startDate, stopDate, employeeId, employeeGroupId, company.ActorCompanyId, false, false, true, false, false);

            //}

            //An employee may have work but not has benn scheduled to work
            if (scheduleItemsForEmployee.Count > 0 || transactionItemsForEmployee.Count > 0)
            {
                String empNr = string.Empty;
                String firstName = string.Empty;
                String name = string.Empty;
                String personalNo = string.Empty;
                ScheduleItem firstScheduleItem = scheduleItemsForEmployee.FirstOrDefault();
                TransactionItem firstTransactionItem = transactionItemsForEmployee.FirstOrDefault();

                if (firstTransactionItem != null)
                {
                    empNr = firstTransactionItem.EmployeeNr;
                    firstName = firstTransactionItem.EmployeeFirstName;
                    name = firstTransactionItem.EmployeeLastName;
                    personalNo = firstTransactionItem.EmployeeSocialSec;
                }
                else if (firstScheduleItem != null)
                {
                    empNr = firstScheduleItem.EmployeeNr;
                    firstName = firstScheduleItem.EmployeeFirstName;
                    name = firstScheduleItem.EmployeeLastName;
                    personalNo = firstScheduleItem.EmployeeSocialSec;
                }

                XElement employee = new XElement("Employee",
                      new XAttribute("EmploymentNo", GetString(empNr)),
                      new XAttribute("FirstName", GetString(firstName)),
                      new XAttribute("Name", GetString(name)),
                      new XAttribute("PersonalNo", GetString(personalNo)),
                      new XAttribute("HourlyWage", GetString("")),
                      new XAttribute("EmplCategory", GetString("")),
                      new XAttribute("FromDate", GetString(startDate)),
                      new XAttribute("ToDate", GetString(stopDate)));

                employee.Add(CreateNormalWorkTimes(employeeId));
                employee.Add(CreateTimes(employeeId));
                employee.Add(CreateTimeAdjustments());
                employee.Add(CreateTimeBalances(timeAccumlators));
                //employee.Add(CreateBookDistributionProjects());
                //employee.Add(CreateBookDistributionResultUnits());
                employee.Add(CreateRegOutlays());
                return employee;
            }
            else
                return null;
        }

        private XElement CreateNormalWorkTimes(int employeeId)
        {
            XElement normalWorkingTimes = new XElement("NormalWorkingTimes");
            List<ScheduleItem> scheduleItemsForEmployee = new List<ScheduleItem>();
            scheduleItemsForEmployee = scheduleItems.Where(s => s.EmployeeId == employeeId.ToString()).OrderBy(o => o.Date).ToList();
            foreach (var item in scheduleItemsForEmployee)
            {
                normalWorkingTimes.Add(CreateNormalWorkTime(item.Date, item.TotalMinutes - item.TotalBreakMinutes));
            }
            return normalWorkingTimes;
        }

        private XElement CreateNormalWorkTime(DateTime date, double time)
        {
            XElement normalWorkTime = new XElement("NormalWorkingTime",
                new XAttribute("NormalWorkingTimeHours", GetString(SalaryExportUtil.GetTimeFromMinutes(time.ToString()))),
                  new XAttribute("DateOfReport", GetString(date)));

            return normalWorkTime;
        }

        private XElement CreateTimes(int employeeId)
        {
            XElement times = new XElement("Times");
            List<TransactionItem> transactionItemsForEmployee = new List<TransactionItem>();
            transactionItemsForEmployee = timeTransactionItems.Where(s => s.EmployeeId == employeeId.ToString()).OrderBy(o => o.Date).ToList();
            foreach (var item in transactionItemsForEmployee)
            {
                times.Add(CreateTime(item));
            }
            return times;
        }

        private XElement CreateTime(TransactionItem transaction)
        {
            XElement time = new XElement("Time",
                 new XAttribute("DateOfReport", GetString(transaction.Date)),
                 new XAttribute("TimeCode", GetString(transaction.ProductNr)),
                 new XAttribute("SumOfHours", GetString(SalaryExportUtil.GetTimeFromMinutes(transaction.Quantity.ToString()))),
                 new XAttribute("ProjectCode", GetString(BusinessExportUtil.GetAccountNr(TermGroup_SieAccountDim.Project, transaction.AccountInternals))),
                 new XAttribute("ResultUnitCode", GetString(BusinessExportUtil.GetAccountNr(TermGroup_SieAccountDim.CostCentre, transaction.AccountInternals))));
            return time;
        }

        private XElement CreateTimeAdjustments()
        {
            XElement timeAdjustments = new XElement("TimeAdjustments");
            //List<int> list = new List<int>();
            //foreach (var item in list)
            //{
            //    timeAdjustments.Add(CreateTimeAdjustment());
            //}         
            return timeAdjustments;
        }

        private XElement CreateTimeAdjustment()
        {
            XElement adjustment = new XElement("Adjustment",
                 new XAttribute("Date", GetString("")),
                 new XAttribute("TimeCode", GetString("")),
                 new XAttribute("Hours", GetString("")),
                 new XAttribute("Comment", GetString("")));
            return adjustment;
        }

        private XElement CreateTimeBalances(List<TimeAccumulatorItem> timeAccumulators)
        {
            XElement timeBalances = new XElement("TimeBalances");
            decimal summery = 0;

            if (timeAccumulators.Count != 0)
            {

                foreach (TimeAccumulatorItem timeacc in timeAccumulators)
                {
                    string name = timeacc.Name;
                    decimal quantity = timeacc.SumInvoicePeriod + timeacc.SumPayrollPeriod + timeacc.SumTimeCodePeriod;

                    timeBalances.Add(CreateTimeBalance(name, quantity, false));

                    summery += quantity;
                }

            }

            timeBalances.Add(CreateTimeBalance("sum", summery, true));

            return timeBalances;
        }

        private XElement CreateTimeBalance(string name, decimal quantity, bool isSummery)
        {
            string q = "0,00";
            string n = "##SumComp##";

            if (quantity != 0 && !isSummery)
            {
                q = (quantity / 60).ToString("#.##");
                n = name;
            }

            XElement timeBalance = new XElement("TimeBalance",
                new XAttribute("TimeCode", GetString(n)),
                //new XAttribute("PeriodRegHours", GetString("")),
                //new XAttribute("ConvPeriodRegHours", GetString("")),
                //new XAttribute("AccRegHours", GetString("")),
                new XAttribute("ConvAccRegHours", GetString(q)));
            return timeBalance;
        }

        private XElement CreateBookDistributionProjects()
        {
            XElement bookDistributionProjects = new XElement("BookDistributionProjects");
            //List<int> list = new List<int>();
            //foreach (var item in list)
            //{
            //    bookDistributionProjects.Add(CreateBookDistributionProject());
            //}
            return bookDistributionProjects;
        }

        private XElement CreateBookDistributionProject()
        {
            XElement bookDistributionProject = new XElement("BookDistributionProject",
               new XAttribute("ProjectCode", GetString("")),
               new XAttribute("Distribution", GetString("")));
            return bookDistributionProject;
        }

        private XElement CreateBookDistributionResultUnits()
        {
            XElement bookDistributionResultUnits = new XElement("BookDistributionResultUnits");
            //List<int> list = new List<int>();
            //foreach (var item in list)
            //{
            //    bookDistributionResultUnits.Add(CreateBookDistributionResultUnit());
            //}
            return bookDistributionResultUnits;
        }

        private XElement CreateBookDistributionResultUnit()
        {
            XElement bookDistributionResultUnit = new XElement("BookDistributionResultUnit",
              new XAttribute("ResultUnitCode", GetString("")),
              new XAttribute("Distribution", GetString("")));
            return bookDistributionResultUnit;
        }

        private XElement CreateRegOutlays()
        {
            XElement regOutlays = new XElement("RegOutlays");
            foreach (var item in outlayTransactions)
            {
                regOutlays.Add(CreateRegOutlay(item));
            }
            return regOutlays;
        }

        private XElement CreateRegOutlay(TransactionItem outlay)
        {
            String outlaytype = String.Empty;
            String comment = String.Empty;
            if (!String.IsNullOrEmpty(outlay.Comment))
            {
                comment = outlay.Comment.Length > 50 ? outlay.Comment.Substring(0, 50) : outlay.Comment;
                outlaytype = comment;
            }
            else
                outlaytype = outlay.ProductNr + "_" + outlay.ProductName;

            XElement regOutlay = new XElement("RegOutlay",
              new XAttribute("DateOfReport", GetString(outlay.Date)),
              new XAttribute("OutlayCode", GetString(outlay.ProductNr)),
              new XAttribute("OutlayCodeName", GetString(outlay.ProductName)),
              new XAttribute("OutlayType", GetString(outlaytype)),
              new XAttribute("NoOfPrivate", GetString("1")),
              new XAttribute("Unit", GetString("")),
              new XAttribute("SumOfPrivate", GetString(outlay.Amount)),
              new XAttribute("SumOfPrivateTax", GetString(outlay.VatAmount)),
              new XAttribute("InternNote", GetString(comment)),
              new XAttribute("ProjectCode", GetString(BusinessExportUtil.GetAccountNr(TermGroup_SieAccountDim.Project, outlay.AccountInternals))),
              new XAttribute("ResultUnitCode", GetString(BusinessExportUtil.GetAccountNr(TermGroup_SieAccountDim.CostCentre, outlay.AccountInternals))));
            return regOutlay;
        }

        #endregion

        #region Help methods

        private String GetString(String value)
        {
            return value;
        }

        private String GetString(int value)
        {
            return value.ToString();
        }

        private String GetString(decimal value)
        {
            return value.ToString();
        }

        private String GetString(DateTime date)
        {
            return date.Date.ToShortDateString();
        }

        #endregion
    }
}
