using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class Kronofogden : ExportFilesBase
    {
        #region Ctor

        public Kronofogden(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        public List<KronofogdenEmployee> GetKronofogdenEmployees(CompEntities entities, out Company company, out bool setAsFinal)
        {

            company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            TryGetBoolFromSelection(reportResult, out setAsFinal, "setAsFinal");

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            List<int> employeeIds = employees.Select(s => s.EmployeeId).ToList();
            var employeeTaxs = entities.EmployeeTaxSE.Where(w => employeeIds.Contains(w.EmployeeId) && w.State == (int)SoeEntityState.Active && !string.IsNullOrEmpty(w.SalaryDistressCase)).ToList();
            employees = employees.Where(w => employeeTaxs.Select(s => s.EmployeeId).Contains(w.EmployeeId)).ToList();
            List<TimePayrollStatisticsSmallDTO> transactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, Company.ActorCompanyId, employees, selectionTimePeriodIds, setPensionCompany: true, ignoreAccounting: true);

            var distressTransactions = transactions.Where(w => PayrollRulesUtil.IsSalaryDistress(w.SysPayrollTypeLevel1, w.SysPayrollTypeLevel2, w.SysPayrollTypeLevel3, w.SysPayrollTypeLevel4)).ToList();
            var absenceTransactions = transactions.Where(w => PayrollRulesUtil.IsAbsence(w.SysPayrollTypeLevel1, w.SysPayrollTypeLevel2, w.SysPayrollTypeLevel3, w.SysPayrollTypeLevel4)).ToList();
            return GetKronofogdenEmployees(distressTransactions, absenceTransactions, employees, employeeTaxs);
        }

        public string CreateKronofogdenFile(CompEntities entities)
        {

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            List<KronofogdenEmployee> kronofogdenEmployees = GetKronofogdenEmployees(entities, out Company company, out bool setAsFinal);

            #endregion

            #region Create File

            StringBuilder sb = new StringBuilder();

            foreach (var kronofogdenEmployee in kronofogdenEmployees)
            {
                sb.Append(kronofogdenEmployee.OutPutString);
                sb.Append(Environment.NewLine);
            }

            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("Kronofogden" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".csv";

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                if (setAsFinal)
                    GeneralManager.SaveEventHistories(entities, kronofogdenEmployees.Select(s => s.EventHistory(company.ActorCompanyId, base.UserId)).ToList(), company.ActorCompanyId);
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        public List<KronofogdenEmployee> GetKronofogdenEmployees(List<TimePayrollStatisticsSmallDTO> distressTransactions, List<TimePayrollStatisticsSmallDTO> absenceTransactions, List<Employee> employees, List<EmployeeTaxSE> employeeTaxSEs)
        {
            List<KronofogdenEmployee> kronofogdenEmployees = new List<KronofogdenEmployee>();

            foreach (var employee in employees)
            {
                var tax = employeeTaxSEs.FirstOrDefault(f => f.EmployeeId == employee.EmployeeId);

                var distressTransactionsOnEmployee = distressTransactions.Where(w => w.EmployeeId == employee.EmployeeId).ToList();
                var absenceTransactionsOnEmployee = absenceTransactions.Where(w => w.EmployeeId == employee.EmployeeId).ToList();

                if (distressTransactionsOnEmployee.Any() || tax.SalaryDistressAmountType == (int)TermGroup_EmployeeTaxSalaryDistressAmountType.AllSalary)
                {

                    KronofogdenEmployee kronofogdenEmployee = new KronofogdenEmployee()
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeNr = employee.EmployeeNr,
                        Personnummer = employee.SocialSec,
                        Beslutsnummer = tax?.SalaryDistressCase.ToString() ?? string.Empty,
                        Namn = employee.Name,
                    };

                    StringBuilder avvikelse = new StringBuilder("Ingen avvikelse");

                    bool avvikelsen = (tax.SalaryDistressAmountType == (int)TermGroup_EmployeeTaxSalaryDistressAmountType.FixedAmount &&
                        Convert.ToInt32(tax.SalaryDistressAmount) != Convert.ToInt32(Math.Abs(distressTransactionsOnEmployee.Any() ? distressTransactionsOnEmployee.Sum(s => s.Amount) : 0))) ||
                        (tax.SalaryDistressAmountType == (int)TermGroup_EmployeeTaxSalaryDistressAmountType.AllSalary &&
                        Convert.ToInt32(Math.Abs(distressTransactionsOnEmployee.Any() ? distressTransactionsOnEmployee.Sum(s => s.Amount) : 0)) == 0);

                    if (absenceTransactionsOnEmployee.Any() && avvikelsen)
                    {
                        avvikelse.Clear();

                        if (absenceTransactions.Any(a => PayrollRulesUtil.IsAbsenceSick(a.SysPayrollTypeLevel1, a.SysPayrollTypeLevel2, a.SysPayrollTypeLevel3, a.SysPayrollTypeLevel4)))
                        {
                            avvikelse.Append("Sjuk");
                        }

                        if (absenceTransactions.Any(a => PayrollRulesUtil.IsLeaveOfAbsence(a.SysPayrollTypeLevel1, a.SysPayrollTypeLevel2, a.SysPayrollTypeLevel3, a.SysPayrollTypeLevel4)))
                        {
                            avvikelse.Append(avvikelse.Length > 0 ? ", " : "");
                            avvikelse.Append("Tjänsteledig");
                        }

                        if (absenceTransactions.Any(a => PayrollRulesUtil.IsParentalLeave(a.SysPayrollTypeLevel1, a.SysPayrollTypeLevel2, a.SysPayrollTypeLevel3, a.SysPayrollTypeLevel4)))
                        {
                            avvikelse.Append(avvikelse.Length > 0 ? ", " : "");
                            avvikelse.Append("Föräldraledig");
                        }

                        if (avvikelse.Length == 0)
                        {
                            avvikelse.Append("Annan ledighet");
                        }
                    }

                    kronofogdenEmployee.Belopp = distressTransactionsOnEmployee.Any() ? Convert.ToInt32(Math.Abs(distressTransactionsOnEmployee.Sum(s => s.Amount))) : 0;
                    kronofogdenEmployee.Avvikelse = avvikelse.ToString();
                    kronofogdenEmployee.UtbetalningsDatum = distressTransactionsOnEmployee.FirstOrDefault(w => w.PaymentDate.HasValue)?.PaymentDate.Value.ToString("yyyy-MM-dd") ?? string.Empty;
                    kronofogdenEmployees.Add(kronofogdenEmployee);
                }
            }

            return kronofogdenEmployees;
        }

    }

    public class KronofogdenEmployee
    {
        public int EmployeeId { get; set; }
        public string Personnummer { get; set; }
        public string EmployeeNr { get; set; }
        public string Namn { get; set; }
        public int Belopp { get; set; }
        public string Beslutsnummer { get; set; }
        public string Avvikelse { get; set; }
        public string Kommentar { get; set; }
        public string UtbetalningsDatum { get; set; } 
        public string OutPutString
        {
        get
            {
                return $"{Personnummer};{Namn};{UtbetalningsDatum}{Belopp};{Beslutsnummer};{Avvikelse};{Kommentar}";
            }
        }

        public XElement OutPutXElement(int xmlId)
        {
            return new XElement("KronofogdenEmployee",
                    new XAttribute("Id", xmlId),
                    new XElement("EmployeeId", this.EmployeeId),
                    new XElement("EmployeeNr", this.EmployeeNr),
                    new XElement("Namn", this.Namn),
                    new XElement("Beslutsnummer", this.Beslutsnummer),
                    new XElement("Avvikelse", this.Avvikelse),
                    new XElement("Kommentar", this.Kommentar),
                   new XElement("Belopp", this.Belopp),
                   new XElement("Personnummer", this.Personnummer),
                   new XElement("UtbetalningsDatum", this.UtbetalningsDatum));
        }

        public EventHistoryDTO EventHistory(int actorCompanyId, int userId)
        {
            return new EventHistoryDTO(actorCompanyId, TermGroup_EventHistoryType.Kronofogden, SoeEntityType.Employee, EmployeeId, userId: userId, decimalValue: Belopp, dateValue: DateTime.Now);
        }
    }
}