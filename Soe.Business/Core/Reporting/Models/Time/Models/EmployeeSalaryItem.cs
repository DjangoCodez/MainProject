using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeSalaryItem
    {
        public EmployeeSalaryItem()
        {
            ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string EmploymentType { get; set; }
        public string Position { get; set; }
        public string SalaryType { get; set; }
        public string SalaryTypeName { get; set; }
        public string SalaryTypeCode { get; set; }
        public string SalaryTypeDesc { get; set; }
        public DateTime? SalaryDateFrom { get; set; }
        public decimal SalaryAmount { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public string CategoryName { get; set; }
        public bool AccordingToPayrollGroup { get; set; }
        public bool IsSecondaryEmployment { get; set; }
        public string PayrollLevel { get; set; }
        public int ExperienceTot { get; set; }
        public string BirthYearMonth { get; set; }
        public int Age { get; set; }
        public decimal SalaryFromPayrollGroup { get; set; }
        public decimal SalaryDiff { get; set; }
        public int EmployeeId { get; set; }
        public List<ExtraFieldAnalysisField> ExtraFieldAnalysisFields { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
    }
    public static class EmployeeSalaryItemExtensions
    {
        public static string GroupOn(this EmployeeSalaryItem employeeSalaryItem, List<EmployeeSalaryReportDataField> employeeSalaryReportDataField, /*List<TermGroup_EmployeeSalaryMatrixColumns> columns,*/ bool mergeOnAccount)
        {
            var columns = employeeSalaryReportDataField.Select(s => s).ToList();
            string value = string.Empty;

            foreach (var column in columns)
            {
                switch (column.Column)
                {
                    case TermGroup_EmployeeSalaryMatrixColumns.EmployeeNr:
                        value += $"#{employeeSalaryItem.EmployeeId}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.EmployeeName:
                        value += $"#{employeeSalaryItem.EmployeeId}{employeeSalaryItem.FirstName}{employeeSalaryItem.LastName}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.FirstName:
                        value += $"#{employeeSalaryItem.FirstName}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.LastName:
                        value += $"#{employeeSalaryItem.LastName}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.Gender:
                        value += $"#{employeeSalaryItem.Gender}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.EmploymentTypeName:
                        value += $"#{employeeSalaryItem.EmploymentType}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.Position:
                        value += $"#{employeeSalaryItem.Position}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryType:
                        value += $"#{employeeSalaryItem.SalaryType}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeName:
                        value += $"#{employeeSalaryItem.SalaryTypeName}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeCode:
                        value += $"#{employeeSalaryItem.SalaryTypeCode}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryTypeDesc:
                        value += $"#{employeeSalaryItem.SalaryTypeDesc}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryDateFrom:
                        value += $"#{employeeSalaryItem.SalaryDateFrom}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryAmount:
                        value += $"#{employeeSalaryItem.SalaryAmount}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.PayrollLevel:
                        value += $"#{employeeSalaryItem.PayrollLevel}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.ExperienceTot:
                        value += $"#{employeeSalaryItem.ExperienceTot}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.BirthYearMonth:
                        value += $"#{employeeSalaryItem.BirthYearMonth}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.Age:
                        value += $"#{employeeSalaryItem.Age}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryFromPayrollGroup:
                        value += $"#{employeeSalaryItem.SalaryFromPayrollGroup}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.SalaryDiff:
                        value += $"#{employeeSalaryItem.SalaryDiff}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.CategoryName:
                        value += $"#{employeeSalaryItem.CategoryName}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.AccordingToPayrollGroup:
                        value += $"#{employeeSalaryItem.AccordingToPayrollGroup}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.IsSecondaryEmployment:
                        value += $"#{employeeSalaryItem.IsSecondaryEmployment}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.Created:
                        value += $"#{employeeSalaryItem.Created}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.CreatedBy:
                        value += $"#{employeeSalaryItem.CreatedBy}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.Modified:
                        value += $"#{employeeSalaryItem.Modified}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.ModifiedBy:
                        value += $"#{employeeSalaryItem.ModifiedBy}";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.AccountInternalNrs:
                    case TermGroup_EmployeeSalaryMatrixColumns.AccountInternalNames:
                        var accountString = employeeSalaryItem.AccountAnalysisFields.Where(s => s.AccountDimId == int.Parse(column.Selection.Options.Key)).Select(s => s.AccountId).JoinToString(",");
                        value += accountString != "" ? $"#{accountString}" : $"#";
                        break;
                    case TermGroup_EmployeeSalaryMatrixColumns.ExtraFieldEmployee:

                        foreach (var extraFiled in employeeSalaryItem.ExtraFieldAnalysisFields)
                        {
                            var extraFieldAnalysisFields = employeeSalaryItem.ExtraFieldAnalysisFields;
                            var efr = extraFieldAnalysisFields[extraFieldAnalysisFields.IndexOf(extraFiled)].ExtraFieldRecord != null ?
                                extraFieldAnalysisFields[extraFieldAnalysisFields.IndexOf(extraFiled)].ExtraFieldRecord.ExtraFieldRecordId.ToString() : "";
                            value += $"#{efr}";
                        }
                        break;
                    default:
                        break;
                }
            }

            return value;
        }
        
    }
}
