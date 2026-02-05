using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeAccount : IEmployeeAuthModel, ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region EmployeeAccount

        public static EmployeeAccountDTO ToDTO(this EmployeeAccount e)
        {
            if (e == null)
                return null;

            EmployeeAccountDTO dto = new EmployeeAccountDTO()
            {
                EmployeeAccountId = e.EmployeeAccountId,
                ParentEmployeeAccountId = e.ParentEmployeeAccountId,
                EmployeeId = e.EmployeeId,
                AccountId = e.AccountId,
                Default = e.Default,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                AddedOtherEmployeeAccount = e.AddedOtherEmployeeAccount,
                MainAllocation = e.MainAllocation,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            // Children
            if (e.Children != null)
            {
                dto.Children = new List<EmployeeAccountDTO>();
                foreach (EmployeeAccount child in e.Children.Where(a => a.State == (int)SoeEntityState.Active).ToList())
                {
                    dto.Children.Add(child.ToDTO());
                }
            }

            return dto;
        }

        public static IEnumerable<EmployeeAccountDTO> ToDTOs(this IEnumerable<EmployeeAccount> l)
        {
            var dtos = new List<EmployeeAccountDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static Dictionary<int, List<EmployeeAccount>> ToDict(this IEnumerable<EmployeeAccount> l)
        {
            Dictionary<int, List<EmployeeAccount>> dict = new Dictionary<int, List<EmployeeAccount>>();
            if (l != null)
            {
                foreach (var grouping in l.GroupBy(g => g.EmployeeId))
                    dict.Add(grouping.Key, grouping.ToList());
            }
            return dict;
        }

        public static List<EmployeeAccount> GetEmployeeAccounts(this IEnumerable<EmployeeAccount> l, DateTime? dateFrom, DateTime? dateTo)
        {
            if (l.IsNullOrEmpty())
                return new List<EmployeeAccount>();

            CalendarUtility.MinAndMaxToNull(ref dateFrom, ref dateTo);
            if (!dateFrom.HasValue && !dateTo.HasValue)
                return l.ToList();

            CalendarUtility.NullToToday(ref dateFrom, ref dateTo);
            List<EmployeeAccount> filtered = l.Where(i => !i.DateTo.HasValue || i.DateTo.Value >= dateFrom).OrderBy(i => i.DateFrom).ToList();
            if (filtered.IsNullOrEmpty())
                return new List<EmployeeAccount>();

            dateFrom = CalendarUtility.GetLatestDate(dateFrom.Value, filtered.First().DateFrom);

            return (from e in filtered
                    where e.State == (int)SoeEntityState.Active &&
                    CalendarUtility.IsDatesOverlapping(dateFrom.Value, dateTo.Value, e.DateFrom, e.DateTo ?? DateTime.MaxValue, validateDatesAreTouching: true)
                    orderby e.DateFrom
                    select e).ToList();
        }

        public static List<EmployeeAccount> GetEmployeeAccounts(this IEnumerable<EmployeeAccount> l, DateTime? date, bool discardDateIfEmpty = false, List<int> accountIds = null)
        {
            if (l.IsNullOrEmpty())
                return new List<EmployeeAccount>();

            if (!date.HasValue)
            {
                if (discardDateIfEmpty)
                    return l.ToList();

                date = DateTime.Today;
            }

            date = date.Value.Date;

            return (from e in l
                    where e.DateFrom.Date <= date &&
                    (!e.DateTo.HasValue || e.DateTo.Value.Date >= date) &&
                    (accountIds == null || accountIds.Contains(e.AccountId ?? 0)) &&
                    e.State == (int)SoeEntityState.Active
                    orderby e.DateFrom
                    select e).ToList();
        }

        public static List<EmployeeAccountDTO> GetEmployeeAccounts(this IEnumerable<EmployeeAccountDTO> l, DateTime? date, bool discardDateIfEmpty = false)
        {
            if (l.IsNullOrEmpty())
                return new List<EmployeeAccountDTO>();

            if (!date.HasValue)
            {
                if (discardDateIfEmpty)
                    return l.ToList();

                date = DateTime.Today;
            }

            date = date.Value.Date;

            return (from e in l
                    where e.DateFrom.Date <= date &&
                    (!e.DateTo.HasValue || e.DateTo.Value.Date >= date) &&
                    e.State == (int)SoeEntityState.Active
                    orderby e.DateFrom
                    select e).ToList();
        }

        public static List<EmployeeAccount> GetEmployeeAccountsByAccount(this IEnumerable<EmployeeAccount> l, int employeeId, int? accountId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultAccounts = false)
        {
            return l?
                .Where(i => i.EmployeeId == employeeId && i.State == (int)SoeEntityState.Active && (!accountId.HasValue || accountId.Value == i.AccountId) && (!onlyDefaultAccounts || i.Default))
                .GetEmployeeAccounts(dateFrom, dateTo) ?? new List<EmployeeAccount>();
        }

        public static List<EmployeeAccount> GetEmployeeAccountsByAccount(this IEnumerable<EmployeeAccount> l, int employeeId, int? accountId, DateTime? date = null, bool discardDateIfEmpty = false, bool onlyDefaultAccounts = false)
        {
            return l?
                .Where(i => i.EmployeeId == employeeId && i.State == (int)SoeEntityState.Active && (!accountId.HasValue || accountId.Value == i.AccountId) && (!onlyDefaultAccounts || i.Default))
                .GetEmployeeAccounts(date, discardDateIfEmpty) ?? new List<EmployeeAccount>();
        }

        public static List<EmployeeAccount> GetEmployeeAccountsByAccounts(this IEnumerable<EmployeeAccount> l, int employeeId, List<int> accountIds, DateTime dateFrom, DateTime dateTo, bool discardDateIfEmpty = false, bool onlyDefaultAccounts = false)
        {
            return l?
                .Where(i => i.EmployeeId == employeeId && i.State == (int)SoeEntityState.Active && i.AccountId.HasValue && accountIds.Contains(i.AccountId.Value) && (!onlyDefaultAccounts || i.Default))
                .GetEmployeeAccounts(dateFrom, dateTo) ?? new List<EmployeeAccount>();
        }

        public static List<EmployeeAccount> GetParentsWithAccountIds(this List<EmployeeAccount> l)
        {
            return l.Where(i => i.AccountId.HasValue && !i.ParentEmployeeAccountId.HasValue).ToList();
        }

        public static List<EmployeeAccount> GetChildrensWithAccountId(this List<EmployeeAccount> l, EmployeeAccount parent)
        {
            return l.Where(i => i.AccountId.HasValue && i.ParentEmployeeAccountId == parent.EmployeeAccountId).ToList();
        }

        public static List<AccountDTO> GetValidAccounts(this List<EmployeeAccount> l, 
            int employeeId, 
            DateTime startDate, 
            DateTime stopDate, 
            List<AccountDTO> allAccounts, 
            List<AccountDTO> userPermittedAccounts,
            bool onlyDefaultAccounts = false, 
            string showAllUnderAccountHierarchyId = ""
            )
        {
            return l.GetValidAccounts(employeeId, startDate, stopDate, allAccounts.ToDict(), userPermittedAccounts.ToDict(), onlyDefaultAccounts, showAllUnderAccountHierarchyId);
        }

        public static List<AccountDTO> GetValidAccounts(this List<EmployeeAccount> l, 
            int employeeId, 
            DateTime startDate, 
            DateTime stopDate, 
            Dictionary<int, AccountDTO> allAccounts, 
            Dictionary<int, AccountDTO> userPermittedAccounts, 
            bool onlyDefaultAccounts = false, 
            string showAllUnderAccountHierarchyId = ""
            )
        {
            List<AccountDTO> validAccounts = new List<AccountDTO>();

            if (l.IsNullOrEmpty() || userPermittedAccounts.IsNullOrEmpty())
                return validAccounts;

            List<EmployeeAccount> employeeAccounts = l.GetEmployeeAccountsByAccount(employeeId, null, startDate, stopDate, onlyDefaultAccounts: onlyDefaultAccounts);
            if (employeeAccounts.IsNullOrEmpty())
                return validAccounts;

            bool useAbstractAccounts = employeeAccounts.Any(e => e.ParentEmployeeAccountId.HasValue);

            List<EmployeeAccount> parentEmployeeAccounts = employeeAccounts.GetParentsWithAccountIds();
            if (parentEmployeeAccounts.IsNullOrEmpty())
                return validAccounts;

            bool showAllAccountsUnderHierarchyId = !String.IsNullOrEmpty(showAllUnderAccountHierarchyId) && showAllUnderAccountHierarchyId != "0" && !allAccounts.IsNullOrEmpty();

            foreach (EmployeeAccount parent in parentEmployeeAccounts)
            {
                if (!IsEmployeeAccountValid(parent, out AccountDTO parentAccount, true))
                    continue;

                List<EmployeeAccount> childs = employeeAccounts.GetChildrensWithAccountId(parent);
                if (childs.IsNullOrEmpty())
                {
                    TryAddParentAccount(parentAccount);
                }
                else
                {
                    foreach (EmployeeAccount child in childs)
                    {
                        if (!IsEmployeeAccountValid(child, out AccountDTO childAccount, false, parentAccount))
                            continue;

                        List<EmployeeAccount> grandChilds = childAccount != null ? employeeAccounts.GetChildrensWithAccountId(child) : null;
                        if (grandChilds.IsNullOrEmpty())
                        {
                            TryAddValidAccount(parentAccount, childAccount);
                        }
                        else
                        {
                            foreach (EmployeeAccount grandChild in grandChilds) //NOSONAR
                            {
                                if (IsEmployeeAccountValid(grandChild, out AccountDTO grandChildAccount, true, parentAccount, childAccount))
                                    TryAddValidAccount(childAccount, grandChildAccount);
                            }
                        }
                    }
                }
            }

            bool IsEmployeeAccountValid(EmployeeAccount employeeAccount, out AccountDTO account, bool validateAccount, params AccountDTO[] parentAccounts)
            {
                bool valid = employeeAccount.IsValid(startDate, stopDate, allAccounts);
                account = valid ? userPermittedAccounts.GetAccountAndResetHierarchy(employeeAccount.AccountId.Value, parentAccounts) : null;
                if (account != null && account.IsAbstract && !useAbstractAccounts)
                    account = null;
                if (valid && validateAccount && account == null)
                    valid = false;
                return valid;
            }
            void TryAddParentAccount(AccountDTO parentAccount)
            {
                if (parentAccount != null && !parentAccount.IsAbstract)
                    validAccounts.Add(parentAccount);
            }
            void TryAddValidAccount(AccountDTO parentAccount, AccountDTO childAccount)
            {
                if (childAccount != null && (!childAccount.ParentAccountId.HasValue || childAccount.ParentAccountId.Value == parentAccount.AccountId))
                {
                    if (!childAccount.IsAbstract)
                        validAccounts.Add(childAccount);
                }
                else if (showAllAccountsUnderHierarchyId && allAccounts.GetAccountHierarchyId(parentAccount, childAccount).StartsWith(showAllUnderAccountHierarchyId))
                    validAccounts.Add(parentAccount); //Valid thru it has account under current account and attestrole has showAll flag
            }

            return validAccounts;
        }

        public static List<DateTime> GetValidDates(this List<EmployeeAccount> l, int employeeId, List<int> accountIds, DateTime dateFrom, DateTime dateTo)
        {
            List<DateTime> validDates = new List<DateTime>();

            List<EmployeeAccount> employeeAccountsForEmployee = l?.Where(i => i.EmployeeId == employeeId).ToList();
            if (employeeAccountsForEmployee.IsNullOrEmpty())
                return validDates;

            DateTime currentDate = dateFrom;
            while (currentDate <= dateTo)
            {
                if (employeeAccountsForEmployee.GetEmployeeAccountsByAccounts(employeeId, accountIds, currentDate, currentDate).Any())
                    validDates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }

            return validDates;
        }

        public static List<string> GetAccountNames(this List<EmployeeAccount> l, List<AccountDTO> allAccounts, DateTime startDate, DateTime stopDate)
        {
            List<string> accountNames = new List<string>();

            List<EmployeeAccount> parentEmployeeAccounts = l.GetParentsWithAccountIds();
            foreach (EmployeeAccount parent in parentEmployeeAccounts)
            {
                if (!IsEmployeeAccountValid(parent, out AccountDTO parentAccount, true))
                    continue;

                List<EmployeeAccount> childs = l.GetChildrensWithAccountId(parent);
                if (childs.IsNullOrEmpty())
                {
                    TryAddAccountName(GetAccountNames(parentAccount), GetIsAllDefault(parent));
                }
                else
                {
                    foreach (EmployeeAccount child in childs)
                    {
                        if (!IsEmployeeAccountValid(child, out AccountDTO childAccount, true))
                            continue;

                        List<EmployeeAccount> grandChilds = childAccount != null ? l.GetChildrensWithAccountId(child) : null;
                        if (grandChilds.IsNullOrEmpty())
                        {
                            TryAddAccountName(GetAccountNames(parentAccount, childAccount), GetIsAllDefault(parent, child));
                        }
                        else
                        {
                            foreach (EmployeeAccount grandChild in grandChilds) //NOSONAR
                            {
                                if (IsEmployeeAccountValid(grandChild, out AccountDTO grandChildAccount, true))
                                    TryAddAccountName(GetAccountNames(parentAccount, childAccount, grandChildAccount), GetIsAllDefault(parent, child, grandChild));
                            }
                        }
                    }
                }

                bool IsEmployeeAccountValid(EmployeeAccount employeeAccount, out AccountDTO account, bool validateAccount)
                {
                    bool valid = employeeAccount.IsValid(startDate, stopDate, allAccounts);
                    account = valid ? allAccounts.FirstOrDefault(i => i.AccountId == employeeAccount.AccountId.Value) : null;
                    if (valid && validateAccount && account == null)
                        valid = false;
                    return valid;
                }
                void TryAddAccountName(string names, string isAllDefault)
                {
                    string accountName = $"{names} {isAllDefault}".Trim();
                    if (!accountNames.Contains(accountName))
                        accountNames.Add(accountName);
                }
                string GetAccountNames(params AccountDTO[] accounts)
                {
                    return accounts.GetAccountNames();
                }
                string GetIsAllDefault(params EmployeeAccount[] employeeAccounts)
                {
                    List<bool> defaults = employeeAccounts?.Select(a => a.Default).ToList() ?? new List<bool>();
                    return !defaults.IsNullOrEmpty() && defaults.All(d => d) ? "(*)" : string.Empty;
                }
            }

            return accountNames;
        }

        public static bool HasAnyValidAccount(
            this List<EmployeeAccount> l, 
            int employeeId, 
            DateTime startDate, 
            DateTime stopDate, 
            List<int> filteredAccountIds, 
            List<AccountDTO> possibleValidAccounts, 
            List<AccountDimDTO> allAccountDims, 
            List<AccountDTO> allAccountInternals, 
            bool onlyDefaultAccounts = false,
            bool doNotValidateAsHiearchy = false
            )
        {
            List<EmployeeAccount> employeeAccountsForEmployee = l.GetEmployeeAccountsByAccount(employeeId, null, startDate, stopDate, onlyDefaultAccounts: onlyDefaultAccounts);
            if (employeeAccountsForEmployee.IsNullOrEmpty())
                return false;

            List<EmployeeAccount> employeeAccountParents = employeeAccountsForEmployee.GetParentsWithAccountIds();
            if (employeeAccountParents.IsNullOrEmpty())
                return false;

            if (employeeAccountParents.Count < employeeAccountsForEmployee.Count)
                doNotValidateAsHiearchy = false; //Must be flat to validate without hierarchy

            bool hasFilter = !filteredAccountIds.IsNullOrEmpty();
            List<int> filteredAccountDimIds = allAccountInternals.Where(i => !hasFilter || filteredAccountIds.Contains(i.AccountId)).Select(i => i.AccountDimId).Distinct().ToList();
            List<AccountDTO> filteredAccounts = hasFilter ? allAccountInternals.Where(a => filteredAccountIds.Contains(a.AccountId)).ToList() : null;
            List<AccountDimDTO> filteredAccountDims = allAccountDims.Where(i => filteredAccountDimIds.Contains(i.AccountDimId)).ToList();
            List<int> filteredDimLevels = filteredAccountDims.Select(i => i.Level).ToList();

            if (hasFilter && filteredAccountDimIds.Count >= 1 && doNotValidateAsHiearchy)
                return HasValidAccountInEachSelectedDimension();
            else
                return HasValidAccountHiearchy();

            bool HasValidAccountHiearchy()
            {
                foreach (var employeeAccountParent in employeeAccountParents)
                {
                    if (!employeeAccountParent.IsDateValid(startDate, stopDate))
                        continue;

                    AccountDTO parentAccount = possibleValidAccounts.FirstOrDefault(i => i.AccountId == employeeAccountParent.AccountId.Value);
                    if (parentAccount == null)
                        continue;

                    AccountDimDTO parentAccountDim = allAccountDims.FirstOrDefault(i => i.AccountDimId == parentAccount.AccountDimId);
                    if (parentAccountDim == null)
                        continue;

                    bool isHigherParentFiltered = filteredDimLevels.Any() && filteredDimLevels.Max() < parentAccountDim.Level;
                    bool isParentAccountDimFiltered = filteredAccountDimIds.Contains(parentAccountDim.AccountDimId);
                    bool isParentAccountFiltered = filteredAccountIds.Contains(parentAccount.AccountId);

                    if (isHigherParentFiltered)
                    {
                        bool existsInAllFilteredDims = true;
                        foreach (AccountDimDTO filteredAccountDim in filteredAccountDims.OrderBy(ad => ad.Level))
                        {
                            List<int> filteredAccountsIdsByDim = filteredAccounts.Where(a => a.AccountDimId == filteredAccountDim.AccountDimId).Select(a => a.AccountId).ToList();
                            if (!parentAccount.DoParentHierachyContainsAny(filteredAccountsIdsByDim))
                            {
                                existsInAllFilteredDims = false;
                                break;
                            }
                        }
                        if (existsInAllFilteredDims)
                            return true;
                    }
                    else if (isParentAccountFiltered || !isParentAccountDimFiltered)
                    {
                        List<EmployeeAccount> employeeAccountChildren = employeeAccountsForEmployee.GetChildrensWithAccountId(employeeAccountParent);
                        if (employeeAccountChildren.IsNullOrEmpty())
                            return true;

                        foreach (EmployeeAccount employeeAccountChild in employeeAccountChildren)
                        {
                            AccountDTO childAccount = possibleValidAccounts.FirstOrDefault(i => i.AccountId == employeeAccountChild.AccountId.Value);
                            if (childAccount == null)
                                continue;

                            AccountDimDTO childAccountDim = allAccountDims.FirstOrDefault(i => i.AccountDimId == childAccount.AccountDimId);
                            if (childAccountDim == null)
                                continue;

                            bool isLowerChildFiltered = filteredDimLevels.Max() > childAccountDim.Level;
                            bool isChildAccountDimFiltered = filteredAccountDimIds.Contains(childAccountDim.AccountDimId);
                            bool isChildAccountFiltered = filteredAccountIds.Contains(childAccount.AccountId);

                            if ((isChildAccountFiltered && employeeAccountChild.IsDateValid(startDate, stopDate)) || !isChildAccountDimFiltered)
                            {
                                if (!isLowerChildFiltered)
                                    return true;

                                List<AccountDTO> childrenAccounts = childAccount.GetChildrens(possibleValidAccounts, oneLevel: false);
                                foreach (AccountDTO childrenAccount in childrenAccounts)
                                {
                                    if (filteredAccountIds.Contains(childrenAccount.AccountId))
                                        return true;
                                }
                            }

                        }
                    }
                }

                return false;
            }
            bool HasValidAccountInEachSelectedDimension()
            {
                foreach (var accountDim in filteredAccountDims)
                {
                    if (accountDim.Level == 1)
                        continue;

                    var filteredAccountsOnDim = filteredAccounts
                        .Where(i => i.AccountDimId == accountDim.AccountDimId)
                        .Select(a => a.AccountId)
                        .ToList();

                    if (filteredAccountsOnDim.IsNullOrEmpty())
                        return false;

                    bool hasValidAccount = employeeAccountParents
                        .Where(employeeAccountParent => employeeAccountParent.IsDateValid(startDate, stopDate))
                        .Any(employeeAccountParent => filteredAccountsOnDim.Contains(employeeAccountParent.AccountId.Value));

                    if (!hasValidAccount)
                        return false;
                }

                return true;
            }
        }

        public static bool ContainsAny(this List<EmployeeAccount> l, List<EmployeeAccount> otherEmployeeAccounts)
        {
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (otherEmployeeAccounts.Any(i => !i.ParentEmployeeAccountId.HasValue && i.AccountId == e.AccountId.Value))
                        return true;
                }
            }
            return false;
        }

        public static bool IsValid(this EmployeeAccount e, DateTime dateFrom, DateTime dateTo, Dictionary<int, AccountDTO> allAccounts)
        {
            return
                e?.AccountId != null &&
                e.IsDateValid(dateFrom, dateTo) &&
                allAccounts.GetValue(e.AccountId.Value)?.State != SoeEntityState.Deleted;
        }

        public static bool IsValid(this EmployeeAccount e, DateTime dateFrom, DateTime dateTo, List<AccountDTO> allAccounts)
        {
            return
                e?.AccountId != null &&
                e.IsDateValid(dateFrom, dateTo) &&
                allAccounts?.FirstOrDefault(a => a.AccountId == e.AccountId.Value)?.State != SoeEntityState.Deleted;
        }

        public static bool IsDateValid(this EmployeeAccount e, DateTime dateFrom, DateTime dateTo)
        {
            return CalendarUtility.IsDatesOverlappingNullable(dateFrom, dateTo, e.DateFrom, e.DateTo, true);
        }

        public static bool IsAccountValid(this EmployeeAccount e, int accountId)
        {
            return e.AccountId == accountId || e.Parent?.AccountId == accountId;
        }

        public static bool IsDateValid(this EmployeeAccount e, DateTime date)
        {
            return e.DateFrom <= date && (!e.DateTo.HasValue || e.DateTo.Value >= date);
        }

        #endregion
    }
}
