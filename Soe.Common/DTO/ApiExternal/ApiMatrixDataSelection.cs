using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{

    public class ApiMatrixReport
    {
        public ApiMatrixReport()
        {
            ApiMatrixReportSelectionInfos = new List<ApiMatrixReportSelectionInfo>();
            ApiMatrixColumns = new List<ApiMatrixColumns>();
        }
        public string GroupName { get; set; }
        public int SysReportTemplateTypeId { get; set; }
        public int? ReportNr { get; set; }
        public string Name { get; set; }
        public int? ReportId { get; set; }
        public string Description { get; set; }
        public List<ApiMatrixReportSelectionInfo> ApiMatrixReportSelectionInfos { get; set; }
        public List<ApiMatrixColumns> ApiMatrixColumns { get; set; }
    }

    public class ApiMatrixReportSelectionInfo
    {
        public ApiMatrixSelectionType ApiMatrixSelectionType { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
    }

    public class ApiMatrixColumns
    {
        public string Field { get; set; }
        public string Title { get; set; }
    }

    public enum ApiMatrixSelectionType
    {
        Unknown = 0,
        Employee = 1,
        DateRange = 2,
        Date = 3,
        IdList = 4,
        Id = 5,
        Bool = 6,
        PayrollProduct = 7,
        Text = 8
    }
    /// <summary>
    /// When ordering an output use information from ApiMatrixReport to make a selection.
    /// ReportId will identity which report that will be used as the bases for the report
    /// </summary>
    public class ApiMatrixDataSelection
    {
        public ApiMatrixDataSelection()
        {
            ApiMatrixDataSelectionDateRanges = new List<ApiMatrixDataSelectionDateRange>();
            ApiMatrixDataSelectionIdLists = new List<ApiMatrixDataSelectionIdList>();
            ApiMatrixDataSelectionBools = new List<ApiMatrixDataSelectionBool>();
            ApiMatrixDataSelectionIds = new List<ApiMatrixDataSelectionId>();
            ApiMatrixDataSelectionDates = new List<ApiMatrixDataSelectionDate>();
            ApiMatrixPayrollProductRowSelections = new List<ApiMatrixPayrollProductRowSelection>();
            ApiMatrixDataSelectionTexts = new List<ApiMatrixDataSelectionText>();
        }

        //When using ReportId selections are mandatory
        public int? ReportId { get; set; }
        //When using ReportUserSelectionId selection will override all posted selections
        public int? ReportUserSelectionId { get; set; }

        /// <summary>
        /// DateRanges is for outputs spanning a range of time
        /// </summary>
        public List<ApiMatrixDataSelectionDateRange> ApiMatrixDataSelectionDateRanges { get; set; }
        /// <summary>
        /// When an list of Ids is needed
        /// </summary>
        public List<ApiMatrixDataSelectionIdList> ApiMatrixDataSelectionIdLists { get; set; }
        /// <summary>
        /// When a single Id is needed
        /// </summary>
        public List<ApiMatrixDataSelectionId> ApiMatrixDataSelectionIds { get; set; }
        /// <summary>
        /// When a single date is needed
        /// </summary>
        public List<ApiMatrixDataSelectionDate> ApiMatrixDataSelectionDates { get; set; }
        /// <summary>
        /// When a boolean is needed
        /// </summary>
        public List<ApiMatrixDataSelectionBool> ApiMatrixDataSelectionBools { get; set; }
        /// <summary>
        /// When a string is needed
        /// </summary>
        public List<ApiMatrixDataSelectionText> ApiMatrixDataSelectionTexts { get; set; }
        /// <summary>
        /// Selection to get the correct number of employees back
        /// </summary>
        public ApiMatrixEmployeeSelection ApiMatrixEmployeeSelections { get; set; }
        /// <summary>
        /// Selection of Paypollproducts, based on Level1, Level2, Level3, Level4. One row for each combination. OR is used between rows
        /// </summary>
        public List<ApiMatrixPayrollProductRowSelection> ApiMatrixPayrollProductRowSelections { get; set; }
        /// <summary>
        /// The Selection of columns/fields. Think of i as an Excel spreadsheet. 
        /// </summary>
        public ApiMatrixColumnsSelection ApiMatrixColumnsSelection { get; set; }
    }

    public class ApiMatrixDataSelectionDateRange
    {
        public string TypeName { get; set; }
        public string Key { get; set; }
        public DateTime SelectFrom { get; set; }
        public DateTime SelectTo { get; set; }
    }

    public class ApiMatrixDataSelectionDate
    {
        /// <summary>
        /// Used for trackability
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// String used to match the type of selection input. Find available ones in ApiMatrixReportSelectionInfo
        /// </summary>
        public string Key { get; set; }
        public DateTime Date { get; set; }
    }

    public class ApiMatrixDataSelectionIdList
    {
        /// <summary>
        /// Used for trackability
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// String used to match the type of selection input. Find available ones in ApiMatrixReportSelectionInfo
        /// </summary>
        public string Key { get; set; }
        public List<int> Ids { get; set; }
    }

    public class ApiMatrixDataSelectionId
    {
        /// <summary>
        /// Used for trackability
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// String used to match the type of selection input. Find available ones in ApiMatrixReportSelectionInfo
        /// </summary>
        public string Key { get; set; }
        public int Id { get; set; }
    }

    public class ApiMatrixDataSelectionText
    {
        /// <summary>
        /// Used for trackability
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// String used to match the type of selection input. Find available ones in ApiMatrixReportSelectionInfo
        /// </summary>
        public string Key { get; set; }
        public string StringValue{ get; set; }
    }

    public class ApiMatrixDataSelectionBool
    {
        /// <summary>
        /// Used for trackability
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// String used to match the type of selection input. Find available ones in ApiMatrixReportSelectionInfo
        /// </summary>
        public string Key { get; set; }
        public bool BooleanValue { get; set; }
    }

    public class ApiMatrixEmployeeSelection
    {
        /// <summary>
        /// Used for trackability
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// If list (and EmployeeNumbers) is null or empty all employees available to user is fetched
        /// </summary>
        public List<int> EmployeeIds { get; set; }
        /// <summary>
        /// If list (and employeeIds) is null or empty all employees available to user is fetched
        /// </summary>
        public List<string> EmployeeNumbers { get; set; }
        /// <summary>
        /// set to true is you want to include employees with ended employments.
        /// </summary>
        public bool IncludeEnded { get; set; }
        /// <summary>
        /// set to true if you do not want to include "ledigt pass", "free shifts"
        /// </summary>
        public bool IncludeHidden { get; set; }
        /// <summary>
        /// set to true if you do not want to include vacant employees
        /// </summary>
        public bool IncludeVacant { get; set; }
    }

    public class ApiMatrixColumnsSelection
    {
        /// <summary>
        /// Used for trackability
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// String used to match the type of selection input. Find available ones in ApiMatrixReportSelectionInfo
        /// </summary>
        public string Key { get; set; }
        public List<ApiMatrixColumnSelection> ApiMatrixColumnSelections { get; set; }
    }
    /// <summary>
    /// Selection based on Levels on PayrollProducts. One row for each combination
    /// Exemple: SysPayrollTypeLevel1: 1000000 (GrossSalary)  SyParollTypeLevel2: 106000 Absence SysPayrollTypeLevel3: 1061000 (Sick)
    /// </summary>
    public class ApiMatrixPayrollProductRowSelection
    {
        public TermGroup_SysPayrollType SysPayrollTypeLevel1 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel2 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel3 { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel4 { get; set; }
    }

    public class ApiMatrixColumnSelection
    {
        public string Field { get; set; }
        public int ItemId { get; set; }
    }
}
