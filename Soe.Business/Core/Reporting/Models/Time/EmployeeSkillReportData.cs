using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class  EmployeeSkillReportData : EconomyReportDataManager, IReportDataModel 
    {
        private readonly EmployeeSkillReportDataOutput _reportDataOutput;

        public EmployeeSkillReportData(ParameterObject parameterObject, EmployeeSkillReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new EmployeeSkillReportDataOutput(reportDataInput);
        }

        public static List<EmployeeSkillReportDataField> GetPossibleDataFields()
        {
            List<EmployeeSkillReportDataField> possibleFields = new List<EmployeeSkillReportDataField>();
            EnumUtility.GetValues<TermGroup_TimeStampMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new EmployeeSkillReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public EmployeeSkillReportDataOutput CreateOutput(CreateReportResult reportResult)
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
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List <Employee> employees, out List<int> selectionEmployeeIds))
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
                    #region Prereq

                    #region Permissions

                    bool userPermission = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_User, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
                    
                    #endregion

                    #region Company settings

                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);
                    #endregion

                    #region Terms and dictionaries

                    int langId = GetLangId();
                    Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);
                    
                    #endregion

                    #region Collections

                    List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(entities, reportResult.ActorCompanyId, (TermGroup_Languages)langId);

                    #endregion

                    #endregion

                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadUser: userPermission);

                    #region Content

                    var employeeAccounts = useAccountHierarchy ? EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo) : null;

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        #region Prereq

                        Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                        if (employee == null)
                            continue;

                        List<Employment> employments = employee.GetEmploymentsDesc(selectionDateFrom, selectionDateTo);
                        if (employments.IsNullOrEmpty())
                            continue;

                        List<string> categoryName = new List<string>();
                        List<string> empTypeName = new List<string>();
                        List<string> ssykCodes = new List<string>();
                        List<string> positionNames = new List<string>();

                        foreach (var employmentsList in employments)
                        {
                            var tempName = employmentsList.GetEmploymentTypeName(employmentTypes, employmentsList.DateFrom);
                            if (!tempName.IsNullOrEmpty() && (empTypeName.IsNullOrEmpty() || !empTypeName.Contains(tempName)))
                                empTypeName.Add(tempName);
                        }

                        List<CompanyCategoryRecord> companyCategoryRecords = !useAccountHierarchy ? CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Employee, SoeCategoryRecordEntity.Employee, employee.EmployeeId, reportResult.ActorCompanyId, false, selectionDateFrom, selectionDateTo) : null;

                        #region Permissions

                        if (userPermission && !employee.UserReference.IsLoaded)
                            employee.UserReference.Load();

                        #endregion

                        if (companyCategoryRecords != null)
                        {
                            foreach (var category in companyCategoryRecords.GetCategoryNames())
                            {
                                if (categoryName.IsNullOrEmpty() || !categoryName.Contains(category))
                                    categoryName.Add(category);

                            }
                        }
                        else if (employeeAccounts != null)
                        {
                            foreach (var category in employeeAccounts)
                            {
                                if (category.EmployeeId == employeeId && !category.Account.Name.IsNullOrEmpty() && (categoryName.IsNullOrEmpty() || !categoryName.Contains(category.Account.Name)))
                                    categoryName.Add(category.Account.Name);

                            }
                        }

                        List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, loadSysPosition: true);

                        foreach (var employeePositionsList in employeePositions)
                        {

                            if (employeePositionsList.Position != null && employeePositionsList.Position.Name != null && (positionNames.IsNullOrEmpty() || !positionNames.Contains(employeePositionsList.Position.Name)))
                                positionNames.Add(employeePositionsList.Position.Name);

                            if (employeePositionsList.Position != null && employeePositionsList.Position.SysPositionCode != null && (ssykCodes.IsNullOrEmpty() || !ssykCodes.Contains(employeePositionsList.Position.SysPositionCode)))
                                ssykCodes.Add(employeePositionsList.Position.SysPositionCode);

                        }
                        
                        List<EmployeeSkill> skills = TimeScheduleManager.GetEmployeeSkills(entities, employee.EmployeeId);
                        foreach (EmployeeSkill skill in skills)
                        {
                            EmployeeSkillItem employeeSkillItem = new EmployeeSkillItem
                            {
                                EmployeeNr = employee.EmployeeNr,
                                CategoryName = categoryName.JoinToString(", "),
                                EmployeeName = employee.Name,
                                FirstName = employee.FirstName,
                                LastName = employee.LastName,
                                Gender = GetValueFromDict((int)employee.Sex, sexDict),
                                BirthYear = CalendarUtility.GetBirthYearFromSecurityNumber(employee.SocialSec),
                                EmploymentTypeName = empTypeName.JoinToString(", "),
                                PositionName = positionNames.JoinToString(", "),
                                SSYKCode = ssykCodes.JoinToString(", "),
                                SkillName = skill.Skill?.Name ?? string.Empty,
                                SkillDate = skill.DateTo != null ? skill.DateTo : null,
                                SkillLevel = skill.SkillLevel,
                                SkillDescription = skill.Skill?.Description ?? string.Empty,
                                SkillTypeName = skill.Skill?.SkillType?.Name ?? string.Empty,
                                SkillTypeId = skill.Skill.SkillType.SkillTypeId,
                                SkillTypeDescription = skill.Skill?.SkillType?.Description ?? string.Empty
                            };

                            _reportDataOutput.EmployeeSkillItems.Add(employeeSkillItem);
                        }
                        if (skills.Count == 0)
                        {

                            EmployeeSkillItem employeeSkillItem = new EmployeeSkillItem
                            {
                                EmployeeNr = employee.EmployeeNr,
                                EmployeeName = employee.Name,
                                CategoryName = categoryName.JoinToString(", "),
                                FirstName = employee.FirstName,
                                LastName = employee.LastName,
                                SkillName = "",
                                SkillDate = null,
                                SkillLevel = null,
                                SkillDescription = "",
                                SkillTypeName = "",
                                SkillTypeDescription = "",
                                EmploymentTypeName = empTypeName.JoinToString(", "),
                                PositionName = positionNames.JoinToString(", "),
                                SSYKCode = ssykCodes.JoinToString(", "),
                                Gender = GetValueFromDict((int)employee.Sex, sexDict),
                                BirthYear = CalendarUtility.GetBirthYearFromSecurityNumber(employee.SocialSec)
                            };
                            _reportDataOutput.EmployeeSkillItems.Add(employeeSkillItem);
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

    public class EmployeeSkillReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_TimeStampMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeSkillReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            Selection = columnSelectionDTO;
            ColumnKey = Selection?.Field;
            Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_TimeStampMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_TimeStampMatrixColumns.Unknown;
        }
    }

    public class EmployeeSkillReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeSkillReportDataField> Columns { get; set; }

        public EmployeeSkillReportDataInput(CreateReportResult reportResult, List<EmployeeSkillReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeSkillReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public EmployeeSkillReportDataInput Input { get; set; }
        public List<EmployeeSkillItem> EmployeeSkillItems { get; set; }

        public EmployeeSkillReportDataOutput(EmployeeSkillReportDataInput input)
        {
            this.Input = input;
            this.EmployeeSkillItems = new List<EmployeeSkillItem>();
        }
    }

}

