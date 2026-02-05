using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class TimeMigrationItem
    {
        public TimeMigrationItem(byte[] content, List<ShiftTypeDTO> shiftTypes, List<CategoryDTO> categories, List<AttestRoleDTO> attestRoles, List<AccountDTO> accounts)
        {
            this.content = content;
            helper = new ExcelHelper();
            ds = helper.GetDataSet(this.content, true);
            ShiftTypeMigrations = new List<ShiftTypeMigration>();
            EmployeeCategoryMigrations = new List<EmployeeCategoryMigration>();
            AttestRoleCategoryMigrations = new List<AttestRoleCategoryMigration>();
            MigrationSettings = new List<MigrationSetting>();
            ShiftTypes = shiftTypes;
            Categories = categories;
            AttestRoles = attestRoles;
            Accounts = accounts;
            CreateMigrationSettings();
            CreateShiftTypeMigrations();

            if (SetAllAccountLevelsOnShiftTypes)
                SetAccountInternalOnShiftType();

            CreateEmployeeCategoryMigrations();
            CreateAttestRoleCategoryMigrations();

        }
        public byte[] content { get; set; }

        public string ValidationErrors()
        {
            string errors = string.Empty;

            errors += ShiftTypeMigrations.Select(s => s.ValidationError).JoinToString();
            errors += EmployeeCategoryMigrations.Select(s => s.ValidationError).JoinToString();
            errors += AttestRoleCategoryMigrations.Select(s => s.ValidationError).JoinToString();
            return errors;
        }
        public List<ShiftTypeMigration> ShiftTypeMigrations { get; set; }
        public List<EmployeeCategoryMigration> EmployeeCategoryMigrations { get; set; }
        public List<AttestRoleCategoryMigration> AttestRoleCategoryMigrations { get; set; }
        public List<MigrationSetting> MigrationSettings { get; set; }
        public List<ShiftTypeDTO> ShiftTypes { get; set; }
        public List<CategoryDTO> Categories { get; set; }
        public List<AttestRoleDTO> AttestRoles { get; set; }
        public List<AccountDTO> Accounts { get; set; }
        public DataSet ds { get; set; }
        private ExcelHelper helper { get; set; }

        private void CreateMigrationSettings()
        {
            #region Data

            if (ds != null)
            {
                foreach (DataRow row in ds.Tables["MigrationSettings"].Rows)
                {
                    MigrationSetting dto = new MigrationSetting();
                    dto.Type = helper.GetStringValue(row, "type", false);
                    dto.Setting = helper.GetStringValue(row, "Setting", false);
                    MigrationSettings.Add(dto);
                }
            }

            #endregion
        }

        private void CreateShiftTypeMigrations()
        {
            #region Data

            if (ds != null)
            {
                foreach (DataRow row in ds.Tables["ShiftTypeMigration"].Rows)
                {
                    ShiftTypeMigration dto = new ShiftTypeMigration();
                    dto.FromShiftTypeCode = helper.GetStringValue(row, "FromShiftTypeCode", false);
                    dto.ToShiftTypeCode = helper.GetStringValue(row, "ToShiftTypeCode", false);
                    dto.FromShiftType = GetShiftType(dto.FromShiftTypeCode);
                    dto.ToShiftType = GetShiftType(dto.ToShiftTypeCode);


                    DateTime? startDateOnShifts = helper.GetDateValue(row, "StartDateOnShifts");
                    DateTime? startDateRecalculateAccountingOnShifts = helper.GetDateValue(row, "StartDateOnShifts");
                    DateTime? startDateRecalculateAccountingOnTransactions = helper.GetDateValue(row, "StartDateOnShifts");

                    dto.StartDateOnShifts = startDateOnShifts.HasValue ? startDateOnShifts.Value : DateTime.Now.AddYears(100);
                    dto.StartDateRecalculateAccountingOnShifts = startDateRecalculateAccountingOnShifts.HasValue ? startDateRecalculateAccountingOnShifts.Value : DateTime.Now.AddYears(100);
                    dto.StartDateRecalculateAccountingOnTransactions = startDateRecalculateAccountingOnTransactions.HasValue ? startDateRecalculateAccountingOnTransactions.Value : DateTime.Now.AddYears(100);

                    if (dto.FromShiftType == null || !ShiftTypeMigrations.Any(a => a.FromShiftType != null && a.FromShiftType.ShiftTypeId == dto.FromShiftType.ShiftTypeId))
                        ShiftTypeMigrations.Add(dto);
                }
            }

            #endregion
        }

        private void CreateEmployeeCategoryMigrations()
        {
            #region Data

            if (ds != null)
            {
                foreach (DataRow row in ds.Tables["EmployeeCategoryMigration"].Rows)
                {
                    EmployeeCategoryMigration dto = new EmployeeCategoryMigration();
                    dto.FromCategoryCode = helper.GetStringValue(row, "FromCategoryCode", false);
                    dto.ToAccountNumber = helper.GetStringValue(row, "ToAccountNumber", false);
                    dto.FromCategory = GetCategory(dto.FromCategoryCode);
                    dto.ToAccount = GetAccount(dto.FromCategory, dto.ToAccountNumber);
                    EmployeeCategoryMigrations.Add(dto);
                }
            }

            #endregion
        }

        private void SetAccountInternalOnShiftType()
        {
            if (SetAllAccountLevelsOnShiftTypes)
            {
                var accountDim = GetAccountDim();

                if (this.SetAllAccountLevelsOnShiftTypesSieDim)
                    accountDim = GetSetAllAccountLevelsOnShiftTypesAccountDimId();

                if (!accountDim.HasValue)
                    accountDim = GetAccountDim();

                if (accountDim.HasValue)
                {
                    foreach (var shift in this.ShiftTypes)
                    {
                        foreach (var item in shift.AccountInternals)
                        {
                            if (item.Value.AccountDimId == 0)
                            {
                                var acc = this.Accounts.FirstOrDefault(f => f.AccountId == item.Value.AccountId);

                                if (acc != null)
                                    item.Value.AccountDimId = acc.AccountDimId;
                            }
                        }

                        var account = shift.AccountInternals.FirstOrDefault(f => f.Value != null && f.Value.AccountDimId == accountDim);

                        List<AccountSmallDTO> internalAndParents = null;

                        if (account.Value != null)
                        {
                            internalAndParents = GetAccountParents(account.Value.AccountId);
                        }
                        else if (shift.AccountId.HasValue)
                        {
                            var acco = this.Accounts.FirstOrDefault(f => f.AccountId == shift.AccountId.Value);

                            if (acco != null && acco.AccountDimId == accountDim)
                            {
                                internalAndParents = GetAccountParents(shift.AccountId.Value);
                                internalAndParents.Add(new AccountSmallDTO() { AccountId = acco.AccountId, AccountDimId = acco.AccountDimId });
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        while (internalAndParents.Any())
                        {
                            var first = internalAndParents.First();

                            if (shift.AccountInternals.Any(a => a.Value != null && a.Value.AccountDimId == first.AccountDimId))
                            {
                                shift.AccountInternals.Remove(shift.AccountInternals.First(a => a.Value != null && a.Value.AccountDimId == first.AccountDimId).Key);
                                shift.AccountInternals.Add(first.AccountId, first);
                            }
                            else
                            {
                                shift.AccountInternals.Add(first.AccountId, first);
                            }

                            internalAndParents.Remove(first);
                        }
                    }
                }
            }
        }

        private void CreateAttestRoleCategoryMigrations()
        {
            #region Data

            if (ds != null)
            {
                foreach (DataRow row in ds.Tables["AttestRoleCategoryMigration"].Rows)
                {
                    AttestRoleCategoryMigration dto = new AttestRoleCategoryMigration();
                    dto.FromAttestRoleCode = helper.GetStringValue(row, "FromAttestRoleCode", false);
                    dto.ToAttestRoleCode = helper.GetStringValue(row, "ToAttestRoleCode", false);
                    dto.FromAttestRole = GetAttestRole(dto.FromAttestRoleCode);
                    dto.ToAttestRole = GetAttestRole(dto.ToAttestRoleCode);
                    AttestRoleCategoryMigrations.Add(dto);
                }
            }

            #endregion
        }

        private ShiftTypeDTO GetShiftType(string code)
        {
            if (this.ShiftTypes == null || string.IsNullOrEmpty(code))
                return null;

            foreach (var type in this.ShiftTypes)
            {
                if (string.IsNullOrEmpty(type.ExternalCode))
                    continue;

                if (type.ExternalCode.Trim().ToLower().Equals(code.Trim().ToLower()))
                    return type;

                int integerValue = 0;

                int.TryParse(code, out integerValue);

                if (integerValue != 0 && type.ExternalCode.Trim().ToLower().Equals(integerValue.ToString()))
                    return type;

            }

            foreach (var type in this.ShiftTypes)
            {
                if (string.IsNullOrEmpty(type.NeedsCode))
                    continue;

                if (type.NeedsCode.Trim().ToLower().Equals(code.Trim().ToLower()))
                    return type;

                int integerValue = 0;

                int.TryParse(code, out integerValue);

                if (integerValue != 0 && type.NeedsCode.Trim().ToLower().Equals(integerValue.ToString()))
                    return type;
            }

            foreach (var type in this.ShiftTypes)
            {
                if (string.IsNullOrEmpty(type.Name))
                    continue;

                if (type.Name.Trim().ToLower().Equals(code.Trim().ToLower()))
                    return type;
            }

            return null;
        }

        private AccountDTO GetAccount(CategoryDTO FromCategory, string code)
        {
            var sieDim = GetAccountSiemDim();

            if (sieDim == TermGroup_SieAccountDim.Invoice || this.Accounts == null || string.IsNullOrEmpty(code))
                return null;

            var account = this.Accounts.FirstOrDefault(a => a.AccountDim != null && a.AccountDim.SysSieDimNr == (int)sieDim && !string.IsNullOrEmpty(a.AccountNr) && a.AccountNr.Trim().ToLower().Equals(code.Trim().ToLower()));

            if (account == null)
                account = this.Accounts.FirstOrDefault(a => a.AccountDim != null && a.AccountDim.SysSieDimNr == (int)sieDim && !string.IsNullOrEmpty(a.ExternalCode) && a.ExternalCode.Trim().ToLower().Equals(code.Trim().ToLower()));

            if (account == null && FromCategory != null)
                account = this.Accounts.FirstOrDefault(a => a.AccountDim != null && a.AccountDim.SysSieDimNr == (int)sieDim && !string.IsNullOrEmpty(a.Name) && a.Name.Trim().ToLower().Equals(FromCategory.Name.Trim().ToLower()));

            return account;
        }

        private CategoryDTO GetCategory(string code)
        {
            if (this.Categories == null || string.IsNullOrEmpty(code))
                return null;

            var category = this.Categories.FirstOrDefault(a => !string.IsNullOrEmpty(a.Code) && a.Code.Trim().ToLower().Equals(code.Trim().ToLower()));
            if (category == null)
                category = this.Categories.FirstOrDefault(a => !string.IsNullOrEmpty(a.Name) && a.Name.Trim().ToLower().Equals(code.Trim().ToLower()));

            return category;
        }

        private AttestRoleDTO GetAttestRole(string code)
        {
            if (this.AttestRoles == null || string.IsNullOrEmpty(code))
                return null;

            var attestRole = this.AttestRoles.FirstOrDefault(f => !f.ExternalCodes.IsNullOrEmpty() && f.ExternalCodes.Any(a => !string.IsNullOrEmpty(a) && a.Trim().ToLower().Equals(code.Trim().ToLower())));
            if (attestRole == null)
                attestRole = this.AttestRoles.FirstOrDefault(a => !string.IsNullOrEmpty(a.Name) && a.Name.Trim().ToLower().Equals(code.Trim().ToLower()));

            return attestRole;
        }

        public List<AccountSmallDTO> GetAccountParents(int accountId)
        {
            List<AccountSmallDTO> accounts = new List<AccountSmallDTO>();
            var currentAccount = this.Accounts.FirstOrDefault(w => w.AccountId == accountId);
            if (currentAccount == null)
                return accounts;

            while (currentAccount != null && currentAccount.ParentAccountId != null && currentAccount.ParentAccountId.HasValue)
            {
                var parentAccount = this.Accounts.FirstOrDefault(w => w.AccountId == currentAccount.ParentAccountId);
                if (parentAccount != null)
                    accounts.Add(new AccountSmallDTO() { AccountId = parentAccount.AccountId, AccountDimId = parentAccount.AccountDimId });
                currentAccount = parentAccount;
            }
            return accounts;
        }

        private TermGroup_SieAccountDim GetAccountSiemDim()
        {
            return this.MigrationSettings.FirstOrDefault(f => f.SieDim != TermGroup_SieAccountDim.Invoice).SieDim;
        }

        private TermGroup_SieAccountDim GetSetAllAccountLevelsOnShiftTypesAccountDim()
        {
            return this.MigrationSettings.FirstOrDefault(f => f.SetAllAccountLevelsOnShiftTypesSieDim != TermGroup_SieAccountDim.Invoice).SetAllAccountLevelsOnShiftTypesSieDim;
        }

        public int? GetAccountDim()
        {
            return this.Accounts.FirstOrDefault(f => f.AccountDim.SysSieDimNr == (int)GetAccountSiemDim())?.AccountDimId;
        }
        public bool SetAllAccountLevelsOnShiftTypes
        {
            get
            {
                return this.MigrationSettings.Any(f => f.SetAllAccountLevelsOnShiftTypes);
            }
        }

        public bool SetAllAccountLevelsOnShiftTypesSieDim
        {
            get
            {
                return this.MigrationSettings.Any(f => f.SetAllAccountLevelsOnShiftTypesSieDim != TermGroup_SieAccountDim.Invoice);
            }
        }

        public int? GetSetAllAccountLevelsOnShiftTypesAccountDimId()
        {
            return this.Accounts.FirstOrDefault(f => f.AccountDim.SysSieDimNr == (int)GetSetAllAccountLevelsOnShiftTypesAccountDim())?.AccountDimId;
        }
    }

    public class AttestRoleCategoryMigration
    {
        #region in Excel spreadsheet

        public string FromAttestRoleCode { get; set; }
        public string ToAttestRoleCode { get; set; }

        #endregion

        #region internal

        public string ValidationError
        {
            get
            {
                string error = string.Empty;

                if (FromAttestRole == null)
                    error += $"FromAttestRoleCode {FromAttestRoleCode} can not be matched" + Environment.NewLine;

                if (ToAttestRole == null)
                    error += $"ToAttestRoleCode {ToAttestRoleCode} can not be matched" + Environment.NewLine;

                return error;
            }
        }

        public AttestRoleDTO FromAttestRole { get; set; }
        public AttestRoleDTO ToAttestRole { get; set; }

        #endregion
    }

    public class EmployeeCategoryMigration
    {
        #region in Excel spreadsheet

        public string FromCategoryCode { get; set; }
        public string ToAccountNumber { get; set; }
        public string DimNr { get; set; }

        #endregion

        #region internal

        public string ValidationError
        {
            get
            {
                string error = string.Empty;

                if (FromCategory == null)
                    error += $"FromCategoryCode {FromCategoryCode} can not be matched" + Environment.NewLine;

                if (ToAccount == null)
                    error += $"ToAccountNumber {ToAccountNumber} can not be matched" + Environment.NewLine;

                return error;
            }
        }

        public CategoryDTO FromCategory { get; set; }
        public AccountDTO ToAccount { get; set; }

        #endregion
    }

    public class MigrationSetting
    {
        #region in Excel spreadsheet

        public string Type { get; set; }
        public string Setting { get; set; }

        #endregion

        #region internal

        public TermGroup_SieAccountDim SieDim
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Type) && this.Type.Trim().ToLower() == "siedim" && !string.IsNullOrEmpty(this.Setting))
                {
                    int value = 0;
                    if (int.TryParse(this.Setting, out value))
                    {
                        if (value != 0 && Enum.IsDefined(typeof(TermGroup_SieAccountDim), value))
                            return (TermGroup_SieAccountDim)value;
                    }
                    return TermGroup_SieAccountDim.Invoice;
                }
                else
                    return TermGroup_SieAccountDim.Invoice;
            }
        }

        public bool SetAllAccountLevelsOnShiftTypes
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Type) && this.Type.Trim().ToLower() == "setallaccountlevelsonshifttype" && !string.IsNullOrEmpty(this.Setting))
                {
                    bool value = false;
                    if (bool.TryParse(this.Setting, out value))
                    {
                        return value;
                    }
                    return false;
                }
                else
                    return false;
            }
        }

        public TermGroup_SieAccountDim SetAllAccountLevelsOnShiftTypesSieDim
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Type) && this.Type.Trim().ToLower() == "setallaccountlevelsonshifttypesiedim" && !string.IsNullOrEmpty(this.Setting))
                {
                    int value = 0;
                    if (int.TryParse(this.Setting, out value))
                    {
                        if (value != 0 && Enum.IsDefined(typeof(TermGroup_SieAccountDim), value))
                            return (TermGroup_SieAccountDim)value;
                    }
                    return TermGroup_SieAccountDim.Invoice;
                }
                else
                    return TermGroup_SieAccountDim.Invoice;
            }
        }

        #endregion
    }

    public class ShiftTypeMigration
    {
        #region in Excel spreadsheet

        public string FromShiftTypeCode { get; set; }
        public string ToShiftTypeCode { get; set; }
        public DateTime StartDateOnShifts { get; set; }
        public DateTime StartDateRecalculateAccountingOnShifts { get; set; }
        public DateTime StartDateRecalculateAccountingOnTransactions { get; set; }
        public List<string> EmployeeNr { get; set; }

        #endregion

        #region internal

        public string ValidationError
        {
            get
            {
                string error = string.Empty;

                if (FromShiftType == null)
                    error += $"FromShiftTypeCode {FromShiftTypeCode} can not be matched" + Environment.NewLine;

                if (ToShiftTypeCode == null)
                    error += $"ToShiftTypeCode {ToShiftTypeCode} can not be matched" + Environment.NewLine;

                return error;
            }
        }

        public ShiftTypeDTO FromShiftType { get; set; }
        public ShiftTypeDTO ToShiftType { get; set; }

        #endregion
    }
}
