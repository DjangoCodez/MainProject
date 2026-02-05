using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SoftOne.Soe.Business.DTO
{
    public class ChangesRepositoryDTO
    {
        #region Variables

        readonly private int actorCompanyId;
        readonly private TermGroup_TrackChangesActionMethod actionMethod;
        readonly private SoeEntityType topEntity;
        private Guid batch;

        #endregion

        #region Ctor

        public ChangesRepositoryDTO(int actorCompanyId, Guid batch, TermGroup_TrackChangesActionMethod actionMethod, SoeEntityType topEntity)
        {
            this.actorCompanyId = actorCompanyId;
            this.batch = batch;
            this.actionMethod = actionMethod;
            this.topEntity = topEntity;
        }

        #endregion

        #region Delete/Insert/Update

        protected TrackChangesDTO CreateDeleteChange(int topRecordId, int recordId, int? parentRecordId, SoeEntityType entity, SoeEntityType parentEntity, string fromValue = null, string fromValueName = null, TermGroup_TrackChangesColumnType columnType = TermGroup_TrackChangesColumnType.Unspecified, string columnName = null, SoeEntityType topEntity = SoeEntityType.None)
        {
            if (topEntity == SoeEntityType.None)
                topEntity = this.topEntity;

            return new TrackChangesDTO(
                this.actorCompanyId, topRecordId, recordId, parentRecordId,
                topEntity, entity, parentEntity,
                TermGroup_TrackChangesAction.Delete, this.actionMethod, columnType, SettingDataType.Integer,
                columnName, null, this.batch,
                fromValue, null, fromValueName, null
            );
        }

        protected TrackChangesDTO CreateInsertChange(int topRecordId, int recordId, SoeEntityType entity, string toValue = null, string toValueName = null)
        {
            return CreateInsertChange(topRecordId, recordId, null, entity, SoeEntityType.None, toValue, toValueName, TermGroup_TrackChangesColumnType.Unspecified, null);
        }

        protected TrackChangesDTO CreateInsertChange(int topRecordId, int recordId, int? parentRecordId, SoeEntityType entity, SoeEntityType parentEntity, string toValue = null, string toValueName = null, TermGroup_TrackChangesColumnType columnType = TermGroup_TrackChangesColumnType.Unspecified, string columnName = null, SoeEntityType topEntity = SoeEntityType.None)
        {
            if (topEntity == SoeEntityType.None)
                topEntity = this.topEntity;

            return new TrackChangesDTO(
                this.actorCompanyId, topRecordId, recordId, parentRecordId,
                topEntity, entity, parentEntity,
                TermGroup_TrackChangesAction.Insert, this.actionMethod, columnType, SettingDataType.Integer,
                columnName, null, this.batch,
                null, toValue, null, toValueName
            );
        }

        protected TrackChangesDTO CreateUpdateChange(int topRecordId, int recordId, SoeEntityType entity, SettingDataType settingDataType, string fromValue, string toValue, string fromValueName = null, string toValueName = null, TermGroup_TrackChangesColumnType columnType = TermGroup_TrackChangesColumnType.Unspecified, string columnName = null)
        {
            return CreateUpdateChange(topRecordId, recordId, null, entity, SoeEntityType.None, settingDataType, fromValue, toValue, fromValueName, toValueName, columnType, columnName);
        }

        protected TrackChangesDTO CreateUpdateChange(int topRecordId, int recordId, int? parentRecordId, SoeEntityType entity, SoeEntityType parentEntity, SettingDataType settingDataType, string fromValue, string toValue, string fromValueName = null, string toValueName = null, TermGroup_TrackChangesColumnType columnType = TermGroup_TrackChangesColumnType.Unspecified, string columnName = null, SoeEntityType topEntity = SoeEntityType.None)
        {
            if (topEntity == SoeEntityType.None)
                topEntity = this.topEntity;

            switch (settingDataType)
            {
                case SettingDataType.Decimal:
                    fromValue = decimal.Parse(fromValue).ToString("N2");
                    toValue = decimal.Parse(toValue).ToString("N2");
                    break;
                case SettingDataType.Date:
                    if (!string.IsNullOrEmpty(fromValue))
                        fromValue = CalendarUtility.GetDateTime(fromValue).ToShortDateString();
                    if (!string.IsNullOrEmpty(toValue))
                        toValue = CalendarUtility.GetDateTime(toValue).ToShortDateString();
                    break;
            }

            return new TrackChangesDTO(
                this.actorCompanyId, topRecordId, recordId, parentRecordId,
                topEntity, entity, parentEntity,
                TermGroup_TrackChangesAction.Update, this.actionMethod, columnType, settingDataType,
                columnName, null, this.batch,
                fromValue, toValue, fromValueName, toValueName
            );
        }

        protected TrackChangesDTO CreateUpdateChange<T>(T beforeItem, T afterItem, PropertyInfo prop, int topRecordId, int? parentRecordId, int recordId, SoeEntityType entity, SoeEntityType parentEntity, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup, string fromValueName = null, string toValueName = null, SoeEntityType topEntity = SoeEntityType.None)
        {
            if (prop == null)
                return null;

            #region Values

            string fromValue = GetPropertyValue<T>(prop, beforeItem);
            string toValue = GetPropertyValue<T>(prop, afterItem);
            if (fromValue == toValue)
                return null;

            if (termGroup != TermGroup.Unknown)
            {
                var tuple = GetTermGroupNames(termGroup, fromValue, toValue);
                fromValueName = tuple?.Item1;
                toValueName = tuple?.Item2;
            }

            #endregion

            #region DataType

            SettingDataType settingDataType = SettingDataType.String;

            if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                settingDataType = SettingDataType.Integer;
            else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                settingDataType = SettingDataType.Decimal;
            else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                settingDataType = SettingDataType.Boolean;
            else if (prop.PropertyType == typeof(DateTime) && Convert.ToDateTime(prop.GetValue(beforeItem)).Date != Convert.ToDateTime(prop.GetValue(beforeItem)))
                settingDataType = SettingDataType.Time;
            else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                settingDataType = SettingDataType.Date;

            #endregion

            return CreateUpdateChange(topRecordId, recordId, parentRecordId, entity, parentEntity, settingDataType, fromValue, toValue, fromValueName, toValueName, columnType, topEntity: topEntity);
        }

        #endregion

        #region Help-methods

        protected virtual List<PropertyInfo> GetDifferences<T>(T beforeItem, T afterItem)
        {
            var differences = new List<PropertyInfo>();
            var propertyInfos = beforeItem.GetType().GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name.ToLower() == "created" || propertyInfo.Name.ToLower() == "createdby" || propertyInfo.Name.ToLower() == "modified" || propertyInfo.Name.ToLower() == "modifiedby")
                    continue;
                if (propertyInfo.IsClass())
                    continue;
                if (propertyInfo.GetSetMethod() == null)
                    continue;
                if (!propertyInfo.CanWrite)
                    continue;
                if (propertyInfo.IsNonStringEnumerable())
                    continue;

                string beforeValue = GetPropertyValue<T>(propertyInfo, beforeItem);
                string afterValue = GetPropertyValue<T>(propertyInfo, afterItem);
                if (beforeValue != afterValue)
                    differences.Add(propertyInfo);
            }

            return differences;
        }

        protected static T Clone<T>(T source)
        {
            var obj = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            using (var stream = new System.IO.MemoryStream())
            {
                obj.WriteObject(stream, source);
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)obj.ReadObject(stream);
            }
        }

        protected Tuple<string, string> GetTermGroupNames(TermGroup termGroup, string fromValue, string toValue)
        {
            var terms = TermCacheManager.Instance.GetTermGroupContent(termGroup);
            Int32.TryParse(fromValue, out int fromValueInt);
            Int32.TryParse(toValue, out int toValueInt);

            return Tuple.Create(
                terms?.FirstOrDefault(f => f.Id == fromValueInt)?.Name ?? fromValueInt.ToString(),
                terms?.FirstOrDefault(f => f.Id == toValueInt)?.Name ?? toValueInt.ToString());
        }

        private string GetPropertyValue<T>(PropertyInfo propertyInfo, T value)
        {
            return typeof(T).GetProperty(propertyInfo.Name).GetValue(value)?.ToString() ?? string.Empty;
        }

        #endregion
    }

    public class EmployeeUserChangesRepositoryDTO : ChangesRepositoryDTO
    {
        #region Variables

        private int? EmployeeId { get; set; }

        public Employee employeeBefore;
        private Employee employeeAfter;
        private bool employeeBeforeIsSet = false;
        private bool employeeAfterIsSet = false;

        private User userBefore;
        private User userAfter;
        private bool userBeforeIsSet = false;
        private bool userAfterIsSet = false;

        private List<UserRolesDTO> userRolesBefore;
        private List<UserRolesDTO> userRolesAfter;
        private bool userRolesBeforeIsSet = false;
        private bool userRolesAfterIsSet = false;

        private List<EmployeeAccount> employeeAccountsBefore;
        private List<EmployeeAccount> employeeAccountsAfter;
        private bool employeeAccountsBeforeIsSet = false;
        private bool employeeAccountsAfterIsSet = false;

        private List<EmployeeSetting> employeeSettingsBefore;
        private List<EmployeeSetting> employeeSettingsAfter;
        private bool employeeSettingsBeforeIsSet = false;
        private bool employeeSettingsAfterIsSet = false;

        private List<EmployeeTaxSE> employeeTaxSEBefore;
        private List<EmployeeTaxSE> employeeTaxSEAfter;
        private bool employeeTaxBeforeIsSet = false;
        private bool employeeTaxAfterIsSet = false;

        private List<FixedPayrollRow> fixedPayrollRowsBefore;
        private List<FixedPayrollRow> fixedPayrollRowsAfter;
        private bool fixedPayrollRowsBeforeIsSet = false;
        private bool fixedPayrollRowsAfterIsSet = false;

        private ContactPerson contactPersonBefore;
        private ContactPerson contactPersonAfter;
        private bool contactPersonBeforeIsSet = false;
        private bool contactPersonAfterIsSet = false;

        private List<ContactAddressItem> contactAddressItemBefore;
        private List<ContactAddressItem> contactAddressItemAfter;
        private bool contactAddressItemsBeforeIsSet = false;
        private bool contactAddressItemsAfterIsSet = false;

        private List<PayrollProduct> payrollProducts;
        private List<Account> accounts;

        protected EmployeeUserApplyFeaturesResult applyFeaturesResult;

        #endregion

        #region Ctor

        public EmployeeUserChangesRepositoryDTO(int actorCompanyId, Guid batch, TermGroup_TrackChangesActionMethod actionMethod, SoeEntityType topEntity, EmployeeUserApplyFeaturesResult applyFeaturesResult) : base(actorCompanyId, batch, actionMethod, topEntity)
        {
            this.applyFeaturesResult = applyFeaturesResult;
        }

        #endregion

        #region Common

        public void SetBeforeValue(Employee employee)
        {
            this.employeeBefore = Clone(employee);

            this.employeeBeforeIsSet = true;
        }

        public void SetBeforeValue(User user)
        {
            if (user != null)
                this.userBefore = Clone(user);

            this.userBeforeIsSet = true;
        }

        public void SetBeforeValue(ContactPerson contactPerson)
        {
            ContactPerson clone = Clone(contactPerson);

            this.contactPersonBefore = clone;
            this.contactPersonBeforeIsSet = true;
        }

        public void SetBeforeValue(List<ContactAddressItem> contactAddresses, int employeeId)
        {
            List<ContactAddressItem> clones = new List<ContactAddressItem>();

            foreach (ContactAddressItem contactAddress in contactAddresses)
                clones.Add(Clone(contactAddress));

            this.EmployeeId = employeeId;
            this.contactAddressItemBefore = clones;
            this.contactAddressItemsBeforeIsSet = true;
        }

        public void SetBeforeValue(List<EmployeeTaxSE> employeeTaxes)
        {
            List<EmployeeTaxSE> clones = new List<EmployeeTaxSE>();

            foreach (EmployeeTaxSE employeeTax in employeeTaxes)
                clones.Add(Clone(employeeTax));

            this.employeeTaxSEBefore = clones;
            this.employeeTaxBeforeIsSet = true;
        }

        public void SetBeforeValue(List<FixedPayrollRow> fixedPayrollRows, List<PayrollProduct> payrollProducts)
        {
            List<FixedPayrollRow> clones = new List<FixedPayrollRow>();

            foreach (FixedPayrollRow fixedPayrollRow in fixedPayrollRows)
                clones.Add(Clone(fixedPayrollRow));

            this.payrollProducts = payrollProducts;
            this.fixedPayrollRowsBefore = clones;
            this.fixedPayrollRowsBeforeIsSet = true;
        }

        public void SetBeforeValue(List<UserRolesDTO> userRoles)
        {
            List<UserRolesDTO> clones = new List<UserRolesDTO>();

            foreach (UserRolesDTO dto in userRoles)
                clones.Add(Clone(dto));

            this.userRolesBefore = clones;
            this.userRolesBeforeIsSet = true;
        }

        public void SetBeforeValue(List<EmployeeAccount> employeeAccounts, List<Account> accounts)
        {
            List<EmployeeAccount> clones = new List<EmployeeAccount>();

            foreach (EmployeeAccount dto in employeeAccounts)
                clones.Add(Clone(dto));

            this.accounts = accounts;
            this.employeeAccountsBefore = clones;
            this.employeeAccountsBeforeIsSet = true;
        }

        public void SetBeforeValue(List<EmployeeSetting> employeeSettings)
        {
            List<EmployeeSetting> clones = new List<EmployeeSetting>();

            foreach (EmployeeSetting dto in employeeSettings)
                clones.Add(Clone(dto));

            this.employeeSettingsBefore = clones;
            this.employeeSettingsBeforeIsSet = true;
        }

        public void SetAfterValue(Employee employee)
        {
            this.employeeAfter = employee;
            this.employeeAfterIsSet = true;
        }

        public void SetAfterValue(User user)
        {
            this.userAfter = user;
            this.userAfterIsSet = true;
        }

        public void SetAfterValue(ContactPerson contactPerson)
        {
            this.contactPersonAfter = contactPerson;
            this.contactPersonAfterIsSet = true;
        }

        public void SetAfterValue(List<ContactAddressItem> contactAddressItems)
        {
            this.contactAddressItemAfter = contactAddressItems;
            this.contactAddressItemsAfterIsSet = true;
        }

        public void SetAfterValue(List<EmployeeTaxSE> employeeTax)
        {
            this.employeeTaxSEAfter = employeeTax;
            this.employeeTaxAfterIsSet = true;
        }

        public void SetAfterValue(List<FixedPayrollRow> fixedPayrollRows)
        {
            this.fixedPayrollRowsAfter = fixedPayrollRows;
            this.fixedPayrollRowsAfterIsSet = true;
        }

        public void SetAfterValue(List<UserRolesDTO> userRoles)
        {
            this.userRolesAfter = userRoles;
            this.userRolesAfterIsSet = true;
        }

        public void SetAfterValue(List<EmployeeAccount> employeeAccounts)
        {
            this.employeeAccountsAfter = employeeAccounts;
            this.employeeAccountsAfterIsSet = true;
        }

        public void SetAfterValue(List<EmployeeSetting> employeeSettings)
        {
            this.employeeSettingsAfter = employeeSettings;
            this.employeeSettingsAfterIsSet = true;
        }

        public List<TrackChangesDTO> GetChanges()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (this.employeeAfterIsSet)
                changes.AddRange(CreateChangesEmployee());
            if (this.userAfterIsSet)
                changes.AddRange(CreateChangesUser());
            if (this.contactPersonAfterIsSet)
                changes.AddRange(CreateChangesContactPerson());
            if (this.contactAddressItemsAfterIsSet)
                changes.AddRange(CreateChangesEComAndAddress());
            if (this.employeeTaxAfterIsSet)
                changes.AddRange(CreateChangesEmployeeTax());
            if (this.fixedPayrollRowsAfterIsSet)
                changes.AddRange(CreateChangesFixedPayrollRows());
            if (this.userRolesAfterIsSet)
                changes.AddRange(CreateChangesUserRoles());
            if (this.employeeAccountsAfterIsSet)
                changes.AddRange(CreateChangesEmployeeAccounts());
            if (this.employeeSettingsAfterIsSet)
                changes.AddRange(CreateChangesEmployeeSettings());

            return changes.Where(w => w != null).ToList();
        }

        #endregion

        #region Employee

        public List<TrackChangesDTO> CreateChangesEmployee()
        {
            List<TrackChangesDTO> employeeChanges = new List<TrackChangesDTO>();

            if (this.employeeBeforeIsSet && this.employeeAfterIsSet)
            {
                if (employeeBefore == null && employeeAfter != null)
                    employeeChanges.Add(this.CreateInsertChange(employeeAfter.EmployeeId, employeeAfter.EmployeeId, SoeEntityType.Employee));
                else if (employeeBefore != null && employeeAfter != null)
                    employeeChanges.AddRange(this.GetEmployeeUpdateChanges());
            }

            return employeeChanges;
        }

        private List<TrackChangesDTO> GetEmployeeUpdateChanges()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (this.employeeBefore == null || this.employeeAfter == null)
                return changes;

            var diffProps = GetDifferences(this.employeeBefore, this.employeeAfter);
            if (diffProps.IsNullOrEmpty())
                return changes;

            foreach (var prop in this.employeeBefore.GetProperties())
            {
                var diffProp = diffProps.FirstOrDefault(f => f.Name == prop.Name);
                if (diffProp == null)
                    continue;

                if (diffProp.Name.Equals(nameof(this.employeeBefore.EmployeeNr)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_EmployeeNr));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.EmploymentDate)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_EmploymentDate));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.EndDate)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_EndDate));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.CardNumber)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedCardNumber))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_Cardnumber));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.Note)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedNote))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_Note));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DisbursementMethod)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedDisbursement))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DisbursementMethod, termGroup: TermGroup.EmployeeDisbursementMethod));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DisbursementClearingNr)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedDisbursement))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DisbursementClearingNr));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DisbursementAccountNr)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedDisbursement))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DisbursementAccountNr));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DontValidateDisbursementAccountNr)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedDisbursement))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DontValidateDisbursementAccountNr));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DisbursementCountryCode)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedDisbursement))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DisbursementCountryCode));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DisbursementBIC)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedDisbursement))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DisbursementBIC));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DisbursementIBAN)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedDisbursement))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DisbursementIBAN));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.ShowNote)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedNote))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_ShowNote));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.HighRiskProtection)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedHighRiskProtection))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_HighRiskProtection));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.HighRiskProtectionTo)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedHighRiskProtection))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_HighRiskProtectionTo));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.MedicalCertificateReminder)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedMedicalCertificateReminder))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_MedicalCertificateReminder));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.MedicalCertificateDays)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedMedicalCertificateReminder))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_MedicalCertificateDays));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.Absence105DaysExcluded)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_Absence105DaysExcluded));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.Absence105DaysExcludedDays)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_Absence105DaysExcludedDays));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.ExternalCode)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_ExternalCode));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.PayrollStatisticsPersonalCategory)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_PayrollStatisticsPersonalCategory, termGroup: TermGroup.PayrollReportsPersonalCategory));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.PayrollStatisticsWorkTimeCategory)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_PayrollStatisticsWorkTimeCategory, termGroup: TermGroup.PayrollReportsWorkTimeCategory));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.PayrollStatisticsSalaryType)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_PayrollStatisticsSalaryType, termGroup: TermGroup.PayrollReportsSalaryType));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.PayrollStatisticsWorkPlaceNumber)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_PayrollStatisticsWorkPlaceNumber));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.PayrollStatisticsCFARNumber)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_PayrollStatisticsCFARNumber));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.WorkPlaceSCB)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_WorkPlaceSCB));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.PartnerInCloseCompany)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_PartnerInCloseCompany));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.BenefitAsPension)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_BenefitAsPension));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.AFACategory)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_AFACategory, termGroup: TermGroup.PayrollReportsAFACategory));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.AFASpecialAgreement)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_AFASpecialAgreement, termGroup: TermGroup.PayrollReportsAFASpecialAgreement));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.AFAWorkplaceNr)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_AFAWorkplaceNr));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.AFAParttimePensionCode)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_AFAParttimePensionCode));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.CollectumITPPlan)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_CollectumITPPlan, termGroup: TermGroup.PayrollReportsCollectumITPplan));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.CollectumAgreedOnProduct)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_CollectumAgreedOnProduct));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.CollectumCostPlace)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_CollectumCostPlace));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.CollectumCancellationDate)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_CollectumCancellationDate));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.CollectumCancellationDateIsLeaveOfAbsence)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_CollectumCancellationDateIsLeaveOfAbsence));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.WantsExtraShifts)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_WantsExtraShifts));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DontNotifyChangeOfDeviations)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DontNotifyChangeOfDeviations));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.DontNotifyChangeOfAttestState)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_DontNotifyChangeOfAttestState));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.KPARetirementAge)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_KPARetirementAge));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.KPABelonging)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_KPABelonging, termGroup: TermGroup.KPABelonging));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.KPAEndCode)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_KPAEndCode, termGroup: TermGroup.KPAEndCode));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.KPAAgreementType)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_KPAAgreementType));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.BygglosenAgreementArea)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_BygglosenAgreementArea));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.BygglosenAllocationNumber)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_BygglosenAllocationNumber));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.BygglosenMunicipalCode)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_BygglosenMunicipalCode));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.ParentId)))
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_Parent));
                else if (diffProp.Name.Equals(nameof(this.employeeBefore.BygglosenSalaryFormula)))
                {
                    //TODO Create dict with formula names and pass to method.
                    changes.Add(GetUpdateChangeEmployee(this.employeeBefore, this.employeeAfter, diffProp, TermGroup_TrackChangesColumnType.Employee_BygglosenSalaryFormula));
                }
            }

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO GetUpdateChangeEmployee(Employee beforeItem, Employee afterItem, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown)
        {
            return CreateUpdateChange(beforeItem, afterItem, prop, afterItem.EmployeeId, afterItem.EmployeeId, beforeItem.EmployeeId, SoeEntityType.Employee, SoeEntityType.Employee, columnType, termGroup);
        }

        #endregion

        #region User

        public List<TrackChangesDTO> CreateChangesUser()
        {
            List<TrackChangesDTO> userChanges = new List<TrackChangesDTO>();

            if (this.userBeforeIsSet && this.userAfterIsSet)
            {
                if (userBefore == null && userAfter != null)
                    userChanges.Add(this.CreateInsertChange(userAfter.UserId, userAfter.UserId, SoeEntityType.User));
                else if (userBefore != null && userAfter != null)
                    userChanges.AddRange(this.CreateUserUpdateChanges());
            }

            return userChanges;
        }

        private List<TrackChangesDTO> CreateUserUpdateChanges()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (this.userBefore == null || this.userAfter == null)
                return changes;

            var diffProps = GetDifferences(this.userBefore, this.userAfter);
            if (diffProps.IsNullOrEmpty())
                return changes;

            foreach (var prop in this.userBefore.GetProperties())
            {
                var diffProp = diffProps.FirstOrDefault(f => f.Name == prop.Name);
                if (diffProp == null)
                    continue;

                if (diffProp.Name.Equals(nameof(this.userBefore.DefaultActorCompanyId)))
                    changes.Add(GetUpdateChangeUser(this.userBefore, this.userAfter, diffProp, TermGroup_TrackChangesColumnType.User_DefaultActorCompanyId));
                else if (diffProp.Name.Equals(nameof(this.userBefore.LangId)))
                    changes.Add(GetUpdateChangeUser(this.userBefore, this.userAfter, diffProp, TermGroup_TrackChangesColumnType.User_LangId));
                else if (diffProp.Name.Equals(nameof(this.userBefore.DepartmentId)))
                    changes.Add(GetUpdateChangeUser(this.userBefore, this.userAfter, diffProp, TermGroup_TrackChangesColumnType.User_DepartmentId));
                else if (diffProp.Name.Equals(nameof(this.userBefore.LoginName)))
                    changes.Add(GetUpdateChangeUser(this.userBefore, this.userAfter, diffProp, TermGroup_TrackChangesColumnType.User_LoginName));
                else if (diffProp.Name.Equals(nameof(this.userBefore.Name)))
                    changes.Add(GetUpdateChangeUser(this.userBefore, this.userAfter, diffProp, TermGroup_TrackChangesColumnType.User_Name));
                else if (diffProp.Name.Equals(nameof(this.userBefore.Email)))
                    changes.Add(GetUpdateChangeUser(this.userBefore, this.userAfter, diffProp, TermGroup_TrackChangesColumnType.User_Email));
                else if (diffProp.Name.Equals(nameof(this.userBefore.EmailCopy)))
                    changes.Add(GetUpdateChangeUser(this.userBefore, this.userAfter, diffProp, TermGroup_TrackChangesColumnType.User_EmailCopy));
            }

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO GetUpdateChangeUser(User before, User after, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown)
        {
            return CreateUpdateChange(before, after, prop, after.UserId, after.UserId, before.UserId, SoeEntityType.User, SoeEntityType.Employee, columnType, termGroup);
        }

        #endregion

        #region UserRoles        

        public List<TrackChangesDTO> CreateChangesUserRoles()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();
            if (this.userRolesBeforeIsSet && this.userRolesAfterIsSet)
            {
                changes.AddRange(this.CreateChangesUserCompanyRole());
                changes.AddRange(this.CreateChangesAttestRoleUser());
            }

            return changes;
        }

        #region UserCompanyRole

        private List<TrackChangesDTO> CreateChangesUserCompanyRole()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            #region Insert/Update

            if (this.userRolesAfter != null)
            {
                // Loop over companies
                foreach (UserRolesDTO afterItem in this.userRolesAfter)
                {
                    // Get before item for current company
                    UserRolesDTO beforeItem = this.userRolesBefore.FirstOrDefault(w => w.ActorCompanyId == afterItem.ActorCompanyId);

                    foreach (UserCompanyRoleDTO afterRole in afterItem.Roles)
                    {
                        UserCompanyRoleDTO beforeRole = beforeItem?.Roles.FirstOrDefault(r => r.UserCompanyRoleId == afterRole.UserCompanyRoleId);
                        if (beforeRole == null)
                        {
                            changes.Add(CreateInsertChangeUserCompanyRole(afterRole));
                        }
                        else
                        {
                            List<PropertyInfo> diffProps = GetDifferences(beforeRole, afterRole);
                            if (!diffProps.IsNullOrEmpty())
                            {
                                foreach (PropertyInfo afterProp in afterRole.GetProperties())
                                {
                                    PropertyInfo diffProp = diffProps.FirstOrDefault(f => f.Name == afterProp.Name);
                                    if (diffProp == null)
                                        continue;

                                    if (diffProp.Name.Equals(nameof(afterRole.Name)))
                                        changes.Add(CreateUpdateChange(beforeRole.UserId, beforeRole.UserCompanyRoleId, SoeEntityType.UserCompanyRole, SettingDataType.String, beforeRole.Name, afterRole.Name, columnType: TermGroup_TrackChangesColumnType.UserCompanyRole_Role));
                                    else if (diffProp.Name.Equals(nameof(afterRole.DateFrom)))
                                        changes.Add(CreateUpdateChange(beforeRole.UserId, beforeRole.UserCompanyRoleId, SoeEntityType.UserCompanyRole, SettingDataType.Date, beforeRole.DateFrom?.ToShortDateString(), afterRole.DateFrom?.ToShortDateString(), columnType: TermGroup_TrackChangesColumnType.UserCompanyRole_DateFrom));
                                    else if (diffProp.Name.Equals(nameof(afterRole.DateTo)))
                                        changes.Add(CreateUpdateChange(beforeRole.UserId, beforeRole.UserCompanyRoleId, SoeEntityType.UserCompanyRole, SettingDataType.Date, beforeRole.DateTo?.ToShortDateString(), afterRole.DateTo?.ToShortDateString(), columnType: TermGroup_TrackChangesColumnType.UserCompanyRole_DateTo));
                                    else if (diffProp.Name.Equals(nameof(afterRole.Default)))
                                        changes.Add(CreateUpdateChange(beforeRole.UserId, beforeRole.UserCompanyRoleId, SoeEntityType.UserCompanyRole, SettingDataType.Boolean, beforeRole.Default.ToString(), afterRole.Default.ToString(), columnType: TermGroup_TrackChangesColumnType.UserCompanyRole_Default));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Delete

            if (this.userRolesBefore != null)
            {
                // Loop over companies
                foreach (UserRolesDTO beforeItem in this.userRolesBefore)
                {
                    // Get after item for current company
                    UserRolesDTO afterItem = this.userRolesAfter.FirstOrDefault(w => w.ActorCompanyId == beforeItem.ActorCompanyId);

                    foreach (UserCompanyRoleDTO beforeRole in beforeItem.Roles)
                    {
                        UserCompanyRoleDTO afterRole = afterItem?.Roles.FirstOrDefault(r => r.UserCompanyRoleId == beforeRole.UserCompanyRoleId);
                        if (afterRole == null)
                            changes.Add(CreateDeleteChangeUserCompanyRole(beforeRole));
                    }
                }
            }

            #endregion

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO CreateInsertChangeUserCompanyRole(UserCompanyRoleDTO afterItem)
        {
            return CreateInsertChange(afterItem.UserId, afterItem.UserCompanyRoleId, afterItem.UserId, SoeEntityType.UserCompanyRole, SoeEntityType.User, columnType: TermGroup_TrackChangesColumnType.UserCompanyRole_Role, toValue: afterItem.Name, topEntity: SoeEntityType.User);
        }

        private TrackChangesDTO CreateDeleteChangeUserCompanyRole(UserCompanyRoleDTO beforeItem)
        {
            return CreateDeleteChange(beforeItem.UserId, beforeItem.UserCompanyRoleId, beforeItem.UserId, SoeEntityType.UserCompanyRole, SoeEntityType.User, fromValue: beforeItem.Name, columnType: TermGroup_TrackChangesColumnType.UserCompanyRole_Role, topEntity: SoeEntityType.User);
        }

        #endregion

        #region AttestRoleUser

        private List<TrackChangesDTO> CreateChangesAttestRoleUser()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            #region Insert/Update

            if (this.userRolesAfter != null)
            {
                // Loop over companies
                foreach (UserRolesDTO afterItem in this.userRolesAfter)
                {
                    // Get before item for current company
                    UserRolesDTO beforeItem = this.userRolesBefore.FirstOrDefault(w => w.ActorCompanyId == afterItem.ActorCompanyId);

                    foreach (UserAttestRoleDTO afterRole in afterItem.AttestRoles)
                    {
                        UserAttestRoleDTO beforeRole = beforeItem?.AttestRoles.FirstOrDefault(r => r.AttestRoleUserId == afterRole.AttestRoleUserId);
                        if (beforeRole == null)
                        {
                            changes.Add(CreateInsertChangeAttestRoleUser(afterRole));
                        }
                        else
                        {
                            List<PropertyInfo> diffProps = GetDifferences(beforeRole, afterRole);
                            if (!diffProps.IsNullOrEmpty())
                            {
                                foreach (PropertyInfo afterProp in afterRole.GetProperties())
                                {
                                    PropertyInfo diffProp = diffProps.FirstOrDefault(f => f.Name == afterProp.Name);
                                    if (diffProp == null)
                                        continue;

                                    if (diffProp.Name.Equals(nameof(afterRole.Name)))
                                        changes.Add(GetUpdateChangeAttestRoleUser(beforeRole, afterRole, diffProp, TermGroup_TrackChangesColumnType.AttestRoleUser_AttestRole));
                                    else if (diffProp.Name.Equals(nameof(afterRole.DateFrom)))
                                        changes.Add(GetUpdateChangeAttestRoleUser(beforeRole, afterRole, diffProp, TermGroup_TrackChangesColumnType.AttestRoleUser_DateFrom));
                                    else if (diffProp.Name.Equals(nameof(afterRole.DateTo)))
                                        changes.Add(GetUpdateChangeAttestRoleUser(beforeRole, afterRole, diffProp, TermGroup_TrackChangesColumnType.AttestRoleUser_DateTo));
                                    else if (diffProp.Name.Equals(nameof(afterRole.MaxAmount)) && beforeRole.MaxAmount != afterRole.MaxAmount)
                                        changes.Add(GetUpdateChangeAttestRoleUser(beforeRole, afterRole, diffProp, TermGroup_TrackChangesColumnType.AttestRoleUser_MaxAmount));
                                    else if (diffProp.Name.Equals(nameof(afterRole.AccountId)))
                                        changes.Add(GetUpdateChangeAttestRoleUser(beforeRole, afterRole, diffProp, TermGroup_TrackChangesColumnType.AttestRoleUser_Account));
                                    else if (diffProp.Name.Equals(nameof(afterRole.IsExecutive)))
                                        changes.Add(GetUpdateChangeAttestRoleUser(beforeRole, afterRole, diffProp, TermGroup_TrackChangesColumnType.AttestRoleUser_IsExecutive));
                                    else if (diffProp.Name.Equals(nameof(afterRole.IsNearestManager)))
                                        changes.Add(GetUpdateChangeAttestRoleUser(beforeRole, afterRole, diffProp, TermGroup_TrackChangesColumnType.AttestRoleUser_IsNearestManager));
                                    else if (diffProp.Name.Equals(nameof(afterRole.AccountPermissionType)))
                                    {
                                        var tuple = GetTermGroupNames(TermGroup.AttestRoleUserAccountPermissionType, ((int)beforeRole.AccountPermissionType).ToString(), ((int)afterRole.AccountPermissionType).ToString());
                                        changes.Add(CreateUpdateChange(afterRole.UserId, afterRole.AttestRoleUserId, afterRole.UserId, SoeEntityType.AttestRoleUser, SoeEntityType.User, SettingDataType.Integer, ((int)beforeRole.AccountPermissionType).ToString(), ((int)afterRole.AccountPermissionType).ToString(), tuple?.Item1, tuple?.Item2, TermGroup_TrackChangesColumnType.AttestRoleUser_AccountPermissionType));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Delete

            if (this.userRolesBefore != null)
            {
                // Loop over companies
                foreach (UserRolesDTO beforeItem in this.userRolesBefore)
                {
                    // Get after item for current company
                    UserRolesDTO afterItem = this.userRolesAfter.FirstOrDefault(w => w.ActorCompanyId == beforeItem.ActorCompanyId);

                    foreach (UserAttestRoleDTO beforeRole in beforeItem.AttestRoles)
                    {
                        UserAttestRoleDTO afterRole = afterItem?.AttestRoles.FirstOrDefault(r => r.AttestRoleUserId == beforeRole.AttestRoleUserId);
                        if (afterRole == null)
                            changes.Add(CreateDeleteChangeAttestRoleUser(beforeRole));
                    }
                }
            }

            #endregion

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO GetUpdateChangeAttestRoleUser(UserAttestRoleDTO beforeItem, UserAttestRoleDTO afterItem, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown)
        {
            return CreateUpdateChange(beforeItem, afterItem, prop, afterItem.UserId, afterItem.UserId, afterItem.AttestRoleUserId, SoeEntityType.AttestRoleUser, SoeEntityType.User, columnType, termGroup, topEntity: SoeEntityType.User);
        }

        private TrackChangesDTO CreateInsertChangeAttestRoleUser(UserAttestRoleDTO afterItem)
        {
            return CreateInsertChange(afterItem.UserId, afterItem.AttestRoleUserId, afterItem.UserId, SoeEntityType.AttestRoleUser, SoeEntityType.User, columnType: TermGroup_TrackChangesColumnType.AttestRoleUser_AttestRole, toValue: afterItem.Name, topEntity: SoeEntityType.User);
        }

        private TrackChangesDTO CreateDeleteChangeAttestRoleUser(UserAttestRoleDTO beforeItem)
        {
            return CreateDeleteChange(beforeItem.UserId, beforeItem.AttestRoleUserId, beforeItem.UserId, SoeEntityType.AttestRoleUser, SoeEntityType.User, fromValue: beforeItem.Name, columnType: TermGroup_TrackChangesColumnType.AttestRoleUser_AttestRole, topEntity: SoeEntityType.User);
        }

        #endregion

        #endregion

        #region EmployeeAccount

        public List<TrackChangesDTO> CreateChangesEmployeeAccounts()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (!this.employeeAccountsBeforeIsSet || !this.employeeAccountsAfterIsSet)
                return changes;

            if (this.employeeAccountsAfter != null)
            {
                foreach (var afterItem in this.employeeAccountsAfter)
                {
                    var beforeItem = this.employeeAccountsBefore?.FirstOrDefault(w => w.EmployeeAccountId == afterItem.EmployeeAccountId);
                    if (afterItem.State == (int)SoeEntityState.Deleted)
                    {
                        if (beforeItem != null && beforeItem.State != (int)SoeEntityState.Deleted)
                            changes.Add(CreateDeleteChangeEmployeeAccount(beforeItem));
                    }
                    else
                    {
                        if (beforeItem == null)
                            changes.Add(CreateInsertChangeEmployeeAccount(afterItem));
                        else
                            changes.AddRange(CreateUpdateChangeEmployeeAccount(beforeItem, afterItem));
                    }
                }
            }

            return changes;
        }

        private TrackChangesDTO CreateInsertChangeEmployeeAccount(EmployeeAccount afterItem)
        {
            return CreateInsertChange(afterItem.EmployeeId, afterItem.EmployeeAccountId, SoeEntityType.EmployeeAccount);
        }

        private List<TrackChangesDTO> CreateUpdateChangeEmployeeAccount(EmployeeAccount beforeItem, EmployeeAccount afterItem)
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            var diffProps = GetDifferences(beforeItem, afterItem);
            if (diffProps.IsNullOrEmpty())
                return changes;

            string fromValueName = beforeItem != null ? accounts?.FirstOrDefault(f => f.AccountId == beforeItem.AccountId)?.Name : null;
            string toValueName = afterItem != null ? accounts?.FirstOrDefault(f => f.AccountId == afterItem.AccountId)?.Name : null;

            foreach (var prop in afterItem.GetProperties())
            {
                var diffProp = diffProps.FirstOrDefault(f => f.Name == prop.Name);
                if (diffProp == null)
                    continue;

                if (diffProp.Name.Equals(nameof(afterItem.AccountId)))
                    changes.Add(GetUpdateChangeEmployeeAccount(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeAccount_Account, fromValueName: fromValueName, toValueName: toValueName));
                else if (diffProp.Name.Equals(nameof(afterItem.DateFrom)))
                    changes.Add(GetUpdateChangeEmployeeAccount(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeAccount_DateFrom));
                else if (diffProp.Name.Equals(nameof(afterItem.DateTo)))
                    changes.Add(GetUpdateChangeEmployeeAccount(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeAccount_DateTo));
                else if (diffProp.Name.Equals(nameof(afterItem.Default)))
                    changes.Add(GetUpdateChangeEmployeeAccount(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeAccount_Default));
            }

            return changes;
        }

        private TrackChangesDTO GetUpdateChangeEmployeeAccount(EmployeeAccount beforeItem, EmployeeAccount afterItem, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown, string fromValueName = null, string toValueName = null)
        {
            return CreateUpdateChange(beforeItem, afterItem, prop, afterItem.EmployeeId, afterItem.EmployeeId, beforeItem.EmployeeAccountId, SoeEntityType.EmployeeAccount, SoeEntityType.Employee, columnType, termGroup, fromValueName, toValueName);
        }

        private TrackChangesDTO CreateDeleteChangeEmployeeAccount(EmployeeAccount beforeItem)
        {
            return CreateDeleteChange(beforeItem.EmployeeId, beforeItem.EmployeeAccountId, beforeItem.EmployeeId, SoeEntityType.EmployeeAccount, SoeEntityType.Employee);
        }

        #endregion

        #region EmployeeSetting

        public List<TrackChangesDTO> CreateChangesEmployeeSettings()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (!this.employeeSettingsBeforeIsSet || !this.employeeSettingsAfterIsSet)
                return changes;

            if (this.employeeSettingsAfter != null)
            {
                foreach (var afterItem in this.employeeSettingsAfter)
                {
                    var beforeItem = this.employeeSettingsBefore?.FirstOrDefault(w => w.EmployeeSettingId == afterItem.EmployeeSettingId);
                    if (afterItem.State == (int)SoeEntityState.Deleted)
                    {
                        if (beforeItem != null && beforeItem.State != (int)SoeEntityState.Deleted)
                            changes.Add(CreateDeleteChangeEmployeeSetting(beforeItem));
                    }
                    else
                    {
                        if (beforeItem == null)
                            changes.Add(CreateInsertChangeEmployeeSetting(afterItem));
                        else
                            changes.AddRange(CreateUpdateChangeEmployeeSetting(beforeItem, afterItem));
                    }
                }
            }

            return changes;
        }

        private TrackChangesDTO CreateInsertChangeEmployeeSetting(EmployeeSetting afterItem)
        {
            string yesString = TermCacheManager.Instance.GetText(52, 1, "Ja");
            string noString = TermCacheManager.Instance.GetText(53, 1, "Nej");

            return CreateInsertChange(afterItem.EmployeeId, afterItem.EmployeeSettingId, afterItem.EmployeeId, SoeEntityType.EmployeeSetting, SoeEntityType.Employee, columnType: TermGroup_TrackChangesColumnType.EmployeeSetting_Value, toValue: GetEmployeeSettingValue(afterItem, yesString, noString));
        }

        private List<TrackChangesDTO> CreateUpdateChangeEmployeeSetting(EmployeeSetting beforeItem, EmployeeSetting afterItem)
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            var diffProps = GetDifferences(beforeItem, afterItem);
            if (diffProps.IsNullOrEmpty())
                return changes;

            foreach (var prop in afterItem.GetProperties())
            {
                var diffProp = diffProps.FirstOrDefault(f => f.Name == prop.Name);
                if (diffProp == null)
                    continue;

                if (diffProp.Name.Equals(nameof(afterItem.EmployeeSettingAreaType)))
                    changes.Add(GetUpdateChangeEmployeeSetting(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeSetting_AreaType, TermGroup.EmployeeSettingType));
                else if (diffProp.Name.Equals(nameof(afterItem.EmployeeSettingGroupType)))
                    changes.Add(GetUpdateChangeEmployeeSetting(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeSetting_GroupType, TermGroup.EmployeeSettingType));
                else if (diffProp.Name.Equals(nameof(afterItem.EmployeeSettingType)))
                    changes.Add(GetUpdateChangeEmployeeSetting(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeSetting_Type, TermGroup.EmployeeSettingType));
                else if (diffProp.Name.Equals(nameof(afterItem.ValidFromDate)))
                    changes.Add(GetUpdateChangeEmployeeSetting(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeSetting_ValidFromDate));
                else if (diffProp.Name.Equals(nameof(afterItem.ValidToDate)))
                    changes.Add(GetUpdateChangeEmployeeSetting(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeSetting_ValidToDate));
                else if (diffProp.Name.Equals(nameof(afterItem.StrData)) || diffProp.Name.Equals(nameof(afterItem.IntData)) || diffProp.Name.Equals(nameof(afterItem.DecimalData)) || diffProp.Name.Equals(nameof(afterItem.BoolData)) || diffProp.Name.Equals(nameof(afterItem.DateData)) || diffProp.Name.Equals(nameof(afterItem.TimeData)))
                    changes.Add(GetUpdateChangeEmployeeSetting(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.EmployeeSetting_Value));
            }

            return changes;
        }

        private TrackChangesDTO GetUpdateChangeEmployeeSetting(EmployeeSetting beforeItem, EmployeeSetting afterItem, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown, string fromValueName = null, string toValueName = null)
        {
            return CreateUpdateChange(beforeItem, afterItem, prop, afterItem.EmployeeId, afterItem.EmployeeId, beforeItem.EmployeeSettingId, SoeEntityType.EmployeeSetting, SoeEntityType.Employee, columnType, termGroup, fromValueName, toValueName);
        }

        private TrackChangesDTO CreateDeleteChangeEmployeeSetting(EmployeeSetting beforeItem)
        {
            string yesString = TermCacheManager.Instance.GetText(52, 1, "Ja");
            string noString = TermCacheManager.Instance.GetText(53, 1, "Nej");

            return CreateDeleteChange(beforeItem.EmployeeId, beforeItem.EmployeeSettingId, beforeItem.EmployeeId, SoeEntityType.EmployeeSetting, SoeEntityType.Employee, fromValue: GetEmployeeSettingValue(beforeItem, yesString, noString));
        }

        private string GetEmployeeSettingValue(EmployeeSetting item, string yesString, string noString)
        {
            switch ((SettingDataType)item.DataType)
            {
                case SettingDataType.String:
                    return item.StrData;
                case SettingDataType.Integer:
                    return item.IntData.HasValue ? item.IntData.ToString() : string.Empty;
                case SettingDataType.Decimal:
                    return item.DecimalData.HasValue ? Decimal.Round(item.DecimalData.Value, 2, MidpointRounding.AwayFromZero).ToString() : string.Empty;
                case SettingDataType.Boolean:
                    return item.BoolData.Value ? yesString : noString;
                case SettingDataType.Date:
                    return item.DateData.HasValue ? item.DateData.Value.ToShortDateString() : string.Empty;
                case SettingDataType.Time:
                    return item.TimeData.HasValue ? item.TimeData.Value.ToShortTimeString() : string.Empty;
            }

            return string.Empty;
        }

        #endregion

        #region EmployeeTax

        public List<TrackChangesDTO> CreateChangesEmployeeTax()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (this.employeeTaxBeforeIsSet && this.employeeTaxAfterIsSet && this.employeeTaxSEAfter != null)
            {
                foreach (EmployeeTaxSE employeeTaxSE in this.employeeTaxSEAfter)
                {
                    var old = this.employeeTaxSEBefore?.FirstOrDefault(w => w.EmployeeTaxId == employeeTaxSE.EmployeeTaxId);
                    if (old == null)
                        changes.Add(this.CreateInsertChange(employeeTaxSE.EmployeeId, employeeTaxSE.EmployeeTaxId, SoeEntityType.EmployeeTaxSE));
                    else
                        changes.AddRange(this.GetEmployeeTaxUpdateChanges());
                }
            }

            return changes;
        }

        private List<TrackChangesDTO> GetEmployeeTaxUpdateChanges()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (!this.employeeTaxBeforeIsSet || this.employeeTaxSEBefore == null)
                return changes;

            foreach (EmployeeTaxSE employeeTaxSE in this.employeeTaxSEAfter)
            {
                if (employeeTaxSE.State == (int)SoeEntityState.Deleted)
                {
                    changes.Add(this.CreateDeleteChange(employeeTaxSE.EmployeeId, employeeTaxSE.EmployeeTaxId, employeeTaxSE.EmployeeId, SoeEntityType.EmployeeTaxSE, SoeEntityType.Employee));
                    continue;
                }

                var old = this.employeeTaxSEBefore?.FirstOrDefault(w => w.EmployeeTaxId == employeeTaxSE.EmployeeTaxId);
                if (old == null)
                    continue;

                var diffProps = GetDifferences(old, employeeTaxSE);
                if (diffProps.IsNullOrEmpty())
                    return changes;

                foreach (var prop in this.employeeTaxSEBefore.GetProperties())
                {
                    var diffProp = diffProps.FirstOrDefault(f => f.Name == prop.Name);
                    if (diffProp == null)
                        continue;

                    if (diffProp.Name.Equals(nameof(employeeTaxSE.MainEmployer)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_MainEmployer));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.Type)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_MainEmployer, termGroup: TermGroup.EmployeeTaxType));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.TaxRate)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_TaxRate));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.TaxRateColumn)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_TaxRateColumn));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.OneTimeTaxPercent)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_OneTimeTaxPercent));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.EstimatedAnnualSalary)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_EstimatedAnnualSalary));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.AdjustmentType)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentType, termGroup: TermGroup.EmployeeTaxAdjustmentType));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.AdjustmentValue)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentValue));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.AdjustmentPeriodFrom)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentPeriodFrom));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.AdjustmentPeriodTo)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_AdjustmentPeriodTo));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.SchoolYouthLimitInitial)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_SchoolYouthLimitInitial));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.SinkType)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_SinkType, termGroup: TermGroup.EmployeeTaxSinkType));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.EmploymentTaxType)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_EmploymentTaxType, termGroup: TermGroup.EmployeeTaxEmploymentTaxType));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.EmploymentAbroadCode)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_EmploymentAbroadCode));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.RegionalSupport)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_RegionalSupport));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.SalaryDistressAmount)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressAmount));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.SalaryDistressAmountType)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressAmountType, termGroup: TermGroup.EmployeeTaxSalaryDistressAmountType));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.SalaryDistressReservedAmount)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_SalaryDistressReserveAmount));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.CSRExportDate)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_CSRExportDate));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.CSRImportDate)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_CSRImportDate));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.TinNumber)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_TinNumber));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.CountryCode)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_CountryCode));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.BirthPlace)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_BirthPlace));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.CountryCodeBirthPlace)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_CountryCodeBirthPlace));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.CountryCodeCitizen)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_CountryCodeCitizen));
                    else if (diffProp.Name.Equals(nameof(employeeTaxSE.State)))
                        changes.Add(GetUpdateChangeEmployeeTax(old, employeeTaxSE, diffProp, TermGroup_TrackChangesColumnType.EmployeeTaxSE_State));
                }
            }

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO GetUpdateChangeEmployeeTax(EmployeeTaxSE old, EmployeeTaxSE tax, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown, string fromValueName = null, string toValueName = null)
        {
            return CreateUpdateChange(old, tax, prop, tax.EmployeeId, tax.EmployeeId, old.EmployeeTaxId, SoeEntityType.EmployeeTaxSE, SoeEntityType.None, columnType, termGroup, fromValueName, toValueName);
        }

        #endregion

        #region FixedPayrollRow

        public List<TrackChangesDTO> CreateChangesFixedPayrollRows()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (this.fixedPayrollRowsBeforeIsSet && this.fixedPayrollRowsAfterIsSet && this.fixedPayrollRowsAfter != null)
            {
                foreach (var afterItem in this.fixedPayrollRowsAfter)
                {
                    var beforeItem = this.fixedPayrollRowsBefore?.FirstOrDefault(w => w.FixedPayrollRowId == afterItem.FixedPayrollRowId);
                    if (beforeItem == null)
                        changes.Add(this.CreateInsertChange(afterItem.EmployeeId, afterItem.FixedPayrollRowId, SoeEntityType.FixedPayrollRow));
                    else
                        changes.AddRange(this.GetFixedPayrollRowUpdateChanges());
                }
            }

            return changes;
        }

        private List<TrackChangesDTO> GetFixedPayrollRowUpdateChanges()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (!this.fixedPayrollRowsBeforeIsSet || this.fixedPayrollRowsBefore == null)
                return changes;

            foreach (var afterItem in this.fixedPayrollRowsAfter)
            {
                var beforeItem = this.fixedPayrollRowsBefore?.FirstOrDefault(w => w.FixedPayrollRowId == afterItem.FixedPayrollRowId);
                string fromValueName = beforeItem != null ? payrollProducts?.FirstOrDefault(f => f.ProductId == beforeItem.ProductId)?.Name : null;
                string toValueName = payrollProducts?.FirstOrDefault(f => f.ProductId == afterItem.ProductId)?.Name;

                if (afterItem.State == (int)SoeEntityState.Deleted)
                {
                    changes.Add(this.CreateDeleteChange(afterItem.EmployeeId, afterItem.FixedPayrollRowId, afterItem.EmployeeId, SoeEntityType.FixedPayrollRow, SoeEntityType.Employee, fromValue: fromValueName ?? toValueName));
                    continue;
                }

                if (beforeItem == null)
                    continue;

                var diffProps = GetDifferences(beforeItem, afterItem);
                if (diffProps.IsNullOrEmpty())
                    return changes;

                foreach (var prop in afterItem.GetProperties())
                {
                    var diffProp = diffProps.FirstOrDefault(f => f.Name == prop.Name);
                    if (diffProp == null)
                        continue;

                    if (diffProp.Name.Equals(nameof(afterItem.ProductId)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_Distribute, fromValueName: fromValueName, toValueName: toValueName));
                    else if (diffProp.Name.Equals(nameof(afterItem.FromDate)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_FromDate));
                    else if (diffProp.Name.Equals(nameof(afterItem.ToDate)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_ToDate));
                    else if (diffProp.Name.Equals(nameof(afterItem.UnitPrice)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_UnitPrice));
                    else if (diffProp.Name.Equals(nameof(afterItem.Quantity)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_Quantity));
                    else if (diffProp.Name.Equals(nameof(afterItem.VatAmount)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_VatAmount));
                    else if (diffProp.Name.Equals(nameof(afterItem.IsSpecifiedUnitPrice)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_IsSpecifiedUnitPrice));
                    else if (diffProp.Name.Equals(nameof(afterItem.Distribute)))
                        changes.Add(GetUpdateChangeFixedPayrollRow(beforeItem, afterItem, diffProp, TermGroup_TrackChangesColumnType.FixedPayrollRow_Distribute));
                }
            }

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO GetUpdateChangeFixedPayrollRow(FixedPayrollRow old, FixedPayrollRow tax, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown, string fromValueName = null, string toValueName = null)
        {
            return CreateUpdateChange(old, tax, prop, tax.EmployeeId, tax.EmployeeId, old.FixedPayrollRowId, SoeEntityType.FixedPayrollRow, SoeEntityType.Employee, columnType, termGroup, fromValueName, toValueName);
        }

        #endregion

        #region ContactPerson

        public List<TrackChangesDTO> CreateChangesContactPerson()
        {
            List<TrackChangesDTO> contactPersonChanges = new List<TrackChangesDTO>();

            if (this.contactPersonBeforeIsSet && this.contactPersonAfterIsSet)
            {
                if (contactPersonBefore == null && contactPersonAfter != null)
                    contactPersonChanges.Add(this.CreateInsertChange(contactPersonAfter.ActorContactPersonId, contactPersonAfter.ActorContactPersonId, SoeEntityType.ContactPerson));
                else if (contactPersonBefore != null && contactPersonAfter != null)
                    contactPersonChanges.AddRange(this.CreateContactPersonUpdateChanges());
            }

            return contactPersonChanges;
        }

        private List<TrackChangesDTO> CreateContactPersonUpdateChanges()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (this.contactPersonBefore == null || this.contactPersonAfter == null)
                return changes;

            var diffProps = GetDifferences(this.contactPersonBefore, this.contactPersonAfter);
            if (diffProps.IsNullOrEmpty())
                return changes;

            foreach (var prop in this.contactPersonBefore.GetProperties())
            {
                var diffProp = diffProps.FirstOrDefault(f => f.Name == prop.Name);
                if (diffProp == null)
                    continue;

                if (diffProp.Name.Equals(nameof(this.contactPersonBefore.FirstName)))
                    changes.Add(GetUpdateChangeContactPerson(this.contactPersonBefore, this.contactPersonAfter, diffProp, TermGroup_TrackChangesColumnType.ContactPerson_FirstName));
                else if (diffProp.Name.Equals(nameof(this.contactPersonBefore.LastName)))
                    changes.Add(GetUpdateChangeContactPerson(this.contactPersonBefore, this.contactPersonAfter, diffProp, TermGroup_TrackChangesColumnType.ContactPerson_LastName));
                else if (diffProp.Name.Equals(nameof(this.contactPersonBefore.Position)))
                    changes.Add(GetUpdateChangeContactPerson(this.contactPersonBefore, this.contactPersonAfter, diffProp, TermGroup_TrackChangesColumnType.ContactPerson_Position));
                else if (diffProp.Name.Equals(nameof(this.contactPersonBefore.Description)))
                    changes.Add(GetUpdateChangeContactPerson(this.contactPersonBefore, this.contactPersonAfter, diffProp, TermGroup_TrackChangesColumnType.ContactPerson_Description));
                else if (diffProp.Name.Equals(nameof(this.contactPersonBefore.SocialSec)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedSocialSec))
                    changes.Add(GetUpdateChangeContactPerson(this.contactPersonBefore, this.contactPersonAfter, diffProp, TermGroup_TrackChangesColumnType.ContactPerson_SocialSec));
                else if (diffProp.Name.Equals(nameof(this.contactPersonBefore.Sex)) && (this.applyFeaturesResult == null || !this.applyFeaturesResult.HasBlankedSocialSec))
                    changes.Add(GetUpdateChangeContactPerson(this.contactPersonBefore, this.contactPersonAfter, diffProp, TermGroup_TrackChangesColumnType.ContactPerson_Sex));
            }

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO GetUpdateChangeContactPerson(ContactPerson before, ContactPerson after, PropertyInfo prop, TermGroup_TrackChangesColumnType columnType, TermGroup termGroup = TermGroup.Unknown, string fromValueName = null, string toValueName = null)
        {
            if (before?.Employee?.FirstOrDefault() == null)
                return null;

            return CreateUpdateChange(before, after, prop, before.Employee.First().EmployeeId, before.Employee.First().EmployeeId, before.ActorContactPersonId, SoeEntityType.ContactPerson, SoeEntityType.Employee, columnType, termGroup, fromValueName, toValueName);
        }

        #endregion

        #region ContactAddress (ECom/Address)

        public List<TrackChangesDTO> CreateChangesEComAndAddress()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();
            changes.AddRange(this.CreateChangesECom());
            changes.AddRange(this.CreateChangesAddress());

            return changes;
        }

        #region ECom

        private List<TrackChangesDTO> CreateChangesECom()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (!this.EmployeeId.HasValue || !this.contactAddressItemsBeforeIsSet || !this.contactAddressItemsAfterIsSet)
                return changes;

            #region Insert/Update

            if (this.contactAddressItemAfter != null)
            {
                foreach (var afterItem in this.contactAddressItemAfter.Where(w => w.ContactEComId != 0))
                {
                    var beforeItem = this.contactAddressItemBefore?.FirstOrDefault(w => w.ContactEComId == afterItem.ContactEComId);
                    if (beforeItem == null)
                    {
                        changes.Add(CreateInsertChangeECom(afterItem));
                    }
                    else
                    {
                        var diffProps = GetDifferences(beforeItem, afterItem);
                        if (diffProps != null)
                        {
                            foreach (var afterProp in afterItem.GetProperties())
                            {
                                var diffProp = diffProps.FirstOrDefault(f => f.Name == afterProp.Name);
                                if (diffProp == null)
                                    continue;

                                if (diffProp.Name.Equals(nameof(afterItem.Name)))
                                    changes.Add(CreateUpdateChangeECom(beforeItem, afterItem, TermGroup_TrackChangesColumnType.ContactEcom_Name));
                                else if (diffProp.Name.Equals(nameof(afterItem.EComText)))
                                    changes.Add(CreateUpdateChangeECom(beforeItem, afterItem, TermGroup_TrackChangesColumnType.ContactEcom_Text));
                                else if (diffProp.Name.Equals(nameof(afterItem.EComDescription)))
                                    changes.Add(CreateUpdateChangeECom(beforeItem, afterItem, TermGroup_TrackChangesColumnType.ContactEcom_Description));
                                else if (diffProp.Name.Equals(nameof(afterItem.IsSecret)))
                                    changes.Add(CreateUpdateChangeECom(beforeItem, afterItem, TermGroup_TrackChangesColumnType.ContactEcom_IsSecret));
                            }
                        }
                    }
                }
            }

            #endregion

            #region Delete

            if (this.contactAddressItemBefore != null)
            {
                foreach (var beforeItem in this.contactAddressItemBefore.Where(w => w.ContactEComId != 0))
                {
                    var afterItem = this.contactAddressItemAfter?.FirstOrDefault(w => w.ContactEComId == beforeItem.ContactEComId);
                    if (afterItem == null)
                        changes.Add(CreateDeleteChangeECom(beforeItem));
                }
            }

            #endregion

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO CreateInsertChangeECom(ContactAddressItem afterItem)
        {
            return CreateInsertChange(this.EmployeeId.Value, afterItem.ContactEComId, this.EmployeeId, SoeEntityType.ContactECom, SoeEntityType.Employee, columnType: TermGroup_TrackChangesColumnType.ContactEcom_Name, toValue: afterItem.ToEComString(), columnName: afterItem.TypeName, topEntity: SoeEntityType.Employee);
        }

        private TrackChangesDTO CreateUpdateChangeECom(ContactAddressItem beforeItem, ContactAddressItem afterItem, TermGroup_TrackChangesColumnType columnType)
        {
            return CreateUpdateChange(this.EmployeeId.Value, beforeItem.ContactEComId, this.EmployeeId, SoeEntityType.ContactECom, SoeEntityType.Employee, SettingDataType.String, beforeItem.ToEComString(), afterItem.ToEComString(), columnType: columnType, columnName: afterItem.TypeName, topEntity: SoeEntityType.Employee);
        }

        private TrackChangesDTO CreateDeleteChangeECom(ContactAddressItem beforeItem)
        {
            return CreateDeleteChange(this.EmployeeId.Value, beforeItem.ContactEComId, this.EmployeeId, SoeEntityType.ContactECom, SoeEntityType.Employee, fromValue: beforeItem.ToEComString(), columnType: TermGroup_TrackChangesColumnType.ContactEcom_Name, columnName: beforeItem.TypeName, topEntity: SoeEntityType.Employee);
        }

        #endregion

        #region Address

        private List<TrackChangesDTO> CreateChangesAddress()
        {
            List<TrackChangesDTO> changes = new List<TrackChangesDTO>();

            if (!this.EmployeeId.HasValue || !this.contactAddressItemsBeforeIsSet || !this.contactAddressItemsAfterIsSet)
                return changes;

            #region Insert/Update

            if (this.contactAddressItemAfter != null)
            {
                foreach (var afterItem in this.contactAddressItemAfter.Where(w => w.ContactAddressId != 0))
                {
                    var beforeItem = this.contactAddressItemBefore?.FirstOrDefault(w => w.ContactAddressId == afterItem.ContactAddressId);
                    if (beforeItem == null)
                    {
                        changes.Add(CreateInsertChangeAddress(afterItem));
                    }
                    else if (!beforeItem.ToAddressString().Equals(afterItem.ToAddressString()))
                    {
                        changes.Add(CreateUpdateChangeAddress(beforeItem, afterItem));
                    }
                }
            }

            #endregion

            #region Delete

            if (contactAddressItemBefore != null)
            {
                foreach (var beforeItem in this.contactAddressItemBefore.Where(w => w.ContactAddressId != 0))
                {
                    var afterItem = this.contactAddressItemAfter?.FirstOrDefault(w => w.ContactAddressId == beforeItem.ContactAddressId);
                    if (afterItem == null)
                        changes.Add(CreateDeleteChangeAddress(beforeItem));
                }
            }

            #endregion

            return changes.Where(w => w != null).ToList();
        }

        private TrackChangesDTO CreateInsertChangeAddress(ContactAddressItem afterItem)
        {
            return CreateInsertChange(this.EmployeeId.Value, afterItem.ContactAddressId, this.EmployeeId, SoeEntityType.ContactAddress, SoeEntityType.Employee, columnType: TermGroup_TrackChangesColumnType.ContactPerson_Address, toValue: afterItem.ToAddressString(), columnName: afterItem.TypeName, topEntity: SoeEntityType.Employee);
        }

        private TrackChangesDTO CreateUpdateChangeAddress(ContactAddressItem beforeItem, ContactAddressItem afterItem)
        {
            return CreateUpdateChange(this.EmployeeId.Value, beforeItem.ContactAddressId, this.EmployeeId, SoeEntityType.ContactAddress, SoeEntityType.Employee, SettingDataType.String, beforeItem.ToAddressString(), afterItem.ToAddressString(), columnType: TermGroup_TrackChangesColumnType.ContactPerson_Address, columnName: afterItem.TypeName, topEntity: SoeEntityType.Employee);
        }

        private TrackChangesDTO CreateDeleteChangeAddress(ContactAddressItem beforeItem)
        {
            return CreateDeleteChange(this.EmployeeId.Value, beforeItem.ContactAddressId, this.EmployeeId, SoeEntityType.ContactAddress, SoeEntityType.Employee, fromValue: beforeItem.ToAddressString(), columnType: TermGroup_TrackChangesColumnType.ContactPerson_Address, columnName: beforeItem.TypeName, topEntity: SoeEntityType.Employee);
        }

        #endregion

        #endregion
    }
}
