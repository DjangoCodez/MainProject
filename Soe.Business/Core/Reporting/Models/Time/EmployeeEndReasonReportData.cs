using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class EmployeeEndReasonsReportData : TimeReportDataManager, IReportDataModel
    {
        private readonly EmployeeEndReasonsReportDataInput _reportDataInput;
        private readonly EmployeeEndReasonsReportDataOutput _reportDataOutput;

        private bool LoadEndReason
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeEndReasonsMatrixColumns.EndReason);
            }
        }
        private bool LoadEmployments
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                       a.Column == TermGroup_EmployeeEndReasonsMatrixColumns.EmploymentDate ||
                       a.Column == TermGroup_EmployeeEndReasonsMatrixColumns.EndDate ||
                       a.Column == TermGroup_EmployeeEndReasonsMatrixColumns.EndReason ||
                       a.Column == TermGroup_EmployeeEndReasonsMatrixColumns.EmploymentTypeName);
            }
        }
        private bool LoadPositions
        {
            get
            {
                return _reportDataInput.Columns.Any(a =>
                    a.Column == TermGroup_EmployeeEndReasonsMatrixColumns.SSYKCode);
            }
        }
        private bool LoadAccountInternal
        {
            get
            {
                return EnumUtility.GetNames<TermGroup_EmployeeEndReasonsMatrixColumns>().Any(a => a.Contains("AccountInternal"));
            }
        }

        public EmployeeEndReasonsReportData(ParameterObject parameterObject, EmployeeEndReasonsReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeEndReasonsReportDataOutput(reportDataInput);
        }

        public static List<EmployeeEndReasonsReportDataField> GetPossibleDataFields()
        {
            List<EmployeeEndReasonsReportDataField> possibleFields = new List<EmployeeEndReasonsReportDataField>();
            EnumUtility.GetValues<TermGroup_EmployeeEndReasonsMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeEndReasonsReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeEndReasonsReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq
    
            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out _, out _, out bool? selectionActiveEmployees);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    var change = "";
                    var categoryName = "";
                    #region Permissions

                    bool employmentPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    bool userPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                   
                    #endregion

                    #region Company settings

                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);
                    var employeeAccounts = useAccountHierarchy ? EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo) : null;
                    
                    #endregion

                    #region Terms and dictionaries

                    int langId = GetLangId();
                    Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);

                    if (LoadEndReason)
                        _reportDataOutput.EndReason = GetTermGroupContent(TermGroup.EmploymentEndReason);

                    #endregion

                    #region Collections

                    List<EmploymentTypeDTO> employmentTypes = new List<EmploymentTypeDTO>();
                   
                    if (LoadEmployments && employmentPermission)
                    {
                        employmentTypes = EmployeeManager.GetEmploymentTypes(entities, reportResult.ActorCompanyId, (TermGroup_Languages)langId);
                    }

                    #endregion             

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: LoadEmployments && employmentPermission, loadUser: userPermission, loadEmploymentVacactionGroup: LoadEmployments && employmentPermission, loadEmploymentAccounts: LoadAccountInternal);

                    #region Content

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;
                        List<CompanyCategoryRecord> companyCategoryRecords = !useAccountHierarchy ? CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, reportResult.ActorCompanyId, false, selectionDateFrom, selectionDateTo) : null;

                        List<Employment> employments = employee.GetActiveEmploymentsDesc();
                        if (employments.IsNullOrEmpty())
                            continue;
                        
                        foreach (Employment employment in employments)
                        {
                            if (employment.GetEndReason() == 0)
                                continue;
                                                       
                            EmployeeEndReasonsItem employeeItem = new EmployeeEndReasonsItem();
                            var sb = new StringBuilder();

                            if (userPermission && !employee.UserReference.IsLoaded)
                                employee.UserReference.Load();
                            if (companyCategoryRecords != null)
                            {
                                foreach (string category in companyCategoryRecords.GetCategoryNames())
                                {
                                    if (sb.Length > 0)
                                        sb.Append(", ");
                                    sb.Append($"{category}");                                   
                                }
                            }
                            else if (employeeAccounts != null)
                            {
                                foreach (EmployeeAccount employeeAccount in employeeAccounts.Where(ea => ea.EmployeeId == employeeId))
                                {
                                    if (sb.Length > 0)
                                        sb.Append(", ");
                                    sb.Append($"{employeeAccount.Account.Name}");
                                }
                            }
                            categoryName = sb.ToString();
                         
                            change = "";
                            foreach (EmploymentChange employmentChange in employment.GetDataChanges(TermGroup_EmploymentChangeFieldType.EmploymentEndReason))
                            {
                                change = $"{employmentChange.EmploymentChangeBatch?.Comment}";
                            }

                            #endregion

                            #region Content

                            employeeItem.EmployeeNr = employee.EmployeeNr;
                            employeeItem.EmployeeName = employee.Name;
                            employeeItem.FirstName = employee.FirstName;
                            employeeItem.LastName = employee.LastName;
                            employeeItem.BirthYear = CalendarUtility.GetBirthYearFromSecurityNumber(employee.SocialSec);
                            employeeItem.Gender = GetValueFromDict((int)employee.Sex, sexDict);
                            employeeItem.CategoryName = categoryName;
                            if (userPermission)
                            {
                                EmployeePosition defaultEmployeePosition = LoadPositions ? EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, loadSysPosition: true)?.FirstOrDefault(f => f.Default) : null;
                                employeeItem.DefaultRole = defaultEmployeePosition?.Position?.Name ?? string.Empty;
                                employeeItem.SSYKCode = defaultEmployeePosition?.Position?.SysPositionCode ?? string.Empty;
                            }
                            if (LoadEmployments && employmentPermission)
                            {
                                employeeItem.EmploymentDate = employment.DateFrom.Value;
                                employeeItem.EndDate = employment.DateTo;
                                employeeItem.EmploymentTypeName = employment.GetEmploymentTypeName(employmentTypes, employment.DateFrom.Value);     
                                employeeItem.EndReason = employment.GetEndReason();
                                employeeItem.Comment = change;
                            }

                            _reportDataOutput.Employees.Add(employeeItem);
                        }

                        #endregion                   
                    }

                    #endregion
                }
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }
    }

    public class EmployeeEndReasonsReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeEndReasonsMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeEndReasonsReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeEndReasonsMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_EmployeeEndReasonsMatrixColumns.Unknown;
        }
    }

    public class EmployeeEndReasonsReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeEndReasonsReportDataField> Columns { get; set; }

        public EmployeeEndReasonsReportDataInput(CreateReportResult reportResult, List<EmployeeEndReasonsReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;

        }
    }

    public class EmployeeEndReasonsReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeEndReasonsItem> Employees { get; set; }
        public EmployeeEndReasonsReportDataInput Input { get; set; }
        public List<GenericType> EndReason { get; set; }

        public EmployeeEndReasonsReportDataOutput(EmployeeEndReasonsReportDataInput input)
        {
            this.Employees = new List<EmployeeEndReasonsItem>();
            this.Input = input;
            this.EndReason = new List<GenericType>();
        }
    }
}
