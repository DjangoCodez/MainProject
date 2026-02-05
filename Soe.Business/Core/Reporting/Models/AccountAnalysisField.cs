using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.Reporting.Models
{
    public class AccountAnalysisField
    {
        public AccountAnalysisField(EmployeeAccount employeeAccount)
        {
            AccountId = employeeAccount.AccountId ?? 0;
            Name = employeeAccount.Account?.Name ?? "UnknownName";
            AccountNr = employeeAccount.Account?.AccountNr ?? "UnknownNr";
            AccountDimId = employeeAccount.Account?.AccountDimId ?? 0;
            MainAllocation = employeeAccount.MainAllocation;
            AccountDimNr = employeeAccount.Account?.AccountDim?.AccountDimNr ?? 0;
        }
        public AccountAnalysisField(Account account)
        {
            AccountId = account.AccountId;
            Name = account.Name;
            AccountNr = account.AccountNr;
            AccountDimId = account.AccountDimId;
            AccountDimNr = account.AccountDim?.AccountDimNr ?? 0;
        }

        public AccountAnalysisField(AccountDTO accountDTO)
        {
            AccountId = accountDTO.AccountId;
            Name = accountDTO.Name;
            AccountNr = accountDTO.AccountNr;
            AccountDimId = accountDTO.AccountDimId;
            AccountDimName = accountDTO.DimNameNumberAndName;
            AccountDimNr = accountDTO.AccountDim?.AccountDimNr ?? accountDTO.AccountDimNr;
        }

        public AccountAnalysisField(AccountInternal accountInternal)
        {
            AccountId = accountInternal.AccountId;
            Name = accountInternal.Account?.Name ?? "UnknownName";
            AccountNr = accountInternal.Account?.AccountNr ?? "UnknownNr";
            AccountDimId = accountInternal.Account?.AccountDimId ?? 0;
            AccountDimNr = accountInternal.Account?.AccountDim?.AccountDimNr ?? 0;
        }

        public AccountAnalysisField(AccountInternalDTO accountInternalDTO)
        {
            AccountId = accountInternalDTO.AccountId;
            Name = accountInternalDTO.Name;
            AccountNr = accountInternalDTO.AccountNr;
            AccountDimId = accountInternalDTO.AccountDimId;
            AccountDimNr = accountInternalDTO.Account?.AccountDim?.AccountDimNr ?? accountInternalDTO.AccountDimNr;
        }
        private AccountAnalysisField() { }
        public AccountAnalysisField Clone()
        {
            return new AccountAnalysisField
            {
                AccountDimId = this.AccountDimId,
                AccountDimName = this.AccountDimName,
                AccountDimNr = this.AccountDimNr,
                AccountId = this.AccountId,
                Name = this.Name,
                AccountNr = this.AccountNr,
                ExecutiveName = this.ExecutiveName,
                ExecutiveUserName = this.ExecutiveUserName,
                ExecutiveEmail = this.ExecutiveEmail
            };
        }

        public int AccountDimId { get; set; }
        public bool MainAllocation { get; }
        public string AccountDimName { get; set; }
        public int AccountDimNr { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string AccountNr { get; set; }
        public string ExecutiveName { get; set; }
        public string ExecutiveUserName { get; set; }
        public string ExecutiveEmail { get; set; }
    }

    public static class AccountAnalysisFieldExtensions
    {
        public static string GetAccountAnalysisFieldValueName(this List<AccountAnalysisField> fields, MatrixDefinitionColumn column)
        {
            if (fields == null || fields.Count == 0)
                return string.Empty;

            MatrixDefinitionColumnOptions options = column.Options;
            if (int.TryParse(options.Key, out int accountDimId))
                return GetAccountAnalysisFieldValue(fields, accountDimId, true, false);

            var input = column.Field;
            Match match = Regex.Match(input, @"(\d+)$");

            if (match.Success)
            {
                string digitsString = match.Groups[1].Value;
                int id;
                if (int.TryParse(digitsString, out id))
                {
                    return GetAccountAnalysisFieldValue(fields, id, true, false);
                }
            }

            return string.Empty;
        }
        public static string GetAccountAnalysisFieldValueNumber(this List<AccountAnalysisField> fields, MatrixDefinitionColumn column)
        {
            if (fields == null || fields.Count == 0)
                return string.Empty;

            MatrixDefinitionColumnOptions options = column.Options;
            if (int.TryParse(options.Key, out int accountDimId))
                return GetAccountAnalysisFieldValue(fields, accountDimId, false, true);

            var input = column.Field;
            Match match = Regex.Match(input, @"(\d+)$");

            if (match.Success)
            {
                string digitsString = match.Groups[1].Value;
                int id;
                if (int.TryParse(digitsString, out id))
                {
                    return GetAccountAnalysisFieldValue(fields, id, true, false);
                }
            }

            return string.Empty;
        }

        public static string GetAccountAnalysisFieldValueNumberAndName(this List<AccountAnalysisField> fields, MatrixDefinitionColumnOptions options)
        {
            if (int.TryParse(options.Key, out int accountDimId))
                return GetAccountAnalysisFieldValue(fields, accountDimId, true, true);

            return string.Empty;
        }

        public static string GetAccountAnalysisFieldValue(this List<AccountAnalysisField> fields, int accountDimId, bool name, bool number)
        {
            if (fields.IsNullOrEmpty())
                return string.Empty;

            var matchedField = fields.FirstOrDefault(f => f.AccountDimId == accountDimId && f.MainAllocation);

            if (matchedField == null)
                matchedField = fields.FirstOrDefault(f => f.AccountDimId == accountDimId);

            if (matchedField == null)
                return string.Empty;

            if (number && name)
                return matchedField.AccountNr ?? "" + " " + matchedField.Name ?? "";

            if (number)
                return matchedField.AccountNr ?? "";

            if (name)
                return matchedField.Name ?? "";

            return string.Empty;
        }

        public static List<AccountAnalysisField> AccountAnalysisFields(this List<AccountInternal> accountInternals)
        {
            var fields = new List<AccountAnalysisField>();
            if (accountInternals == null)
                return new List<AccountAnalysisField>();

            foreach (var accountInternal in accountInternals)
                fields.Add(new AccountAnalysisField(accountInternal));

            return fields;
        }

        public static List<AccountAnalysisField> AccountAnalysisFields(this List<AccountInternalDTO> accountInternals)
        {
            var fields = new List<AccountAnalysisField>();
            if (accountInternals == null)
                return new List<AccountAnalysisField>();

            foreach (var accountInternal in accountInternals)
                fields.Add(new AccountAnalysisField(accountInternal));

            return fields;
        }

        public static List<AccountAnalysisField> AccountAnalysisFields(this Employment employment, List<EmployeeAccount> employeeAccounts, List<CompanyCategoryRecord> categoryRecords, List<CategoryAccount> categoryAccounts, List<AccountDTO> accountDTOs, DateTime selectionDateFrom, DateTime? selectionDateTo, int employmentAccountStdId = 0)
        {
            var accountAnalysisFields = new List<AccountAnalysisField>();
            if (!employeeAccounts.IsNullOrEmpty())
                accountAnalysisFields = employeeAccounts.AccountAnalysisFields(accountDTOs, selectionDateFrom, selectionDateTo);
            else if (!categoryRecords.IsNullOrEmpty() && !categoryAccounts.IsNullOrEmpty())
            {
                categoryAccounts = categoryAccounts.Where(w => categoryRecords.Select(s => s.CategoryId).Contains(w.CategoryId)).ToList();
                accountAnalysisFields = categoryAccounts.AccountAnalysisFields(accountDTOs, selectionDateFrom, selectionDateTo);
            }

            EmploymentAccountStd employmentAccount = null;

            if (employmentAccountStdId > 0)
                employmentAccount = employment.EmploymentAccountStd?.FirstOrDefault(x => x.EmploymentAccountStdId == employmentAccountStdId);
            else
                employmentAccount = employment.EmploymentAccountStd?.FirstOrDefault();

            if (employmentAccount != null)
            {
                var fromAccountInternals = GetConnectedAccounts(accountDTOs, employmentAccount.AccountInternal.Select(s => s.AccountId).ToList() ?? new List<int>());

                if (!fromAccountInternals.IsNullOrEmpty())
                {
                    var accountAnalysisFieldsFromAccountInternals = CreateAccountAnalysisFields(fromAccountInternals);

                    foreach (var accountAnalysisFieldsFromAccountInternal in accountAnalysisFieldsFromAccountInternals)
                    {
                        if (!accountAnalysisFields.Any(a => a.AccountDimId == accountAnalysisFieldsFromAccountInternal.AccountDimId))
                            accountAnalysisFields.Add(accountAnalysisFieldsFromAccountInternal);
                    }
                }
            }

            return accountAnalysisFields;
        }

        public static List<AccountAnalysisField> AccountAnalysisFields(this List<EmployeeAccount> employeeAccounts, List<AccountDTO> accountDTOs, DateTime selectionDateFrom, DateTime? selectionDateTo, bool focusOnOnlyHierarchyAccounts = false, bool doNotFocusOnOnlyHierarchyAccounts = false)
        {
            var employeeAccountsOnEmployee = employeeAccounts.GetEmployeeAccounts(selectionDateFrom, selectionDateTo);
            var filteredEmployeeAccountsOnEmployee = new List<EmployeeAccount>();

            foreach (var dimGroup in employeeAccountsOnEmployee.GroupBy(g => g.Account.AccountDimId))
            {

                if (focusOnOnlyHierarchyAccounts)
                {
                    bool added = false;
                    var onDimGroup = dimGroup.Where(w => w.Account.HierarchyOnly).ToList();

                    if (onDimGroup.Count() > 1)
                    {
                        var mainAllocations = dimGroup.Where(w => w.MainAllocation).ToList();

                        if (mainAllocations.Count == 1)
                        {
                            filteredEmployeeAccountsOnEmployee.AddRange(mainAllocations);
                            added = true;
                        }
                    }
                    else if (onDimGroup.Count() == 1)
                    {
                        filteredEmployeeAccountsOnEmployee.AddRange(onDimGroup);
                        added = true;
                    }
                    if (added)
                        continue;
                }
                else if (doNotFocusOnOnlyHierarchyAccounts)
                {
                    bool added = false;
                    var onDimGroup = dimGroup.Where(w => !w.Account.HierarchyOnly).ToList();

                    if (onDimGroup.Count() > 1)
                    {
                        var mainAllocations = dimGroup.Where(w => w.MainAllocation).ToList();

                        if (mainAllocations.Count == 1)
                        {
                            filteredEmployeeAccountsOnEmployee.AddRange(mainAllocations);
                            added = true;
                        }
                    }
                    else if (onDimGroup.Count() == 1)
                    {
                        filteredEmployeeAccountsOnEmployee.AddRange(onDimGroup);
                        added = true;
                    }

                    if (added)
                        continue;
                }


                if (dimGroup.Count() > 1)
                {
                    var mainAllocations = dimGroup.Where(w => w.MainAllocation).ToList();

                    if (mainAllocations.Count == 1)
                        filteredEmployeeAccountsOnEmployee.AddRange(mainAllocations);
                    else
                    {

                        //If more than on account with same dim, check if any of them is default
                        var defaults = dimGroup.Where(w => w.Default).ToList();

                        if (defaults.Count == 1)
                            filteredEmployeeAccountsOnEmployee.AddRange(defaults);
                        else
                        {
                            //If more than one default, check if any of them is active at the end of the interval
                            var enddate = defaults.GetEmployeeAccounts(selectionDateTo, selectionDateTo);

                            if (enddate.Any())
                                filteredEmployeeAccountsOnEmployee.AddRange(enddate);
                            else
                            {
                                //If no default is active at the end of the interval, check if any of them is active at the start of the interval
                                var startdate = defaults.GetEmployeeAccounts(selectionDateFrom, selectionDateFrom);

                                if (startdate.Any())
                                    filteredEmployeeAccountsOnEmployee.AddRange(startdate);
                                else
                                    filteredEmployeeAccountsOnEmployee.AddRange(defaults);
                            }
                        }
                    }
                }
                else
                    filteredEmployeeAccountsOnEmployee.AddRange(dimGroup);
            }
            var connetedAccounts = GetConnectedAccounts(accountDTOs, filteredEmployeeAccountsOnEmployee.Where(w => w.AccountId.HasValue).Select(s => s.AccountId.Value).ToList());
            return CreateAccountAnalysisFields(connetedAccounts);
        }

        private static List<AccountAnalysisField> AccountAnalysisFields(this List<CategoryAccount> categoryAccounts, List<AccountDTO> accountDTOs, DateTime selectionDateFrom, DateTime? selectionDateTo)
        {
            var connetedAccounts = GetConnectedAccounts(accountDTOs, categoryAccounts.Select(s => s.AccountId).ToList());
            return CreateAccountAnalysisFields(connetedAccounts);
        }

        private static List<AccountAnalysisField> CreateAccountAnalysisFields(List<AccountDTO> accounts)
        {
            var fields = new List<AccountAnalysisField>();
            if (accounts == null)
                return new List<AccountAnalysisField>();

            foreach (var ca in accounts)
                fields.Add(new AccountAnalysisField(ca));

            return fields;
        }

        private static List<AccountDTO> GetConnectedAccounts(List<AccountDTO> accounts, List<int> accountIds)
        {
            List<AccountDTO> accountsOnEmployee = new List<AccountDTO>();

            if (accountIds.Any())
            {
                var onEmployee = accounts.Where(w => accountIds.Contains(w.AccountId)).ToList();

                if (onEmployee.Any())
                {
                    accountsOnEmployee.AddRange(onEmployee);
                    onEmployee.ForEach(f => accountsOnEmployee.AddRange(f.ParentAccounts ?? new List<AccountDTO>()));
                    accountsOnEmployee = accountsOnEmployee.Distinct().ToList();
                }
            }

            return accountsOnEmployee;
        }
    }
}
