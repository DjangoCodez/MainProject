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
    public class EmployeeMeetingReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly EmployeeMeetingReportDataOutput _reportDataOutput;
        private readonly EmployeeMeetingReportDataInput _reportDataInput;

        private bool loadAccountInternal => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("accountInternal"));
        private bool loadPositions => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeMeetingMatrixColumns.Position || a.Column == TermGroup_EmployeeMeetingMatrixColumns.SSYKCode);
        private bool loadCategories => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeMeetingMatrixColumns.CategoryName);
        private bool loadExtraFields => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("extraField"));

        public EmployeeMeetingReportData(ParameterObject parameterObject, EmployeeMeetingReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeMeetingReportDataOutput(reportDataInput);
        }

        public EmployeeMeetingReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out List<Employee> employees, out List<int> selectionEmployeeIds, out _, out TermGroup_EmployeeSelectionAccountingType selectionAccountingType))
                return new ActionResult(false);

            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out _, out bool? selectionActiveEmployees);

            List<int> selectionCategoryIds = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees")?.CategoryIds;
            string employmentTypeNames;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            try
            {

                using (CompEntities entities = new CompEntities())
                {
                    int langId = GetLangId();
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, reportResult.ActorCompanyId);

                    Dictionary<int, string> sexDict = base.GetTermGroupDict(TermGroup.Sex, langId);
                    List<AccountDTO> companyAccountsDTO = new List<AccountDTO>();
                    List<Account> companyAccounts;
                    List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                    List<CompanyCategoryRecord> companyCategoryRecords = new List<CompanyCategoryRecord>();
                    List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();

                    if (loadExtraFields)
                    {
                        extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(entities, selectionEmployeeIds, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, true, true).ToDTOs();
                    }

                    if (useAccountHierarchy)
                    {
                        companyAccounts = AccountManager.GetAccountsByCompany(base.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true);
                        employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                        companyAccountsDTO = companyAccounts.ToDTOs();
                    }
                    else
                    {
                        companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, base.ActorCompanyId);
                    }

                    #region ------ Load employees ------
                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees, loadEmployment: true, loadEmploymentPriceType: true);

                    if (selectionIncludeInactive)
                    {
                        List<Employee> employeesInactive = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, active: false, orderByName: false, loadEmployment: false, loadUser: true, loadEmploymentAccounts: true);
                        if (!employeesInactive.IsNullOrEmpty())
                        {
                            employees = employees.Concat(employeesInactive).ToList();
                        }
                    }
                    #endregion ------ Load employees ------

                    Dictionary<int, string> yesNoDictionary = base.GetTermGroupDict(TermGroup.YesNo, base.GetLangId());

                    foreach (Employee employee in employees)
                    {
                        List<ExtraFieldAnalysisField> extraField = new List<ExtraFieldAnalysisField>();
                        if (loadExtraFields && extraFieldRecords.Any())
                        {
                            var extraFieldRecordsOnEmployee = extraFieldRecords.Where(w => w.RecordId == employee.EmployeeId).ToList();

                            foreach (var column in _reportDataInput.Columns.Where(w => w.Column == TermGroup_EmployeeMeetingMatrixColumns.ExtraFieldEmployee))
                            {
                                if (column.Selection?.Options?.Key != null && int.TryParse(column.Selection.Options.Key, out int recordId))
                                {
                                    var matchOnEmployee = extraFieldRecordsOnEmployee.FirstOrDefault(f => f.ExtraFieldId == recordId);
                                    if (matchOnEmployee != null)
                                    {
                                        extraField.Add(new ExtraFieldAnalysisField(matchOnEmployee, yesNoDictionary));
                                    }
                                }
                            }
                            if (extraField.Count == 0)
                            {
                                extraField.Add(new ExtraFieldAnalysisField(null));
                            }
                        }

                        string employeeCategoryString = null;
                        List<EmployeePosition> employeePositions = loadPositions ? EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, loadSysPosition: true).DistinctBy(i => i.PositionId).ToList() : new List<EmployeePosition>();
                        List<EmployeeMeeting> employeeMeetings = EmployeeManager.GetEmployeeMeetings(employee.EmployeeId, base.ActorCompanyId, base.UserId, true).Where(m => m.StartTime >= selectionDateFrom && m.StartTime <= selectionDateTo).ToList();
                        List<EmploymentDTO> employments = employee.GetEmployments(selectionDateFrom, selectionDateTo, includeSecondary: true).ToSplittedDTOs();
                        EmploymentDTO lastEmployment = employments.OrderBy(o => o.DateFrom).LastOrDefault();
                        if (lastEmployment != null && !employments.Any(e => e.EmploymentId == lastEmployment.EmploymentId))
                        {
                            employments.Add(lastEmployment);
                            employments = employments.OrderBy(o => o.DateFrom).ToList();
                        }

                        if (!employments.IsNullOrEmpty())
                        {

                            #region ------ Employment type ------
                            employmentTypeNames = "";
                            foreach (EmploymentDTO employment in employments)
                            {
                                if (employmentTypeNames == "")
                                    employmentTypeNames = $"{employment.EmploymentTypeName}";
                                else
                                {
                                    if (!employmentTypeNames.Contains(employment.EmploymentTypeName))
                                        employmentTypeNames += $", {employment.EmploymentTypeName}";
                                }
                            }
                            #endregion ------ Employment type ------



                            if (useAccountHierarchy)
                            {
                                var connectedToAccounts = employeeAccounts.Where(r => r.EmployeeId == employee.EmployeeId && r.Default);
                                //employeeAccountsString = string.Join(", ", connectedToAccounts.Select(t => t.Account?.AccountNrPlusName));
                                if (!connectedToAccounts.IsNullOrEmpty())
                                {
                                    foreach (var connectedToAccount in connectedToAccounts)
                                    {
                                        bool filteredOk = true;

                                        if (selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeAccount && !filteredOk)
                                            continue;

                                        foreach (EmployeeMeeting employeeMeeting in employeeMeetings)
                                        {

                                            #region ------ Create item ------

                                            var meetingItem = new EmployeeMeetingItem();

                                            if (loadAccountInternal && companyAccountsDTO.Any(a => a.AccountId == connectedToAccount.AccountId))
                                            {
                                                if (meetingItem.AccountAnalysisFields == null)
                                                {
                                                    meetingItem.AccountAnalysisFields = new List<AccountAnalysisField>();
                                                }
                                                if (meetingItem.ExtraFieldAnalysisFields == null)
                                                {
                                                    meetingItem.ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>();
                                                }
                                                var accountDTO = companyAccountsDTO.FirstOrDefault(a => a.AccountId == connectedToAccount.AccountId);
                                                if (accountDTO.ParentAccounts != null)
                                                {
                                                    foreach (var parentAccount in accountDTO.ParentAccounts)
                                                    {
                                                        meetingItem.AccountAnalysisFields.Add(new AccountAnalysisField(parentAccount));
                                                    }
                                                }
                                                meetingItem.AccountAnalysisFields.Add(new AccountAnalysisField(accountDTO));
                                            }

                                            meetingItem.EmployeeNr = employee.EmployeeNr;
                                            meetingItem.EmployeeName = employee.Name;
                                            meetingItem.FirstName = employee.FirstName;
                                            meetingItem.LastName = employee.LastName;
                                            meetingItem.Gender = GetValueFromDict((int)employee.Sex, sexDict);
                                            meetingItem.BirthDate = CalendarUtility.GetDateTime(employee.ContactPerson.SocialSec.Substring(0, 8), "yyyyMMdd");
                                            meetingItem.EmploymentType = employmentTypeNames;
                                            meetingItem.Position = !employeePositions.IsNullOrEmpty() ? string.Join(", ", employeePositions.Select(t => t.Position?.Name ?? string.Empty)) : string.Empty;
                                            meetingItem.SSYKCode = !employeePositions.IsNullOrEmpty() ? string.Join(", ", employeePositions.Select(t => t.Position?.SysPositionCode ?? string.Empty)) : string.Empty;
                                            meetingItem.MeetingType = employeeMeeting.FollowUpType?.Name ?? string.Empty;
                                            meetingItem.StartTime = employeeMeeting.StartTime;
                                            meetingItem.Participants = !employeeMeeting.Participant.IsNullOrEmpty() ? string.Join(", ", employeeMeeting.Participant?.Select(t => t.Name)) : string.Empty;
                                            meetingItem.OtherParticipants = employeeMeeting.OtherParticipants;
                                            meetingItem.Completed = employeeMeeting.Completed;
                                            meetingItem.Reminder = employeeMeeting.Reminder;
                                            meetingItem.CategoryName = string.Empty;
                                            meetingItem.ExtraFieldAnalysisFields = extraField;
                                            _reportDataOutput.EmployeeMeetingItems.Add(meetingItem);

                                            #endregion ------ Create item ------
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (EmployeeMeeting employeeMeeting in employeeMeetings)
                                    {
                                        #region ------ Create item ------

                                        var meetingItem = new EmployeeMeetingItem()
                                        {
                                            EmployeeNr = employee.EmployeeNr,
                                            EmployeeName = employee.Name,
                                            FirstName = employee.FirstName,
                                            LastName = employee.LastName,
                                            Gender = GetValueFromDict((int)employee.Sex, sexDict),
                                            BirthDate = CalendarUtility.GetDateTime(employee.ContactPerson.SocialSec.Substring(0, 8), "yyyyMMdd"),
                                            EmploymentType = employmentTypeNames,
                                            Position = !employeePositions.IsNullOrEmpty() ? string.Join(", ", employeePositions.Select(t => t.Position?.Name ?? string.Empty)) : string.Empty,
                                            SSYKCode = !employeePositions.IsNullOrEmpty() ? string.Join(", ", employeePositions.Select(t => t.Position?.SysPositionCode ?? string.Empty)) : string.Empty,
                                            MeetingType = employeeMeeting.FollowUpType?.Name ?? string.Empty,
                                            StartTime = employeeMeeting.StartTime,
                                            Participants = !employeeMeeting.Participant.IsNullOrEmpty() ? string.Join(", ", employeeMeeting.Participant?.Select(t => t.Name)) : string.Empty,
                                            OtherParticipants = employeeMeeting.OtherParticipants,
                                            Completed = employeeMeeting.Completed,
                                            CategoryName = string.Empty,
                                            ExtraFieldAnalysisFields = extraField,
                                            AccountAnalysisFields = new List<AccountAnalysisField>()
                                        };

                                        _reportDataOutput.EmployeeMeetingItems.Add(meetingItem);

                                        #endregion ------ Create item ------
                                    }
                                }

                            }
                            else
                            {
                                if (loadCategories)
                                {
                                    var selectedEmployeeCategoryRecords = companyCategoryRecords.Where(r => r.RecordId == employee.EmployeeId && r.Default) ?? null; //Category

                                    //If category filter is applied
                                    if (selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeCategory && !selectionCategoryIds.IsNullOrEmpty())
                                    {
                                        selectedEmployeeCategoryRecords = selectedEmployeeCategoryRecords.Where(r => selectionCategoryIds.Contains(r.CategoryId)).ToList();
                                    }
                                    if (!selectedEmployeeCategoryRecords.IsNullOrEmpty())
                                        employeeCategoryString = string.Join(", ", selectedEmployeeCategoryRecords.Select(t => t.Category?.Name ?? string.Empty));
                                }

                                foreach (EmployeeMeeting employeeMeeting in employeeMeetings)
                                {
                                    #region ------ Create item ------

                                    var meetingItem = new EmployeeMeetingItem()
                                    {
                                        EmployeeNr = employee.EmployeeNr,
                                        EmployeeName = employee.Name,
                                        FirstName = employee.FirstName,
                                        LastName = employee.LastName,
                                        Gender = GetValueFromDict((int)employee.Sex, sexDict),
                                        BirthDate = CalendarUtility.GetDateTime(employee.ContactPerson.SocialSec.Substring(0, 8), "yyyyMMdd"),
                                        EmploymentType = employmentTypeNames,
                                        Position = !employeePositions.IsNullOrEmpty() ? string.Join(", ", employeePositions.Select(t => t.Position?.Name ?? string.Empty)) : string.Empty,
                                        SSYKCode = !employeePositions.IsNullOrEmpty() ? string.Join(", ", employeePositions.Select(t => t.Position?.SysPositionCode ?? string.Empty)) : string.Empty,
                                        MeetingType = employeeMeeting.FollowUpType?.Name ?? string.Empty,
                                        StartTime = employeeMeeting.StartTime,
                                        Participants = !employeeMeeting.Participant.IsNullOrEmpty() ? string.Join(", ", employeeMeeting.Participant?.Select(t => t.Name)) : string.Empty,
                                        OtherParticipants = employeeMeeting.OtherParticipants,
                                        Completed = employeeMeeting.Completed,
                                        CategoryName = employeeCategoryString ?? string.Empty,
                                        AccountAnalysisFields = new List<AccountAnalysisField>(),
                                        ExtraFieldAnalysisFields = new List<ExtraFieldAnalysisField>()
                                    };

                                    _reportDataOutput.EmployeeMeetingItems.Add(meetingItem);

                                    #endregion ------ Create item ------
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(ex);
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

    public class EmployeeMeetingReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeMeetingMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeMeetingReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            var col = this.Selection != null && this.ColumnKey != null && (this.Selection.Options?.Key ?? "").Length > 0 ? this.ColumnKey.Replace(this.Selection.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeMeetingMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_EmployeeMeetingMatrixColumns.Unknown;
        }
    }

    public class EmployeeMeetingReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeMeetingItem> EmployeeMeetingItems { get; set; }
        public EmployeeMeetingReportDataInput Input { get; set; }

        public EmployeeMeetingReportDataOutput(EmployeeMeetingReportDataInput input)
        {
            this.EmployeeMeetingItems = new List<EmployeeMeetingItem>();
            this.Input = input;
        }
    }

    public class EmployeeMeetingReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeMeetingReportDataField> Columns { get; set; }

        public EmployeeMeetingReportDataInput(CreateReportResult reportResult, List<EmployeeMeetingReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

}
