using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight
{
    public interface IPreFlightSalaryAdapter
    {
        SalaryAdapterResult GetSalaryFiles(SalaryAdapterInput salaryAdapterInput);
    }

    public interface ISalaryAdapterResult
    {
        List<SalaryAdapterFile> SalaryFiles { get; }
    }

    public interface ISalaryAdapterFile
    {
        byte[] File { get; set; }
        string FileName { get; set; }
        string FileType { get; set; }
    }

    public interface ISalaryAdapterInput
    {
        bool IsTest { get; set; }
        SalaryExportCompany SalaryCompany { get; set; }
        List<SalaryExportEmployee> Employees { get; set; }
    }

    public class SalaryAdapterFile : ISalaryAdapterFile
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
    }

    public class SalaryAdapterResult : ISalaryAdapterResult
    {
        public List<SalaryAdapterFile> SalaryFiles { get; set; }
    }

    public class SalaryAdapterInput : ISalaryAdapterInput
    {
        public SalaryAdapterInput(string companyCode, DateTime periodFromDate, DateTime periodToDate, List<TransactionItem> payrollTransactions, List<ScheduleItem> scheduleItems, List<Employee> employees, List<EmployeeChildDTO> employeeChildren, List<TimeDeviationCause> timeDeviationCauses)
        {
            this.IsTest = ConfigurationSetupUtil.GetSiteType()  == TermGroup_SysPageStatusSiteType.Test;
            this.PeriodFromDate = periodFromDate;
            this.PeriodToDate = periodToDate;   
            this.EmployeeChildren = employeeChildren ?? new List<EmployeeChildDTO>();

            this.SalaryCompany = new SalaryExportCompany()
            {
                CompanyCode = companyCode
            };

            foreach (var employee in employees)
            {
                var salaryEmployee = new SalaryExportEmployee
                {
                    EmployeeExternalCode = employee.ExternalCode,
                    EmployeeNr = employee.EmployeeNr.ToString(),
                    EmployeeId = employee.EmployeeId,
                };
                var payrollTransactionsForEmployee = payrollTransactions.Where(x => x.EmployeeId == employee.EmployeeId.ToString()).ToList();
                var scheduleItemsForEmployee = scheduleItems.Where(x => x.EmployeeId == employee.EmployeeId.ToString()).ToList();

                foreach (var payrollTransaction in payrollTransactionsForEmployee)
                {
                    string externalCode = string.Empty;
                    if (timeDeviationCauses != null && timeDeviationCauses.Any())
                        externalCode = timeDeviationCauses.FirstOrDefault(x => x.TimeDeviationCauseId == payrollTransaction.TimeDeviationCauseId)?.ExtCode ?? string.Empty;
                    if (externalCode == null)
                        externalCode = string.Empty;

                    SalaryExportCostAllocation costAllocation = new SalaryExportCostAllocation
                    {
                        Costcenter = GetAccountNr(TermGroup_SieAccountDim.CostCentre, payrollTransaction.AccountInternals),
                        Project = GetAccountNr(TermGroup_SieAccountDim.Project, payrollTransaction.AccountInternals),
                        Department = GetAccountNr(TermGroup_SieAccountDim.Department, payrollTransaction.AccountInternals),
                        //EmployeeCostCenter = employee.CostCenter,
                        //EmployeeProject = employee.Project,
                        //EmployeeDepartment = employee.Department
                    };

                    salaryEmployee.SalaryTransactions.Add(new SalaryExportTransaction
                    {
                        Code = payrollTransaction.ProductCode,
                        Date = payrollTransaction.Date,
                        Hours = payrollTransaction.IsRegistrationQuantity ? payrollTransaction.Quantity : Decimal.Divide(Convert.ToDecimal(payrollTransaction.Time), 60),
                        Amount = payrollTransaction.Amount,
                        Quantity = payrollTransaction.Quantity,
                        IsAbsence = payrollTransaction.IsAbsence,
                        CostAllocation = costAllocation,
                        EmployeeChildId = payrollTransaction.EmployeeChildId,
                        OfFullDayAbsence = payrollTransaction.IsAbsence && (scheduleItemsForEmployee.Where(s => s.Date == payrollTransaction.Date).Sum(s => s.TotalMinutes - s.TotalBreakMinutes) == payrollTransactionsForEmployee.Where(t => t.IsAbsence && t.Date == payrollTransaction.Date && t.ProductCode == payrollTransaction.ProductCode).Sum(t => t.Time)),
                        IsRegistrationQuantity = payrollTransaction.IsRegistrationQuantity,
                        IsPaidVacation = payrollTransaction.SysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Paid,
                        ExternalCode = externalCode,
                    });
                }

                foreach (var scheduleItem in scheduleItemsForEmployee)
                {
                    salaryEmployee.SalarySchedules.Add(new SalaryExportSchedule
                    {
                        Date = scheduleItem.Date,
                        ScheduleHours = decimal.Divide(Convert.ToDecimal(scheduleItem.TotalMinutes - scheduleItem.TotalBreakMinutes), 60),
                    });
                }

                if (this.Employees == null)
                    this.Employees = new List<SalaryExportEmployee>();

                this.Employees.Add(salaryEmployee);
            }
        }

        public string GetFileNameInformation(bool isChildFile)
        {
            //          T_SoftOne_POL_Tid_Export_xx_yyyy-mm-ddtttttt.txt
            var type = isChildFile ? "EmployeeChild" : "Tid_Export";
            var prefix = !IsTest ? $"P_SoftOne_POL_{type}" : $"T_SoftOne_POL_{type}";
            var companyCode = SalaryCompany.CompanyCode.Length < 2 ? SalaryCompany.CompanyCode.PadLeft(2, '0') : SalaryCompany.CompanyCode.Substring(SalaryCompany.CompanyCode.Length - 2, 2);
            var now = DateTime.Now;
            var twoDigitMonth = now.Month.ToString().PadLeft(2, '0');
            var twoDigitDay = now.Day.ToString().PadLeft(2, '0');
            var twoDigitHour = now.Hour.ToString().PadLeft(2, '0');
            var twoDigitMinute = now.Minute.ToString().PadLeft(2, '0');
            var twoDigitSecond = now.Second.ToString().PadLeft(2, '0');
            
            return $"{prefix}_{companyCode}_{now.Year}-{twoDigitMonth}-{twoDigitDay}{twoDigitHour}{twoDigitMinute}{twoDigitSecond}.txt";
        }

        public DateTime PeriodFromDate { get; set; }
        public DateTime PeriodToDate { get; set; }

        private string GetAccountNr(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
        {
            if (!internalAccounts.IsNullOrEmpty())
                foreach (AccountInternal internalAccount in internalAccounts)
                    if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue && internalAccount.Account.AccountDim.SysSieDimNr.Value == (int)accountDim)
                        return internalAccount.Account.AccountNr;

            return "";
        }

        public bool IsTest { get; set; }
        public SalaryExportCompany SalaryCompany { get; set; }
        public List<SalaryExportEmployee> Employees { get; set; }
        public List<EmployeeChildDTO> EmployeeChildren { get; set; }
    }
}
