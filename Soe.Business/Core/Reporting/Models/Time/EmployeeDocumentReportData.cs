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
    public class EmployeeDocumentReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly EmployeeDocumentReportDataInput _reportDataInput;
        private readonly EmployeeDocumentReportDataOutput _reportDataOutput;

        private bool loadCategories => _reportDataInput.Columns.Any(a => a.Column == TermGroup_EmployeeDocumentMatrixColumns.CategoryName);
        private bool loadAccountInternal => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("ccountInternal"));
        private bool loadExtraFields => _reportDataInput.Columns.Any(a => a.ColumnKey.Contains("xtraField"));

        public EmployeeDocumentReportData(ParameterObject parameterObject, EmployeeDocumentReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new EmployeeDocumentReportDataOutput(reportDataInput);
        }

        public EmployeeDocumentReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            TryGetBoolFromSelection(reportResult, out bool includeUserDocuments, "includeUserDocuments");
            TryGetIncludeInactiveFromSelection(reportResult, out bool selectionIncludeInactive, out _, out bool? selectionActiveEmployees);

            List<int> selectionCategoryIds = reportResult?.Input?.GetSelection<EmployeeSelectionDTO>("employees")?.CategoryIds;

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();

            #endregion

            try
            {

                using (CompEntities entities = new CompEntities())
                {
                    using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
                    bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entitiesReadonly, reportResult.ActorCompanyId);

                    #region Terms and dictionaries

                    int langId = GetLangId();
                    Dictionary<int, string> yesNoDict = base.GetTermGroupDict(TermGroup.YesNo, langId);
                    Dictionary<int, string> attestStatusDict = base.GetTermGroupDict(TermGroup.DataStorageRecordAttestStatus, langId);

                    List<AttestState> attestStates = AttestManager.GetAttestStates(entities, base.ActorCompanyId);
                    List<EmployeeAccount> employeeAccounts = new List<EmployeeAccount>();
                    List<CompanyCategoryRecord> companyCategoryRecords = new List<CompanyCategoryRecord>();
                    List<ExtraFieldRecordDTO> extraFieldRecords = new List<ExtraFieldRecordDTO>();
                    List<AccountDTO> companyAccountsDTO = new List<AccountDTO>();

                    #endregion

                    if (!useAccountHierarchy)
                    {
                        companyCategoryRecords = CategoryManager.GetCompanyCategoryRecords(SoeCategoryType.Employee, base.ActorCompanyId);
                    }

                    if (loadAccountInternal)
                    {
                        companyAccountsDTO = AccountManager.GetAccountsByCompany(base.ActorCompanyId, onlyInternal: true, loadAccount: true, loadAccountDim: true, loadAccountMapping: true).ToDTOs();
                        companyAccountsDTO.ForEach(f => f.ParentAccounts = f.GetParentAccounts(companyAccountsDTO));
                        if (useAccountHierarchy)
                            employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, selectionEmployeeIds, selectionDateFrom, selectionDateTo);
                    }
                    if (loadExtraFields)
                    {
                        extraFieldRecords = ExtraFieldManager.GetExtraFieldRecords(entities, selectionEmployeeIds, (int)SoeEntityType.Employee, reportResult.ActorCompanyId, true, true).ToDTOs();
                    }


                    #region ------ Load employees ------
                    if (employees == null)
                        employees = EmployeeManager.GetAllEmployeesByIds(reportResult.Input.ActorCompanyId, selectionEmployeeIds, active: selectionActiveEmployees);

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
                        string employeeCategoryString = null;

                        #region Get documents attached in messages
                        List<DocumentDTO> employeeDocuments = new List<DocumentDTO>();
                        List<DocumentDTO> userDocuments = new List<DocumentDTO>();

                        if (!employee.UserReference.IsLoaded)
                            employee.UserReference.Load();

                        if (!employee.UserId.IsNullOrEmpty())
                        {
                            userDocuments = GeneralManager.GetMyDocuments(base.ActorCompanyId, 0, (int)employee.UserId);

                            // Filter on date selection (maybe move into manager method for better performance)
                            if (selectionDateFrom.Date == selectionDateTo.Date)
                                userDocuments = userDocuments.Where(d => d.Created <= selectionDateTo).ToList();
                            else
                                userDocuments = userDocuments.Where(d => d.Created >= selectionDateFrom && d.Created <= selectionDateTo).ToList();

                        }
                        #endregion

                        #region Get documents connected to employee
                        List<DataStorageRecord> employeeDocumentsConnected = GeneralManager.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, employee.EmployeeId, SoeEntityType.Employee, includeDataStorage: true, includeDataStorageRecipient: true, skipDecompress: true, includeAttestState: true, loadConfirmationStatus: true);

                        // Filter on date selection (maybe move into manager method for better performance)
                        if (selectionDateFrom.Date == selectionDateTo.Date)
                            employeeDocumentsConnected = employeeDocumentsConnected.Where(d => d.DataStorage.Created <= selectionDateTo).ToList();
                        else
                            employeeDocumentsConnected = employeeDocumentsConnected.Where(d => d.DataStorage.Created >= selectionDateFrom && d.DataStorage.Created <= selectionDateTo).ToList();

                        // Add userDocuments that is not in employeeDocumentsConnected
                        if (includeUserDocuments)
                        {
                            foreach (DocumentDTO userDocument in userDocuments.Where(d => !employeeDocumentsConnected.Any(e => e.DataStorageId == d.DataStorageId)).ToList())
                            {
                                DocumentDTO document = userDocument.CloneDTO();

                                DataStorageRecipientDTO recipient = document.Recipients.FirstOrDefault(r => r.UserId == employee.UserId);
                                if (recipient != null && recipient.ConfirmedDate.HasValue)
                                    document.ConfirmedDate = recipient.ConfirmedDate;
                                else if (document.AnswerDate.HasValue) //Its needed because the Web client looks only on answerdate, the app looks on confirmeddate.
                                    document.ConfirmedDate = document.AnswerDate;

                                employeeDocuments.Add(document);
                            }
                        }

                        foreach (DataStorageRecord employeeDocumentConnected in employeeDocumentsConnected)
                        {
                            DocumentDTO userDocument = userDocuments.FirstOrDefault(d => d.DataStorageId == employeeDocumentConnected.DataStorageId);

                            DocumentDTO document = employeeDocumentConnected.DataStorage.ToDocumentDTO();
                            document.Name = employeeDocumentConnected.DataStorage.Name;
                            document.Description = employeeDocumentConnected.DataStorage.Description;
                            document.Folder = employeeDocumentConnected.DataStorage.Folder;
                            document.Created = employeeDocumentConnected.DataStorage.Created;
                            document.AttestStatus = (TermGroup_DataStorageRecordAttestStatus)employeeDocumentConnected.AttestStatus;
                            document.CurrentAttestUsers = employeeDocumentConnected.CurrentAttestUsers;
                            document.AttestStateId = employeeDocumentConnected.AttestStateId;

                            // Add user related insformation of the document
                            document.MessageId = userDocument?.MessageId ?? null;
                            document.ReadDate = userDocument?.ReadDate ?? null;
                            document.NeedsConfirmation = userDocument?.NeedsConfirmation ?? employeeDocumentConnected.DataStorage.NeedsConfirmation;
                            document.AnswerType = userDocument?.AnswerType ?? XEMailAnswerType.None;
                            document.AnswerDate = userDocument?.AnswerDate ?? null;
                            if (document.Recipients.IsNullOrEmpty() && userDocument != null && !userDocument.Recipients.IsNullOrEmpty())
                                document.Recipients = userDocument.Recipients;

                            DataStorageRecipientDTO recipient = document.Recipients.FirstOrDefault(r => r.UserId == employee.UserId);
                            if (recipient != null && recipient.ConfirmedDate.HasValue)
                                document.ConfirmedDate = recipient.ConfirmedDate;
                            else if (document.AnswerDate.HasValue) //Its needed because the Web client looks only on answerdate, the app looks on confirmeddate.
                                document.ConfirmedDate = document.AnswerDate;

                            employeeDocuments.Add(document);
                        }
                        #endregion

                        List<ExtraFieldAnalysisField> extraField = new List<ExtraFieldAnalysisField>();

                        if (loadExtraFields && extraFieldRecords.Any())
                        {
                            var extraFieldRecordsOnEmployee = extraFieldRecords.Where(w => w.RecordId == employee.EmployeeId).ToList();

                            foreach (var column in _reportDataInput.Columns.Where(w => w.Column == TermGroup_EmployeeDocumentMatrixColumns.ExtraFieldEmployee))
                            {
                                if (column.Selection?.Options?.Key != null && int.TryParse(column.Selection.Options.Key, out int recordId))
                                {
                                    var matchOnEmployee = extraFieldRecordsOnEmployee.FirstOrDefault(f => f.ExtraFieldId == recordId);
                                    if (matchOnEmployee != null)
                                    {
                                        extraField.Add(new ExtraFieldAnalysisField(matchOnEmployee, yesNoDictionary));
                                        continue;
                                    }
                                }
                                extraField.Add(new ExtraFieldAnalysisField(null));
                            }
                        }

                        if (useAccountHierarchy)
                        {
                            var connectedToAccounts = employeeAccounts.Where(r => r.EmployeeId == employee.EmployeeId && r.Default);

                            if (!connectedToAccounts.IsNullOrEmpty())
                            {
                                foreach (var connectedToAccount in connectedToAccounts)
                                {
                                    bool filteredOk = true;

                                    if (selectionAccountingType == TermGroup_EmployeeSelectionAccountingType.EmployeeAccount && !filteredOk)
                                        continue;

                                    foreach (DocumentDTO employeeDocument in employeeDocuments)
                                    {
                                        EmployeeDocumentItem documentItem = new EmployeeDocumentItem();

                                        if (loadAccountInternal && companyAccountsDTO.Any(a => a.AccountId == connectedToAccount.AccountId))
                                        {
                                            var accountDTO = companyAccountsDTO.FirstOrDefault(a => a.AccountId == connectedToAccount.AccountId);
                                            foreach (var parentAccount in accountDTO.ParentAccounts)
                                            {
                                                documentItem.AccountAnalysisFields.Add(new AccountAnalysisField(parentAccount));
                                            }
                                            documentItem.AccountAnalysisFields.Add(new AccountAnalysisField(accountDTO));
                                        }

                                        #region ------ Create item ------

                                        documentItem.EmployeeNr = employee.EmployeeNr;
                                        documentItem.EmployeeName = employee.Name;
                                        documentItem.FirstName = employee.FirstName;
                                        documentItem.LastName = employee.LastName;
                                        documentItem.FileName = employeeDocument.FileName;
                                        documentItem.Description = employeeDocument.Description;
                                        documentItem.FileType = employeeDocument.Extension;
                                        documentItem.Created = employeeDocument.Created;
                                        documentItem.NeedsConfirmation = employeeDocument.NeedsConfirmation;
                                        documentItem.Confirmed = employeeDocument.ConfirmedDate != null;
                                        documentItem.ConfirmedDate = employeeDocument.ConfirmedDate;
                                        documentItem.Read = employeeDocument.ReadDate;
                                        documentItem.Answered = employeeDocument.AnswerDate;
                                        documentItem.AnswerType = GetValueFromDict((int)employeeDocument.AnswerType, yesNoDict);
                                        documentItem.ByMessage = !employeeDocument.MessageId.IsNullOrEmpty();
                                        documentItem.ValidFrom = employeeDocument.ValidFrom;
                                        documentItem.ValidTo = employeeDocument.ValidTo;
                                        documentItem.AttestStatus = GetValueFromDict((int)employeeDocument.AttestStatus, attestStatusDict);
                                        documentItem.AttestState = attestStates.FirstOrDefault(a => a.AttestStateId == employeeDocument.AttestStateId)?.Name ?? string.Empty;
                                        documentItem.CurrentAttestUsers = employeeDocument.CurrentAttestUsers;
                                        documentItem.CategoryName = string.Empty;
                                        documentItem.ExtraFieldAnalysisFields = extraField;

                                        _reportDataOutput.EmployeeDocumentItems.Add(documentItem);

                                        #endregion ------ Create item ------
                                    }

                                }
                            }
                            else
                            {
                                foreach (DocumentDTO employeeDocument in employeeDocuments)
                                {

                                    #region ------ Create item ------

                                    var documentItem = new EmployeeDocumentItem()
                                    {
                                        EmployeeNr = employee.EmployeeNr,
                                        EmployeeName = employee.Name,
                                        FirstName = employee.FirstName,
                                        LastName = employee.LastName,
                                        FileName = employeeDocument.FileName,
                                        Description = employeeDocument.Description,
                                        FileType = employeeDocument.Extension,
                                        Created = employeeDocument.Created,
                                        NeedsConfirmation = employeeDocument.NeedsConfirmation,
                                        Confirmed = employeeDocument.ConfirmedDate != null,
                                        ConfirmedDate = employeeDocument.ConfirmedDate,
                                        Read = employeeDocument.ReadDate,
                                        Answered = employeeDocument.AnswerDate,
                                        AnswerType = GetValueFromDict((int)employeeDocument.AnswerType, yesNoDict),
                                        ByMessage = !employeeDocument.MessageId.IsNullOrEmpty(),
                                        ValidFrom = employeeDocument.ValidFrom,
                                        ValidTo = employeeDocument.ValidTo,
                                        AttestStatus = GetValueFromDict((int)employeeDocument.AttestStatus, attestStatusDict),
                                        AttestState = attestStates.FirstOrDefault(a => a.AttestStateId == employeeDocument.AttestStateId)?.Name ?? string.Empty,
                                        CurrentAttestUsers = employeeDocument.CurrentAttestUsers,
                                        CategoryName = string.Empty,
                                        ExtraFieldAnalysisFields = extraField,
                                        AccountAnalysisFields = new List<AccountAnalysisField>()
                                    };

                                    _reportDataOutput.EmployeeDocumentItems.Add(documentItem);

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

                            foreach (DocumentDTO employeeDocument in employeeDocuments)
                            {

                                #region ------ Create item ------

                                var documentItem = new EmployeeDocumentItem()
                                {
                                    EmployeeNr = employee.EmployeeNr,
                                    EmployeeName = employee.Name,
                                    FirstName = employee.FirstName,
                                    LastName = employee.LastName,
                                    FileName = employeeDocument.FileName,
                                    Description = employeeDocument.Description,
                                    FileType = employeeDocument.Extension,
                                    Created = employeeDocument.Created,
                                    NeedsConfirmation = employeeDocument.NeedsConfirmation,
                                    Confirmed = employeeDocument.ConfirmedDate != null,
                                    ConfirmedDate = employeeDocument.ConfirmedDate,
                                    Read = employeeDocument.ReadDate,
                                    Answered = employeeDocument.AnswerDate,
                                    AnswerType = GetValueFromDict((int)employeeDocument.AnswerType, yesNoDict),
                                    ByMessage = !employeeDocument.MessageId.IsNullOrEmpty(),
                                    ValidFrom = employeeDocument.ValidFrom,
                                    ValidTo = employeeDocument.ValidTo,
                                    AttestStatus = GetValueFromDict((int)employeeDocument.AttestStatus, attestStatusDict),
                                    AttestState = attestStates.FirstOrDefault(a => a.AttestStateId == employeeDocument.AttestStateId)?.Name ?? string.Empty,
                                    CurrentAttestUsers = employeeDocument.CurrentAttestUsers,
                                    CategoryName = employeeCategoryString ?? string.Empty,
                                    ExtraFieldAnalysisFields = extraField,
                                    AccountAnalysisFields = new List<AccountAnalysisField>()
                                };

                                _reportDataOutput.EmployeeDocumentItems.Add(documentItem);

                                #endregion ------ Create item ------
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

    public class EmployeeDocumentReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_EmployeeDocumentMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public EmployeeDocumentReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field ?? "" + (Selection?.Options?.Key ?? "");
            var col = (Selection?.Options?.Key ?? "").Length > 0 ? ColumnKey.Replace(Selection?.Options?.Key ?? "", "") : ColumnKey;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_EmployeeDocumentMatrixColumns>(col.FirstCharToUpperCase()) : TermGroup_EmployeeDocumentMatrixColumns.Unknown;
        }
    }

    public class EmployeeDocumentReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<EmployeeDocumentReportDataField> Columns { get; set; }

        public EmployeeDocumentReportDataInput(CreateReportResult reportResult, List<EmployeeDocumentReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class EmployeeDocumentReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<EmployeeDocumentItem> EmployeeDocumentItems { get; set; }
        public EmployeeDocumentReportDataInput Input { get; set; }

        public EmployeeDocumentReportDataOutput(EmployeeDocumentReportDataInput input)
        {
            this.EmployeeDocumentItems = new List<EmployeeDocumentItem>();
            this.Input = input;
        }
    }
}
