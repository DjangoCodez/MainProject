using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util.Logger;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Common.Util
{
    public static class Extensions
    {
        #region Tables

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertiesCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static PropertyInfo[] GetCachedProperties<T>(T source)
        {
            Type type = source.GetType();
            if (!propertiesCache.TryGetValue(type, out PropertyInfo[] properties))
            {
                properties = type.GetProperties().Where(pi => pi.CanWrite).ToArray();
                propertiesCache[type] = properties;
            }
            return properties;
        }

        public static List<T> CloneDTOs<T>(this List<T> source)
        {
            List<T> dtos = new List<T>();
            foreach (T dto in source)
            {
                dtos.Add(dto.CloneDTO());
            }
            return dtos;
        }

        public static T CloneDTO<T>(this T source)
        {
            Type t = source.GetType();
            T clone = (T)Activator.CreateInstance(t);

            var clonePis = GetCachedProperties(clone);
            foreach (var clonePi in clonePis)
            {
                PropertyInfo prototypePi = GetCachedProperties(source).FirstOrDefault(p => p.Name == clonePi.Name);
                if (prototypePi != null)
                {
                    clonePi.SetValue(clone, prototypePi.GetValue(source, null), null);
                }
            }

            return clone;
        }

        #region AccountDTO

        public static List<AccountDTO> ForAccountDim(this List<AccountDTO> l, int accountDimId)
        {
            return l?.Where(e => e.AccountDimId == accountDimId).ToList() ?? new List<AccountDTO>();
        }

        public static List<AccountDTO> ByName(this List<AccountDTO> l)
        {
            return l?.OrderBySorting(i => i.Name) ?? new List<AccountDTO>();
        }

        public static List<T> OrderBySorting<T, TKey>(this List<T> list, Func<T, TKey> keySelector)
        {
            if (list == null || list.Count <= 1)
                return list ?? new List<T>();

            list.Sort((x, y) => Comparer<TKey>.Default.Compare(keySelector(x), keySelector(y)));
            return list;
        }

        public static List<AccountDTO> GetParentAccounts(this AccountDTO e, List<AccountDTO> accounts)
        {
            List<AccountDTO> parentAccounts = new List<AccountDTO>();

            AccountDTO currentAccount = e;
            while (currentAccount != null && currentAccount.ParentAccountId.HasValue)
            {
                AccountDTO parentAccount = accounts?.GetAccount(currentAccount.ParentAccountId.Value);
                if (parentAccount != null)
                    parentAccounts.Add(parentAccount);
                currentAccount = parentAccount;
            }

            return parentAccounts;
        }

        public static List<AccountDTO> GetChildrens(this AccountDTO e, List<AccountDTO> accounts, bool oneLevel = true)
        {
            if (accounts.IsNullOrEmpty())
                return new List<AccountDTO>();

            var childAccounts = accounts.Where(account => account.HasParentAndReset(e)).ToList();
            if (!oneLevel)
            {
                foreach (AccountDTO childAccount in childAccounts.ToList()) //prevent collection was modified...
                {
                    childAccounts.AddRange(childAccount.GetChildrens(accounts, oneLevel: false));
                }
            }

            return childAccounts;
        }

        public static List<AccountDTO> GetIdentifiableAccounts(this List<AccountDTO> l, List<int> accountIds)
        {
            if (l.IsNullOrEmpty() || accountIds.IsNullOrEmpty())
                return new List<AccountDTO>();
            if (accountIds.Count == 1)
                return l.Where(i => i.AccountId == accountIds.First()).ToList();

            List<AccountDTO> valid = new List<AccountDTO>();

            AccountDTO prev = null;
            for (int pos = accountIds.Count - 1; pos >= 0; pos--)
            {
                AccountDTO e = l.FirstOrDefault(i => i.AccountId == accountIds[pos]);
                if (e != null)
                {
                    valid.Add(e);
                    if (prev != null && !prev.VirtualParentAccountId.HasValue)
                        prev.VirtualParentAccountId = e.AccountId;

                    if (e.ParentAccountId.HasValue)
                        break; //Continue until one account has parent (and thus can be identified)
                    else
                        prev = e;
                }
            }

            return valid;
        }

        public static List<AccountDTO> OrderByAccount(this IEnumerable<AccountDTO> accounts)
        {
            if (accounts.IsNullOrEmpty())
                return new List<AccountDTO>();

            List<AccountDTO> orderedAccounts = new List<AccountDTO>();

            int startLevel = accounts.Min(i => i.NoOParentHierachys);
            List<AccountDTO> topLevelAccounts = accounts.Where(i => i.NoOParentHierachys == startLevel).ToList();
            foreach (AccountDTO topLevelAccount in topLevelAccounts.OrderBy(i => i.Name))
            {
                orderedAccounts.Add(topLevelAccount);
                orderedAccounts.AddRange(accounts.OrderByAccount(topLevelAccount, topLevelAccount.NoOParentHierachys + 1));
            }

            foreach (AccountDTO account in accounts.OrderBy(i => i.Name))
            {
                if (!orderedAccounts.Any(i => i.AccountId == account.AccountId))
                    orderedAccounts.Add(account);
            }

            return orderedAccounts;
        }

        public static List<AccountDTO> OrderByAccount(this IEnumerable<AccountDTO> accounts, AccountDTO parentAccount, int level)
        {
            List<AccountDTO> orderedAccounts = new List<AccountDTO>();
            if (accounts.IsNullOrEmpty() || parentAccount == null || level <= 0)
                return orderedAccounts;

            List<AccountDTO> childrenAccounts = accounts.Where(account => account.IsParent(parentAccount, level)).OrderBy(i => i.Name).ToList();
            foreach (AccountDTO account in childrenAccounts)
            {
                orderedAccounts.Add(account);
                orderedAccounts.AddRange(accounts.OrderByAccount(account, level + 1));
            }

            return orderedAccounts;
        }

        public static List<AccountDTO> GetHighestAccounts(this List<AccountDTO> accounts, List<AccountDimDTO> accountDims)
        {
            accountDims.CalculateLevels();
            AccountDimDTO highestAccountDim = accountDims.OrderBy(i => i.Level).FirstOrDefault();
            return highestAccountDim != null ? accounts.Where(i => i.AccountDimId == highestAccountDim.AccountDimId && !i.ParentAccountId.HasValue).ToList() : new List<AccountDTO>();
        }

        public static List<AccountDTO> GetFilteredAccounts(this List<AccountDTO> accounts, List<int> accountIds)
        {
            if (accountIds.IsNullOrEmpty())
                return accounts;

            List<AccountDTO> filteredAccounts = new List<AccountDTO>();
            foreach (int accountId in accountIds)
            {
                AccountDTO account = accounts.FirstOrDefault(i => i.AccountId == accountId);
                if (account != null)
                    filteredAccounts.Add(account);
            }
            return filteredAccounts;
        }

        public static Dictionary<int, AccountDTO> ToDict(this List<AccountDTO> accounts)
        {
            Dictionary<int, AccountDTO> dict = new Dictionary<int, AccountDTO>();
            if (!accounts.IsNullOrEmpty())
            {
                foreach (AccountDTO account in accounts)
                {
                    if (!dict.ContainsKey(account.AccountId))
                        dict.Add(account.AccountId, account);
                }
            }
            return dict;
        }

        public static Dictionary<string, string> GetHierarchies(this List<AccountDTO> accounts)
        {
            Dictionary<string, string> accountHierarchy = new Dictionary<string, string>();
            if (accounts.IsNullOrEmpty())
                return accountHierarchy;

            List<AccountDTO> accountsOrdered = accounts.OrderByAccount();
            foreach (AccountDTO account in accountsOrdered)
            {
                foreach (var hiearchy in account.GetHierarchys())
                {
                    if (!accountHierarchy.ContainsKey(hiearchy.Key))
                        accountHierarchy.Add(hiearchy.Key, hiearchy.Value);
                }
            }

            return accountHierarchy;
        }

        public static Dictionary<int, string> GetHierarchyState(this List<AccountDTO> accounts)
        {
            return accounts?.ToDictionary(k => k.AccountId, v => v.AccountHierarchyUniqueId) ?? new Dictionary<int, string>();
        }

        public static void ApplyHierarchyState(this AccountDTO account, Dictionary<int, string> hierarchieStates)
        {
            if (account == null || hierarchieStates.IsNullOrEmpty())
                return;

            var hierarchieState = hierarchieStates.GetValue(account.AccountId);
            if (!hierarchieState.IsNullOrEmpty())
                account.ResetAccountHierarchy(hierarchieState);
        }

        public static AccountDTO GetAccount(this List<AccountDTO> accounts, int accountId)
        {
            return accounts?.FirstOrDefault(i => i.AccountId == accountId && i.State == (int)SoeEntityState.Active);
        }

        public static AccountDTO GetAccount(this List<AccountDTO> l, string externalCodeOrNumber)
        {
            return
                l?.FirstOrDefault(e => e.ExternalCode == externalCodeOrNumber) ??
                l?.FirstOrDefault(e => e.AccountNr == externalCodeOrNumber);
        }
        public static AccountDTO GetAccount(this List<AccountDTO> l, string externalCodeOrNumber, TermGroup_SieAccountDim sie)
        {
            return
                l?.FirstOrDefault(e => e.ExternalCode == externalCodeOrNumber && e.AccountDim?.SysSieDimNr == (int)sie) ??
                l?.FirstOrDefault(e => e.AccountNr == externalCodeOrNumber && e.AccountDim?.SysSieDimNr == (int)sie);
        }

        public static AccountDTO GetAccountAndResetHierarchy(this Dictionary<int, AccountDTO> accounts, int accountId, params AccountDTO[] parentAccounts)
        {
            AccountDTO account = accounts.GetValue(accountId);
            if (account != null)
                account.ResetAccountHierarchy(parentAccounts);
            return account;
        }

        public static string GetAccountHierarchyId(this Dictionary<int, AccountDTO> l, AccountDTO parentAccount, AccountDTO childAccount)
        {
            string hierarchyId = childAccount?.AccountId.ToString() ?? string.Empty;

            if (!l.IsNullOrEmpty())
            {
                AccountDTO currrentAccount = parentAccount;
                while (currrentAccount != null)
                {
                    string prefix = currrentAccount.AccountIdWithDelimeter;
                    if (hierarchyId.Contains(prefix))
                        return string.Empty; //invalid - prevent eternity loops

                    hierarchyId = string.Concat(prefix, hierarchyId);
                    currrentAccount = currrentAccount.ParentAccountId.HasValue ? l.GetValue(currrentAccount.ParentAccountId.Value) : null;
                }
            }

            return hierarchyId;
        }

        public static string GetAccountNames(this IEnumerable<AccountDTO> accounts)
        {
            List<string> names = accounts?.Select(a => a.Name).ToList() ?? new List<string>();
            return string.Join(AccountDTO.HIERARCHYDELIMETER.ToString(), names);
        }

        public static bool ContainsAccountDim(this IEnumerable<AccountDTO> l, int accountDimId)
        {
            return l?.Any(e => e.AccountDimId == accountDimId) ?? false;
        }

        public static bool DoParentHierachyContainsAny(this AccountDTO a, List<int> accountIds)
        {
            if (a?.ParentHierachy == null || accountIds == null)
                return false;

            return a.ParentHierachy.Any(h => accountIds.Contains(h.Key));
        }

        public static bool IsAccountOrChildValidInFilter(this AccountDTO account, List<AccountDTO> childAccounts, List<int> filterAccountIds, out bool isAccountAbstract)
        {
            isAccountAbstract = false;
            if (filterAccountIds == null)
                return true; //No filter
            if (account.IsAccountInFilter(filterAccountIds))
                return true;
            if (childAccounts != null)
            {
                foreach (AccountDTO childAccount in childAccounts)
                {
                    if (childAccount.IsAccountInFilter(filterAccountIds))
                    {
                        isAccountAbstract = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsAccountInFilter(this AccountDTO account, List<int> filterAccountIds)
        {
            if (account != null)
            {
                if (filterAccountIds == null)
                    return true; //No filter
                if (filterAccountIds.Contains(account.AccountId))
                    return true; //Filtered by account
                if (account.ParentAccounts != null && filterAccountIds.ContainsAny(account.ParentAccounts.Select(i => i.AccountId)))
                    return true; //Filtered by parent account
            }
            return false;
        }

        public static bool TryGetVirtualParent(this AccountDTO e, List<AccountDTO> accounts, out AccountDTO parentAccount)
        {
            parentAccount = e?.VirtualParentAccountId != null ? accounts?.FirstOrDefault(a => a.AccountId == e.VirtualParentAccountId.Value) : null;
            return parentAccount != null;
        }

        public static void TrySetVirtualParentId(this AccountDTO e, AccountDTO parentAccount)
        {
            if (e != null && !e.ParentAccountId.HasValue && parentAccount != null)
                e.VirtualParentAccountId = parentAccount.AccountId;
        }

        public static void ResetVirtualParentId(this List<AccountDTO> l)
        {
            if (!l.IsNullOrEmpty())
                l.Where(a => a.VirtualParentAccountId.HasValue).ToList().ForEach(a => a.VirtualParentAccountId = null);
        }

        #endregion

        #region AccountDim

        public static AccountDimDTO GetAccountDim(this List<AccountDimDTO> accountDims, AccountDTO account)
        {
            return accountDims?.FirstOrDefault(i => i.AccountDimId == account?.AccountDimId && i.State == (int)SoeEntityState.Active);
        }

        public static List<AccountDimDTO> GetChildrensByName(this AccountDimDTO accountDim, List<AccountDimDTO> accountDims, bool fallbackToDimNr = false)
        {
            if (accountDim == null || accountDims == null)
                return new List<AccountDimDTO>();

            var result = accountDims.Where(i => i.ParentAccountDimId.HasValue && i.ParentAccountDimId.Value == accountDim.AccountDimId).OrderBy(i => i.Name).ToList();
            if (result.IsNullOrEmpty() && fallbackToDimNr)
                result = accountDims.Where(i => i.AccountDimNr > accountDim.AccountDimNr).OrderBy(o => o.AccountDimNr).FirstOrDefault()?.ObjToList();

            if (result?.FirstOrDefault() == null)
                return new List<AccountDimDTO>();

            return result;
        }

        public static void CalculateLevels(this List<AccountDimDTO> accountDims)
        {
            accountDims = accountDims.Where(i => i.IsInternal && i.State == SoeEntityState.Active).ToList();

            bool isCalculateNeeded = accountDims.Any(i => i.Level <= 0);
            if (!isCalculateNeeded)
                return;

            //Can only have 1 highest, otherwise invalid setup
            if (accountDims.Count(i => !i.ParentAccountDimId.HasValue) != 1)
            {
                var accountDimsWithoutParent = accountDims.Where(i => !i.ParentAccountDimId.HasValue && accountDims.Select(s => s.ParentAccountDimId).Contains(i.AccountDimId)).OrderBy(f => f.AccountDimNr).ToList();
                AccountDimDTO accountDimTopLevel = accountDimsWithoutParent?.FirstOrDefault(i => !i.ParentAccountDimId.HasValue);
                if (accountDimTopLevel != null)
                    accountDims.CalculateLevels(accountDimTopLevel, 1, accountDimsWithoutParent.Count > 1);
            }
            else
            {
                AccountDimDTO accountDimTopLevel = accountDims.FirstOrDefault(i => !i.ParentAccountDimId.HasValue);
                accountDims.CalculateLevels(accountDimTopLevel, 1, false);
            }

            if (accountDims.Exists(ad => ad.Level <= 0 && !ad.ParentAccountDimId.HasValue) && accountDims.Exists(ad => ad.Level == 1 && !ad.ParentAccountDimId.HasValue))
                accountDims.Where(ad => ad.Level <= 0 && !ad.ParentAccountDimId.HasValue).ToList().ForEach(ad => ad.Level = Int32.MaxValue);
        }

        public static void CalculateLevels(this List<AccountDimDTO> accountDims, AccountDimDTO parentAccountDim, int level, bool fallbackToDimNr)
        {
            if (parentAccountDim == null)
                return;

            parentAccountDim.Level = level;

            level++;
            foreach (AccountDimDTO accountDimChild in parentAccountDim.GetChildrensByName(accountDims, fallbackToDimNr))
            {
                if (level > 10)
                    return;

                accountDims.CalculateLevels(accountDimChild, level, fallbackToDimNr);
            }
        }

        #endregion

        #region AttestEmployeeDayDTO

        public static string GetComments(this AttestEmployeeDayDTO e)
        {
            return e.AttestPayrollTransactions.GetComments();
        }

        public static bool EvaluateNoneInitialTransactions(this AttestEmployeeDayDTO e, List<AttestPayrollTransactionDTO> transactions, bool discardExpense)
        {
            if (e == null || e.AttestStates.IsNullOrEmpty() || transactions.IsNullOrEmpty())
                return false;
            if (!e.AttestStates.Any(i => i.AttestStateId > 0 && !i.Initial))
                return false;

            foreach (var transaction in transactions.Where(i => i.AttestStateId > 0))
            {
                if (discardExpense && (transaction.IsAdditionOrDeduction))
                    continue;

                var attestState = e.AttestStates.FirstOrDefault(i => i.AttestStateId == transaction.AttestStateId);
                if (attestState != null && !attestState.Initial)
                    return true;
            }

            return false;
        }

        public static bool EvaluateIsDayAttested(this AttestEmployeeDayDTO e, AttestStateDTO attestStateSalaryExportPayrollMin, List<AttestPayrollTransactionDTO> transactions, bool discardExpense)
        {
            if (e == null || e.AttestStates.IsNullOrEmpty() || attestStateSalaryExportPayrollMin == null)
                return false;
            if (!e.HasSameAttestState)
                return false;
            if (discardExpense && !transactions.Any(i => !i.IsAdditionOrDeduction))
                return false;

            AttestStateDTO attestState = e.AttestStates.FirstOrDefault(i => i.AttestStateId > 0);
            return attestState != null && (attestState.AttestStateId == attestStateSalaryExportPayrollMin.AttestStateId || attestState.Sort > attestStateSalaryExportPayrollMin.Sort);
        }

        #endregion

        #region AttestPayrollTransactionDTO

        public static string GetComments(this IEnumerable<AttestPayrollTransactionDTO> l)
        {
            StringBuilder text = new StringBuilder();

            foreach (var e in l)
            {
                if (text.Length > 0)
                    text.Append("\r\n");
                text.Append(e.Comment);
            }

            return text.ToString();
        }

        public static IEnumerable<int> GetAttestStateIds(this IEnumerable<AttestPayrollTransactionDTO> l)
        {
            return l.Select(i => i.AttestStateId).Distinct();
        }

        public static List<AttestStateDTO> GetAttestStates(this IEnumerable<AttestPayrollTransactionDTO> l)
        {
            List<AttestStateDTO> attestStates = new List<AttestStateDTO>();

            foreach (var e in l)
            {
                if (!attestStates.Any(i => i.AttestStateId == e.AttestStateId))
                    attestStates.Add(e.GetAttestState());
            }

            return attestStates;
        }

        public static AttestStateDTO GetAttestState(this AttestPayrollTransactionDTO e)
        {
            //Create dummy AttestState instead of fetching the real AttestState from db
            return new AttestStateDTO()
            {
                AttestStateId = e.AttestStateId,
                Name = e.AttestStateName,
                Color = e.AttestStateColor,
                Initial = e.AttestStateInitial,
                Sort = e.AttestStateSort,
            };
        }

        #endregion

        #region AttestStateDTO

        public static List<AttestStateDTO> Exclude(this List<AttestStateDTO> l, List<AttestStateDTO> exclude)
        {
            var attestStates = new List<AttestStateDTO>();
            foreach (var e in l)
            {
                if (!attestStates.Exists(i => i.AttestStateId == e.AttestStateId) && !exclude.Exists(i => i.AttestStateId == e.AttestStateId))
                    attestStates.Add(e);
            }
            return attestStates;
        }

        public static string GetAttestStateString(this IEnumerable<AttestStateDTO> l, IEnumerable<int> filterAttestStateIds = null)
        {
            if (l.IsNullOrEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            var list = l.Where(i => i.Sort >= 0).OrderBy(i => i.Sort).ToList();
            list.AddRange(l.Where(i => i.Sort < 0));

            foreach (var e in list.Distinct())
            {
                if (filterAttestStateIds != null && !filterAttestStateIds.Contains(e.AttestStateId))
                    continue;
                if (String.IsNullOrEmpty(e.Name))
                    continue;

                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(e.Name);
            }

            return sb.ToString();
        }

        public static bool Contains(this List<AttestStateDTO> l, int attestStateId)
        {
            return l?.Exists(i => i.AttestStateId == attestStateId) ?? false;
        }

        public static bool ContainsTheSameId(this List<AttestStateDTO> l, params int[] attestStateIds)
        {
            if (l != null)
            {
                int total = l.Count;
                foreach (int attestStateId in attestStateIds)
                {
                    if (total == l.Count(i => i.AttestStateId == attestStateId))
                        return true;
                }
            }
            return false;
        }

        public static bool HasId(this AttestStateDTO e, params int[] attestStateIds)
        {
            return attestStateIds?.Contains(e.AttestStateId) ?? false;
        }

        public static void SetLowestAttestState(this IAttestStateLowest e, IEnumerable<AttestStateDTO> attestStates, string defaultColor = "#FFFFFF")
        {
            AttestStateDTO attestStateLowest = attestStates?.Where(i => i.AttestStateId > 0).OrderBy(i => i.Sort).FirstOrDefault(); //discard warnings
            e.AttestStateId = attestStateLowest?.AttestStateId ?? 0;
            e.AttestStateName = attestStateLowest?.Name ?? attestStates?.OrderBy(i => i.Sort).FirstOrDefault()?.Name ?? string.Empty; //include warnings if no regular atteststate
            e.AttestStateSort = attestStateLowest?.Sort ?? 0;
            e.AttestStateColor = attestStateLowest?.Color ?? defaultColor;
        }

        #endregion

        #region AccountSmallDTO

        public static int GetAccountId(this Dictionary<int, AccountSmallDTO> dict, int key)
        {
            return dict != null && dict.ContainsKey(key) && dict[key] != null ? dict[key].AccountId : 0;
        }

        public static string GetAccountNr(this Dictionary<int, AccountSmallDTO> dict, int key)
        {
            return dict != null && dict.ContainsKey(key) && dict[key] != null ? dict[key].Number : String.Empty;
        }

        public static string GetAccountName(this Dictionary<int, AccountSmallDTO> dict, int key)
        {
            return dict != null && dict.ContainsKey(key) && dict[key] != null ? dict[key].Name : String.Empty;
        }

        public static decimal GetPercent(this Dictionary<int, AccountSmallDTO> dict, int key)
        {
            return dict != null && dict.ContainsKey(key) && dict[key] != null ? dict[key].Percent : 0;
        }

        #endregion

        #region AccountSettingRowDTO

        public static List<int> GetAccountInternalIds(this AccountingSettingsRowDTO e)
        {
            List<int> ids = new List<int>();
            if (e.Account2Id != 0)
                ids.Add(e.Account2Id);
            if (e.Account3Id != 0)
                ids.Add(e.Account3Id);
            if (e.Account4Id != 0)
                ids.Add(e.Account4Id);
            if (e.Account5Id != 0)
                ids.Add(e.Account5Id);
            if (e.Account6Id != 0)
                ids.Add(e.Account6Id);

            return ids;
        }

        #endregion

        #region AttestTransition

        public static List<AttestStateDTO> GetAttestStatesTo(this List<AttestTransitionDTO> attestTransitions, params int[] excludeIds)
        {
            List<AttestStateDTO> attestStates = new List<AttestStateDTO>();
            foreach (var attestTransition in attestTransitions)
            {
                if (attestTransition.AttestStateTo == null)
                    continue;
                if (attestStates.Any(i => i.AttestStateId == attestTransition.AttestStateTo.AttestStateId))
                    continue;
                if (excludeIds.Where(i => i != 0).Contains(attestTransition.AttestStateTo.AttestStateId))
                    continue;

                attestStates.Add(attestTransition.AttestStateTo);
            }
            return attestStates.OrderBy(i => i.Sort).ToList();
        }

        #endregion

        #region BudgetHead

        public static decimal GetAmount(this List<BudgetPeriodDTO> l, DistributionCodeBudgetType budgetType)
        {
            return l.Where(i => i.BudgetRow != null && i.BudgetRow.BudgetHead.Type == (int)budgetType).Sum(i => i.Amount);
        }

        public static decimal GetQuantity(this List<BudgetPeriodDTO> l, DistributionCodeBudgetType budgetType)
        {
            return l.Where(i => i.BudgetRow != null && i.BudgetRow.BudgetHead.Type == (int)budgetType).Sum(i => i.Quantity);
        }

        public static decimal GetBudgetCost(this List<BudgetPeriodDTO> l, DistributionCodeBudgetType budgetType, decimal costPerHour)
        {
            return l.Where(i => i.BudgetRow != null && i.BudgetRow.BudgetHead.Type == (int)budgetType).Sum(i => i.Quantity * costPerHour);
        }

        public static DateTime GetStopTime(this BudgetPeriodDTO e)
        {
            DateTime stopTime = CalendarUtility.DATETIME_DEFAULT;
            if (e.StartDate.HasValue)
            {
                switch (e.Type)
                {
                    case (int)BudgetRowPeriodType.Year:
                        stopTime = e.StartDate.Value.AddYears(1);
                        break;
                    case (int)BudgetRowPeriodType.SixMonths:
                        stopTime = e.StartDate.Value.AddMonths(6);
                        break;
                    case (int)BudgetRowPeriodType.Quarter:
                        stopTime = e.StartDate.Value.AddMonths(3);
                        break;
                    case (int)BudgetRowPeriodType.Month:
                        stopTime = e.StartDate.Value.AddMonths(1);
                        break;
                    case (int)BudgetRowPeriodType.Week:
                        stopTime = e.StartDate.Value.AddDays(7);
                        break;
                    case (int)BudgetRowPeriodType.Day:
                        stopTime = e.StartDate.Value.AddDays(1);
                        break;
                    case (int)BudgetRowPeriodType.Hour:
                        stopTime = e.StartDate.Value.AddHours(1);
                        break;
                }
            }
            return stopTime;
        }

        #endregion

        #region ChecklistRowDTO

        public static ChecklistExtendedRowDTO ToExtendedDTO(this ChecklistRowDTO e, int newHeadRecordId)
        {
            if (e == null)
                return null;

            ChecklistExtendedRowDTO dto = new ChecklistExtendedRowDTO()
            {
                Guid = Guid.NewGuid(),
                Name = e.ChecklistHead != null ? e.ChecklistHead.Name : String.Empty,
                RowId = e.ChecklistRowId,
                HeadId = e.ChecklistHeadId,
                RowNr = e.RowNr,
                Text = e.Text,
                Type = e.Type,
                Mandatory = e.Mandatory,
                RowRecordId = 0,
                HeadRecordId = e.ChecklistHead == null ? newHeadRecordId : e.ChecklistHead.ChecklistHeadRecordId,
                Comment = String.Empty,
                Date = null,
                BoolData = null,
                CheckListMultipleChoiceAnswerHeadId = e.CheckListMultipleChoiceAnswerHeadId,
            };

            switch (dto.Type)
            {
                case TermGroup_ChecklistRowType.String:
                    dto.DataTypeId = (int)SettingDataType.String;
                    break;
                case TermGroup_ChecklistRowType.YesNo:
                case TermGroup_ChecklistRowType.Checkbox:
                    dto.DataTypeId = (int)SettingDataType.Boolean;
                    break;
                case TermGroup_ChecklistRowType.MultipleChoice:
                    dto.DataTypeId = (int)SettingDataType.String;
                    break;
            }

            return dto;
        }

        public static IEnumerable<ChecklistExtendedRowDTO> ToExtendedDTOs(this IEnumerable<ChecklistRowDTO> l, int newHeadRecordId)
        {
            var dtos = new List<ChecklistExtendedRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToExtendedDTO(newHeadRecordId));
                }
            }
            return dtos;
        }

        #endregion

        #region CategoryDTO

        public static List<EmployeeDTO> GetEmployeesInCategory(this CategoryDTO e, IEnumerable<EmployeeDTO> employees, DateTime? dateFrom, DateTime? dateTo)
        {
            List<EmployeeDTO> validEmployees = new List<EmployeeDTO>();

            if (e.CompanyCategoryRecords != null && e.CompanyCategoryRecords.Count > 0)
            {
                foreach (var employee in employees.Where(i => !i.Hidden))
                {
                    bool existsInCategory = false;
                    if (dateFrom.HasValue && dateTo.HasValue)
                        existsInCategory = e.CompanyCategoryRecords.GetCategoryRecords(employee.EmployeeId, SoeCategoryRecordEntity.Employee, dateFrom.Value, dateTo.Value).Any();
                    else
                        existsInCategory = e.CompanyCategoryRecords.GetCategoryRecords(employee.EmployeeId, SoeCategoryRecordEntity.Employee, date: null, discardDateIfEmpty: true).Any();

                    if (existsInCategory)
                        validEmployees.Add(employee);
                }
            }

            return validEmployees;
        }

        #endregion

        #region CompanyCategoryRecordDTO

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, int recordId, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.RecordId == recordId && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, int recordId, DateTime? date = null, bool discardDateIfEmpty = false, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.RecordId == recordId && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, SoeCategoryRecordEntity entity, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.Entity == entity && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, SoeCategoryRecordEntity entity, DateTime? date = null, bool discardDateIfEmpty = false, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.Entity == entity && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, int recordId, SoeCategoryRecordEntity entity, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.RecordId == recordId && i.Entity == entity && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, int recordId, SoeCategoryRecordEntity entity, DateTime? date = null, bool discardDateIfEmpty = false, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.RecordId == recordId && i.Entity == entity && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, int recordId, int categoryId, DateTime? dateFrom = null, DateTime? dateTo = null, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.RecordId == recordId && i.CategoryId == categoryId && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(dateFrom, dateTo);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, int recordId, int categoryId, DateTime? date = null, bool discardDateIfEmpty = false, bool onlyDefaultCategories = false)
        {
            return l.Where(i => i.RecordId == recordId && i.CategoryId == categoryId && (!onlyDefaultCategories || i.Default)).ToList().GetCategoryRecords(date, discardDateIfEmpty);
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, DateTime? dateFrom, DateTime? dateTo)
        {
            if (l == null)
                return new List<CompanyCategoryRecordDTO>();

            if (dateFrom.HasValue && dateFrom.Value == CalendarUtility.DATETIME_MINVALUE)
                dateFrom = null;
            if (dateTo.HasValue && dateTo.Value == CalendarUtility.DATETIME_MAXVALUE)
                dateTo = null;

            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.ToList();
            if (!l.Any())
                return l.ToList();

            List<CompanyCategoryRecordDTO> result = new List<CompanyCategoryRecordDTO>();

            if (!dateFrom.HasValue)
                dateFrom = DateTime.Today;
            if (!dateTo.HasValue)
                dateTo = DateTime.Today;

            DateTime date = dateFrom.Value;
            while (date <= dateTo.Value)
            {
                foreach (var item in l.GetCategoryRecords(date))
                {
                    if (!result.Any(i => i.CompanyCategoryId == item.CompanyCategoryId))
                        result.Add(item);
                }

                if (date < CalendarUtility.DATETIME_MAXVALUE)
                    date = date.AddDays(1);
            }

            return result;
        }

        public static List<CompanyCategoryRecordDTO> GetCategoryRecords(this List<CompanyCategoryRecordDTO> l, DateTime? date, bool discardDateIfEmpty = false)
        {
            if (!date.HasValue)
            {
                if (discardDateIfEmpty)
                    return l.ToList();

                date = DateTime.Today;
            }

            date = date.Value.Date;

            return (from e in l
                    where (!e.DateFrom.HasValue || e.DateFrom.Value.Date <= date) &&
                    (!e.DateTo.HasValue || e.DateTo.Value.Date >= date)
                    orderby e.DateFrom
                    select e).ToList();
        }

        public static List<CompanyCategoryRecordDTO> SortDescending(this List<CompanyCategoryRecordDTO> l)
        {
            List<CompanyCategoryRecordDTO> sortedDtos = new List<CompanyCategoryRecordDTO>();

            //NULL to NULL
            sortedDtos.AddRange(l.Where(i => !i.DateFrom.HasValue && !i.DateTo.HasValue && !sortedDtos.Contains(i)));
            //Date to NULL
            sortedDtos.AddRange(l.Where(i => i.DateFrom.HasValue && !i.DateTo.HasValue && !sortedDtos.Contains(i)).OrderByDescending(i => i.DateFrom.Value));
            //Date to Date
            sortedDtos.AddRange(l.Where(i => i.DateTo.HasValue && !sortedDtos.Contains(i)).OrderByDescending(i => i.DateTo.Value));
            //NULL to Date
            sortedDtos.AddRange(l.Where(i => !i.DateTo.HasValue && i.DateTo.HasValue && !sortedDtos.Contains(i)).OrderByDescending(i => i.DateTo.Value));

            return sortedDtos;
        }

        public static bool IsEndingBeforeLastEmployment(this List<CompanyCategoryRecordDTO> categeoryRecords, List<EmploymentDTO> employments)
        {
            if (employments.IsNullOrEmpty())
                return false;

            List<EmploymentDTO> activeEmployments = employments.Where(i => i.State == (int)SoeEntityState.Active).ToList();

            //Case 1: Dont have any Employments
            if (activeEmployments.IsNullOrEmpty())
                return false;

            //Case 2: Dont have any Categories
            if (categeoryRecords.IsNullOrEmpty())
                return false;

            //Case 3: Have category without stopdate
            if (categeoryRecords.Any(i => !i.DateTo.HasValue))
                return false;

            //Case 4: Have employment without stopdate (and category with stopdate)
            if (activeEmployments.Any(i => !i.DateTo.HasValue))
                return true;

            //Case 5: Compare dates
            DateTime latestCategoryDateTo = categeoryRecords.Where(i => i.DateTo.HasValue).OrderByDescending(i => i.DateTo.Value).First().DateTo.Value;
            DateTime latestEmploymentDateTo = activeEmployments.Where(i => i.DateTo.HasValue).OrderByDescending(i => i.DateTo.Value).First().DateTo.Value;
            return latestCategoryDateTo < latestEmploymentDateTo;
        }

        #endregion

        #region EmployeeCSRExportDTO

        public static List<EmployeeCSRExportDTO> SortAlphanumericByCSREmployeeNr(this List<EmployeeCSRExportDTO> l)
        {
            var sortedItems = new List<EmployeeCSRExportDTO>();
            if (!l.IsNullOrEmpty())
            {
                var employeeNrs = l.Select(i => i.EmployeeNr).ToArray();
                Array.Sort(employeeNrs, new AlphanumComparator());
                foreach (var employeeNr in employeeNrs)
                {
                    foreach (var employeeItem in l.Where(i => i.EmployeeNr == employeeNr))
                    {
                        sortedItems.Add(employeeItem);
                    }
                }
            }
            return sortedItems;
        }

        #endregion

        #region EmployeeDTO

        public static List<EmployeeDTO> GetEmployeesInEmployeeGroup(this IEnumerable<EmployeeDTO> employees, int employeeGroupId, DateTime? dateFrom, DateTime? dateTo)
        {
            List<EmployeeDTO> validEmployees = new List<EmployeeDTO>();

            foreach (EmployeeDTO employee in employees)
            {
                EmploymentDTO employment = employee.GetEmployment(dateFrom, dateTo);
                if (employment != null && employment.EmployeeGroupId == employeeGroupId)
                    validEmployees.Add(employee);
            }

            return validEmployees;
        }

        public static List<EmployeeDTO> GetEmployeesInPayrollGroup(this IEnumerable<EmployeeDTO> employees, int payrollGroupId, DateTime? dateFrom, DateTime? dateTo)
        {
            List<EmployeeDTO> validEmployees = new List<EmployeeDTO>();

            foreach (EmployeeDTO employee in employees)
            {
                EmploymentDTO employment = employee.GetEmployment(dateFrom, dateTo);
                if (employment != null && NumberUtility.IsEqual(employment.PayrollGroupId, payrollGroupId))
                    validEmployees.Add(employee);
            }

            return validEmployees;
        }

        public static List<EmployeeDTO> GetEmployeesInPayrollGroups(this IEnumerable<EmployeeDTO> employees, List<int> payrollGroupIds, DateTime? dateFrom, DateTime? dateTo)
        {
            List<EmployeeDTO> validEmployees = new List<EmployeeDTO>();

            foreach (EmployeeDTO employee in employees)
            {
                EmploymentDTO employment = employee.GetEmployment(dateFrom, dateTo, forward: false);
                if (employment == null)
                    continue;

                foreach (int payrollGroupId in payrollGroupIds)
                {
                    if (!NumberUtility.IsEqual(employment.PayrollGroupId, payrollGroupId) && !validEmployees.Any(i => i.EmployeeId == employee.EmployeeId))
                        validEmployees.Add(employee);
                }
            }

            return validEmployees;
        }

        public static List<EmployeeDTO> GetEmployeesInVacationGroup(this IEnumerable<EmployeeDTO> employees, int vacationGroupId, DateTime? dateFrom, DateTime? dateTo)
        {
            List<EmployeeDTO> validEmployees = new List<EmployeeDTO>();

            foreach (EmployeeDTO employee in employees)
            {
                EmploymentDTO employment = employee.GetEmployment(dateFrom, dateTo);
                if (employment == null)
                    continue;

                if (employment.IsInVacationGroup(vacationGroupId, dateFrom))
                    validEmployees.Add(employee);
            }

            return validEmployees;
        }

        public static EmploymentDTO GetEmployment(this EmployeeDTO e, DateTime? dateFrom, DateTime? dateTo, bool forward = true)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
                return e.GetEmployment(dateFrom.Value, dateTo.Value, forward);
            else
                return e.GetEmployment();
        }

        public static EmploymentDTO GetEmployment(this EmployeeDTO e, DateTime dateFrom, DateTime dateTo, bool forward = true)
        {
            return e.Employments?.GetEmployment(dateFrom, dateTo, forward: forward);
        }

        public static EmploymentDTO GetEmployment(this EmployeeDTO e, DateTime? date = null)
        {
            return e.Employments?.GetEmployment(date);
        }

        #endregion

        #region EmployeeAccount

        public static bool IsEndingBeforeLastEmployment(this List<EmployeeAccountDTO> employeeAccounts, List<EmploymentDTO> employments)
        {
            if (employments.IsNullOrEmpty())
                return false;

            List<EmploymentDTO> activeEmployments = employments.Where(i => i.State == (int)SoeEntityState.Active).ToList();

            //Case 1: Dont have any Employments
            if (activeEmployments.IsNullOrEmpty())
                return false;

            //Case 2: Dont have any Categories
            if (employeeAccounts.IsNullOrEmpty())
                return false;

            //Case 3: Have category without stopdate
            if (employeeAccounts.Any(i => !i.DateTo.HasValue))
                return false;

            //Case 4: Have employment without stopdate (and category with stopdate)
            if (activeEmployments.Any(i => !i.DateTo.HasValue))
                return true;

            //Case 5: Compare dates
            DateTime latestAccountDateTo = employeeAccounts.Where(i => i.DateTo.HasValue).OrderByDescending(i => i.DateTo.Value).First().DateTo.Value;
            DateTime latestEmploymentDateTo = activeEmployments.Where(i => i.DateTo.HasValue).OrderByDescending(i => i.DateTo.Value).First().DateTo.Value;
            return latestAccountDateTo < latestEmploymentDateTo;
        }

        public static List<EmployeeAccountDTO> GetChildrens(this EmployeeAccountDTO e, DateTime date)
        {
            return e?.Children?.FilterOnDate(date) ?? new List<EmployeeAccountDTO>();
        }

        public static List<EmployeeAccountDTO> FilterOnDate(this IEnumerable<EmployeeAccountDTO> l, DateTime date)
        {
            return l?.Where(a => !a.DateTo.HasValue || (a.DateTo.HasValue && a.DateTo.Value > date)).ToList() ?? new List<EmployeeAccountDTO>();
        }

        #endregion

        #region EmployeeGroup

        public static bool UseQualifyingDayCalculationRuleWorkTimeWeekPlusExtraShifts(this EmployeeGroupDTO e)
        {
            return e?.QualifyingDayCalculationRule == TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusExtraShifts;
        }

        #endregion

        #region EmploymentDTO

        public static void ApplyEmploymentHistory(this IEnumerable<EmploymentDTO> l, Dictionary<int, string> fieldTypesDict, List<EmployeeGroupDTO> employeeGroups, List<PayrollGroupDTO> payrollGroups, List<EmploymentTypeDTO> employmentTypes, Dictionary<int, string> employmentEndReasonsDict, Dictionary<int, string> payrollPriceTypesDict, List<AnnualLeaveGroupDTO> annualLeaveGroups)
        {
            foreach (EmploymentDTO employment in l)
            {
                foreach (EmploymentChangeDTO change in employment.Changes)
                {
                    if (fieldTypesDict != null)
                    {
                        change.FieldTypeName = StringUtility.GetDictStringValue(fieldTypesDict, (int)change.FieldType);
                        if (!change.FieldTypeNameSuffix.IsNullOrEmpty())
                            change.FieldTypeName = $"{change.FieldTypeName} ({change.FieldTypeNameSuffix})";
                    }

                    switch (change.FieldType)
                    {
                        case TermGroup_EmploymentChangeFieldType.EmployeeGroupId:
                            #region EmployeeGroupId

                            Int32.TryParse(change.FromValue, out int employeeGroupIdFrom);
                            if (employeeGroups != null && employeeGroupIdFrom > 0)
                            {
                                EmployeeGroupDTO employeeGroup = employeeGroups.FirstOrDefault(p => p.EmployeeGroupId == employeeGroupIdFrom);
                                change.FromValueName = employeeGroup != null ? employeeGroup.Name : String.Empty;
                            }

                            Int32.TryParse(change.ToValue, out int employeeGroupIdTo);
                            if (employeeGroups != null && employeeGroupIdTo > 0)
                            {
                                EmployeeGroupDTO employeeGroup = employeeGroups.FirstOrDefault(p => p.EmployeeGroupId == employeeGroupIdTo);
                                change.ToValueName = employeeGroup != null ? employeeGroup.Name : String.Empty;
                            }

                            #endregion
                            break;
                        case TermGroup_EmploymentChangeFieldType.PayrollGroupId:
                            #region PayrollGroupId

                            Int32.TryParse(change.FromValue, out int payrollGroupIdFrom);
                            if (payrollGroups != null && payrollGroupIdFrom > 0)
                            {
                                PayrollGroupDTO payrollGroup = payrollGroups.FirstOrDefault(p => p.PayrollGroupId == payrollGroupIdFrom);
                                change.FromValueName = payrollGroup != null ? payrollGroup.Name : String.Empty;
                            }

                            Int32.TryParse(change.ToValue, out int payrollGroupIdTo);

                            if (payrollGroups != null && payrollGroupIdTo > 0)
                            {
                                PayrollGroupDTO payrollGroup = payrollGroups.FirstOrDefault(p => p.PayrollGroupId == payrollGroupIdTo);
                                change.ToValueName = payrollGroup != null ? payrollGroup.Name : String.Empty;
                            }

                            #endregion
                            break;
                        case TermGroup_EmploymentChangeFieldType.PayrollPriceTypeId:
                            #region PayrollPriceTypeId

                            Int32.TryParse(change.FromValue, out int payrollPriceTypeIdFrom);
                            if (payrollPriceTypesDict != null && payrollPriceTypeIdFrom > 0)
                                change.FromValueName = StringUtility.GetDictStringValue(payrollPriceTypesDict, payrollPriceTypeIdFrom);

                            Int32.TryParse(change.ToValue, out int payrollPriceTypeIdTo);
                            if (payrollPriceTypesDict != null && payrollPriceTypeIdTo > 0)
                                change.ToValueName = StringUtility.GetDictStringValue(payrollPriceTypesDict, payrollPriceTypeIdTo);

                            #endregion
                            break;
                        case TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId:
                            #region AnnualLeaveGroupId

                            Int32.TryParse(change.FromValue, out int annualLeaveGroupIdFrom);
                            if (annualLeaveGroups != null && annualLeaveGroupIdFrom > 0)
                            {
                                AnnualLeaveGroupDTO annualLeaveGroup = annualLeaveGroups.FirstOrDefault(p => p.AnnualLeaveGroupId == annualLeaveGroupIdFrom);
                                change.FromValueName = annualLeaveGroup != null ? annualLeaveGroup.Name : String.Empty;
                            }

                            Int32.TryParse(change.ToValue, out int annualLeaveGroupIdTo);

                            if (annualLeaveGroups != null && annualLeaveGroupIdTo > 0)
                            {
                                AnnualLeaveGroupDTO annualLeaveGroup = annualLeaveGroups.FirstOrDefault(p => p.AnnualLeaveGroupId == annualLeaveGroupIdTo);
                                change.ToValueName = annualLeaveGroup != null ? annualLeaveGroup.Name : String.Empty;
                            }

                            #endregion
                            break;
                        case TermGroup_EmploymentChangeFieldType.EmploymentType:
                            #region EmploymentType

                            if (!employmentTypes.IsNullOrEmpty())
                            {
                                if (Int32.TryParse(change.FromValue, out int employmentTypeFrom) && employmentTypeFrom > 0)
                                    change.FromValueName = employmentTypes.GetName(employmentTypeFrom);
                                if (Int32.TryParse(change.ToValue, out int employmentTypeTo) && employmentTypeTo > 0)
                                    change.ToValueName = employmentTypes.GetName(employmentTypeTo);
                            }

                            #endregion
                            break;
                        case TermGroup_EmploymentChangeFieldType.WorkTimeWeek:
                        case TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek:
                        case TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek:
                            #region WorkTimeWeek

                            change.FromValueName = CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(change.FromValue)), false, false);
                            change.ToValueName = CalendarUtility.FormatTimeSpan(CalendarUtility.MinutesToTimeSpan(Convert.ToInt32(change.ToValue)), false, false);

                            #endregion
                            break;
                        case TermGroup_EmploymentChangeFieldType.Percent:
                            #region Percent

                            if (!Decimal.TryParse(change.FromValue.Replace(',', '.'), out decimal percentFrom))
                                Decimal.TryParse(change.FromValue, out percentFrom);
                            percentFrom = Math.Round(percentFrom, 2);
                            change.FromValueName = percentFrom.ToString();

                            if (!Decimal.TryParse(change.ToValue.Replace(',', '.'), out decimal percentTo))
                                Decimal.TryParse(change.ToValue, out percentTo);
                            percentTo = Math.Round(percentTo, 2);
                            change.ToValueName = percentTo.ToString();

                            #endregion
                            break;
                        case TermGroup_EmploymentChangeFieldType.EmploymentEndReason:
                            #region EmploymentEndReason

                            Int32.TryParse(change.FromValue, out int employmentEndReasonFrom);
                            if (employmentEndReasonsDict != null && employmentEndReasonFrom > 0)
                                change.FromValueName = StringUtility.GetDictStringValue(employmentEndReasonsDict, employmentEndReasonFrom);

                            Int32.TryParse(change.ToValue, out int employmentEndReasonTo);
                            if (employmentEndReasonsDict != null && employmentEndReasonTo > 0)
                                change.ToValueName = StringUtility.GetDictStringValue(employmentEndReasonsDict, employmentEndReasonTo);

                            #endregion
                            break;
                        default:
                            #region Default

                            change.FromValueName = change.FromValue;
                            change.ToValueName = change.ToValue;

                            #endregion
                            break;
                    }

                }

                #region Filter obsolete changes

                //For EmploymentChanges with same FieldType and dates, the last should be active and the others deleted
                var changesGroupedByFieldType = employment.Changes.GroupBy(g => g.FieldType).ToList();
                foreach (var changesByFieldType in changesGroupedByFieldType)
                {
                    var changes = changesByFieldType.ToList();

                    while (changes.Any())
                    {
                        //Find similar changes
                        var firstChange = changes.First();
                        var matchingChanges = (from i in changes
                                               where i.FromDate == firstChange.FromDate &&
                                               i.ToDate == firstChange.ToDate
                                               orderby i.Created descending
                                               select i).ToList();

                        //Latest change
                        var firstMatchingChange = matchingChanges.First();

                        foreach (var matchingChange in matchingChanges)
                        {
                            if (matchingChange.EmploymentChangeId == firstMatchingChange.EmploymentChangeId)
                            {
                                //The latest should be active
                                firstMatchingChange.State = SoeEntityState.Active;
                            }
                            else
                            {
                                //Delete other changes than the latest
                                matchingChange.State = SoeEntityState.Deleted;
                            }

                            changes.Remove(matchingChange);
                        }
                    }
                }

                #endregion
            }
        }

        public static void ApplyEmploymentChanges(this IEnumerable<EmploymentDTO> l, DateTime date, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null, List < EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null)
        {
            date = date.Date;

            foreach (var dto in l)
            {
                dto.ApplyEmploymentChanges(date, employeeGroups, payrollGroups, annualLeaveGroups, employmentTypes, employmentEndReasonsDict);
            }
        }

        public static void ApplyEmploymentChanges(this EmploymentDTO e, DateTime date, TermGroup_EmploymentChangeFieldType? fieldType = null)
        {
            e.ApplyEmploymentChanges(date, null, null, null, null, null, fieldType);
        }

        public static bool ApplyEmploymentChanges(this EmploymentDTO e, DateTime date, List<EmployeeGroupDTO> employeeGroups, List<PayrollGroupDTO> payrollGroups, List<AnnualLeaveGroupDTO> annualLeaveGroups, List<EmploymentTypeDTO> employmentTypes, Dictionary<int, string> employmentEndReasons, TermGroup_EmploymentChangeFieldType? fieldType = null)
        {
            if (e == null)
                return false;

            DateTime? validDate = CalendarUtility.GetDateTimeInInterval(date, e.DateFrom, e.DateTo);
            if (!validDate.HasValue || e.IsHibernating(validDate.Value))
                return false;

            e.CurrentApplyChangeDate = date;
            e.RestoreToOriginalValues(employeeGroups, payrollGroups, employmentTypes, employmentEndReasons, fieldType);

            List<EmploymentChangeDTO> validEmploymentChanges = e.Changes.FilterEmploymentChanges(validDate.Value, null, fieldType);
            foreach (EmploymentChangeDTO change in validEmploymentChanges.OrderBy(i => i.FromDate ?? e.DateFrom).ThenBy(i => i.Created))
            {
                switch (change.FieldType)
                {
                    case TermGroup_EmploymentChangeFieldType.EmployeeGroupId:
                        e.SetEmployeeGroupValues(change.ToValue, change.ToValueName, employeeGroups);
                        break;
                    case TermGroup_EmploymentChangeFieldType.PayrollGroupId:
                        e.SetPayrollGroupValues(change.ToValue, change.ToValueName, payrollGroups);
                        break;
                    case TermGroup_EmploymentChangeFieldType.PayrollPriceTypeId:
                        break;
                    case TermGroup_EmploymentChangeFieldType.PayrollPriceTypeAmount:
                        break;
                    case TermGroup_EmploymentChangeFieldType.AnnualLeaveGroupId:
                        e.SetAnnualLeaveGroupValues(change.ToValue, change.ToValueName, annualLeaveGroups);
                        break;
                    case TermGroup_EmploymentChangeFieldType.EmploymentType:
                        e.SetEmploymentTypeValues(change.ToValue, change.ToValueName, employmentTypes);
                        break;
                    case TermGroup_EmploymentChangeFieldType.Name:
                        e.Name = StringUtility.NullToEmpty(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.Percent:
                        e.Percent = NumberUtility.ToDecimal(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.WorkTimeWeek:
                        e.WorkTimeWeek = NumberUtility.ToInteger(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek:
                        e.BaseWorkTimeWeek = NumberUtility.ToInteger(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.FullTimeWorkTimeWeek:
                        e.FullTimeWorkTimeWeek = NumberUtility.ToInteger(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
                        e.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = StringUtility.GetNullableBool(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.ExperienceMonths:
                        e.ExperienceMonths = NumberUtility.ToInteger(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.ExperienceAgreedOrEstablished:
                        e.ExperienceAgreedOrEstablished = StringUtility.ToBool(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.WorkTasks:
                        e.WorkTasks = StringUtility.NullToEmpty(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.WorkPlace:
                        e.WorkPlace = StringUtility.NullToEmpty(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.SpecialConditions:
                        e.SpecialConditions = StringUtility.NullToEmpty(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.SubstituteFor:
                        e.SubstituteFor = StringUtility.NullToEmpty(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.SubstituteForDueTo:
                        e.SubstituteForDueTo = StringUtility.NullToEmpty(change.ToValue);
                        break;
                    case TermGroup_EmploymentChangeFieldType.EmploymentEndReason:
                        e.SetEmploymentEndReason(change.ToValue, employmentEndReasons);
                        break;
                    case TermGroup_EmploymentChangeFieldType.ExternalCode:
                        e.ExternalCode = StringUtility.NullToEmpty(change.ToValue);
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        public static void RestoreToOriginalValues(this EmploymentDTO e, List<EmployeeGroupDTO> employeeGroups, List<PayrollGroupDTO> payrollGroups, List<EmploymentTypeDTO> employmentTypes, Dictionary<int, string> employmentEndReasons, TermGroup_EmploymentChangeFieldType? fieldType)
        {
            if (e.Changes.IsNullOrEmpty())
                return;

            Dictionary<TermGroup_EmploymentChangeFieldType, string> originalValues = e.OriginalValues ?? new Dictionary<TermGroup_EmploymentChangeFieldType, string>();
            if (fieldType.HasValue)
                originalValues = originalValues.Where(ov => ov.Key == fieldType.Value).ToDictionary(k => k.Key, v => v.Value);

            foreach (var originalValue in originalValues)
            {
                switch (originalValue.Key)
                {
                    case TermGroup_EmploymentChangeFieldType.DateFrom:
                        e.DateFrom = CalendarUtility.ToNullableDateTime(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.DateTo:
                        e.DateTo = CalendarUtility.ToNullableDateTime(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.State:
                        e.State = (SoeEntityState)NumberUtility.ToInteger(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.EmployeeGroupId:
                        e.SetEmployeeGroupValues(originalValue.Value, e.GetOriginalValue(TermGroup_EmploymentChangeFieldType.EmployeeGroupName), employeeGroups);
                        break;
                    case TermGroup_EmploymentChangeFieldType.PayrollGroupId:
                        e.SetPayrollGroupValues(originalValue.Value, e.GetOriginalValue(TermGroup_EmploymentChangeFieldType.PayrollGroupName), payrollGroups);
                        break;
                    case TermGroup_EmploymentChangeFieldType.PayrollPriceTypeId:
                    case TermGroup_EmploymentChangeFieldType.PayrollPriceTypeAmount:
                        break;
                    case TermGroup_EmploymentChangeFieldType.EmploymentType:
                        e.SetEmploymentTypeValues(originalValue.Value, null, employmentTypes);
                        break;
                    case TermGroup_EmploymentChangeFieldType.Name:
                        e.Name = StringUtility.NullToEmpty(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.Percent:
                        e.Percent = NumberUtility.ToDecimal(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.WorkTimeWeek:
                        e.WorkTimeWeek = NumberUtility.ToInteger(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.BaseWorkTimeWeek:
                        e.BaseWorkTimeWeek = NumberUtility.ToInteger(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.ExperienceMonths:
                        e.ExperienceMonths = NumberUtility.ToInteger(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.ExperienceAgreedOrEstablished:
                        e.ExperienceAgreedOrEstablished = StringUtility.ToBool(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.WorkTasks:
                        e.WorkTasks = StringUtility.NullToEmpty(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.WorkPlace:
                        e.WorkPlace = StringUtility.NullToEmpty(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.SpecialConditions:
                        e.SpecialConditions = StringUtility.NullToEmpty(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.SubstituteFor:
                        e.SubstituteFor = StringUtility.NullToEmpty(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.SubstituteForDueTo:
                        e.SubstituteForDueTo = StringUtility.NullToEmpty(originalValue.Value);
                        break;
                    case TermGroup_EmploymentChangeFieldType.EmploymentEndReason:
                        e.SetEmploymentEndReason(originalValue.Value, employmentEndReasons);
                        break;
                    case TermGroup_EmploymentChangeFieldType.ExternalCode:
                        e.ExternalCode = StringUtility.NullToEmpty(originalValue.Value);
                        break;
                    default:
                        break;
                }
            }
        }

        public static void SetEmployeeGroupValues(this EmploymentDTO e, string value, string defaultValue, List<EmployeeGroupDTO> employeeGroups)
        {
            Int32.TryParse(value, out int employeeGroupId);
            e.EmployeeGroupId = employeeGroupId;

            if (!employeeGroups.IsNullOrEmpty())
            {
                EmployeeGroupDTO employeeGroup = employeeGroups.FirstOrDefault(i => i.EmployeeGroupId == e.EmployeeGroupId);
                e.EmployeeGroupName = employeeGroup?.Name ?? string.Empty;
                e.EmployeeGroupWorkTimeWeek = employeeGroup?.RuleWorkTimeWeek ?? 0;
            }
            else
                e.EmployeeGroupName = defaultValue;
        }

        public static void SetPayrollGroupValues(this EmploymentDTO e, string value, string defaultValue, List<PayrollGroupDTO> payrollGroups)
        {
            Int32.TryParse(value, out int payrollGroupId);
            e.PayrollGroupId = payrollGroupId.ToNullable();

            if (!payrollGroups.IsNullOrEmpty() && e.PayrollGroupId.HasValue)
            {
                PayrollGroupDTO payrollGroup = payrollGroups.FirstOrDefault(i => i.PayrollGroupId == e.PayrollGroupId.Value);
                e.PayrollGroupName = payrollGroup?.Name ?? string.Empty;
            }
            else
                e.PayrollGroupName = defaultValue;
        }

        public static void SetAnnualLeaveGroupValues(this EmploymentDTO e, string value, string defaultValue, List<AnnualLeaveGroupDTO> annualLeaveGroups)
        {
            Int32.TryParse(value, out int annualLeaveGroupId);
            e.AnnualLeaveGroupId = annualLeaveGroupId.ToNullable();

            if (!annualLeaveGroups.IsNullOrEmpty() && e.PayrollGroupId.HasValue)
            {
                AnnualLeaveGroupDTO annualLeaveGroup = annualLeaveGroups.FirstOrDefault(i => i.AnnualLeaveGroupId == e.AnnualLeaveGroupId.Value);
                e.AnnualLeaveGroupName = annualLeaveGroup?.Name ?? string.Empty;
            }
            else
                e.AnnualLeaveGroupName = defaultValue;
        }

        public static void SetEmploymentEndReason(this EmploymentDTO e, string values, Dictionary<int, string> employmentEndReasons)
        {
            e.EmploymentEndReason = NumberUtility.ToInteger(values);
            e.EmploymentEndReasonName = StringUtility.GetDictStringValue(employmentEndReasons, e.EmploymentEndReason);
        }

        public static void SetEmploymentTypeValues(this EmploymentDTO e, string value, string defaultValue, List<EmploymentTypeDTO> employmentTypes)
        {
            Int32.TryParse(value, out int typeId);
            if (e.EmploymentType != typeId)
            {
                e.EmploymentType = typeId;
                e.EmploymentTypeName = employmentTypes?.GetName(e.EmploymentType) ?? defaultValue.NullToEmpty();
            }
        }

        public static void SetEmploymentTypeNames(this IEnumerable<EmploymentDTO> l, List<EmploymentTypeDTO> employmentTypes)
        {
            if (employmentTypes.IsNullOrEmpty())
                return;

            foreach (EmploymentDTO e in l)
            {
                e.SetEmploymentTypeName(employmentTypes);
            }
        }

        public static void SetEmploymentTypeName(this EmploymentDTO e, List<EmploymentTypeDTO> employmentTypes)
        {
            if (employmentTypes.IsNullOrEmpty())
                return;

            e.EmploymentTypeName = employmentTypes.GetName(e.EmploymentType);
        }

        public static ActionResult ValidateEmploymentVacationGroups(this List<EmploymentDTO> l)
        {
            ActionResult result = new ActionResult(true);
            if (!l.IsNullOrEmpty())
            {
                foreach (var e in l.Where(i => i.FinalSalaryStatus != SoeEmploymentFinalSalaryStatus.AppliedFinalSalary && i.State == SoeEntityState.Active))
                {
                    result = e.EmploymentVacationGroup.ValidateEmploymentVacationGroups();
                    if (!result.Success)
                        break;
                }
            }
            return result;
        }

        public static ActionResult ValidateEmploymentVacationGroups(this List<EmploymentVacationGroupDTO> l)
        {
            if (l.IsNullOrEmpty() || l.Count == 1)
                return new ActionResult();

            DateTime? prevFromDate = null;
            foreach (var e in l.Where(i => i.State == SoeEntityState.Active).OrderBy(i => i.FromDate ?? DateTime.MinValue))
            {
                DateTime fromDate = e.FromDate ?? DateTime.MinValue;
                if (prevFromDate.HasValue && prevFromDate.Value == fromDate)
                    return new ActionResult((int)ActionResultSave.EmploymentVacationGroupsCannotBeDuplicate, "", stringValue: e.FromDate?.ToShortDateTime() ?? "");

                prevFromDate = fromDate;
            }

            return new ActionResult(true);
        }

        public static ActionResult ValidateFixedTerm14days(this EmploymentDTO e)
        {
            ActionResult result = new ActionResult(true);

            if (e != null && e.EmploymentType == (int)TermGroup_EmploymentType.SE_FixedTerm14days && e.GetEmploymentDays() != 14)
                return new ActionResult((int)ActionResultSave.EmployeeEmploymentsInvalidFixedTerm14days);

            return result;
        }

        public static bool IsValidFixedTerm14days(this EmploymentDTO e)
        {
            if (e?.EmploymentType == (int)TermGroup_EmploymentType.SE_FixedTerm14days)
                return e.DateFrom.HasValue && e.DateTo.HasValue && (int)e.DateTo.Value.Subtract(e.DateFrom.Value).TotalDays + 1 == 14;
            return true;
        }

        public static bool IsValidateWorkTimeWeek(this EmploymentDTO e, List<EmployeeGroupDTO> employeeGroups, out string workTimeWeekFormatted, out string employeeGroupWorkTimeWeekFormatted)
        {
            workTimeWeekFormatted = "";
            employeeGroupWorkTimeWeekFormatted = "";

            if (e != null && !e.DateTo.HasValue)
            {
                TimeSpan workTimeWeek = CalendarUtility.MinutesToTimeSpan(e.WorkTimeWeek);
                if (workTimeWeek.TotalMinutes == 0)
                {
                    EmployeeGroupDTO employeeGroup = employeeGroups.FirstOrDefault(i => i.EmployeeGroupId == e.EmployeeGroupId);
                    TimeSpan employeeGroupWorkTimeWeek = employeeGroup != null ? CalendarUtility.MinutesToTimeSpan(employeeGroup.RuleWorkTimeWeek) : new TimeSpan();
                    if (employeeGroupWorkTimeWeek.TotalMinutes > 0)
                    {
                        workTimeWeekFormatted = CalendarUtility.FormatTimeSpan(workTimeWeek, false, false);
                        employeeGroupWorkTimeWeekFormatted = CalendarUtility.FormatTimeSpan(employeeGroupWorkTimeWeek, false, false);
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsDatesEqual(this EmploymentDTO e, DateTime? startDate, DateTime? stopDate)
        {
            return e != null && CalendarUtility.IsDatesEqual(e.DateFrom, startDate) && CalendarUtility.IsDatesEqual(e.DateTo, stopDate);
        }

        public static ActionResult ValidateEmployments(this List<EmploymentDTO> l, List<EmployeeGroupDTO> employeeGroups = null, bool validateFixedTerm14days = false, bool validateWorkTimeWeek = false)
        {
            if (l.IsNullOrEmpty())
                return new ActionResult(true);

            if (employeeGroups.IsNullOrEmpty())
                validateWorkTimeWeek = false;

            var validationItems = new List<DateIntervalValidationDTO>();

            foreach (var e in l)
            {
                if (e.IsSecondaryEmployment)
                {
                    //State may not been changed to Hidden yet
                    if (!e.IsActiveOrHidden())
                        continue;
                }
                else if (e.State != SoeEntityState.Active)
                    continue;

                if (e.State == SoeEntityState.Active && e.EmployeeGroupId.ToNullable() == null)
                    return new ActionResult((int)ActionResultSave.EmployeeGroupMandatory);

                if (validateFixedTerm14days && !e.IsValidFixedTerm14days())
                    return new ActionResult((int)ActionResultSave.EmployeeEmploymentsInvalidFixedTerm14days);

                if (validateWorkTimeWeek && !e.IsValidateWorkTimeWeek(employeeGroups, out string workTimeWeekFormatted, out string employeeGroupWorkTimeWeekFormatted))
                    return new ActionResult((int)ActionResultSave.EmployeeEmploymentsInvalidWorkTimeWeek)
                    {
                        Strings = new List<string>()
                        {
                            workTimeWeekFormatted,
                            employeeGroupWorkTimeWeekFormatted,
                        }
                    };

                validationItems.Add(e.ToDateIntervalValidationDTO());
            }

            return validationItems.Validate(true, false);
        }

        public static List<DateIntervalValidationDTO> ToDateIntervalValidationDTOs(this List<EmploymentDTO> l, List<string> excludeUniqueIds = null)
        {
            List<DateIntervalValidationDTO> validationItems = new List<DateIntervalValidationDTO>();
            if (!l.IsNullOrEmpty())
            {
                foreach (var e in l.Where(i => i.State == SoeEntityState.Active))
                {
                    DateIntervalValidationDTO validationItem = e.ToDateIntervalValidationDTO(excludeUniqueIds);
                    if (validationItem != null)
                        validationItems.Add(validationItem);
                }
            }

            return validationItems;
        }

        public static DateIntervalValidationDTO ToDateIntervalValidationDTO(this EmploymentDTO e, List<string> excludeUniqueIds = null)
        {
            if (excludeUniqueIds != null && excludeUniqueIds.Contains(e.UniqueId))
                return null;
            return new DateIntervalValidationDTO(e.DateFrom, e.DateTo, !e.IsSecondaryEmployment && !e.IsTemporaryPrimary);
        }

        public static EmploymentChangeDTO GetLastValidChange(this EmploymentDTO e, TermGroup_EmploymentChangeFieldType type, DateTime? fromDate, DateTime? toDate)
        {
            EmploymentChangeDTO lastChange = e?.GetLastChange(type, true);
            if (lastChange == null)
                return null;
            if ((lastChange.FromDate ?? DateTime.MinValue) > (fromDate ?? DateTime.MinValue))
                return null;
            if ((lastChange.ToDate ?? DateTime.MaxValue) < (toDate ?? DateTime.MaxValue))
                return null;
            return lastChange;
        }

        public static int GetLastValidChangeValue(this EmploymentDTO e, TermGroup_EmploymentChangeFieldType type, DateTime? fromDate, DateTime? toDate, int defaultValue)
        {
            string value = e.GetLastValidChange(type, fromDate, toDate)?.ToValue;
            if (value.IsNullOrEmpty())
                return defaultValue;

            return Convert.ToInt32(value);
        }

        #endregion

        #region EmploymentChangeDTO

        public static List<EmploymentChangeDTO> GetChanges(this EmploymentDTO e)
        {
            return e?.Changes?.Where(i => !i.IsDeleted).ToList() ?? new List<EmploymentChangeDTO>();
        }

        public static EmploymentChangeDTO GetLastChange(this EmploymentDTO e, TermGroup_EmploymentChangeFieldType fieldType, bool includeCurrent)
        {
            if (e == null)
                return null;

            return (includeCurrent && !e.CurrentChanges.IsNullOrEmpty() ? e.Changes.Concat(e.CurrentChanges) : e.Changes)
                .OrderByDescending(o => o.FromDate ?? o.ToDate)
                .ThenByDescending(o => o.Created)
                .FirstOrDefault(i => i.FieldType == fieldType);
        }

        public static bool HasAnyChanges(this EmploymentDTO e)
        {
            return e?.Changes.IsNullOrEmpty() ?? false;
        }

        public static bool HasChange(this EmploymentDTO e, TermGroup_EmploymentChangeFieldType fieldType, bool includeCurrent)
        {
            if (includeCurrent && !e.CurrentChanges.IsNullOrEmpty())
                return (e.GetChanges().Concat(e.CurrentChanges)).Any(i => i.FieldType == fieldType);
            else
                return e.GetChanges().Any(i => i.FieldType == fieldType);
        }

        public static bool HasChangeThatStartsBefore(this EmploymentDTO e, DateTime date)
        {
            if (!e.HasAnyChanges())
                return false;

            return e.GetChanges().Any(i => i.FromDate.HasValue && i.FromDate.Value <= date);
        }

        public static bool HasChangeThatStartsAfter(this EmploymentDTO e, DateTime date)
        {
            if (!e.HasAnyChanges())
                return false;
            return e.GetChanges().Any(i => i.FromDate.HasValue && i.FromDate.Value >= date);
        }

        #endregion

        #region EmploymentVacationGroupDTO

        public static bool HasOtherVacationGroupBetweenDates(this EmploymentVacationGroupDTO e, List<EmploymentVacationGroupDTO> others, DateTime newFromDate)
        {
            if (e == null || !e.FromDate.HasValue || e.FromDate.Value == newFromDate || others.IsNullOrEmpty())
                return false;

            return others.Any(other => other.VacationGroupId != e.VacationGroupId && other.FromDate.HasValue && CalendarUtility.IsDateInRange(other.FromDate.Value, e.FromDate, newFromDate));
        }

        #endregion

        #region EmploymentCalendarDTO

        public static EmploymentCalenderDTO GetFirst(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return l?.OrderBy(f => f.Date).FirstOrDefault();
        }

        public static EmploymentCalenderDTO GetLast(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return l?.OrderByDescending(f => f.Date).FirstOrDefault();
        }

        public static List<Tuple<int?, DateTime, DateTime>> GetVacationGroupPeriods(this List<EmploymentCalenderDTO> l)
        {
            List<Tuple<int?, DateTime, DateTime>> tuples = new List<Tuple<int?, DateTime, DateTime>>();

            if (!l.IsNullOrEmpty())
            {
                var dtos = l.OrderBy(o => o.Date).ToList();
                var startDate = dtos.OrderBy(o => o.Date).First().Date;
                var stopDate = dtos.OrderBy(o => o.Date).Last().Date;
                var currentDate = startDate;
                int? currentVacationGroupId = dtos.OrderBy(o => o.Date).First().VacationGroupId;

                while (currentDate <= stopDate)
                {
                    if (currentVacationGroupId != dtos.GetVacationGroupId(currentDate) /* && !tuples.Any()*/)
                    {
                        tuples.Add(Tuple.Create(currentVacationGroupId, startDate, currentDate.AddDays(-1)));
                        startDate = currentDate;
                        currentVacationGroupId = dtos.GetVacationGroupId(currentDate);
                    }
                    currentDate = currentDate.AddDays(1);
                }

                if (!tuples.Any())
                    tuples.Add(Tuple.Create(currentVacationGroupId, startDate, stopDate));
                else if (currentDate.AddDays(-1) != tuples.OrderBy(o => o.Item3).First().Item3)
                    tuples.Add(Tuple.Create(currentVacationGroupId, startDate, stopDate));
            }

            return tuples;
        }

        public static int? GetEmployeeGroupId(this List<EmploymentCalenderDTO> l, DateTime date)
        {
            return l?.FirstOrDefault(f => f.Date == date)?.EmployeeGroupId;
        }

        public static int? GetPayrollGroupId(this List<EmploymentCalenderDTO> l, DateTime date)
        {
            return l?.FirstOrDefault(f => f.Date == date)?.PayrollGroupId;
        }

        public static int? GetVacationGroupId(this List<EmploymentCalenderDTO> l, DateTime date)
        {
            return l?.FirstOrDefault(f => f.Date == date)?.VacationGroupId;
        }

        public static bool IsCoherent(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!l.IsNullOrEmpty() && dateFrom.HasValue && dateTo.HasValue && l.Count < CalendarUtility.GetTotalDays(dateFrom, dateTo))
                return false;

            return
                l.IsEmploymentIdCoherent(dateFrom, dateTo) &&
                l.IsPercentCoherent(dateFrom, dateTo) &&
                l.IsEmployeeGroupCoherent(dateFrom, dateTo) &&
                l.IsPayrollGroupCoherent(dateFrom, dateTo) &&
                l.IsVacationGroupCoherent(dateFrom, dateTo) &&
                l.IsEmploymentTypeCoherent(dateFrom, dateTo);
        }

        public static bool IsEmploymentIdCoherent(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.GroupBy(f => f.EmploymentId).Count() == 1;
            else
                return l.Where(f => f.Date >= (dateFrom ?? DateTime.MinValue) && f.Date <= (dateTo ?? DateTime.MaxValue)).GroupBy(g => g.EmploymentId).Count() == 1;
        }

        public static bool IsPercentCoherent(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.GroupBy(f => f.Percent).Count() == 1;
            else
                return l.Where(f => f.Date >= (dateFrom ?? DateTime.MinValue) && f.Date <= (dateTo ?? DateTime.MaxValue)).GroupBy(g => g.Percent).Count() == 1;
        }

        public static bool IsEmployeeGroupCoherent(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.GroupBy(f => f.EmployeeGroupId).Count() == 1;
            else
                return l.Where(f => f.Date >= (dateFrom ?? DateTime.MinValue) && f.Date <= (dateTo ?? DateTime.MaxValue)).GroupBy(g => g.EmployeeGroupId).Count() == 1;
        }

        public static bool IsPayrollGroupCoherent(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.GroupBy(f => f.PayrollGroupId).Count() == 1;
            else
                return l.Where(f => f.Date >= (dateFrom ?? DateTime.MinValue) && f.Date <= (dateTo ?? DateTime.MaxValue)).GroupBy(g => g.PayrollGroupId).Count() == 1;
        }

        public static bool IsVacationGroupCoherent(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.GroupBy(f => f.VacationGroupId).Count() == 1;
            else
                return l.Where(f => f.Date >= (dateFrom ?? DateTime.MinValue) && f.Date <= (dateTo ?? DateTime.MaxValue)).GroupBy(g => g.VacationGroupId).Count() == 1;
        }

        public static bool IsEmploymentTypeCoherent(this List<EmploymentCalenderDTO> l, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.GroupBy(f => f.EmploymentType).Count() == 1;
            else
                return l.Where(f => f.Date >= (dateFrom ?? DateTime.MinValue) && f.Date <= (dateTo ?? DateTime.MaxValue)).GroupBy(g => g.EmploymentType).Count() == 1;
        }

        public static bool IsEmployeedInPeriod(this List<EmploymentCalenderDTO> e, DateTime dateFrom, DateTime dateTo)
        {
            return CalendarUtility.IsDatesInInterval(dateFrom, dateTo, e.GetFirst().Date, e.GetLast().Date);
        }

        public static void GetPeriodDates(this List<EmploymentCalenderDTO> l, TimePeriodDTO timePeriod, out DateTime newDateFrom, out DateTime newDateTo)
        {
            l.GetValidDates(timePeriod.StartDate, timePeriod.StopDate, timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value, out newDateFrom, out newDateTo);
        }

        public static void GetValidDates(this List<EmploymentCalenderDTO> l, DateTime timePeriodStartDate, DateTime timePeriodStopDate, DateTime payrollstartDate, DateTime payrollstopDate, out DateTime newDateFrom, out DateTime newDateTo)
        {
            newDateFrom = timePeriodStartDate;
            newDateTo = timePeriodStopDate;

            if (l.IsEmployeedInPeriod(timePeriodStartDate, timePeriodStopDate))
            {
                newDateFrom = timePeriodStartDate;
                newDateTo = timePeriodStopDate;
            }
            else if (l.IsEmployeedInPeriod(timePeriodStartDate, payrollstopDate))
            {
                newDateFrom = timePeriodStartDate;
                newDateTo = payrollstopDate;
            }
            else if (l.IsEmployeedInPeriod(payrollstartDate, payrollstopDate))
            {
                newDateFrom = payrollstartDate;
                newDateTo = payrollstopDate;
            }
            else if (l.GetLast().Date < timePeriodStartDate)
            {
                newDateFrom = l.GetLast().Date;
                newDateTo = l.GetLast().Date;
            }
            else if (l.GetFirst().Date > timePeriodStopDate)
            {
                newDateFrom = l.GetFirst().Date;
                newDateTo = l.GetFirst().Date;
            }
        }

        #endregion

        #region EmploymentChangeDTO

        public static List<EmploymentChangeDTO> FilterEmploymentChanges(this List<EmploymentChangeDTO> l, DateTime changesForDate, TermGroup_EmploymentChangeType? type, TermGroup_EmploymentChangeFieldType? fieldType)
        {
            if (l.IsNullOrEmpty())
                return new List<EmploymentChangeDTO>();

            if (type.HasValue)
                l = l.Where(e => e.Type == type.Value).ToList();
            if (fieldType.HasValue)
                l = l.Where(e => e.FieldType == fieldType.Value).ToList();

            return
                l.Where(e => (
                    (!e.FromDate.HasValue && e.ToDate.HasValue && e.ToDate.Value >= changesForDate) ||
                    (!e.ToDate.HasValue && e.FromDate.HasValue && e.FromDate.Value <= changesForDate) ||
                    (e.FromDate.HasValue && e.ToDate.HasValue && e.FromDate.Value <= changesForDate && e.ToDate.Value >= changesForDate) ||
                    (!e.FromDate.HasValue && !e.ToDate.HasValue)
                    ))
                .ToList();
        }

        #endregion

        #region EmploymentPriceType

        public static EmploymentPriceTypePeriodDTO GetEmploymentPriceTypePeriod(this EmploymentPriceTypeDTO e, DateTime? date)
        {
            if (e.Periods == null)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            return e.Periods.Where(p => (!p.FromDate.HasValue || p.FromDate.Value <= date.Value)).OrderBy(p => p.FromDate).LastOrDefault();
        }

        public static decimal? GetEmploymentPriceTypeAmount(this EmploymentPriceTypeDTO e, DateTime? date)
        {
            if (e == null)
                return 0;

            return e.EmploymentPriceTypeId > 0 ? e.GetEmploymentPriceTypePeriod(date)?.Amount : e.PayrollGroupAmount;
        }

        #endregion

        #region EmployeeRequest

        public static decimal? GetRatio(this EmployeeRequestDTO e)
        {
            if (e?.ExtendedSettings == null || !e.ExtendedSettings.PercentalAbsence)
                return null;
            return e.ExtendedSettings.PercentalValue;
        }

        #endregion

        #region EmploymentVacationGroup

        public static EmploymentVacationGroupDTO GetEmploymentVacationGroup(this List<EmploymentVacationGroupDTO> l, DateTime date)
        {
            if (l == null || l.Count == 0)
                return null;

            return (from evg in l
                    where (!evg.FromDate.HasValue || evg.FromDate.Value <= date)
                    orderby evg.FromDate descending
                    select evg).FirstOrDefault();
        }

        public static bool IsInVacationGroup(this EmploymentDTO e, int vacationGroupId, DateTime? date)
        {
            if (e.EmploymentVacationGroup.IsNullOrEmpty())
                return false;

            return (from evg in e.EmploymentVacationGroup
                    where evg.VacationGroupId == vacationGroupId &&
                    (!evg.FromDate.HasValue || !date.HasValue || evg.FromDate.Value <= date.Value)
                    orderby evg.FromDate descending
                    select evg).Any();
        }

        #endregion

        #region IncomingDeliveryHeadDTO

        public static bool IsBaseNeed(this IncomingDeliveryHeadDTO e, DayOfWeek givenDayOfWeek)
        {
            return DailyRecurrencePatternDTO.IsBaseNeed(e.RecurrencePattern, givenDayOfWeek);
        }

        public static bool IsSpecificNeed(this IncomingDeliveryHeadDTO e)
        {
            return (e.StartDate == e.StopDate || e.NbrOfOccurrences == 1) || DailyRecurrencePatternDTO.IsAdditionalNeed(e.RecurrencePattern);
        }

        public static bool HasNoRecurrencePattern(this IncomingDeliveryHeadDTO e)
        {
            return DailyRecurrencePatternDTO.HasNoRecurrencePattern(e.RecurrencePattern);
        }

        #endregion

        #region PayrollGroupReportDTO

        public static ReportViewDTO ToReportViewDTO(this PayrollGroupReportDTO e)
        {
            if (e == null)
                return null;

            ReportViewDTO dto = new ReportViewDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                ReportId = e.ReportId,
                ReportName = e.ReportName,
                ReportNr = e.ReportNr,
                ReportDescription = e.ReportDescription,
                SysReportTemplateTypeId = e.SysReportTemplateTypeId,
            };

            return dto;
        }

        public static List<ReportViewDTO> ToReportViewDTOs(this IEnumerable<PayrollGroupReportDTO> l)
        {
            var dtos = new List<ReportViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToReportViewDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region InvoiceProduct

        public static InvoiceProductGridDTO ToGridDTO(this InvoiceProductDTO e)
        {
            return new InvoiceProductGridDTO()
            {
                ProductId = e.ProductId,
                Number = e.Number,
                Name = e.Name,
                ProductGroupId = e.ProductGroupId,
                SysProductId = e.SysProductId,
                External = e.SysProductId.HasValue,
                State = (SoeEntityState)e.State
            };
        }

        #endregion

        #region PayrollGroupDTO

        public static List<int> GetValidPayrollGroups(this List<PayrollGroupDTO> payrollGroups, int timePeriodHeadId)
        {
            return payrollGroups.Where(x => x.TimePeriodHeadId.HasValue && x.TimePeriodHeadId.Value == timePeriodHeadId).Select(x => x.PayrollGroupId).ToList();
        }

        #endregion

        #region ScanningEntry

        public static ScanningEntryRowDTO GetScanningEntryRow(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            return e?.ScanningEntryRow?.FirstOrDefault(i => i.Type == type);
        }

        public static string GetScanningEntryRowStringValue(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            return e?.GetScanningEntryRow(type)?.Text ?? string.Empty;
        }

        public static int GetScanningEntryRowIntValue(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? NumberUtility.ToInteger(row.Text) : 0;
        }

        public static decimal GetScanningEntryRowDecimalValue(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? NumberUtility.ToDecimal(row.Text) : Decimal.Zero;
        }

        public static bool GetScanningEntryRowBoolValue(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null && StringUtility.GetBool(row.Text);
        }

        public static DateTime GetScanningEntryRowDateValue(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? CalendarUtility.GetDateTime(row.Text) : CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime? GetScanningEntryRowNullableDateValue(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? CalendarUtility.GetNullableDateTime(row.Text) : (DateTime?)null;
        }

        #region Interpretation

        public static TermGroup_ScanningInterpretation GetScanningInterpretation(this ScanningEntryDTO e, ScanningEntryRowType type)
        {
            var row = e.GetScanningEntryRow(type);
            return row != null ? Validator.ValidateScanningEntryRow(row.NewText, row.ValidationError) : TermGroup_ScanningInterpretation.ValueNotFound;
        }

        public static bool IsAllRowsValid(this ScanningEntryDTO e)
        {
            if (e.ScanningEntryRow == null)
                return false;
            List<ScanningEntryRowDTO> rows = e.ScanningEntryRow.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            return rows.Count > 0 && rows.Count == rows.Count(i => i.ValueIsValid());
        }

        public static TermGroup_ScanningInterpretation GetBillingTypeInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.IsCreditInvoice);
        }

        public static TermGroup_ScanningInterpretation GeInvoiceNrInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.InvoiceNr);
        }

        public static TermGroup_ScanningInterpretation GetInvoiceDateInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.InvoiceDate);
        }

        public static TermGroup_ScanningInterpretation GetDueDateInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.DueDate);
        }

        public static TermGroup_ScanningInterpretation GetOrderNrInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.OrderNr);
        }

        public static TermGroup_ScanningInterpretation GetReferenceYourInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.ReferenceYour);
        }

        public static TermGroup_ScanningInterpretation GetVatAmountInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.VatAmount);
        }

        public static TermGroup_ScanningInterpretation GetTotalAmountIncludeVatInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.TotalAmountIncludeVat);
        }

        public static TermGroup_ScanningInterpretation GetCurrencyCodeInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.CurrencyCode);
        }

        public static TermGroup_ScanningInterpretation GetOCRInterpretation(this ScanningEntryDTO e)
        {
            return e.GetScanningInterpretation(ScanningEntryRowType.OCR);
        }

        public static bool ValueIsValid(this ScanningEntryRowDTO e)
        {
            return e.ValidationError == "0";
        }

        public static bool ValueIsUnsettled(this ScanningEntryRowDTO e)
        {
            return e.ValidationError == "1";
        }

        public static bool ValueNotFound(this ScanningEntryRowDTO e)
        {
            return e.ValidationError == "2";
        }

        #endregion

        #region HeaderFields

        #region BillingType

        public static TermGroup_BillingType GetBillingType(this ScanningEntryDTO e, TermGroup_BillingType defaultBillingType)
        {
            var row = e.GetScanningEntryRow(ScanningEntryRowType.IsCreditInvoice);
            if (row == null)
                return defaultBillingType;

            return StringUtility.GetBool(row.Text) ? TermGroup_BillingType.Credit : TermGroup_BillingType.Debit;
        }

        public static bool IsCredit(this ScanningEntryDTO e)
        {
            return e.GetBillingType(TermGroup_BillingType.Debit) == TermGroup_BillingType.Credit;
        }

        #endregion

        #region InvoiceNr

        public static string GetInvoiceNr(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.InvoiceNr);
        }

        #endregion

        #region InvoiceDate

        public static DateTime? GetInvoiceDate(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowNullableDateValue(ScanningEntryRowType.InvoiceDate);
        }

        #endregion

        #region DueDate

        public static DateTime? GetDueDate(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowNullableDateValue(ScanningEntryRowType.DueDate);
        }

        #endregion

        #region OrderNr

        public static string GetOrderNr(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.OrderNr);
        }

        #endregion

        #region ReferenceYour

        public static string GetReferenceYour(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.ReferenceYour);
        }

        #endregion

        #region ReferenceOur

        public static string GetReferenceOur(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.ReferenceOur);
        }

        #endregion

        #region TotalAmountIncludeVat

        public static decimal GetTotalAmountIncludeVat(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.TotalAmountIncludeVat);
        }

        #endregion

        #region AmountExcludeVat

        public static decimal GetTotalAmountExludeVat(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.TotalAmountExludeVat);
        }

        #endregion

        #region VatAmount

        public static decimal GetVatAmount(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.VatAmount);
        }

        #endregion

        #region CurrencyCode

        public static string GetCurrencyCode(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.CurrencyCode);
        }

        #endregion

        #region OCRNr

        public static string GetOCRNr(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.OCR);
        }

        #endregion

        #region Plusgiro

        public static string GetPlusgiro(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.Plusgiro);
        }

        #endregion

        #region Bankgiro

        public static string GetBankgiro(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.Bankgiro);
        }

        #endregion

        #region OrgNr

        public static string GetOrgNr(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.OrgNr);
        }

        #endregion

        #region IBAN

        public static string GetIBAN(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.IBAN);
        }

        #endregion

        #region VatRate

        public static decimal? GetVatRate(this ScanningEntryDTO e)
        {
            return NumberUtility.GetDecimalRemovePercentageSign(e.GetScanningEntryRowStringValue(ScanningEntryRowType.VatRate));
        }

        #endregion

        #region VatNr

        public static string GetVatNr(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowStringValue(ScanningEntryRowType.VatNr);
        }

        #endregion

        #region FreightAmount

        public static decimal GetFreightAmount(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.FreightAmount);
        }

        #endregion

        #region CentRounding

        public static decimal GetCentRounding(this ScanningEntryDTO e)
        {
            return e.GetScanningEntryRowDecimalValue(ScanningEntryRowType.CentRounding);
        }

        #endregion

        #endregion

        #endregion

        #region TimeBreakTemplateDTO

        public static int GetMinTimeAfterStart(this TimeBreakTemplateDTO e)
        {
            return e.TimeBreakTemplateRows?.OrderBy(i => i.MinTimeAfterStart).FirstOrDefault()?.MinTimeAfterStart ?? 0;
        }

        public static int GetMinTimeBeforeEnd(this TimeBreakTemplateDTO e)
        {
            return e.TimeBreakTemplateRows?.OrderBy(i => i.MinTimeBeforeEnd).FirstOrDefault()?.MinTimeBeforeEnd ?? 0;
        }

        public static List<TimeBreakTemplateRowDTO> GetTemplateRows(this TimeBreakTemplateDTO e)
        {
            return e.GetTemplateRows(SoeTimeBreakTemplateType.None);
        }

        public static List<TimeBreakTemplateRowDTO> GetTemplateRows(this TimeBreakTemplateDTO e, SoeTimeBreakTemplateType type)
        {
            return e.TimeBreakTemplateRows?
                .Where(i => i.State == SoeEntityState.Active && (type == SoeTimeBreakTemplateType.None || i.Type == type))
                .OrderBy(i => i.MinTimeAfterStart)
                .ThenByDescending(i => i.MinTimeBeforeEnd)
                .ToList() ?? new List<TimeBreakTemplateRowDTO>();
        }

        public static TimeBreakTemplateRowDTO GetTemplateRow(this TimeBreakTemplateDTO e, Guid guid)
        {
            return e.TimeBreakTemplateRows?.FirstOrDefault(i => i.Guid == guid);
        }

        #endregion

        #region TimeCodeDTO

        public static TimeCodeRuleDTO GetTimeCodeRule(this TimeCodeDTO e, TermGroup_TimeCodeRuleType type)
        {
            return e.TimeCodeRules.FirstOrDefault(i => i.Type == (int)type);
        }

        public static bool IsWork(this TimeCodeDTO e)
        {
            return PayrollRulesUtil.IsWork(e.Type);
        }

        public static bool IsAbsence(this TimeCodeDTO e)
        {
            return PayrollRulesUtil.IsAbsence(e.Type);
        }

        public static bool IsBreak(this TimeCodeDTO e)
        {
            return PayrollRulesUtil.IsBreak(e.Type);
        }

        public static bool IsAdditionAndDeduction(this TimeCodeDTO e)
        {
            return PayrollRulesUtil.IsAdditionAndDeduction(e.Type);
        }

        public static bool IsMaterial(this TimeCodeDTO e)
        {
            return PayrollRulesUtil.IsMaterial(e.Type);
        }

        public static int GetTimeCodeRuleValue(this TimeCodeSaveDTO e, TermGroup_TimeCodeRuleType type)
        {
            return e?.TimeCodeRules?.FirstOrDefault(t => t.Type == (int)type)?.Value ?? 0;
        }

        #endregion

        #region TimePeriodDTO

        public static List<TimePeriodDTO> GetTimePeriods(this List<TimePeriodHeadDTO> l, DateTime? paymentDate = null)
        {
            List<TimePeriodDTO> timePeriods = new List<TimePeriodDTO>();
            foreach (var head in l)
            {
                timePeriods.AddRange(head.TimePeriods.Where(x => x.PaymentDate.HasValue && (!paymentDate.HasValue || x.PaymentDate.Value == paymentDate.Value.Date)).ToList());
            }

            return timePeriods;
        }

        public static TimePeriodDTO GetTimePeriod(this List<TimePeriodDTO> l, DateTime date)
        {
            return l?.FirstOrDefault(i => i.StartDate <= date && i.StopDate >= date);
        }

        public static List<DateTime> GetDistinctPaymentDates(this List<TimePeriodDTO> l)
        {
            return l?.Where(x => x.PaymentDate.HasValue).Select(x => x.PaymentDate.Value).Distinct().ToList() ?? new List<DateTime>();
        }

        #endregion

        #region TimeScheduleTaskDTO

        public static bool IsBaseNeed(this TimeScheduleTaskDTO e, DayOfWeek givenDayOfWeek)
        {
            return DailyRecurrencePatternDTO.IsBaseNeed(e.RecurrencePattern, givenDayOfWeek);
        }

        public static bool IsSpecificNeed(this TimeScheduleTaskDTO e)
        {
            return (e.StartDate == e.StopDate || e.NbrOfOccurrences == 1) || DailyRecurrencePatternDTO.IsAdditionalNeed(e.RecurrencePattern);
        }

        public static bool HasNoRecurrencePattern(this TimeScheduleTaskDTO e)
        {
            return DailyRecurrencePatternDTO.HasNoRecurrencePattern(e.RecurrencePattern);
        }

        #endregion

        #region TimeScheduleTemplateBlockDTO

        public static List<TimeScheduleTemplateBlockDTO> GetWork(this List<TimeScheduleTemplateBlockDTO> l)
        {
            return l.Where(i => !i.IsBreak).OrderBy(i => i.StartTime).ToList();
        }

        public static List<TimeScheduleTemplateBlockDTO> GetBreaks(this List<TimeScheduleTemplateBlockDTO> l)
        {
            return l.Where(i => i.IsBreak).OrderBy(i => i.StartTime).ToList();
        }

        public static List<TimeScheduleTemplateBlockDTO> RemoveDuplicates(this List<TimeScheduleTemplateBlockDTO> l)
        {
            if (l.IsNullOrEmpty() || l.Count == 1)
                return l;

            var valid = new List<TimeScheduleTemplateBlockDTO>
            {
                l[0],
            };

            for (int i = 1; i < l.Count; i++)
            {
                var current = l[i];
                var prev = l[i - 1];
                if (!current.IsOverlapping(prev))
                    valid.Add(current);
            }
            return valid;
        }

        public static bool IsOverlapping(this TimeScheduleTemplateBlockDTO e1, TimeScheduleTemplateBlockDTO e2)
        {
            return
                e1 != null &&
                e2 != null &&
                CalendarUtility.IsDatesOverlapping(e1.StartTime, e1.StopTime, e2.StartTime, e2.StopTime);
        }

        public static TimeScheduleTemplateBlockDTO GetScheduleInTemplateBlock(this List<TimeScheduleTemplateBlockDTO> templateBlocks, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            return (from tb in templateBlocks
                    where tb.IsScheduleTime(timeScheduleTypeIdsIsNotScheduleTime)
                    orderby tb.StartTime ascending
                    select tb).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlockDTO GetScheduleOutTemplateBlock(this List<TimeScheduleTemplateBlockDTO> templateBlocks, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            return (from tb in templateBlocks
                    where tb.IsScheduleTime(timeScheduleTypeIdsIsNotScheduleTime)
                    orderby tb.StopTime descending
                    select tb).FirstOrDefault();
        }

        public static bool IsScheduleTime(this TimeScheduleTemplateBlockDTO templateBlock, List<int> timeScheduleTypeIdsIsNotScheduleTime)
        {
            if (templateBlock.Type != (int)TermGroup_TimeScheduleTemplateBlockType.Schedule)
                return false;
            if (!templateBlock.TimeScheduleTypeId.HasValue)
                return true;
            if (timeScheduleTypeIdsIsNotScheduleTime == null || timeScheduleTypeIdsIsNotScheduleTime.Count == 0)
                return true;
            if (!timeScheduleTypeIdsIsNotScheduleTime.Contains(templateBlock.TimeScheduleTypeId.Value))
                return true;

            return false;
        }

        public static DateTime GetScheduleIn(this List<TimeScheduleTemplateBlockDTO> templateBlocks, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            return templateBlocks.GetScheduleIn(out _, timeScheduleTypeIdsIsNotScheduleTime);
        }

        public static DateTime GetScheduleIn(this List<TimeScheduleTemplateBlockDTO> templateBlocks, out int timeScheduleTemplatePeriodId, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            timeScheduleTemplatePeriodId = 0;

            var scheduleInTemplateBlock = templateBlocks.GetScheduleInTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleInTemplateBlock == null)
                return CalendarUtility.DATETIME_DEFAULT;

            if (scheduleInTemplateBlock.TimeScheduleTemplatePeriodId.HasValue)
                timeScheduleTemplatePeriodId = scheduleInTemplateBlock.TimeScheduleTemplatePeriodId.Value;
            return scheduleInTemplateBlock.StartTime;
        }

        public static DateTime GetScheduleOut(this List<TimeScheduleTemplateBlockDTO> templateBlocks, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            return templateBlocks.GetScheduleOut(out _, timeScheduleTypeIdsIsNotScheduleTime);
        }

        public static DateTime GetScheduleOut(this List<TimeScheduleTemplateBlockDTO> templateBlocks, out int timeScheduleTemplatePeriodId, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            timeScheduleTemplatePeriodId = 0;

            var scheduleOutTemplateBlock = templateBlocks.GetScheduleOutTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleOutTemplateBlock == null)
                return CalendarUtility.DATETIME_DEFAULT;

            if (scheduleOutTemplateBlock.TimeScheduleTemplatePeriodId.HasValue)
                timeScheduleTemplatePeriodId = scheduleOutTemplateBlock.TimeScheduleTemplatePeriodId.Value;
            return scheduleOutTemplateBlock.StopTime;
        }

        public static int GetScheduleInMinutes(this List<TimeScheduleTemplateBlockDTO> templateBlocks, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            TimeScheduleTemplateBlockDTO scheduleInTemplateBlock = templateBlocks.GetScheduleInTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleInTemplateBlock == null)
                return 0;

            return CalendarUtility.TimeToMinutes(scheduleInTemplateBlock.StartTime);
        }

        public static int GetScheduleOutMinutes(this List<TimeScheduleTemplateBlockDTO> templateBlocks, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            TimeScheduleTemplateBlockDTO scheduleOutTemplateBlock = templateBlocks.GetScheduleOutTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleOutTemplateBlock == null)
                return 0;

            return CalendarUtility.TimeToMinutes(scheduleOutTemplateBlock.StopTime);
        }

        public static List<TimeSchedulePlanningDayDTO> ToTimeSchedulePlanningDayDTOs(this List<TimeScheduleTemplateBlockDTO> l, bool groupOnDateAndEmployeeInsteadOfPeriod = false)
        {
            var dtos = new List<TimeSchedulePlanningDayDTO>();
            if (l != null)
            {
                foreach (var e in l.Where(i => !i.IsBreak))
                {
                    var dto = e.ToTimeSchedulePlanningDayDTO();
                    if (dto != null)
                    {
                        #region Breaks

                        if (e.TimeScheduleEmployeePeriodId.HasValue || groupOnDateAndEmployeeInsteadOfPeriod)
                        {
                            var breaks =
                                groupOnDateAndEmployeeInsteadOfPeriod ?
                                l.Where(tb => tb.IsBreak && tb.EmployeeId != null && tb.EmployeeId.Value == e.EmployeeId.Value && tb.Date != CalendarUtility.DATETIME_DEFAULT && tb.Date == e.Date).ToList() :
                                l.Where(tb => tb.IsBreak && tb.TimeScheduleEmployeePeriodId.HasValue && tb.TimeScheduleEmployeePeriodId.Value == e.TimeScheduleEmployeePeriodId.Value).ToList();

                            int breakNr = 1;
                            foreach (TimeScheduleTemplateBlockDTO b in breaks)
                            {
                                int id = b.TimeScheduleTemplateBlockId;
                                int timeCodeId = b.TimeCodeId;
                                DateTime startTime = b.Date.HasValue ? CalendarUtility.GetDateTime(b.Date.Value, b.StartTime) : b.StartTime;
                                int length = (int)b.StopTime.Subtract(b.StartTime).TotalMinutes;

                                switch (breakNr)
                                {
                                    case 1:
                                        dto.Break1Id = id;
                                        dto.Break1Link = e.Link;
                                        dto.Break1TimeCodeId = timeCodeId;
                                        dto.Break1StartTime = startTime;
                                        dto.Break1Minutes = length;
                                        break;
                                    case 2:
                                        dto.Break2Id = id;
                                        dto.Break2Link = e.Link;
                                        dto.Break2TimeCodeId = timeCodeId;
                                        dto.Break2StartTime = startTime;
                                        dto.Break2Minutes = length;
                                        break;
                                    case 3:
                                        dto.Break3Id = id;
                                        dto.Break3Link = e.Link;
                                        dto.Break3TimeCodeId = timeCodeId;
                                        dto.Break3StartTime = startTime;
                                        dto.Break3Minutes = length;
                                        break;
                                    case 4:
                                        dto.Break4Id = id;
                                        dto.Break4Link = e.Link;
                                        dto.Break4TimeCodeId = timeCodeId;
                                        dto.Break4StartTime = startTime;
                                        dto.Break4Minutes = length;
                                        break;
                                }
                                breakNr++;
                            }
                        }

                        #endregion

                        dtos.Add(dto);
                    }
                }
            }
            return dtos;
        }

        public static TimeSchedulePlanningDayDTO ToTimeSchedulePlanningDayDTO(this TimeScheduleTemplateBlockDTO e)
        {
            if (e == null || !e.Date.HasValue || e.IsBreak)
                return null;

            DateTime startTime = CalendarUtility.MergeDateAndTime(e.Date.Value.AddDays((e.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), e.StartTime);
            DateTime stopTime = startTime.AddMinutes(e.TotalMinutes);

            TimeSchedulePlanningDayDTO dayDTO = new TimeSchedulePlanningDayDTO()
            {
                UniqueId = Guid.NewGuid().ToString(),
                Type = e.Type,
                StartTime = startTime,
                StopTime = stopTime,
                PlannedTime = e.PlannedTime,
                BelongsToPreviousDay = e.BelongsToPreviousDay,
                BelongsToNextDay = e.BelongsToNextDay,
                IsPreliminary = e.IsPreliminary,
                ExtraShift = e.ExtraShift,
                SubstituteShift = e.SubstituteShift,
                ShiftStatus = e.ShiftStatus,
                ShiftUserStatus = e.ShiftUserStatus,
                Link = e.Link,
                Description = e.Description,

                //Set FK
                EmployeeId = e.EmployeeId ?? 0,
                EmployeeChildId = e.EmployeeChildId,
                AccountId = e.AccountId,
                AccountIds = e.AccountInternals?.Select(a => a.AccountId).ToList() ?? e.AccountInternalIds,
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                TimeScheduleEmployeePeriodId = e.TimeScheduleEmployeePeriodId ?? 0,
                TimeScheduleTypeId = e.TimeScheduleTypeId ?? 0,
                TimeCodeId = e.TimeCodeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                ShiftTypeId = e.ShiftTypeId ?? 0,
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                StaffingNeedsRowPeriodId = e.StaffingNeedsRowPeriodId,
            };

            // Order planning
            if (e.CustomerInvoiceId.HasValue)
            {
                dayDTO.Order = new OrderListDTO()
                {
                    OrderId = e.CustomerInvoiceId.Value,
                    ProjectId = e.ProjectId
                };
            }

            return dayDTO;
        }

        #endregion

        #region TimeScheduleTemplateBlockDTO

        public static List<TimeScheduleTemplateBlockSmallDTO> GetSchedule(this List<TimeScheduleTemplateBlockSmallDTO> l, DateTime? date = null)
        {
            return l.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule && (!date.HasValue || date.Value == i.Date)).ToList();
        }

        public static List<TimeScheduleTemplateBlockSmallDTO> GetStandby(this List<TimeScheduleTemplateBlockSmallDTO> l, DateTime? date = null)
        {
            return l.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby && (!date.HasValue || date.Value == i.Date)).ToList();
        }

        public static List<TimeScheduleTemplateBlockSmallDTO> GetWork(this List<TimeScheduleTemplateBlockSmallDTO> l, DateTime? date = null)
        {
            return l.Where(i => !i.IsBreak).OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static List<TimeScheduleTemplateBlockSmallDTO> GetBreaks(this List<TimeScheduleTemplateBlockSmallDTO> l)
        {
            return l.Where(i => i.IsBreak).OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        #endregion

        #region TimeScheduleTemplateBlockTaskDTO

        public static List<TimeScheduleTemplateBlockTaskDTO> GetOverlappingTask(this List<TimeScheduleTemplateBlockTaskDTO> tasks, DateTime time)
        {
            return tasks.Where(x => x.IsOverlapped(time)).ToList();
        }

        public static List<TimeScheduleTemplateBlockTaskDTO> GetTaskThatEndsBeforeGivenTime(this List<TimeScheduleTemplateBlockTaskDTO> tasks, DateTime time)
        {
            return tasks.Where(x => x.EndsBeforeGivenTime(time)).ToList();
        }

        public static List<TimeScheduleTemplateBlockTaskDTO> GetTaskThatStartsAfterGivenTime(this List<TimeScheduleTemplateBlockTaskDTO> tasks, DateTime time)
        {
            return tasks.Where(x => x.StartsAfterGivenTime(time)).ToList();
        }

        public static bool IsOverlapped(this TimeScheduleTemplateBlockTaskDTO task, DateTime time)
        {
            return task.StartTime < time && task.StopTime > time;
        }

        public static bool EndsBeforeGivenTime(this TimeScheduleTemplateBlockTaskDTO task, DateTime time)
        {
            return task.StopTime <= time;
        }

        public static bool StartsAfterGivenTime(this TimeScheduleTemplateBlockTaskDTO task, DateTime time)
        {
            return task.StartTime >= time;
        }

        public static bool IsWithInRange(this TimeScheduleTemplateBlockTaskDTO task, DateTime rangeFrom, DateTime rangeTo)
        {
            return CalendarUtility.IsNewOverlappedByCurrent(task.StartTime, task.StopTime, rangeFrom, rangeTo);
        }

        public static TimeScheduleTemplateBlockTaskDTO CopyAsNew(this TimeScheduleTemplateBlockTaskDTO task)
        {
            return new TimeScheduleTemplateBlockTaskDTO()
            {
                TimeScheduleTemplateBlockTaskId = 0,
                TimeScheduleTemplateBlockId = null,
                TimeScheduleTaskId = task.TimeScheduleTaskId,
                IncomingDeliveryRowId = task.IncomingDeliveryRowId,
                StartTime = task.StartTime,
                StopTime = task.StopTime,
                State = SoeEntityState.Active,
            };
        }

        #endregion

        #region UserCompanyRoleDTO

        public static List<UserCompanyRoleDTO> GetSortedAttestRoleUsers(this IEnumerable<UserCompanyRoleDTO> l)
        {
            return l?.OrderBy(o => o.RoleSort).ToList() ?? new List<UserCompanyRoleDTO>();
        }

        public static UserCompanyRoleDTO GetExecutiveUserCompanyRoleUser(this List<UserCompanyRoleDTO> l, List<int> userIds, int startSort = 0)
        {
            var sorted = l?
                .Where(w => userIds.Contains(w.UserId))
                .GetSortedAttestRoleUsers()
                .Where(w => w.RoleSort > startSort)
                .ToList() ?? new List<UserCompanyRoleDTO>();

            if (sorted.Count == 1)
                return sorted.First();
            if (sorted.Count > 1 && sorted.First().RoleSort != sorted.Skip(1).First().RoleSort)
                return sorted.First();
            return null;
        }

        #endregion

        #region VacationGroupDTO

        public static DateTime CalculateFromDate(this VacationGroupDTO e, DateTime date)
        {
            int month = e.FromDate.Month;
            int year = date.Month < month ? date.Year - 1 : date.Year;
            return new DateTime(year, month, 1);
        }

        public static DateTime GetPrevDay(this VacationGroupDTO e)
        {
            return e.RealDateFrom.AddDays(-1);
        }

        public static DateTime GetPrevYear(this VacationGroupDTO e)
        {
            return e.RealDateFrom.AddYears(-1);
        }

        #endregion

        #endregion

        #region Views

        #region ChangeStatusGridViewDTO

        public static Dictionary<int, string> ToDictionary(this List<ChangeStatusGridViewDTO> items)
        {
            var dict = new Dictionary<int, string>();
            foreach (var item in items)
            {
                if (!dict.ContainsKey(item.InvoiceId))
                    dict.Add(item.InvoiceId, item.InvoiceNr);
            }
            return dict;
        }

        #endregion

        #region EmployeeSchedulePlacementGridViewDTO

        public static void SetCurrentEmploymentProperties(this EmployeeSchedulePlacementGridViewDTO dto, DateTime dateFrom, DateTime dateTo, bool forward = false)
        {
            if (dto.Employments != null)
                dto.SetCurrentEmploymentProperties(dto.Employments.GetEmployment(dateFrom, dateTo, forward: forward));
        }

        public static void SetCurrentEmploymentProperties(this EmployeeSchedulePlacementGridViewDTO dto, DateTime? date)
        {
            if (dto.Employments != null)
                dto.SetCurrentEmploymentProperties(dto.Employments.GetEmployment(date));
        }

        #region Help-methods

        private static void SetCurrentEmploymentProperties(this EmployeeSchedulePlacementGridViewDTO dto, EmploymentDTO employmentDTO)
        {
            if (employmentDTO == null)
                return;

            //EmployeeGroup
            dto.EmployeeGroupId = employmentDTO.EmployeeGroupId;
            dto.EmployeeGroupName = employmentDTO.EmployeeGroupName;
        }

        #endregion

        #endregion

        #endregion

        #region Enums

        #region SoeReportTemplateType

        public static bool IsReportMigrated(this SoeReportTemplateType type)
        {
            switch (type)
            {
                case SoeReportTemplateType.AgdEmployeeReport:
                case SoeReportTemplateType.CertificateOfEmploymentReport:
                case SoeReportTemplateType.CollectumReport:
                case SoeReportTemplateType.CSR:
                case SoeReportTemplateType.EmployeeListReport:
                case SoeReportTemplateType.EmployeeTimePeriodReport:
                case SoeReportTemplateType.EmployeeVacationDebtReport:
                case SoeReportTemplateType.EmployeeVacationInformationReport:
                case SoeReportTemplateType.ForaReport:
                case SoeReportTemplateType.ForaMonthlyReport:
                case SoeReportTemplateType.KPAReport:
                case SoeReportTemplateType.KU10Report:
                case SoeReportTemplateType.PayrollAccountingReport:
                case SoeReportTemplateType.PayrollVacationAccountingReport:
                case SoeReportTemplateType.PayrollPeriodWarningCheck:
                case SoeReportTemplateType.PayrollProductReport:
                case SoeReportTemplateType.PayrollSlip:
                case SoeReportTemplateType.PayrollTransactionStatisticsReport:
                case SoeReportTemplateType.PayrollVacationPayReport:
                case SoeReportTemplateType.SCB_KLPReport:
                case SoeReportTemplateType.SCB_KSJUReport:
                case SoeReportTemplateType.SCB_KSPReport:
                case SoeReportTemplateType.SCB_SLPReport:
                case SoeReportTemplateType.SKDReport:
                case SoeReportTemplateType.SNReport:
                case SoeReportTemplateType.TimeAbsenceReport:
                case SoeReportTemplateType.TimeAccumulatorReport:
                case SoeReportTemplateType.TimeAccumulatorDetailedReport:
                case SoeReportTemplateType.TimeCategorySchedule:
                case SoeReportTemplateType.TimeCategoryStatistics:
                case SoeReportTemplateType.TimeEmployeeLineSchedule:
                case SoeReportTemplateType.TimeEmployeeSchedule:
                case SoeReportTemplateType.TimeEmployeeScheduleSmallReport:
                case SoeReportTemplateType.TimeEmployeeTemplateSchedule:
                case SoeReportTemplateType.TimeEmploymentContract:
                case SoeReportTemplateType.TimeEmploymentDynamicContract:
                case SoeReportTemplateType.TimeMonthlyReport:
                case SoeReportTemplateType.TimePayrollTransactionReport:
                case SoeReportTemplateType.TimePayrollTransactionSmallReport:
                case SoeReportTemplateType.TimeSalaryControlInfoReport:
                case SoeReportTemplateType.TimeSalarySpecificationReport:
                case SoeReportTemplateType.TimeSaumaSalarySpecificationReport:
                case SoeReportTemplateType.TimeScheduleBlockHistory:
                case SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport:
                case SoeReportTemplateType.TimeStampEntryReport:
                case SoeReportTemplateType.RoleReport:
                case SoeReportTemplateType.GeneralLedger:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPayrollReport(this SoeReportTemplateType type)
        {
            switch (type)
            {
                case SoeReportTemplateType.PayrollSlip:
                case SoeReportTemplateType.CSR:
                case SoeReportTemplateType.SNReport:
                case SoeReportTemplateType.PayrollVacationPayReport:
                case SoeReportTemplateType.EmployeeTimePeriodReport:
                case SoeReportTemplateType.PayrollPeriodWarningCheck:
                case SoeReportTemplateType.PayrollTransactionStatisticsReport:
                case SoeReportTemplateType.PayrollVacationAccountingReport:
                case SoeReportTemplateType.PayrollAccountingReport:
                case SoeReportTemplateType.EmployeeVacationInformationReport:
                case SoeReportTemplateType.EmployeeVacationDebtReport:
                case SoeReportTemplateType.KU10Report:
                case SoeReportTemplateType.SKDReport:
                case SoeReportTemplateType.CertificateOfEmploymentReport:
                case SoeReportTemplateType.CollectumReport:
                case SoeReportTemplateType.SCB_SLPReport:
                case SoeReportTemplateType.KPAReport:
                case SoeReportTemplateType.KPADirektReport:
                case SoeReportTemplateType.SkandiaPension:
                case SoeReportTemplateType.ForaMonthlyReport:
                case SoeReportTemplateType.ForaReport:
                case SoeReportTemplateType.SCB_KSPReport:
                case SoeReportTemplateType.SCB_KSJUReport:
                case SoeReportTemplateType.SCB_KLPReport:
                case SoeReportTemplateType.AgdEmployeeReport:
                case SoeReportTemplateType.Bygglosen:
                case SoeReportTemplateType.Kronofogden:

                case SoeReportTemplateType.PayrollTransactionAnalysis:
                case SoeReportTemplateType.EmployeeTimePeriodAnalysis:
                case SoeReportTemplateType.AgiAbsenceAnalysis:

                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPayrollReportWithPeriods(this SoeReportTemplateType type)
        {
            if (type.IsCollectum() ||
                type.IsCertificateOfEmployment() ||
                type.IsKU10() ||
                type.IsSCB() ||
                type.IsPayrollTransactionStatistics() ||
                type.IsSKD() ||
                type.IsPayrollAccounting() ||
                type.IsPayrollVacationAccounting() ||
                type.IsAGD() ||
                type.IsPayrollPeriodWarningCheck() ||
                type.IsKpaDirektReport() ||
                type.IsSkandiaReport() ||
                type.IsKronofogden() ||
                type.IsEmployeeTimePeriodReport() ||
                type.IsPayrollAnalysis() ||
                type.IsForaMonthly()
                )
                return true;
            return false;
        }

        public static bool IsPayrollSlip(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.PayrollSlip;
        }

        public static bool IsCollectum(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.CollectumReport;
        }

        public static bool IsSCB(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.SCB_KLPReport || type == SoeReportTemplateType.SCB_KSJUReport || type == SoeReportTemplateType.SCB_KSPReport || type == SoeReportTemplateType.SCB_SLPReport;
        }

        public static bool IsPayrollTransactionStatistics(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.PayrollTransactionStatisticsReport;
        }

        public static bool IsCertificateOfEmployment(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.CertificateOfEmploymentReport;
        }

        public static bool IsSKD(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.SKDReport;
        }

        public static bool IsFora(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.ForaReport;
        }
        public static bool IsForaMonthly(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.ForaMonthlyReport;
        }
        public static bool IsKU10(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.KU10Report;
        }

        public static bool IsAGD(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.AgdEmployeeReport || type == SoeReportTemplateType.EmployeeTimePeriodAnalysis;
        }

        public static bool IsPayrollAccounting(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.PayrollAccountingReport;
        }

        public static bool IsPayrollVacationAccounting(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.PayrollVacationAccountingReport;
        }

        public static bool IsPayrollPeriodWarningCheck(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.PayrollPeriodWarningCheck;
        }

        public static bool IsEmployeeTimePeriodReport(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.EmployeeTimePeriodReport;
        }

        public static bool IsPayrollAnalysis(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.PayrollTransactionAnalysis;
        }

        public static bool LoadEmployeeVacationData(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.EmployeeVacationDebtReport || type == SoeReportTemplateType.EmployeeVacationInformationReport || type == SoeReportTemplateType.PayrollTransactionStatisticsReport;
        }

        public static bool IsKpaDirektReport(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.KPADirektReport;
        }
        public static bool IsSkandiaReport(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.SkandiaPension;
        }
        public static bool IsKronofogden(this SoeReportTemplateType type)
        {
            return type == SoeReportTemplateType.Kronofogden;
        }


        #endregion

        #region TermGroup_ShiftHistoryType

        public static bool IsSaveTimeScheduleShift(this TermGroup_ShiftHistoryType type)
        {
            return type == TermGroup_ShiftHistoryType.TaskSaveTimeScheduleShift;
        }
        public static bool IsCreatingScenario(this TermGroup_ShiftHistoryType type)
        {
            return type == TermGroup_ShiftHistoryType.CreateScenario;
        }
        public static bool IsActivatingScenario(this TermGroup_ShiftHistoryType type)
        {
            return type == TermGroup_ShiftHistoryType.ActivateScenario;
        }
        public static bool IsApplyingAbsence(this TermGroup_ShiftHistoryType type)
        {
            return type == TermGroup_ShiftHistoryType.AbsencePlanning || type == TermGroup_ShiftHistoryType.AbsenceRequestPlanning;
        }

        #endregion

        #region Permission

        public static bool IsValid(this Permission currentPermission, Permission permission)
        {
            if (permission == Permission.Readonly)
                return currentPermission == Permission.Readonly || currentPermission == Permission.Modify;
            else if (permission == Permission.Modify)
                return currentPermission == Permission.Modify;
            else
                return false;
        }

        public static Permission EvaluatePermission(this Permission higherLayerPermission, Permission lowerLayerPermission)
        {
            if (lowerLayerPermission == Permission.Modify)
                return higherLayerPermission;
            if (lowerLayerPermission == Permission.Readonly && higherLayerPermission >= lowerLayerPermission)
                return lowerLayerPermission;
            return Permission.None;
        }

        public static Permission ToPermission(this int permissionId)
        {
            if (Enum.IsDefined(typeof(Permission), permissionId))
                return (Permission)permissionId;
            return Permission.None;
        }

        #endregion

        #endregion

        #region T

        public static List<T> ObjToList<T>(this T obj)
        {
            List<T> list = new List<T>();
            if (obj != null)
                list.Add(obj);
            return list;
        }

        public static List<T2> GetList<T1, T2>(this Dictionary<T1, List<T2>> d, List<T1> keys, bool nullIfNotFound = false, params bool[] conditions)
        {
            List<T2> result = new List<T2>();
            foreach (var key in keys)
            {
                result.AddRange(d.GetList(key, nullIfNotFound, conditions));
            }
            return result;
        }

        public static List<T2> GetList<T1, T2>(this Dictionary<T1, List<T2>> d, T1 key, bool nullIfNotFound = false, params bool[] conditions)
        {
            if (d != null && d.ContainsKey(key) && (conditions.Length == 0 || conditions.All(b => b)))
                return d[key];
            else
                return nullIfNotFound ? null : new List<T2>();
        }

        public static T2 GetValue<T1, T2>(this Dictionary<T1, T2> d, T1 key, params bool[] conditions)
        {
            if (d != null && d.ContainsKey(key) && (conditions.Length == 0 || conditions.All(b => b)))
                return d[key];
            else
                return default(T2);
        }

        public static void SetValue<T1, T2>(this Dictionary<T1, T2> d, T1 key, T2 value)
        {
            if (value == null)
                return;

            if (d.ContainsKey(key))
                d[key] = value;
            else
                d.Add(key, value);
        }

        #endregion

        #region Interfaces

        #region ICreatedModified

        public static void SetCreated(this ICreated e, DateTime created, string createdBy)
        {
            if (e != null && !e.Created.HasValue)
            {
                e.Created = created;
                e.CreatedBy = createdBy;
            }
        }

        public static void SetCreated(this ICreatedNotNull e, DateTime created, string createdBy)
        {
            if (e != null)
            {
                e.Created = created;
                e.CreatedBy = createdBy;
            }
        }

        public static void SetModified(this IModified e, DateTime modified, string modifiedBy)
        {
            if (e != null)
            {
                e.Modified = modified;
                e.ModifiedBy = modifiedBy;
            }
        }

        #endregion

        #region IEmployment

        #region Filtering / Sorting

        public static IEnumerable<T> FilterState<T>(this IEnumerable<T> l, bool discardState = false, bool discardTemporaryPrimary = false, bool includeSecondary = false) where T : IEmployment
        {
            if (includeSecondary)
                discardState = false;

            return l?.Where(e =>
                        (discardState || (e.StateId == (int)SoeEntityState.Active || (includeSecondary && e.StateId == (int)SoeEntityState.Hidden))) &&
                        (!discardTemporaryPrimary || !e.IsTemporaryPrimary));
        }

        public static IEnumerable<T> FilterDates<T>(this IEnumerable<T> l, DateTime date) where T : IEmployment
        {
            return l.Where(e => e.GetDateFromOrMin() <= date && e.GetDateToOrMax() >= date).OrderBy(e => e.GetDateFromOrMin());
        }

        public static IEnumerable<T> FilterDates<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo) where T : IEmployment
        {
            return l.Where(e => e.GetDateFromOrMin() <= dateTo.Date && e.GetDateToOrMax() >= dateFrom.Date).OrderBy(e => e.GetDateFromOrMin());
        }

        public static IEnumerable<T> OnlyTemporaryPrimary<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l?.Where(e => e.IsTemporaryPrimary);
        }

        public static IOrderedEnumerable<T> SortByDateAndTemporaryPrimary<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l.SortByDate().ThenByTemporaryPrimary();
        }

        public static IOrderedEnumerable<T> SortByDateAndTemporaryPrimaryDesc<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l.SortByDateDesc().ThenByTemporaryPrimary();
        }

        public static IOrderedEnumerable<T> SortByDate<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l.OrderBy(e => e.DateFrom ?? CalendarUtility.DATETIME_DEFAULT);
        }

        public static IOrderedEnumerable<T> SortByDateDesc<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l.OrderByDescending(e => e.GetDateToOrMax());
        }

        public static IOrderedEnumerable<T> SortByTemporaryPrimary<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l.OrderByDescending(e => e.IsTemporaryPrimary);
        }

        public static IOrderedEnumerable<T> ThenByTemporaryPrimary<T>(this IOrderedEnumerable<T> l) where T : IEmployment
        {
            return l.ThenByDescending(e => e.IsTemporaryPrimary);
        }

        #endregion

        #region Get List

        public static List<T> GetActiveEmployments<T>(this IEnumerable<T> l, bool discardTemporaryPrimary = false, bool includeSecondary = false) where T : IEmployment
        {
            return l?.FilterState(false, discardTemporaryPrimary, includeSecondary).SortByDateAndTemporaryPrimary().ToEmploymentList() ?? new List<T>();
        }

        public static List<T> GetActiveEmploymentsDesc<T>(this IEnumerable<T> l, bool discardTemporaryPrimary = false, bool includeSecondary = false) where T : IEmployment
        {
            return l?.FilterState(false, discardTemporaryPrimary, includeSecondary).SortByDateAndTemporaryPrimaryDesc().ToEmploymentList() ?? new List<T>();
        }

        public static List<T> GetEmployments<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo, bool discardState = false, bool discardTemporaryPrimary = false, bool includeSecondary = false) where T : IEmployment
        {
            return l?.FilterState(discardState, discardTemporaryPrimary, includeSecondary).FilterDates(dateFrom, dateTo).SortByDateAndTemporaryPrimary().ToEmploymentList() ?? new List<T>();
        }

        public static List<T> GetEmploymentsDesc<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo, bool discardState = false, bool discardTemporaryPrimary = false, bool includeSecondary = false) where T : IEmployment
        {
            return l?.FilterState(discardState, discardTemporaryPrimary, includeSecondary).FilterDates(dateFrom, dateTo).SortByDateAndTemporaryPrimaryDesc().ToEmploymentList() ?? new List<T>();
        }

        #endregion

        #region Get Single

        public static T GetEmployment<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo, bool forward = true, bool forceNoApply = false, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null) where T : IEmployment
        {
            T e = default(T);
            if (!l.IsNullOrEmpty())
            {
                if (forward)
                {
                    DateTime date = dateFrom;
                    while (date <= dateTo && e == null)
                    {
                        e = l.GetEmployment(date, false, false, forceNoApply, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict);
                        if (date < CalendarUtility.DATETIME_MAXVALUE)
                            date = date.AddDays(1);
                    }
                }
                else
                {
                    DateTime date = dateTo;
                    while (date >= dateFrom && e == null)
                    {
                        e = l.GetEmployment(date, false, false, forceNoApply, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict);
                        if (date > CalendarUtility.DATETIME_MINVALUE)
                            date = date.AddDays(-1);
                    }
                }
            }
            return e;
        }

        public static T GetEmployment<T>(this IEnumerable<T> l, DateTime? date = null, bool discardState = false, bool discardParallell = false, bool forceNoApply = false, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return default(T);

            var filtered = l.FilterState(discardState, discardParallell).FilterDates(date ?? DateTime.Today);
            var e = (filtered.ContainsTemporaryPrimary() ? filtered.SortByTemporaryPrimary() : filtered.SortByDate()).FirstOrDefault();
            if (!forceNoApply)
                e.ApplyEmployment(l, date, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, annualLeaveGroups);
            return e;
        }

        public static T GetEmployment<T>(this IEnumerable<T> l, int employmentId, bool forceNoApply = false, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return default(T);

            var e = l.FirstOrDefault(f => f.EmploymentId == employmentId);
            if (!forceNoApply)
                e.ApplyEmployment(l, e?.DateFrom, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, annualLeaveGroups);
            return e;
        }

        public static T GetSecondaryEmployment<T>(this IEnumerable<T> l, DateTime? date = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return default(T);
            return l.FilterDates(date ?? DateTime.Today).Where(e => e.IsSecondaryEmployment).SortByDate().FirstOrDefault();
        }

        public static T GetFirstEmployment<T>(this IEnumerable<T> l, bool discardState = false, bool discardParallell = false, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return default(T);
            var e = l.FilterState(discardState, discardParallell).SortByDateAndTemporaryPrimary().FirstOrDefault();
            e.ApplyEmployment(l, e?.DateFrom, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, annualLeaveGroups);
            return e;
        }

        public static T GetLastEmployment<T>(this IEnumerable<T> l, DateTime? limitStartDate = null, bool discardState = false, bool discardParallell = false, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return default(T);
            var e = l.FilterState(discardState, discardParallell).SortByDateDesc().FirstOrDefault();
            if (e?.DateTo != null && limitStartDate.HasValue && e.DateTo.Value < limitStartDate.Value)
                return default(T);
            e.ApplyEmployment(l, e?.DateTo, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, annualLeaveGroups);
            return e;
        }

        public static T GetPrevEmployment<T>(this IEnumerable<T> l, DateTime date, bool discardState = false, bool discardParallell = false, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return default(T);
            var e = l.FilterState(discardState).Where(i => i.GetDateToOrMax() < date).SortByDateAndTemporaryPrimaryDesc().FirstOrDefault();
            e.ApplyEmployment(l, e?.DateTo, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, annualLeaveGroups);
            return e;
        }

        public static T GetNextEmployment<T>(this IEnumerable<T> l, DateTime date, bool discardState = false, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return default(T);
            var e = l.FilterState(discardState).Where(i => i.GetDateFromOrMin() >= date).SortByDateAndTemporaryPrimary().FirstOrDefault();
            e.ApplyEmployment(l, e?.DateFrom, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, annualLeaveGroups);
            return e;
        }

        public static T GetNearestEmployment<T>(this IEnumerable<T> l, DateTime date, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List<EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            var e = l.GetEmployment(date);
            if (e != null)
                return e;

            //if the employee HAS been or WILL be employed first and last employment should not be null
            var first = l.GetFirstEmployment();
            var last = l.GetLastEmployment();
            if (first == null || last == null)
                return default(T);

            DateTime earliestDate = first.DateFrom ?? date.AddYears(-1);
            DateTime latestDate = last.DateTo ?? date.AddMonths(1);
            if (earliestDate <= date)
                e = l.GetEmployment(earliestDate, date, forward: false);
            else if (date <= latestDate)
                e = l.GetEmployment(date, latestDate, forward: true);
            if (e == null)
                e = last;

            e.ApplyEmployment(l, date, employeeGroups, payrollGroups, employmentTypes, employmentEndReasonsDict, annualLeaveGroups);
            return e;
        }

        public static List<T> GetActiveOrHidden<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l?.Where(e => e.IsActiveOrHidden()).ToList() ?? new List<T>();
        }

        public static bool IsActiveOrHidden<T>(this T e) where T : IEmployment
        {
            return e != null && (e.StateId == (int)SoeEntityState.Active || e.StateId == (int)SoeEntityState.Hidden);
        }

        public static int? GetEmploymentId<T>(this List<T> l, DateTime dateFrom, DateTime dateTo, bool forward = true) where T : IEmployment
        {
            return l?.GetEmployment(dateFrom, dateTo, forward, forceNoApply: true)?.EmploymentId;
        }

        public static int? GetEmploymentId<T>(this List<T> l, DateTime? date = null) where T : IEmployment
        {
            return l?.GetEmployment(date, forceNoApply: true)?.EmploymentId;
        }

        public static bool HasEmployment<T>(this IEnumerable<T> l, DateTime? date = null) where T : IEmployment
        {
            return l.GetEmployment(date, forceNoApply: true) != null;
        }

        public static bool HasEmploymentAllDays<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return false;

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                if (!l.HasEmployment(date))
                    return false;
                date = date.AddDays(1);
            }
            return true;
        }

        public static bool HasSameEmploymentAllDays<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return false;

            l = l?.FilterDates(dateFrom, dateTo);
            return l.Count() == 1;
        }

        public static bool HasTemporaryEmploymentAnyDay<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo, int? skipEmploymentId = null) where T : IEmployment
        {
            l = l?.FilterState().FilterDates(dateFrom, dateTo).OnlyTemporaryPrimary();
            return l?.Any(e => e.EmploymentId > 0 && (!skipEmploymentId.HasValue || e.EmploymentId != skipEmploymentId.Value) && CalendarUtility.IsDatesOverlappingNullable(dateFrom, dateTo, e.DateFrom, e.DateTo)) ?? false;
        }

        public static bool HasActiveEmployments<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l?.FilterState().Any() ?? false;
        }

        public static bool IsNew(this IEmployment e)
        {
            return e.EmploymentId == 0 && e.IsActiveOrHidden();
        }

        #endregion

        #region Get EmploymentDates

        public static List<DateTime> GetEmploymentDates<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo, List<DateTime> allDates = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return new List<DateTime>();
            if (!l.ContainsTemporaryPrimary() && l.Any(e => e.GetDateFromOrMin() <= dateFrom && e.GetDateToOrMax() >= dateTo))
                return allDates ?? CalendarUtility.GetDates(dateFrom, dateTo);

            List<DateTime> employmentDates = new List<DateTime>();
            DateTime currentDate = dateFrom;
            while (currentDate <= dateTo)
            {
                if (l.HasEmployment(currentDate))
                    employmentDates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
            return employmentDates;
        }

        public static int GetEmploymentDaysToDate<T>(this IEnumerable<T> l, DateTime? date = null, DateTime? defaultStopDateIfEmpty = null) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return 0;

            date = date?.Date ?? DateTime.Today;

            int employmentDays = 0;
            foreach (var e in l.GetActiveEmployments().Where(e => e.GetDateFromOrMin() <= date))
            {
                employmentDays += e.GetEmploymentDaysToDate(date.Value);
            }
            return employmentDays;
        }

        public static int GetEmploymentDaysToDate(this IEmployment e, DateTime dateTo)
        {
            if (e.HasHibernatingPeriods())
                return e.GetNoneHibernatingPeriods(stop: dateTo).Sum(p => p.GetNumberOfDays());
            else
                return (int)CalendarUtility.GetEarliestDate(e.GetDateToOrMax(), dateTo).Subtract(e.DateFrom.Value).TotalDays + 1;
        }

        public static int GetEmploymentDays<T>(this T e, DateTime? startDate = null, DateTime? stopDate = null) where T : IEmployment
        {
            if (e.GetDateFromOrMin() > e.GetDateToOrMax())
                return 0;
            if (startDate.HasValue && stopDate.HasValue && startDate.Value > stopDate.Value)
                return 0;

            DateTime employmentFromDate = CalendarUtility.GetLatestDate(e.GetDateFromOrMin(), startDate);
            DateTime employmentToDate = CalendarUtility.GetEarliestDate(e.GetDateToOrMax(max: DateTime.Today), stopDate);
            if (employmentFromDate > employmentToDate)
                return 0;

            if (e.HasHibernatingPeriods())
                return e.GetNoneHibernatingPeriods(employmentFromDate, employmentToDate).Sum(p => p.GetNumberOfDays());
            else
                return (int)employmentToDate.Subtract(employmentFromDate).TotalDays + 1;
        }

        public static int GetEmploymentDaysUntilDate(this IEmployment e, DateTime dateTo)
        {
            DateTime startDate = e.GetDateFromOrMin();
            if (startDate == dateTo)
                return 1;

            return e != null ? (int)dateTo.Subtract(startDate).TotalDays : 0;
        }

        public static DateTime? GetEmploymentDate(this IEmployment e)
        {
            return e?.DateFrom;
        }

        public static DateTime? GetEmploymentDate(this IEnumerable<IEmployment> l)
        {
            return l?.GetFirstEmployment()?.DateFrom;
        }

        public static DateTime? GetEndDate(this IEmployment e)
        {
            return e?.DateTo;
        }

        public static DateTime? GetEndDate(this IEnumerable<IEmployment> l)
        {
            return l?.GetLastEmployment()?.DateTo;
        }

        public static DateTime GetValidEmploymentDate(this IEmployment e, DateTime date)
        {
            if (e != null && e.DateFrom.HasValue && e.DateFrom.Value > date)
                return e.DateFrom.Value;
            else if (e != null && e.DateTo.HasValue && e.DateTo.Value < date)
                return e.DateTo.Value;
            return date;
        }

        public static DateTime GetDateFromOrMin(this IEmployment e)
        {
            return e?.DateFrom?.Date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime GetDateToOrMax(this IEmployment e, DateTime? max = null)
        {
            return e?.DateTo?.Date ?? (max ?? DateTime.MaxValue);
        }

        public static void EnsureDateFrom<T>(this List<T> l) where T : IEmployment
        {
            if (l.IsNullOrEmpty())
                return;
            l.Where(e => !e.DateFrom.HasValue).ToList().ForEach(e => e.GetDateFromOrMin());
        }

        #endregion

        #region TemporaryPrimary / Hibernating 

        public static List<T> GetTemporaryPrimary<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l?.Where(e => e.IsTemporaryPrimary && e.StateId == (int)SoeEntityState.Active).ToList() ?? new List<T>();
        }

        public static List<DateRangeDTO> GetNoneHibernatingPeriods(this IEmployment e, DateTime? start = null, DateTime? stop = null)
        {
            if (e == null)
                return new List<DateRangeDTO>();

            List<DateRangeDTO> noneHibernatingPeriods = new List<DateRangeDTO>();
            if (e.HasHibernatingPeriods())
            {
                DateTime min = CalendarUtility.GetLatestDate(e.GetDateFromOrMin(), start);
                DateTime max = CalendarUtility.GetEarliestDate(e.GetDateToOrMax(), stop);
                DateTime currentStart = min;
                foreach (DateRangeDTO hibernatingPeriod in e.HibernatingPeriods)
                {
                    if (!CalendarUtility.IsDatesOverlapping(currentStart, max, hibernatingPeriod.Start, hibernatingPeriod.Stop))
                        continue;

                    DateTime currentStop = CalendarUtility.GetEarliestDate(hibernatingPeriod.Start.AddDays(-1), max);
                    if (currentStart <= currentStop)
                        noneHibernatingPeriods.Add(new DateRangeDTO(currentStart, currentStop));

                    currentStart = hibernatingPeriod.Stop.AddDays(1);
                }
                if (max > currentStart)
                    noneHibernatingPeriods.Add(new DateRangeDTO(currentStart, max));
            }
            if (!noneHibernatingPeriods.Any())
                noneHibernatingPeriods.Add(new DateRangeDTO(e.GetDateFromOrMin(), e.GetDateToOrMax()));
            return noneHibernatingPeriods;
        }

        public static List<DateRangeDTO> GetHibernatingPeriods<T>(this List<T> l, DateTime dateFrom, DateTime dateTo) where T : IEmployment
        {
            if (!l.ContainsTemporaryPrimary())
                return new List<DateRangeDTO>();

            return l
                .FilterDates(dateFrom, dateTo)
                .Where(e => !e.HibernatingPeriods.IsNullOrEmpty())
                .SelectMany(e => e.HibernatingPeriods)
                .Where(p => CalendarUtility.IsDatesOverlapping(dateFrom, dateTo, p.Start, p.Stop))
                .OrderBy(p => p.Start)
                .ToList();
        }

        public static void SetHibernatingPeriods<T>(this IEmployment e, IEnumerable<T> l) where T : IEmployment
        {
            if (e != null && !e.IsTemporaryPrimary && !e.HasHibernatingPeriods() && l.ContainsTemporaryPrimary())
            {
                e.HibernatingPeriods = new List<DateRangeDTO>();
                foreach (var other in l.FilterState().OnlyTemporaryPrimary().SortByDate())
                {
                    if (CalendarUtility.GetOverlappingDates(other.GetDateFromOrMin(), other.GetDateToOrMax(), e.GetDateFromOrMin(), e.GetDateToOrMax(), out DateTime hibernatingStart, out DateTime hibernatingStop))
                    {
                        DateRangeDTO hibernatingPeriod = new DateRangeDTO(hibernatingStart, hibernatingStop);
                        e.HibernatingPeriods.Add(hibernatingPeriod);
                        if (e is EmploymentDTO employment)
                            employment.SetHibernatingTimeDeviationCauseName(hibernatingPeriod);
                    }

                }
            }
        }

        public static void SetHibernatingTimeDeviationCauseName(this EmploymentDTO e, DateRangeDTO hibernatingPeriod)
        {
            if (e?.Changes == null || hibernatingPeriod == null)
                return;

            List<EmploymentChangeDTO> hibernatingChanges = e.Changes
                .Where(c => c.FieldType == TermGroup_EmploymentChangeFieldType.Hibernating && CalendarUtility.IsDatesOverlapping(hibernatingPeriod.Start, hibernatingPeriod.Stop, e.GetDateFromOrMin(), e.GetDateToOrMax()))
                .OrderByDescending(c => c.Created)
                .ToList();

            if (hibernatingChanges.Any())
            {
                hibernatingPeriod.Comment = hibernatingChanges.FirstOrDefault()?.Comment ?? "Vilande";
                hibernatingChanges.ForEach(c => c.FieldTypeNameSuffix = hibernatingPeriod.Comment);
            }
        }

        public static bool HasHibernatingPeriods(this IEmployment e)
        {
            return e != null && !e.HibernatingPeriods.IsNullOrEmpty();
        }

        public static bool IsHibernating(this IEmployment e, DateTime date)
        {
            return e?.HibernatingPeriods?.Any(p => p.Start <= date && p.Stop >= date) ?? false;
        }

        public static bool ContainsTemporaryPrimary<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return !l.GetTemporaryPrimary().IsNullOrEmpty();
        }

        public static ActionResult ValidateTemporaryPrimaryEmployment<T>(this List<T> l, DateTime? dateFrom, DateTime? dateTo, bool isSecondary) where T : IEmployment
        {
            ActionResultSave? errorCode = null;

            if (!dateFrom.HasValue || !dateTo.HasValue)
                errorCode = ActionResultSave.TemporaryPrimaryEmploymentMustHaveDateFromAndDateTo;
            else if (!l.HasEmploymentAllDays(dateFrom.Value, dateTo.Value))
                errorCode = ActionResultSave.TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval;
            else if (l.HasTemporaryEmploymentAnyDay(dateFrom.Value, dateTo.Value))
                errorCode = ActionResultSave.TemporaryPrimaryAlreadyExistsInInterval;
            else if (isSecondary)
                errorCode = ActionResultSave.TemporaryPrimaryCannotBeSecondary;
            return errorCode.HasValue ? new ActionResult(false, (int)errorCode.Value, "") : new ActionResult(true);
        }

        #endregion

        #region FinalSalary

        public static List<T> GetFinalSalaryEmployments<T>(this IEnumerable<T> l, DateTime dateFrom, DateTime dateTo) where T : IEmployment
        {
            return l.Where(i => i.HasAppliedFinalSalaryOrManually()).Where(e => CalendarUtility.IsDatesOverlapping(e.GetDateFromOrMin(), e.GetDateToOrMax(), dateFrom, dateTo, validateDatesAreTouching: true)).FilterState().SortByDateAndTemporaryPrimary().ToEmploymentList();
        }

        public static T GetApplyFinalSalaryEmployment<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l.Where(i => i.DateTo.HasValue && i.FinalSalaryStatusId == (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary && !i.IsSecondaryEmployment).OrderByDescending(i => i.DateTo).FirstOrDefault();
        }

        public static T GetAppliedFinalSalaryEmployment<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l.Where(i => i.DateTo.HasValue && i.HasAppliedFinalSalary()).OrderByDescending(i => i.DateTo).FirstOrDefault();
        }

        public static DateTime? GetApplyFinalSalaryEndDate<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l?.GetApplyFinalSalaryEmployment()?.DateTo;
        }

        public static DateTime? GetAppliedFinalSalaryEndDate<T>(this IEnumerable<T> l) where T : IEmployment
        {
            return l?.GetAppliedFinalSalaryEmployment()?.DateTo;
        }

        public static bool HasAppliedFinalSalary(this IEmployment e)
        {
            return e?.FinalSalaryStatusId == (int)SoeEmploymentFinalSalaryStatus.AppliedFinalSalary;
        }

        public static bool HasAppliedFinalSalaryOrManually(this IEmployment e)
        {
            return e?.FinalSalaryStatusId == (int)SoeEmploymentFinalSalaryStatus.AppliedFinalSalary || e?.FinalSalaryStatusId == (int)SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually;
        }

        public static bool HasApplyFinalSalaryOrManually(this IEmployment e)
        {
            return e?.FinalSalaryStatusId == (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary || e?.FinalSalaryStatusId == (int)SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually;
        }

        public static bool HasChangedFromAppliedManually(this IEmployment e, SoeEmploymentFinalSalaryStatus newStatus)
        {
            return e.FinalSalaryStatusId == (int)SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually && newStatus != SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually;
        }

        #endregion

        #region Apply

        public static List<T> ToEmploymentList<T>(this IEnumerable<T> l) where T : IEmployment
        {
            var list = l?.ToList();
            if (list != null)
                list.SetHibernatingPeriods();
            return list;
        }

        public static void SetHibernatingPeriods<T>(this IEnumerable<T> l) where T : IEmployment
        {
            if (l != null)
                l.ToList().ForEach(e => e.SetHibernatingPeriods(l));
        }

        private static void ApplyEmployment<T>(this IEmployment e, IEnumerable<T> l, DateTime? date, List<EmployeeGroupDTO> employeeGroups = null, List<PayrollGroupDTO> payrollGroups = null, List < EmploymentTypeDTO> employmentTypes = null, Dictionary<int, string> employmentEndReasonsDict = null, List<AnnualLeaveGroupDTO> annualLeaveGroups = null) where T : IEmployment
        {
            if (e == null)
                return;

            e.SetHibernatingPeriods(l);
            if (e is EmploymentDTO && date.HasValue)
                (e as EmploymentDTO).ApplyEmploymentChanges(date.Value, employeeGroups, payrollGroups, annualLeaveGroups, employmentTypes, employmentEndReasonsDict);
        }

        #endregion

        #endregion

        #region IModifiedWithNoCheckes

        public static void SetModifiedWithNoCheckes(this IModifiedWithNoCheckes e)
        {
            if (e != null)
                e.ModifiedWithNoCheckes = true;
        }

        #endregion

        #region IScheduleBlockObject

        public static bool ContainsAccount<T>(this List<T> l, DateTime date, List<int> accountIds, List<AccountDTO> allAccounts, out bool allContainsAccount, ref bool hasSchedule) where T : IScheduleBlockAccounting
        {
            List<bool> results = new List<bool>();

            var templateBlocksOnDate = l?.Where(t => t.Date == date).ToList();
            if (!templateBlocksOnDate.IsNullOrEmpty())
            {
                hasSchedule = true;

                if (templateBlocksOnDate.All(tb => !tb.AccountId.HasValue))
                {
                    results.Add(true);
                }
                else
                {
                    foreach (var templateBlocksByDateAndAccount in templateBlocksOnDate.Where(t => t.AccountId.HasValue).GroupBy(t => t.AccountId.Value))
                    {
                        bool result = false;

                        try
                        {
                            AccountDTO account = allAccounts?.FirstOrDefault(i => i.AccountId == templateBlocksByDateAndAccount.Key);
                            if (account == null)
                                continue;

                            if (account.ParentAccounts == null)
                                account.ParentAccounts = account.GetParentAccounts(allAccounts);

                            List<int> templateBlockAccountIds = account.ParentAccounts.Select(a => a.AccountId).ToList();
                            if (!templateBlockAccountIds.Contains(account.AccountId))
                                templateBlockAccountIds.Add(account.AccountId);
                            if (templateBlockAccountIds.Intersect(accountIds).Any())
                                result = true;
                        }
                        finally
                        {
                            results.Add(result);
                        }
                    }
                }
            }

            allContainsAccount = results.All(b => b);
            return results.Any(b => b);
        }

        #endregion

        #region IPayrollTransaction

        public static bool IsExcludedInTime(this IPayrollTransaction e)
        {
            //Also, see IsExcludedInTime extension on TimeTransactionItem

            if (e.IsAdded)
                return true;
            if (e.IsFixed)
                return true;
            if (e.IsCentRounding)
                return true;
            if (e.IsQuantityRounding)
                return true;
            if (e.EmployeeVehicleId.HasValue)
                return true;
            if (e.UnionFeeId.HasValue)
                return true;
            if (e.IsVacationCompensation())
                return true;
            if (e.IsTaxAndNotOptional())
                return true;
            if (e.IsEmploymentTaxCredit())
                return true;
            if (e.IsEmploymentTaxDebit())
                return true;
            if (e.IsSupplementCharge())
                return true;
            if (e.IsDeductionSalaryDistress())
                return true;
            if (e.IsBenefit())
                return true;
            if (e.IsNetSalary())
                return true;
            return false;
        }

        public static bool IsExcludedInPayroll(this IPayrollTransaction e)
        {
            if (!e.PayrollProductUseInPayroll)
                return true;
            return false;
        }

        public static bool IsExcludedInRecalculateAccounting(this IPayrollTransaction e)
        {
            if (e.IsAdded)
                return true;
            if (e.IsFixed)
                return true;
            if (e.UnionFeeId.HasValue)
                return true;
            if (e.EmployeeVehicleId.HasValue)
                return true;
            if (e.IsVacationAdditionOrSalaryPrepaymentInvert())
                return true;
            if (e.IsVacationAdditionOrSalaryVariablePrepaymentInvert())
                return true;
            if (e.IsSupplementCharge())
                return true;
            if (e.IsEmploymentTax())
                return true;
            if (e.IsTaxAndNotOptional())
                return true;
            if (e.IsNetSalary())
                return true;

            return false;
        }

        public static bool HasAttestTransitionPermission(this IPayrollTransaction e, List<AttestTransitionDTO> attestTransitions, int attestStateToId)
        {
            if (attestStateToId == 0)
                return false;
            return attestTransitions.Any(t => t.AttestStateFrom.AttestStateId == e.AttestStateId && t.AttestStateTo.AttestStateId == attestStateToId);
        }

        #endregion

        #region IPayrollTransactionAccounting

        public static bool AnyContainsAccount<T>(this List<T> l, DateTime date, List<int> accountIds, out bool allContainsAccount, ref bool hasTransactions) where T : IPayrollTransactionAccounting
        {
            List<bool> results = new List<bool>();

            var transactionsOnDate = l?.Where(i => i.Date == date).ToList() ?? new List<T>();
            if (transactionsOnDate.Any())
            {
                hasTransactions = true;
                foreach (var e in transactionsOnDate)
                {
                    results.Add(accountIds.Intersect(e.AccountInternalIds).Any());
                }
            }

            allContainsAccount = results.All(b => b);
            return results.Any(b => b);
        }

        #endregion

        #region IPayrollType

        public static bool IsNull(this IPayrollType e)
        {
            return PayrollRulesUtil.Isnull(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsence(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsence(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceSick(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceSick(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceSickDayQualifyingDay(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceSickDayQualifyingDay(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceSickDay2_14(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceSickDay2_14(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceSickDay15(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceSickDay15(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsence_SicknessSalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsence_SicknessSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsence_SicknessSalary_Day2_14(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsence_SicknessSalary_Day2_14(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsence_SicknessSalary_Deduction(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsence_SicknessSalary_Deduction(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsence_SicknessSalary_Day15(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsence_SicknessSalary_Day15(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceWorkInjury(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceWorkInjury(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceCost(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsencePayedAbsence(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4) ||
                   PayrollRulesUtil.IsVacationCost(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4) ||
                   PayrollRulesUtil.IsVacationAddition(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4) ||
                   PayrollRulesUtil.IsAbsence_SicknessSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4) ||
                   PayrollRulesUtil.IsGrossSalaryLayOffSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4) ||
                   PayrollRulesUtil.IsAbsencePermission(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);


        }

        public static bool IsAbsencePayedAbsence(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsencePayedAbsence(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsencePermission(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsencePermission(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceSickOrWorkInjury(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceSickOrWorkInjury(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsencePayrollExport(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsencePayrollExport(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceVacation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceVacation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceVacationNoVacationDaysDeducted(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceVacationNoVacationDaysDeducted(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsQualifyingDeduction(this IPayrollType e)
        {
            return PayrollRulesUtil.IsQualifyingDeduction(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTimeAccumulatorMinusTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTimeAccumulatorMinusTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAddition(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAddition(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionVariable(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionVariable(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionVariablePaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionVariablePaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionVariableAdvance(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionVariableAdvance(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSalaryPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSalaryPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCost(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCost(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceVacationAll(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceVacationAll(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationUnPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationUnPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdvance(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdvance(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSavedYear1(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSavedYear1(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSavedYear2(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSavedYear2(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSavedYear3(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSavedYear3(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSavedYear4(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSavedYear4(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSavedYear5(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSavedYear5(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationSavedOverdue(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationSavedOverdue(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionOrSalaryPrepayment(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionOrSalaryPrepayment(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionOrSalaryPrepaymentPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionOrSalaryPrepaymentPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionOrSalaryPrepaymentInvert(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionOrSalaryPrepaymentInvert(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionOrSalaryVariablePrepayment(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionOrSalaryVariablePrepayment(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionOrSalaryVariablePrepaymentPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionOrSalaryVariablePrepaymentPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationAdditionOrSalaryVariablePrepaymentInvert(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationAdditionOrSalaryVariablePrepaymentInvert(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsWorkTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsWorkTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsValidAbsenceAsWorkTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsValidAbsenceAsWorkTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsInvalidAbsenceAsWorkTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsInvalidAbsenceAsWorkTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryTimeHourMonthly(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryTimeHourMonthly(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDutySalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDutySalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsWeekendSalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsWeekendSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOverTimeAddition(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOverTimeAddition(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAddedOrOverTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddedOrOverTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAddedTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddedTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAddedTimeCompensation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddedTimeCompensation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsAddedTimeCompensation35(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddedTimeCompensation35(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsAddedTimeCompensation70(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddedTimeCompensation70(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsAddedTimeCompensation100(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddedTimeCompensation100(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsAddedTimeAddition(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddedTimeAddition(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAddition(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAddition(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDeduction(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDeduction(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDeductionCarBenefit(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDeductionCarBenefit(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDeductionHouseKeeping(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDeductionHouseKeeping(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDeductionOther(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDeductionOther(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDeductionSalaryDistress(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDeductionSalaryDistress(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_Rental(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_Rental(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_CarCompensation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_CarCompensation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_CarCompensation_BenefitCar(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_CarCompensation_BenefitCar(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_CarCompensation_PrivateCar(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_CarCompensation_PrivateCar(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_Other(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_Other(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_Other_Taxfree(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_Other_Taxfree(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsCompensation_Other_Taxable(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_Other_Taxable(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_Representation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_Representation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_SportsActivity(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_SportsActivity(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_TravelAllowance(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_TravelAllowance(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_TravelAllowance_DomesticShortTerm(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_TravelAllowance_DomesticShortTerm(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_TravelAllowance_ForeignShortTerm(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_TravelAllowance_ForeignShortTerm(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_TravelAllowance_DomesticLongTerm(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_TravelAllowance_DomesticLongTerm(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_TravelAllowance_DomesticLongTermOrOverTwoYears(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_TravelAllowance_DomesticLongTermOrOverTwoYears(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_TravelAllowance_ForeignLongTermOrOverTwoYears(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_TravelAllowance_ForeignLongTermOrOverTwoYears(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_TravelCost(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_TravelCost(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_Accomodation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_Accomodation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCompensation_Vat(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompensation_Vat(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsPersonellAcquisitionOptions(this IPayrollType e)
        {
            return PayrollRulesUtil.IsPersonellAcquisitionOptions(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDuty(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDuty(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition40(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition40(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition50(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition50(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition57(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition57(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition70(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition70(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition79(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition79(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition100(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition100(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOBAddition113(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOBAddition113(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryDuty(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryDuty(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsContract(this IPayrollType e)
        {
            return PayrollRulesUtil.IsContract(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsDutyAndBenefitNotInvert(this IPayrollType e)
        {
            return PayrollRulesUtil.IsDutyAndBenefitNotInvert(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTimeScheduledTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTimeScheduledTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTimeAccumulatorTimeOrAddedTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTimeAccumulatorTimeOrAddedTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTimeAccumulatorAddedTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTimeAccumulatorAddedTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTax(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTax(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOptionalTax(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOptionalTax(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsSINKTax(this IPayrollType e)
        {
            return PayrollRulesUtil.IsSINKTax(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsASINKTax(this IPayrollType e)
        {
            return PayrollRulesUtil.IsASINKTax(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTaxAndNotOptional(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTaxAndNotOptional(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsEmploymentTax(this IPayrollType e)
        {
            return PayrollRulesUtil.IsEmploymentTax(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsEmploymentTaxDebit(this IPayrollType e)
        {
            return PayrollRulesUtil.IsEmploymentTaxDebit(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsEmploymentTaxCredit(this IPayrollType e)
        {
            return PayrollRulesUtil.IsEmploymentTaxCredit(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsEmploymentTaxCreditTo37(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsEmploymentTaxCreditTo37(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsEmploymentTaxCreditEarlyPension(this IPayrollType e, int year, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsEmploymentTaxCreditEarlyPension(year, e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsEmploymentTaxCredit51To90(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsEmploymentTaxCredit51To90(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsEmploymentTaxCreditFrom89(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsEmploymentTaxCreditFrom89(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsEmploymentTaxCredit91ToNow(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsEmploymentTaxCredit91ToNow(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsSupplementChargeDebit(this IPayrollType e)
        {
            return PayrollRulesUtil.IsSupplementChargeDebit(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsSupplementChargeCredit(this IPayrollType e)
        {
            return PayrollRulesUtil.IsSupplementChargeCredit(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsSupplementCharge(this IPayrollType e)
        {
            return PayrollRulesUtil.IsSupplementCharge(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsParentalLeave(this IPayrollType e)
        {
            return PayrollRulesUtil.IsParentalLeave(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceTemporaryParentalLeave(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceTemporaryParentalLeave(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceParentalLeaveOrTemporaryParentalLeave(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceParentalLeaveOrTemporaryParentalLeave(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceMilitaryService(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceMilitaryService(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceMilitaryServiceTotal(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceMilitaryServiceTotal(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsLeaveOfAbsence(this IPayrollType e)
        {
            return PayrollRulesUtil.IsLeaveOfAbsence(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsLeaveOfAbsenceOrParentalLeaveOrTemporaryParentalLeave(this IPayrollType e)
        {
            return PayrollRulesUtil.IsLeaveOfAbsenceOrParentalLeaveOrTemporaryParentalLeave(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsNetSalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsNetSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsNetSalaryPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsNetSalaryPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsNetSalaryRounded(this IPayrollType e)
        {
            return PayrollRulesUtil.IsNetSalaryRounded(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsWork(this IPayrollType e)
        {
            return PayrollRulesUtil.IsWork(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsWorkForHourlyPay(this IPayrollType e)
        {
            return PayrollRulesUtil.IsWorkForHourlyPay(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsWorkHourlySalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsWorkHourlySalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryStandby(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryStandby(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryCarAllowanceFlat(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryCarAllowanceFlat(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryAllowanceStandard(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryAllowanceStandard(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryLayOffSalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryLayOffSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryRetroactive(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryRetroactive(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryTravelTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryTravelTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryCommision(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryCommision(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryWeekendSalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryWeekendSalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryEarnedHolidayPayment(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryEarnedHolidayPayment(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsGrossSalaryEarlyPension(this IPayrollType e, int year, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsGrossSalaryEarlyPension(year, e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsGrossSalaryFrom89(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsGrossSalaryFrom89(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsGrossSalaryYouth(this IPayrollType e, int year, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsGrossSalaryYouth(year, e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsGrossSalaryTo37(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsGrossSalaryTo37(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsGrossSalaryTimeWorkReduction(this IPayrollType e)
        {
            return PayrollRulesUtil.IsGrossSalaryTimeWorkReduction(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvert(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvert(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertWithLevel3NotNull(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertWithLevel3NotNull(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOvertimeCompensation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeCompensation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsOvertimeCompensation35(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeCompensation35(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsOvertimeCompensation50(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeCompensation50(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOvertimeCompensation70(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeCompensation70(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOvertimeCompensation100(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeCompensation100(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOvertimeAddition(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeAddition(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsOvertimeAddition35(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeAddition35(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsOvertimeAddition50(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeAddition50(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOvertimeAddition70(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeAddition70(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsOvertimeAddition100(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOvertimeAddition100(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitAndNotInvert38To50(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsBenefitAndNotInvert38To50(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsBenefitAndNotInvert38To52(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsBenefitAndNotInvert38To52(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsBenefitAndNotInvertFrom89(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsBenefitAndNotInvertFrom89(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsBenefitAndNotInvertFrom91(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsBenefitAndNotInvertFrom91(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsBenefitAndNotInvertTo37(this IPayrollType e, DateTime? birthDate = null)
        {
            return PayrollRulesUtil.IsBenefitAndNotInvertTo37(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4, birthDate);
        }

        public static bool IsOccupationalPension(this IPayrollType e)
        {
            return PayrollRulesUtil.IsOccupationalPension(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBoardRemuneration(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBoardRemuneration(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsRoleSupplement(this IPayrollType e)
        {
            return PayrollRulesUtil.IsRoleSupplement(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsActivitySupplement(this IPayrollType e)
        {
            return PayrollRulesUtil.IsActivitySupplement(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsCompetenceSupplement(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCompetenceSupplement(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsResponsibilitySupplement(this IPayrollType e)
        {
            return PayrollRulesUtil.IsResponsibilitySupplement(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsBenefitOther(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitOther(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitParking(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitParking(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitPropertyHouse(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitPropertyHouse(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitPropertyNotHouse(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitPropertyNotHouse(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitROT(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitROT(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitRUT(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitRUT(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitBorrowedComputer(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitBorrowedComputer(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitCompanyCar(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitCompanyCar(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitFood(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitFood(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitFuel(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitFuel(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInterest(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInterest(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertOther(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertOther(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertPropertyNotHouse(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertPropertyNotHouse(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertPropertyHouse(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertPropertyHouse(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertFuel(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertFuel(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertROT(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertROT(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertRUT(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertRUT(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertFood(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertFood(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertBorrowedComputer(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertBorrowedComputer(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertParking(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertParking(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertInterest(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertInterest(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertCompanyCar(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertCompanyCar(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitInvertStandard(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitInvertStandard(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefitAndNotInvert(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefitAndNotInvert(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Other(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Other(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Not_CompanyCar_And_FuelBenefit(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Not_CompanyCar_And_FuelBenefit(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Fuel_PartNotAnnualized(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Fuel_PartNotAnnualized(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Fuel_PartAnnualized(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Fuel_PartAnnualized(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_CompanyCar(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_CompanyCar(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_PropertyNotHouse(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_PropertyNotHouse(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_PropertyHouse(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_PropertyHouse(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Fuel(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Fuel(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Parking(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Parking(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Food(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Food(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_BorrowedComputer(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_BorrowedComputer(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_ROT(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_ROT(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_RUT(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_RUT(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Interest(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBenefit_Interest(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCostDeduction(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCostDeduction(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsSalaryDistress(this IPayrollType e)
        {
            return PayrollRulesUtil.IsSalaryDistress(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsUnionFee(this IPayrollType e)
        {
            return PayrollRulesUtil.IsUnionFee(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsEmployeeVehicleTransaction(this PayrollCalculationProductDTO e)
        {
            return (PayrollRulesUtil.IsEmployeeVehicleTransaction(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4));
        }

        public static bool IsCollectumArslon(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCollectumArslon(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsCollectumLonevaxling(this IPayrollType e)
        {
            return PayrollRulesUtil.IsCollectumLonevaxling(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsAbsenceNoVacation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsAbsenceNoVacation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensation(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensation(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationEarned(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationEarned(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationDirectPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationDirectPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationSavedOverdueVariable(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationSavedOverdueVariable(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationPaid(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationPaid(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationAdvance(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationAdvance(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationSavedYear1(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationSavedYear1(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationSavedYear2(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationSavedYear2(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationSavedYear3(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationSavedYear3(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationSavedYear4(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationSavedYear4(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationSavedYear5(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationSavedYear5(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsVacationCompensationSavedOverdue(this IPayrollType e)
        {
            return PayrollRulesUtil.IsVacationCompensationSavedOverdue(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTimeAccumulator(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTimeAccumulator(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTimeAccumulatorNegate(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTimeAccumulatorNegate(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTimeAccumulatorOverTime(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTimeAccumulatorOverTime(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsTaxBasis(this IPayrollType e)
        {
            return PayrollRulesUtil.IsTaxBasis(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsEmploymentTaxBasis(this IPayrollType e)
        {
            return PayrollRulesUtil.IsEmploymentTaxBasis(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsSupplementChargeBasis(this IPayrollType e)
        {
            return PayrollRulesUtil.IsSupplementChargeBasis(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsHourlySalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsHourlySalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsMonthlySalary(this IPayrollType e)
        {
            return PayrollRulesUtil.IsMonthlySalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }
        public static bool IsBygglosenPaidoutExcess(this IPayrollType e)
        {
            return PayrollRulesUtil.IsBygglosenPaidoutExcess(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);
        }

        public static bool IsQuantity(this IPayrollType e, TermGroup_PayrollResultType resultType)
        {

            if (resultType == TermGroup_PayrollResultType.Quantity ||
                e.IsTax() ||
                e.IsEmploymentTaxCredit() ||
                e.IsEmploymentTaxDebit() ||
                e.IsSupplementCharge() ||
                e.IsNetSalary() ||
                e.IsVacationCompensationDirectPaid() ||
                e.IsVacationAdditionOrSalaryPrepaymentInvert() ||
                e.IsVacationAdditionOrSalaryPrepaymentPaid() ||
                e.IsVacationAdditionOrSalaryVariablePrepaymentPaid() ||
                e.IsBenefitInvert() ||
                e.IsVacationCompensationAdvance() ||
                e.IsWeekendSalary())
            {
                return true;
            }

            return false;
        }


        #endregion

        #region IState

        public static void SetState(this IState e, int state)
        {
            if (e != null)
                e.State = state;
        }

        #endregion

        #region ITask

        public static void SetCreatedByTask(this ITask e, long taskId)
        {
            if (e != null && !e.CreatedByTask.HasValue)
                e.CreatedByTask = taskId;
        }
        public static void SetModifiedByTask(this ITask e, long taskId)
        {
            if (e != null && e.CreatedByTask != taskId)
                e.ModifiedByTask = taskId;
        }

        #endregion

        #region IUserCompanyRole

        public static int GetDefaultRoleId<T>(this List<T> l, int actorCompanyId, DateTime? date = null) where T : IUserCompanyRole
        {
            if (l.IsNullOrEmpty() || actorCompanyId <= 0)
                return 0;

            if (!date.HasValue)
                date = DateTime.Today;

            return l.GetRoles(actorCompanyId, date).ChooseDefaultRoleId();
        }

        public static List<T> GetRoles<T>(this List<T> l, int actorCompanyId, DateTime? date = null) where T : IUserCompanyRole
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return l.Where(i =>
                i.ActorCompanyId == actorCompanyId &&
                (i.DateFrom ?? DateTime.MinValue) <= date.Value &&
                (i.DateTo ?? DateTime.MaxValue) >= date &&
                i.StateId == (int)SoeEntityState.Active
                ).ToList();
        }

        public static int ChooseDefaultRoleId<T>(this List<T> l) where T : IUserCompanyRole
        {
            if (l.IsNullOrEmpty())
                return 0; //has no roles

            var defaultRoles = l.GetDefaults();
            if (defaultRoles.IsNullOrEmpty())
                return l.OrderBy(e => e.GetDateFrom()).First().RoleId; //has no defaults, chhose first non-default

            if (defaultRoles.Count == 1)
                return defaultRoles.First().RoleId; //has only one default

            return defaultRoles.OrderBy(e => e.GetDays()).FirstOrDefault().RoleId; //choose default with least days
        }

        public static List<T> GetDefaults<T>(this List<T> l) where T : IUserCompanyRole
        {
            return l?.Where(e => e.Default).ToList() ?? new List<T>();
        }

        public static int GetDays<T>(this T e) where T : IUserCompanyRole
        {
            return (int)e.GetDateTo().Subtract(e.GetDateFrom()).TotalDays + 1;
        }

        public static DateTime GetDateFrom<T>(this T e) where T : IUserCompanyRole
        {
            return e?.DateFrom ?? DateTime.MinValue;
        }

        public static DateTime GetDateTo<T>(this T e) where T : IUserCompanyRole
        {
            return e?.DateTo ?? DateTime.MaxValue;
        }

        #endregion

        #endregion

        #region POCO

        #region AccountHierarchyParams

        public static bool GetValue(this AccountHierarchyInput input, AccountHierarchyParamType paramType)
        {
            if (input == null || input.ParamValues == null || !input.ParamValues.ContainsKey(paramType))
                return paramType.GetDefaultValue();

            return input.ParamValues[paramType];
        }

        public static bool GetDefaultValue(this AccountHierarchyParamType paramType)
        {
            switch (paramType)
            {
                case AccountHierarchyParamType.OnlyDefaultAccounts:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region ActivateScheduleControlResultDTO

        public static Dictionary<int, bool> GetEmployeeRequestIdsToReCreate(this ActivateScheduleControlDTO e, int employeeId)
        {
            if (e == null || e.ResultHeads.IsNullOrEmpty())
                return new Dictionary<int, bool>();

            return e.ResultHeads
                .Where(i =>
                    i.Type == TermGroup_ControlEmployeeSchedulePlacementType.ShortenHasAbsenceRequest &&
                    i.EmployeeRequestId.HasValue &&
                    i.EmployeeId == employeeId)
                .ToDictionary(x => x.EmployeeRequestId.Value, x => x.ReActivateAbsenceRequest);
        }

        #endregion

        #region ActiveScheduleControlRowDTO

        public static List<ActivateScheduleControlRowDTO> Filter(this List<ActivateScheduleControlRowDTO> l, DateTime dateFrom, DateTime dateTo)
        {
            return l?.Where(r => CalendarUtility.IsDateInRange(r.Date, dateFrom, dateTo)).ToList() ?? new List<ActivateScheduleControlRowDTO>();
        }
        public static List<ActivateScheduleControlRowDTO> Filter(this List<ActivateScheduleControlRowDTO> l, TermGroup_ControlEmployeeSchedulePlacementType type)
        {
            return l?.Where(t => t.Type == type).ToList() ?? new List<ActivateScheduleControlRowDTO>();
        }
        public static bool ContainsRow(this List<ActivateScheduleControlRowDTO> l, DateTime date, params TermGroup_ControlEmployeeSchedulePlacementType[] types)
        {
            return l?.Any(r => r.Date == date && types.Any(t => t == r.Type)) ?? false;
        }

        #endregion

        #region AgGridColumnSettingDTO

        public static AgGridColumnSettingDTO Get(this List<AgGridColumnSettingDTO> l, string colId)
        {
            return l?.FirstOrDefault(e => e.colId == colId);
        }

        #endregion

        #region AttestEmployeeDaySmallDTO

        public static List<int> GetEmployeeIds(this List<AttestEmployeeDaySmallDTO> l)
        {
            return l?.Select(e => e.EmployeeId).Distinct().ToList() ?? new List<int>();
        }
        public static int GetNrOfEmployees(this List<AttestEmployeeDaySmallDTO> l)
        {
            return l?.Select(e => e.EmployeeId).Distinct().Count() ?? 0;
        }
        public static int GetNrOfDates(this List<AttestEmployeeDaySmallDTO> l)
        {
            return l?.Select(i => i.Date).Distinct().Count() ?? 0;
        }
        public static string GetDateInterval(this List<AttestEmployeeDaySmallDTO> l)
        {
            if (l.IsNullOrEmpty())
                return string.Empty;
            if (l.Count == 1)
                return l.First().Date.ToShortDateString();
            return $"{l.Min(e => e.Date).ToShortDateString()} - {l.Max(e => e.Date).ToShortDateString()}";
        }

        #endregion

        #region AttestDTO

        #region TimeEmployeeTreeDTO

        public static List<TimeEmployeeTreeNodeDTO> GetEmployeeNodes(this TimeEmployeeTreeDTO e)
        {
            var employeeNodes = new List<TimeEmployeeTreeNodeDTO>();

            var allEmployeeNodes = e.GroupNodes?.SelectMany(groupNode => groupNode.GetEmployeeNodes()).ToList() ?? new List<TimeEmployeeTreeNodeDTO>();
            foreach (var employeeNodesByEmployee in allEmployeeNodes.GroupBy(i => i.EmployeeId))
            {
                var employeeNode = employeeNodesByEmployee.ToList().GetEmployeeNodeForEmployeeByPrio(employeeNodesByEmployee.Key);
                if (employeeNode != null)
                    employeeNodes.Add(employeeNode);
            }

            return employeeNodes;
        }

        public static TimeEmployeeTreeNodeDTO GetEmployeeNodeForEmployeeByPrio(this List<TimeEmployeeTreeNodeDTO> employeeNodes, int employeeId)
        {
            employeeNodes = employeeNodes.Where(i => i.EmployeeId == employeeId).ToList();

            TimeEmployeeTreeNodeDTO employeeNode = employeeNodes.FirstOrDefault(i => !i.IsAdditional);
            if (employeeNode == null)
                employeeNode = employeeNodes.OrderBy(i => i.EmployeeName).FirstOrDefault();

            return employeeNode;
        }

        public static List<AttestStateDTO> GetAttestStates(this TimeEmployeeTreeDTO e)
        {
            var attestStates = new List<AttestStateDTO>();
            if (e.GroupNodes != null)
            {
                foreach (var group in e.GroupNodes)
                {
                    attestStates.AddRange(group.GetAttestStates().Exclude(attestStates));
                }
            }
            return attestStates;
        }

        public static List<TimeEmployeeTreeGroupNodeDTO> SortGroups(this List<TimeEmployeeTreeGroupNodeDTO> l, TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting, bool? additional = null)
        {
            var sortedGroups = new List<TimeEmployeeTreeGroupNodeDTO>();

            foreach (var e in l.Filter(additional).SortAlphanumeric(grouping, sorting))
            {
                if (!sortedGroups.Any(i => i.Id == e.Id))
                    sortedGroups.Add(e);

                e.ChildGroupNodes.SortGroups(grouping, sorting, additional);
            }

            return sortedGroups;
        }

        public static void Sort(this TimeEmployeeTreeDTO e)
        {
            var sortedGroups = new List<TimeEmployeeTreeGroupNodeDTO>();
            if (e.GroupNodes != null)
            {
                sortedGroups.AddRange(e.GroupNodes.SortGroups(e.Grouping, e.Sorting, false));
                sortedGroups.AddRange(e.GroupNodes.SortGroups(e.Grouping, e.Sorting, true));
                e.GroupNodes.Clear();
                e.GroupNodes.AddRange(sortedGroups);
            }
            else
            {
                e.GroupNodes = new List<TimeEmployeeTreeGroupNodeDTO>();
            }
        }

        #endregion

        #region ITimeTreeNode

        public static List<TimeTreeEmployeeWarning> GetWarnings(this IEnumerable<ITimeTreeNode> nodes)
        {
            return nodes?.SelectMany(e => e.Warnings).Distinct().ToList() ?? new List<TimeTreeEmployeeWarning>();
        }

        public static IEnumerable<TimeTreeEmployeeWarning> GetWarnings(this ITimeTreeNode node, SoeTimeAttestWarningGroup group, bool isStopping)
        {
            return node?.Warnings.Where(w => w.WarningGroup == group && w.IsStopping == isStopping);
        }

        public static List<AttestStateDTO> GetAttestStates(this IEnumerable<ITimeTreeNode> nodes)
        {
            var attestStates = new List<AttestStateDTO>();
            foreach (var node in nodes.Where(i => i.AttestStates != null))
            {
                foreach (var attestState in node.AttestStates)
                {
                    if (!attestStates.Exists(i => i.AttestStateId == attestState.AttestStateId))
                        attestStates.Add(attestState);
                }
            }
            return attestStates;
        }

        public static void AddAttestStates(this ITimeTreeNode e, List<AttestStateDTO> attestStates)
        {
            if (e == null)
                return;

            if (attestStates != null && !e.AttestStates.IsNullOrEmpty())
                e.SetAttestStates(attestStates.Concat(e.AttestStates));
            else
                e.SetAttestStates(attestStates);
        }

        public static void SetAttestStates(this ITimeTreeNode e, IEnumerable<AttestStateDTO> attestStates)
        {
            if (e == null)
                return;

            e.SetLowestAttestState(attestStates);
            e.AttestStates = attestStates?.ToList();
            e.WarningMessageTime = string.Empty; //loaded separate
        }

        public static void FormatWarnings(this ITimeTreeNode e)
        {
            if (e?.Warnings == null)
                return;

            e.Warnings = e.Warnings.Distinct().ToList();
            e.WarningMessageTime = e.GetWarnings(SoeTimeAttestWarningGroup.Time, false).GetWarningMessage();
            e.WarningMessagePayroll = e.GetWarnings(SoeTimeAttestWarningGroup.Payroll, false).GetWarningMessage();
            e.WarningMessagePayrollStopping = e.GetWarnings(SoeTimeAttestWarningGroup.Payroll, true).GetWarningMessage();
        }

        #endregion

        #region TimeEmployeeTreeGroupDTO

        public static List<TimeEmployeeTreeNodeDTO> GetEmployeeNodes(this TimeEmployeeTreeGroupNodeDTO e)
        {
            var employeeNodes = new List<TimeEmployeeTreeNodeDTO>();
            if (e.EmployeeNodes != null)
            {
                foreach (var employeeNode in e.EmployeeNodes)
                {
                    if (!employeeNodes.Any(i => i.EmployeeId == employeeNode.EmployeeId))
                        employeeNodes.Add(employeeNode);
                }
            }
            if (e.ChildGroupNodes != null)
            {
                foreach (var childGroupNode in e.ChildGroupNodes)
                {
                    employeeNodes.AddRange(childGroupNode.GetEmployeeNodes().Exclude(employeeNodes));
                }
            }
            return employeeNodes;
        }

        public static List<AttestStateDTO> GetAttestStates(this TimeEmployeeTreeGroupNodeDTO e)
        {
            List<AttestStateDTO> attestStates = e.AttestStates ?? new List<AttestStateDTO>();

            if (e.ChildGroupNodes != null)
                attestStates.AddRange(e.ChildGroupNodes.GetAttestStates().Exclude(attestStates));

            return attestStates;
        }

        public static List<TimeEmployeeTreeGroupNodeDTO> Filter(this List<TimeEmployeeTreeGroupNodeDTO> items, bool? additional = null)
        {
            return items.Where(i => ((!additional.HasValue) ||
                                     (additional == true && i.IsAdditional) ||
                                     (additional == false && !i.IsAdditional))
                                     ).OrderBy(i => i.Code).ToList();
        }

        public static List<TimeEmployeeTreeGroupNodeDTO> SortAlphanumeric(this List<TimeEmployeeTreeGroupNodeDTO> l, TermGroup_AttestTreeGrouping grouping, TermGroup_AttestTreeSorting sorting)
        {
            var sortedGroups = new List<TimeEmployeeTreeGroupNodeDTO>();
            if (!l.IsNullOrEmpty())
            {
                bool sortByCode = grouping == TermGroup_AttestTreeGrouping.EmployeeAuthModel;
                bool sortByDefinedOrder = grouping == TermGroup_AttestTreeGrouping.AttestState && l.GroupBy(e => e.DefinedSort).Count() > 1;

                string[] sortedArray;
                if (sortByCode)
                    sortedArray = l.Select(i => i.Code).ToArray();
                else if (sortByDefinedOrder)
                    sortedArray = l.Select(i => i.DefinedSort.ToString()).ToArray();
                else
                    sortedArray = l.Select(i => i.Name).ToArray();

                Array.Sort(sortedArray, new AlphanumComparator());
                foreach (string artefact in sortedArray)
                {
                    List<TimeEmployeeTreeGroupNodeDTO> groups;
                    if (sortByCode)
                        groups = l.Where(i => i.Code == artefact).ToList();
                    else if (sortByDefinedOrder && Int32.TryParse(artefact, out int definedSort))
                        groups = l.Where(i => i.DefinedSort == definedSort).ToList();
                    else
                        groups = l.Where(i => i.Name == artefact).ToList();

                    foreach (TimeEmployeeTreeGroupNodeDTO group in groups)
                    {
                        if (sortedGroups.Any(i => i.Id == group.Id))
                            continue;
                        if (sorting != TermGroup_AttestTreeSorting.None)
                            group.SortEmployeesAlphanumeric(sorting);
                        sortedGroups.Add(group);
                    }
                }
            }
            return sortedGroups;
        }

        public static void SortEmployeesAlphanumeric(this TimeEmployeeTreeGroupNodeDTO e, TermGroup_AttestTreeSorting sorting)
        {
            var sortedEmployees = new List<TimeEmployeeTreeNodeDTO>();

            //Regular
            foreach (var employeeNode in e.EmployeeNodes.Filter(false).SortAlphanumeric(sorting))
            {
                if (!sortedEmployees.Exists(i => i.EmployeeId == employeeNode.EmployeeId))
                    sortedEmployees.Add(employeeNode);
            }

            //Additional
            foreach (var employeeNode in e.EmployeeNodes.Filter(true).SortAlphanumeric(sorting))
            {
                if (!sortedEmployees.Exists(i => i.EmployeeId == employeeNode.EmployeeId))
                    sortedEmployees.Add(employeeNode);
            }

            e.EmployeeNodes.Clear();
            e.EmployeeNodes.AddRange(sortedEmployees);
        }

        #endregion

        #region TimeEmployeeTreeNodeDTO

        public static List<TimeEmployeeTreeNodeDTO> Exclude(this List<TimeEmployeeTreeNodeDTO> l, List<TimeEmployeeTreeNodeDTO> exclude)
        {
            List<TimeEmployeeTreeNodeDTO> employeeNodes = new List<TimeEmployeeTreeNodeDTO>();
            foreach (var e in l)
            {
                if (!employeeNodes.Exists(i => i.EmployeeId == e.EmployeeId) &&
                    !exclude.Exists(i => i.EmployeeId == e.EmployeeId))
                    employeeNodes.Add(e);
            }
            return employeeNodes;
        }

        public static List<TimeEmployeeTreeNodeDTO> Filter(this List<TimeEmployeeTreeNodeDTO> l, bool? additional = null)
        {
            return l.Where(i => ((!additional.HasValue) ||
                                (additional == true && i.IsAdditional) ||
                                (additional == false && !i.IsAdditional))
                                ).OrderBy(i => i.EmployeeNr).ToList();
        }

        public static List<TimeEmployeeTreeNodeDTO> SortAlphanumeric(this List<TimeEmployeeTreeNodeDTO> l, TermGroup_AttestTreeSorting sorting)
        {
            var sortedEmployees = new List<TimeEmployeeTreeNodeDTO>();
            if (sorting == TermGroup_AttestTreeSorting.EmployeeNr)
                sortedEmployees = l.SortAlphanumericByEmployeeNr();
            else if (sorting == TermGroup_AttestTreeSorting.FirstName)
                sortedEmployees = l.SortAlphanumericByEmployeeByFirstName();
            else if (sorting == TermGroup_AttestTreeSorting.LastName)
                sortedEmployees = l.SortAlphanumericByEmployeeByLastName();
            return sortedEmployees;
        }

        public static List<TimeEmployeeTreeNodeDTO> SortAlphanumericByEmployeeNr(this List<TimeEmployeeTreeNodeDTO> l)
        {
            var sortedEmployees = new List<TimeEmployeeTreeNodeDTO>();
            if (l.Count > 0)
            {
                var employeeNrs = l.Select(i => i.EmployeeNr).ToArray();
                Array.Sort(employeeNrs, new AlphanumComparator());
                foreach (string employeeNr in employeeNrs)
                {
                    foreach (var employee in l.Where(i => i.EmployeeNr == employeeNr))
                    {
                        sortedEmployees.Add(employee);
                    }
                }
            }
            return sortedEmployees;
        }

        public static List<TimeEmployeeTreeNodeDTO> SortAlphanumericByEmployeeByFirstName(this List<TimeEmployeeTreeNodeDTO> l)
        {
            var sortedEmployees = new List<TimeEmployeeTreeNodeDTO>();
            if (l.Count > 0)
            {
                var employeeFirstNames = l.Select(i => i.EmployeeFirstName).ToArray();
                Array.Sort(employeeFirstNames, new AlphanumComparator());
                foreach (var employeeFirstName in employeeFirstNames)
                {
                    foreach (var employee in l.Where(i => i.EmployeeFirstName == employeeFirstName))
                    {
                        sortedEmployees.Add(employee);
                    }
                }
            }
            return sortedEmployees;
        }

        public static List<TimeEmployeeTreeNodeDTO> SortAlphanumericByEmployeeByLastName(this List<TimeEmployeeTreeNodeDTO> l)
        {
            var sortedEmployees = new List<TimeEmployeeTreeNodeDTO>();
            if (!l.IsNullOrEmpty())
            {
                var employeeLastNames = l.Select(i => i.EmployeeLastName).ToArray();
                Array.Sort(employeeLastNames, new AlphanumComparator());
                foreach (var employeeLastName in employeeLastNames)
                {
                    foreach (var employee in l.Where(i => i.EmployeeLastName == employeeLastName))
                    {
                        sortedEmployees.Add(employee);
                    }
                }
            }
            return sortedEmployees;
        }

        #endregion

        #region TimeTreeEmployeeWarning

        public static string GetWarningMessage(this IEnumerable<TimeTreeEmployeeWarning> l)
        {
            return l?
                .Select(e => e.Message)
                .ToCommaSeparated() ?? string.Empty;
        }

        #endregion

        #region AttestEmployeeDayDTO

        public static List<AttestEmployeeDayDTO> CopyCollection(this IEnumerable<AttestEmployeeDayDTO> items)
        {
            var newItems = new List<AttestEmployeeDayDTO>();
            foreach (var item in items)
            {
                newItems.Add(CopyItem(item, false));
            }

            return newItems;
        }

        public static AttestEmployeeDayDTO CopyItem(this AttestEmployeeDayDTO prototype, bool copyCollections)
        {
            var clone = new AttestEmployeeDayDTO(prototype.EmployeeId, prototype.Date);

            var clonePis = clone.GetType().GetProperties();
            foreach (var clonePi in clonePis)
            {
                PropertyInfo prototypePi = prototype.GetType().GetProperty(clonePi.Name);
                if (prototypePi.CanWrite)
                    clonePi.SetValue(clone, prototypePi.GetValue(prototype, null), null);
            }

            if (copyCollections)
            {
                clone.AttestPayrollTransactions = new List<AttestPayrollTransactionDTO>();
                foreach (var transactionItem in prototype.AttestPayrollTransactions)
                {
                    var transactionClone = new AttestPayrollTransactionDTO();
                    var transactionClonePis = transactionClone.GetType().GetProperties();
                    foreach (var transactionClonePi in transactionClonePis)
                    {
                        PropertyInfo prototypePi = transactionItem.GetType().GetProperty(transactionClonePi.Name);
                        if (prototypePi.CanWrite)
                            transactionClonePi.SetValue(transactionClone, prototypePi.GetValue(transactionItem, null), null);
                    }
                    clone.AttestPayrollTransactions.Add(transactionClone);
                }

                clone.PresenceBreakItems = new List<AttestEmployeeBreakDTO>();
                foreach (var breakItem in prototype.PresenceBreakItems)
                {
                    var breakClone = new AttestEmployeeBreakDTO();
                    var breakClonePis = breakClone.GetType().GetProperties();
                    foreach (var breakClonePi in breakClonePis)
                    {
                        PropertyInfo prototypePi = breakItem.GetType().GetProperty(breakClonePi.Name);
                        if (prototypePi.CanWrite)
                            breakClonePi.SetValue(breakClone, prototypePi.GetValue(breakItem, null), null);
                    }
                    clone.PresenceBreakItems.Add(breakClone);
                }
            }

            return clone;
        }

        public static string GetDateName(this AttestEmployeeDayDTO e)
        {
            string dateName = CalendarUtility.GetDayNameFromCulture(e.Date) + " " + e.Date.ToShortDateString();
            if (!String.IsNullOrEmpty(e.HolidayName))
                dateName += String.Format(" ({0})", e.HolidayName);
            return dateName;
        }

        public static bool HasAttestTransitionPermission(this AttestEmployeeDayDTO e, List<AttestTransitionDTO> attestTransitions, int attestStateToId)
        {
            if (attestStateToId == 0)
                return false;

            foreach (var attestState in e.AttestStates)
            {
                int counter = (from t in attestTransitions
                               where t.AttestStateFrom.AttestStateId == attestState.AttestStateId &&
                               t.AttestStateTo.AttestStateId == attestStateToId
                               select t).Count();

                //Has permission to apply Attest for at least one transaction
                if (counter > 0)
                    return true;
            }

            return false;
        }

        public static bool HasAttestTransitionPermission(this AttestEmployeeDayDTO e, List<AttestTransitionDTO> employeeGroupAttestTransitions, List<AttestUserRoleDTO> userAttestTransitions, int attestStateToId, bool isMySelf)
        {
            if (attestStateToId == 0)
                return false;

            foreach (var attestState in e.AttestStates)
            {
                int counter = 0;

                if (isMySelf)
                {
                    //Validate against AttestTransitions
                    counter = (from t in employeeGroupAttestTransitions
                               where t.AttestStateFrom.AttestStateId == attestState.AttestStateId &&
                               t.AttestStateTo.AttestStateId == attestStateToId
                               select t).Count();
                }
                else
                {
                    //Validate against User's AttestRole's valid AttestTransitions
                    counter = (from t in userAttestTransitions
                               where t.AttestStateFromId == attestState.AttestStateId &&
                               t.AttestStateToId == attestStateToId &&
                               (!t.DateFrom.HasValue || t.DateFrom.Value <= e.Date) &&
                               (!t.DateTo.HasValue || t.DateTo.Value >= e.Date)
                               select t).Count();
                }

                //Has permission to apply Attest for at least one transaction
                if (counter > 0)
                    return true;
            }

            return false;
        }

        public static bool HasAnyPeriodNoAttestStates(this List<AttestEmployeeDayDTO> l, bool autogenTimeBlocks)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.HasPeriodNoAttestStates(autogenTimeBlocks));
        }

        public static bool HasAnyPeriodTimeStampEntriesWithoutTransactions(this List<AttestEmployeeDayDTO> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.HasPeriodTimeStampsWithoutTransactions());
        }

        public static bool HasAnyPeriodChangedScheduleFromTemplate(this List<AttestEmployeeDayDTO> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.HasPeriodChangedScheduleFromTemplate());
        }

        public static bool IsAnyPreliminary(this List<AttestEmployeeDayDTO> l)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.IsPrel);
        }

        public static bool HasAdditionOrDeductionTransactions(this AttestEmployeeDayDTO e)
        {
            return (from t in e.AttestPayrollTransactions
                    where (t.TimeCodeType == SoeTimeCodeType.AdditionDeduction)
                    select t).Any();
        }

        public static bool HasAttestStateNoneInitial(this AttestEmployeeDayDTO e, int initialAttestStateId)
        {
            if (e.AttestPayrollTransactions.IsNullOrEmpty())
                return false;
            return e.AttestPayrollTransactions.Any(i => !i.IsScheduleTransaction && i.AttestStateId != initialAttestStateId && !i.IsReversed);
        }

        public static bool HasPeriodNoAttestStates(this AttestEmployeeDayDTO e, bool autogenTimeBlocks)
        {
            if (e == null)
                return false;
            return !e.IsScheduleZeroDay && !e.AttestStates.Any() && (autogenTimeBlocks || DateTime.Now.Date >= e.Date.Date);
        }

        public static bool HasPeriodTimeStampsWithoutTransactions(this AttestEmployeeDayDTO e)
        {
            if (e == null)
                return false;
            return e.HasTimeStampsWithoutTransactions;
        }

        public static bool HasPeriodChangedScheduleFromTemplate(this AttestEmployeeDayDTO e)
        {
            if (e == null)
                return false;
            return e.IsScheduleChangedFromTemplate;
        }

        public static bool IsItemModified(this AttestEmployeeDayDTO originalItem, AttestEmployeeDayDTO currentItem)
        {
            if (currentItem == null)
                return false;
            if (originalItem.PresenceStartTime.HasValue && currentItem.PresenceStartTime.HasValue && originalItem.PresenceStartTime.Value != currentItem.PresenceStartTime.Value)
                return true;
            if (originalItem.PresenceStopTime.HasValue && currentItem.PresenceStopTime.HasValue && originalItem.PresenceStopTime.Value != currentItem.PresenceStopTime.Value)
                return true;
            if (originalItem.PresenceBreakMinutes.HasValue && currentItem.PresenceBreakMinutes.HasValue && originalItem.PresenceBreakMinutes.Value != currentItem.PresenceBreakMinutes.Value)
                return true;
            return false;
        }

        public static AttestEmployeeDayDTO GetOriginalItem(this AttestEmployeeDayDTO e, List<AttestEmployeeDayDTO> originalItems)
        {
            if (e == null)
                return null;
            return originalItems?.FirstOrDefault(i => i.UniqueId == e.UniqueId);
        }

        public static void SetAttestStateProperties(this AttestEmployeeDayDTO e)
        {
            if (e == null || e.AttestStates.IsNullOrEmpty())
                return;

            e.SetLowestAttestState(e.AttestStates);
        }

        #endregion

        #region AttestEmployeePeriodDTO

        public static bool ContainsEmployee(this List<AttestEmployeePeriodDTO> dtos, int employeeId)
        {
            return dtos?.Any(i => i.EmployeeId == employeeId) ?? false;
        }

        public static List<AttestEmployeePeriodDTO> SortAlphanumeric(this List<AttestEmployeePeriodDTO> l)
        {
            var sortedEmployeeItems = new List<AttestEmployeePeriodDTO>();

            var employeeNrs = l.Select(i => i.EmployeeNr).ToArray();
            Array.Sort(employeeNrs, new AlphanumComparator());

            foreach (var employeeNr in employeeNrs)
            {
                var employeeItem = l.FirstOrDefault(i => i.EmployeeNr == employeeNr);
                if (employeeItem != null && !sortedEmployeeItems.ContainsEmployee(employeeItem.EmployeeId))
                    sortedEmployeeItems.Add(employeeItem);
            }

            return sortedEmployeeItems;
        }

        public static string ToIdString(this List<AttestEmployeePeriodDTO> l)
        {
            return StringUtility.GetCommaSeparatedString<int>(l.Select(i => i.EmployeeId).Distinct().ToList());
        }

        #endregion

        #region PayrollCalculationEmployeePeriodDTO

        public static bool ContainsEmployee(this List<PayrollCalculationEmployeePeriodDTO> l, int employeeId)
        {
            if (l.IsNullOrEmpty())
                return false;
            return l.Any(i => i.EmployeeId == employeeId);
        }

        public static List<PayrollCalculationEmployeePeriodDTO> SortAlphanumeric(this List<PayrollCalculationEmployeePeriodDTO> l)
        {
            var sortedItems = new List<PayrollCalculationEmployeePeriodDTO>();

            var employeeNrs = l.Select(i => i.EmployeeNr).ToArray();
            Array.Sort(employeeNrs, new AlphanumComparator());

            foreach (var employeeNr in employeeNrs)
            {
                var employeeItem = l.FirstOrDefault(i => i.EmployeeNr == employeeNr);
                if (employeeItem == null)
                    continue;

                if (!sortedItems.ContainsEmployee(employeeItem.EmployeeId))
                    sortedItems.Add(employeeItem);
            }

            return sortedItems;
        }

        public static void SetAttestStates(this PayrollCalculationEmployeePeriodDTO e, List<AttestStateDTO> attestStates)
        {
            if (attestStates == null)
                return;

            e.SetLowestAttestState(attestStates);
            e.AttestStates = attestStates;
        }

        #endregion

        #region PayrollCalculationProductDTO

        public static List<PayrollCalculationProductDTO> CopyCollection(this IEnumerable<PayrollCalculationProductDTO> l)
        {
            var newItems = new List<PayrollCalculationProductDTO>();
            foreach (var e in l)
            {
                newItems.Add(e.CopyItem(false));
            }

            return newItems;
        }

        public static PayrollCalculationProductDTO CopyItem(this PayrollCalculationProductDTO prototype, bool copyCollections)
        {
            var clone = new PayrollCalculationProductDTO();

            var clonePis = clone.GetType().GetProperties();
            foreach (var clonePi in clonePis)
            {
                PropertyInfo prototypePi = prototype.GetType().GetProperty(clonePi.Name);
                if (prototypePi.CanWrite)
                    clonePi.SetValue(clone, prototypePi.GetValue(prototype, null), null);
            }

            if (copyCollections)
            {
                clone.AttestPayrollTransactions = new List<AttestPayrollTransactionDTO>();
                foreach (var transactionItem in prototype.AttestPayrollTransactions)
                {
                    var transactionClone = new AttestPayrollTransactionDTO();
                    var transactionClonePis = transactionClone.GetType().GetProperties();
                    foreach (var transactionClonePi in transactionClonePis)
                    {
                        PropertyInfo prototypePi = transactionItem.GetType().GetProperty(transactionClonePi.Name);
                        if (prototypePi.CanWrite)
                            transactionClonePi.SetValue(transactionClone, prototypePi.GetValue(transactionItem, null), null);
                    }
                    clone.AttestPayrollTransactions.Add(transactionClone);
                }
            }

            return clone;
        }

        public static void SetAttestStates(this PayrollCalculationProductDTO e, bool transactionHasSameAttestState)
        {
            e.SetLowestAttestState(e.AttestStates);
            e.HasSameAttestState = transactionHasSameAttestState && e.AttestStates.Count <= 1;
        }

        public static List<AttestPayrollTransactionDTO> GetTransactions(this List<PayrollCalculationProductDTO> dtos, bool onlyTimePayrollTransactions)
        {
            var transactions = new List<AttestPayrollTransactionDTO>();

            foreach (var dto in dtos)
            {
                transactions.AddRange(dto.AttestPayrollTransactions);
            }

            if (onlyTimePayrollTransactions)
                return transactions.Where(x => !x.IsScheduleTransaction).ToList();

            return transactions;
        }

        public static List<AttestPayrollTransactionDTO> GetScheduleTransactions(this List<PayrollCalculationProductDTO> dtos)
        {
            var transactions = new List<AttestPayrollTransactionDTO>();

            foreach (var dto in dtos)
            {
                transactions.AddRange(dto.AttestPayrollTransactions);
            }

            return transactions.Where(x => x.IsScheduleTransaction).ToList();
        }

        #endregion

        #region AttestPayrollTransactionDTO

        public static List<PayrollCalculationPeriodSumItemDTO> ToPayrollCalculationPeriodSumItemDTOs(this IEnumerable<AttestPayrollTransactionDTO> transactionItems)
        {
            var sums = new List<PayrollCalculationPeriodSumItemDTO>();

            foreach (var transactionItem in transactionItems)
            {
                sums.Add(transactionItem.ToPayrollCalculationPeriodSumItemDTO());
            }

            return sums;
        }

        public static PayrollCalculationPeriodSumItemDTO ToPayrollCalculationPeriodSumItemDTO(this AttestPayrollTransactionDTO transactionItem)
        {
            return new PayrollCalculationPeriodSumItemDTO
            {
                SysPayrollTypeLevel1 = transactionItem.TransactionSysPayrollTypeLevel1,
                SysPayrollTypeLevel2 = transactionItem.TransactionSysPayrollTypeLevel2,
                SysPayrollTypeLevel3 = transactionItem.TransactionSysPayrollTypeLevel3,
                SysPayrollTypeLevel4 = transactionItem.TransactionSysPayrollTypeLevel4,
                Amount = transactionItem.Amount,
            };
        }

        //Assumes that the input is grouped by product and unitprice
        public static void SetQuantityDaysValues(this PayrollCalculationProductDTO item, IEnumerable<AttestPayrollTransactionDTO> transactions, decimal quantityDays)
        {
            if (item.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.Hours)
            {
                #region Quantity

                decimal quantity = 0;
                foreach (var transactionsByDate in transactions.GroupBy(i => i.Date))
                {
                    quantity += transactionsByDate.FirstOrDefault()?.QuantityDays ?? 0;
                }

                item.Quantity = quantity;

                #endregion

                #region UnitPrice

                bool isVacationCompensation = item.IsVacationCompensation();
                bool isVacationFiveDaysPerWeek = item.IsVacationFiveDaysPerWeek && item.IsVacationSalary();

                if (!isVacationCompensation && !isVacationFiveDaysPerWeek)
                {
                    decimal unitPrice = 0;
                    decimal unitPriceCurrency = 0;
                    decimal unitPriceEntCurrency = 0;

                    var transactionsForFirstDay = transactions.GroupBy(i => i.Date).FirstOrDefault().ToList();
                    foreach (var transaction in transactionsForFirstDay)
                    {
                        //Since transaction.UnitPrice is rounded to 2 decimals we have lost decimals
                        //transaction.Amount has been calculated with a unitprice that is not rounded so it should be more accurate
                        if (transaction.Amount.HasValue)
                            unitPrice += transaction.Amount.Value;
                        if (transaction.AmountCurrency.HasValue)
                            unitPriceCurrency += transaction.AmountCurrency.Value;
                        if (transaction.AmountEntCurrency.HasValue)
                            unitPriceEntCurrency += transaction.AmountEntCurrency.Value;

                        //if (transaction.UnitPrice.HasValue)
                        //    unitPrice += Math.Round((transaction.Quantity / 60) * transaction.UnitPrice.Value, 2);
                        //if (transaction.UnitPriceCurrency.HasValue)
                        //    unitPriceCurrency += Math.Round((transaction.Quantity / 60) * transaction.UnitPriceCurrency.Value, 2);
                        //if (transaction.UnitPrice.HasValue)
                        //    unitPriceEntCurrency += Math.Round((transaction.Quantity / 60) * transaction.UnitPriceEntCurrency.Value, 2);
                    }

                    item.UnitPrice = quantityDays != 0 ? Math.Round((unitPrice / quantityDays), 2) : 0;
                    item.UnitPriceCurrency = quantityDays != 0 ? Math.Round((unitPriceCurrency / quantityDays), 2) : 0;
                    item.UnitPriceEntCurrency = quantityDays != 0 ? Math.Round((unitPriceEntCurrency / quantityDays), 2) : 0;

                    //fix ex: 1190 -> 1190.00 (needed in Angular) 
                    item.UnitPrice = Math.Round(decimal.Multiply(item.UnitPrice.Value, 1.00m), 2);
                    item.UnitPriceCurrency = Math.Round(decimal.Multiply(item.UnitPriceCurrency.Value, 1.00m), 2);
                    item.UnitPriceEntCurrency = Math.Round(decimal.Multiply(item.UnitPriceEntCurrency.Value, 1.00m), 2);
                }

                #endregion
            }
        }

        public static List<AttestPayrollTransactionDTO> CopyCollection(this IEnumerable<AttestPayrollTransactionDTO> l)
        {
            var dtos = new List<AttestPayrollTransactionDTO>();

            foreach (var e in l)
            {
                dtos.Add(CopyItem(e, false));
            }

            return dtos;
        }

        public static AttestPayrollTransactionDTO CopyItem(this AttestPayrollTransactionDTO e, bool copyCollections)
        {
            var clone = new AttestPayrollTransactionDTO();

            var clonePis = clone.GetType().GetProperties();
            foreach (var clonePi in clonePis)
            {
                PropertyInfo prototypePi = e.GetType().GetProperty(clonePi.Name);
                if (prototypePi.CanWrite)
                    clonePi.SetValue(clone, prototypePi.GetValue(e, null), null);
            }

            if (copyCollections)
            {
                if (e.AttestTransitionLogs != null)
                {
                    clone.AttestTransitionLogs = new List<AttestTransitionLogDTO>();
                    foreach (var logItem in e.AttestTransitionLogs)
                    {
                        var logClone = new AttestTransitionLogDTO();
                        var logClonePis = logClone.GetType().GetProperties();
                        foreach (var logClonePi in logClonePis)
                        {
                            PropertyInfo prototypePi = logItem.GetType().GetProperty(logClonePi.Name);
                            if (prototypePi.CanWrite)
                                logClonePi.SetValue(logClone, prototypePi.GetValue(logItem, null), null);
                        }
                        clone.AttestTransitionLogs.Add(logClone);
                    }
                }

                if (e.AccountInternals != null)
                {
                    clone.AccountInternals = new List<AccountDTO>();
                    foreach (var accountInternal in e.AccountInternals)
                    {
                        var accountInternalClone = new AccountDTO();
                        var accountInternalClonePis = accountInternalClone.GetType().GetProperties();
                        foreach (var accountInternalPi in accountInternalClonePis)
                        {
                            PropertyInfo prototypePi = accountInternal.GetType().GetProperty(accountInternalPi.Name);
                            if (prototypePi.CanWrite)
                                accountInternalPi.SetValue(accountInternalClone, prototypePi.GetValue(accountInternal, null), null);
                        }
                        clone.AccountInternals.Add(accountInternalClone);
                    }
                }
            }

            return clone;
        }

        public static void SetAccountingStrings(this AttestPayrollTransactionDTO e)
        {
            e.AccountingShortString = e.GetAccountingString(false);
            e.AccountingLongString = e.GetAccountingString(true);
        }

        public static void SetAccountingStrings(this AttestPayrollTransactionDTO e, TimeTransactionItem item, List<AccountDimDTO> accountDims)
        {
            e.AccountingShortString = item.GetAccountingString(accountDims, false, out _, out _);
            e.AccountingLongString = item.GetAccountingString(accountDims, true, out int accountStdId, out List<int> accountInternalIds);
            e.AccountStdId = accountStdId;
            e.AccountInternalIds = accountInternalIds;
        }

        public static string GetAccountingString(this AttestPayrollTransactionDTO e, bool showDetails)
        {
            StringBuilder sb = new StringBuilder();

            if (e.AccountDims != null)
            {
                foreach (var accountDim in e.AccountDims.OrderBy(i => i.AccountDimNr))
                {
                    if (accountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD)
                    {
                        #region AccountStd

                        //When AccountStd is null, exclude if from the string 
                        if (e.AccountStd == null)
                            continue;

                        if (showDetails)
                        {
                            sb.Append(accountDim.Name);
                            sb.Append(": ");
                            sb.Append(e.AccountStd.AccountNr);
                            sb.Append(". ");
                            sb.Append(e.AccountStd.Name);
                        }
                        else
                        {
                            sb.Append(e.AccountStd.AccountNr);
                        }

                        #endregion
                    }
                    else
                    {
                        #region AccountInternal

                        AccountDTO accountInternal = e.AccountInternals.FirstOrDefault(i => i.AccountDimId == accountDim.AccountDimId);

                        if (showDetails)
                        {
                            sb.Append(accountDim.Name);
                            sb.Append(": ");
                            if (accountInternal != null)
                            {
                                //When name-convention is [AccountNr Name], exclude AccountNr and only show name (to prevent ex: 130. 130 Kassa)
                                bool excludeAccountNr = !String.IsNullOrEmpty(accountInternal.AccountNr) && !String.IsNullOrEmpty(accountInternal.Name) && accountInternal.Name.StartsWith(accountInternal.AccountNr + " ");
                                if (!excludeAccountNr)
                                {
                                    sb.Append(accountInternal.AccountNr);
                                    sb.Append(". ");
                                }
                                sb.Append(accountInternal.Name);
                            }
                        }
                        else if (accountInternal != null)
                        {
                            sb.Append(accountInternal.AccountNr);
                        }

                        #endregion
                    }

                    sb.Append(showDetails ? "; " : ";");
                }
            }

            return sb.ToString();
        }

        public static string GetAccountingString(this TimeTransactionItem e, List<AccountDimDTO> accountDims, bool showDetails, out int accountStdId, out List<int> accountInternalIds)
        {
            StringBuilder sb = new StringBuilder();

            accountStdId = 0;
            accountInternalIds = new List<int>();

            bool displayAccountNrForNoneDetails = false;

            if (accountDims != null)
            {
                int accountDimCounter = 1;
                foreach (var accountDim in accountDims.OrderBy(i => i.AccountDimNr))
                {
                    if (accountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD)
                    {
                        #region AccountStd

                        //When AccountStd is null, exclude if from the string 
                        if (e.Dim1Id == 0)
                            continue;

                        //When AccountStd is not null, always display AccountNr instead of Name
                        displayAccountNrForNoneDetails = true;

                        if (showDetails)
                        {
                            sb.Append(accountDim.Name);
                            sb.Append(": ");
                            sb.Append(e.Dim1Nr);
                            sb.Append(". ");
                            sb.Append(e.Dim1Name);
                        }
                        else
                        {
                            sb.Append(displayAccountNrForNoneDetails ? e.Dim1Nr : e.Dim1Name);
                        }

                        accountStdId = e.Dim1Id;

                        #endregion
                    }
                    else
                    {
                        #region AccountInternal

                        e.GetDimValues(accountDimCounter, out int accountId, out string accountNr, out string accountName);

                        if (showDetails)
                        {
                            sb.Append(accountDim.Name);
                            sb.Append(": ");
                            if (accountId > 0)
                            {
                                //When name-convention is [AccountNr Name], exclude AccountNr and only show name (to prevent ex: 130. 130 Kassa)
                                bool excludeAccountNr = !String.IsNullOrEmpty(accountNr) && !String.IsNullOrEmpty(accountName) && accountName.StartsWith(accountNr + " ");
                                if (!excludeAccountNr)
                                {
                                    sb.Append(accountNr);
                                    sb.Append(". ");
                                }
                                sb.Append(accountName);
                            }
                        }
                        else
                        {
                            if (accountId > 0)
                            {
                                sb.Append(displayAccountNrForNoneDetails ? accountNr : accountName);
                            }
                        }

                        accountInternalIds.Add(accountId);

                        #endregion
                    }

                    sb.Append(showDetails ? "; " : ";");
                    accountDimCounter++;
                }
            }

            return sb.ToString();
        }

        public static string GetAccountingIdString(this AttestPayrollTransactionDTO e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(e.AccountStd.AccountId);

            if (e.AccountInternals != null)
            {
                foreach (var ai in e.AccountInternals)
                    sb.Append($"|{ai.AccountId}");
            }
            return sb.ToString();
        }

        #endregion

        #region SaveAttestTransactionDTO

        public static int GetNrOfTransactions(this IEnumerable<SaveAttestTransactionDTO> l)
        {
            int count = 0;
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (e.TimePayrollTransactionIds != null)
                        count = count + e.TimePayrollTransactionIds.Count;
                }
            }
            return count;
        }

        #endregion

        #endregion

        #region AttestPayrollTransactionDTO

        public static decimal? GetUnitPrice(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<decimal?>(l.Select(i => i.UnitPrice).ToList(), out hasMultiple);
        }

        public static decimal? GetUnitPriceCurrency(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<decimal?>(l.Select(i => i.UnitPriceCurrency).ToList(), out hasMultiple);
        }

        public static decimal? GetUnitPriceEntCurrency(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<decimal?>(l.Select(i => i.UnitPriceEntCurrency).ToList(), out hasMultiple);
        }

        public static bool GetIsRounding(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<bool>(l.Select(i => i.IsRounding).ToList(), out hasMultiple);
        }

        public static bool GetIsCentRounding(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<bool>(l.Select(i => i.IsCentRounding).ToList(), out hasMultiple);
        }

        public static bool GetIncludedInPayrollProductChain(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<bool>(l.Select(i => i.IncludedInPayrollProductChain).ToList(), out hasMultiple);
        }

        public static bool GetIsAverageCalculated(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<bool>(l.Select(i => i.IsAverageCalculated).ToList(), out hasMultiple);
        }

        public static int GetTimeUnit(this List<AttestPayrollTransactionDTO> l, out bool hasMultiple)
        {
            return Extensions.GetUniqueValue<int>(l.Select(i => i.TimeUnit).ToList(), out hasMultiple);
        }

        #endregion

        #region AttestEmployeeAdditionDeductionTransactionDTO

        public static AttestPayrollTransactionDTO ToAttestPayrollTransactionDTO(this AttestEmployeeAdditionDeductionTransactionDTO e, int employeeId)
        {
            return new AttestPayrollTransactionDTO()
            {
                TimePayrollTransactionId = e.TransactionId,
                EmployeeId = employeeId,
                Date = e.Date,
                PayrollProductId = e.ProductId,
                PayrollProductName = e.ProductName,
                AttestStateId = e.AttestStateId,
                AttestStateName = e.AttestStateName,
                AttestStateColor = e.AttestStateColor,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                Amount = e.Amount,
                VatAmount = e.VatAmount,
                Comment = e.Comment,
                IsScheduleTransaction = false,
            };
        }

        public static List<AttestPayrollTransactionDTO> ToAttestPayrollTransactionDTOs(this List<AttestEmployeeAdditionDeductionTransactionDTO> l, int employeeId)
        {
            var dtos = new List<AttestPayrollTransactionDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToAttestPayrollTransactionDTO(employeeId));
                }
            }
            return dtos;
        }

        #endregion

        #region BreakDTO

        public static List<WorkIntervalDTO> GetWorkIntervals(this List<BreakDTO> l, int employeeId, DateTime scheduleIn, DateTime scheduleOut)
        {
            List<WorkIntervalDTO> workIntervals = new List<WorkIntervalDTO>();

            if (l.Count > 0)
            {
                var firstBreak = l[0];
                var lastBreak = l[l.Count - 1];
                if (firstBreak.StartTime >= scheduleIn && lastBreak.StopTime <= scheduleOut)
                {
                    //Schedule in -> First break
                    WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, scheduleIn, firstBreak.StartTime);
                    if (workInterval.TotalMinutes != 0)
                        workIntervals.Add(workInterval);

                    //Last break -> Schedule out
                    workInterval = new WorkIntervalDTO(employeeId, lastBreak.StopTime, scheduleOut);
                    if (workInterval.TotalMinutes != 0)
                        workIntervals.Add(workInterval);

                    //Between breaks
                    for (int breakNr = 1; breakNr <= l.Count; breakNr++)
                    {
                        if (breakNr == 1)
                            continue;

                        BreakDTO prevBrk = l[breakNr - 2];
                        BreakDTO brk = l[breakNr - 1];
                        workInterval = new WorkIntervalDTO(employeeId, prevBrk.StopTime, brk.StartTime);
                        if (workInterval.TotalMinutes != 0)
                            workIntervals.Add(workInterval);
                    }
                }
            }
            else
            {
                workIntervals.Add(new WorkIntervalDTO(employeeId, scheduleIn, scheduleOut));
            }

            return workIntervals.Where(i => i.StartTime < i.StopTime).OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        #endregion

        #region DateRangeDTO

        public static string GetIntervals(this List<DateRangeDTO> l)
        {
            return l?.Select(p => p.GetInterval()).ToCommaSeparated();
        }

        public static string GetComments(this List<DateRangeDTO> l)
        {
            return l?.Select(p => p.Comment).Distinct().ToCommaSeparated();
        }

        public static bool Overlaps(this DateRangeDTO interval1, DateRangeDTO interval2) => interval1.Start <= interval2.Start ? interval1.Stop >= interval2.Start : interval2.Stop >= interval1.Start;

        public static DateRangeDTO MergeWith(this DateRangeDTO interval1, DateRangeDTO interval2) => new DateRangeDTO
        {
            Start = new DateTime(Math.Min(interval1.Start.Ticks, interval2.Start.Ticks)),
            Stop = new DateTime(Math.Max(interval1.Stop.Ticks, interval2.Stop.Ticks))
        };

        #endregion

        #region EmployeeAgeDTO

        public static List<EmployeeAgeDTO> GetMinors(this List<EmployeeAgeDTO> l)
        {
            return l.Where(i => i.IsAgeYoungerThan18).ToList();
        }

        public static List<EmployeeAgeDTO> GetSeniors(this List<EmployeeAgeDTO> l)
        {
            return l.Where(i => !i.IsAgeYoungerThan18).ToList();
        }

        public static List<EmployeeAgeDTO> WithEmployment(this List<EmployeeAgeDTO> l, DateTime date)
        {
            return l.Where(i => i.HasEmployment(date)).ToList();
        }

        #endregion

        #region EmployeeListDTO

        public static List<EmployeeSmallDTO> ToEmployeeSmallDTOs(this List<EmployeeListDTO> list, bool concatNumberAndName)
        {
            List<EmployeeSmallDTO> dtos = new List<EmployeeSmallDTO>();
            foreach (EmployeeListDTO dto in list)
            {
                string name = concatNumberAndName ? String.Format("({0}) {1}", dto.EmployeeNr, dto.Name) : dto.Name;

                dtos.Add(new EmployeeSmallDTO()
                {
                    EmployeeId = dto.EmployeeId,
                    EmployeeNr = dto.EmployeeNr,
                    Name = name
                });
            }

            return dtos;
        }

        public static Dictionary<int, string> ToEmployeeDictionary(this List<EmployeeListDTO> list, bool concatNumberAndName)
        {
            return list.ToDictionary(e => e.EmployeeId, e => concatNumberAndName ? String.Format("({0}) {1}", e.EmployeeNr, e.Name) : e.Name);
        }

        #endregion

        #region EmployeeTimeWorkAccount

        public static EmployeeTimeWorkAccountDTO GetClosestTimeWorkAccount(this IEnumerable<EmployeeTimeWorkAccountDTO> l, TimeWorkAccountDTO timeWorkAccount, DateTime fromDate, DateTime? toDate, DateTime minDate, DateTime maxDate)
        {
            if (l == null || timeWorkAccount == null)
                return null;

            List<EmployeeTimeWorkAccountDTO> matchingTimeWorkAccounts = l.Where(a => a.TimeWorkAccountId == timeWorkAccount.TimeWorkAccountId && a.State == (int)SoeEntityState.Active && CalendarUtility.IsDatesOverlapping(fromDate, (toDate ?? maxDate), a.DateFrom ?? minDate, (a.DateTo.HasValue && a.DateTo.Value != CalendarUtility.DATETIME_DEFAULT ? a.DateTo.Value : maxDate))).ToList();

            EmployeeTimeWorkAccountDTO e = null;
            if (matchingTimeWorkAccounts.Count > 1)
                e = matchingTimeWorkAccounts.FirstOrDefault(i => !i.DateTo.HasValue);
            if (e == null)
                e = matchingTimeWorkAccounts.OrderByDescending(i => i.DateTo).FirstOrDefault();
            return e;
        }

        public static bool IsOverlapping(this List<EmployeeTimeWorkAccountDTO> l, Guid key, DateTime? dateFrom, DateTime? dateTo)
        {
            return l?.Any(e => e.Key != key && CalendarUtility.IsDatesOverlappingNullable(dateFrom, dateTo, e.DateFrom, e.DateTo, validateDatesAreTouching: true)) ?? false;
        }

        #endregion

        #region EmploymentType

        public static List<EmploymentTypeSmallDTO> ToSmallEmploymentTypes(this List<EmploymentTypeDTO> l)
        {
            List<EmploymentTypeSmallDTO> genericList = new List<EmploymentTypeSmallDTO>();
            foreach (var e in l)
            {
                genericList.Add(new EmploymentTypeSmallDTO()
                {
                    Id = e.GetEmploymentType(),
                    Name = e.Name,
                    Type = e.Type,
                    Active = e.Active,
                });
            }
            return genericList;
        }

        public static EmploymentTypeDTO GetEmploymentType(this List<EmploymentTypeDTO> l, int type)
        {
            EmploymentTypeDTO employmentType = null;
            if (EmploymentTypeDTO.IsStandard(type))
            {
                employmentType = l?.FirstOrDefault(e => e.Type == type && e.Standard);
                if (employmentType != null)
                {
                    // Check if there is an overridden type
                    EmploymentTypeDTO copy = l?.FirstOrDefault(e => e.Type == type && !e.Standard && e.SettingOnly);
                    if (copy != null)
                    {
                        employmentType.Code = copy.Code;
                        employmentType.ExternalCode = copy.ExternalCode;
                        employmentType.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = copy.ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
                    }
                }
            }
            else
            {
                employmentType = l?.FirstOrDefault(e => e.EmploymentTypeId == type);
            }

            return employmentType;
        }

        public static int GetType(this List<EmploymentTypeDTO> l, int type)
        {
            if (EmploymentTypeDTO.IsStandard(type))
            {
                if (l.IsNullOrEmpty() || !l.Any(i => i.Type == type))
                    return (int)TermGroup_EmploymentType.Unknown;
                return type;
            }
            else
                return l?.FirstOrDefault(i => i.EmploymentTypeId == type)?.Type ?? (int)TermGroup_EmploymentType.Unknown;
        }

        public static string GetName(this List<EmploymentTypeDTO> l, int type)
        {
            return l.GetEmploymentType(type)?.Name ?? string.Empty;
        }

        public static bool Exists(this List<EmploymentTypeDTO> l, int type)
        {
            return l?.Any(i => i.Type == type) ?? false;
        }

        #endregion

        #region GrossNetCost

        public static TimeSpan GetNetTime(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            TimeSpan ts = new TimeSpan();
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                ts = ts.Add(dto.NetTime);
            }
            return ts;
        }

        public static int GetNetTimeMinutes(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            return (int)dtos.GetNetTime(timeScheduleTemplateBlockId).TotalMinutes;
        }

        public static TimeSpan GetGrossTime(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            TimeSpan ts = new TimeSpan();
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                ts = ts.Add(dto.GrossTime);
            }
            return ts;
        }

        public static int GetGrossTimeMinutes(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            return (int)dtos.GetGrossTime(timeScheduleTemplateBlockId).TotalMinutes;
        }

        public static TimeSpan GetBreakTime(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            TimeSpan ts = new TimeSpan();
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                ts = ts.Add(dto.BreakTime);
            }
            return ts;
        }

        public static int GetBreakTimeMinutes(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            return (int)dtos.GetBreakTime(timeScheduleTemplateBlockId).TotalMinutes;
        }

        public static TimeSpan GetIwhTime(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            TimeSpan ts = new TimeSpan();
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                ts = ts.Add(dto.IwhTime);
            }
            return ts;
        }

        public static int GetIwhTimeMinutes(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            return (int)dtos.GetIwhTime(timeScheduleTemplateBlockId).TotalMinutes;
        }

        public static TimeSpan GetGrossNetDiff(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            TimeSpan ts = new TimeSpan();
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                ts.Add(dto.GrossNetDiff);
            }
            return ts;
        }

        public static decimal GetCostPerHour(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            decimal d = 0;
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                d += dto.CostPerHour;
            }
            return d;
        }

        public static decimal GetEmploymentTaxCost(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            decimal d = 0;
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                d += dto.EmploymentTaxCost;
            }
            return d;
        }

        public static decimal GetSupplementChargeCost(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            decimal d = 0;
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                d += dto.SupplementChargeCost;
            }
            return d;
        }

        public static decimal GetTotalCost(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            decimal d = 0;
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                d += dto.TotalCost;
            }
            return d;
        }

        public static decimal GetTotalCostIncludingEmploymentTaxAndSupplementCharge(this List<GrossNetCostDTO> dtos, int? timeScheduleTemplateBlockId = null)
        {
            decimal d = 0;
            foreach (var dto in dtos)
            {
                if (timeScheduleTemplateBlockId.HasValue && timeScheduleTemplateBlockId.Value != dto.TimeScheduleTemplateBlockId)
                    continue;
                d += dto.TotalCostIncEmpTaxAndSuppCharge;
            }
            return d;
        }

        #endregion

        #region PayrollImportEmployeeTransactionDTO

        public static List<AttestPayrollTransactionDTO> CreateTransactionItems(this IEnumerable<PayrollImportEmployeeTransactionDTO> l, int employeeId, int? defaultAccountStdId = null)
        {
            List<AttestPayrollTransactionDTO> transactionItems = new List<AttestPayrollTransactionDTO>();
            foreach (PayrollImportEmployeeTransactionDTO e in l)
            {
                transactionItems.Add(e.CreateTransactionItem(employeeId, defaultAccountStdId));
            }
            return transactionItems;
        }

        public static AttestPayrollTransactionDTO CreateTransactionItem(this PayrollImportEmployeeTransactionDTO e, int employeeId, int? defaultAccountStdId = null)
        {
            AttestPayrollTransactionDTO transactionItem = new AttestPayrollTransactionDTO()
            {
                Date = e.Date,
                Quantity = e.Quantity,
                Amount = e.Amount,
                IsSpecifiedUnitPrice = e.Amount != 0,
                ManuallyAdded = true,
                Comment = e.Note,

                EmployeeId = employeeId,
                PayrollProductId = e.PayrollProductId ?? 0,
                PayrollImportEmployeeTransactionId = e.PayrollImportEmployeeTransactionId,
                AccountStdId = e.AccountStdId ?? defaultAccountStdId ?? 0,
                AccountInternalIds = e.AccountInternals?.Where(i => i.AccountId.HasValue).Select(i => i.AccountId.Value).ToList() ?? new List<int>(),
            };

            return transactionItem;
        }

        #endregion

        #region ReportDataSelectionDTO

        public static TSelection GetSelection<TSelection>(this IEnumerable<ReportDataSelectionDTO> selections, string key = null) where TSelection : ReportDataSelectionDTO
        {
            return selections?.GetSelections<TSelection>().FirstOrDefault(s => key == null || s.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) ?? null;
        }

        public static IEnumerable<TSelection> GetSelections<TSelection>(this IEnumerable<ReportDataSelectionDTO> selections) where TSelection : ReportDataSelectionDTO
        {
            return (selections ?? Enumerable.Empty<ReportDataSelectionDTO>()).Where(s => s is TSelection).Cast<TSelection>();
        }

        #endregion

        #region StaffingNeedsHead

        public static List<StaffingNeedsRowPeriodDTO> GetPeriods(this StaffingNeedsHeadDTO head, int? timeScheduleTaskId = null, DateTime? time = null)
        {
            return head != null ? head.Rows.GetPeriods() : new List<StaffingNeedsRowPeriodDTO>();
        }

        #endregion

        #region StaffingNeedsRow

        public static List<StaffingNeedsRowPeriodDTO> GetPeriods(this List<StaffingNeedsRowDTO> rows, int? timeScheduleTaskId = null)
        {
            List<StaffingNeedsRowPeriodDTO> periods = new List<StaffingNeedsRowPeriodDTO>();

            foreach (StaffingNeedsRowDTO row in rows)
            {
                foreach (StaffingNeedsRowPeriodDTO period in row.Periods)
                {
                    if (!timeScheduleTaskId.HasValue || timeScheduleTaskId.Value == period.TimeScheduleTaskId)
                        periods.Add(period);
                }
            }

            return periods;
        }

        #endregion

        #region TimeBlockDTO

        public static List<WorkIntervalDTO> GetWorkIntervals(this List<TimeBlockDTO> l)
        {
            List<WorkIntervalDTO> workIntervals = new List<WorkIntervalDTO>();

            if (l.Count == 0)
                return workIntervals;

            foreach (var employeeGrouping in l.GroupBy(i => i.EmployeeId))
            {
                int employeeId = employeeGrouping.Key;

                foreach (var employeeDateGrouping in employeeGrouping.GroupBy(i => i.StartTime.Date))
                {
                    DateTime date = employeeDateGrouping.Key;
                    List<TimeBlockDTO> timeBlocksByEmployeeAndDate = employeeDateGrouping.Where(x => !x.IsBreak).ToList();

                    foreach (var timeBlock in timeBlocksByEmployeeAndDate)
                    {
                        DateTime start = timeBlock.StartTime;
                        DateTime stop = timeBlock.StopTime;

                        WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, start, stop);
                        if (workInterval.TotalMinutes != 0)
                            workIntervals.Add(workInterval);
                    }
                }
            }

            return workIntervals.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static List<List<TimeBlockDTO>> GetCoherentTimeBlocks(this List<TimeBlockDTO> l)
        {
            List<List<TimeBlockDTO>> coherentGroups = new List<List<TimeBlockDTO>>();
            List<TimeBlockDTO> coherentBlocks = new List<TimeBlockDTO>();
            var blocks = l.Where(x => x.Date.HasValue).ToList();

            if (blocks.Count == 0)
            {
                return coherentGroups;
            }

            if (blocks.Count == 1)
            {
                coherentGroups.Add(l);
                return coherentGroups;
            }

            foreach (var currentBlock in blocks.OrderBy(x => x.ActualStartTime.Value))
            {
                TimeBlockDTO previousShift = coherentBlocks.GetPrev(currentBlock);
                if (previousShift != null)
                {
                    if (previousShift.ActualStopTime == currentBlock.ActualStartTime)
                    {
                        coherentBlocks.Add(currentBlock);
                    }
                    else
                    {
                        //A gap is found..

                        //...close current coherent chain....
                        coherentGroups.Add(coherentBlocks);
                        coherentBlocks = new List<TimeBlockDTO>();

                        //...and start a new chain
                        coherentBlocks.Add(currentBlock);
                    }
                }
                else
                {
                    //Fist shift has no previous
                    coherentBlocks.Add(currentBlock);
                }
            }

            coherentGroups.Add(coherentBlocks);
            return coherentGroups;
        }

        public static TimeBlockDTO GetPrev(this List<TimeBlockDTO> l, TimeBlockDTO e)
        {
            return l.Where(i => i.Date.HasValue && i.ActualStopTime <= e.ActualStartTime).OrderByDescending(i => i.ActualStopTime).FirstOrDefault();
        }

        public static List<WorkIntervalDTO> GetCoherentWorkIntervals(this List<TimeBlockDTO> l)
        {
            if (l.IsNullOrEmpty())
                return new List<WorkIntervalDTO>();

            List<WorkIntervalDTO> workIntervals = new List<WorkIntervalDTO>();

            foreach (var employeeShifts in l.GroupBy(i => i.EmployeeId))
            {
                int employeeId = employeeShifts.Key;

                List<List<TimeBlockDTO>> coherentGroups = employeeShifts.ToList().GetCoherentTimeBlocks();
                foreach (var coherentBlocks in coherentGroups)
                {
                    WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, coherentBlocks.GetIn(), coherentBlocks.GetOut());
                    if (workInterval.TotalMinutes != 0)
                        workIntervals.Add(workInterval);
                }
            }

            return workIntervals.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static DateTime GetIn(this List<TimeBlockDTO> l)
        {
            if (l.Where(x => x.Date.HasValue).IsNullOrEmpty())
                return CalendarUtility.DATETIME_DEFAULT;
            return l.Where(x => x.Date.HasValue).OrderBy(i => i.ActualStartTime).FirstOrDefault().ActualStartTime.Value;
        }

        public static DateTime GetOut(this List<TimeBlockDTO> l)
        {
            if (l.Where(x => x.Date.HasValue).IsNullOrEmpty())
                return CalendarUtility.DATETIME_DEFAULT;
            return l.Where(x => x.Date.HasValue).OrderByDescending(i => i.ActualStopTime).FirstOrDefault().ActualStopTime.Value;
        }
        #endregion

        #region TimeRuleIwhDTO

        public static bool IsDateValid(this TimeRuleIwhDTO e, DateTime date)
        {
            return (!e.RuleStartDate.HasValue || e.RuleStartDate.Value >= date) && (!e.RuleStopDate.HasValue || e.RuleStopDate.Value <= date);
        }

        #endregion

        #region TimeTransactionItem

        public static TimeTransactionItem GetFirst(this List<TimeTransactionItem> l)
        {
            return l.OrderBy(t => t.Date).FirstOrDefault();
        }

        public static DateTime? GetFirstDate(this List<TimeTransactionItem> l)
        {
            var e = l.GetFirst();
            return e != null ? e.Date : (DateTime?)null;
        }

        public static TimeTransactionItem GetLast(this List<TimeTransactionItem> l)
        {
            return l.OrderByDescending(t => t.Date).FirstOrDefault();
        }

        public static DateTime? GetLastDate(this List<TimeTransactionItem> l)
        {
            var e = l.GetLast();
            return e != null ? e.Date : (DateTime?)null;
        }

        public static bool HasAccountStd(this TimeTransactionItem e)
        {
            return e.Dim1Id > 0;
        }

        public static bool HasAccountInternal(this TimeTransactionItem e, int accountDimCounter)
        {
            if (accountDimCounter == 2)
                return e.Dim2Id > 0;
            else if (accountDimCounter == 3)
                return e.Dim3Id > 0;
            else if (accountDimCounter == 4)
                return e.Dim4Id > 0;
            else if (accountDimCounter == 5)
                return e.Dim5Id > 0;
            else if (accountDimCounter == 6)
                return e.Dim6Id > 0;
            return false;
        }

        public static void RemoveAccountInternal(this TimeTransactionItem e, int accountDimId)
        {
            if (accountDimId == 2)
                e.Dim2Id = 0;
            else if (accountDimId == 3)
                e.Dim3Id = 0;
            else if (accountDimId == 4)
                e.Dim4Id = 0;
            else if (accountDimId == 5)
                e.Dim5Id = 0;
            else if (accountDimId == 6)
                e.Dim6Id = 0;
        }
        public static void ClearAccountInternals(this TimeTransactionItem e)
        {
            e.Dim2Id = 0;
            e.Dim3Id = 0;
            e.Dim4Id = 0;
            e.Dim5Id = 0;
            e.Dim6Id = 0;
        }

        public static bool IsExcludedInTime(this TimeTransactionItem e)
        {
            //Also, see IsExcludedInTime extension on IPayrollTransction

            if (e.IsAdded)
                return true;
            if (e.IsFixed)
                return true;
            if (e.IsRounding)
                return true;
            //EmployeeVehicleId
            if (e.IsUnionFee())
                return true;
            if (e.IsVacationCompensation())
                return true;
            if (e.IsTaxAndNotOptional())
                return true;
            if (e.IsEmploymentTaxCredit())
                return true;
            if (e.IsEmploymentTaxDebit())
                return true;
            if (e.IsSupplementCharge())
                return true;
            if (e.IsDeductionSalaryDistress())
                return true;
            if (e.IsBenefit())
                return true;
            if (e.IsNetSalary())
                return true;
            return false;
        }

        public static void GetChain(this List<TimeTransactionItem> items, TimeTransactionItem parentTransaction, List<TimeTransactionItem> chainedTransactions)
        {
            if (chainedTransactions == null)
                chainedTransactions = new List<TimeTransactionItem>();

            if (chainedTransactions.Count == 0)
                chainedTransactions.Add(parentTransaction);

            var childTransaction = items.FirstOrDefault(x => x.ParentGuidId.HasValue && x.ParentGuidId.Value == parentTransaction.GuidId);
            if (childTransaction != null)
            {
                chainedTransactions.Add(childTransaction);
                GetChain(items, childTransaction, chainedTransactions);
            }
        }

        #endregion

        #region TimeSchedulePlanningDayDTO

        public static List<List<TimeSchedulePlanningDayDTO>> GetCoherentShifts(this List<TimeSchedulePlanningDayDTO> l)
        {
            List<List<TimeSchedulePlanningDayDTO>> coherentShiftGroups = new List<List<TimeSchedulePlanningDayDTO>>();
            List<TimeSchedulePlanningDayDTO> coherentShifts = new List<TimeSchedulePlanningDayDTO>();

            if (l.Count == 0)
            {
                return coherentShiftGroups;
            }

            if (l.Count == 1)
            {
                coherentShiftGroups.Add(l);
                return coherentShiftGroups;
            }

            foreach (var currentShift in l.OrderBy(x => x.StartTime))
            {
                TimeSchedulePlanningDayDTO previousShift = coherentShifts.GetPrev(currentShift);
                if (previousShift != null)
                {
                    if (previousShift.StopTime == currentShift.StartTime)
                    {
                        coherentShifts.Add(currentShift);
                    }
                    else
                    {
                        //A gap is found..

                        //...close current coherent chain....
                        coherentShiftGroups.Add(coherentShifts);
                        coherentShifts = new List<TimeSchedulePlanningDayDTO>();

                        //...and start a new chain
                        coherentShifts.Add(currentShift);
                    }
                }
                else
                {
                    //Fist shift has no previous
                    coherentShifts.Add(currentShift);
                }
            }

            coherentShiftGroups.Add(coherentShifts);
            return coherentShiftGroups;
        }

        public static TimeSchedulePlanningDayDTO GetPrev(this List<TimeSchedulePlanningDayDTO> l, TimeSchedulePlanningDayDTO e)
        {
            return l.Where(i => i.StopTime <= e.StartTime).OrderByDescending(i => i.StopTime).FirstOrDefault();
        }

        public static TimeSchedulePlanningDayDTO GetNext(this List<TimeSchedulePlanningDayDTO> l, TimeSchedulePlanningDayDTO e)
        {
            return l.Where(i => i.StartTime >= e.StopTime).OrderBy(i => i.StartTime).FirstOrDefault();
        }

        public static TimeSchedulePlanningDayDTO GetFirst(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l.OrderBy(i => i.StartTime).FirstOrDefault();
        }

        public static List<TimeBlockDTO> ToTimeBlockDTOs(this List<TimeSchedulePlanningDayDTO> l)
        {
            var dtos = new List<TimeBlockDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    var dto = e.ToTimeBlockDTO();
                    if (dto != null)
                        dtos.Add(dto);
                }
            }
            return dtos;
        }

        public static TimeBlockDTO ToTimeBlockDTO(this TimeSchedulePlanningDayDTO e)
        {
            if (e == null)
                return null;

            return new TimeBlockDTO()
            {
                StartTime = e.StartTime,
                StopTime = e.StopTime,
            };
        }

        public static List<WorkIntervalDTO> GetWorkIntervals(this List<TimeSchedulePlanningDayDTO> l, bool loadGrossNetCost = false)
        {
            List<WorkIntervalDTO> workIntervals = new List<WorkIntervalDTO>();
            if (l.Count == 0)
                return workIntervals;

            foreach (var employeeGrouping in l.GroupBy(i => i.EmployeeId))
            {
                int employeeId = employeeGrouping.Key;

                foreach (var employeeDateGrouping in employeeGrouping.GroupBy(i => i.StartTime.Date))
                {
                    DateTime date = employeeDateGrouping.Key;
                    List<TimeSchedulePlanningDayDTO> shiftsByEmployeeAndDate = employeeDateGrouping.ToList();
                    List<BreakDTO> breaksByEmployeeAndDate = shiftsByEmployeeAndDate.GetBreaks(true);
                    List<int> usedBreaks = new List<int>();
                    foreach (TimeSchedulePlanningDayDTO shift in shiftsByEmployeeAndDate)
                    {
                        DateTime start = shift.StartTime;
                        DateTime stop = shift.StopTime;

                        bool restart;
                        do
                        {
                            restart = false;
                            foreach (BreakDTO brk in breaksByEmployeeAndDate.ToList())
                            {
                                if (usedBreaks.Contains(brk.Id) && start > brk.StartTime && start < brk.StopTime)
                                    start = brk.StopTime;

                                if (CalendarUtility.GetOverlappingMinutes(brk.StartTime, brk.StopTime, start, stop) > 0)
                                {
                                    if (brk.StartTime >= start)
                                    {
                                        // Break starts inside presence
                                        WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, start, brk.StartTime, grossNetCost: loadGrossNetCost && !shift.IsOnDuty() ? GrossNetCostDTO.Create(employeeId, shift.TimeScheduleTemplateBlockId, shift.TimeScheduleTypeId, shift.TimeDeviationCauseId, date) : null);
                                        if (workInterval.TotalMinutes != 0)
                                            workIntervals.Add(workInterval);
                                        start = brk.StopTime;
                                        restart = true;
                                        usedBreaks.Add(brk.Id);
                                        break;
                                    }
                                    else if (brk.StopTime < stop)
                                    {
                                        // Break ends inside presence
                                        WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, brk.StopTime, stop, grossNetCost: loadGrossNetCost && !shift.IsOnDuty() ? GrossNetCostDTO.Create(employeeId, shift.TimeScheduleTemplateBlockId, shift.TimeScheduleTypeId, shift.TimeDeviationCauseId, date) : null);
                                        if (workInterval.TotalMinutes != 0)
                                            workIntervals.Add(workInterval);
                                        start = stop;
                                        usedBreaks.Add(brk.Id);
                                    }
                                    else
                                    {
                                        // Break competely overlaps presence
                                        start = stop;
                                    }
                                }
                            }
                        } while (restart);

                        if (stop > start)
                        {
                            WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, start, stop, grossNetCost: loadGrossNetCost && !shift.IsOnDuty() ? GrossNetCostDTO.Create(employeeId, shift.TimeScheduleTemplateBlockId, shift.TimeScheduleTypeId, shift.TimeDeviationCauseId, date) : null);
                            if (workInterval.TotalMinutes != 0)
                                workIntervals.Add(workInterval);
                        }
                    }
                }
            }

            return workIntervals.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static List<WorkIntervalDTO> GetCoherentWorkIntervals(this List<TimeSchedulePlanningDayDTO> l, List<TimeScheduleTypeDTO> scheduleTypes)
        {
            List<WorkIntervalDTO> workIntervals = new List<WorkIntervalDTO>();


            foreach (var employeeShifts in l.GroupBy(i => i.EmployeeId))
            {
                int employeeId = employeeShifts.Key;

                List<List<TimeSchedulePlanningDayDTO>> coherentShiftGroups = employeeShifts.ToList().GetCoherentShifts();
                foreach (var coherentShifts in coherentShiftGroups)
                {
                    WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, coherentShifts.GetScheduleIn(), coherentShifts.GetScheduleOut());
                    workInterval.HasBilagaJ = coherentShifts.HasBilagaJScheduleType(scheduleTypes);
                    workIntervals.Add(workInterval);
                }
            }

            return workIntervals.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static List<BreakDTO> GetBreaks(this List<TimeSchedulePlanningDayDTO> l, bool includeGaps)
        {
            if (l.Count == 0)
                return new List<BreakDTO>();

            List<BreakDTO> breaks = l.First().GetBreaks();
            if (includeGaps)
                breaks.AddRange(l.GetGaps());
            return breaks.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static List<BreakDTO> GetGaps(this List<TimeSchedulePlanningDayDTO> l)
        {
            List<BreakDTO> gaps = new List<BreakDTO>();

            if (l.Count > 1)
            {
                for (int blockNr = 1; blockNr < l.Count; blockNr++)
                {
                    TimeSchedulePlanningDayDTO block = l[blockNr];
                    TimeSchedulePlanningDayDTO prevBlock = l[blockNr - 1];
                    int gapMinutes = Convert.ToInt32(block.StartTime.Subtract(prevBlock.StopTime).TotalMinutes);
                    if (gapMinutes > 1)
                    {
                        gaps.Add(new BreakDTO()
                        {
                            Id = 0,
                            TimeCodeId = 0,
                            StartTime = prevBlock.StopTime,
                            BreakMinutes = gapMinutes,
                            Link = null,
                        });
                    }
                }
            }

            return gaps;
        }

        public static List<Tuple<string, string>> GetLinkMappings(this List<TimeSchedulePlanningDayDTO> l)
        {
            List<Tuple<string, string>> linkMappings = new List<Tuple<string, string>>();
            var guids = l.Where(x => x.Link.HasValue).Select(x => x.Link.Value).ToList();
            foreach (var item in l)
            {
                guids.AddRange(item.GetBreaks().Where(x => x.Link.HasValue).Select(x => x.Link.Value).ToList());
            }
            foreach (var linkGroup in guids.GroupBy(g => g))
            {
                linkMappings.Add(Tuple.Create(linkGroup.Key.ToString(), Guid.NewGuid().ToString()));
            }

            return linkMappings;
        }

        public static List<TimeSchedulePlanningDayDTO> ExcludeEmployeeId(this List<TimeSchedulePlanningDayDTO> l, int excludeEmployeeId)
        {
            return l?.Where(x => x.EmployeeId != excludeEmployeeId).ToList();
        }

        public static List<int> GetEmployeeIds(this List<TimeSchedulePlanningDayDTO> l, int otherEmployeeId)
        {
            var employeeIds = l?.Where(e => e.EmployeeId > 0).Select(e => e.EmployeeId).Distinct().ToList() ?? new List<int>();
            if (!employeeIds.Contains(otherEmployeeId))
                employeeIds.Add(otherEmployeeId);
            return employeeIds;
        }

        public static List<DateTime> GetDates(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Select(i => i.ActualDate).ToList() ?? new List<DateTime>();
        }

        public static DateTime GetScheduleIn(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l?.OrderBy(i => i.StartTime).FirstOrDefault()?.StartTime ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime GetScheduleOut(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l?.OrderByDescending(i => i.StopTime).FirstOrDefault()?.StopTime ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime GetStartDate(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l?.OrderBy(s => s.StartTime.Date).FirstOrDefault()?.StartTime.Date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime GetStopDate(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l?.OrderBy(s => s.StartTime.Date).LastOrDefault()?.StartTime.Date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static bool HasAbsence(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => x.TimeDeviationCauseId.HasValue) ?? false;
        }

        public static bool IsPartTimeAbsence(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return !l.IsNullOrEmpty() && l.HasAbsence() && l.Count(x => x.TimeDeviationCauseId.HasValue) < l.Count();
        }

        public static bool IsWholeDayAbsence(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return !l.IsNullOrEmpty() && l.Count(x => x.TimeDeviationCauseId.HasValue) == l.Count();
        }

        public static bool IamInQueue(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => x.IamInQueue) ?? false;
        }

        public static bool UnWantedShiftsExists(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => x.ShiftUserStatus == TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted) ?? false;
        }

        public static bool AbsenceRequestedShiftsExists(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => x.ShiftUserStatus == TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested) ?? false;
        }

        public static bool WantedShiftsExists(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return (l?.Sum(x => x.NbrOfWantedInQueue) ?? 0) > 0;
        }

        public static bool HasDescription(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => !String.IsNullOrEmpty(x.Description)) ?? false;
        }

        public static bool HasOnDuty(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => x.IsOnDuty()) ?? false;
        }

        public static bool HasShiftRequest(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => x.HasShiftRequest) ?? false;
        }

        public static bool HasShiftRequestAnswer(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            return l?.Any(x => x.HasShiftRequestAnswer) ?? false;
        }

        public static List<IGrouping<Guid?, TimeSchedulePlanningDayDTO>> GetLinkedShifts(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l.GroupBy(x => x.Link).ToList();
        }

        public static int GetBreakLength(this TimeSchedulePlanningDayDTO e, DateTime shiftStart, DateTime shiftEnd)
        {
            int totalBreakMinutes = 0;
            //make sure shiftStart and shiftEnd has the same date "offset" as the breaks in TimeSchedulePlanningDayDTO
            DateTime start = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, shiftStart.TimeOfDay);
            DateTime stop = start.AddMinutes((shiftEnd - shiftStart).TotalMinutes);

            if (e.Break1Id != 0)
                totalBreakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(start, stop, e.Break1StartTime, e.Break1StartTime.AddMinutes(e.Break1Minutes)).TotalMinutes;
            if (e.Break2Id != 0)
                totalBreakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(start, stop, e.Break2StartTime, e.Break2StartTime.AddMinutes(e.Break2Minutes)).TotalMinutes;
            if (e.Break3Id != 0)
                totalBreakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(start, stop, e.Break3StartTime, e.Break3StartTime.AddMinutes(e.Break3Minutes)).TotalMinutes;
            if (e.Break4Id != 0)
                totalBreakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(start, stop, e.Break4StartTime, e.Break4StartTime.AddMinutes(e.Break4Minutes)).TotalMinutes;

            return totalBreakMinutes;
        }

        public static int GetWorkMinutes(this List<TimeSchedulePlanningDayDTO> l, List<TimeScheduleTypeDTO> scheduleTypes)
        {
            int totalBreakMinutes = 0;
            int totalWorkAndBreakMinutes = 0;

            foreach (IGrouping<DateTime, TimeSchedulePlanningDayDTO> shiftsGroupedByDate in l.GroupBy(g => g.StartTime.Date).ToList())
            {
                foreach (TimeSchedulePlanningDayDTO shift in shiftsGroupedByDate)
                {
                    if (shift.TimeScheduleTypeId != 0)
                    {
                        var scheduleType = scheduleTypes?.FirstOrDefault(x => x.TimeScheduleTypeId == shift.TimeScheduleTypeId);
                        if (scheduleType != null && scheduleType.IsNotScheduleTime)
                            continue;
                    }

                    int workMinutes = (int)(shift.StopTime - shift.StartTime).TotalMinutes;
                    totalWorkAndBreakMinutes += workMinutes;

                    int factorMinutes = shift.GetTimeScheduleTypeFactorsWithinShift();
                    totalWorkAndBreakMinutes += factorMinutes;

                    int breakMinutes = shift.GetBreakTimeWithinShift();
                    totalBreakMinutes += breakMinutes;
                }
            }

            return totalWorkAndBreakMinutes - totalBreakMinutes;
        }

        public static void SetUniqueId(this List<TimeSchedulePlanningDayDTO> l)
        {
            if (l.IsNullOrEmpty())
                return;

            foreach (var e in l)
            {
                if (e.UniqueId == null || e.UniqueId == Guid.Empty.ToString())
                    e.UniqueId = Guid.NewGuid().ToString();
            }
        }

        public static bool IsConsideredToBeSame(this TimeSchedulePlanningDayDTO e, TimeSchedulePlanningDayDTO other, List<string> disapproveUniqueIds = null)
        {
            if (e == null || other == null)
                return false;
            if (disapproveUniqueIds != null && disapproveUniqueIds.Contains(e.UniqueId))
                return false;
            return e.StartTime == other.StartTime || e.StopTime == other.StopTime;
        }

        public static void SetNewLinks(this List<TimeSchedulePlanningDayDTO> l)
        {
            foreach (TimeSchedulePlanningDayDTO e in l.Where(i => !i.Link.HasValue))
            {
                e.Link = Guid.NewGuid();
            }

            foreach (var group in l.GroupBy(i => i.Link))
            {
                var link = Guid.NewGuid();

                foreach (var e in group)
                {
                    e.Link = link;
                }
            }
        }

        public static bool IsAssociatedShiftLended(this List<TimeSchedulePlanningDayDTO> l, int breakId)
        {
            foreach (var shift in l.Where(x => x.IsLended))
            {
                var breaks = shift.GetMyBreaks();
                if (breaks.Any(x => x.Id == breakId))
                    return shift.IsLended;
            }
            return false;
        }

        public static bool HasBilagaJScheduleType(this List<TimeSchedulePlanningDayDTO> l, List<TimeScheduleTypeDTO> scheduleTypes)
        {
            if (l.Any(x => x.IsScheduleTypeBilagaJ(scheduleTypes)))
                return true;

            return false;
        }

        public static bool IsScheduleTypeBilagaJ(this TimeSchedulePlanningDayDTO e, List<TimeScheduleTypeDTO> scheduleTypes)
        {
            if (e.TimeScheduleTypeId != 0)
                return scheduleTypes.FirstOrDefault(x => x.TimeScheduleTypeId == e.TimeScheduleTypeId)?.IsBilagaJ ?? false;

            return false;
        }

        #endregion

        #region UserAttestRoleDTO

        public static UserAttestRoleDTO GetClosestAttestRole(this List<UserAttestRoleDTO> l, AttestRoleDTO attestRole, AccountDTO account, DateTime fromDate, DateTime? toDate, DateTime minDate, DateTime maxDate)
        {
            if (l == null || attestRole == null)
                return null;

            List<UserAttestRoleDTO> matchingAttestRoles = l.Where(a => a.AttestRoleId == attestRole.AttestRoleId && (account == null || account.AccountId == a.AccountId) && CalendarUtility.IsDatesOverlapping(fromDate, (toDate ?? maxDate), a.DateFrom ?? minDate, (a.DateTo.HasValue && a.DateTo.Value != CalendarUtility.DATETIME_DEFAULT ? a.DateTo.Value : maxDate))).ToList();

            UserAttestRoleDTO e = null;
            if (matchingAttestRoles.Count > 1)
                e = matchingAttestRoles.FirstOrDefault(i => !i.DateTo.HasValue);
            if (e == null)
                e = matchingAttestRoles.OrderByDescending(i => i.DateTo).FirstOrDefault();
            return e;
        }

        #endregion

        #region WorkIntervalDTO

        public static List<GrossNetCostDTO> GetGrossNetCosts(this List<WorkIntervalDTO> l)
        {
            return l?.Where(i => i.GrossNetCost != null).Select(i => i.GrossNetCost).ToList();
        }

        #endregion

        #endregion

        #region DataTypes

        #region Claim

        public static bool IsValidClaim(this Claim claim, out string value)
        {
            if (claim == null || claim.Value == "0")
            {
                value = null;
                return false;
            }
            value = claim.Value;
            return true;
        }
        public static bool TryGetInt(this ClaimsIdentity identity, string type, out int value)
        {
            value = 0;
            return identity?.FindFirst(type).TryGetInt(out value) ?? false;
        }
        public static bool TryGetInt(this Claim claim, out int intValue)
        {
            if (claim.IsValidClaim(out string value))
            {
                intValue = int.Parse(value);
                return true;
            }
            else
            {
                intValue = 0;
                return false;
            }
        }
        public static bool TryGetBool(this ClaimsIdentity identity, string type, out bool value)
        {
            value = false;
            return identity?.FindFirst(type).TryGetBool(out value) ?? false;
        }
        public static bool TryGetBool(this Claim claim, out bool boolValue)
        {
            if (claim.IsValidClaim(out string value))
            {
                boolValue = bool.Parse(value);
                return true;
            }
            else
            {
                boolValue = false;
                return false;
            }
        }
        public static bool TryGetString(this ClaimsIdentity identity, string type, out string value)
        {
            var claim = identity?.FindFirst(type);
            value = claim?.Value;
            return claim != null;
        }

        #endregion

        #region Generic

        public static T GetUniqueValue<T>(List<T> source, out bool hasMultiple)
        {
            int count = source.Distinct().Count();
            hasMultiple = count > 1;
            return count == 1 ? source[0] : default(T);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                    yield return element;
            }
        }

        public static bool TryAdd<T>(this List<T> collection, T item)
        {
            if (collection == null || item == null || collection.Contains(item))
                return false;

            collection.Add(item);
            return true;
        }

        #endregion

        #region Enum

        public static bool IsValidIn(this Enum value, params Enum[] validValues)
        {
            return validValues.Contains(value);
        }

        public static bool IsForeign(this SoeOriginStatusClassification classification)
        {
            return classification.ToString().EndsWith("Foreign");
        }

        public static T ParseToEnum<T>(this int value) where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value))
                return default(T);

            return (T)((object)value);
        }

        #endregion

        #region DateTime

        public static DateTime ToValueOrDefault(this DateTime? date)
        {
            return date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime ToValueOrToday(this DateTime? date)
        {
            return date ?? DateTime.Today;
        }

        public static DateTime RemoveSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
        }

        public static string ToShortDateTime(this DateTime dateTime)
        {
            string year = dateTime.Year.ToString();
            string month = AddZeroIfNeeded(dateTime.Month.ToString());
            string day = AddZeroIfNeeded(dateTime.Day.ToString());
            string hour = AddZeroIfNeeded(dateTime.Hour.ToString());
            string minute = AddZeroIfNeeded(dateTime.Minute.ToString());
            string value = year + "-" + month + "-" + day + "_" + hour + minute;
            return value;
        }

        public static string AddZeroIfNeeded(string value)
        {
            if (value.Length == 1)
                value = "0" + value;

            return value;
        }

        /// <summary>
        /// Round time to nearest minute of specified interval.
        /// If in middle, the lower one will be returned
        /// E.g. 2013-01-31 14:10, 30 will return 2013-01-31 14:00
        ///      2013-01-31 14:15, 30 will return 2013-01-31 14:00
        ///      2013-01-31 14:20, 30 will return 2013-01-31 14:30
        /// </summary>
        /// <param name="time">Time to round</param>
        /// <param name="interval">Interval in minutes</param>
        /// <returns>New rounded time</returns>
        public static DateTime RoundTime(this DateTime time, int interval)
        {
            if (interval == 0 || (time.TimeOfDay.TotalMinutes % interval == 0))
                return time;

            DateTime newTime = time.Date;
            DateTime prevTime = time.Date;
            while (newTime < time)
            {
                prevTime = newTime;
                newTime = newTime.AddMinutes(interval);
            }

            if ((newTime - time).TotalMinutes < (time - prevTime).TotalMinutes)
                return newTime;
            else
                return prevTime;
        }

        public static int RoundTime(this int minutes, int interval)
        {
            if (interval == 0 || (minutes % interval == 0))
                return minutes;

            int newTime = (minutes / interval) * interval;
            int prevTime = (minutes / interval) * interval;
            while (newTime < minutes)
            {
                prevTime = newTime;
                newTime = newTime += interval;
            }

            if (newTime - minutes < minutes - prevTime)
                return newTime;
            else
                return prevTime;
        }

        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            var modTicks = dt.Ticks % d.Ticks;
            var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static string ToShortDateString(this DateTime? dt)
        {
            return dt.HasValue ? dt.Value.ToShortDateString() : String.Empty;
        }

        public static string ToShortDateString(this DateTime time)
        {
            // ToShortTimeString() is not supported in a Portable Class Library.
            // The return value is identical to the value returned by specifying the "t" standard DateTime format string with the ToString(String) method.
            return time.ToString("d");
        }

        public static string ToShortTimeString(this DateTime? dt)
        {
            return dt.HasValue ? dt.Value.ToShortTimeString() : String.Empty;
        }

        public static string ToShortTimeString(this DateTime time)
        {
            // ToShortTimeString() is not supported in a Portable Class Library.
            // The return value is identical to the value returned by specifying the "t" standard DateTime format string with the ToString(String) method.
            return time.ToString("t");
        }

        public static DateTime Trim(this DateTime date, long ticks)
        {
            return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
        }

        public static DateTime GetLastMinuteOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddMinutes(-1);
        }

        public static List<Tuple<DateTime, DateTime>> GetCoherentDateRanges(this IEnumerable<DateTime> inputDates)
        {
            List<DateTime> dates = inputDates?.Distinct().OrderBy(d => d).ToList();
            if (dates.IsNullOrEmpty())
                return new List<Tuple<DateTime, DateTime>>();

            List<Tuple<DateTime, DateTime>> ranges = new List<Tuple<DateTime, DateTime>>();

            dates = dates.Distinct().ToList();
            if (dates.Count == 1)
            {
                ranges.Add(Tuple.Create(dates.First(), dates.First()));
            }
            else if (dates.Count > 1)
            {
                DateTime? rangeStart = null;
                DateTime dateFrom = dates.OrderBy(date => date.Date).First();
                DateTime dateTo = dates.OrderBy(date => date.Date).Last();
                DateTime currentDate = dateFrom;
                while (currentDate <= dateTo)
                {
                    if (dates.Any(date => date == currentDate))
                    {
                        //Start/continue range
                        if (!rangeStart.HasValue)
                            rangeStart = currentDate;
                    }
                    else if (rangeStart.HasValue)
                    {
                        //Close range
                        ranges.Add(Tuple.Create(rangeStart.Value, currentDate.AddDays(-1)));
                        rangeStart = null;
                    }

                    //Close last range
                    if (currentDate == dateTo && rangeStart.HasValue)
                        ranges.Add(Tuple.Create(rangeStart.Value, dateTo));

                    currentDate = currentDate.AddDays(1);
                }
            }

            return ranges;
        }

        public static string GetCohereDateRangesText(this IEnumerable<DateTime> inputDates, int? maxIntervals = null, string defaultTextIfMoreThanMax = null)
        {
            var ranges = inputDates.Select(i => i.Date).GetCoherentDateRanges();
            if (maxIntervals.HasValue && maxIntervals.Value < ranges.Count)
                return defaultTextIfMoreThanMax ?? $">{maxIntervals}";


            var rangeStrings = ranges.Select(range =>
                range.Item1 == range.Item2
                    ? range.Item1.ToShortDateString()
                    : $"{range.Item1.ToShortDateString()} - {range.Item2.ToShortDateString()}"
            );
            return string.Join(",", rangeStrings);
        }

        public static string GetCoherentDateRangeText(this IEnumerable<DateTime> inputDates, bool appendLinebreak = false)
        {
            if (inputDates.IsNullOrEmpty())
                return string.Empty;

            List<Tuple<DateTime, DateTime>> ranges = inputDates.GetCoherentDateRanges();
            if (ranges.IsNullOrEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var range in ranges)
            {
                if (sb.Length > 0)
                    sb.Append(appendLinebreak ? "\n" : ", ");
                if (range.Item1 == range.Item2)
                    sb.Append(range.Item1.ToShortDateString());
                else if (range.Item1 < range.Item2)
                    sb.Append($"{range.Item1.ToShortDateString()}-{range.Item2.ToShortDateString()}");
            }
            return sb.ToString();
        }

        public static string GetCoherentDateRangesDescription(this IEnumerable<DateTime> inputDates)
        {
            List<DateTime> dates = inputDates?.Distinct().OrderBy(date => date).ToList();
            if (dates.IsNullOrEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            if (dates?.Count == 1)
            {
                sb.Append(dates.First().Date.ToShortDateString());
            }
            else
            {
                List<Tuple<DateTime, DateTime>> dateRanges = dates.GetCoherentDateRanges();
                if (!dateRanges.IsNullOrEmpty())
                {
                    foreach (var dateRange in dateRanges.OrderBy(i => i.Item1))
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");

                        if (dateRange.Item1 == dateRange.Item2)
                            sb.Append(dateRange.Item1.ToShortDateString());
                        else
                            sb.Append($"{dateRange.Item1.ToShortDateString()} - {dateRange.Item2.ToShortDateString()}");
                    }
                }
            }

            return sb.ToString();
        }

        public static List<Tuple<DateTime, DateTime>> GetCoherentTimeRanges(this IEnumerable<DateTime> inputDateTimes, int interval)
        {
            List<Tuple<DateTime, DateTime>> ranges = new List<Tuple<DateTime, DateTime>>();

            List<DateTime> dateTimes = inputDateTimes?.Distinct().OrderBy(date => date).ToList();
            if (dateTimes?.Count == 1)
            {
                ranges.Add(Tuple.Create(dateTimes.First(), dateTimes.First().AddMinutes(interval)));
            }
            else if (dateTimes?.Count > 1)
            {
                DateTime? currentIntervalStart = null;
                DateTime? currentIntervalStop = null;
                foreach (DateTime time in dateTimes.OrderBy(time => time))
                {
                    if (!currentIntervalStart.HasValue)
                    {
                        //Open first
                        currentIntervalStart = time;
                    }
                    if (currentIntervalStop.HasValue && currentIntervalStop.Value < time)
                    {
                        //Out of range, close and start new (if not last)
                        ranges.Add(Tuple.Create(currentIntervalStart.Value, currentIntervalStop.Value));
                        currentIntervalStart = time;
                    }
                    if (time == dateTimes.Max())
                    {
                        //Last, closen and dont open new
                        ranges.Add(Tuple.Create(currentIntervalStart.Value, time.AddMinutes(interval)));
                    }

                    currentIntervalStop = time.AddMinutes(interval);
                }
            }

            return ranges;
        }

        public static List<DateTime> Exclude(this List<DateTime> l, List<DateTime> exclude)
        {
            return l?.Where(d => !exclude.Contains(d)).Distinct().ToList() ?? new List<DateTime>();
        }

        public static IEnumerable<DateRangeDTO> MergeOverlapping(this IEnumerable<DateRangeDTO> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    yield break;
                var previousInterval = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    var nextInterval = enumerator.Current;
                    if (!previousInterval.Overlaps(nextInterval))
                    {
                        yield return previousInterval;
                        previousInterval = nextInterval;
                    }
                    else
                    {
                        previousInterval = previousInterval.MergeWith(nextInterval);
                    }
                }
                yield return previousInterval;
            }
        }

        #endregion

        #region Int

        public static string ToCommaSeparated<T>(this IEnumerable<T> values, bool distinct = true, bool addWhiteSpace = false)
        {
            return StringUtility.GetCommaSeparatedString<T>(values, distinct: distinct, addWhiteSpace: addWhiteSpace);
        }

        public static bool ContainsAll(this List<int> l, params int[] values)
        {
            foreach (int e in l)
            {
                if (!values.Contains(e))
                    return false;
            }
            return true;
        }

        public static bool AreEqualTo(this List<int> list1, List<int> list2)
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null || list2 == null)
                return false;
            if (list1.Count != list2.Count)
                return false;

            return list1.SequenceEqual(list2);
        }

        public static bool IsEqualToAny(this int value, params int[] values)
        {
            return values != null && values.Contains(value);
        }

        public static bool IsEqualToAny(this IEnumerable<int> ids, params int[] values)
        {
            if (ids.IsNullOrEmpty() || values.IsNullOrEmpty())
                return false;

            foreach (var id in ids)
            {
                if (id.IsEqualToAny(values))
                    return true;
            }
            return false;
        }

        public static bool IsNullOrEmpty(this int? value)
        {
            return !value.HasValue || value.Value == 0;
        }

        public static bool HasValidValue(this int? value)
        {
            return value.HasValue && value.Value != 0;
        }

        public static Nullable<T> ToNullable<T>(this T value, T defaultValue = default(T)) where T : struct
        {
            if (value.Equals(defaultValue))
                return default(T);
            else
                return value;
        }

        public static int ToInt(this int? value)
        {
            return value ?? 0;
        }

        public static int? ToNullable(this int value)
        {
            return value > 0 ? value : (int?)null;
        }

        public static int FromNullable(this int? value)
        {
            return value ?? 0;
        }

        public static int? ToNullable(this int? value)
        {
            if (!value.HasValue)
                return null;

            return value.Value.ToNullable();
        }

        public static int ToInt(this bool value, int? trueValue = null)
        {
            return value ? (trueValue ?? 1) : 0;
        }

        public static int ToInt(this bool? value)
        {
            if (value == null)
                return 0;

            return value.Value ? 1 : 0;
        }

        public static string ToValueOrEmpty(this int? value, bool removeEmpty = true)
        {
            if (removeEmpty)
                return value.HasValue ? value.Value.ToString() : string.Empty;
            else
                return value.ToInt().ToString();
        }

        public static string ToValueOrNull(this int? value, bool removeEmpty = true)
        {
            if (removeEmpty)
                return value.HasValue ? value.Value.ToString() : null;
            else
                return value.ToInt().ToString();
        }

        #endregion

        #region Decimal

        public static decimal ToDecimal(this decimal? value)
        {
            return value ?? Decimal.Zero;
        }

        public static string ToValueOrEmpty(this decimal? value, bool removeEmpty = true, int? decimals = 0)
        {
            if (removeEmpty)
                return value.HasValue ? value.Value.ToString() : string.Empty;
            else
                return Math.Round(value.ToDecimal(), decimals.Value).ToString();
        }

        public static string ToValueOrNull(this decimal? value, bool removeEmpty = true, int? decimals = 0)
        {
            if (removeEmpty)
                return value.HasValue ? value.Value.ToString() : null;
            else
                return Math.Round(value.ToDecimal(), decimals.Value).ToString();
        }

        public static bool IsNullOrEmpty(this decimal? value)
        {
            return !value.HasValue || value.Value == 0;
        }

        #endregion

        #region IEnumerable

        public static string JoinToString<T>(this IEnumerable<T> collection, string separator = "")
        {
            if (collection.IsNullOrEmpty())
                return "";

            string value = "" + collection.First();
            foreach (T item in collection.Skip(1))
            {
                value += separator + item;
            }
            return value;
        }

        public static List<T> ToEmptyIfNull<T>(this IEnumerable<T> collection)
        {
            return collection?.ToList() ?? new List<T>();
        }

        public static List<T> ToNullIfEmpty<T>(this IEnumerable<T> collection)
        {
            return collection != null && !collection.Any() ? null : collection?.ToList();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        public static IEnumerable<List<T>> SplitList<T>(this List<T> list, int nSize = 50)
        {
            for (int i = 0; i < list.Count; i += nSize)
            {
                yield return list.GetRange(i, Math.Min(nSize, list.Count - i));
            }
        }

        #endregion

        #region List

        /// <summary>
        /// Convert a List of GenericTypes to a Dictionary
        /// </summary>
        /// <param name="list">The List</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<int, string> ToDictionary(this List<GenericType> list)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (var item in list)
            {
                dict.Add(item.Id, item.Name);
            }

            return dict;
        }

        public static List<T> ToNullable<T>(this List<T> list)
        {
            return !list.IsNullOrEmpty() ? list : null;
        }

        public static List<int> Values(this List<int?> list)
        {
            return list.Where(i => i.HasValue).Select(i => i.Value).ToList();
        }

        /// <summary>
        /// Removes all elements that occurs in the provided selection of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">List to remove from</param>
        /// <param name="items">Items to remove</param>
        /// <returns>The number of items that were removed</returns>
        public static int RemoveRange<T>(this ICollection<T> list, IEnumerable<T> items)
        {
            int count = 0;
            if (items != null)
            {
                foreach (T item in items.ToList())
                {
                    if (list.Remove(item))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static void AddIfNotNull(this List<int> l, int? value)
        {
            if (l != null && value.HasValue)
                l.Add(value.Value);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (T item in items)
                {
                    collection.Add(item);
                }
            }
        }

        public static bool ContainsAny<T>(this List<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (list.Contains(item))
                    return true;
            }
            return false;
        }

        public static bool ContainsAll<T>(this List<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                if (!list.Contains(item))
                    return false;
            }
            return true;
        }

        public static List<T> SkipAndTake<T>(this List<T> list, int skip, int take)
        {
            return list?.Skip(skip).Take(take).ToList() ?? new List<T>();
        }

        public static bool EqualsToDecimal(this string s, decimal d)
        {
            return !s.IsNullOrEmpty() && Decimal.TryParse(s, out decimal value) && Decimal.Equals(d, value);
        }

        public static bool EqualsToInt(this string s, int i)
        {
            return !s.IsNullOrEmpty() && Int32.TryParse(s, out int value) && Int32.Equals(i, value);
        }

        public static bool EqualsToStringIgnoreCase(this string s1, string s2)
        {
            return s1?.Equals(s2, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        #endregion

        #region String

        public static List<string> Trim(this IEnumerable<string> l)
        {
            if (l.IsNullOrEmpty())
                return new List<string>();

            var result = new List<string>();
            foreach (var s in l)
            {
                result.Add(s.Trim());
            }
            return result;
        }

        /// <summary>
        /// It is assumed that text is not already in html form.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToHTML(this string text)
        {
            foreach (KeyValuePair<string, string> conversion in HtmlConvertions)
            {
                text = text.Replace(conversion.Key, conversion.Value);
            }
            return text;
        }

        public static string ToValidFileName(this string filename, string replaceValue = "")
        {
            if (String.IsNullOrEmpty(filename))
                filename = Guid.NewGuid().ToString();

            List<string> notValidCharacters = new List<string>();
            notValidCharacters.Add("\\");
            notValidCharacters.Add("/");
            notValidCharacters.Add(":");
            notValidCharacters.Add("*");
            notValidCharacters.Add("?");
            notValidCharacters.Add("\"");
            notValidCharacters.Add("<");
            notValidCharacters.Add(">");
            notValidCharacters.Add("|");
            notValidCharacters.Add("\t");
            notValidCharacters.Add("\n");
            notValidCharacters.Add("\r");

            foreach (String character in notValidCharacters)
            {
                filename = filename.Replace(character, replaceValue);
            }

            return filename;
        }

        public static string NullToEmpty<T>(this T obj)
        {
            return obj != null ? obj.ToString() : String.Empty;
        }

        public static string EmptyToNull<T>(this T obj)
        {
            return !String.IsNullOrEmpty(obj?.ToString()) ? obj.ToString() : null;
        }

        public static bool HasValue(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static string Right(this string str, int length)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= length)
                return str;
            return str.Substring(str.Length - length);
        }

        public static string Left(this string str, int length)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= length)
                return str;

            return str.Substring(0, Math.Min(str.Length, length));
        }

        public static string SubstringToLengthOfString(this string str, int startIndex, int length = int.MaxValue)
        {
            return str.Substring(startIndex, str.Length - startIndex >= length ? length : str.Length - startIndex);
        }

        public static string SafeSubstring(this string text, int start, int length)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= start)
                return string.Empty;

            return text.Length - start <= length ? text.Substring(start) : text.Substring(start, length);
        }

        public static string ToNumeric(this string value)
        {
            return Regex.Replace(value, "\\D", "");
        }

        public static string AddLeadingZerosTruncate(this string value, int totalLength)
        {
            return AddLeadingZeros(value, totalLength).Truncate(totalLength);
        }

        public static string AddLeadingZeros(this string value, int totalLength)
        {
            return AddLeadingChars(value, totalLength, '0');
        }

        public static string RemoveLeadingZeros(string value)
        {
            return value.TrimStart('0');
        }

        public static string AddLeadingChars(this string value, int totalLength, char character)
        {
            if (value == null)
                throw new ArgumentNullException();

            return value.PadLeft(totalLength, character);
        }

        public static string AddTrailingBlanks(this string value, int totalLength)
        {
            return AddTrailingChars(value, totalLength, ' ');
        }

        public static string AddTrailingChars(this string value, int totalLength, char character)
        {
            if (value == null)
                throw new ArgumentNullException();

            return value.PadRight(totalLength, character);
        }

        public static string AddLeft(this string value, int charsToAdd, char character)
        {
            for (int i = 0; i < charsToAdd; i++)
            {
                value = character + value;
            }

            return value;
        }

        public static string Truncate(this string value, int maxNoChars, bool treatNullAsStringEmpty = false)
        {
            if (value == null)
            {
                if (treatNullAsStringEmpty)
                    return string.Empty;
                else
                    throw new ArgumentNullException();
            }

            if (value.Length > maxNoChars)
                return value.Substring(0, maxNoChars);

            return value;
        }

        /// <summary>
        /// Add more conversions here when needed.
        /// The conversions will be performed in order from top to bottom. 
        /// Make sure special characters are not inserted that will later be converted (e.g. inserting tags before "&lt; &gt;-signs ar converted)
        /// </summary>
        private static readonly List<KeyValuePair<string, string>> HtmlConvertions = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("&", "&amp;"),
            new KeyValuePair<string, string>("<", "&lt;"),
            new KeyValuePair<string, string>(">", "&gt;"),
            new KeyValuePair<string, string>("\r", ""),
            new KeyValuePair<string, string>("\n", "<br>")
        };

        public static string RemoveÅÄÖ(this string value)
        {
            value = value.Replace('Å', 'A');
            value = value.Replace('å', 'a');
            value = value.Replace('Ä', 'A');
            value = value.Replace('ä', 'a');
            value = value.Replace('Ö', 'O');
            value = value.Replace('ö', 'o');

            return value;
        }

        public static string RemoveWhiteSpaceAndHyphen(this string value)
        {
            return value.RemoveWhiteSpace('-');
        }

        public static string RemoveWhiteSpace(this string value, params char[] alsoRemove)
        {
            var hash = new HashSet<char>(new[] { ' ', '\t', '\n', '\r' });
            hash.AddRange(alsoRemove);
            return ExceptChars(value, hash);
        }

        public static string RemoveNewLine(this string value)
        {
            if (value == null)
                return string.Empty;

            value = value.Replace("\n", " "); //Replace with whitespace

            return value;
        }

        public static string RemoveTralingZeros(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            var count = value.Length;
            for (var i = value.Length - 1; i > 0; i--)
            {
                if (value[i] == '0')
                    count--;
                else
                    break;
            }
            return value.Substring(0, count);
        }

        public static string RemoveTags(this string value)
        {
            if (value == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            bool isInsideTag = false;
            foreach (char c in value)
            {
                if (c == '<')
                    isInsideTag = true;
                if (!isInsideTag)
                    sb.Append(c);
                if (c == '>')
                    isInsideTag = false;
            }
            return sb.ToString();
        }

        public static string StripNewLineAndHyphen(this string value)
        {
            if (value == null)
                return String.Empty;

            value = value.Replace("\r", " "); //Replace with whitespace
            value = value.Replace("\n", " "); //Replace with whitespace
            value = value.Replace("\"", ""); //Remove

            return value;
        }

        public static string ExceptChars(this string str, IEnumerable<char> toExclude)
        {
            if (String.IsNullOrEmpty(str))
                return String.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (!toExclude.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static string FirstCharToLowerCase(this string str)
        {
            string convStr = str;
            if (!string.IsNullOrEmpty(str) && !char.IsLower(str[0]))
            {
                convStr = char.ToLower(str[0]).ToString();
                if (str.Length > 1)
                    convStr += str.Substring(1);
            }

            return convStr;
        }

        public static string FirstCharToUpperCase(this string str)
        {
            string convStr = str;
            if (!string.IsNullOrEmpty(str) && !char.IsUpper(str[0]))
            {
                convStr = char.ToUpper(str[0]).ToString();
                if (str.Length > 1)
                    convStr += str.Substring(1);
            }

            return convStr;
        }

        public static string ReplaceDecimalSeparator(this string str)
        {
            CultureInfo currentLocale = CultureInfo.CurrentCulture;
            string from = currentLocale.IsEnglish() ? "," : ".";
            str = str.Replace(from, currentLocale.NumberFormat.NumberDecimalSeparator);
            return str;
        }

        #endregion

        #region Bool

        public static bool ExceedsThreshold(int threshold, params bool[] bools)
        {
            return bools.Count(b => b) > threshold;
        }

        public static InputLoadType ToLoadType(this bool value, InputLoadType loadType)
        {
            return value ? loadType : InputLoadType.None;
        }

        #endregion

        #region Dictionary

        public static Dictionary<string, object> HashtableToDictionary(this Hashtable hashtable)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (DictionaryEntry entry in hashtable)
            {
                dictionary.Add(entry.Key.ToString(), entry.Value);
            }
            return dictionary;
        }

        public static Hashtable DictionaryToHashtable(this Dictionary<string, object> dictionary)
        {
            var hashtable = new Hashtable();
            foreach (var entry in dictionary)
            {
                hashtable.Add(entry.Key, entry.Value);
            }
            return hashtable;
        }

        public static Dictionary<T, string> MergeDictionaries<T>(Dictionary<T, string> dict1, Dictionary<T, string> dict2)
        {
            foreach (var kvp in dict2)
            {
                if (!dict1.ContainsKey(kvp.Key))
                    dict1.Add(kvp.Key, kvp.Value);
            }
            return dict1;
        }

        public static Dictionary<int, string> Sort(this Dictionary<int, string> dict, bool ascending = true, bool sortByValue = true)
        {
            List<KeyValuePair<int, string>> list = dict.ToList();

            if (sortByValue)
            {
                if (ascending)
                    list.Sort((x, y) => x.Value.CompareTo(y.Value));
                else
                    list.Sort((x, y) => y.Value.CompareTo(x.Value));
            }
            else
            {
                if (ascending)
                    list.Sort((x, y) => x.Key.CompareTo(y.Key));
                else
                    list.Sort((x, y) => y.Key.CompareTo(x.Key));
            }

            return list.ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Convert a Dictionary to a list of SmallGenericTypes
        /// </summary>
        /// <param name="dict">The dictionary</param>
        /// <returns>List of SmallGenericTypes</returns>
        public static List<SmallGenericType> ToSmallGenericTypes(this IDictionary<int, string> dict)
        {
            List<SmallGenericType> smallList = new List<SmallGenericType>();
            foreach (var item in dict)
            {
                smallList.Add(new SmallGenericType()
                {
                    Id = item.Key,
                    Name = item.Value,
                });
            }
            return smallList;
        }

        /// <summary>
        /// Convert a list of GenericTypes to a list of SmallGenericTypes
        /// </summary>
        /// <param name="l">The List</param>
        /// <returns>List of SmallGenericTypes</returns>
        public static List<SmallGenericType> ToSmallGenericTypes(this List<GenericType> l)
        {
            List<SmallGenericType> result = new List<SmallGenericType>();
            foreach (var e in l)
            {
                var dto = e.ToSmallGenericType();
                if (dto != null)
                    result.Add(dto);
            }
            return result;
        }

        public static SmallGenericType ToSmallGenericType(this GenericType e)
        {
            if (e == null)
                return null;

            return new SmallGenericType()
            {
                Id = e.Id,
                Name = e.Name,
            };
        }

        public static List<GenericType> ToGenericTypes(this Dictionary<int, string> dict)
        {
            List<GenericType> genericList = new List<GenericType>();
            foreach (var item in dict)
            {
                genericList.Add(new GenericType()
                {
                    Id = item.Key,
                    Name = item.Value,
                });
            }
            return genericList;
        }

        public static List<TKey> GetKeys<TKey, TValue>(this Dictionary<TKey, TValue> dict, params Dictionary<TKey, TValue>[] others)
        {
            List<TKey> keys = dict?.Select(i => i.Key).ToList() ?? new List<TKey>();
            foreach (var other in others)
            {
                foreach (var pair in other)
                {
                    if (!keys.Contains(pair.Key))
                        keys.Add(pair.Key);
                }
            }

            return keys;
        }

        public static T GetValue<T>(this Dictionary<int, T> dict, int key) where T : struct
        {
            if (dict != null && dict.ContainsKey(key))
                return dict[key];
            return default(T);
        }

        public static int GetValue(this Dictionary<CompanySettingType, int> l, CompanySettingType settingType)
        {
            if (l == null || !l.ContainsKey(settingType))
                return 0;
            return l[settingType];
        }

        public static List<int> GetValues(this Dictionary<CompanySettingType, int> l, params CompanySettingType[] settingTypes)
        {
            List<int> values = new List<int>();
            foreach (CompanySettingType settingType in settingTypes)
            {
                values.Add(l.GetValue(settingType));

            }
            return values;
        }

        public static Dictionary<int, List<DateTime>> FilterValues(this Dictionary<int, List<DateTime>> dict, int key, List<DateTime> values)
        {
            if (dict.IsNullOrEmpty())
                return new Dictionary<int, List<DateTime>>();
            if (!dict.ContainsKey(key))
                return dict;

            if (values.IsNullOrEmpty())
                dict[key] = new List<DateTime>();
            else
                dict[key] = dict[key]?.Where(date => values.Contains(date)).ToList() ?? new List<DateTime>();

            return dict;
        }

        public static void AddTime<T>(this Dictionary<T, TimeSpan> dict, T key, TimeSpan value)
        {
            if (dict == null || key == null || value.TotalMinutes < 1)
                return;

            if (dict.ContainsKey(key))
                dict[key] = dict[key].Add(value);
            else
                dict.Add(key, value);
        }

        public static Dictionary<T, TimeSpan> AddTimes<T>(this Dictionary<T, TimeSpan> dict, Dictionary<T, TimeSpan> value)
        {
            if (value.IsNullOrEmpty())
                return dict;

            if (dict == null)
                dict = new Dictionary<T, TimeSpan>();

            foreach (var pair in value)
            {
                dict.AddTime(pair.Key, pair.Value);
            }

            return dict;
        }

        public static Dictionary<T, string> ToStringValueDict<T>(this Dictionary<T, TimeSpan> dict)
        {
            if (dict == null)
                return new Dictionary<T, string>();

            Dictionary<T, string> strDict = new Dictionary<T, string>();

            foreach (var pair in dict)
            {
                strDict.Add(pair.Key, CalendarUtility.GetHoursAndMinutesString((int)pair.Value.TotalMinutes));
            }

            return strDict;
        }

        public static List<int> GetKeysWithValue<T>(this Dictionary<int, List<T>> dict)
        {
            return dict.Where(i => i.Value != null && i.Value.Any()).Select(i => i.Key).ToList();
        }

        public static bool ContainsAnyValue<T>(this Dictionary<int, List<T>> dict, int key)
        {
            if (dict.IsNullOrEmpty())
                return false;
            if (!dict.ContainsKey(key))
                return false;
            return dict[key]?.Any() ?? false;
        }

        #endregion

        #region Exception

        public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
        {
            return GetInnerExceptions<Exception>(ex);
        }

        public static IEnumerable<T> GetInnerExceptions<T>(this Exception ex) where T : Exception
        {
            while (ex != null)
            {
                if (ex is T)
                    yield return (T)ex;

                ex = ex.InnerException;
            }
        }

        public static IEnumerable<string> GetInnerExceptionMessages(this Exception ex)
        {
            return GetInnerExceptions<Exception>(ex).Select(e => e.Message);
        }

        public static string GetExceptionMessage(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            if (ex?.Message != null)
                sb.Append(ex.Message);

            if (ex?.InnerException != null)
            {
                Exception innerException = ex.InnerException;
                while (innerException != null)
                {
                    sb.Append(innerException.Message);
                    innerException = innerException.InnerException;
                }
            }

            return sb.ToString();
        }

        #endregion

        #region TimeSpan

        public static TimeSpan AddMinutes(this TimeSpan? span, int minutes)
        {
            TimeSpan spanToAdd = CalendarUtility.MinutesToTimeSpan(minutes);
            return span.HasValue ? span.Value.Add(spanToAdd) : spanToAdd;
        }

        public static TimeSpan? Update(this TimeSpan? span, TimeSpan value)
        {
            if (value.TotalMinutes < 1)
                return span;

            return span?.Add(value) ?? value;
        }

        #endregion

        #region Type

        public static bool HasCleanerAttribute(this Type type)
        {
            if (type == null)
                return false;

            return type.GetCustomAttributes(typeof(CleanerAttribute), true).Length > 0;
        }

        public static bool HasLogAttribute(this Type type)
        {
            if (type == null)
                return false;

            return type.GetCustomAttributes(typeof(LogAttribute), true).Length > 0;
        }

        public static bool HasLogSocSecAttribute(this Type type)
        {
            if (type == null)
                return false;

            return type.GetProperties().Any(x => x.GetCustomAttributes(typeof(LogSocSecAttribute), false).Length > 0);
        }

        public static bool HasEmployeeIdAttribute(this Type type)
        {
            if (type == null)
                return false;

            return type.GetProperties().Any(x => x.GetCustomAttributes(typeof(LogEmployeeIdAttribute), false).Length > 0);
        }

        public static bool HasLogActorIdAttribute(this Type type)
        {
            if (type == null)
                return false;

            return type.GetProperties().Any(x => x.GetCustomAttributes(typeof(LogActorIdAttribute), false).Length > 0);
        }

        public static bool IsNonStringEnumerable(this Type type)
        {
            if (type == null || type == typeof(string))
                return false;

            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsGenericList(this Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
        }

        #endregion

        #region Object

        public static PropertyInfo[] GetProperties(this object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();

            // Sort by name
            Array.Sort(properties, delegate (PropertyInfo propInfo1, PropertyInfo propInfo2)
            {
                return propInfo1.Name.CompareTo(propInfo2.Name);
            });

            return properties;
        }

        public static PropertyInfo GetProperty(this object obj, string name)
        {
            var allproperties = GetProperties(obj);
            return !allproperties.IsNullOrEmpty() ? allproperties.FirstOrDefault(f => f.Name.ToLower() == name.ToLower()) : null;
        }

        public static object GetPropertyValue(this object obj, string name)
        {
            return GetProperty(obj, name)?.GetValue(obj);
        }

        public static bool IsNonStringEnumerable(this object obj)
        {
            if (obj == null)
                return false;

            return obj.GetType().IsNonStringEnumerable();
        }

        public static bool IsList(this object obj)
        {
            if (obj == null)
                return false;

            return obj is IList &&
                   obj.GetType().IsGenericType &&
                   obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsListOfType<T>(this object obj)
        {
            if (obj == null)
                return false;

            return obj is IList &&
                   obj.GetType().IsGenericType &&
                   obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<T>));
        }

        public static bool IsDictionary(this object obj)
        {
            if (obj == null)
                return false;

            return obj is IDictionary &&
                   obj.GetType().IsGenericType &&
                   obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }

        public static bool IsDateTime(this object obj)
        {
            if (obj == null)
                return false;

            return obj is DateTime || obj is DateTime?;
        }

        public static KeyValuePair<string, object> GetKeyValue(this object obj)
        {
            var propInfo = obj.GetProperties().ToList().GetKeyProperty();
            return new KeyValuePair<string, object>(propInfo?.Name, propInfo?.GetValue(obj, null));
        }

        public static int GetListCount(this object obj)
        {
            IList col = obj as IList;
            return col?.Count ?? 0;
        }

        public static int GetDictionaryCount(this object obj)
        {
            ICollection col = obj as ICollection;
            return col?.Count ?? 0;
        }

        #endregion

        #region PropertyInfo

        public static PropertyInfo GetKeyProperty(this List<PropertyInfo> propInfos)
        {
            return propInfos.FirstOrDefault(propInfo => propInfo.HasKeyAttribute());
        }

        public static bool HasClearFieldAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(ClearFieldAttribute), false).Any();
        }
        public static bool HasEmployeeLoggingAttribute(this PropertyInfo propInfo, List<TermGroup_PersonalDataInformationType> filterInformationTypes = null)
        {
            if (propInfo == null)
                return false;

            return (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.SocialSec)) && propInfo.HasLogSocSecAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.EmployeeMeeting)) && propInfo.HasLogEmployeeMeetingIdAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.ParentalLeaveAndChild)) && propInfo.HasLogParentalLeaveAndChildAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.IllnessInformation)) && propInfo.HasLogIllnessInformationAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.SalaryDistress)) && propInfo.HasLogSalaryDistressAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.Unionfee)) && propInfo.HasLogEmployeeUnionFeeAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.VehicleInformation)) && propInfo.HasLogVehiclenformationAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.Ecom)) && propInfo.HasLogEmployeeEcomAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.Address)) && propInfo.HasLogEmployeeAddressAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.ClosestRelative)) && propInfo.HasLogEmployeeClosestRelativeAttribute();
        }

        public static bool HasActorLoggingAttribute(this PropertyInfo propInfo, List<TermGroup_PersonalDataInformationType> filterInformationTypes = null)
        {
            if (propInfo == null)
                return false;

            return (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.PrivateCustomer)) && propInfo.HasLogPrivateCustomerAttribute() ||
                   (filterInformationTypes == null || filterInformationTypes.Contains(TermGroup_PersonalDataInformationType.PrivateSupplier)) && propInfo.HasLogPrivateSupplierAttribute();
        }

        public static bool HasEmployeeIdAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogEmployeeIdAttribute), false).Length > 0;
        }

        public static bool HasKeyAttribute(this PropertyInfo propInfo)
        {
            return false; // propInfo != null && propInfo.GetCustomAttributes(typeof(LogKeyAttribute), false).Any();
        }

        public static bool HasActorIdAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogActorIdAttribute), false).Any();
        }

        public static bool HasLogSocSecAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogSocSecAttribute), false).Any();
        }

        public static bool HasLogEmployeeMeetingIdAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogEmployeeMeetingIdAttribute), false).Any();
        }

        public static bool HasLogParentalLeaveAndChildAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogParentalLeaveAndChildAttribute), false).Any();
        }

        public static bool HasLogIllnessInformationAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogIllnessInformationAttribute), false).Any();
        }

        public static bool HasLogVehiclenformationAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogVehiclenformationAttribute), false).Any();
        }

        public static bool HasLogSalaryDistressAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogSalaryDistressAttribute), false).Any();
        }

        public static bool HasLogEmployeeUnionFeeAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogEmployeeUnionFeeAttribute), false).Any();
        }
        public static bool HasLogEmployeeClosestRelativeAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogEmployeeClosestRelativeAttribute), false).Any();
        }
        public static bool HasLogEmployeeAddressAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogEmployeeAddressAttribute), false).Any();
        }
        public static bool HasLogEmployeeEcomAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogEmployeeEcomAttribute), false).Any();
        }

        public static bool HasLogPrivateCustomerAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogPrivateCustomerAttribute), false).Any();
        }

        public static bool HasLogPrivateSupplierAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogPrivateSupplierAttribute), false).Any();
        }

        public static bool HasLogHouseholdDeductionApplicantIdAttribute(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.GetCustomAttributes(typeof(LogHouseholdDeductionApplicantIdAttribute), false).Any();
        }

        public static bool IsClass(this PropertyInfo propInfo)
        {
            return propInfo != null && !propInfo.PropertyType.IsValueType && propInfo.PropertyType.IsClass && propInfo.PropertyType != typeof(string);
        }

        public static bool IsList(this PropertyInfo propInfo)
        {
            return propInfo != null && !propInfo.PropertyType.IsValueType && propInfo.IsNonStringEnumerable();
        }

        public static bool IsNonStringEnumerable(this PropertyInfo propInfo)
        {
            return propInfo != null && propInfo.PropertyType.IsNonStringEnumerable();
        }

        public static TermGroup_PersonalDataInformationType GetPersonalDataInformationType(this PropertyInfo propInfo)
        {
            if (propInfo != null)
            {
                if (propInfo.HasLogEmployeeMeetingIdAttribute())
                    return TermGroup_PersonalDataInformationType.EmployeeMeeting;
                if (propInfo.HasLogIllnessInformationAttribute())
                    return TermGroup_PersonalDataInformationType.IllnessInformation;
                if (propInfo.HasLogParentalLeaveAndChildAttribute())
                    return TermGroup_PersonalDataInformationType.ParentalLeaveAndChild;
                if (propInfo.HasLogSalaryDistressAttribute())
                    return TermGroup_PersonalDataInformationType.SalaryDistress;
                if (propInfo.HasLogSocSecAttribute())
                    return TermGroup_PersonalDataInformationType.SocialSec;
                if (propInfo.HasLogEmployeeUnionFeeAttribute())
                    return TermGroup_PersonalDataInformationType.Unionfee;
                if (propInfo.HasLogVehiclenformationAttribute())
                    return TermGroup_PersonalDataInformationType.VehicleInformation;
                if (propInfo.HasLogHouseholdDeductionApplicantIdAttribute())
                    return TermGroup_PersonalDataInformationType.HouseholdDeduction;
            }
            return TermGroup_PersonalDataInformationType.Unspecified;
        }

        #endregion

        #region WildCard

        public static bool Compare(this WildCard wildCard, decimal leftValue, decimal rightValue)
        {
            switch (wildCard)
            {
                case WildCard.LessThan:
                    return (leftValue < rightValue);
                case WildCard.LessThanOrEquals:
                    return (leftValue <= rightValue);
                case WildCard.Equals:
                    return (leftValue == rightValue);
                case WildCard.GreaterThanOrEquals:
                    return (leftValue >= rightValue);
                case WildCard.GreaterThan:
                    return (leftValue > rightValue);
                case WildCard.NotEquals:
                    return (leftValue == rightValue);

            }

            return false;
        }

        #endregion

        #region CultureInfo

        public static bool IsEnglish(this CultureInfo info)
        {
            return info?.Parent != null && info.Parent.Name == "en";
        }

        #endregion

        #endregion
    }
}
