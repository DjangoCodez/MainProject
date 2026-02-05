using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL
{
    public class PolSalaryAdapter : IPreFlightSalaryAdapter
    {
        public SalaryAdapterResult GetSalaryFiles(SalaryAdapterInput salaryAdapterInput)
        {

            return new SalaryAdapterResult()
            {
                SalaryFiles = CreatePolSalaryExport(salaryAdapterInput)
            };
        }

        private List<SalaryAdapterFile> CreatePolSalaryExport(SalaryAdapterInput salaryAdapterInput)
        {
            List<SalaryAdapterFile> salaryAdapterFiles = new List<SalaryAdapterFile>();
            StringBuilder scheduleSB = new StringBuilder();
            foreach (var employee in salaryAdapterInput.Employees)
            {
                var polTransactions = new List<PolScheduleTransaction>();
                employee.SalarySchedules.ForEach(f => polTransactions.Add(new PolScheduleTransaction(salaryAdapterInput.SalaryCompany, employee, f)));
                var perWeek = PolScheduleTransaction.GetTransactionsForWeeks(polTransactions);

                foreach (var week in perWeek)
                {
                    var fistDate = week.Value.OrderBy(o => o.Begynnelsedatum).First().Begynnelsedatum;
                    var lastDate = week.Value.OrderBy(o => o.Begynnelsedatum).Last().Begynnelsedatum;
                    var lastDayOfWeek = CalendarUtility.GetLastDateOfWeek(lastDate);
                    if (lastDayOfWeek < salaryAdapterInput.PeriodToDate)
                        lastDate = lastDayOfWeek;
                    else
                        lastDate = salaryAdapterInput.PeriodToDate;

                    scheduleSB.AppendLine(week.Value.First().ToPolStringForOneWeek(fistDate, lastDate, week.Value));
                }
            }

            Dictionary<int, List<EmployeeChildDTO>> employeeChildren = new Dictionary<int, List<EmployeeChildDTO>>();
            StringBuilder absenceSB = new StringBuilder();
            foreach (var employee in salaryAdapterInput.Employees)
            {
                int? employeeChildId = null;
                foreach (var productCodeGroup in employee.SalaryTransactions.Where(w => w.IsAbsence).GroupBy(g => g.Code))
                {
                    var input = new SalaryMergeInput(productCodeGroup.Key, SalaryExportTransactionGroupType.ByCostAllocation, SalaryExportTransactionDateMergeType.Day, productCodeGroup.ToList(), useCostCenter: true);
                    var group = new SalaryExportTrancationGroup(input);
                    group.MergeSalaryExportTransaction();

                    foreach (var absence in group.MergedTransactions.OrderBy(w => w.Date))
                    {
                        absenceSB.Append(new PolAbsenceTransaction(salaryAdapterInput.SalaryCompany, employee, absence).ToPolString());
                        if (absence.EmployeeChildId.HasValue && !employeeChildId.HasValue)
                            employeeChildId = absence.EmployeeChildId.Value;
                    }
                    if (employeeChildId.HasValue && !employeeChildren.ContainsKey(employee.EmployeeId)) 
                    {
                        // employeeChildrenIds.Add(employee.EmployeeId, employeeChildId.Value);
                        employeeChildren.Add(employee.EmployeeId, salaryAdapterInput.EmployeeChildren.Where(c => c.EmployeeId == employee.EmployeeId).ToList());
                    }
                        
                }
            }

            StringBuilder transactionSB = new StringBuilder();
            foreach (var employee in salaryAdapterInput.Employees)
            {
                foreach (var productCodeGroup in employee.SalaryTransactions.Where(w => !w.IsAbsence).GroupBy(g => g.Code))
                {
                    var input = new SalaryMergeInput(productCodeGroup.Key, SalaryExportTransactionGroupType.ByCostAllocation, SalaryExportTransactionDateMergeType.Day, productCodeGroup.ToList(), useCostCenter: true, useProject: true);
                    var group = new SalaryExportTrancationGroup(input);
                    group.MergeSalaryExportTransaction();

                    foreach (var transaction in group.MergedTransactions.OrderBy(w => w.Date))
                    {
                        if (transaction.Hours > 0 || transaction.Amount > 0)
                            transactionSB.Append(new PolCompensationTransaction(salaryAdapterInput.SalaryCompany, employee, transaction).ToPolString());
                    }
                }
            }

            var combinedSB = new StringBuilder();
            combinedSB.Append(scheduleSB);
            combinedSB.Append(absenceSB);
            combinedSB.Append(transactionSB);
              salaryAdapterFiles.Add(new SalaryAdapterFile
            {
                FileName = salaryAdapterInput.GetFileNameInformation(false),
                File = Encoding.GetEncoding("ISO-8859-1").GetBytes(combinedSB.ToString())
            });

            #region EmployeeChildTransactions

            if (!employeeChildren.IsNullOrEmpty())
            {
                StringBuilder employeeSB = new StringBuilder();
                foreach (var employeeId in employeeChildren.Keys)
                {
                    var employee = salaryAdapterInput.Employees.Where(e => e.EmployeeId == employeeId).FirstOrDefault();
                    
                    if (employee != null)
                    {
                        foreach (var child in employeeChildren[employeeId])
                        {
                            employee.EmployeeChildren.Add(new SalaryExportEmployeeChild()
                            {
                                FirstName = child.FirstName,
                                LastName = child.LastName,
                                BirthDate = child.BirthDate,
                                EmployeeId = employeeId,
                            });
                        }
                        foreach (var child in employee.EmployeeChildren)
                            employeeSB.Append(new PolEmployeeChildTransaction(salaryAdapterInput.SalaryCompany, employee, salaryAdapterInput.PeriodFromDate, child).ToPolString());
                    }
                }

                salaryAdapterFiles.Add(new SalaryAdapterFile
                {
                    FileName = salaryAdapterInput.GetFileNameInformation(true),
                    File = Encoding.GetEncoding("ISO-8859-1").GetBytes(employeeSB.ToString())
                });
            }

            #endregion

            return salaryAdapterFiles;
        }
    }
}
