using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using SoftOne.Soe.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class EmployeeChangeIOItem
    {
        #region Variables

        public List<EmployeeChangeIODTO> EmployeeChangeIODTOs { get; set; }

        private readonly ApiLookupEmployee lookup = null;
        private readonly Dictionary<EmployeeChangeType, bool> permissions = null;
        public bool IsTest { get; private set; }
        public bool SaveUser { get; set; }
        public readonly static char DELIMETER = '#';

        #endregion

        #region Ctor

        public EmployeeChangeIOItem(List<EmployeeChangeIODTO> employeeChangeIODTOs, ApiLookupEmployee lookup, Dictionary<EmployeeChangeType, bool> permissions)
        {
            this.EmployeeChangeIODTOs = employeeChangeIODTOs;
            this.lookup = lookup;
            this.permissions = permissions;
        }

        #endregion

        #region Valid rows

        private List<EmployeeChangeRowIODTO> validRows;
        private bool TrySetValidRows(EmployeeChangeIODTO employeeChange)
        {
            this.validRows = GetValidEmployeeChangeRows(employeeChange);
            return !this.validRows.IsNullOrEmpty();
        }
        private List<EmployeeChangeRowIODTO> GetRows(params EmployeeChangeType[] types)
        {
            return (types.IsNullOrEmpty() ? this.validRows : this.validRows?.Where(row => types.Contains(row.EmployeeChangeType)).ToList()) ?? new List<EmployeeChangeRowIODTO>();
        }
        private EmployeeChangeRowIODTO GetFirstRow(EmployeeChangeType type)
        {
            return this.validRows?.FirstOrDefault(row => row.EmployeeChangeType == type);
        }
        private EmployeeChangeRowIODTO GetLastRow(EmployeeChangeType type)
        {
            return this.validRows?.LastOrDefault(row => row.EmployeeChangeType == type);
        }
        private bool HasAnyRow(params EmployeeChangeType[] types)
        {
            return this.validRows?.Any(row => types.Contains(row.EmployeeChangeType)) ?? false;
        }
        private bool TryGetRow(EmployeeChangeType type, out EmployeeChangeRowIODTO row)
        {
            row = GetLastRow(type);
            return row != null;
        }
        private int GetNrOfRows(EmployeeChangeType type)
        {
            return GetRows(type).Count;
        }

        #endregion

        #region Public methods

        public void ApplyChangesToEmployee(EmployeeUserImport import)
        {
            if (import?.EmployeeUser == null || !Valid())
                return;

            #region Prereq

            EmployeeUserDTO employee = import.EmployeeUser;
            EmployeeChangeIODTO employeeChange = GetEmployeeChange(employee);
            if (!TrySetValidRows(employeeChange))
                return;

            UserRolesDTO userRoles = GetUserRoles(employee);

            #endregion

            #region Employment

            if (HasAnyRow(EmployeeChangeType.NewEmployments) || employee.EmployeeId == 0)
                CreateNewEmployments(employee);

            EmploymentDTO employment = GetEmployment(employee, date: null) ?? CreateEmployment(employee, CalendarUtility.DATETIME_DEFAULT);
            SetEmploymentStartAndStopDate(employee, employment, userRoles);
            SetClearScheduleFrom(employee);

            #endregion

            #region EmployeeChangeType

            if (TryGetRow(EmployeeChangeType.ChangeToNotTemporaryPrimary, out EmployeeChangeRowIODTO changeToNotTemporaryRow))
                SetEmploymentNotTemporary(employee, changeToNotTemporaryRow);

            bool setOptionalCodesFirst = GetRows().Any(a => string.IsNullOrEmpty(a.OptionalEmploymentExternalCode));
            if (setOptionalCodesFirst)
                SetEmploymentExternalCode(employee, GetRows(EmployeeChangeType.EmploymentExternalCode));

            if (HasAnyRow(EmployeeChangeType.HierarchicalAccount))
                SetHierarchicalAccount(employee, GetRows(EmployeeChangeType.HierarchicalAccount));

            foreach (EmployeeChangeRowIODTO row in GetRows())
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.Active:
                        SetActive(employee, row);
                        break;
                    case EmployeeChangeType.FirstName:
                        SetFirstName(employee, row);
                        break;
                    case EmployeeChangeType.LastName:
                        SetLastName(employee, row);
                        break;
                    case EmployeeChangeType.SocialSec:
                        SetSocialSec(employee, row);
                        break;
                    case EmployeeChangeType.Vacant:
                        SetVacant(employee, row);
                        break;
                    case EmployeeChangeType.DisbursementMethod:
                        SetDisbursementMethod(employee, row);
                        break;
                    case EmployeeChangeType.DisbursementAccountNr:
                        SetDisbursementClearingAndAccountNr(employee, row);
                        break;
                    case EmployeeChangeType.ExternalCode:
                        SetExternalCode(employee, row);
                        break;
                    case EmployeeChangeType.EmployeeTemplateId:
                        SetEmployeeTemplateId(employee, row);
                        break;
                    case EmployeeChangeType.Email:
                        SetEmail(employee, row);
                        break;
                    case EmployeeChangeType.PhoneHome:
                        SetPhoneHome(employee, row);
                        break;
                    case EmployeeChangeType.PhoneMobile:
                        SetPhoneMobile(employee, row);
                        break;
                    case EmployeeChangeType.PhoneJob:
                        SetPhoneJob(employee, row);
                        break;
                    case EmployeeChangeType.ExtraFieldEmployee:
                        SetExtraField(employee, row);
                        break;
                    case EmployeeChangeType.ExcludeFromPayroll:
                        SetExcludeFromPayroll(employee, row);
                        break;
                    case EmployeeChangeType.WantsExtraShifts:
                        SetWantsExtraShifts(employee, row);
                        break;
                    case EmployeeChangeType.EmployeeGroup:
                        SetEmployeeGroup(employee, row);
                        break;
                    case EmployeeChangeType.PayrollGroup:
                        SetPayrollGroup(employee, row);
                        break;
                    case EmployeeChangeType.VacationGroup:
                        SetVacationGroup(employee, row);
                        break;
                    case EmployeeChangeType.AnnualLeaveGroup:
                        SetAnnualLeaveGroup(employee, row);
                        break;
                    case EmployeeChangeType.EmploymentPriceType:
                        SetEmploymentPriceType(employee, row);
                        break;
                    case EmployeeChangeType.EmploymentExternalCode:
                        if (!setOptionalCodesFirst)
                            SetEmploymentExternalCode(employee, row);
                        break;
                    case EmployeeChangeType.IsSecondaryEmployment:
                        SetEmploymentIsSecondaryEmployment(employee, row);
                        break;
                    case EmployeeChangeType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
                        SetEmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(employee, row);
                        break;
                    case EmployeeChangeType.WorkTasks:
                        SetEmploymentWorkTasks(employee, row);
                        break;
                    case EmployeeChangeType.EmploymentEndReason:
                        SetEmploymentEndReason(employee, row);
                        break;
                    case EmployeeChangeType.EmploymentType:
                        SetEmploymentType(employee, row);
                        break;
                    case EmployeeChangeType.TaxRate:
                        SetEmployeeTaxRate(employee, row);
                        break;
                    case EmployeeChangeType.TaxTinNumber:
                        SetEmployeeTaxTinNumber(employee, row);
                        break;
                    case EmployeeChangeType.TaxCountryCode:
                        SetEmployeeTaxCountryCode(employee, row);
                        break;
                    case EmployeeChangeType.TaxBirthPlace:
                        SetEmployeeTaxBirthPlace(employee, row);
                        break;
                    case EmployeeChangeType.TaxCountryCodeBirthPlace:
                        SetEmployeeTaxCountryCodeBirthPlace(employee, row);
                        break;
                    case EmployeeChangeType.TaxCountryCodeCitizen:
                        SetEmployeeTaxCountryCodeCitizen(employee, row);
                        break;
                    case EmployeeChangeType.SpecialConditions:
                        SetEmploymentSpecialConditions(employee, row);
                        break;
                    case EmployeeChangeType.WorkPlace:
                        SetEmploymentWorkPlace(employee, row);
                        break;
                    case EmployeeChangeType.ExperienceMonths:
                        SetEmploymentExperienceMonths(employee, row);
                        break;
                    case EmployeeChangeType.ExperienceAgreedOrEstablished:
                        SetEmploymentExperienceAgreedOrEstablished(employee, row);
                        break;
                    case EmployeeChangeType.SubstituteFor:
                        SetEmploymentSubstituteFor(employee, row);
                        break;
                    case EmployeeChangeType.SubstituteForDueTo:
                        SetEmploymentSubstituteForDueTo(employee, row);
                        break;
                    case EmployeeChangeType.BlockedFromDate:
                        SetBlockedFromDate(employee, row);
                        break;
                    case EmployeeChangeType.ExternalAuthId:
                        SetExternalAuthId(employee, row);
                        break;
                    case EmployeeChangeType.VacationDaysPaid:
                        SetVacationDaysPaid(employee, row);
                        break;
                    case EmployeeChangeType.VacationDaysUnPaid:
                        SetVacationDaysUnPaid(employee, row);
                        break;
                    case EmployeeChangeType.VacationDaysAdvance:
                        SetVacationDaysAdvance(employee, row);
                        break;
                    case EmployeeChangeType.DoNotValidateAccount:
                        SetDoNotValidateAccount(employee, row);
                        break;
                    case EmployeeChangeType.DisbursementCountryCode:
                        SetDisbursementCountryCode(employee, row);
                        break;
                    case EmployeeChangeType.DisbursementBIC:
                        SetDisbursementBIC(employee, row);
                        break;
                    case EmployeeChangeType.DisbursementIBAN:
                        SetDisbursementIBAN(employee, row);
                        break;
                    case EmployeeChangeType.ParentEmployeeNr:
                        SetParentEmployeeNr(employee, row);
                        break;
                }
            }

            //Attest/UserRoles
            if (HasAnyRow(GetTypesUserRole()))
                SetUserCompanyRoles(employee, GetRows(GetTypesUserRole()), userRoles);
            if (HasAnyRow(EmployeeChangeType.AttestRole))
                SetUserAttestRoles(employee, GetRows(EmployeeChangeType.AttestRole), userRoles);

            //Address/ECom
            if (HasAnyClosestRelativeRows())
                SetClosestRelatives(employee);
            if (HasAnyAddressRows())
                SetAddresses(employee);

            //Employment
            if (HasAnyRow(EmployeeChangeType.FullTimeWorkTimeWeekMinutes))
                SetEmploymentFullTimeWorkTimeWeekMinutes(employee, GetRows(EmployeeChangeType.FullTimeWorkTimeWeekMinutes));
            if (HasAnyRow(EmployeeChangeType.BaseWorkTimeWeek))
                SetEmploymentBaseWorkTimeWeek(employee, GetRows(EmployeeChangeType.BaseWorkTimeWeek));
            if (HasAnyRow(EmployeeChangeType.WorkTimeWeekMinutes))
                SetEmploymentWorkTimeWeekMinutes(employee, GetRows(EmployeeChangeType.WorkTimeWeekMinutes));
            if (HasAnyRow(EmployeeChangeType.EmploymentPercent))
                SetEmploymentPercent(employee, GetRows(EmployeeChangeType.EmploymentPercent));

            //Employee
            if (HasAnyRow(GetTypesEmployeePosition()))
                SetEmployeePositions(employee, GetRows(GetTypesEmployeePosition()));
            if (HasAnyRow(EmployeeChangeType.TimeWorkAccount))
                SetTimeWorkAccounts(employee, GetRows(EmployeeChangeType.TimeWorkAccount), employee.EmployeeTimeWorkAccounts);
            if (HasAnyRow(EmployeeChangeType.AccountNrSieDim))
                SetEmploymentAccounts(employee, GetRows(EmployeeChangeType.AccountNrSieDim));
            if (HasAnyRow(EmployeeChangeType.EmployerRegistrationNr))
                SetEmployerRegistrations(employee, GetRows(EmployeeChangeType.EmployerRegistrationNr));


            //Reports
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesPayrollStatistics()))
                SetReportSettingsPayrollStatistics(employee, GetRows(GetTypesReportEmployeeChangeTypesPayrollStatistics()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesControlTask()))
                SetReportSettingsControlTask(employee, GetRows(GetTypesReportEmployeeChangeTypesControlTask()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesAFA()))
                SetReportSettingsAFA(employee, GetRows(GetTypesReportEmployeeChangeTypesAFA()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesCollectum()))
                SetReportSettingsCollectum(employee, GetRows(GetTypesReportEmployeeChangeTypesCollectum()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesKPA()))
                SetReportSettingsKPA(employee, GetRows(GetTypesReportEmployeeChangeTypesKPA()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesBygglösen()))
                SetReportSettingsBygglösen(employee, GetRows(GetTypesReportEmployeeChangeTypesBygglösen()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesGTP()))
                SetReportSettingsGTP(employee, GetRows(GetTypesReportEmployeeChangeTypesGTP()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesAGI()))
                SetReportSettingsAGI(employee, GetRows(GetTypesReportEmployeeChangeTypesAGI()));
            if (HasAnyRow(GetTypesReportEmployeeChangeTypesIFMetall()))
                SetReportSettingsIFMetall(employee, GetRows(GetTypesReportEmployeeChangeTypesIFMetall()));

            ApplyKeeepEmploymentSettings(employee);

            #endregion
        }

        public List<GenericType> GetTerms()
        {
            return this.GetTerms(TermGroup.ApiEmployee);
        }

        public Dictionary<EmployeeChangeType, bool> GetPermissions()
        {
            return this.permissions ?? new Dictionary<EmployeeChangeType, bool>();
        }

        public TermGroup_Country GetCompanyCountry()
        {
            return this.lookup?.CmpanyCountryId ?? TermGroup_Country.Uknown;
        }

        public List<ContactAddressItem> GetContactAddressItems(string employeeNr)
        {
            if (this.lookup.ContactAddressItemsByEmployee.TryGetValue(employeeNr, out List<ContactAddressItem> l))
                return l;
            return null;
        }

        public List<EmployeePositionDTO> GetEmployeePositions(string employeeNr)
        {
            if (this.lookup.EmployeePositionsByEmployee.TryGetValue(employeeNr, out List<EmployeePositionDTO> l))
                return l;
            return null;
        }

        public List<ExtraFieldRecordDTO> GetExtraFieldRecords(string employeeNr)
        {
            if (this.lookup.ExtraFieldRecordsByEmployee.TryGetValue(employeeNr, out List<ExtraFieldRecordDTO> l))
                return l;
            return null;
        }

        public void TryAddContactAddressItems(string employeeNr, List<ContactAddressItem> contactAddressItems)
        {
            if (!employeeNr.IsNullOrEmpty() && !this.lookup.ContactAddressItemsByEmployee.ContainsKey(employeeNr))
                this.lookup.ContactAddressItemsByEmployee.Add(employeeNr, contactAddressItems ?? new List<ContactAddressItem>());
        }

        public void TryAddEmployeePositions(string employeeNr, List<EmployeePositionDTO> employeePositions)
        {
            if (!employeeNr.IsNullOrEmpty() && !this.lookup.EmployeePositionsByEmployee.ContainsKey(employeeNr))
                this.lookup.EmployeePositionsByEmployee.Add(employeeNr, employeePositions ?? new List<EmployeePositionDTO>());
        }

        public void TryAddExtraFieldRecords(string employeeNr, List<ExtraFieldRecordDTO> extraFieldRecords)
        {
            if (!employeeNr.IsNullOrEmpty() && !this.lookup.ExtraFieldRecordsByEmployee.ContainsKey(employeeNr))
                this.lookup.ExtraFieldRecordsByEmployee.Add(employeeNr, extraFieldRecords ?? new List<ExtraFieldRecordDTO>());
        }

        public void SetAsTest()
        {
            this.IsTest = true;
        }

        #endregion

        #region Field changes

        #region AttestRole

        private void SetUserAttestRoles(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows, UserRolesDTO userRole)
        {
            if (employee == null || userRole == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                #region Dates

                DateTime minDate = this.lookup.MinDate;
                DateTime maxDate = this.lookup.MaxDate;
                DateTime fromDate = ParseFromDate(row);
                DateTime? toDate = ParseToDate(row);

                #endregion

                #region AttestRole

                AttestRoleDTO attestRole = GetAttestRole(row);
                if (attestRole == null)
                {
                    AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueAttestRoleNotFound);
                    return;
                }

                #endregion

                #region Delete

                if (row.Delete)
                {
                    DeleteAttestRole(employee, userRole, attestRole);
                    continue;
                }

                #endregion

                #region Account

                AccountDTO account = GetAccountByOptionalExternalCode(row);
                if (!string.IsNullOrEmpty(row.OptionalExternalCode) && account == null)
                {
                    AddValidationError(employee, row, row.OptionalExternalCode, EmploymentChangeValidationError.InvalidOptionalExternalCodeAccountNotFound);
                    return;
                }

                int? accountId = account?.AccountId;
                if (!accountId.HasValue && !employee.Accounts.IsNullOrEmpty())
                {
                    List<EmployeeAccountDTO> accountsForDates = employee.Accounts.Where(i => CalendarUtility.IsDatesOverlapping(fromDate, (toDate ?? maxDate), i.DateFrom, i.DateTo ?? maxDate)).ToList();
                    if (accountsForDates.Count == 1)
                        accountId = accountsForDates.First().AccountId;
                }

                if (account == null && accountId.HasValue)
                    account = GetAccount(accountId.Value);

                #endregion

                #region Add/Update

                //If role is the same and account is the same and date
                UserAttestRoleDTO userAttestRole =
                    !toDate.HasValue && fromDate != CalendarUtility.DATETIME_DEFAULT
                    ?
                    userRole.AttestRoles?.FirstOrDefault(a => a.AttestRoleId == attestRole.AttestRoleId && a.AccountId == accountId && a.DateTo.HasValue && a.DateTo.Value == fromDate.AddDays(-1))
                    :
                    null;

                if (userAttestRole != null)
                {
                    if (userAttestRole.DateTo.HasValue)
                        UpdateAttestRoleDateTo(employee, userAttestRole, attestRole, null);
                }
                else
                {
                    userAttestRole = userRole.AttestRoles?.GetClosestAttestRole(attestRole, account, fromDate, toDate, minDate, maxDate);
                    if (userAttestRole != null)
                        UpdateAttestRole(employee, row, userAttestRole, attestRole, account, fromDate, toDate);
                    else
                        AddAttestRole(employee, userRole, attestRole, account, fromDate, toDate);
                }

                #endregion
            }

            SetSaveUser();
        }

        private AttestRoleDTO GetAttestRole(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return null;

            AttestRoleDTO attestRole = this.lookup.AttestRoles?.FirstOrDefault(ar => !string.IsNullOrEmpty(ar.Description) && ar.Description.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (attestRole == null)
                attestRole = this.lookup.AttestRoles?.FirstOrDefault(ar => !string.IsNullOrEmpty(ar.Name) && ar.Name.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (attestRole == null)
                attestRole = this.lookup.AttestRoles?.FirstOrDefault(ar => ar.ExternalCodes != null && ar.ExternalCodes.Where(w => !string.IsNullOrEmpty(w)).Select(s => s.Trim().ToLower()).Contains(row.Value.Trim().ToLower()));

            return attestRole;
        }

        private AttestRoleDTO GetAttestRole(int attestRoleId)
        {
            return this.lookup.AttestRoles?.FirstOrDefault(i => i.AttestRoleId == attestRoleId);
        }

        private void AddAttestRole(EmployeeUserDTO employee, UserRolesDTO userRole, AttestRoleDTO attestRole, AccountDTO account, DateTime? dateFrom, DateTime? dateTo)
        {
            if (employee == null || userRole == null || attestRole == null)
                return;

            UserAttestRoleDTO userAttestRole = new UserAttestRoleDTO()
            {
                AttestRoleId = attestRole.AttestRoleId,
                AccountId = account?.AccountId,
                IsExecutive = attestRole.IsExecutive,
            };

            if (dateFrom.HasValue && dateFrom != CalendarUtility.DATETIME_DEFAULT)
                userAttestRole.DateFrom = dateFrom.Value.Date;
            if (dateTo.HasValue && (!userAttestRole.DateFrom.HasValue || userAttestRole.DateFrom.Value <= dateTo.Value))
                userAttestRole.DateTo = dateTo.Value.Date;

            if (userRole.AttestRoles == null)
                userRole.AttestRoles = new List<UserAttestRoleDTO>();
            userRole.AttestRoles.Add(userAttestRole);
            userAttestRole.IsModified = true;
            userAttestRole.IsExecutive = attestRole.IsExecutive;

            employee.AddCurrentChange((int)EmployeeChangeType.AttestRole, null, GetAttestRoleAccountName(attestRole, account), fromDate: userAttestRole.DateFrom, toDate: userAttestRole.DateTo);
        }

        private void AddAttestRoleAccount(EmployeeUserDTO employee, UserAttestRoleDTO userAttestRole, AttestRoleDTO attestRole, AccountDTO account)
        {
            if (employee == null || userAttestRole == null || attestRole == null)
                return;
            if (userAttestRole.AccountId == account.AccountId)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AttestRole, 0, account.AccountId, null, account.Name);
            userAttestRole.AccountId = account.AccountId;
            userAttestRole.IsModified = true;
        }

        private void UpdateAttestRole(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, UserAttestRoleDTO userAttestRole, AttestRoleDTO attestRole, AccountDTO account, DateTime? dateFrom, DateTime? dateTo)
        {
            if (employee == null || row == null || userAttestRole == null || attestRole == null || account == null)
                return;

            if (userAttestRole.AccountId.HasValue && userAttestRole.AccountId.Value != account.AccountId)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidAttestRole);
                return;
            }

            if (dateFrom.HasValue && dateFrom.Value != CalendarUtility.DATETIME_DEFAULT && dateFrom < userAttestRole.DateFrom)
                UpdateAttestRoleDateFrom(employee, userAttestRole, attestRole, dateFrom.Value.Date);
            if (dateTo.HasValue && dateTo.Value != userAttestRole.DateTo && dateTo.Value >= userAttestRole.DateFrom)
                UpdateAttestRoleDateTo(employee, userAttestRole, attestRole, dateTo.Value.Date);
            if (!userAttestRole.AccountId.HasValue)
                AddAttestRoleAccount(employee, userAttestRole, attestRole, account);
        }

        private void UpdateAttestRoleDateFrom(EmployeeUserDTO employee, UserAttestRoleDTO userAttestRole, AttestRoleDTO attestRole, DateTime? date)
        {
            if (employee == null || userAttestRole == null || attestRole == null)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AttestRole, userAttestRole.DateFrom, date, attestRole.Name, attestRole.Name, "Fr.o.m", date, userAttestRole.DateTo);
            userAttestRole.DateFrom = date;
            userAttestRole.IsModified = true;
        }

        private void UpdateAttestRoleDateTo(EmployeeUserDTO employee, UserAttestRoleDTO userAttestRole, AttestRoleDTO attestRole, DateTime? date)
        {
            if (employee == null || userAttestRole == null || attestRole == null)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AttestRole, userAttestRole.DateTo, date, attestRole.Name, attestRole.Name, "T.o.m", userAttestRole.DateFrom, date);
            userAttestRole.DateTo = date;
            userAttestRole.IsModified = true;
        }

        private void DeleteAttestRole(EmployeeUserDTO employee, UserRolesDTO userRole, AttestRoleDTO attestRole)
        {
            if (employee == null || userRole == null || attestRole == null)
                return;

            UserAttestRoleDTO userAttestRole = userRole.AttestRoles?.FirstOrDefault(i => i.AttestRoleId == attestRole.AttestRoleId);
            if (userAttestRole == null)
                return;

            userAttestRole.State = SoeEntityState.Deleted;
            userAttestRole.IsModified = true;
            employee.AddCurrentChange((int)EmployeeChangeType.AttestRole, attestRole.AttestRoleId, 0, fromValueName: attestRole.Name);
        }

        private string GetAttestRoleAccountName(AttestRoleDTO attestRole, AccountDTO account)
        {
            if (attestRole == null)
                return null;

            return account != null ? $"{attestRole.Name}/{account.Name}" : attestRole.Name;
        }

        #endregion

        #region Contact

        private void SetEmail(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!ValidateEmail(employee, row, out string value))
                return;

            if (string.IsNullOrEmpty(employee.Email) || !employee.Email.Equals(value))
            {
                employee.AddCurrentChange((int)EmployeeChangeType.Email, employee.Email, value);
                employee.Email = value;
            }

            TrySaveContactECom(employee, row, EmployeeChangeType.Email, ContactAddressItemType.EComEmail, value, isSecret: IsSecretContactInformation(row));
            SetSaveUser();
        }

        private void SetPhoneHome(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!ValidatePhoneNumber(employee, row, out string value))
                return;

            TrySaveContactECom(employee, row, EmployeeChangeType.PhoneHome, ContactAddressItemType.EComPhoneHome, value, isSecret: IsSecretContactInformation(row));
            SetSaveUser();
        }

        private void SetPhoneJob(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!ValidatePhoneNumber(employee, row, out string value))
                return;

            TrySaveContactECom(employee, row, EmployeeChangeType.PhoneJob, ContactAddressItemType.EComPhoneJob, value, isSecret: IsSecretContactInformation(row));
            SetSaveUser();
        }

        private void SetPhoneMobile(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!ValidatePhoneNumber(employee, row, out string value))
                return;

            TrySaveContactECom(employee, row, EmployeeChangeType.PhoneMobile, ContactAddressItemType.EComPhoneMobile, value, isSecret: IsSecretContactInformation(row));
            SetSaveUser();
        }

        private void SetClosestRelatives(EmployeeUserDTO employee)
        {
            foreach (int sequence in GetClosestRelativeSequences())
            {
                if (HasAnyClosestRelativeRows(sequence))
                    SetClosestRelative(employee, sequence);
            }
        }

        private void SetClosestRelative(EmployeeUserDTO employee, int sequence)
        {
            if (employee == null)
                return;

            List<EmployeeChangeRowIODTO> closesRelativeRows = GetClosestRelativeRows(sequence);
            if (closesRelativeRows.IsNullOrEmpty())
                return;

            string number = GetClosestRelativeNr(closesRelativeRows, sequence);
            string name = GetClosestRelativeName(closesRelativeRows, sequence);
            string relation = GetClosestRelativeRelation(closesRelativeRows, sequence);
            string description = GetClosestRelativeDescription(name, relation);
            bool? hidden = GetClosestRelativeHidden(closesRelativeRows, sequence);

            EmployeeChangeRowIODTO closesRelativeRow = GetFirstRow(closesRelativeRows,
                EmployeeChangeType.ClosestRelativeNr,
                EmployeeChangeType.ClosestRelativeNr2,
                EmployeeChangeType.ClosestRelativeName,
                EmployeeChangeType.ClosestRelativeName2,
                EmployeeChangeType.ClosestRelativeRelation,
                EmployeeChangeType.ClosestRelativeRelation2,
                EmployeeChangeType.ClosestRelativeHidden,
                EmployeeChangeType.ClosestRelativeHidden2);

            TrySaveContactECom(employee, closesRelativeRow, GetClosestRelativeNrType(sequence), ContactAddressItemType.ClosestRelative, number, description, hidden, sequence);
            SetSaveUser();
        }

        private bool IsSecretContactInformation(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return false;
            return (!row.OptionalExternalCode.IsNullOrEmpty() && row.OptionalExternalCode.ToLower().Equals("secret") ? true : false);
        }

        private bool ValidateEmail(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, out string value)
        {
            value = row.Value;
            if (string.IsNullOrEmpty(value))
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmailCannotBeEmpty);
                return false;
            }
            return true;
        }

        private bool ValidatePhoneNumber(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, out string value)
        {
            value = StringUtility.CleanPhoneNumber(row.Value);
            if (string.IsNullOrEmpty(value))
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidPhoneNumberIsMandatory);
                return false;
            }
            return true;
        }

        #endregion

        #region ContactAddress

        private void SetAddresses(EmployeeUserDTO employee)
        {
            if (employee == null)
                return;

            List<EmployeeChangeRowIODTO> adressRows = GetAddressRows();

            string address = GetFirstValue(adressRows, EmployeeChangeType.Address);
            string addressCO = GetFirstValue(adressRows, EmployeeChangeType.AddressCO);
            string postalCode = GetFirstValue(adressRows, EmployeeChangeType.AddressPostCode);
            string postalAddress = GetFirstValue(adressRows, EmployeeChangeType.AddressPostalAddress);
            string country = GetFirstValue(adressRows, EmployeeChangeType.AddressCountry);
            bool? hidden = GetBool(adressRows, EmployeeChangeType.AddressHidden);

            TrySaveContactAddresses(employee, ContactAddressItemType.AddressDistribution, address, addressCO, postalCode, postalAddress, country, hidden);
            SetSaveUser();
        }

        private List<ContactAddressItem> GetContactAddressItem(string employeeNr, ContactAddressItemType type, int sequence = 1)
        {
            if (this.lookup.ContactAddressItemsByEmployee.IsNullOrEmpty() || !this.lookup.ContactAddressItemsByEmployee.ContainsKey(employeeNr))
                return null;

            var items = this.lookup.ContactAddressItemsByEmployee
                .GetList(employeeNr, nullIfNotFound: true)
                ?.Where(i => i.ContactAddressItemType == type)
                .ToList();

            if (items.IsNullOrEmpty())
                return new List<ContactAddressItem>();

            if (type == ContactAddressItemType.ClosestRelative)
            {
                int skip = sequence > 0 ? sequence - 1 : 0;
                return items.Skip(skip).FirstOrDefault().ObjToList();
            }
            else
                return items.ToList();
        }

        private void AddContactAddressItemToRepository(EmployeeUserDTO employee, ContactAddressItem item)
        {
            if (employee == null || item == null)
                return;

            if (this.lookup.ContactAddressItemsByEmployee.ContainsKey(employee.EmployeeNr))
            {
                var employeeItems = this.lookup.ContactAddressItemsByEmployee[employee.EmployeeNr];
                employeeItems.Add(item);
                this.lookup.ContactAddressItemsByEmployee[employee.EmployeeNr] = employeeItems;
            }
            else
            {
                this.lookup.ContactAddressItemsByEmployee.Add(employee.EmployeeNr, new List<ContactAddressItem> { item });
            }
        }

        #endregion

        #region ConctactAddress

        private void TrySaveContactAddresses(EmployeeUserDTO employee, ContactAddressItemType addressType, string address, string addressCO, string postalCode, string postalAddress, string country, bool? isSecret)
        {
            if (employee == null)
                return;

            var items = GetContactAddressItem(employee.EmployeeNr, addressType);
            if (!items.IsNullOrEmpty())
            {
                if (items.Count == 1)
                {
                    //Update first/only
                    UpdateContactAddressItem(employee, items.First(), address, addressCO, postalCode, postalAddress, country, isSecret);
                }
                else
                {
                    //Add if not exists
                    if (!items.Any(a => a.Address.Equals(address, StringComparison.OrdinalIgnoreCase)))
                        AddContactAddressItem(employee, addressType, address, addressCO, postalCode, postalAddress, country, isSecret);
                }
            }
            else
            {
                AddContactAddressItem(employee, addressType, address, addressCO, postalCode, postalAddress, country, isSecret);
            }
        }

        private void AddContactAddressItem(EmployeeUserDTO employee, ContactAddressItemType type, string address, string addressCO, string postalCode, string postalAddress, string country, bool? isSecret)
        {
            if (employee == null)
                return;

            var item = new ContactAddressItem()
            {
                ContactAddressItemType = type,
                SysContactAddressTypeId = (int)type,
                Name = this.lookup.ContactAddresses?.FirstOrDefault(i => i.Id == (int)type)?.Name,
                IsAddress = true,
            };

            UpdateContactAddressItem(employee, item, address, addressCO, postalCode, postalAddress, country, isSecret);
            item.SetDisplayAddress();
            AddContactAddressItemToRepository(employee, item);
        }

        private void UpdateContactAddressItem(EmployeeUserDTO employee, ContactAddressItem item, string address, string addressCO, string postalCode, string postalAddress, string country, bool? isSecret)
        {
            if (item == null || employee == null)
                return;

            UpdateContactAddress(employee, item, address);
            UpdateContactAddressCO(employee, item, addressCO);
            UpdateContactAddressPostalCode(employee, item, postalCode);
            UpdateContactAddressPostalAddress(employee, item, postalAddress);
            UpdateContactAddressCountry(employee, item, country);
            UpdateContactAddressSecret(employee, item, isSecret);
        }

        private void UpdateContactAddress(EmployeeUserDTO employee, ContactAddressItem item, string value)
        {
            if (item == null || employee == null || value.NullToEmpty().Equals(item.Address.NullToEmpty()))
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.Address, item.Address, value);
            item.Address = value;
        }

        private void UpdateContactAddressCO(EmployeeUserDTO employee, ContactAddressItem item, string value)
        {
            if (item == null || employee == null || value.NullToEmpty().Equals(item.AddressCO.NullToEmpty()))
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AddressCO, item.AddressCO, value);
            item.AddressCO = value;
        }

        private void UpdateContactAddressPostalCode(EmployeeUserDTO employee, ContactAddressItem item, string value)
        {
            if (item == null || employee == null || value.NullToEmpty().Equals(item.PostalCode.NullToEmpty()))
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AddressPostCode, item.PostalCode, value);
            item.PostalCode = value;
        }

        private void UpdateContactAddressPostalAddress(EmployeeUserDTO employee, ContactAddressItem item, string value)
        {
            if (item == null || employee == null || value.NullToEmpty().Equals(item.PostalAddress.NullToEmpty()))
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AddressPostalAddress, item.PostalAddress, value);
            item.PostalAddress = value;
        }

        private void UpdateContactAddressCountry(EmployeeUserDTO employee, ContactAddressItem item, string value)
        {
            if (item == null || employee == null || value.NullToEmpty().Equals(item.Country.NullToEmpty()))
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AddressCountry, item.Country, value);
            item.Country = value;
        }

        private void UpdateContactAddressSecret(EmployeeUserDTO employee, ContactAddressItem item, bool? value)
        {
            if (item == null || employee == null || (!value.HasValue || value.Value == item.IsSecret))
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.AddressHidden, item.AddressIsSecret, value);
            item.AddressIsSecret = value.Value;
            item.IsSecret = value.Value;
        }

        private bool HasAnyAddressRows() => !GetAddressRows().IsNullOrEmpty();
        private List<EmployeeChangeRowIODTO> GetAddressRows() => GetRows(EmployeeChangeType.Address, EmployeeChangeType.AddressCO, EmployeeChangeType.AddressPostCode, EmployeeChangeType.AddressPostalAddress, EmployeeChangeType.AddressCountry, EmployeeChangeType.AddressHidden);

        #endregion

        #region ContactECom

        private void TrySaveContactECom(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, EmployeeChangeType fieldType, ContactAddressItemType addressType, string value, string description = null, bool? isSecret = null, int sequence = 1)
        {
            if (employee == null || row == null)
                return;

            var items = GetContactAddressItem(employee.EmployeeNr, addressType, sequence);
            if (items == null)
                return;

            if (items.Any())
            {
                if (items.Count == 1)
                {
                    //Update first/only
                    UpdateContactEComItem(employee, items.First(), fieldType, value, description, isSecret, sequence);
                }
                else
                {
                    //Add if not exists
                    if (!items.Any(a => a.EComText.Equals(value, StringComparison.OrdinalIgnoreCase)))
                        AddContactEComItem(employee, row, fieldType, addressType, value, description, isSecret, sequence);
                }
            }
            else
            {
                //Add
                AddContactEComItem(employee, row, fieldType, addressType, value, description, isSecret, sequence);
            }
        }

        private void AddContactEComItem(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, EmployeeChangeType fieldType, ContactAddressItemType type, string value, string description = "", bool? isSecret = null, int sequence = 1)
        {
            if (employee == null || row == null)
                return;

            if (string.IsNullOrEmpty(value) && type == ContactAddressItemType.ClosestRelative)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueIsMandatoryClosestRelative);
                return;
            }

            int sysContactEComTypeId = ((int)type) - 10;

            var item = new ContactAddressItem()
            {
                ContactAddressItemType = type,
                SysContactEComTypeId = sysContactEComTypeId,
                Name = this.lookup.ContactEcoms?.FirstOrDefault(i => i.Id == sysContactEComTypeId)?.Name,
                IsAddress = false,
                EComText = value,
            };

            employee.AddCurrentChange((int)fieldType, null, value);
            UpdateContactEComDescriptionAndSecret(employee, item, fieldType, description, isSecret, sequence);
            AddContactAddressItemToRepository(employee, item);
        }

        private void UpdateContactEComItem(EmployeeUserDTO employee, ContactAddressItem item, EmployeeChangeType fieldType, string value, string description = "", bool? isSecret = null, int sequence = 1)
        {
            if (item == null || employee == null)
                return;

            if (!string.IsNullOrEmpty(value) && !item.EComText.Equals(value))
            {
                employee.AddCurrentChange((int)fieldType, item.EComText, value);
                item.EComText = value;
            }

            UpdateContactEComDescriptionAndSecret(employee, item, fieldType, description, isSecret, sequence);
        }

        private void UpdateContactEComDescriptionAndSecret(EmployeeUserDTO employee, ContactAddressItem item, EmployeeChangeType type, string description, bool? isSecret, int sequence = 1)
        {
            bool isClosestRelative = IsClosestRelativeNrType(type);

            if (!string.IsNullOrEmpty(description))
            {
                string existingDescription = item.EComDescription.NullToEmpty();
                if (!existingDescription.Equals(description))
                {
                    if (isClosestRelative)
                        UpdateClosestRelativeNameAndRelation(item, employee, description, sequence);
                    else
                        UpdateContactEComDescription(employee, item, type, description);
                }
            }

            if (isSecret.HasValue && item.IsSecret != isSecret.Value)
            {
                if (isClosestRelative)
                    UpdateClosestRelativeSecret(item, employee, isSecret.Value, sequence);
                else
                    UpdateContactEComSecret(employee, item, type, isSecret.Value);
            }
        }

        private void UpdateContactEComDescription(EmployeeUserDTO employee, ContactAddressItem item, EmployeeChangeType fieldType, string description)
        {
            if (item == null || employee == null)
                return;

            employee.AddCurrentChange((int)fieldType, item.EComDescription, description);
            item.EComDescription = description;
        }

        private void UpdateContactEComSecret(EmployeeUserDTO employee, ContactAddressItem item, EmployeeChangeType fieldType, bool isSecret)
        {
            if (item == null || employee == null)
                return;

            employee.AddCurrentChange((int)fieldType, item.IsSecret, isSecret);
            item.IsSecret = isSecret;
        }

        #region ClosestRelative

        private List<int> GetClosestRelativeSequences()
        {
            return new List<int> { 1, 2 };
        }

        private List<EmployeeChangeRowIODTO> GetClosestRelativeRows(int sequence)
        {
            List<EmployeeChangeRowIODTO> closestRelativeRows = null;

            switch (sequence)
            {
                case 1:
                    closestRelativeRows = GetRows(EmployeeChangeType.ClosestRelativeName, EmployeeChangeType.ClosestRelativeNr, EmployeeChangeType.ClosestRelativeRelation, EmployeeChangeType.ClosestRelativeHidden);
                    break;
                case 2:
                    closestRelativeRows = GetRows(EmployeeChangeType.ClosestRelativeName2, EmployeeChangeType.ClosestRelativeNr2, EmployeeChangeType.ClosestRelativeRelation2, EmployeeChangeType.ClosestRelativeHidden2);
                    break;
            }

            return closestRelativeRows ?? new List<EmployeeChangeRowIODTO>();
        }

        private bool HasAnyClosestRelativeRows()
        {
            return GetClosestRelativeSequences().Any(sequence => HasAnyClosestRelativeRows(sequence));
        }

        private bool HasAnyClosestRelativeRows(int sequence)
        {
            return !GetClosestRelativeRows(sequence).IsNullOrEmpty();
        }

        private bool IsClosestRelativeNrType(EmployeeChangeType type)
        {
            return
                type == EmployeeChangeType.ClosestRelativeNr ||
                type == EmployeeChangeType.ClosestRelativeNr2;
        }

        private EmployeeChangeType GetClosestRelativeNrType(int sequence)
        {
            EmployeeChangeType type = EmployeeChangeType.ClosestRelativeNr;
            if (sequence == 2)
                type = EmployeeChangeType.ClosestRelativeNr2;
            return type;
        }

        private EmployeeChangeType GetClosestRelativeNameType(int sequence)
        {
            EmployeeChangeType type = EmployeeChangeType.ClosestRelativeName;
            if (sequence == 2)
                type = EmployeeChangeType.ClosestRelativeName2;
            return type;
        }

        private EmployeeChangeType GetClosestRelativeRelationType(int sequence)
        {
            EmployeeChangeType type = EmployeeChangeType.ClosestRelativeRelation;
            if (sequence == 2)
                type = EmployeeChangeType.ClosestRelativeRelation2;
            return type;
        }

        private EmployeeChangeType GetClosestRelativeHiddenType(int sequence)
        {
            EmployeeChangeType type = EmployeeChangeType.ClosestRelativeHidden;
            if (sequence == 2)
                type = EmployeeChangeType.ClosestRelativeHidden2;
            return type;
        }

        private string GetClosestRelativeNr(List<EmployeeChangeRowIODTO> rows, int sequence)
        {
            return GetFirstValue(rows, GetClosestRelativeNrType(sequence));
        }

        private string GetClosestRelativeName(List<EmployeeChangeRowIODTO> rows, int sequence)
        {
            return GetFirstValue(rows, GetClosestRelativeNameType(sequence));
        }

        private string GetClosestRelativeRelation(List<EmployeeChangeRowIODTO> rows, int sequence)
        {
            return GetFirstValue(rows, GetClosestRelativeRelationType(sequence));
        }

        private bool? GetClosestRelativeHidden(List<EmployeeChangeRowIODTO> rows, int sequence)
        {
            return GetBool(rows, GetClosestRelativeHiddenType(sequence));
        }

        private string GetClosestRelativeDescription(string name, string relation)
        {
            return $"{name};{relation}";
        }

        private void GetClosestRelativeNameAndRelation(string description, out string name, out string relation)
        {
            string[] parts = description?.Split(';');
            name = parts?[0] ?? string.Empty;
            relation = parts?.Length > 1 ? parts[1] : string.Empty;
        }

        private void UpdateClosestRelativeNameAndRelation(ContactAddressItem item, EmployeeUserDTO employee, string description, int sequence)
        {
            if (item == null || employee == null)
                return;

            GetClosestRelativeNameAndRelation(item.EComDescription, out string existingName, out string existingRelation);
            GetClosestRelativeNameAndRelation(description, out string name, out string relation);

            if (string.IsNullOrEmpty(name))
                name = existingName;
            if (string.IsNullOrEmpty(relation))
                relation = existingRelation;

            bool isNameChanged = !existingName.Equals(name);
            bool isRelationChanged = !existingRelation.Equals(relation);

            if (isNameChanged)
                employee.AddCurrentChange((int)GetClosestRelativeNameType(sequence), existingName, name);
            if (isRelationChanged)
                employee.AddCurrentChange((int)GetClosestRelativeRelationType(sequence), existingRelation, relation);
            if (isNameChanged || isRelationChanged)
                item.EComDescription = GetClosestRelativeDescription(name, relation);
        }

        private void UpdateClosestRelativeSecret(ContactAddressItem item, EmployeeUserDTO employee, bool isSecret, int sequence)
        {
            if (item == null || employee == null)
                return;

            employee.AddCurrentChange((int)GetClosestRelativeHiddenType(sequence), item.IsSecret, isSecret);
            item.IsSecret = isSecret;
        }

        #endregion

        #endregion

        #region Employment

        #region Employment - Read

        private EmploymentDTO GetEmployment(EmployeeUserDTO employee, DateTime? date, bool checkOnlyOnDate = false, string optionalEmploymentExternalCode = null)
        {
            if (!string.IsNullOrEmpty(optionalEmploymentExternalCode))
            {
                var employments = employee.Employments.Where(w => w.State != SoeEntityState.Deleted && !string.IsNullOrEmpty(w.ExternalCode) && w.ExternalCode.Equals(optionalEmploymentExternalCode, StringComparison.OrdinalIgnoreCase)).ToList();
                if (employments.Any())
                {
                    if (employments.Count == 1)
                        return employments.First();
                    if (date.HasValue && date != CalendarUtility.DATETIME_DEFAULT)
                        return employments.GetEmployment(date, employeeGroups: this.lookup.EmployeeGroups, payrollGroups: this.lookup.Payrollgroups);
                    return employments.FirstOrDefault();
                }
            }

            EmploymentDTO employment = null;
            if (employee.Employments != null)
            {
                if (date.HasValue && date != CalendarUtility.DATETIME_DEFAULT && checkOnlyOnDate)
                    return employee.Employments.GetEmployment(date, employeeGroups: this.lookup.EmployeeGroups, payrollGroups: this.lookup.Payrollgroups);

                if (date != CalendarUtility.DATETIME_DEFAULT)
                    employment = employee.Employments.GetEmployment(date, employeeGroups: this.lookup.EmployeeGroups, payrollGroups: this.lookup.Payrollgroups);
                if (employment == null)
                    employment = employee.Employments.GetEmployment(employeeGroups: this.lookup.EmployeeGroups, payrollGroups: this.lookup.Payrollgroups);
                if (employment == null)
                    employment = employee.Employments.GetLastEmployment(employeeGroups: this.lookup.EmployeeGroups, payrollGroups: this.lookup.Payrollgroups);
                if (employment == null)
                    employment = employee.Employments.GetPrevEmployment(DateTime.Today.AddDays(100), employeeGroups: this.lookup.EmployeeGroups, payrollGroups: this.lookup.Payrollgroups);
            }

            return employment;
        }

        private bool HasEmployeeIdenticalEmployment(
            EmployeeUserDTO employee,
            DateTime? dateFrom,
            DateTime? dateTo = null,
            EmploymentTypeDTO employmentType = null,
            EmployeeGroupDTO employeeGroup = null,
            PayrollGroupDTO payrollGroup = null,
            VacationGroupDTO vacationGroup = null,
            int workTimeWeek = 0,
            decimal percent = 0,
            int baseWorkTimeWeek = 0,
            string employmentExternalCode = null,
            bool isTemporaryPrimary = false
            )
        {
            if (employee?.Employments == null)
                return false;

            return employee.Employments.Any(e =>
                e.State == (int)SoeEntityState.Active &&
                e.DateFrom == dateFrom &&
                e.DateTo == dateTo &&
                e.IsTemporaryPrimary == isTemporaryPrimary &&
                e.ExternalCode == employmentExternalCode &&
                e.EmploymentType == (employmentType?.GetEmploymentType() ?? 0) &&
                e.WorkTimeWeek == workTimeWeek &&
                e.Percent == percent &&
                e.BaseWorkTimeWeek == baseWorkTimeWeek &&
                e.EmployeeGroupId == (employeeGroup?.EmployeeGroupId ?? 0) &&
                e.PayrollGroupId == payrollGroup?.PayrollGroupId &&
                ((e.EmploymentVacationGroup.IsNullOrEmpty() && vacationGroup == null) || (vacationGroup != null && e.EmploymentVacationGroup?.Count == 1 && e.EmploymentVacationGroup.FirstOrDefault()?.VacationGroupId == vacationGroup.VacationGroupId))
                );
        }

        #endregion

        #region Employment - Creation

        private void CreateNewEmployments(EmployeeUserDTO employee)
        {
            if (employee.Employments == null)
                employee.Employments = new List<EmploymentDTO>();

            var row = GetFirstRow(EmployeeChangeType.NewEmployments);
            if (row != null && (row?.Value != null || !row.NewEmploymentRows.IsNullOrEmpty()))
            {
                var newEmploymentRows = row.NewEmploymentRows.IsNullOrEmpty() ? JsonConvert.DeserializeObject<List<NewEmploymentRowIO>>(row.Value) : row.NewEmploymentRows;
                if (!newEmploymentRows.IsNullOrEmpty())
                {
                    foreach (var newEmploymentRow in newEmploymentRows)
                    {
                        newEmploymentRow.DateFrom = newEmploymentRow.DateFrom.Date;
                        newEmploymentRow.DateTo = newEmploymentRow.DateTo?.Date;

                        var result = ParseTemporaryPrimary(employee, row, newEmploymentRow, out bool isTemporaryPrimary, out TimeDeviationCauseDTO hibernatingTimeDeviationCause);
                        if (!result.Success)
                            continue;

                        EmployeeGroupDTO employeeGroup = null;
                        if (!newEmploymentRow.EmployeeGroupCode.IsNullOrEmpty())
                        {
                            employeeGroup = GetEmployeeGroup(newEmploymentRow.EmployeeGroupCode);
                            if (employeeGroup == null)
                                AddValidationError(employee, row, newEmploymentRow.EmployeeGroupCode, EmploymentChangeValidationError.InvalidValueEmployeeGroupNotFound);
                        }

                        PayrollGroupDTO payrollGroup = null;
                        if (!string.IsNullOrEmpty(newEmploymentRow.PayrollGroupCode))
                        {
                            payrollGroup = GetPayrollGroup(newEmploymentRow.PayrollGroupCode);
                            if (payrollGroup == null)
                                AddValidationError(employee, row, newEmploymentRow.PayrollGroupCode, EmploymentChangeValidationError.InvalidValuePayrollGroupNotFound);
                        }

                        VacationGroupDTO vacationGroup = null;
                        if (!string.IsNullOrEmpty(newEmploymentRow.VacationGroupCode))
                        {
                            vacationGroup = GetVacationGroup(newEmploymentRow.VacationGroupCode);
                            if (vacationGroup == null)
                                AddValidationError(employee, row, newEmploymentRow.VacationGroupCode, EmploymentChangeValidationError.InvalidValueVacationGroupNotFound);
                        }

                        EmploymentTypeDTO employmentType = null;
                        if (!string.IsNullOrEmpty(newEmploymentRow.EmploymentTypeCode))
                        {
                            employmentType = GetEmploymentType(newEmploymentRow.EmploymentTypeCode);
                            if (employmentType == null)
                                AddValidationError(employee, row, newEmploymentRow.EmploymentTypeCode, EmploymentChangeValidationError.InvalidValueEmploymentTypeNotFound);
                        }

                        int workTimeMinutes = newEmploymentRow.WorkTimeWeek;
                        decimal employmentPercent = newEmploymentRow.EmploymentPercent;
                        if (workTimeMinutes > 0)
                            employmentPercent = GetEmploymentPercentFromWorkTimeWeek(employeeGroup, workTimeMinutes, newEmploymentRow.FullTimeWorkTimeWeek);
                        else if (employmentPercent > 0)
                            workTimeMinutes = GetWorkTimeWeekFromPercent(employeeGroup, employmentPercent, newEmploymentRow.FullTimeWorkTimeWeek);

                        if (employee.EmployeeId > 0 && HasEmployeeIdenticalEmployment(employee, newEmploymentRow.DateFrom, newEmploymentRow.DateTo, employmentType, employeeGroup, payrollGroup, vacationGroup, workTimeMinutes, employmentPercent, newEmploymentRow.BaseWorkTimeWeek, newEmploymentRow.ExternalCode, isTemporaryPrimary))
                        {
                            //Do not log this (ica special request)
                            continue;
                        }

                        var employment = CreateEmployment(
                            employee,
                            newEmploymentRow.DateFrom,
                            newEmploymentRow.DateTo,
                            employmentType,
                            employeeGroup,
                            payrollGroup,
                            vacationGroup,
                            hibernatingTimeDeviationCause,
                            workTimeMinutes,
                            employmentPercent,
                            newEmploymentRow.BaseWorkTimeWeek,
                            newEmploymentRow.ExternalCode,
                            isTemporaryPrimary,
                            excludeFromWorkTimeWeekCalculationOnSecondaryEmployment: newEmploymentRow.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment
                            );

                        if (employment != null)
                        {
                            AddEmploymentWorkTasks(employment, newEmploymentRow.WorkTasks);
                            AddEmploymentWorkPlace(employment, newEmploymentRow.WorkPlace);
                            AddEmploymentSpecialConditions(employment, newEmploymentRow.SpecialConditions);
                            AddEmploymentSubstituteFor(employment, newEmploymentRow.SubstituteFor);
                            AddEmploymentSubstituteForDueTo(employment, newEmploymentRow.SubstituteForDueTo);
                            AddEmploymentExperienceMonths(employment, newEmploymentRow.ExperienceMonths);
                            AddEmploymentExperienceAgreedOrEstablished(employment, newEmploymentRow.ExperienceAgreedOrEstablished);
                            AddEmploymentIsSecondaryEmployment(employment, newEmploymentRow.IsSecondaryEmployment);
                            AddEmploymentFullTimeWorkTimeWeek(employment, newEmploymentRow.FullTimeWorkTimeWeek, employmentPercent, null, null);
                            employment.Comment = newEmploymentRow.Comment;

                            if (newEmploymentRow.ToDateOnPreviousEmploymentIfExists.HasValue && newEmploymentRow.ToDateOnPreviousEmploymentIfExists.Value < newEmploymentRow.DateFrom)
                            {
                                EmploymentDTO previousEmployment = GetEmployment(employee, newEmploymentRow.ToDateOnPreviousEmploymentIfExists.Value, true, newEmploymentRow.OptionalEmploymentExternalCodeOnPreviousEmployment);
                                if (previousEmployment != null && !previousEmployment.DateTo.HasValue)
                                    previousEmployment.DateTo = newEmploymentRow.ToDateOnPreviousEmploymentIfExists.Value;
                            }
                        }
                    }
                }
            }
        }

        private EmploymentDTO CreateEmployment(
            EmployeeUserDTO employee,
            DateTime? dateFrom,
            DateTime? dateTo = null,
            EmploymentTypeDTO employmentType = null,
            EmployeeGroupDTO employeeGroup = null,
            PayrollGroupDTO payrollGroup = null,
            VacationGroupDTO vacationGroup = null,
            TimeDeviationCauseDTO hibernatingTimeDeviationCause = null,
            int workTimeWeek = 0,
            decimal employmentPercent = 0,
            int baseWorkTimeWeek = 0,
            string employmentExternalCode = null,
            bool isTemporaryPrimary = false,
            bool isSecondaryEmployment = false,
            bool isExperienceAgreedOrEstablished = false,
            int experienceMonths = 0,
            string specialConditions = null,
            string substituteFor = null,
            string substituteForDueTo = null,
            string workPlace = null,
            string workTasks = null,
            bool? excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = null,
            AnnualLeaveGroupDTO annualLeaveGroup = null
            )
        {
            if (employee?.Employments == null)
                return null;

            dateFrom = dateFrom?.Date ?? CalendarUtility.DATETIME_DEFAULT;
            dateTo = dateTo?.Date;

            if (!isTemporaryPrimary)
                hibernatingTimeDeviationCause = null;

            EmploymentDTO employment = new EmploymentDTO()
            {
                ActorCompanyId = this.lookup.ActorCompanyId,
                DateFrom = dateFrom.Value,
                DateTo = dateTo,
                IsTemporaryPrimary = isTemporaryPrimary,
                HibernatingTimeDeviationCauseId = hibernatingTimeDeviationCause?.TimeDeviationCauseId,
                EmploymentVacationGroup = new List<EmploymentVacationGroupDTO>(),
            };

            string toValueName = null;
            if (employment.IsTemporaryPrimary && hibernatingTimeDeviationCause != null)
                toValueName = $"Tillfälligt aktiv ({$"{hibernatingTimeDeviationCause.ExtCode} {hibernatingTimeDeviationCause.Name}"})";
            else if (employment.IsTemporaryPrimary)
                toValueName = $"Tillfälligt aktiv";

            employee.AddCurrentChange((int)EmployeeChangeType.NewEmployment, null, null, toValueName: toValueName, fromDate: employment.DateFrom, toDate: employment.DateTo);

            AddEmployeeGroup(employment, employeeGroup, dateFrom, dateTo);
            AddPayrollGroup(employment, payrollGroup, dateFrom, dateTo);
            AddAnnualLeaveGroup(employment, annualLeaveGroup, dateFrom, dateTo);

            if (vacationGroup != null || !HasAnyRow(EmployeeChangeType.VacationGroup))
                AddVacationGroup(employee, employment, vacationGroup, dateFrom, payrollGroup);
            if (employmentType != null)
                AddEmploymentType(employment, employmentType, dateFrom, dateTo);
            if (employee.BlockedFromDate.HasValue && (!dateTo.HasValue || dateTo.Value >= employee.BlockedFromDate))
                ClearBlockedFromDate(employee);

            if (workTimeWeek > 0)
                AddEmploymentWorkTimeWeek(employment, workTimeWeek, employmentPercent, dateFrom, dateTo);
            else if (employmentPercent > 0)
                AddEmploymentPercent(employment, employmentPercent, workTimeWeek, dateFrom, dateTo);

            AddEmploymentExternalCode(employment, employmentExternalCode, dateFrom, dateTo);
            AddEmploymentIsSecondaryEmployment(employment, isSecondaryEmployment, dateFrom, dateTo);
            AddEmploymentEmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(employment, excludeFromWorkTimeWeekCalculationOnSecondaryEmployment, dateFrom, dateTo);
            AddEmploymentBaseWorkTimeWeek(employment, baseWorkTimeWeek, dateFrom, dateTo);
            AddEmploymentExperienceAgreedOrEstablished(employment, isExperienceAgreedOrEstablished, dateFrom, dateTo);
            AddEmploymentExperienceMonths(employment, experienceMonths, dateFrom, dateTo);
            AddEmploymentSpecialConditions(employment, specialConditions, dateFrom, dateTo);
            AddEmploymentSubstituteFor(employment, substituteFor, dateFrom, dateTo);
            AddEmploymentSubstituteForDueTo(employment, substituteForDueTo, dateFrom, dateTo);
            AddEmploymentWorkPlace(employment, workPlace, dateFrom, dateTo);
            AddEmploymentWorkTasks(employment, workTasks, dateFrom, dateTo);

            employee.Employments.Add(employment);

            return employment;
        }

        #endregion

        #region Employment - StartDate/StopDate

        private void SetEmploymentStartAndStopDate(EmployeeUserDTO employee, EmploymentDTO employment, UserRolesDTO userRoles)
        {
            if (GetNrOfRows(EmployeeChangeType.EmploymentStopDateChange) > 1 && (TrySetEmploymentStartAndStopDateMultiple(employee, employment, userRoles, out int nrOfAffectedRows) || nrOfAffectedRows > 0))
                return;

            bool hasSetStopDateChange = false;
            if (TryGetRow(EmployeeChangeType.EmploymentStopDateChange, out EmployeeChangeRowIODTO stopDateChangeRow))
                hasSetStopDateChange = SetEmploymentStopDateChange(stopDateChangeRow, employee, userRoles, doKeepSettings: DoKeepScheduleHierarchicalAccountAndAttestRole());

            if (TryGetRow(EmployeeChangeType.EmploymentStartDateChange, out EmployeeChangeRowIODTO startDateChangeRow))
            {
                if (GetRows().DoCreateNewEmployment(employee.Employments, employment, out DateTime? newEmploymentFromDate, out DateTime? newEmploymentToDate))
                {
                    EmploymentDTO newEmployment = CreateEmployment(employee, newEmploymentFromDate?.Date, newEmploymentToDate);
                    if (newEmployment != null && !newEmployment.DateTo.HasValue && !hasSetStopDateChange && stopDateChangeRow != null)
                        SetEmploymentStopDateChange(stopDateChangeRow, employee, userRoles);
                }

                SetEmploymentStartDateChange(startDateChangeRow, employee);
            }
        }

        private bool TrySetEmploymentStartAndStopDateMultiple(EmployeeUserDTO employee, EmploymentDTO employment, UserRolesDTO userRoles, out int nrOfAffectedRows)
        {
            nrOfAffectedRows = 0;

            var rows = GetRows(EmployeeChangeType.EmploymentStartDateChange, EmployeeChangeType.EmploymentStopDateChange).SortByValueAsDate();
            if (rows.IsNullOrEmpty() || rows.ContainsAnyInvalidStartDateChange() || !rows.IsAlternatingBetweenTypes(EmployeeChangeType.EmploymentStartDateChange, EmployeeChangeType.EmploymentStopDateChange))
                return false;

            bool doKeepSettings = DoKeepScheduleHierarchicalAccountAndAttestRole();

            foreach (var row in rows)
            {
                bool isHandled = HandleRow(employee, employment, userRoles, row, doKeepSettings);
                if (isHandled)
                    nrOfAffectedRows++;
                else if (!row.ValidationErrors.IsNullOrEmpty())
                    return false;
            }

            return true;
        }

        private bool HandleRow(EmployeeUserDTO employee, EmploymentDTO employment, UserRolesDTO userRoles, EmployeeChangeRowIODTO row, bool doKeepSettings)
        {
            if (row.EmployeeChangeType == EmployeeChangeType.EmploymentStopDateChange)
                return SetEmploymentStopDateChange(row, employee, userRoles, doKeepSettings: doKeepSettings);
            else if (row.EmployeeChangeType == EmployeeChangeType.EmploymentStartDateChange)
                return HandleStartDateChange(employee, employment, row);

            return false;
        }

        private bool HandleStartDateChange(EmployeeUserDTO employee, EmploymentDTO employment, EmployeeChangeRowIODTO row)
        {
            if (row.ObjToList().DoCreateNewEmployment(employee.Employments, employment, out DateTime? newEmploymentFromDate, out DateTime? newEmploymentToDate))
                return CreateEmployment(employee, newEmploymentFromDate?.Date, newEmploymentToDate) != null;
            else
                return SetEmploymentStartDateChange(row, employee);
        }

        private bool SetEmploymentStartDateChange(EmployeeChangeRowIODTO row, EmployeeUserDTO employee)
        {
            if (employee == null || row == null)
                return false;

            if (!row.TryParseValueAsDate(out DateTime? newStartDateNullable, acceptNullOrEmpty: false) || !newStartDateNullable.HasValue)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentStartDateCannotBeParsed);
                return false;
            }

            DateTime newStartDate = newStartDateNullable.Value.Date;
            DateTime? date = ParseOptionalEmploymentDate(row, newStartDate);

            EmploymentDTO employment = GetEmployment(employee, date, true, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentCouldNotBeCreatedOrUpdated);
                return false;
            }
            if (employment.DateFrom == newStartDate)
                return false;

            if (row.FromDate.HasValue && row.FromDate.Value > DateTime.Now.AddYears(-200) && row.FromDate.Value.Date > newStartDate)
            {
                LogCollector.LogCollector.LogError($"API: SetEmploymentStartDateChange uses FromDate. ActorCompanyId:{this.lookup.ActorCompanyId}");
                newStartDate = row.FromDate.Value.Date;
            }

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.DateFrom, employment.DateFrom, newStartDate, employment.DateFrom?.ToShortDateString(), newStartDate.ToShortDateString());
            employment.DateFrom = newStartDate;
            return true;
        }

        private bool SetEmploymentStopDateChange(EmployeeChangeRowIODTO row, EmployeeUserDTO employee, UserRolesDTO userRole, bool doKeepSettings = false)
        {
            if (employee == null || row == null)
                return false;

            if (!row.TryParseValueAsDate(out DateTime? newStopDate, acceptNullOrEmpty: true))
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentStopDateCannotBeParsed);
                return false;
            }

            DateTime? date = ParseOptionalEmploymentDate(row, newStopDate);
            if (!date.HasValue && !newStopDate.HasValue)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentIfEmploymentDontHaveStopDateOptionaEmploymentdateMustContainDate);
                return false;
            }

            EmploymentDTO employment = GetEmployment(employee, date, checkOnlyOnDate: true, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
                return false;

            if (!newStopDate.HasValue && employment.IsTemporaryPrimary)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentTemporaryPrimaryMustHaveStopDate);
                return false;
            }
            if (!newStopDate.HasValue && employee.Employments.Any(i => i.State != SoeEntityState.Deleted && !i.DateTo.HasValue && i.EmploymentId != employment.EmploymentId))
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentEmpoyeeCanHaveOnlyOneEmploymentWithoutStopDate);
                return false;
            }
            if (!IsValidEmploymentStopDateChange(employment, newStopDate))
                return false;

            if (employee.Employments.Any(e => (!e.DateTo.HasValue || e.DateTo.Value > newStopDate) && e.EmploymentId != employment.EmploymentId && e.State == (int)SoeEntityState.Active))
                doKeepSettings = true;
            if (!doKeepSettings && DoCloseScheduleOnEmploymentStopDateChange(employment, newStopDate, out DateTime? clearScheduleFrom))
                employee.ClearScheduleFrom = clearScheduleFrom;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.DateTo, employment.DateTo, newStopDate, employment.DateTo?.ToShortDateString(), newStopDate?.ToShortDateString(), toValueName: doKeepSettings ? GetTermValue(43) : null);
            employment.DateTo = newStopDate;

            if (!doKeepSettings && GetSettingAutoCloseEmployeeAccountAndAttestRole(employee, employment))
            {
                CloseEmployeeAccounts(employee, employment);
                CloseAttestRoles(employee, employment, userRole);
            }

            return true;
        }

        private void CloseEmployeeAccounts(EmployeeUserDTO employee, EmploymentDTO employment)
        {
            if (employee == null || employee.Accounts.IsNullOrEmpty())
                return;

            foreach (var employeeAccount in employee.Accounts.FilterOnDate(employment.DateTo.Value))
            {
                List<AccountDTO> validAccounts = GetAccountsForHierchalAccount(employee);
                UpdateEmployeeAccountDateTo(employee, employeeAccount, validAccounts?.FirstOrDefault(i => i.AccountId == employeeAccount.AccountId), employment.DateTo);

                foreach (var childAccount in employeeAccount.GetChildrens(employment.DateTo.Value))
                {
                    childAccount.DateTo = employment.DateTo;
                    foreach (var grandChildAccount in childAccount.GetChildrens(employment.DateTo.Value))
                    {
                        grandChildAccount.DateTo = employment.DateTo;
                    }
                }
            }
        }

        private void CloseAttestRoles(EmployeeUserDTO employee, EmploymentDTO employment, UserRolesDTO userRole)
        {
            if (employee == null || employment == null || userRole == null || userRole.AttestRoles.IsNullOrEmpty() || HasAnyStartDateChangeAfterDate(employment.DateTo.Value))
                return;

            foreach (UserAttestRoleDTO userAttestRole in userRole.AttestRoles.Where(attestRole => !attestRole.DateTo.HasValue || attestRole.DateTo.Value > employment.DateTo.Value))
            {
                UpdateAttestRoleDateTo(employee, userAttestRole, GetAttestRole(userAttestRole.AttestRoleId), employment.DateTo);
            }
        }

        private void SetClearScheduleFrom(EmployeeUserDTO employee)
        {
            if (TryGetRow(EmployeeChangeType.ClearScheduleFrom, out EmployeeChangeRowIODTO clearScheduleFromRow) && CalendarUtility.GetNullableDateTime(clearScheduleFromRow.Value).HasValue && !employee.ClearScheduleFrom.HasValue)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.ClearScheduleFrom, "", clearScheduleFromRow.Value, toValueName: GetTermValue(47));
                employee.ClearScheduleFrom = CalendarUtility.GetNullableDateTime(clearScheduleFromRow.Value);
            }
        }

        private bool IsValidEmploymentStopDateChange(EmploymentDTO employment, DateTime? newStopDate)
        {
            if (employment == null)
                return false;
            if (newStopDate.HasValue && employment.DateFrom.HasValue && newStopDate.Value < employment.DateFrom)
                return false;
            return employment.DateTo != newStopDate;
        }

        private bool DoCloseScheduleOnEmploymentStopDateChange(EmploymentDTO employment, DateTime? newStopDate, out DateTime? clearScheduleFrom)
        {
            clearScheduleFrom = null;
            bool doClose = false;
            if (employment != null && employment.EmploymentId > 0 && newStopDate.HasValue)
            {
                if (!employment.DateTo.HasValue)
                    doClose = true;
                if (employment.DateTo.HasValue && employment.DateTo.Value > newStopDate.Value)
                    doClose = true;
                if (doClose)
                    clearScheduleFrom = newStopDate.Value.Date.AddDays(1);
            }
            return doClose;
        }

        private bool DoKeepScheduleHierarchicalAccountAndAttestRole()
        {
            var doKeepSettingsRow = GetLastRow(EmployeeChangeType.EmploymentStopDateKeepScheduleHierarchicalAccountAndAttestRole);
            return doKeepSettingsRow != null && StringUtility.GetBool(doKeepSettingsRow.Value);
        }

        private bool HasAnyStartDateChangeAfterDate(DateTime dateTo)
        {
            List<EmployeeChangeRowIODTO> rows = GetRows(EmployeeChangeType.EmploymentStartDateChange);
            foreach (EmployeeChangeRowIODTO row in rows)
            {
                if (!DateTime.TryParse(row.Value, out DateTime date))
                    continue;

                if (date > dateTo)
                    return true;
            }

            return false;
        }

        #endregion

        #region Employment - Accounting

        private void SetEmploymentAccounts(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                DateTime? date = ParseOptionalEmploymentDate(row, row.FromDate ?? DateTime.Today);

                EmploymentDTO employment = GetEmployment(employee, date, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
                if (employment == null)
                {
                    AddValidationError(employee, row, date.ToString(), EmploymentChangeValidationError.InvalidEmploymentNotFound);
                    continue;
                }

                if (string.IsNullOrEmpty(row.OptionalExternalCode))
                {
                    AddValidationError(employee, row, row.OptionalExternalCode, EmploymentChangeValidationError.OptionalExternalCodeMissing);
                    continue;
                }

                int.TryParse(row.OptionalExternalCode, out int sieDimNr);
                if (sieDimNr == 0)
                {
                    AddValidationError(employee, row, row.OptionalExternalCode, EmploymentChangeValidationError.InvalidOptionalExternalCodeAccountCannotBeParsed);
                    continue;
                }

                AccountDTO account = GetAccountBySieDim(row, sieDimNr);
                if (account == null)
                {
                    AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidOptionalExternalCodeAccountNotFound);
                    continue;
                }

                AccountingSettingsRowDTO accountSetting = employment.AccountingSettings?.FirstOrDefault(a => a.Type == (int)EmploymentAccountType.Cost);
                if (accountSetting == null)
                {
                    accountSetting = new AccountingSettingsRowDTO
                    {
                        Type = (int)EmploymentAccountType.Cost
                    };
                }

                SetEmploymentAccounts(employee, employment, accountSetting, account, row.Delete);
            }
        }

        private void SetEmploymentAccounts(EmployeeUserDTO employee, EmploymentDTO employment, AccountingSettingsRowDTO accountSetting, AccountDTO account, bool delete)
        {
            if (employee == null || employment == null || accountSetting == null || account == null)
                return;

            string prevAccountNr = "";

            int dimNr = GetAccountDimNrFromAccountSetting(account.AccountDimId, accountSetting);
            if (dimNr == 2)
            {
                prevAccountNr = accountSetting.Account2Nr.NullToEmpty();
                accountSetting.Account2Id = delete ? 0 : account.AccountId;
                accountSetting.Account2Nr = account.AccountNr;
                accountSetting.Account2Name = account.Name;
            }
            else if (dimNr == 3)
            {
                prevAccountNr = accountSetting.Account3Nr.NullToEmpty();
                accountSetting.Account3Id = delete ? 0 : account.AccountId;
                accountSetting.Account3Nr = account.AccountNr;
                accountSetting.Account3Name = account.Name;
            }
            else if (dimNr == 4)
            {
                prevAccountNr = accountSetting.Account4Nr.NullToEmpty();
                accountSetting.Account4Id = delete ? 0 : account.AccountId;
                accountSetting.Account4Nr = account.AccountNr;
                accountSetting.Account4Name = account.Name;
            }
            else if (dimNr == 5)
            {
                prevAccountNr = accountSetting.Account5Nr.NullToEmpty();
                accountSetting.Account5Id = delete ? 0 : account.AccountId;
                accountSetting.Account5Nr = account.AccountNr;
                accountSetting.Account5Name = account.Name;
            }
            else if (dimNr == 6)
            {
                prevAccountNr = accountSetting.Account6Nr.NullToEmpty();
                accountSetting.Account6Id = delete ? 0 : account.AccountId;
                accountSetting.Account6Nr = account.AccountNr;
                accountSetting.Account6Name = account.Name;
            }

            if (prevAccountNr != account.AccountNr)
                employee.AddCurrentChange((int)EmployeeChangeType.AccountNrSieDim, prevAccountNr, (delete ? string.Empty : account.AccountNr), fromDate: employment.DateFrom, toDate: employment.DateTo);

            if (employment.AccountingSettings == null)
                employment.AccountingSettings = new List<AccountingSettingsRowDTO>();
            if (!employment.AccountingSettings.Contains(accountSetting))
                employment.AccountingSettings.Add(accountSetting);
        }

        private void CopyEmploymentAccounts(EmploymentDTO employment, AccountingSettingsRowDTO accountingSetting)
        {
            if (employment == null || accountingSetting == null)
                return;

            if (employment.AccountingSettings == null)
                employment.AccountingSettings = new List<AccountingSettingsRowDTO>();
            employment.AccountingSettings.Add(accountingSetting);
        }

        #endregion

        #region Employment - EmployeeGroup

        private void SetEmployeeGroup(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EmployeeGroupDTO employeeGroup = GetEmployeeGroup(row);
            if (employeeGroup == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueEmployeeGroupNotFound);
                return;
            }

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, employeeGroup: employeeGroup);
            }
            else
            {
                if (employment.EmployeeGroupId == 0)
                    AddEmployeeGroup(employment, employeeGroup, fromDate, toDate);
                else
                    UpdateEmployeeGroup(employment, employeeGroup, fromDate, toDate);
            }
        }

        private EmployeeGroupDTO GetEmployeeGroup(EmployeeChangeRowIODTO row)
        {
            return row != null ? GetEmployeeGroup(row.Value) : null;
        }

        private EmployeeGroupDTO GetEmployeeGroup(string value)
        {
            return
                this.lookup.EmployeeGroups?.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase)) ??
                this.lookup.EmployeeGroups?.FirstOrDefault(f => f.ExternalCodes != null && f.ExternalCodes.Where(w => !string.IsNullOrEmpty(w)).Select(s => s.Trim().ToLower()).Contains(value.Trim().ToLower()));
        }

        private EmployeeGroupDTO GetEmployeeGroup(EmploymentDTO employment, DateTime date)
        {
            EmployeeGroupDTO employeeGroup = null;

            int employeeGroupId = employment?.GetCurrentChangeValue(TermGroup_EmploymentChangeFieldType.EmployeeGroupId, date, employment.EmployeeGroupId) ?? 0;
            if (employeeGroupId > 0)
                employeeGroup = this.lookup.EmployeeGroups?.FirstOrDefault(i => i.EmployeeGroupId == employeeGroupId);
            if (employeeGroup == null)
                employeeGroup = this.lookup.DefaultEmployeeGroup;

            return employeeGroup;
        }

        private void AddEmployeeGroup(EmploymentDTO employment, EmployeeGroupDTO employeeGroup, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null)
                return;

            if (employeeGroup == null)
                employeeGroup = this.lookup.DefaultEmployeeGroup;
            if (employeeGroup == null)
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.EmployeeGroupId, fromDate, toDate, null, employeeGroup.EmployeeGroupId.ToString(), toValueName: employeeGroup.Name);
            employment.EmployeeGroupId = employeeGroup.EmployeeGroupId;
        }

        private void UpdateEmployeeGroup(EmploymentDTO employment, EmployeeGroupDTO employeeGroup, DateTime? fromDate, DateTime? toDate)
        {
            if (employeeGroup == null || IsEqual(employment, TermGroup_EmploymentChangeFieldType.EmployeeGroupId, employment.EmployeeGroupId.ToString(), employeeGroup.EmployeeGroupId.ToString(), fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.EmployeeGroupId, fromDate, toDate, employment.EmployeeGroupId.ToString(), employeeGroup.EmployeeGroupId.ToString(), this.lookup.EmployeeGroups?.FirstOrDefault(i => i.EmployeeGroupId == employment.EmployeeGroupId)?.Name, employeeGroup.Name);
            if (DoSetValueOnEmployment(employment, fromDate))
                employment.EmployeeGroupId = employeeGroup.EmployeeGroupId;
        }

        #endregion

        #region Employment - EndReason

        private void SetEmploymentEndReason(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EndReasonDTO employmentEndReason = GetEmploymentEndReason(row);
            if (employmentEndReason == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueEndReasonNotFound);
                return;
            }

            DateTime? date = ParseOptionalEmploymentDate(row, null);

            EmploymentDTO employment = date.HasValue ? GetEmployment(employee, date, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode) : null;
            if (employment == null || !employment.DateTo.HasValue)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentNotFoundEndReasonNotSet);
                return;
            }

            UpdateEmploymentEndReason(employment, employmentEndReason, employment.DateFrom, employment.DateTo);
        }

        private EndReasonDTO GetEmploymentEndReason(EmployeeChangeRowIODTO row)
        {
            if (row?.Value == null)
                return null;

            return GetEmploymentEndReason(row.Value);
        }

        private EndReasonDTO GetEmploymentEndReason(string value)
        {
            var employmentType = this.lookup.EmploymentEndReasons?.FirstOrDefault(f => f.Code != null && f.Code.Equals(value, StringComparison.OrdinalIgnoreCase) && !f.SystemEndReson);
            if (employmentType == null)
                employmentType = this.lookup.EmploymentEndReasons?.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase) && !f.SystemEndReson);
            if (employmentType == null)
                employmentType = this.lookup.EmploymentEndReasons?.FirstOrDefault(f => f.EndReasonId.ToString().Equals(value, StringComparison.OrdinalIgnoreCase) && f.SystemEndReson);

            return employmentType;
        }

        private void UpdateEmploymentEndReason(EmploymentDTO employment, EndReasonDTO endReason, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || endReason == null)
                return;

            if (employment.EmploymentEndReason != endReason.EndReasonId)
                employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.EmploymentEndReason, fromDate, toDate, employment.EmploymentEndReason.ToString(), endReason.EndReasonId.ToString(), employment.EmploymentEndReasonName, endReason.Name);
        }

        #endregion

        #region Employment - ExternalCode

        private void SetEmploymentExternalCode(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                SetEmploymentExternalCode(employee, row);
            }
        }

        private void SetEmploymentExternalCode(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            string externalCode = row.Value;

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, employmentExternalCode: externalCode);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentExternalCode(employment, externalCode, fromDate, toDate);
                else
                    UpdateEmploymentExternalCode(employment, externalCode, fromDate, toDate);
            }
        }

        private void AddEmploymentExternalCode(EmploymentDTO employment, string externalCode, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null || externalCode.IsNullOrEmpty())
                return;

            employment.ExternalCode = externalCode;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExternalCode, fromDate, toDate, null, employment.ExternalCode);
        }

        private void UpdateEmploymentExternalCode(EmploymentDTO employment, string externalCode, DateTime? fromDate, DateTime? toDate)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.ExternalCode, employment.ExternalCode.NullToEmpty(), externalCode, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExternalCode, fromDate, toDate, employment.ExternalCode, externalCode);
        }

        #endregion

        #region Employment - Experience

        private void SetEmploymentExperienceMonths(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;
            if (!int.TryParse(row.Value, out int experienceMonths))
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, experienceMonths: experienceMonths);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentExperienceMonths(employment, experienceMonths, fromDate, toDate);
                else
                    UpdateEmploymentExperienceMonths(employment, experienceMonths, fromDate, toDate);
            }
        }

        private void SetEmploymentExperienceAgreedOrEstablished(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;
            if (!StringUtility.IsValidBool(row.Value))
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            bool isExperienceAgreedOrEstablished = StringUtility.GetBool(row.Value);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, isExperienceAgreedOrEstablished: isExperienceAgreedOrEstablished);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentExperienceAgreedOrEstablished(employment, isExperienceAgreedOrEstablished, fromDate, toDate);
                else
                    UpdateEmploymentExperienceAgreedOrEstablished(employment, isExperienceAgreedOrEstablished, fromDate, toDate);
            }
        }

        private void AddEmploymentExperienceMonths(EmploymentDTO employment, int experienceMonths, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || experienceMonths == 0)
                return;

            employment.ExperienceMonths = experienceMonths;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExperienceMonths, fromDate, toDate, null, employment.ExperienceMonths.ToString());
        }

        private void UpdateEmploymentExperienceMonths(EmploymentDTO employment, int experienceMonths, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.ExperienceMonths, employment.ExperienceMonths, experienceMonths, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExternalCode, fromDate, toDate, employment.ExperienceMonths.ToString(), experienceMonths.ToString());
        }

        private void AddEmploymentExperienceAgreedOrEstablished(EmploymentDTO employment, bool isExperienceAgreedOrEstablished, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || !isExperienceAgreedOrEstablished)
                return;

            employment.ExperienceAgreedOrEstablished = isExperienceAgreedOrEstablished;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExperienceAgreedOrEstablished, fromDate, toDate, null, employment.ExperienceAgreedOrEstablished.ToString());
        }

        private void UpdateEmploymentExperienceAgreedOrEstablished(EmploymentDTO employment, bool isExperienceAgreedOrEstablished, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.ExperienceAgreedOrEstablished, employment.ExperienceAgreedOrEstablished.ToInt(), isExperienceAgreedOrEstablished.ToInt(), fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExternalCode, fromDate, toDate, employment.ExperienceAgreedOrEstablished.ToInt().ToString(), isExperienceAgreedOrEstablished.ToInt().ToString());
        }

        #endregion

        #region Employment - Percent/WorkTimeWeek

        private void SetEmploymentWorkTimeWeekMinutes(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                if (!TryParseWorkTimeMinutes(row, out int workTimeMinutes))
                    continue;

                DateTime fromDate = ParseFromDate(row);
                DateTime? toDate = ParseToDate(row);

                EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);

                int? fulltimeWorkTimeWeekEmployee = employment?.GetLastValidChangeValue(TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek, fromDate, toDate, employment.FullTimeWorkTimeWeek);
                decimal employmentPercent = GetEmploymentPercentFromWorkTimeWeek(employment, fromDate, workTimeMinutes, fulltimeWorkTimeWeekEmployee);

                if (employment == null)
                {
                    CreateEmployment(employee, fromDate, toDate, workTimeWeek: workTimeMinutes, employmentPercent: employmentPercent);
                }
                else
                {
                    if (DoSetValueOnEmployment(employment, fromDate))
                        AddEmploymentWorkTimeWeek(employment, workTimeMinutes, employmentPercent, fromDate, toDate);
                    else
                        UpdateEmploymentWorkTimeWeek(employment, workTimeMinutes, employmentPercent, fromDate, toDate);
                }
            }
        }

        private void SetEmploymentPercent(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                decimal employmentPercent = ParseEmploymentPercent(row);
                if (employmentPercent == 0)
                    return;

                DateTime fromDate = ParseFromDate(row);
                DateTime? toDate = ParseToDate(row);

                EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
                int workTimeMinutes = GetWorkTimeWeekFromPercent(employment, fromDate, employmentPercent);

                if (employment == null)
                {
                    CreateEmployment(employee, fromDate, toDate, workTimeWeek: workTimeMinutes, employmentPercent: employmentPercent);
                }
                else
                {
                    if (DoSetValueOnEmployment(employment, fromDate))
                        AddEmploymentPercent(employment, employmentPercent, workTimeMinutes, fromDate, toDate);
                    else
                        UpdateEmploymentPercent(employment, employmentPercent, workTimeMinutes, fromDate, toDate);
                }
            }
        }

        private void SetEmploymentFullTimeWorkTimeWeekMinutes(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                if (!TryParseFullTimeWorkTimeMinutes(row, out int fullTimeWorkTimeWeekMinutes))
                    continue;

                DateTime fromDate = ParseFromDate(row);
                DateTime? toDate = ParseToDate(row);

                EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
                decimal employmentPercent = HasAnyRow(EmployeeChangeType.WorkTimeWeekMinutes) ? employment.Percent : GetEmploymentPercentFromWorkTimeWeek(employment, fromDate, employment?.WorkTimeWeek ?? 0, fullTimeWorkTimeWeekMinutes);

                if (employment != null)
                {
                    if (DoSetValueOnEmployment(employment, fromDate))
                        AddEmploymentFullTimeWorkTimeWeek(employment, fullTimeWorkTimeWeekMinutes, employmentPercent, fromDate, toDate);
                    else
                        UpdateEmploymentFullTimeWorkTimeWeek(employment, fullTimeWorkTimeWeekMinutes, employmentPercent, fromDate, toDate);
                }

            }
        }

        private void SetEmploymentBaseWorkTimeWeek(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                if (!TryParseWorkTimeMinutes(row, out int baseWorkTimeWeek))
                    continue;

                DateTime fromDate = ParseFromDate(row);
                DateTime? toDate = ParseToDate(row);

                EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
                if (employment == null)
                {
                    CreateEmployment(employee, fromDate, toDate, baseWorkTimeWeek: baseWorkTimeWeek);
                }
                else
                {
                    if (DoSetValueOnEmployment(employment, fromDate))
                        AddEmploymentBaseWorkTimeWeek(employment, baseWorkTimeWeek, fromDate, toDate);
                    else
                        UpdateEmploymentBaseWorkTimeWeek(employment, baseWorkTimeWeek, fromDate, toDate);
                }
            }
        }

        private int GetWorkTimeWeekFromPercent(EmployeeGroupDTO employeeGroup, decimal employmentPercent, int? fulltimeWorkTimeWeekEmployee)
        {
            return FormatWorkTimeWeekMinutes(GetFullTimeWorkTimeWeekFromPrio(employeeGroup, fulltimeWorkTimeWeekEmployee), employmentPercent);
        }

        private int GetWorkTimeWeekFromPercent(EmploymentDTO employment, DateTime date, decimal employmentPercent)
        {
            return FormatWorkTimeWeekMinutes(GetFullTimeWorkTimeWeekFromPrio(employment, date, employment?.FullTimeWorkTimeWeek), employmentPercent);
        }

        private decimal GetEmploymentPercentFromWorkTimeWeek(EmployeeGroupDTO employeeGroup, int workTimeWeek, int? fulltimeWorkTimeWeekEmployee)
        {
            return Employment.FormatEmploymentPercent(GetFullTimeWorkTimeWeekFromPrio(employeeGroup, fulltimeWorkTimeWeekEmployee), workTimeWeek);
        }

        private decimal GetEmploymentPercentFromWorkTimeWeek(EmploymentDTO employment, DateTime date, int workTimeWeek, int? fulltimeWorkTimeWeekEmployee)
        {
            return Employment.FormatEmploymentPercent(GetFullTimeWorkTimeWeekFromPrio(employment, date, fulltimeWorkTimeWeekEmployee), workTimeWeek);
        }

        private static int FormatWorkTimeWeekMinutes(int fulltimeWorkTimeWeekMinutes, decimal employmentPercent)
        {
            return fulltimeWorkTimeWeekMinutes == 0 || employmentPercent == 0 ? 0 : Convert.ToInt32(Decimal.Multiply(NumberUtility.DividePercentIfAboveOne(employmentPercent), Convert.ToDecimal(fulltimeWorkTimeWeekMinutes)));
        }

        private int GetFullTimeWorkTimeWeekFromPrio(EmployeeGroupDTO employeeGroup, int? fulltimeWorkTimeWeekEmployee)
        {
            return
                fulltimeWorkTimeWeekEmployee.ToNullable() ??
                employeeGroup?.RuleWorkTimeWeek ??
                this.lookup.DefaultEmployeeGroup?.RuleWorkTimeWeek ??
                0;
        }

        private int GetFullTimeWorkTimeWeekFromPrio(EmploymentDTO employment, DateTime date, int? fulltimeWorkTimeWeekEmployee)
        {
            return
                fulltimeWorkTimeWeekEmployee.ToNullable() ??
                GetEmployeeGroup(employment, date)?.RuleWorkTimeWeek ??
                this.lookup.DefaultEmployeeGroup?.RuleWorkTimeWeek ??
                0;
        }

        private decimal ParseEmploymentPercent(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return 0;

            decimal.TryParse(row.Value.ReplaceDecimalSeparator(), out decimal percent);
            return percent;
        }

        private bool TryParseWorkTimeMinutes(EmployeeChangeRowIODTO row, out int workTimeMinutes)
        {
            workTimeMinutes = 0;
            if (row == null)
                return false;

            int.TryParse(row.Value, out workTimeMinutes);
            if (workTimeMinutes == 0)
            {
                decimal.TryParse(row.Value.ReplaceDecimalSeparator(), out decimal decimalMinutes);
                if (decimalMinutes != 0)
                    workTimeMinutes = Convert.ToInt32(decimal.Round(decimalMinutes));
            }

            return true;
        }

        private bool TryParseFullTimeWorkTimeMinutes(EmployeeChangeRowIODTO row, out int fullTimeWorkTimeMinutes)
        {
            fullTimeWorkTimeMinutes = 0;
            if (row == null)
                return false;

            int.TryParse(row.Value, out fullTimeWorkTimeMinutes);
            if (fullTimeWorkTimeMinutes == 0)
            {
                decimal.TryParse(row.Value.ReplaceDecimalSeparator(), out decimal decimalMinutes);
                if (decimalMinutes != 0)
                    fullTimeWorkTimeMinutes = Convert.ToInt32(decimal.Round(decimalMinutes));
            }

            return true;
        }

        private void AddEmploymentWorkTimeWeek(EmploymentDTO employment, int workTimeMinutes, decimal employmentPercent, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null || (workTimeMinutes == 0 && employmentPercent == 0))
                return;

            employment.WorkTimeWeek = workTimeMinutes;
            employment.Percent = employment.Percent == 0 ? employmentPercent : employment.Percent;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, fromDate, toDate, null, employment.WorkTimeWeek.ToString());
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.Percent, fromDate, toDate, null, employment.Percent.ToString());
        }

        private void UpdateEmploymentWorkTimeWeek(EmploymentDTO employment, int workTimeMinutes, decimal employmentPercent, DateTime? fromDate, DateTime? toDate)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.WorkTimeWeek, employment.WorkTimeWeek, workTimeMinutes, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, fromDate, toDate, employment.WorkTimeWeek.ToString(), workTimeMinutes.ToString());
            TryAddEmploymentPercentChange(employment, employmentPercent, fromDate, toDate);
        }

        private void AddEmploymentFullTimeWorkTimeWeek(EmploymentDTO employment, int fullTimeWorkTimeWeekMinutes, decimal employmentPercent, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || fullTimeWorkTimeWeekMinutes == 0)
                return;

            employment.FullTimeWorkTimeWeek = fullTimeWorkTimeWeekMinutes;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek, fromDate, toDate, null, employment.WorkTimeWeek.ToString());
            TryAddEmploymentPercentChange(employment, employmentPercent, fromDate, toDate);
        }

        private void UpdateEmploymentFullTimeWorkTimeWeek(EmploymentDTO employment, int fullTimeWorkTimeWeekMinutes, decimal employmentPercent, DateTime? fromDate, DateTime? toDate)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek, employment.FullTimeWorkTimeWeek, fullTimeWorkTimeWeekMinutes, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek, fromDate, toDate, employment.WorkTimeWeek.ToString(), fullTimeWorkTimeWeekMinutes.ToString());
            TryAddEmploymentPercentChange(employment, employmentPercent, fromDate, toDate);
        }

        private static void TryAddEmploymentPercentChange(EmploymentDTO employment, decimal employmentPercent, DateTime? fromDate, DateTime? toDate)
        {
            if (employment.Percent.ToString() != employmentPercent.ToString())
                employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.Percent, fromDate, toDate, employment.Percent.ToString(), employmentPercent.ToString());
        }

        private void AddEmploymentPercent(EmploymentDTO employment, decimal percent, int workTimeMinutes, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null || (percent == 0 && workTimeMinutes == 0))
                return;

            employment.WorkTimeWeek = employment.WorkTimeWeek == 0 ? workTimeMinutes : employment.WorkTimeWeek;
            employment.Percent = percent;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.Percent, fromDate, toDate, null, employment.Percent.ToString());
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, fromDate, toDate, null, employment.WorkTimeWeek.ToString());
        }

        private void UpdateEmploymentPercent(EmploymentDTO employment, decimal percent, int workTimeMinutes, DateTime? fromDate, DateTime? toDate)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.Percent, employment.Percent, percent, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.Percent, fromDate, toDate, employment.Percent.ToString(), percent.ToString());
            if (!IsEqual(employment, TermGroup_EmploymentChangeFieldType.WorkTimeWeek, employment.WorkTimeWeek, workTimeMinutes, fromDate, toDate))
                employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkTimeWeek, fromDate, toDate, employment.WorkTimeWeek.ToString(), workTimeMinutes.ToString());
        }

        private void AddEmploymentBaseWorkTimeWeek(EmploymentDTO employment, int baseWorkTimeWeek, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null || baseWorkTimeWeek == 0)
                return;

            employment.BaseWorkTimeWeek = baseWorkTimeWeek;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek, fromDate, toDate, null, employment.BaseWorkTimeWeek.ToString());
        }

        private void UpdateEmploymentBaseWorkTimeWeek(EmploymentDTO employment, int baseWorkTimeWeek, DateTime? fromDate, DateTime? toDate)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek, employment.WorkTimeWeek, baseWorkTimeWeek, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek, fromDate, toDate, employment.BaseWorkTimeWeek.ToString(), baseWorkTimeWeek.ToString());
        }

        #endregion

        #region Employment - PayrollGroup

        private void SetPayrollGroup(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            PayrollGroupDTO payrollGroup = GetPayrollGroup(row);
            if (payrollGroup == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValuePayrollGroupNotFound);
                return;
            }

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, payrollGroup: payrollGroup);
            }
            else
            {
                if (employment.PayrollGroupId == 0)
                    AddPayrollGroup(employment, payrollGroup, fromDate, toDate);
                else
                    UpdatePayrollGroup(employment, payrollGroup, fromDate, toDate);
            }
        }

        private PayrollGroupDTO GetPayrollGroup(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return null;

            return GetPayrollGroup(row.Value);
        }

        private PayrollGroupDTO GetPayrollGroup(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var payrollGroup = this.lookup.Payrollgroups?.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (payrollGroup == null)
                payrollGroup = this.lookup.Payrollgroups?.FirstOrDefault(f => f.ExternalCodes != null && f.ExternalCodes.Where(w => !string.IsNullOrEmpty(w)).Select(s => s.Trim().ToLower()).Contains(value.Trim().ToLower()));

            return payrollGroup;
        }

        private void AddPayrollGroup(EmploymentDTO employment, PayrollGroupDTO payrollGroup, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null)
                return;

            if (payrollGroup == null)
                payrollGroup = this.lookup.DefaultPayrollGroup;
            if (payrollGroup == null)
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.PayrollGroupId, fromDate, toDate, null, payrollGroup.PayrollGroupId.ToString(), toValueName: payrollGroup.Name);
            employment.PayrollGroupId = payrollGroup.PayrollGroupId;
        }

        private void UpdatePayrollGroup(EmploymentDTO employment, PayrollGroupDTO payrollGroup, DateTime? fromDate, DateTime? toDate)
        {
            if (payrollGroup == null || IsEqual(employment, TermGroup_EmploymentChangeFieldType.PayrollGroupId, employment.PayrollGroupId.ToString(), payrollGroup.PayrollGroupId.ToString(), fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.PayrollGroupId, fromDate, toDate, employment.PayrollGroupId.ToString(), payrollGroup.PayrollGroupId.ToString(), this.lookup.Payrollgroups?.FirstOrDefault(i => i.PayrollGroupId == employment.PayrollGroupId)?.Name, payrollGroup.Name);
            if (DoSetValueOnEmployment(employment, fromDate))
                employment.PayrollGroupId = payrollGroup.PayrollGroupId;
        }

        #endregion

        #region Employment - PriceType

        private void SetEmploymentPriceType(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null || !row.FromDate.HasValue || row.FromDate.Value == CalendarUtility.DATETIME_DEFAULT)
                return;

            List<string> externalCodes = ParseExternalCode(row);
            string externalCodePriceType = externalCodes.Count > 0 ? externalCodes[0] : null;
            string externalCodePayrollLevel = externalCodes.Count > 0 ? externalCodes.Skip(1).FirstOrDefault() : null;

            PayrollPriceTypeDTO payrollPriceType = externalCodePriceType != null ? GetPayrollPriceType(externalCodePriceType) : null;
            if (payrollPriceType == null)
            {
                AddValidationError(employee, row, row.OptionalExternalCode, EmploymentChangeValidationError.InvalidValuePayrollPriceTypeNotFound);
                return;
            }

            PayrollLevelDTO payrollLevel = null;
            if (externalCodePayrollLevel != null)
            {
                payrollLevel = GetPayrollLevel(externalCodePayrollLevel);
                if (payrollLevel == null)
                {
                    AddValidationError(employee, row, externalCodePayrollLevel, EmploymentChangeValidationError.InvalidOptionalExternalCodePayrollLevelNotFound);
                    return;
                }
            }

            if (!decimal.TryParse(row.Value.ReplaceDecimalSeparator(), out decimal amount))
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValuePayrollPriceTypeCannotBeParsed);
                return;
            }

            EmploymentDTO employment = GetEmployment(employee, row.FromDate.Value, false, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode) ?? CreateEmployment(employee, ParseFromDate(row), ParseToDate(row));
            EmploymentPriceTypeDTO employmentPriceType = null;

            #region Update

            List<EmploymentPriceTypeDTO> employmentPriceTypes = employment.PriceTypes?.Where(a => a.PayrollPriceTypeId == payrollPriceType.PayrollPriceTypeId).ToList();
            if (!employmentPriceTypes.IsNullOrEmpty())
            {
                if (employmentPriceTypes.Any(a => a.Periods.Any(p => p.FromDate == row.FromDate.Value)))
                {
                    employmentPriceType = employmentPriceTypes.First(f => f.Periods.Any(p => p.FromDate == row.FromDate.Value));

                    EmploymentPriceTypePeriodDTO employmentPriceTypePeriod = employmentPriceType?.Periods?.FirstOrDefault(p => p.FromDate == row.FromDate.Value);
                    if (employmentPriceTypePeriod == null)
                        AddEmploymentPriceTypePeriod(employee, employmentPriceType, payrollPriceType, payrollLevel, row.FromDate.Value.Date, amount);
                    else
                        UpdateEmploymentPriceTypePeriod(employee, employmentPriceTypePeriod, payrollPriceType, payrollLevel, row.FromDate.Value.Date, amount);
                }
                else if (employmentPriceTypes.Any() && !employmentPriceTypes.Any(a => a.Periods.Any(p => p.FromDate == row.FromDate.Value)))
                {
                    EmploymentPriceTypeDTO last = employmentPriceType = employmentPriceTypes.OrderBy(o => o.EmploymentPriceTypeId).Last();
                    if (!last.Periods.IsNullOrEmpty() && last.Periods.Any() && last.Periods.OrderBy(o => o.FromDate).Last().Amount != amount)
                    {
                        employmentPriceType = last;
                        AddEmploymentPriceTypePeriod(employee, employmentPriceType, payrollPriceType, payrollLevel, row.FromDate.Value.Date, amount);
                    }
                }
            }

            #endregion

            #region Add

            if (employmentPriceType == null)
                AddEmploymentPriceType(employee, employment, payrollPriceType, payrollLevel, row.FromDate.Value.Date, amount);

            #endregion
        }

        private void AddEmploymentPriceType(EmployeeUserDTO employee, EmploymentDTO employment, PayrollPriceTypeDTO payrollPriceType, PayrollLevelDTO payrollLevel, DateTime date, decimal amount)
        {
            if (employee == null || employment == null || payrollPriceType == null)
                return;

            EmploymentPriceTypeDTO employmetPriceType = new EmploymentPriceTypeDTO()
            {
                EmploymentId = employment.EmploymentId,
                PayrollPriceTypeId = payrollPriceType.PayrollPriceTypeId,
                Periods = new List<EmploymentPriceTypePeriodDTO>()
                {
                    new EmploymentPriceTypePeriodDTO()
                    {
                        FromDate = date,
                        Amount = amount,
                        PayrollLevelId = payrollLevel?.PayrollLevelId,
                    }
                }
            };

            if (employment.PriceTypes == null)
                employment.PriceTypes = new List<EmploymentPriceTypeDTO>();
            employment.PriceTypes.Add(employmetPriceType);
            employee.AddCurrentChange((int)EmployeeChangeType.EmploymentPriceType, null, GetPayrollPriceTypeName(payrollPriceType, payrollLevel, amount), fromDate: date);
        }

        private void CopyEmploymentPriceType(EmployeeUserDTO employee, EmploymentDTO employment, EmploymentPriceTypeDTO existingEmploymentPriceType)
        {
            if (employee == null || employment == null || existingEmploymentPriceType == null)
                return;

            EmploymentPriceTypeDTO employmentPriceType = new EmploymentPriceTypeDTO()
            {
                EmploymentId = 0,
                PayrollPriceTypeId = existingEmploymentPriceType.PayrollPriceTypeId,
                Periods = new List<EmploymentPriceTypePeriodDTO>()
            };
            if (!existingEmploymentPriceType.Periods.IsNullOrEmpty())
            {
                foreach (EmploymentPriceTypePeriodDTO existingPeriod in existingEmploymentPriceType.Periods)
                {
                    employmentPriceType.Periods.Add(new EmploymentPriceTypePeriodDTO()
                    {
                        FromDate = existingPeriod.FromDate,
                        Amount = existingPeriod.Amount,
                        PayrollLevelId = existingPeriod.PayrollLevelId,
                    });
                }
            }

            if (employment.PriceTypes == null)
                employment.PriceTypes = new List<EmploymentPriceTypeDTO>();
            employment.PriceTypes.Add(employmentPriceType);
        }

        private void AddEmploymentPriceTypePeriod(EmployeeUserDTO employee, EmploymentPriceTypeDTO employmentPriceType, PayrollPriceTypeDTO payrollPriceType, PayrollLevelDTO payrollLevel, DateTime fromDate, decimal amount)
        {
            if (employee == null || employmentPriceType == null)
                return;

            string fromValue = string.Empty;
            string toValue = GetPayrollPriceTypeName(payrollPriceType, payrollLevel, amount);
            employee.AddCurrentChange((int)EmployeeChangeType.EmploymentPriceType, fromValue, toValue, fromDate: fromDate);
            employmentPriceType.Periods.Add(new EmploymentPriceTypePeriodDTO()
            {
                FromDate = fromDate,
                Amount = amount,
                PayrollLevelId = payrollLevel?.PayrollLevelId
            });
        }

        private void UpdateEmploymentPriceTypePeriod(EmployeeUserDTO employee, EmploymentPriceTypePeriodDTO employmentPriceTypePeriod, PayrollPriceTypeDTO payrollPriceType, PayrollLevelDTO payrollLevel, DateTime fromDate, decimal amount)
        {
            if (employee == null || employmentPriceTypePeriod == null)
                return;

            int? payrollTypeLevelId = payrollLevel?.PayrollLevelId ?? employmentPriceTypePeriod.PayrollLevelId;
            if (employmentPriceTypePeriod.Amount == amount && employmentPriceTypePeriod.PayrollLevelId == payrollTypeLevelId)
                return;

            PayrollLevelDTO existingPayrollLevel = employmentPriceTypePeriod.PayrollLevelId.HasValue ? GetPayrollLevel(employmentPriceTypePeriod.PayrollLevelId.Value) : null;
            string fromValue = GetPayrollPriceTypeName(payrollPriceType, existingPayrollLevel, employmentPriceTypePeriod.Amount);
            string toValue = GetPayrollPriceTypeName(payrollPriceType, payrollLevel ?? existingPayrollLevel, amount);
            employee.AddCurrentChange((int)EmployeeChangeType.EmploymentPriceType, fromValue, toValue, fromDate: fromDate);
            employmentPriceTypePeriod.Amount = amount;
            employmentPriceTypePeriod.PayrollLevelId = payrollTypeLevelId;
        }

        private string GetPayrollPriceTypeName(PayrollPriceTypeDTO payrollPriceType, PayrollLevelDTO payrollLevel, decimal amount)
        {
            if (payrollPriceType == null)
                return null;

            if (payrollLevel != null)
                return $"{payrollPriceType.Name}/{amount}/{payrollLevel.Name}";
            else
                return $"{payrollPriceType.Name}/{amount}";
        }

        private PayrollPriceTypeDTO GetPayrollPriceType(string externalCode)
        {
            if (externalCode.IsNullOrEmpty())
                return null;

            var type = this.lookup.PayrollPriceTypes?.FirstOrDefault(f => f.Code.Equals(externalCode, StringComparison.OrdinalIgnoreCase));

            if (type == null)
                type = this.lookup.PayrollPriceTypes?.FirstOrDefault(f => f.Name.Equals(externalCode, StringComparison.OrdinalIgnoreCase));

            if (type == null && int.TryParse(externalCode, out int id))
                type = this.lookup.PayrollPriceTypes?.FirstOrDefault(f => f.PayrollPriceTypeId == id);

            if (type == null && externalCode.Length > 2)
                type = this.lookup.PayrollPriceTypes?.FirstOrDefault(f => !string.IsNullOrEmpty(f.Description) && f.Description.Equals(externalCode, StringComparison.OrdinalIgnoreCase));

            return type;
        }

        private PayrollLevelDTO GetPayrollLevel(string externalCode)
        {
            if (externalCode.IsNullOrEmpty())
                return null;

            var level = this.lookup.PayrollLevels?.FirstOrDefault(f => !string.IsNullOrEmpty(f.ExternalCode) && f.ExternalCode.Equals(externalCode, StringComparison.OrdinalIgnoreCase));
            if (level == null)
                level = this.lookup.PayrollLevels?.FirstOrDefault(f => !string.IsNullOrEmpty(f.Code) && f.Code.Equals(externalCode, StringComparison.OrdinalIgnoreCase));
            if (level == null)
                level = this.lookup.PayrollLevels?.FirstOrDefault(f => f.Name.Equals(externalCode, StringComparison.OrdinalIgnoreCase));
            if (level == null && int.TryParse(externalCode, out int id))
                level = this.lookup.PayrollLevels?.FirstOrDefault(f => f.PayrollLevelId == id);
            return level;
        }

        private PayrollLevelDTO GetPayrollLevel(int payrollLevelId)
        {
            return this.lookup.PayrollLevels?.FirstOrDefault(p => p.PayrollLevelId == payrollLevelId);
        }

        #endregion

        #region Employment - Secondary

        private void SetEmploymentIsSecondaryEmployment(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;
            if (!StringUtility.IsValidBool(row.Value))
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            bool isSecondaryEmployment = StringUtility.GetBool(row.Value);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment != null && employment.IsTemporaryPrimary)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.EmploymentCannotBeTemporaryPrimaryAndSecondary);
                return;
            }

            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, isSecondaryEmployment: isSecondaryEmployment);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentIsSecondaryEmployment(employment, isSecondaryEmployment, fromDate, toDate);
                else
                    UpdateEmploymentIsSecondaryEmployment(employment, isSecondaryEmployment, fromDate, toDate);
            }
        }

        private void AddEmploymentIsSecondaryEmployment(EmploymentDTO employment, bool isSecondaryEmployment, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || !isSecondaryEmployment)
                return;

            employment.IsSecondaryEmployment = isSecondaryEmployment;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.IsSecondaryEmployment, fromDate, toDate, null, employment.IsSecondaryEmployment.ToString());
        }

        private void UpdateEmploymentIsSecondaryEmployment(EmploymentDTO employment, bool isSecondaryEmployment, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.IsSecondaryEmployment, employment.IsSecondaryEmployment.ToInt(), isSecondaryEmployment.ToInt(), fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.IsSecondaryEmployment, fromDate, toDate, employment.IsSecondaryEmployment.ToInt().ToString(), isSecondaryEmployment.ToInt().ToString());
        }

        #endregion

        #region Employment - EmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment

        private void SetEmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            bool? excludeFromCalculation = null;
            int exclude = 0;
            Int32.TryParse(row.Value, out exclude);
            if (exclude > 0)
            {
                if (exclude == (int)TermGroup_ExcludeFromWorkTimeWeekCalculationItems.Yes)
                    excludeFromCalculation = true;
                else if (exclude == (int)TermGroup_ExcludeFromWorkTimeWeekCalculationItems.No)
                    excludeFromCalculation = false;
            }

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment != null && employment.IsTemporaryPrimary)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.EmploymentCannotBeTemporaryPrimaryAndSecondary);
                return;
            }

            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, excludeFromWorkTimeWeekCalculationOnSecondaryEmployment: excludeFromCalculation);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentEmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(employment, excludeFromCalculation, fromDate, toDate);
                else
                    UpdateEmploymentEmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(employment, excludeFromCalculation, fromDate, toDate);
            }
        }

        private void AddEmploymentEmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(EmploymentDTO employment, bool? excludeFromCalculation, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null)
                return;

            employment.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = excludeFromCalculation;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment, fromDate, toDate, null, employment.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment.ToString());
        }

        private void UpdateEmploymentEmploymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment(EmploymentDTO employment, bool? excludeFromCalculation, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment, employment.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment.ToString(), excludeFromCalculation.ToString(), fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment, fromDate, toDate, employment.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment.ToString(), excludeFromCalculation.ToString());
        }

        #endregion

        #region Employment - SpecialConditions

        private void SetEmploymentSpecialConditions(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            string specialConditions = row.Value;

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, specialConditions: specialConditions);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentSpecialConditions(employment, specialConditions, fromDate, toDate);
                else
                    UpdateEmploymentSpecialConditions(employment, specialConditions, fromDate, toDate);
            }
        }

        private void AddEmploymentSpecialConditions(EmploymentDTO employment, string specialConditions, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || specialConditions.IsNullOrEmpty())
                return;

            employment.SpecialConditions = specialConditions;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.SpecialConditions, fromDate, toDate, null, employment.SpecialConditions);
        }

        private void UpdateEmploymentSpecialConditions(EmploymentDTO employment, string specialConditions, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.SpecialConditions, employment.SpecialConditions.NullToEmpty(), specialConditions, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.SpecialConditions, fromDate, toDate, employment.SpecialConditions, specialConditions);
        }

        #endregion

        #region Employment - SubstituteFor

        private void SetEmploymentSubstituteFor(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            string substituteFor = row.Value;

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, substituteFor: substituteFor);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentSubstituteFor(employment, substituteFor, fromDate, toDate);
                else
                    UpdateEmploymentSubstituteFor(employment, substituteFor, fromDate, toDate);
            }
        }

        private void SetEmploymentSubstituteForDueTo(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            string substituteForDueTo = row.Value;

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, substituteForDueTo: substituteForDueTo);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentSubstituteForDueTo(employment, row.Value, fromDate, toDate);
                else
                    UpdateEmploymentSubstituteForDueTo(employment, row.Value, fromDate, toDate);
            }
        }

        private void AddEmploymentSubstituteFor(EmploymentDTO employment, string substituteFor, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || substituteFor.IsNullOrEmpty())
                return;

            employment.SubstituteFor = substituteFor;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.SubstituteFor, fromDate, toDate, null, employment.SubstituteFor);
        }

        private void UpdateEmploymentSubstituteFor(EmploymentDTO employment, string substituteFor, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.SubstituteFor, employment.SubstituteFor.NullToEmpty(), substituteFor, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.SubstituteFor, fromDate, toDate, employment.SubstituteFor, substituteFor);
        }

        private void AddEmploymentSubstituteForDueTo(EmploymentDTO employment, string substituteForDueTo, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || substituteForDueTo.IsNullOrEmpty())
                return;

            employment.SubstituteForDueTo = substituteForDueTo;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.SubstituteForDueTo, fromDate, toDate, null, employment.SubstituteForDueTo);
        }

        private void UpdateEmploymentSubstituteForDueTo(EmploymentDTO employment, string substituteForDueTo, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.SubstituteForDueTo, employment.SubstituteForDueTo.NullToEmpty(), substituteForDueTo, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.SubstituteForDueTo, fromDate, toDate, employment.SubstituteForDueTo, substituteForDueTo);
        }

        #endregion

        #region Employment - TemporaryPrimary

        private void SetEmploymentNotTemporary(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!StringUtility.GetBool(row.Value))
                return;

            DateTime? date = ParseOptionalEmploymentDate(row, null);
            EmploymentDTO employment = GetEmployment(employee, date, true, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null || !employment.IsTemporaryPrimary || employment.DateFrom > row.FromDate)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidEmploymentTemporaryPrimaryNotValidToChangeToNotPrimary);
                return;
            }

            employment.CurrentChangeDateFrom = row.FromDate;
            employment.CurrentChangeDateTo = row.ToDate;
            employment.IsChangingToNotTemporary = true;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.HibernatingClosed, row.FromDate, row.ToDate, "0", "1");
        }

        private ActionResult ParseTemporaryPrimary(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, NewEmploymentRowIO newEmploymentRow, out bool isTemporaryPrimary, out TimeDeviationCauseDTO hibernatingTimeDeviationCause)
        {
            ActionResult result = new ActionResult(true);
            isTemporaryPrimary = false;
            hibernatingTimeDeviationCause = null;

            if (employee != null && row != null && newEmploymentRow != null && newEmploymentRow.IsTemporaryPrimary)
            {
                result = ValidateTemporaryPrimaryEmployment(employee, newEmploymentRow.DateFrom, newEmploymentRow.DateTo, newEmploymentRow.IsSecondaryEmployment);
                if (result.Success)
                {
                    isTemporaryPrimary = true;

                    //TimeDeviationCause is optional
                    if (!newEmploymentRow.HibernatingEmploymentAbsenceCause.IsNullOrEmpty())
                    {
                        hibernatingTimeDeviationCause = GetTimeDeviationCause(newEmploymentRow.HibernatingEmploymentAbsenceCause);
                        if (hibernatingTimeDeviationCause == null)
                            AddValidationError(employee, row, newEmploymentRow.HibernatingEmploymentAbsenceCause, EmploymentChangeValidationError.InvalidValueAbsenceCauseNotFound);
                    }
                }
                else
                {
                    AddValidationError(employee, row, newEmploymentRow.HibernatingEmploymentAbsenceCause, (EmploymentChangeValidationError)result.ErrorNumber);
                }
            }

            return result;
        }

        private ActionResult ValidateTemporaryPrimaryEmployment(EmployeeUserDTO employee, DateTime? dateFrom, DateTime? dateTo, bool isSecondary)
        {
            ActionResult result = employee.Employments.ValidateTemporaryPrimaryEmployment(dateFrom, dateTo, isSecondary);
            if (!result.Success)
            {
                switch (result.ErrorNumber)
                {
                    case (int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveDateFromAndDateTo:
                        result.ErrorNumber = (int)EmploymentChangeValidationError.TemporaryPrimaryEmploymentMustHaveDateFromAndDateTo;
                        break;
                    case (int)ActionResultSave.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval:
                        result.ErrorNumber = (int)EmploymentChangeValidationError.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval;
                        break;
                    case (int)ActionResultSave.TemporaryPrimaryAlreadyExistsInInterval:
                        result.ErrorNumber = (int)EmploymentChangeValidationError.TemporaryPrimaryAlreadyExistsInInterval;
                        break;
                    case (int)ActionResultSave.TemporaryPrimaryCannotBeSecondary:
                        result.ErrorNumber = (int)EmploymentChangeValidationError.EmploymentCannotBeTemporaryPrimaryAndSecondary;
                        break;
                }
            }
            return result;
        }

        private TimeDeviationCauseDTO GetTimeDeviationCause(string value)
        {
            return
                (this.lookup.TimeDeviationCauses?.FirstOrDefault(f => f.ExtCode != null && f.ExtCode.Equals(value, StringComparison.OrdinalIgnoreCase))) ??
                (this.lookup.TimeDeviationCauses?.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase)));
        }

        #endregion

        #region Employment - Type

        private void SetEmploymentType(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EmploymentTypeDTO employmentType = GetEmploymentType(row);
            if (employmentType == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueEmploymentTypeNotFound);
                return;
            }

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, employmentType: employmentType);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentType(employment, employmentType, fromDate, toDate);
                else
                    UpdateEmploymentType(employment, employmentType, fromDate, toDate);
            }
        }

        private EmploymentTypeDTO GetEmploymentType(EmployeeChangeRowIODTO row)
        {
            if (row?.Value == null)
                return null;

            return GetEmploymentType(row.Value);
        }

        private EmploymentTypeDTO GetEmploymentType(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var employmentType = this.lookup.EmploymentTypes?.FirstOrDefault(f => f.Code != null && f.Code.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (employmentType == null)
                employmentType = this.lookup.EmploymentTypes?.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (employmentType == null)
                employmentType = this.lookup.EmploymentTypes?.FirstOrDefault(f => f.Type.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
            if (employmentType == null && value.Length > 4)
                employmentType = this.lookup.EmploymentTypes?.FirstOrDefault(f => f.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase));
            if (employmentType == null && Int32.TryParse(value, out int employmentTypeId))
                employmentType = this.lookup.EmploymentTypes?.FirstOrDefault(e => e.EmploymentTypeId == employmentTypeId);

            return employmentType;
        }

        private void AddEmploymentType(EmploymentDTO employment, EmploymentTypeDTO employmentType, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null || employmentType == null)
                return;

            employment.EmploymentType = employmentType.GetEmploymentType();
            employment.EmploymentTypeName = employmentType.Name;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.EmploymentType, fromDate, toDate, null, employment.EmploymentType.ToString(), "", employment.EmploymentTypeName);
        }

        private void UpdateEmploymentType(EmploymentDTO employment, EmploymentTypeDTO employmentType, DateTime? fromDate, DateTime? toDate)
        {
            if (employmentType == null || IsEqual(employment, TermGroup_EmploymentChangeFieldType.EmploymentType, employment.EmploymentType, employmentType.GetEmploymentType(), fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.EmploymentType, fromDate, toDate, employment.EmploymentType.ToString(), employmentType.GetEmploymentType().ToString(), employment.EmploymentTypeName, employmentType.Name);
        }

        #endregion

        #region Employment - VacationGroup

        private void SetVacationGroup(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            VacationGroupDTO vacationGroup = GetVacationGroup(row);
            if (vacationGroup == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueVacationGroupNotFound);
                return;
            }

            DateTime fromDate = ParseFromDate(row);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null && !row.Delete)
            {
                CreateEmployment(employee, fromDate, vacationGroup: vacationGroup);
            }
            else if (employment != null)
            {
                if (row.Delete)
                {
                    List<EmploymentVacationGroupDTO> employmentVacationGroups = employment.EmploymentVacationGroup.Where(e => e.VacationGroupId == vacationGroup.VacationGroupId).ToList();
                    foreach (EmploymentVacationGroupDTO employmentVacationGroup in employmentVacationGroups)
                    {
                        RemoveVacationGroup(employee, vacationGroup, employmentVacationGroup);
                    }
                }
                else
                {
                    EmploymentVacationGroupDTO employmentVacationGroup = employment.EmploymentVacationGroup?
                        .FirstOrDefault(e =>
                            e.VacationGroupId == vacationGroup.VacationGroupId &&
                            e.FromDate == fromDate &&
                            e.State == (int)SoeEntityState.Active);

                    if (employmentVacationGroup == null)
                    {
                        employmentVacationGroup = employment.EmploymentVacationGroup?
                            .FirstOrDefault(e =>
                                e.VacationGroupId == vacationGroup.VacationGroupId &&
                                e.State == (int)SoeEntityState.Active);

                        if (employmentVacationGroup == null || employmentVacationGroup.HasOtherVacationGroupBetweenDates(employment.EmploymentVacationGroup, fromDate))
                        {
                            AddVacationGroup(employee, employment, vacationGroup, fromDate);

                            EmploymentVacationGroupDTO otherEmploymentVacationGroup = employment.EmploymentVacationGroup?
                                .FirstOrDefault(e =>
                                    e.VacationGroupId != vacationGroup.VacationGroupId &&
                                    e.FromDate == fromDate &&
                                    e.State == SoeEntityState.Active);

                            if (otherEmploymentVacationGroup != null)
                                RemoveVacationGroup(employee, GetVacationGroup(otherEmploymentVacationGroup.VacationGroupId), otherEmploymentVacationGroup);
                        }
                        else
                        {
                            UpdateVacationGroup(employee, vacationGroup, employmentVacationGroup, fromDate);
                        }
                    }
                }
            }
        }

        private VacationGroupDTO GetVacationGroup(int vacationGroupId)
        {
            return this.lookup.VacationsGroups?.FirstOrDefault(i => i.VacationGroupId == vacationGroupId);
        }

        private VacationGroupDTO GetVacationGroup(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return null;

            return GetVacationGroup(row.Value);
        }

        private VacationGroupDTO GetVacationGroup(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var vacationGroup = this.lookup.VacationsGroups?.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (vacationGroup == null)
                vacationGroup = this.lookup.VacationsGroups?.FirstOrDefault(f => f.ExternalCodes != null && f.ExternalCodes.Where(w => !string.IsNullOrEmpty(w)).Select(s => s.Trim().ToLower()).Contains(value.Trim().ToLower()));

            return vacationGroup;
        }

        private void AddVacationGroup(EmployeeUserDTO employee, EmploymentDTO employment, VacationGroupDTO vacationGroup, DateTime? fromDate, PayrollGroupDTO payrollGroup = null)
        {
            if (employee == null || employment == null)
                return;

            if (vacationGroup == null)
            {
                if (payrollGroup != null)
                {
                    var idFromPayrollGroup = payrollGroup.Vacations?.FirstOrDefault(s => s.IsDefault)?.VacationGroupId;
                    if (!idFromPayrollGroup.HasValue)
                        idFromPayrollGroup = payrollGroup.Vacations?.FirstOrDefault()?.VacationGroupId;
                    if (idFromPayrollGroup.HasValue)
                        vacationGroup = this.lookup.VacationsGroups.FirstOrDefault(f => f.VacationGroupId == idFromPayrollGroup.Value);
                }

                if (vacationGroup == null)
                    vacationGroup = this.lookup.DefaultVacationGroup;
            }

            if (vacationGroup == null)
                return;

            if (employment.EmploymentVacationGroup == null)
                employment.EmploymentVacationGroup = new List<EmploymentVacationGroupDTO>();

            employment.EmploymentVacationGroup.Add(new EmploymentVacationGroupDTO
            {
                VacationGroupId = vacationGroup.VacationGroupId,
                FromDate = fromDate
            });
            employee.AddCurrentChange<int?>((int)EmployeeChangeType.VacationGroup, null, vacationGroup.VacationGroupId, toValueName: vacationGroup.Name, fromDate: fromDate);
        }

        private void UpdateVacationGroup(EmployeeUserDTO employee, VacationGroupDTO vacationGroup, EmploymentVacationGroupDTO employmentVacationGroup, DateTime? fromDate)
        {
            if (employee == null || vacationGroup == null || employmentVacationGroup?.FromDate == null)
                return;
            if (employmentVacationGroup.FromDate <= fromDate)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.VacationGroup, employmentVacationGroup.FromDate, fromDate, vacationGroup.Name, vacationGroup.Name, valuePrefix: "Fr.o.m", fromDate: fromDate);
            employmentVacationGroup.FromDate = fromDate;
        }

        private void RemoveVacationGroup(EmployeeUserDTO employee, VacationGroupDTO vacationGroup, EmploymentVacationGroupDTO employmentVacationGroup)
        {
            if (employee == null || vacationGroup == null || employmentVacationGroup == null)
                return;
            if (employmentVacationGroup.State != SoeEntityState.Active)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.VacationGroup, "Aktiv", "Borttagen", vacationGroup.Name, vacationGroup.Name, fromDate: employmentVacationGroup.FromDate);
            employmentVacationGroup.State = SoeEntityState.Deleted;
        }

        #endregion

        #region Employment - AnnualLeaveGroup

        private void SetAnnualLeaveGroup(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            AnnualLeaveGroupDTO annualLeaveGroup = GetAnnualLeaveGroup(row);
            if (annualLeaveGroup == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueAnnualLeaveGroupNotFound);
                return;
            }

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, annualLeaveGroup: annualLeaveGroup);
            }
            else
            {
                if (employment.AnnualLeaveGroupId == 0)
                    AddAnnualLeaveGroup(employment, annualLeaveGroup, fromDate, toDate);
                else
                    UpdateAnnualLeaveGroup(employment, annualLeaveGroup, fromDate, toDate);
            }
        }

        private AnnualLeaveGroupDTO GetAnnualLeaveGroup(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return null;

            return GetAnnualLeaveGroup(row.Value);
        }

        private AnnualLeaveGroupDTO GetAnnualLeaveGroup(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return this.lookup.AnnualLeaveGroups?.FirstOrDefault(f => f.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
        }

        private void AddAnnualLeaveGroup(EmploymentDTO employment, AnnualLeaveGroupDTO annualLeaveGroup, DateTime? fromDate, DateTime? toDate)
        {
            if (employment == null)
                return;

            if (annualLeaveGroup == null)
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId, fromDate, toDate, null, annualLeaveGroup.AnnualLeaveGroupId.ToString(), toValueName: annualLeaveGroup.Name);
            employment.AnnualLeaveGroupId = annualLeaveGroup.AnnualLeaveGroupId;
        }

        private void UpdateAnnualLeaveGroup(EmploymentDTO employment, AnnualLeaveGroupDTO annualLeaveGroup, DateTime? fromDate, DateTime? toDate)
        {
            if (annualLeaveGroup == null || IsEqual(employment, TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId, employment.AnnualLeaveGroupId.ToString(), annualLeaveGroup.AnnualLeaveGroupId.ToString(), fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId, fromDate, toDate, employment.AnnualLeaveGroupId.ToString(), annualLeaveGroup.AnnualLeaveGroupId.ToString(), this.lookup.AnnualLeaveGroups?.FirstOrDefault(i => i.AnnualLeaveGroupId == employment.AnnualLeaveGroupId)?.Name, annualLeaveGroup.Name);
            if (DoSetValueOnEmployment(employment, fromDate))
                employment.AnnualLeaveGroupId = annualLeaveGroup.AnnualLeaveGroupId;
        }

        #endregion

        #region Employment - WorkTasks

        private void SetEmploymentWorkTasks(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            string workTasks = row.Value;

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, workTasks: workTasks);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentWorkTasks(employment, workTasks, fromDate, toDate);
                else
                    UpdateEmploymentWorkTasks(employment, workTasks, fromDate, toDate);
            }
        }

        private void AddEmploymentWorkTasks(EmploymentDTO employment, string workTasks, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || workTasks.IsNullOrEmpty())
                return;

            employment.WorkTasks = workTasks;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkTasks, fromDate, toDate, null, employment.WorkTasks);
        }

        private void UpdateEmploymentWorkTasks(EmploymentDTO employment, string workTasks, DateTime? fromDate, DateTime? toDate)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.WorkTasks, employment.WorkTasks.NullToEmpty(), workTasks, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkTasks, fromDate, toDate, employment.WorkTasks, workTasks);
        }

        #endregion

        #region Employment - WorkPlace

        private void SetEmploymentWorkPlace(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);
            string workPlace = row.Value;

            EmploymentDTO employment = GetEmployment(employee, fromDate, optionalEmploymentExternalCode: row.OptionalEmploymentExternalCode);
            if (employment == null)
            {
                CreateEmployment(employee, fromDate, toDate, workPlace: workPlace);
            }
            else
            {
                if (DoSetValueOnEmployment(employment, fromDate))
                    AddEmploymentWorkPlace(employment, workPlace, fromDate, toDate);
                else
                    UpdateEmploymentWorkPlace(employment, workPlace, fromDate, toDate);
            }
        }

        private void AddEmploymentWorkPlace(EmploymentDTO employment, string workPlace, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employment == null || workPlace.IsNullOrEmpty())
                return;

            employment.WorkPlace = workPlace;
            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkPlace, fromDate, toDate, null, employment.WorkPlace);
        }

        private void UpdateEmploymentWorkPlace(EmploymentDTO employment, string workPlace, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (IsEqual(employment, TermGroup_EmploymentChangeFieldType.WorkPlace, employment.WorkPlace.NullToEmpty(), workPlace, fromDate, toDate))
                return;

            employment.AddCurrentChange(TermGroup_EmploymentChangeFieldType.WorkPlace, fromDate, toDate, employment.WorkPlace, workPlace);
        }

        #endregion

        #endregion

        #region Employee

        #region Employee - Identity

        private void SetFirstName(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (string.IsNullOrEmpty(employee.FirstName) || !employee.FirstName.Equals(row.Value))
            {
                employee.AddCurrentChange((int)EmployeeChangeType.FirstName, employee.FirstName, row.Value);
                employee.FirstName = row.Value;
            }
        }

        private void SetLastName(EmployeeUserDTO employee, EmployeeChangeRowIODTO rows)
        {
            if (employee == null || rows == null)
                return;

            if (string.IsNullOrEmpty(employee.LastName) || !employee.LastName.Equals(rows.Value))
            {
                employee.AddCurrentChange((int)EmployeeChangeType.LastName, employee.LastName, rows.Value);
                employee.LastName = rows.Value;
            }
        }

        private void SetSocialSec(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            TermGroup_Country companyCountry = GetCompanyCountry();
            string socialSec = (companyCountry == TermGroup_Country.SE) ? StringUtility.SocialSecYYYYMMDD_Dash_XXXX(row.Value) : row.Value;
            if (string.IsNullOrEmpty(employee.SocialSec) || !employee.SocialSec.Equals(socialSec))
            {
                employee.AddCurrentChange((int)EmployeeChangeType.SocialSec, employee.SocialSec, socialSec);
                employee.SocialSec = socialSec;
            }
        }

        private void SetExternalAuthId(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row?.Value == null)
                return;

            if (string.IsNullOrEmpty(employee.ExternalAuthId) || !employee.ExternalAuthId.Equals(row.Value))
            {
                if (row.Value.Length < 2 || row.Delete)
                {

                    if (row.Trace == null)
                        row.Trace = string.Empty;
                    row.Trace += $"ExternalAuthId has been cleared in SetExternalAuthId value {row.Value} delete {row.Delete}";
                    row.Value = string.Empty;
                }

                employee.AddCurrentChange((int)EmployeeChangeType.ExternalAuthId, employee.ExternalAuthId, row.Value);
                employee.ExternalAuthId = row.Value;
                employee.ExternalAuthIdModified = true;
            }
        }

        private void SetExternalCode(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee != null && !employee.ExternalCode.NullToEmpty().Equals(row.Value))
            {
                employee.AddCurrentChange((int)EmployeeChangeType.ExternalCode, employee.ExternalCode, row.Value);
                employee.ExternalCode = row.Value;
            }
        }

        #endregion

        #region Employee - Flags

        private void SetActive(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!StringUtility.IsValidBool(row.Value))
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueActiveStateNotFound);
                return;
            }

            bool value = StringUtility.GetBool(row.Value);
            if (value && employee.State != SoeEntityState.Active)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.Active, 0, 1);
                employee.State = SoeEntityState.Active;
            }
            else if (!value && employee.State == SoeEntityState.Active)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.Active, 1, 0);
                employee.State = SoeEntityState.Inactive;
            }

            SetSaveUser();
        }

        private void SetDoNotValidateAccount(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (!StringUtility.IsValidBool(row.Value))
                return;

            var doNotValidateAccount = StringUtility.GetBool(row.Value);

            if (employee.DontValidateDisbursementAccountNr != doNotValidateAccount)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.DoNotValidateAccount, null, null, employee.DontValidateDisbursementAccountNr.ToString(), doNotValidateAccount.ToString());
                employee.DontValidateDisbursementAccountNr = doNotValidateAccount;
            }
        }

        private void SetExcludeFromPayroll(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null || !StringUtility.IsValidBool(row.Value))
                return;

            bool value = StringUtility.GetBool(row.Value);
            if (value != employee.ExcludeFromPayroll)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.ExcludeFromPayroll, employee.ExcludeFromPayroll, value);
                employee.ExcludeFromPayroll = value;
            }
        }

        private void SetExtraField(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!string.IsNullOrEmpty(row.Value) &&
                !string.IsNullOrEmpty(row.OptionalExternalCode))
            {
                int.TryParse(row.OptionalExternalCode, out int id);
                ExtraFieldDTO extraField = id != 0 ? this.lookup.ExtraFields?.FirstOrDefault(f => f.ExtraFieldId == id) : null;

                if (extraField == null && !string.IsNullOrEmpty(row.OptionalExternalCode))
                {
                    extraField = this.lookup.ExtraFields?.FirstOrDefault(f => f.ExternalCodes?.Any(c => c.Equals(row.OptionalExternalCode, StringComparison.OrdinalIgnoreCase)) ?? false);

                    if (extraField == null)
                        extraField = this.lookup.ExtraFields?.FirstOrDefault(f => f.Text.Equals(row.OptionalExternalCode, StringComparison.OrdinalIgnoreCase));
                }

                if (extraField != null)
                {
                    if (employee.ExtraFieldRecords == null)
                    {
                        // Load existing records
                        employee.ExtraFieldRecords = GetExtraFieldRecords(employee.EmployeeNr);
                        if (employee.ExtraFieldRecords == null)
                            employee.ExtraFieldRecords = new List<ExtraFieldRecordDTO>();
                    }

                    // Check if extra field already exists on employee
                    ExtraFieldRecordDTO extraFieldRecordDTO = employee.ExtraFieldRecords.FirstOrDefault(r => r.ExtraFieldId == extraField.ExtraFieldId);
                    bool isNew = false;
                    if (extraFieldRecordDTO == null)
                    {
                        isNew = true;
                        extraFieldRecordDTO = new ExtraFieldRecordDTO()
                        {
                            ExtraFieldRecordId = employee.EmployeeId,
                            ExtraFieldId = extraField.ExtraFieldId,
                            ExtraFieldType = (int)extraField.Type,
                        };
                    }

                    void AddExtraFieldChange<T>(T fromValue, T toValue, string fromValueName = null, string toValueName = null)
                    {
                        employee.AddCurrentChange(
                            (int)EmployeeChangeType.ExtraFieldEmployee,
                            fromValueName == null ? $"{extraField.Text}: {fromValue}" : $"{extraField.Text}: {fromValueName} ({fromValue})",
                            toValueName == null ? $"{extraField.Text}: {toValue}" : $"{extraField.Text}: {toValueName} ({toValue})"
                        );
                    }

                    bool invalid = false;
                    switch (extraField.Type)
                    {
                        case TermGroup_ExtraFieldType.FreeText:
                            if (extraFieldRecordDTO.StrData != row.Value)
                            {
                                AddExtraFieldChange(extraFieldRecordDTO.StrData, row.Value);
                                extraFieldRecordDTO.StrData = row.Value;
                            }
                            break;
                        case TermGroup_ExtraFieldType.Integer:
                            if (int.TryParse(row.Value, out int intValue))
                            {
                                if (extraFieldRecordDTO.IntData != intValue)
                                {
                                    AddExtraFieldChange(extraFieldRecordDTO.IntData, intValue);
                                    extraFieldRecordDTO.IntData = intValue;
                                }
                            }
                            else
                                invalid = true;
                            break;
                        case TermGroup_ExtraFieldType.Decimal:
                            if (decimal.TryParse(row.Value, out decimal decimalValue))
                            {
                                if (extraFieldRecordDTO.DecimalData != decimalValue)
                                {
                                    AddExtraFieldChange(extraFieldRecordDTO.DecimalData, decimalValue);
                                    extraFieldRecordDTO.DecimalData = decimalValue;
                                }
                            }
                            else
                                invalid = true;
                            break;
                        case TermGroup_ExtraFieldType.YesNo:
                            if (StringUtility.IsValidBool(row.Value))
                            {
                                int yesNoValue = StringUtility.GetBool(row.Value) ? (int)TermGroup_YesNo.Yes : (int)TermGroup_YesNo.No;
                                if (extraFieldRecordDTO.IntData != yesNoValue)
                                {
                                    AddExtraFieldChange(extraFieldRecordDTO.IntData, yesNoValue);
                                    extraFieldRecordDTO.IntData = yesNoValue;
                                }
                            }
                            else
                                invalid = true;
                            break;
                        case TermGroup_ExtraFieldType.Checkbox:
                            if (StringUtility.IsValidBool(row.Value))
                            {
                                bool boolValue = StringUtility.GetBool(row.Value);
                                if (extraFieldRecordDTO.BoolData != boolValue)
                                {
                                    AddExtraFieldChange(extraFieldRecordDTO.BoolData, boolValue);
                                    extraFieldRecordDTO.BoolData = boolValue;
                                }
                            }
                            else
                                invalid = true;
                            break;
                        case TermGroup_ExtraFieldType.Date:
                            var newDateData = StringUtility.GetNullableDateTime(row.Value);
                            if (newDateData.HasValue)
                            {
                                newDateData = CalendarUtility.GetBeginningOfDay(newDateData);
                                if (!extraFieldRecordDTO.DateData.HasValue || !extraFieldRecordDTO.Equals(newDateData.Value))
                                {
                                    AddExtraFieldChange(extraFieldRecordDTO.DateData?.ToString("yyyy-MM-dd"), newDateData.Value.ToString("yyyy-MM-dd"));
                                    extraFieldRecordDTO.DateData = newDateData.Value;
                                }
                            }
                            else
                                invalid = true;
                            break;
                        case TermGroup_ExtraFieldType.SingleChoice:
                            ExtraFieldValueDTO newExtraFieldValue = null;

                            if (int.TryParse(row.Value, out int valueId))
                                newExtraFieldValue = extraField.ExtraFieldValues.FirstOrDefault(v => v.ExtraFieldValueId == valueId);

                            if (newExtraFieldValue == null)
                                newExtraFieldValue = extraField.ExtraFieldValues.FirstOrDefault(v => v.Value.Equals(row.Value, StringComparison.OrdinalIgnoreCase));

                            if (newExtraFieldValue != null)
                                if (extraFieldRecordDTO.IntData != newExtraFieldValue.ExtraFieldValueId)
                                {
                                    string fromValueName = extraField.ExtraFieldValues.FirstOrDefault(v => v.ExtraFieldValueId == extraFieldRecordDTO.IntData)?.Value;
                                    AddExtraFieldChange(extraFieldRecordDTO.IntData, newExtraFieldValue.ExtraFieldValueId, fromValueName, newExtraFieldValue.Value);
                                    extraFieldRecordDTO.IntData = newExtraFieldValue.ExtraFieldValueId;
                                }
                                else
                                    invalid = true;
                            break;
                        case TermGroup_ExtraFieldType.MultiChoice:
                            invalid = true;
                            break;
                        default:
                            invalid = true;
                            break;
                    }

                    if (isNew && !invalid)
                        employee.ExtraFieldRecords.Add(extraFieldRecordDTO);
                }
            }
        }

        private void SetVacant(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!StringUtility.IsValidBool(row.Value))
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidBool);
                return;
            }
            employee.AddCurrentChange((int)EmployeeChangeType.Vacant, 0, 1);
            employee.Vacant = StringUtility.GetBool(row.Value);
        }

        private void SetWantsExtraShifts(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null || !StringUtility.IsValidBool(row.Value))
                return;

            bool value = StringUtility.GetBool(row.Value);
            if (value != employee.WantsExtraShifts)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.WantsExtraShifts, employee.WantsExtraShifts, value);
                employee.WantsExtraShifts = value;
            }
        }

        #endregion

        #region Employee - Account (HierarchicalAccount)

        private void SetHierarchicalAccount(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            rows.Where(w => w.OptionalExternalCode == null).ToList().ForEach(f => f.OptionalExternalCode = "");

            // row.OptionalExternalCode.Contains("|") means that the sort order i set as Sort|Code

            foreach (EmployeeChangeRowIODTO row in rows.OrderBy(o => o.Sort).ThenBy(o => o.OptionalExternalCode))
            {
                bool mainAllocation = false;
                bool isDefault = true;

                #region Find Account

                List<AccountDTO> validAccounts = GetAccountsForHierchalAccount(employee);

                var account = validAccounts.FirstOrDefault(f => !string.IsNullOrEmpty(f.ExternalCode) && f.ExternalCode.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
                if (account == null)
                    account = validAccounts.FirstOrDefault(f => !string.IsNullOrEmpty(f.AccountNr) && f.AccountNr.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
                if (account == null)
                    account = validAccounts.FirstOrDefault(f => !string.IsNullOrEmpty(f.Name) && f.Name.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
                if (account == null && int.TryParse(row.Value, out int accountId))
                {
                    if (row.Value.StartsWith("0"))
                        account = validAccounts.FirstOrDefault(f => !string.IsNullOrEmpty(f.AccountNr) && f.AccountNr.Equals(accountId.ToString(), StringComparison.OrdinalIgnoreCase));

                    if (account == null)
                        account = validAccounts.FirstOrDefault(f => f.AccountId == accountId);
                }

                if (account == null)
                {
                    AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueAccountNotFound);
                    continue;
                }

                if (!row.OptionalExternalCode.IsNullOrEmpty())
                {
                    string[] parts = row.OptionalExternalCode.Split('|');
                    if (parts.Length > 0)
                    {
                        string last = parts[parts.Length - 1];
                        if (last.Contains('#'))
                        {
                            string[] bools = last.Split('#');
                            if (bools.Length == 2)
                            {
                                mainAllocation = Convert.ToBoolean(bools[0]);
                                isDefault = Convert.ToBoolean(bools[1]);
                            }
                        }
                    }
                }

                #endregion

                #region Find parent

                EmployeeAccountDTO parentMatch = null;

                if ((!account.ParentAccountId.HasValue || row.OptionalExternalCode.Contains("|")) && !string.IsNullOrEmpty(row.OptionalExternalCode))
                {
                    // If first in hierarchy (top) do not search for parent account
                    EmployeeChangeRowIODTO firstRow = rows.OrderBy(o => o.Sort).ThenBy(o => o.OptionalExternalCode).First();
                    if (row != firstRow)
                    {
                        bool optionalCodeIsNotAboutParentAccount = false;
                        if (!string.IsNullOrEmpty(row.OptionalExternalCode) &&
                           row.OptionalExternalCode.Contains("#") &&
                           (!row.OptionalExternalCode.Contains("|") || row.OptionalExternalCode.StartsWith("|")))
                        {
                            optionalCodeIsNotAboutParentAccount = true;
                        }

                        AccountDTO parentAccount = GetAccountByOptionalExternalCode(row);

                        if (parentAccount != null || !optionalCodeIsNotAboutParentAccount)
                        {
                            if (parentAccount == null)
                            {
                                AddValidationError(employee, row, row.OptionalExternalCode, EmploymentChangeValidationError.InvalidOptionalExternalCodeParentAccountNotFound);
                                continue;
                            }

                            if (!employee.Accounts.IsNullOrEmpty())
                            {
                                parentMatch = employee.Accounts.FirstOrDefault(f => f.AccountId == parentAccount.AccountId);
                                if (parentMatch == null)
                                {
                                    foreach (var employeeAccount in employee.Accounts)
                                    {
                                        if (parentMatch == null)
                                            parentMatch = employeeAccount.Children.FirstOrDefault(f => f.AccountId == parentAccount.AccountId);
                                    }
                                }
                            }
                        }
                    }
                }


                #endregion

                #region EmployeeAccount

                if (parentMatch == null)
                    employee.Accounts = SaveEmployeeAccounts(employee, row, employee.Accounts, account, validAccounts, mainAllocation, isDefault);
                else
                    parentMatch.Children = SaveEmployeeAccounts(employee, row, parentMatch.Children, account, validAccounts, mainAllocation, isDefault);

                #endregion
            }
        }

        private void SetEmployerRegistrations(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            foreach (var row in rows)
            {
                SetEmployerRegistrationNr(employee, row);
            }
        }

        private List<AccountDTO> GetAccountsForHierchalAccount(EmployeeUserDTO employee)
        {
            List<AccountDTO> validAccounts = this.lookup.AccountInternals ?? new List<AccountDTO>();
            int? accountDimId = GetSettingAccountDimIdForEmployeeAccount(employee);
            if (accountDimId.HasValue)
                validAccounts = validAccounts.Where(x => x.AccountDimId == accountDimId.Value).ToList();
            return validAccounts;
        }

        private List<EmployeeAccountDTO> SaveEmployeeAccounts(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, List<EmployeeAccountDTO> employeeAccounts, AccountDTO account, List<AccountDTO> validAccounts, bool mainAllocation = false, bool isDefault = false)
        {
            if (employee == null)
                return employeeAccounts;

            DateTime fromDate = ParseFromDate(row);
            DateTime? toDate = ParseToDate(row);

            if (!employeeAccounts.IsNullOrEmpty())
            {
                #region From PreviousValue
                EmployeeAccountDTO employeeAccountFromPrevious = null;
                var accountNr = string.Empty;
                DateTime? previousRowDate = null;
                AccountDTO previousAccount = null;
                bool handled = false;
                if (row.PreviousValue != null)
                {
                    var split = row.PreviousValue.Split('|');
                    accountNr = split[0];

                    if (split.Count() > 1 && DateTime.TryParse(split[1], out DateTime dateOnPrev))
                        previousRowDate = dateOnPrev;

                    if (!string.IsNullOrEmpty(accountNr))
                    {
                        previousAccount = validAccounts.FirstOrDefault(w => w.AccountNr.Equals(accountNr));

                        if (previousAccount == null)
                            previousAccount = validAccounts.FirstOrDefault(w => w.ExternalCode.Equals(accountNr));
                    }
                }

                if (previousAccount != null && previousRowDate.HasValue)
                    employeeAccountFromPrevious = employee.Accounts.FirstOrDefault(w => w.AccountId == previousAccount.AccountId && w.DateFrom == previousRowDate.Value);

                if (employeeAccountFromPrevious == null && previousAccount != null)
                    employeeAccountFromPrevious = employee.Accounts.FirstOrDefault(w => w.AccountId == previousAccount.AccountId);


                if (employeeAccountFromPrevious != null)
                {
                    if (employeeAccountFromPrevious.AccountId != account.AccountId)
                    {
                        // new row is needed close old row
                        employeeAccountFromPrevious.DateTo = row.FromDate.Value.AddDays(-1);
                    }
                    else
                    {
                        if (row.Delete)
                            DeleteEmployeeAccount(employee, employeeAccountFromPrevious);
                        else if (fromDate != CalendarUtility.DATETIME_DEFAULT && fromDate != employeeAccountFromPrevious.DateFrom)
                            UpdateEmployeeAccountDateFrom(employee, employeeAccountFromPrevious, account, fromDate.Date);
                        else if (toDate.HasValue && toDate != employeeAccountFromPrevious.DateTo)
                            UpdateEmployeeAccountDateTo(employee, employeeAccountFromPrevious, account, toDate.Value.Date);
                        else if (!toDate.HasValue && employeeAccountFromPrevious.DateTo.HasValue)
                            UpdateEmployeeAccountDateTo(employee, employeeAccountFromPrevious, account, toDate);
                        employeeAccountFromPrevious.MainAllocation = mainAllocation;
                        employeeAccountFromPrevious.Default = isDefault;
                        handled = true;
                    }
                }

                #endregion

                #region Employee has EmployeeAccounts

                if (!handled)
                {
                    if (!employeeAccounts.Any(a => a.AccountId == account.AccountId))
                    {
                        #region Employee has no EmployeeAccount with given Account - add

                        employeeAccounts.Add(AddEmployeeAccount(employee, account, fromDate, toDate, mainAllocation, isDefault));

                        #endregion
                    }
                    else
                    {
                        #region Employee has EmployeeAccount with given Account - add or update

                        EmployeeAccountDTO employeeAccount = null;
                        if (toDate == CalendarUtility.DATETIME_DEFAULT)
                            employeeAccount = employeeAccounts.OrderByDescending(o => o.DateFrom).FirstOrDefault(a => a.AccountId == account.AccountId && !a.DateTo.HasValue);
                        if (employeeAccount == null)
                            employeeAccount = employeeAccounts.OrderByDescending(o => o.DateFrom).FirstOrDefault(a => a.AccountId == account.AccountId && a.DateFrom == fromDate && a.DateTo.HasValue);
                        if (employeeAccount == null)
                            employeeAccount = employeeAccounts.OrderByDescending(o => o.DateFrom).FirstOrDefault(a => a.AccountId == account.AccountId && a.DateFrom == fromDate);
                        if (employeeAccount == null)
                            employeeAccount = employeeAccounts.OrderByDescending(o => o.DateFrom).FirstOrDefault(a => a.AccountId == account.AccountId);

                        if (employeeAccount != null)
                        {
                            if (row.Delete)
                                DeleteEmployeeAccount(employee, employeeAccount);
                            else if (employeeAccount.DateTo.HasValue && fromDate >= employeeAccount.DateTo)
                                employeeAccounts.Add(AddEmployeeAccount(employee, account, fromDate, toDate, mainAllocation, isDefault));
                            else if (fromDate != CalendarUtility.DATETIME_DEFAULT && fromDate < employeeAccount.DateFrom)
                                UpdateEmployeeAccountDateFrom(employee, employeeAccount, account, fromDate.Date);
                            else if (toDate.HasValue && toDate != employeeAccount.DateTo)
                                UpdateEmployeeAccountDateTo(employee, employeeAccount, account, toDate.Value.Date);
                            else if (!toDate.HasValue && employeeAccount.DateTo.HasValue)
                                UpdateEmployeeAccountDateTo(employee, employeeAccount, account, toDate);
                        }

                        #endregion
                    }
                }

                #endregion
            }
            else
            {
                #region Employee has no EmployeeAccounts

                employeeAccounts = new List<EmployeeAccountDTO>
                {
                    AddEmployeeAccount(employee, account, fromDate, toDate, mainAllocation, isDefault),
                };

                #endregion
            }

            return employeeAccounts;
        }

        private EmployeeAccountDTO AddEmployeeAccount(EmployeeUserDTO employee, AccountDTO account, DateTime fromDate, DateTime? toDate, bool mainAllocation = false, bool isDefault = false)
        {
            if (employee == null || account == null)
                return null;

            EmployeeAccountDTO employeeAccount = new EmployeeAccountDTO()
            {
                AccountId = account.AccountId,
                MainAllocation = mainAllocation,
                Default = isDefault,
                DateFrom = fromDate != CalendarUtility.DATETIME_DEFAULT ? fromDate.Date : CalendarUtility.DATETIME_DEFAULT,
                DateTo = toDate
            };

            employee.AddCurrentChange((int)EmployeeChangeType.HierarchicalAccount, null, account.Name, fromDate: employeeAccount.DateFrom, toDate: employeeAccount.DateTo);

            return employeeAccount;
        }

        private void UpdateEmployeeAccountDateFrom(EmployeeUserDTO employee, EmployeeAccountDTO employeeAccount, AccountDTO account, DateTime date)
        {
            if (employee == null || employeeAccount == null || account == null)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.HierarchicalAccount, employeeAccount.DateFrom, date, account.Name, account.Name, valuePrefix: "Fr.o.m", fromDate: date, toDate: employeeAccount.DateTo);
            employeeAccount.DateFrom = date;
        }

        private void UpdateEmployeeAccountDateTo(EmployeeUserDTO employee, EmployeeAccountDTO employeeAccount, AccountDTO account, DateTime? date)
        {
            if (employee == null || employeeAccount == null || account == null)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.HierarchicalAccount, employeeAccount.DateTo, date, account.Name, account.Name, valuePrefix: "T.o.m", fromDate: employeeAccount.DateFrom, toDate: date);
            employeeAccount.DateTo = date;
        }

        private void DeleteEmployeeAccount(EmployeeUserDTO employee, EmployeeAccountDTO employeeAccount)
        {
            if (employee == null || employeeAccount == null)
                return;
            employeeAccount.State = SoeEntityState.Deleted;
            employee.AddCurrentChange((int)EmployeeChangeType.HierarchicalAccount, "Aktiv", "Borttagen");
        }

        #endregion

        #region Employee - BlockedFromDate

        private void SetBlockedFromDate(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!string.IsNullOrEmpty(row.Value))
            {
                if (!DateTime.TryParse(row.Value, out DateTime blockedFromDate))
                    return;

                if (employee.BlockedFromDate != blockedFromDate.Date)
                {
                    employee.AddCurrentChange((int)EmployeeChangeType.BlockedFromDate, employee.BlockedFromDate, blockedFromDate.Date);
                    employee.BlockedFromDate = blockedFromDate.Date;
                }
            }
            else
            {
                ClearBlockedFromDate(employee);
            }

            SetSaveUser();
        }

        private void ClearBlockedFromDate(EmployeeUserDTO employee)
        {
            if (employee == null || !employee.BlockedFromDate.HasValue)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.BlockedFromDate, employee.BlockedFromDate, null);
            employee.BlockedFromDate = null;
        }

        #endregion

        #region Employee - Disbursement

        private void SetDisbursementMethod(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (int.TryParse(row.Value, out int disbursementMethodId))
            {
                TermGroup_EmployeeDisbursementMethod disbursementMethod = (TermGroup_EmployeeDisbursementMethod)disbursementMethodId;
                if (employee.DisbursementMethod != disbursementMethod)
                {
                    employee.AddCurrentChange((int)EmployeeChangeType.DisbursementMethod, (int)employee.DisbursementMethod, disbursementMethodId);
                    employee.DisbursementMethod = disbursementMethod;
                }
            }
        }

        private void SetDisbursementClearingAndAccountNr(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            string[] values = StringUtility.Split(new char[1] { '#' }, row.Value);
            if (values.Count() >= 2)
            {
                string clearingNr = values[0].NullToEmpty();
                string accountNr = values[1].NullToEmpty();
                bool dontValidate = (values.Count() > 2 && values[2].NullToEmpty() == "1");

                if (!employee.DisbursementClearingNr.NullToEmpty().Equals(clearingNr) || !employee.DisbursementAccountNr.NullToEmpty().Equals(values[1]))
                {
                    employee.AddCurrentChange((int)EmployeeChangeType.DisbursementAccountNr, employee.DisbursementClearingNr + " " + employee.DisbursementAccountNr, clearingNr + " " + accountNr);
                    employee.DisbursementClearingNr = clearingNr;
                    employee.DisbursementAccountNr = accountNr;
                }

                if (employee.DontValidateDisbursementAccountNr != dontValidate)
                {
                    employee.AddCurrentChange((int)EmployeeChangeType.DontValidateDisbursementAccountNr, employee.DontValidateDisbursementAccountNr, dontValidate);
                    employee.DontValidateDisbursementAccountNr = dontValidate;
                }
            }
        }

        private void SetDisbursementCountryCode(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            string disbursementCountryCode = row.Value.Trim();

            if (employee.DisbursementCountryCode.NullToEmpty() != disbursementCountryCode)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.DisbursementCountryCode, null, null, employee.DisbursementCountryCode, disbursementCountryCode);
                employee.DisbursementCountryCode = disbursementCountryCode;
            }
        }

        private void SetDisbursementBIC(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            string disbursementBIC = row.Value.Trim();

            if (employee.DisbursementBIC.NullToEmpty() != disbursementBIC)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.DisbursementBIC, null, null, employee.DisbursementBIC, disbursementBIC);
                employee.DisbursementBIC = disbursementBIC;
            }
        }

        private void SetDisbursementIBAN(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            string disbursementIBAN = row.Value.Trim();

            if (employee.DisbursementIBAN.NullToEmpty() != disbursementIBAN)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.DisbursementIBAN, null, null, employee.DisbursementIBAN, disbursementIBAN);
                employee.DisbursementIBAN = disbursementIBAN;
            }
        }

        private void SetEmployerRegistrationNr(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            string employerRegistrationNr = row.Value.Trim();

            if (employee.EmployeeEmployeers == null)
                employee.EmployeeEmployeers = new List<EmployeeEmployeerDTO>();

            var match = employee.EmployeeEmployeers.FirstOrDefault(a => a.EmployerRegistrationNumber == employerRegistrationNr && a.DateFrom == row.FromDate);

            if (match != null)
            {
                if (match.DateTo != row.ToDate)
                {
                    employee.AddCurrentChange((int)EmployeeChangeType.EmployerRegistrationNr, match.DateTo, row.ToDate, valuePrefix: $"Org.nr: {employerRegistrationNr}");
                    match.DateTo = row.ToDate;
                }

                if (row.Delete)
                {
                    employee.AddCurrentChange((int)EmployeeChangeType.EmployerRegistrationNr, "Aktiv", "Borttagen", fromDate: match.DateFrom, toDate: match.DateTo, valuePrefix: $"Org.nr: {employerRegistrationNr}");
                    match.State = SoeEntityState.Deleted;
                }
                return;
            }
            else
            {
                employee.EmployeeEmployeers.Add(new EmployeeEmployeerDTO()
                {
                    EmployerRegistrationNumber = employerRegistrationNr,
                    DateFrom = row.FromDate ?? CalendarUtility.DATETIME_DEFAULT,
                    DateTo = row.ToDate,
                });
            }
        }

        private void SetParentEmployeeNr(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;
            var parentEmployeeNr = row.Value.Trim();

            if (employee.ParentEmployeeNr != parentEmployeeNr)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.ParentEmployeeNr, null, null, employee.ParentEmployeeNr, parentEmployeeNr);
                employee.ParentEmployeeNr = parentEmployeeNr;
            }
        }

        #endregion

        #region Employee - Reports

        private void SetReportSettingsPayrollStatistics(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                int? result;
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.PayrollStatisticsPersonalCategory:
                        if (TryUpdateEmployeeReportField(employee, row, employee.PayrollReportsPersonalCategory, out result, TermGroup.PayrollReportsPersonalCategory))
                            employee.PayrollReportsPersonalCategory = result;
                        break;
                    case EmployeeChangeType.PayrollStatisticsWorkTimeCategory:
                        if (TryUpdateEmployeeReportField(employee, row, employee.PayrollReportsWorkTimeCategory, out result, TermGroup.PayrollReportsWorkTimeCategory))
                            employee.PayrollReportsWorkTimeCategory = result;
                        break;
                    case EmployeeChangeType.PayrollStatisticsSalaryType:
                        if (TryUpdateEmployeeReportField(employee, row, employee.PayrollReportsSalaryType, out result, TermGroup.PayrollReportsSalaryType))
                            employee.PayrollReportsSalaryType = result;
                        break;
                    case EmployeeChangeType.PayrollStatisticsWorkPlaceNumber:
                        if (TryUpdateEmployeeReportField(employee, row, employee.PayrollReportsWorkPlaceNumber, out result))
                            employee.PayrollReportsWorkPlaceNumber = result;
                        break;
                    case EmployeeChangeType.PayrollStatisticsCFARNumber:
                        if (TryUpdateEmployeeReportField(employee, row, employee.PayrollReportsCFARNumber, out result))
                            employee.PayrollReportsCFARNumber = result;
                        break;
                }
            }
        }

        private void SetReportSettingsControlTask(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.ControlTaskWorkPlaceSCB:
                        if (TryUpdateEmployeeReportField(employee, row, employee.WorkPlaceSCB, out string workPlaceSCB))
                            employee.WorkPlaceSCB = workPlaceSCB;
                        break;
                    case EmployeeChangeType.ControlTaskPartnerInCloseCompany:
                        if (TryUpdateEmployeeReportField(employee, row, employee.PartnerInCloseCompany, out bool? partnerInCloseCompany))
                            employee.PartnerInCloseCompany = partnerInCloseCompany ?? false;
                        break;
                    case EmployeeChangeType.ControlTaskBenefitAsPension:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BenefitAsPension, out bool? benefitAsPension))
                            employee.BenefitAsPension = benefitAsPension ?? false;
                        break;
                }
            }
        }

        private void SetReportSettingsAFA(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.AFACategory:
                        if (TryUpdateEmployeeReportField(employee, row, employee.AFACategory, out int? afaCategory, TermGroup.PayrollReportsAFACategory))
                            employee.AFACategory = afaCategory ?? 0;
                        break;
                    case EmployeeChangeType.AFASpecialAgreement:
                        if (TryUpdateEmployeeReportField(employee, row, employee.AFASpecialAgreement, out int? afaSpecialAgreement, TermGroup.PayrollReportsAFASpecialAgreement))
                            employee.AFASpecialAgreement = afaSpecialAgreement ?? 0;
                        break;
                    case EmployeeChangeType.AFAWorkplaceNr:
                        if (TryUpdateEmployeeReportField(employee, row, employee.AFAWorkplaceNr, out string afaWorkplaceNr))
                            employee.AFAWorkplaceNr = afaWorkplaceNr;
                        break;
                    case EmployeeChangeType.AFAParttimePensionCode:
                        if (TryUpdateEmployeeReportField(employee, row, employee.AFAParttimePensionCode, out bool? afaParttimePensionCode))
                            employee.AFAParttimePensionCode = afaParttimePensionCode ?? false;
                        break;
                }
            }
        }

        private void SetReportSettingsCollectum(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.CollectumITPPlan:
                        if (TryUpdateEmployeeReportField(employee, row, employee.CollectumITPPlan, out int? collectumITPPlan, TermGroup.PayrollReportsCollectumITPplan))
                            employee.CollectumITPPlan = collectumITPPlan ?? 0;
                        break;
                    case EmployeeChangeType.CollectumAgreedOnProduct:
                        if (TryUpdateEmployeeReportField(employee, row, employee.CollectumAgreedOnProduct, out string collectumAgreedOnProduct))
                            employee.CollectumAgreedOnProduct = collectumAgreedOnProduct.NullToEmpty();
                        break;
                    case EmployeeChangeType.CollectumCostPlace:
                        if (TryUpdateEmployeeReportField(employee, row, employee.CollectumCostPlace, out string collectumCostPlace))
                            employee.CollectumCostPlace = collectumCostPlace.NullToEmpty();
                        break;
                    case EmployeeChangeType.CollectumCancellationDate:
                        if (TryUpdateEmployeeReportField(employee, row, employee.CollectumCancellationDate, out DateTime? collectumCancellationDate))
                            employee.CollectumCancellationDate = collectumCancellationDate;
                        break;
                    case EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence:
                        if (TryUpdateEmployeeReportField(employee, row, employee.CollectumCancellationDateIsLeaveOfAbsence, out bool? collectumCancellationDateIsLeaveOfAbsence))
                            employee.CollectumCancellationDateIsLeaveOfAbsence = collectumCancellationDateIsLeaveOfAbsence ?? false;
                        break;
                }
            }
        }

        private void SetReportSettingsKPA(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.KPARetirementAge:
                        if (TryUpdateEmployeeReportField(employee, row, employee.KpaRetirementAge, out int? kpaRetirementAge))
                            employee.KpaRetirementAge = kpaRetirementAge ?? 0;
                        break;
                    case EmployeeChangeType.KPABelonging:
                        if (TryUpdateEmployeeReportField(employee, row, employee.KpaBelonging, out int? kpaBelonging, TermGroup.KPABelonging))
                            employee.KpaBelonging = kpaBelonging ?? 0;
                        break;
                    case EmployeeChangeType.KPAEndCode:
                        if (TryUpdateEmployeeReportField(employee, row, employee.KpaEndCode, out int? kpaEndCode, TermGroup.KPAEndCode))
                            employee.KpaEndCode = kpaEndCode ?? 0;
                        break;
                    case EmployeeChangeType.KPAAgreementType:
                        if (TryUpdateEmployeeReportField(employee, row, employee.KpaAgreementType, out int? kpaAgreementType, TermGroup.KPAAgreementType))
                            employee.KpaAgreementType = kpaAgreementType ?? 0;
                        break;

                }
            }
        }

        private void SetReportSettingsBygglösen(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.BygglosenAgreementArea:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenAgreementArea, out string bygglosenAgreementArea))
                            employee.BygglosenAgreementArea = bygglosenAgreementArea.NullToEmpty();
                        break;
                    case EmployeeChangeType.BygglosenAllocationNumber:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenAllocationNumber, out string bygglosenAllocationNumber))
                            employee.BygglosenAllocationNumber = bygglosenAllocationNumber.NullToEmpty();
                        break;
                    case EmployeeChangeType.BygglosenMunicipalCode:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenMunicipalCode, out string bygglosenMunicipalCode))
                            employee.BygglosenMunicipalCode = bygglosenMunicipalCode.NullToEmpty();
                        break;
                    case EmployeeChangeType.BygglosenSalaryFormula:
                        if (TryUpdateBygglösenSalaryFormula(employee, row, employee.BygglosenSalaryFormula, out int? bygglosenSalaryFormula))
                            employee.BygglosenSalaryFormula = bygglosenSalaryFormula ?? 0;
                        break;
                    case EmployeeChangeType.BygglosenProfessionCategory:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenProfessionCategory, out string bygglosenProfessionCategory))
                            employee.BygglosenProfessionCategory = bygglosenProfessionCategory.NullToEmpty();
                        break;
                    case EmployeeChangeType.BygglosenSalaryType:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenSalaryType, out int? bygglosenSalaryType))
                            employee.BygglosenSalaryType = bygglosenSalaryType ?? 0;
                        break;
                    case EmployeeChangeType.BygglosenWorkPlaceNumber:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenWorkPlaceNumber, out string bygglosenWorkPlaceNumber))
                            employee.BygglosenWorkPlaceNumber = bygglosenWorkPlaceNumber.NullToEmpty();
                        break;
                    case EmployeeChangeType.BygglosenLendedToOrgNr:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenLendedToOrgNr, out string bygglosenLendedToOrgNr))
                            employee.BygglosenLendedToOrgNr = bygglosenLendedToOrgNr.NullToEmpty();
                        break;
                    case EmployeeChangeType.BygglosenAgreedHourlyPayLevel:
                        if (TryUpdateEmployeeReportField(employee, row, employee.BygglosenAgreedHourlyPayLevel, out decimal? bygglosenAgreedHourlyPayLevel))
                            employee.BygglosenAgreedHourlyPayLevel = bygglosenAgreedHourlyPayLevel ?? decimal.Zero;
                        break;
                }
            }
        }

        private void SetReportSettingsGTP(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.GTPAgreementNumber:
                        if (TryUpdateEmployeeReportField(employee, row, employee.GtpAgreementNumber, out int? gtpAgreementNumber, TermGroup.GTPAgreementNumber))
                            employee.GtpAgreementNumber = gtpAgreementNumber ?? 0;
                        break;
                    case EmployeeChangeType.GTPExcluded:
                        if (TryUpdateEmployeeReportField(employee, row, employee.GtpExcluded, out bool? gtpExcluded))
                            employee.GtpExcluded = gtpExcluded ?? false;
                        break;
                }
            }
        }

        private void SetReportSettingsAGI(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.AGIPlaceOfEmploymentAddress:
                        if (TryUpdateEmployeeReportField(employee, row, employee.AGIPlaceOfEmploymentAddress, out string agiPlaceOfEmploymentAddress))
                            employee.AGIPlaceOfEmploymentAddress = agiPlaceOfEmploymentAddress;
                        break;
                    case EmployeeChangeType.AGIPlaceOfEmploymentCity:
                        if (TryUpdateEmployeeReportField(employee, row, employee.AGIPlaceOfEmploymentCity, out string agiPlaceOfEmploymentCity))
                            employee.AGIPlaceOfEmploymentCity = agiPlaceOfEmploymentCity;
                        break;
                    case EmployeeChangeType.AGIPlaceOfEmploymentIgnore:
                        if (TryUpdateEmployeeReportField(employee, row, employee.AGIPlaceOfEmploymentIgnore, out bool? agiPlaceOfEmploymentIgnore))
                            employee.AGIPlaceOfEmploymentIgnore = agiPlaceOfEmploymentIgnore ?? false;
                        break;
                }
            }
        }

        private void SetReportSettingsIFMetall(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                switch (row.EmployeeChangeType)
                {
                    case EmployeeChangeType.IFAssociationNumber:
                        if (TryUpdateEmployeeReportField(employee, row, employee.IFAssociationNumber, out int? ifAssociationNumber))
                            employee.IFAssociationNumber = ifAssociationNumber ?? 0;
                        break;
                    case EmployeeChangeType.IFPaymentCode:
                        if (TryUpdateEmployeeReportField(employee, row, employee.IFPaymentCode, out int? ifPaymentCode))
                            employee.IFPaymentCode = ifPaymentCode ?? 0;
                        break;
                    case EmployeeChangeType.IFWorkPlace:
                        if (TryUpdateEmployeeReportField(employee, row, employee.IFWorkPlace, out string ifWorkPlace))
                            employee.IFWorkPlace = ifWorkPlace;
                        break;
                }
            }
        }

        private bool TryUpdateEmployeeReportField(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, string fromValue, out string toValue)
        {
            toValue = row.Delete ? null : row.Value;
            if (fromValue.IsNullOrEmpty() || !fromValue.Equals(toValue))
            {
                employee.AddCurrentChange((int)row.EmployeeChangeType, fromValue, toValue);
                return true;
            }
            return false;
        }

        private bool TryUpdateEmployeeReportField(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, DateTime? fromValue, out DateTime? toValue)
        {
            toValue = row.Delete ? null : StringUtility.GetNullableDateTime(row.Value);
            if (fromValue != toValue)
            {
                employee.AddCurrentChange((int)row.EmployeeChangeType, fromValue, toValue);
                return true;
            }
            return false;
        }

        private bool TryUpdateEmployeeReportField(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, bool? fromValue, out bool? toValue)
        {
            toValue = row.Delete ? null : StringUtility.GetNullableBool(row.Value);
            if (fromValue != toValue)
            {
                employee.AddCurrentChange((int)row.EmployeeChangeType, fromValue, toValue);
                return true;
            }
            return false;
        }
        private bool TryUpdateEmployeeReportField(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, decimal? fromValue, out decimal? toValue)
        {
            toValue = row.Delete ? null : StringUtility.GetNullableDecimal(row.Value);
            if (fromValue == null || !fromValue.Equals(toValue))
            {
                employee.AddCurrentChange((int)row.EmployeeChangeType, fromValue, toValue);
                return true;
            }
            return false;
        }
        private bool TryUpdateEmployeeReportField(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, int? fromValue, out int? toValue, TermGroup? termGroup = null)
        {
            toValue = row.Delete ? null : StringUtility.GetNullableInt(row.Value);
            if (fromValue == null || !fromValue.Equals(toValue))
            {
                employee.AddCurrentChange((int)row.EmployeeChangeType, fromValue, toValue, fromValueName: GetEmployeeReportTerm(termGroup, fromValue), toValueName: GetEmployeeReportTerm(termGroup, toValue));
                return true;
            }
            return false;
        }

        private bool TryUpdateBygglösenSalaryFormula(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, int? fromValue, out int? toValue)
        {
            PayrollPriceFormulaDTO payrollPriceFormula = GetPayrollPriceFormula(row);
            toValue = row.Delete || payrollPriceFormula == null ? null : (int?)payrollPriceFormula.PayrollPriceFormulaId;
            if (fromValue == null || !fromValue.Equals(toValue))
            {
                employee.AddCurrentChange((int)row.EmployeeChangeType, fromValue, toValue);
                return true;
            }
            return false;
        }


        private PayrollPriceFormulaDTO GetPayrollPriceFormula(EmployeeChangeRowIODTO row)
        {
            return row != null ? this.lookup.PayrollPriceFormulas?.FirstOrDefault(f => f.Name.Equals(row.Value)) : null;
        }

        #endregion

        #region Employee - Positions

        private void SetEmployeePositions(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows)
        {
            if (employee == null || rows.IsNullOrEmpty())
                return;

            foreach (EmployeeChangeRowIODTO row in rows.Where(r => r.EmployeeChangeType == EmployeeChangeType.EmployeePosition))
            {
                PositionDTO position = GetEmployeePosition(row);
                if (position == null)
                {
                    AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValuePositionNotFound);
                    continue;
                }

                if (row.Delete)
                {
                    DeleteEmployeePosition(employee, position);
                }
                else
                {
                    bool isDefault = rows.Any(r => r.EmployeeChangeType == EmployeeChangeType.EmployeePositionDefault && r.Value == row.Value);

                    EmployeePositionDTO employeePosition = this.lookup.EmployeePositionsByEmployee.GetList(employee.EmployeeNr)?.FirstOrDefault(p => p.PositionId == position.PositionId);
                    if (employeePosition == null)
                        employeePosition = AddEmployeePosition(employee, position, isDefault);
                    else
                        UpdateEmployeePosition(employee, position, employeePosition, isDefault);

                    if (isDefault)
                        UpdateOtherEmployeePositionToNotStandard(employee, employeePosition);
                }
            }
        }

        private PositionDTO GetEmployeePosition(EmployeeChangeRowIODTO row)
        {
            if (row.Value == null)
                return null;

            PositionDTO position = this.lookup.EmployeePositions?.FirstOrDefault(p => !string.IsNullOrEmpty(p.Code) && p.Code.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (position == null)
                this.lookup.EmployeePositions?.FirstOrDefault(p => !string.IsNullOrEmpty(p.Description) && p.Description.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (position == null)
                position = this.lookup.EmployeePositions?.FirstOrDefault(p => !string.IsNullOrEmpty(p.Name) && p.Name.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (position == null && row.Value.Length > 3)
                position = this.lookup.EmployeePositions?.FirstOrDefault(p => !string.IsNullOrEmpty(p.Description) && p.Description.Equals(row.Value, StringComparison.OrdinalIgnoreCase));

            // Employee templates use ID
            if (position == null && Int32.TryParse(row.Value, out int positionId))
                position = this.lookup.EmployeePositions?.FirstOrDefault(p => p.PositionId == positionId);

            return position;
        }

        private EmployeePositionDTO AddEmployeePosition(EmployeeUserDTO employee, PositionDTO position, bool isDefault)
        {
            if (employee == null || position == null)
                return null;

            EmployeePositionDTO employeePosition = new EmployeePositionDTO()
            {
                PositionId = position.PositionId,
                Default = isDefault,
                EmployeePositionName = position.Name,
            };

            if (this.lookup.EmployeePositionsByEmployee.ContainsKey(employee.EmployeeNr))
            {
                var employeePositions = this.lookup.EmployeePositionsByEmployee[employee.EmployeeNr];
                employeePositions.Add(employeePosition);
                this.lookup.EmployeePositionsByEmployee[employee.EmployeeNr] = employeePositions;
            }
            else
            {
                this.lookup.EmployeePositionsByEmployee.Add(employee.EmployeeNr, new List<EmployeePositionDTO> { employeePosition });
            }

            employee.AddCurrentChange((int)EmployeeChangeType.EmployeePosition, 0, position.PositionId, toValueName: GetPositionName(position.Name, isDefault));
            return employeePosition;
        }

        private void UpdateEmployeePosition(EmployeeUserDTO employee, PositionDTO position, EmployeePositionDTO employeePosition, bool isDefault)
        {
            if (employee == null || position == null || employeePosition == null)
                return;

            if (employeePosition.Default != isDefault)
                UpdateEmployeePositionDefault(employee, employeePosition, isDefault);
        }

        private void UpdateOtherEmployeePositionToNotStandard(EmployeeUserDTO employee, EmployeePositionDTO employeePosition)
        {
            if (employee == null || employeePosition == null)
                return;

            List<EmployeePositionDTO> employeePositions = this.lookup.EmployeePositionsByEmployee.GetList(employee.EmployeeNr).Where(e => e.Default && e.PositionId != employeePosition.PositionId).ToList();
            foreach (EmployeePositionDTO otherEmployeePosition in employeePositions)
            {
                UpdateEmployeePositionDefault(employee, otherEmployeePosition, false);
            }
        }

        private void UpdateEmployeePositionDefault(EmployeeUserDTO employee, EmployeePositionDTO employeePosition, bool isDefault)
        {
            if (employee == null || employeePosition == null)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.EmployeePosition, employeePosition.PositionId, employeePosition.PositionId, GetPositionName(employeePosition.EmployeePositionName, employeePosition.Default), GetPositionName(employeePosition.EmployeePositionName, isDefault));
            employeePosition.Default = isDefault;
        }

        private void DeleteEmployeePosition(EmployeeUserDTO employee, PositionDTO position)
        {
            if (employee == null || position == null)
                return;

            EmployeePositionDTO employeePosition = this.lookup.EmployeePositionsByEmployee.GetList(employee.EmployeeNr)?.FirstOrDefault(i => i.PositionId == position.PositionId);
            if (employeePosition == null)
                return;

            this.lookup.EmployeePositionsByEmployee[employee.EmployeeNr] = this.lookup.EmployeePositionsByEmployee[employee.EmployeeNr].Where(i => i.PositionId != employeePosition.PositionId).ToList();
            employee.AddCurrentChange((int)EmployeeChangeType.EmployeePosition, position.PositionId, 0, fromValueName: position.Name);
        }

        private string GetPositionName(string name, bool isDefault)
        {
            string standard = isDefault ? "Standard" : "Ej standard";
            return $"{name} ({standard})";
        }

        #endregion

        #region Employee - Tax

        private void SetEmployeeTaxRate(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!int.TryParse(row.Value, out int taxRate))
                return;

            EmployeeTaxSEDTO employeeTax = GetOrCreateEmployeeTax(employee, row);
            if (employeeTax != null)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.TaxRate, "0", taxRate.ToString());
                employeeTax.TaxRate = taxRate;

                // TODO: Not used?
                employee.TempTaxRate = taxRate;
            }
        }


        private void SetEmployeeTaxTinNumber(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EmployeeTaxSEDTO employeeTax = GetOrCreateEmployeeTax(employee, row);
            if (employeeTax != null)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.TaxTinNumber, employeeTax.TinNumber, row.Value);
                employeeTax.TinNumber = row.Value;
            }
        }

        private void SetEmployeeTaxCountryCode(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EmployeeTaxSEDTO employeeTax = GetOrCreateEmployeeTax(employee, row);
            if (employeeTax != null)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.TaxCountryCode, employeeTax.CountryCode, row.Value);
                employeeTax.CountryCode = row.Value;
            }
        }

        private void SetEmployeeTaxBirthPlace(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EmployeeTaxSEDTO employeeTax = GetOrCreateEmployeeTax(employee, row);
            if (employeeTax != null)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.TaxBirthPlace, employeeTax.BirthPlace, row.Value);
                employeeTax.BirthPlace = row.Value;
            }
        }

        private void SetEmployeeTaxCountryCodeBirthPlace(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EmployeeTaxSEDTO employeeTax = GetOrCreateEmployeeTax(employee, row);
            if (employeeTax != null)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.TaxCountryCodeBirthPlace, employeeTax.CountryCodeBirthPlace, row.Value);
                employeeTax.CountryCodeBirthPlace = row.Value;
            }
        }

        private void SetEmployeeTaxCountryCodeCitizen(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            EmployeeTaxSEDTO employeeTax = GetOrCreateEmployeeTax(employee, row);
            if (employeeTax != null)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.TaxCountryCodeCitizen, employeeTax.CountryCodeCitizen, row.Value);
                employeeTax.CountryCodeCitizen = row.Value;
            }
        }

        private EmployeeTaxSEDTO GetOrCreateEmployeeTax(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee.EmployeeTax == null)
            {
                DateTime date = ParseFromDate(row);
                if (date.Year == 1900)
                    date = DateTime.Today;

                employee.EmployeeTax = new EmployeeTaxSEDTO()
                {
                    Year = date.Year,
                    MainEmployer = true,
                    Type = TermGroup_EmployeeTaxType.TableTax,
                    EmploymentTaxType = TermGroup_EmployeeTaxEmploymentTaxType.EmploymentTax
                };

            }

            return employee.EmployeeTax;
        }

        #endregion

        #region Employee - Template

        private void SetEmployeeTemplateId(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!int.TryParse(row.Value, out int employeeTemplateId))
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.EmployeeTemplateId, employee.EmployeeTemplateId.ToString(), row.Value);
            employee.EmployeeTemplateId = employeeTemplateId;
        }

        #endregion

        #region Employee - TimeWorkAccount

        private void SetTimeWorkAccounts(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows, List<EmployeeTimeWorkAccountDTO> employeeTimeWorkAccounts)
        {
            if (employee == null || rows.IsNullOrEmpty() || employeeTimeWorkAccounts == null)
                return;

            foreach (EmployeeChangeRowIODTO row in rows)
            {
                DateTime? fromDate = ParseNullableFromDate(row);
                DateTime? toDate = ParseToDate(row);

                TimeWorkAccountDTO timeWorkAccount = GetTimeWorkAccount(row);
                if (timeWorkAccount == null)
                {
                    AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueTimeWorkAccountNotFound);
                    return;
                }

                #region Delete

                if (row.Delete)
                {
                    DeleteTimeWorkAccount(employee, employeeTimeWorkAccounts, timeWorkAccount);
                    continue;
                }

                #endregion

                #region Add/Update

                EmployeeTimeWorkAccountDTO employeeTimeWorkAccount = employeeTimeWorkAccounts.FirstOrDefault(a => a.TimeWorkAccountId == timeWorkAccount.TimeWorkAccountId);
                if (employeeTimeWorkAccounts.IsOverlapping(employeeTimeWorkAccount?.Key ?? Guid.NewGuid(), fromDate, toDate))
                {
                    AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueTimeWorkOverlapping);
                    return;
                }

                if (employeeTimeWorkAccount != null)
                    UpdateTimeWorkAccount(employee, row, employeeTimeWorkAccount, timeWorkAccount, fromDate, toDate);
                else
                    AddTimeWorkAccount(employee, employeeTimeWorkAccounts, timeWorkAccount, fromDate, toDate);

                #endregion
            }
        }

        private TimeWorkAccountDTO GetTimeWorkAccount(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return null;

            return this.lookup.TimeWorkAccounts?.FirstOrDefault(ar => !string.IsNullOrEmpty(ar.Name) && ar.Name.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
        }

        private void AddTimeWorkAccount(EmployeeUserDTO employee, List<EmployeeTimeWorkAccountDTO> employeeTimeWorkAccounts, TimeWorkAccountDTO timeWorkAccount, DateTime? dateFrom, DateTime? dateTo)
        {
            if (employee == null || employeeTimeWorkAccounts == null || timeWorkAccount == null)
                return;
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
                return;

            EmployeeTimeWorkAccountDTO employeeTimeWorkAccount = new EmployeeTimeWorkAccountDTO()
            {
                TimeWorkAccountId = timeWorkAccount.TimeWorkAccountId,
                EmployeeId = employee.EmployeeId,
                ActorCompanyId = employee.ActorCompanyId,
            };

            employeeTimeWorkAccount.DateFrom = dateFrom?.Date;
            employeeTimeWorkAccount.DateTo = dateTo?.Date;
            employeeTimeWorkAccounts.Add(employeeTimeWorkAccount);

            employee.AddCurrentChange((int)EmployeeChangeType.TimeWorkAccount, null, timeWorkAccount.Name, fromDate: employeeTimeWorkAccount.DateFrom, toDate: employeeTimeWorkAccount.DateTo);
        }

        private void UpdateTimeWorkAccount(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, EmployeeTimeWorkAccountDTO employeeTimeWorkAccount, TimeWorkAccountDTO timeWorkAccount, DateTime? dateFrom, DateTime? dateTo)
        {
            if (employee == null || row == null || employeeTimeWorkAccount == null || timeWorkAccount == null)
                return;
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
                return;

            UpdateTimeWorkAccountDateFrom(employee, employeeTimeWorkAccount, timeWorkAccount, dateFrom);
            UpdateTimeWorkAccountDateTo(employee, employeeTimeWorkAccount, timeWorkAccount, dateTo);
        }

        private void UpdateTimeWorkAccountDateFrom(EmployeeUserDTO employee, EmployeeTimeWorkAccountDTO employeeTimeWorkAccount, TimeWorkAccountDTO timeWorkAccount, DateTime? date)
        {
            if (employee == null || employeeTimeWorkAccount == null || timeWorkAccount == null)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.TimeWorkAccount, employeeTimeWorkAccount.DateFrom, date, timeWorkAccount.Name, timeWorkAccount.Name, "Fr.o.m", date, employeeTimeWorkAccount.DateTo);
            employeeTimeWorkAccount.DateFrom = date;
        }

        private void UpdateTimeWorkAccountDateTo(EmployeeUserDTO employee, EmployeeTimeWorkAccountDTO employeeTimeWorkAccount, TimeWorkAccountDTO timeWorkAccount, DateTime? date)
        {
            if (employee == null || employeeTimeWorkAccount == null || timeWorkAccount == null)
                return;

            employee.AddCurrentChange((int)EmployeeChangeType.TimeWorkAccount, employeeTimeWorkAccount.DateTo, date, timeWorkAccount.Name, timeWorkAccount.Name, "T.o.m", employeeTimeWorkAccount.DateFrom, date);
            employeeTimeWorkAccount.DateTo = date;
        }

        private void DeleteTimeWorkAccount(EmployeeUserDTO employee, List<EmployeeTimeWorkAccountDTO> employeeTimeWorkAccounts, TimeWorkAccountDTO timeWorkAccount)
        {
            if (employee == null || employeeTimeWorkAccounts == null || timeWorkAccount == null)
                return;

            EmployeeTimeWorkAccountDTO employeeTimeWorkAccount = employeeTimeWorkAccounts.FirstOrDefault(e => e.TimeWorkAccountId == timeWorkAccount.TimeWorkAccountId && e.State == SoeEntityState.Active);
            if (employeeTimeWorkAccount == null)
                return;

            employeeTimeWorkAccount.State = SoeEntityState.Deleted;
            employee.AddCurrentChange((int)EmployeeChangeType.TimeWorkAccount, timeWorkAccount.TimeWorkAccountId, 0, fromValueName: timeWorkAccount.Name);
        }

        #endregion

        #region Employee - VacationSE

        private void SetVacationDaysPaid(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!decimal.TryParse(row.Value, out decimal vacationDaysPaid))
                return;

            if (employee.EmployeeVacationSE == null)
                AddVacationDaysPaid(employee, vacationDaysPaid);
            else if (employee.EmployeeVacationSE.RemainingDaysPaid != vacationDaysPaid)
                UpdateVacationDaysPaid(employee, vacationDaysPaid);

            employee.AddCurrentChange((int)EmployeeChangeType.VacationDaysPaid, employee.EmployeeVacationSE?.PaidVacationAllowance?.ToString() ?? "0", row.Value);
        }

        private void SetVacationDaysUnPaid(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!decimal.TryParse(row.Value, out decimal vacationDaysUnPaid))
                return;

            if (employee.EmployeeVacationSE == null)
                AddVacationDaysUnPaid(employee, vacationDaysUnPaid);
            else if (employee.EmployeeVacationSE.RemainingDaysUnpaid != vacationDaysUnPaid)
                UpdateVacationDaysUnPaid(employee, vacationDaysUnPaid);

            employee.AddCurrentChange((int)EmployeeChangeType.VacationDaysUnPaid, employee.EmployeeVacationSE?.RemainingDaysUnpaid?.ToString() ?? "0", row.Value);
        }

        private void SetVacationDaysAdvance(EmployeeUserDTO employee, EmployeeChangeRowIODTO row)
        {
            if (employee == null || row == null)
                return;

            if (!decimal.TryParse(row.Value, out decimal vacationDaysAdvance))
                return;

            if (employee.EmployeeVacationSE == null)
                AddVacationDaysAdvance(employee, vacationDaysAdvance);
            else if (employee.EmployeeVacationSE.RemainingDaysAdvance != vacationDaysAdvance)
                UpdateVacationDaysAdvance(employee, vacationDaysAdvance);

            employee.AddCurrentChange((int)EmployeeChangeType.VacationDaysAdvance, employee.EmployeeVacationSE?.RemainingDaysAdvance?.ToString() ?? "0", row.Value);
        }

        private void AddVacationDaysPaid(EmployeeUserDTO employee, decimal vacationDaysPaid)
        {
            if (employee.EmployeeVacationSE == null)
                employee.EmployeeVacationSE = new EmployeeVacationSEDTO();

            employee.EmployeeVacationSE.RemainingDaysPaid = vacationDaysPaid;
        }

        private void UpdateVacationDaysPaid(EmployeeUserDTO employee, decimal vacationDaysPaid)
        {
            AddVacationDaysPaid(employee, vacationDaysPaid);
        }

        private void AddVacationDaysUnPaid(EmployeeUserDTO employee, decimal vacationDaysUnPaid)
        {
            if (employee.EmployeeVacationSE == null)
                employee.EmployeeVacationSE = new EmployeeVacationSEDTO();

            employee.EmployeeVacationSE.RemainingDaysUnpaid = vacationDaysUnPaid;
        }

        private void UpdateVacationDaysUnPaid(EmployeeUserDTO employee, decimal vacationDaysUnPaid)
        {
            AddVacationDaysUnPaid(employee, vacationDaysUnPaid);
        }

        private void AddVacationDaysAdvance(EmployeeUserDTO employee, decimal vacationDaysAdvance)
        {
            if (employee.EmployeeVacationSE == null)
                employee.EmployeeVacationSE = new EmployeeVacationSEDTO();

            employee.EmployeeVacationSE.RemainingDaysAdvance = vacationDaysAdvance;
        }

        private void UpdateVacationDaysAdvance(EmployeeUserDTO employee, decimal vacationDaysAdvance)
        {
            AddVacationDaysAdvance(employee, vacationDaysAdvance);
        }

        #endregion

        #endregion

        #region User

        private void SetSaveUser()
        {
            this.SaveUser = true;
        }

        #endregion

        #region UserRole

        private void SetUserCompanyRoles(EmployeeUserDTO employee, List<EmployeeChangeRowIODTO> rows, UserRolesDTO userRole)
        {
            if (employee == null || userRole == null || rows.IsNullOrEmpty())
                return;

            bool doSetDefaultCompanyWhenUpdatingRoles = GetSettingDoSetDefaultCompanyWhenUpdatingRoles();

            foreach (EmployeeChangeRowIODTO row in rows.Where(r => r.EmployeeChangeType == EmployeeChangeType.UserRole))
            {
                RoleDTO role = GetRole(row);
                if (role == null)
                {
                    AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidValueRoleNotFound);
                    continue;
                }

                if (row.Delete)
                {
                    DeleteUserRole(employee, userRole, role);
                }
                else
                {
                    DateTime? fromDate = ParseNullableFromDate(row);
                    DateTime? toDate = ParseToDate(row);
                    bool isDefault = rows.Any(r => r.EmployeeChangeType == EmployeeChangeType.DefaultUserRole && r.Value == row.Value);

                    UserCompanyRoleDTO userCompanyRole = userRole.Roles?.Where(i => i.RoleId == role.RoleId).OrderByDescending(i => i.DateTo ?? DateTime.MaxValue).FirstOrDefault();
                    if (userCompanyRole == null || fromDate > userCompanyRole.DateTo)
                        AddUserRole(employee, userRole, role, fromDate, toDate);
                    else
                        UpdateUserRole(employee, role, userCompanyRole, fromDate, toDate);

                    if (isDefault || role.RoleId == this.lookup.DefaultRoleId || GetSettingSetChefToDefaultRole(employee, role))
                        SetDefaultUserRole(employee, row, userRole, role, fromDate, toDate);
                }

                if (doSetDefaultCompanyWhenUpdatingRoles && !userRole.DefaultCompany)
                {
                    userRole.DefaultCompany = true;
                    employee.UserRoles.Where(r => r.ActorCompanyId != userRole.ActorCompanyId).ToList().ForEach(r => r.DefaultCompany = false);
                }
            }

            SetSaveUser();
        }

        private UserRolesDTO GetUserRoles(EmployeeUserDTO employee)
        {
            if (employee == null)
                return null;

            UserRolesDTO userRole = employee.UserRoles?.FirstOrDefault(i => i.ActorCompanyId == employee.ActorCompanyId);
            if (userRole == null)
            {
                userRole = new UserRolesDTO()
                {
                    ActorCompanyId = employee.ActorCompanyId
                };
                if (employee.UserRoles == null)
                    employee.UserRoles = new List<UserRolesDTO>();
                employee.UserRoles.Add(userRole);
            }
            userRole.IsDeltaChange = true;

            return userRole;
        }

        private RoleDTO GetRole(EmployeeChangeRowIODTO row)
        {
            if (row == null)
                return null;

            var userRole = this.lookup.UserRoles?.FirstOrDefault(f => !string.IsNullOrEmpty(f.ActualName) && f.ActualName.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (userRole == null)
                userRole = this.lookup.UserRoles?.FirstOrDefault(f => f.ExternalCodes != null && f.ExternalCodes.Where(w => !string.IsNullOrEmpty(w)).Select(s => s.Trim().ToLower()).Contains(row.Value.Trim().ToLower()));

            return userRole;
        }

        private void SetDefaultUserRole(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, UserRolesDTO userRole, RoleDTO role, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employee == null || row == null || userRole == null || role == null)
                return;

            UserCompanyRoleDTO userCompanyRole;
            if (userRole.Roles?.Count(i => i.RoleId == role.RoleId) == 1)
                userCompanyRole = userRole.Roles?.FirstOrDefault(i => i.RoleId == role.RoleId);
            else
                userCompanyRole = userRole.Roles?.FirstOrDefault(i => i.RoleId == role.RoleId && i.DateFrom == fromDate && i.DateTo == toDate);
            if (userCompanyRole == null)
            {
                AddValidationError(employee, row, row.Value, EmploymentChangeValidationError.InvalidDefaultUserRole);
                return;
            }

            if (!userCompanyRole.Default)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.DefaultUserRole, 0, 1, toValueName: role.Name, fromDate: fromDate, toDate: toDate);
                userCompanyRole.Default = true;
                userCompanyRole.IsModified = true;
            }
        }

        private void AddUserRole(EmployeeUserDTO employee, UserRolesDTO userRole, RoleDTO role, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employee == null || userRole == null || role == null)
                return;

            var userCompanyRole = new UserCompanyRoleDTO()
            {
                UserId = employee.UserId,
                RoleId = role.RoleId,
                ActorCompanyId = this.lookup.ActorCompanyId,
                DateFrom = fromDate,
                DateTo = toDate,
                IsModified = true
            };

            if (userRole.Roles == null)
                userRole.Roles = new List<UserCompanyRoleDTO>();
            userRole.Roles.Add(userCompanyRole);
            employee.AddCurrentChange((int)EmployeeChangeType.UserRole, 0, role.RoleId, toValueName: role.Name, fromDate: fromDate, toDate: toDate);
        }

        private void UpdateUserRole(EmployeeUserDTO employee, RoleDTO role, UserCompanyRoleDTO userCompanyRole, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (employee == null || role == null || userCompanyRole == null)
                return;

            if (userCompanyRole.DateFrom != fromDate)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.UserRole, userCompanyRole.DateFrom, fromDate, role.Name, role.Name, valuePrefix: "Fr.o.m", fromDate: fromDate, toDate: userCompanyRole.DateTo);
                userCompanyRole.DateFrom = fromDate;
                userCompanyRole.IsModified = true;
            }
            if (userCompanyRole.DateTo != toDate)
            {
                employee.AddCurrentChange((int)EmployeeChangeType.UserRole, userCompanyRole.DateTo, toDate, role.Name, role.Name, valuePrefix: "T.o.m", fromDate: userCompanyRole.DateFrom, toDate: toDate);
                userCompanyRole.DateTo = toDate;
                userCompanyRole.IsModified = true;
            }
        }

        private void DeleteUserRole(EmployeeUserDTO employee, UserRolesDTO userRole, RoleDTO role)
        {
            if (employee == null || userRole == null || role == null)
                return;

            UserCompanyRoleDTO existingRole = userRole.Roles?.FirstOrDefault(i => i.RoleId == role.RoleId);
            if (existingRole == null)
                return;

            existingRole.State = SoeEntityState.Deleted;
            existingRole.IsModified = true;
            employee.AddCurrentChange((int)EmployeeChangeType.UserRole, role.RoleId, 0, fromValueName: role.Name);
        }

        #endregion

        #endregion

        #region Account

        private AccountDTO GetAccountByOptionalExternalCode(EmployeeChangeRowIODTO row)
        {
            var value = row.OptionalExternalCode;

            if (string.IsNullOrEmpty(value))
                return null;

            if (row.OptionalExternalCode.Contains("|") && row.OptionalExternalCode.Split('|').Count() > 1)
                value = row.OptionalExternalCode.Split('|')[0];

            return GetAccount(value);
        }

        private AccountDTO GetAccount(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var account = this.lookup.AccountInternals?.FirstOrDefault(f => !string.IsNullOrEmpty(f.ExternalCode) && f.ExternalCode.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (account == null)
                account = this.lookup.AccountInternals?.FirstOrDefault(f => !string.IsNullOrEmpty(f.AccountNr) && f.AccountNr.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (account == null)
                account = this.lookup.AccountInternals?.FirstOrDefault(f => !string.IsNullOrEmpty(f.Name) && f.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (account == null && int.TryParse(value, out int accountId))
            {
                if (value.StartsWith("0"))
                    account = this.lookup.AccountInternals?.FirstOrDefault(f => !string.IsNullOrEmpty(f.AccountNr) && f.AccountNr.Equals(accountId.ToString(), StringComparison.OrdinalIgnoreCase));

                if (account == null)
                    account = this.lookup.AccountInternals.FirstOrDefault(f => f.AccountId == accountId);
            }

            return account;
        }

        private AccountDTO GetAccount(int accountId)
        {
            return this.lookup.AccountInternals?.FirstOrDefault(i => i.AccountId == accountId);
        }

        private AccountDTO GetAccountBySieDim(EmployeeChangeRowIODTO row, int sieDimNr)
        {
            if (row == null)
                return null;

            var account = this.lookup.AccountInternals?.FirstOrDefault(f => f.AccountDim.SysSieDimNr == sieDimNr && !string.IsNullOrEmpty(f.ExternalCode) && f.ExternalCode.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (account == null)
                account = this.lookup.AccountInternals?.FirstOrDefault(f => f.AccountDim.SysSieDimNr == sieDimNr && !string.IsNullOrEmpty(f.AccountNr) && f.AccountNr.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (account == null)
                account = this.lookup.AccountInternals?.FirstOrDefault(f => f.AccountDim.SysSieDimNr == sieDimNr && !string.IsNullOrEmpty(f.Name) && f.Name.Equals(row.Value, StringComparison.OrdinalIgnoreCase));
            if (account == null && row.Delete)
                account = this.lookup.AccountInternals?.FirstOrDefault(f => f.AccountDim.SysSieDimNr == sieDimNr);

            return account;
        }

        private int GetAccountDimNrFromAccountSetting(int accountDimId, AccountingSettingsRowDTO accountSetting)
        {
            if (accountSetting == null)
                return 0;

            int dimNr = GetAccountDimNr(accountDimId);
            if (ExistsAccount(accountDimId, accountSetting.Account2Id))
                dimNr = 2;
            if (ExistsAccount(accountDimId, accountSetting.Account3Id))
                dimNr = 3;
            if (ExistsAccount(accountDimId, accountSetting.Account4Id))
                dimNr = 4;
            if (ExistsAccount(accountDimId, accountSetting.Account5Id))
                dimNr = 5;
            if (ExistsAccount(accountDimId, accountSetting.Account6Id))
                dimNr = 6;

            return dimNr;
        }

        private int GetAccountDimNr(int accountDimId)
        {
            int accountDimCounter = 1;
            List<AccountDimDTO> accountDims = this.lookup.AccountDims ?? new List<AccountDimDTO>();
            foreach (AccountDimDTO accountDim in accountDims.OrderBy(a => a.AccountDimNr))
            {
                if (accountDimId == accountDim.AccountDimId)
                    return accountDimCounter;

                accountDimCounter++;
            }

            return 0;
        }

        private bool ExistsAccount(int accountDimId, int accountId)
        {
            if (accountId == 0)
                return false;
            return this.lookup.AccountInternals?.Any(a => a.AccountDimId == accountDimId && a.AccountId == accountId) ?? false;
        }

        #endregion

        #region ApiSettings

        public bool GetBoolSetting(TermGroup_ApiSettingType setting)
        {
            return this.lookup.Settings.GetBool(setting);
        }

        public void ApplyKeeepEmploymentSettings(EmployeeUserDTO employee)
        {
            if (employee?.Employments == null || this.lookup.Settings.IsNullOrEmpty())
                return;

            List<EmploymentDTO> employments = employee.Employments.Where(e => e.State != SoeEntityState.Deleted).OrderBy(e => e.DateFrom).ToList();
            if (employments.IsNullOrEmpty())
                return;

            EmploymentDTO prevEmployment = null;
            foreach (EmploymentDTO employment in employments)
            {
                if (prevEmployment != null)
                {
                    if (GetSettingKeepEmploymentAccount(employment, prevEmployment))
                        prevEmployment.AccountingSettings.ForEach(accountingSetting => CopyEmploymentAccounts(employment, accountingSetting));
                    if (GetSettingKeepEmploymentPriceType(employment, prevEmployment))
                        prevEmployment.PriceTypes.ForEach(priceType => CopyEmploymentPriceType(employee, employment, priceType));
                }
                prevEmployment = employment;
            }
        }

        private bool GetSettingKeepEmploymentAccount(EmploymentDTO employment, EmploymentDTO prevEmployment)
        {
            if (employment == null || prevEmployment == null || employment.EmploymentId > 0)
                return false;
            if (!this.lookup.Settings.GetBool(TermGroup_ApiSettingType.KeepEmploymentAccount))
                return false;
            if (!employment.AccountingSettings.IsNullOrEmpty() || prevEmployment.AccountingSettings.IsNullOrEmpty())
                return false;
            return true;
        }

        private bool GetSettingKeepEmploymentPriceType(EmploymentDTO employment, EmploymentDTO prevEmployment)
        {
            if (employment == null || prevEmployment == null || employment.EmploymentId > 0)
                return false;
            if (!this.lookup.Settings.GetBool(TermGroup_ApiSettingType.KeepEmploymentPriceType))
                return false;
            if (!employment.PriceTypes.IsNullOrEmpty() || prevEmployment.PriceTypes.IsNullOrEmpty())
                return false;
            return true;
        }

        private bool GetSettingSetChefToDefaultRole(EmployeeUserDTO employee, RoleDTO role)
        {
            if (employee == null || role == null)
                return false;
            if (this.lookup.Settings.GetBool(TermGroup_ApiSettingType.DoNotSetChiefToStandard))
                return false;
            return role.Name.ToLower().Contains("chef") || role.Name.ToLower().Contains("chief");
        }

        private bool GetSettingAutoCloseEmployeeAccountAndAttestRole(EmployeeUserDTO employee, EmploymentDTO employment)
        {
            if (employee == null || employment == null || employment.EmploymentId == 0)
                return false;
            if (this.lookup.Settings.GetBool(TermGroup_ApiSettingType.DoNotCloseEmployeeAccountAndAttestRole))
                return false;
            return employment.DateTo.HasValue;
        }

        private int? GetSettingAccountDimIdForEmployeeAccount(EmployeeUserDTO employee)
        {
            if (employee == null)
                return null;
            return this.lookup.Settings.GetNullableInt(TermGroup_ApiSettingType.AccountDimIdForEmployeeAccount).ToNullable();
        }

        private bool GetSettingDoSetDefaultCompanyWhenUpdatingRoles()
        {
            return this.lookup.Settings.GetBool(TermGroup_ApiSettingType.DoSetDefaultCompanyWhenUpdatingRoles);
        }

        #endregion

        #region ApiTerms

        private string GetValidationErrorTerm(EmploymentChangeValidationError error)
        {
            return GetTermValue((int)error);
        }

        private string GetTermValue(int key)
        {
            return GetTermValue(TermGroup.ApiEmployee, key);
        }

        private string GetTermValue(TermGroup termGroup, int key)
        {
            return GetTerms(termGroup)?.FirstOrDefault(t => t.Id == key)?.Name ?? key.ToString();
        }

        private List<GenericType> GetTerms(TermGroup termGroup)
        {
            if (this.lookup?.Terms != null && this.lookup.Terms.TryGetValue(termGroup, out List<GenericType> terms))
                return terms;
            else
                return null;
        }

        private string GetEmployeeReportTerm(TermGroup? termGroup, int? id)
        {
            return termGroup.HasValue && id.HasValue ? GetTermValue(termGroup.Value, id.Value) : null;
        }

        #endregion

        #region EmploymentChange

        private bool IsEqual(EmploymentDTO employment, TermGroup_EmploymentChangeFieldType type, int from, int to, DateTime? fromDate, DateTime? toDate) =>
            GetLastValidChange(employment, type, fromDate, toDate).EqualsToInt(to) ||
            HasNotAnyChange(employment, type, from == to, fromDate, toDate);
        private bool IsEqual(EmploymentDTO employment, TermGroup_EmploymentChangeFieldType type, decimal from, decimal to, DateTime? fromDate, DateTime? toDate) =>
            GetLastValidChange(employment, type, fromDate, toDate).EqualsToDecimal(to) ||
            HasNotAnyChange(employment, type, from == to, fromDate, toDate);
        private bool IsEqual(EmploymentDTO employment, TermGroup_EmploymentChangeFieldType type, string from, string to, DateTime? fromDate, DateTime? toDate) =>
            GetLastValidChange(employment, type, fromDate, toDate).EqualsToStringIgnoreCase(to) ||
            HasNotAnyChange(employment, type, from.EqualsToStringIgnoreCase(to), fromDate, toDate);

        private bool HasNotAnyChange(EmploymentDTO employment, TermGroup_EmploymentChangeFieldType type, bool isValueEqual, DateTime? fromDate, DateTime? toDate)
            => isValueEqual && employment != null && !employment.HasChange(type, true) && employment.IsDatesEqual(fromDate, toDate);
        private string GetLastValidChange(EmploymentDTO employment, TermGroup_EmploymentChangeFieldType type, DateTime? fromDate, DateTime? toDate)
            => employment?.GetLastValidChange(type, fromDate, toDate)?.ToValue;
        private bool DoSetValueOnEmployment(EmploymentDTO employment, DateTime? fromDate)
            => employment?.EmploymentId == 0 && employment.DateFrom == fromDate;

        #endregion

        #region EmployeeChangeType

        public static EmployeeChangeType[] GetTypesEmployeePosition()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.EmployeePosition,
                EmployeeChangeType.EmployeePositionDefault,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesUserRole()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.UserRole,
                EmployeeChangeType.DefaultUserRole,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesPayrollStatistics()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.PayrollStatisticsPersonalCategory,
                EmployeeChangeType.PayrollStatisticsWorkTimeCategory,
                EmployeeChangeType.PayrollStatisticsSalaryType,
                EmployeeChangeType.PayrollStatisticsWorkPlaceNumber,
                EmployeeChangeType.PayrollStatisticsCFARNumber,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesControlTask()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.ControlTaskWorkPlaceSCB,
                EmployeeChangeType.ControlTaskPartnerInCloseCompany,
                EmployeeChangeType.ControlTaskBenefitAsPension,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesAFA()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.AFACategory,
                EmployeeChangeType.AFASpecialAgreement,
                EmployeeChangeType.AFAWorkplaceNr,
                EmployeeChangeType.AFAParttimePensionCode,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesCollectum()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.CollectumITPPlan,
                EmployeeChangeType.CollectumAgreedOnProduct,
                EmployeeChangeType.CollectumCostPlace,
                EmployeeChangeType.CollectumCancellationDate,
                EmployeeChangeType.CollectumCancellationDateIsLeaveOfAbsence,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesKPA()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.KPARetirementAge,
                EmployeeChangeType.KPABelonging,
                EmployeeChangeType.KPAEndCode,
                EmployeeChangeType.KPAAgreementType,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesBygglösen()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.BygglosenAgreementArea,
                EmployeeChangeType.BygglosenAllocationNumber,
                EmployeeChangeType.BygglosenSalaryFormula,
                EmployeeChangeType.BygglosenMunicipalCode,
                EmployeeChangeType.BygglosenProfessionCategory,
                EmployeeChangeType.BygglosenSalaryType,
                EmployeeChangeType.BygglosenWorkPlaceNumber,
                EmployeeChangeType.BygglosenLendedToOrgNr,
                EmployeeChangeType.BygglosenAgreedHourlyPayLevel,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesGTP()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.GTPAgreementNumber,
                EmployeeChangeType.GTPExcluded,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesAGI()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.AGIPlaceOfEmploymentAddress,
                EmployeeChangeType.AGIPlaceOfEmploymentCity,
                EmployeeChangeType.AGIPlaceOfEmploymentIgnore,
            }.ToArray();
        }

        public static EmployeeChangeType[] GetTypesReportEmployeeChangeTypesIFMetall()
        {
            return new List<EmployeeChangeType>
            {
                EmployeeChangeType.IFAssociationNumber,
                EmployeeChangeType.IFPaymentCode,
                EmployeeChangeType.IFWorkPlace,
            }.ToArray();
        }

        #endregion

        #region EmployeeChangeIODTO

        private bool Valid()
        {
            return !this.EmployeeChangeIODTOs.IsNullOrEmpty();
        }

        private EmployeeChangeIODTO GetEmployeeChange(EmployeeUserDTO employee)
        {
            return this.Valid() && employee != null ? this.EmployeeChangeIODTOs?.FirstOrDefault(f => f.EmployeeNr.Equals(employee.EmployeeNr, StringComparison.OrdinalIgnoreCase)) : null;
        }

        #endregion

        #region EmployeeChangeRowIODTO

        private List<EmployeeChangeRowIODTO> GetValidEmployeeChangeRows(EmployeeChangeIODTO employeeChange)
        {
            return employeeChange?.EmployeeChangeRowIOs?.Where(i => i.IsValid).ToList();
        }

        private EmployeeChangeRowIODTO GetFirstRow(List<EmployeeChangeRowIODTO> rows, params EmployeeChangeType[] types)
        {
            foreach (EmployeeChangeType type in types)
            {
                EmployeeChangeRowIODTO row = rows?.FirstOrDefault(f => f.EmployeeChangeType == type);
                if (row != null)
                    return row;
            }

            return null;
        }

        private List<string> ParseExternalCode(EmployeeChangeRowIODTO row)
        {
            string[] externalCodes = row != null && !row.OptionalExternalCode.IsNullOrEmpty() ? row.OptionalExternalCode.Split(DELIMETER) : null;
            return externalCodes?.ToList() ?? new List<string>();
        }

        private string GetFirstValue(List<EmployeeChangeRowIODTO> rows, EmployeeChangeType type)
        {
            return GetFirstRow(rows, type)?.Value ?? string.Empty;
        }

        private bool? GetBool(List<EmployeeChangeRowIODTO> rows, EmployeeChangeType type)
        {
            if (rows.IsNullOrEmpty() || !rows.Any(f => f.EmployeeChangeType == type))
                return null;

            return StringUtility.GetBool(GetFirstValue(rows, type));
        }

        private DateTime ParseFromDate(EmployeeChangeRowIODTO row)
        {
            return row != null && row.FromDate.HasValue && row.FromDate.Value > this.lookup.MinDate ? row.FromDate.Value.Date : CalendarUtility.DATETIME_DEFAULT;
        }

        private DateTime? ParseNullableFromDate(EmployeeChangeRowIODTO row)
        {
            return row != null && row.FromDate.HasValue && row.FromDate.Value > this.lookup.MinDate ? row.FromDate.Value.Date : (DateTime?)null;
        }

        private DateTime? ParseToDate(EmployeeChangeRowIODTO row)
        {
            return row != null && row.ToDate.HasValue && row.ToDate.Value > this.lookup.MinDate ? row.ToDate.Value.Date : (DateTime?)null;
        }

        private DateTime? ParseOptionalEmploymentDate(EmployeeChangeRowIODTO row, DateTime? defaultDate)
        {
            return row != null && row.OptionalEmploymentDate != DateTime.MinValue ? row.OptionalEmploymentDate : defaultDate;
        }

        private void AddValidationError(EmployeeUserDTO employee, EmployeeChangeRowIODTO row, string value, EmploymentChangeValidationError error)
        {
            if (employee == null || row == null)
                return;

            EmployeeChangeRowValidation validationError = new EmployeeChangeRowValidation(row.EmployeeChangeType, error, value, GetValidationErrorTerm(error));
            row.AddValidationError(validationError);
            employee.AddErrorChange((int)row.EmployeeChangeType, validationError.Message.ToString());
        }

        #endregion
    }
}
