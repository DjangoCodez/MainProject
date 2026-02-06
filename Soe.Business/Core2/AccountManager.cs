using SoftOne.Soe.Business.Core.Interfaces;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;
using Account = SoftOne.Soe.Data.Account;

namespace SoftOne.Soe.Business.Core
{
    public class AccountManager : ManagerBase, IAccountManager
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Ctor

        public AccountManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion


        #region AccountDimsFromCache

        public void FlushAccountDimsFromCache(int actorCompanyId)
        {
            base.FlushAccountDimsFromCache(CacheConfig.Company(actorCompanyId));
        }

        #endregion

        #region Account

        public List<Account> GetAccounts(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccounts(entities, actorCompanyId);
        }

        public List<Account> GetAccounts(CompEntities entities, int actorCompanyId)
        {
            return (from a in entities.Account
                    .Include("AccountInternal")
                    where a.ActorCompanyId == actorCompanyId
                    select a).ToList();
        }

        public List<Account> GetAccounts(int actorCompanyId, int accountDimId, int accountYearId, bool loadAccountDim = false, bool loadInternalAccounts = false, bool loadBalance = false, bool onlyActive = true, bool setExtensionNames = false, bool setLinkedToShiftType = false, bool getCategories = false, bool setParentAccount = false, bool ignoreHierarchyOnly = false, int? accountId = 0)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            entities.AccountStd.NoTracking();
            IQueryable<Account> query = (from a in entities.Account.Include("AccountStd")
                                         where a.AccountDimId == accountDimId &&
                                          a.ActorCompanyId == actorCompanyId
                                         select a);

            if (loadAccountDim)
            {
                entities.AccountDim.NoTracking();
                query = query.Include("AccountDim");
            }
            if (loadInternalAccounts)
            {
                entities.AccountInternal.NoTracking();
                query = query.Include("AccountInternal");
                query = query.Include("AccountMapping.AccountInternal.Account");
            }
            if (getCategories)
            {
                entities.CategoryAccount.NoTracking();
                query = query.Include("AccountInternal.CategoryAccount.Category");
            }

            if (onlyActive)
                query = query.Where(a => a.State == (int)SoeEntityState.Active);
            else
                query = query.Where(a => a.State != (int)SoeEntityState.Deleted);

            if (ignoreHierarchyOnly)
                query = query.Where(a => !a.HierarchyOnly);

            if (accountId > 0)
                query = query.Where(a => a.AccountId == accountId);

            List<Account> accounts = query.ToList();

            if (loadBalance || setExtensionNames || setParentAccount)
            {
                List<AccountBalance> balances = null;
                List<CompTermDTO> compTerms = null;
                List<GenericType> accountTypes = null;
                Dictionary<int, string> vatAccounts = null;

                if (loadBalance)
                {
                    AccountBalanceManager abm = new AccountBalanceManager(null, actorCompanyId);
                    balances = abm.GetAccountBalancesByYear(accountYearId);
                }

                if (setExtensionNames)
                {
                    int langId = GetLangId();
                    compTerms = TermManager.GetCompTermDTOsByLanguage(CompTermsRecordType.AccountName, langId).ToList();
                    accountTypes = base.GetTermGroupContent(TermGroup.AccountType, langId);
                    vatAccounts = GetSysVatAccountsDict(langId, false);
                }

                // Load all accounts
                List<Account> allAccounts = null;
                if (setParentAccount)
                    allAccounts = GetAccounts(actorCompanyId);

                foreach (Account account in accounts)
                {
                    if (loadBalance && account.AccountStd != null)
                    {
                        AccountBalance balance = balances.FirstOrDefault(b => b.AccountId == account.AccountId);
                        if (balance != null)
                            account.Balance = balance.Balance;
                    }

                    if (setExtensionNames)
                    {
                        // Name
                        CompTermDTO compTerm = compTerms.FirstOrDefault(t => t.RecordId == account.AccountId);
                        if (compTerm != null)
                            account.Name = String.Format("{0} ({1})", account.Name, compTerm.Name);

                        if (account.AccountStd != null)
                        {
                            // AccountType
                            GenericType accountType = accountTypes.FirstOrDefault(t => t.Id == account.AccountStd.AccountTypeSysTermId);
                            if (accountType != null)
                                account.Type = accountType.Name;

                            // VatType
                            if (account.AccountStd.SysVatAccountId.HasValue && account.AccountStd.SysVatAccountId.Value != 0 && vatAccounts.ContainsKey(account.AccountStd.SysVatAccountId.Value))
                                account.VatType = vatAccounts[account.AccountStd.SysVatAccountId.Value];
                        }
                    }

                    if (setParentAccount && account.ParentAccountId.HasValue)
                    {
                        var parentAccount = allAccounts.FirstOrDefault(a => a.AccountId == account.ParentAccountId);
                        if (parentAccount != null)
                            account.ParentAccountName = parentAccount.Name;
                    }
                }
            }

            if (setLinkedToShiftType)
            {
                AccountDimDTO dim = base.GetShiftTypeAccountDimFromCache(entities, actorCompanyId);
                if (dim != null && dim.AccountDimId == accountDimId)
                {
                    List<ShiftType> shiftTypes = base.GetShiftTypesFromCache(entities, CacheConfig.Company(actorCompanyId)).Where(s => s.AccountId.HasValue).ToList();
                    foreach (ShiftType shiftType in shiftTypes)
                    {
                        Account account = accounts.FirstOrDefault(a => a.AccountId == shiftType.AccountId.Value);
                        if (account != null)
                            account.IsLinkedToShiftType = true;
                    }
                }
            }

            return accounts.OrderBy(a => a.AccountNrSort).ToList();
        }

        public List<Account> GetAccounts(List<int> accountIds, int actorCompanyId, bool includeInactive = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccounts(entities, accountIds, actorCompanyId);
        }

        public List<Account> GetAccounts(CompEntities entities, List<int> accountIds, int actorCompanyId, bool includeInactive = false)
        {
            if (accountIds.IsNullOrEmpty())
                return new List<Account>();

            return (from a in entities.Account
                    where a.ActorCompanyId == actorCompanyId &&
                    accountIds.Any(accountId => accountId == a.AccountId) &&
                    (includeInactive ? a.State <= (int)SoeEntityState.Inactive : a.State == (int)SoeEntityState.Active)
                    select a).ToList();
        }

        public List<Account> GetAccountsStdsByCompany(int actorCompanyId, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountsStdsByCompany(entities, actorCompanyId, loadAccount, loadAccountDim, loadAccountMapping);
        }

        public List<Account> GetAccountsStdsByCompany(CompEntities entities, int actorCompanyId, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false)
        {
            return GetAccountsByCompany(entities, actorCompanyId, onlyStd: true, loadAccount: loadAccount, loadAccountDim: loadAccountDim, loadAccountMapping: loadAccountMapping);
        }

        public List<Account> GetAccountsInternalsByCompany(int actorCompanyId, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountsInternalsByCompany(entities, actorCompanyId, loadAccount, loadAccountDim, loadAccountMapping);
        }

        public List<Account> GetAccountsInternalsByCompany(CompEntities entities, int actorCompanyId, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool includeInactive = false)
        {
            return GetAccountsByCompany(entities, actorCompanyId, onlyInternal: true, loadAccount: loadAccount, loadAccountDim: loadAccountDim, loadAccountMapping: loadAccountMapping, includeInactive: includeInactive);
        }

        public List<Account> GetAccountsByCompany(int actorCompanyId, bool onlyStd = false, bool onlyInternal = false, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool includeInactive = false, List<int> ids = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountsByCompany(entities, actorCompanyId, onlyStd, onlyInternal, loadAccount, loadAccountDim, loadAccountMapping, includeInactive, ids);
        }

        public List<Account> GetAccountsByCompany(CompEntities entities, int actorCompanyId, bool onlyStd = false, bool onlyInternal = false, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool includeInactive = false, List<int> ids = null)
        {
            bool fetachAll = !onlyStd && !onlyInternal;
            bool fetchAccountStd = fetachAll || onlyStd;
            bool fetchAccountInternal = fetachAll || onlyInternal;

            IQueryable<Account> query = (from a in entities.Account
                                         where a.ActorCompanyId == actorCompanyId
                                         select a);

            if (loadAccountDim)
            {
                query = query.Include("AccountDim.Parent");
            }

            if (loadAccount)
            {
                if (fetchAccountStd)
                    query = query.Include("AccountStd.AccountSru");
                if (fetchAccountInternal)
                    query = query.Include("AccountInternal");
            }

            if (loadAccountMapping)
            {
                query = query.Include("AccountMapping.AccountInternal.Account");
                query = query.Include("AccountMapping.AccountDim");
            }

            if (includeInactive)
                query = query.Where(a => a.State < (int)SoeEntityState.Deleted);
            else
                query = query.Where(a => a.State == (int)SoeEntityState.Active);


            if (onlyStd)
                query = query.Where(a => a.AccountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
            else if (onlyInternal)
                query = query.Where(a => a.AccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD);

            if (ids != null)
            {
                query = query.Where(a => ids.Contains(a.AccountId));
            }

            return query.ToList();
        }

        public List<Account> GetAccountsByDim(int accountDimId, int actorCompanyId, bool? active, bool loadAccount = false, bool loadAccountDim = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountsByDim(entities, accountDimId, actorCompanyId, active, loadAccount, loadAccountDim);
        }

        public List<Account> GetAccountsByDim(CompEntities entities, int accountDimId, int actorCompanyId, bool? active, bool loadAccount = false, bool loadAccountDim = false)
        {
            IQueryable<Account> query = (from a in entities.Account
                                         where a.AccountDimId == accountDimId &&
                                         a.ActorCompanyId == actorCompanyId &&
                                         a.State != (int)SoeEntityState.Deleted
                                         select a);

            if (loadAccount)
                query = query.Include("AccountStd").Include("AccountInternal");
            if (loadAccountDim)
                query = query.Include("AccountDim");

            if (active == true)
                query = query.Where(a => a.State == (int)SoeEntityState.Active);
            else if (active == false)
                query = query.Where(a => a.State == (int)SoeEntityState.Inactive);

            return query.OrderBy(a => a.AccountNr).ThenBy(a => a.Name).ToList();
        }

        public List<Account> GetAccountsByDimAndInterval(CompEntities entities, int accountDimId, int actorCompanyId, string accountFrom, string accountTo)
        {
            var accountsByDim = GetAccountsByDim(entities, accountDimId, actorCompanyId, true, true, true).ToList();
            return FilterAccountsByDimAndInterval(accountsByDim, accountDimId, accountFrom, accountTo);
        }

        public List<Account> FilterAccountsByDimAndInterval(List<Account> accounts, int accountDimId, string accountFrom, string accountTo)
        {
            List<Account> accountsInInterval = new List<Account>();

            AccountIntervalDTO accountInterval = new AccountIntervalDTO()
            {
                AccountDimId = accountDimId,
                AccountNrFrom = accountFrom,
                AccountNrTo = accountTo
            };

            foreach (Account account in accounts)
            {
                if (Validator.IsAccountInInterval(account.AccountNr, account.AccountDimId, accountInterval))
                {
                    accountsInInterval.Add(account);
                }
            }

            return accountsInInterval;
        }

        public List<Account> GetAccountsByAccountNr(string accountNr, int accountDimId, bool matchAll = false, bool loadAccountStd = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountsByAccountNr(entities, accountNr, accountDimId, matchAll, loadAccountStd);
        }

        public List<Account> GetAccountsByAccountNr(CompEntities entities, string accountNr, int accountDimId, bool matchAll = false, bool loadAccountStd = false)
        {
            IQueryable<Account> query = (from a in entities.Account
                                         where a.AccountDimId == accountDimId &&
                                         a.State == (int)SoeEntityState.Active
                                         select a);
            if (loadAccountStd)
                query = query.Include("AccountStd");

            if (matchAll)
                query = query.Where(a => a.AccountNr == accountNr);
            else
                query = query.Where(a => a.AccountNr.StartsWith(accountNr));

            return query.OrderBy(a => a.AccountNr).ThenBy(a => a.Name).ToList();
        }

        public List<Account> GetAccountsBySearch(int accountDimId, string search)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountsBySearch(entities, accountDimId, search);
        }

        public List<Account> GetAccountsBySearch(CompEntities entities, int accountDimId, string search)
        {
            return (from a in entities.Account
                    where a.AccountDimId == accountDimId &&
                    a.Name.Contains(search) &&
                    a.State == (int)SoeEntityState.Active
                    orderby a.AccountNr, a.Name
                    select a).ToList();
        }

        public List<Account> GetChildrenAccounts(int actorCompanyId, int parentAccountId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetChildrenAccounts(entities, actorCompanyId, parentAccountId);
        }

        public List<Account> GetChildrenAccounts(CompEntities entities, int actorCompanyId, int parentAccountId)
        {
            return (from a in entities.Account
                    where a.ParentAccountId == parentAccountId &&
                    a.State == (int)SoeEntityState.Active &&
                    a.ActorCompanyId == actorCompanyId
                    orderby a.AccountNr
                    select a).ToList();
        }

        public List<Account> GetAllChildrenAccounts(CompEntities entities, int actorCompanyId, int parentAccountId, int recursive = 0)
        {
            recursive++;
            if (recursive > 5)
                return new List<Account>();

            var accounts = GetChildrenAccounts(entities, actorCompanyId, parentAccountId);
            List<Account> grandChildren = new List<Account>();

            foreach (var account in accounts)
            {
                var grandchilds = GetAllChildrenAccounts(entities, actorCompanyId, account.AccountId, recursive);
                grandChildren.AddRange(grandchilds);
            }

            accounts.AddRange(grandChildren);

            return accounts.Distinct().ToList();
        }

        public List<AccountDTO> GetAccountsSortedByDim(int actorCompanyId)
        {
            List<AccountDTO> result = new List<AccountDTO>();

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            List<AccountDimDTO> accountDims = GetAccountDimsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            if (accountDims.IsNullOrEmpty())
                return result;

            List<AccountDTO> accounts = GetAccountInternalsFromCache(entitiesReadOnly, CacheConfig.Company(actorCompanyId));
            if (accounts.IsNullOrEmpty())
                return result;

            foreach (AccountDimDTO accountDim in accountDims.OrderBy(ad => ad.Name))
            {
                foreach (AccountDTO account in accounts.Where(a => a.AccountDimId == accountDim.AccountDimId).OrderBy(a => a.NumberName))
                {
                    account.AccountDim = accountDim;
                    result.Add(account);
                }
            }

            return result;
        }

        public Dictionary<int, string> GetAccountsDict(int actorCompanyId, int accountDimId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            List<Account> accounts = (from a in entities.Account
                                      where a.AccountDimId == accountDimId &&
                                      a.State == (int)SoeEntityState.Active &&
                                      a.ActorCompanyId == actorCompanyId
                                      select a).ToList();

            foreach (Account account in accounts.OrderBy(i => i.AccountNrSort).ThenBy(i => i.Name))
            {
                dict.Add(account.AccountId, account.AccountNrPlusName);
            }

            return dict;
        }

        public AccountDTO GetAccountDTO(int actorCompanyId, int accountId, bool onlyActive, List<AccountDTO> accounts = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountDTO(entities, actorCompanyId, accountId, onlyActive, accounts);
        }

        public AccountDTO GetAccountDTO(CompEntities entities, int actorCompanyId, int accountId, bool onlyActive, List<AccountDTO> allAccounts = null)
        {
            return allAccounts?.FirstOrDefault(i => i.AccountId == accountId) ?? GetAccount(entities, actorCompanyId, accountId, onlyActive)?.ToDTO();
        }

        public Account GetAccount(int actorCompanyId, int accountId, bool onlyActive = true, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool loadAccountSru = false, bool loadCompanyExternalCodes = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccount(entities, actorCompanyId, accountId, onlyActive, loadAccount, loadAccountDim, loadAccountMapping, loadAccountSru, loadCompanyExternalCodes);
        }

        public Account GetAccount(CompEntities entities, int actorCompanyId, int accountId, bool onlyActive = true, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool loadAccountSru = false, bool loadCompanyExternalCodes = false)
        {
            if (accountId == 0)
                return null;

            IQueryable<Account> oQuery = entities.Account;
            if (loadAccount)
            {
                oQuery = oQuery.Include("AccountStd");
                oQuery = oQuery.Include("AccountInternal");
            }
            if (loadAccountDim)
                oQuery = oQuery.Include("AccountDim");

            Account account = oQuery.FirstOrDefault(a => a.AccountId == accountId && a.ActorCompanyId == actorCompanyId && a.State != (int)SoeEntityState.Deleted);
            if (account == null || (onlyActive && account.State != (int)SoeEntityState.Active))
                return null;

            if (loadAccountMapping && !account.AccountMapping.IsLoaded)
                account.AccountMapping.Load();
            if (loadAccountSru && account.AccountStd != null && !account.AccountStd.AccountSru.IsLoaded)
                account.AccountStd.AccountSru.Load();

            if (loadCompanyExternalCodes)
                LoadAccountExternalCodes(entities, account);

            return account;
        }

        public void LoadAccountExternalCodes(CompEntities entities, Account account)
        {
            if (account == null)
                return;

            CompanyExternalCode accountHierachyPayrollExport = ActorManager.GetCompanyExternalCode(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExport, account.AccountId, account.ActorCompanyId);
            if (accountHierachyPayrollExport != null)
                account.AccountHierachyPayrollExportExternalCode = accountHierachyPayrollExport.ExternalCode;
            CompanyExternalCode accountHierachyPayrollExportUnit = ActorManager.GetCompanyExternalCode(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExportUnit, account.AccountId, account.ActorCompanyId);
            if (accountHierachyPayrollExportUnit != null)
                account.AccountHierachyPayrollExportUnitExternalCode = accountHierachyPayrollExportUnit.ExternalCode;
        }

        public Account GetAccount(CompEntities entities, int actorCompanyId, List<int> accountIds, int accountDimId)
        {
            return (from a in entities.Account
                    where a.ActorCompanyId == actorCompanyId &&
                    a.AccountDimId == accountDimId &&
                    accountIds.Any(accountId => accountId == a.AccountId) &&
                    a.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault();
        }

        public Account GetAccountByNr(string accountNr, int accountDimId, int actorCompanyId, bool onlyActive = true, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool loadAccountSru = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountByNr(entities, accountNr, accountDimId, actorCompanyId, onlyActive, loadAccount, loadAccountDim, loadAccountMapping, loadAccountSru);
        }

        public Account GetAccountByNr(CompEntities entities, string accountNr, int accountDimId, int actorCompanyId, bool onlyActive = true, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool loadAccountSru = false)
        {
            IQueryable<Account> query = entities.Account;
            if (loadAccount)
            {
                query = query.Include("AccountStd");
                query = query.Include("AccountInternal");
            }
            if (loadAccountDim)
                query = query.Include("AccountDim");

            Account account = (from a in query
                               where a.AccountNr == accountNr &&
                               a.AccountDimId == accountDimId &&
                               a.ActorCompanyId == actorCompanyId &&
                               a.State != (int)SoeEntityState.Deleted
                               select a).FirstOrDefault();

            if (account != null)
            {
                if (onlyActive && account.State != (int)SoeEntityState.Active)
                    return null;

                if (loadAccountMapping && !account.AccountMapping.IsLoaded)
                    account.AccountMapping.Load();
                if (loadAccountSru && account.AccountStd != null && !account.AccountStd.AccountSru.IsLoaded)
                    account.AccountStd.AccountSru.Load();
            }
            return account;
        }

        public Account GetAccountByDimNr(string accountNr, int accountDimNr, int actorCompanyId, bool onlyActive = true, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool loadAccountSru = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountByDimNr(entities, accountNr, accountDimNr, actorCompanyId, onlyActive, loadAccount, loadAccountDim, loadAccountMapping, loadAccountSru);
        }

        public Account GetAccountByDimNr(CompEntities entities, string accountNr, int accountDimNr, int actorCompanyId, bool onlyActive = true, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountMapping = false, bool loadAccountSru = false)
        {
            IQueryable<Account> query = entities.Account;
            if (loadAccount)
            {
                query = query.Include("AccountStd");
                query = query.Include("AccountInternal");
            }
            if (loadAccountDim)
                query = query.Include("AccountDim");

            Account account = (from a in query
                               where a.AccountNr == accountNr &&
                               a.AccountDim.AccountDimNr == accountDimNr &&
                               a.ActorCompanyId == actorCompanyId &&
                               a.State != (int)SoeEntityState.Deleted
                               select a).FirstOrDefault();

            if (account != null)
            {
                if (onlyActive && account.State != (int)SoeEntityState.Active)
                    return null;

                if (loadAccountMapping && !account.AccountMapping.IsLoaded)
                    account.AccountMapping.Load();
                if (loadAccountSru && account.AccountStd != null && !account.AccountStd.AccountSru.IsLoaded)
                    account.AccountStd.AccountSru.Load();
            }
            return account;
        }

        public int GetAccountIdByDimNr(CompEntities entities, string accountNr, int accountDimNr, int actorCompanyId, bool onlyActive)
        {
            return GetAccountByDimNr(entities, accountNr, accountDimNr, actorCompanyId, onlyActive)?.AccountId ?? 0;
        }

        public string GetAccountName(int actorCompanyId, int accountId, bool onlyActive)
        {
            return GetAccount(actorCompanyId, accountId, onlyActive: onlyActive)?.Name ?? string.Empty;
        }

        public string GetAccountNr(int actorCompanyId, int accountId, bool onlyActive)
        {
            return GetAccount(actorCompanyId, accountId, onlyActive: onlyActive)?.AccountNr ?? string.Empty;
        }

        public bool AccountExist(int actorCompanyId, int accountId, bool onlyActive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Company.NoTracking();
            return AccountExist(entities, actorCompanyId, accountId, onlyActive);
        }

        public bool AccountExist(CompEntities entities, int actorCompanyId, int accountId, bool onlyActive)
        {
            return GetAccount(entities, actorCompanyId, accountId, onlyActive) != null;
        }

        public bool AccountExist(int actorCompanyId, int accountDimId, string accountNr, bool onlyActive)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var (exists, _) = AccountExist(entitiesReadOnly, actorCompanyId, accountDimId, accountNr, onlyActive);
            return exists;
        }

        private (bool, bool) AccountExist(CompEntities entities, int actorCompanyId, int accountDimId, string accountNr, bool onlyActive)
        {
            var account = GetAccountByNr(entities, accountNr, accountDimId, actorCompanyId, onlyActive);
            var isActive = false;
            if (account != null)
                isActive = account.State == 0;
            return (account != null, isActive);
        }

        public string GetAccountingString(List<AccountDim> accountDims, Account accountStd, List<Account> accountInternals, bool showDetails)
        {
            StringBuilder sb = new StringBuilder();

            //When AccountStd is null, exclude if from the string 
            bool displayAccountNrForNoneDetails = accountStd != null;

            if (accountDims != null)
            {
                foreach (AccountDim accountDim in accountDims.OrderBy(i => i.AccountDimNr))
                {
                    if (accountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD)
                    {
                        #region AccountStd

                        if (accountStd == null)
                            continue;

                        if (showDetails)
                        {
                            sb.Append(accountDim.Name);
                            sb.Append(": ");
                            sb.Append(accountStd.AccountNr);
                            sb.Append(". ");
                            sb.Append(accountStd.Name);
                        }
                        else
                        {
                            sb.Append(accountStd.AccountNr);
                        }

                        #endregion
                    }
                    else
                    {
                        #region AccountInternal

                        Account accountInternal = accountInternals.FirstOrDefault(i => i.AccountDimId == accountDim.AccountDimId);

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
                            sb.Append(displayAccountNrForNoneDetails ? accountInternal.AccountNr : accountInternal.Name);
                        }

                        #endregion
                    }

                    sb.Append(showDetails ? "; " : ";");
                }
            }

            return sb.ToString();
        }

        public ActionResult AddAccount(Account account, int accountDimId, int actorCompanyId, int userId)
        {
            using (CompEntities entities = new CompEntities())
            {
                return AddAccount(entities, account, accountDimId, actorCompanyId, userId);
            }
        }

        public ActionResult AddAccount(string accountNr, string name, int accountTypeId, int vatAccountId, int sruCode1Id, int actorCompanyId, int userId)
        {
            AccountDTO outputAccount = new AccountDTO();
            ActionResult result;

            #region Prereq

            int amountStop = 1;
            if (accountTypeId == 2 || accountTypeId == 3)
                amountStop = 2;

            #endregion

            Account account;

            using (CompEntities entities = new CompEntities())
            {
                account = new Account()
                {
                    AccountNr = accountNr,
                    Name = name,
                    State = 0,
                    ActorCompanyId = actorCompanyId,
                };

                account.AccountStd = new AccountStd()
                {
                    AccountTypeSysTermId = accountTypeId,
                    SysVatAccountId = vatAccountId,
                    AmountStop = amountStop,
                };

                result = AddAccount(entities, account, GetAccountDimStdId(actorCompanyId), actorCompanyId, userId);
            }

            if (result.Success)
            {
                int?[] sruCodes = new int?[1];
                sruCodes[0] = sruCode1Id;
                SaveAccountSru(account.AccountStd, sruCodes);

                // Reload Account to get AccountDim relation
                if (result.IntegerValue != 0)
                    account = GetAccount(actorCompanyId, result.IntegerValue, onlyActive: false, loadAccountDim: true);

                FlushAccountDimsFromCache(actorCompanyId);
                outputAccount = account.ToDTO(true);
            }

            result.Value = outputAccount;
            return result;
        }

        public ActionResult AddAccount(CompEntities entities, Account account, int accountDimId, int actorCompanyId, int userId)
        {
            ActionResult result = new ActionResult();

            if (account == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Account");

            using (entities)
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var (exists, isActive) = AccountExist(entities, actorCompanyId, accountDimId, account.AccountNr, false);
                        if (exists && isActive)
                            return new ActionResult((int)ActionResultSave.AccountExist, GetText(7471, "Kontonumret finns redan") + " " + account.AccountNr);
                        else if (exists && !isActive)
                            return new ActionResult((int)ActionResultSave.AccountExist, GetText(7472, "Finns redan ett inaktivt konto med samma nummer") + " " + account.AccountNr);
                        account.AccountDimId = accountDimId;

                        //Add Account
                        result = AddEntityItem(entities, account, "Account", transaction);
                        if (result.Success && userId != 0)
                        {
                            //AccountHistory
                            AccountHistory accountHistory = new AccountHistory()
                            {
                                Name = account.Name,
                                AccountNr = account.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = null,
                                SieKpTyp = account.AccountStd?.SieKpTyp,

                                //Set FK
                                UserId = userId,

                                //References
                                Account = account,
                            };

                            result = AddEntityItem(entities, accountHistory, "AccountHistory", transaction);
                            if (!result.Success)
                                return result;
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties      
                        result.IntegerValue = account.AccountId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult UpdateAccount(Account account, int actorCompanyId, int userId)
        {
            ActionResult result = new ActionResult();

            if (account == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "Account");

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        Account originalAccount = GetAccount(entities, actorCompanyId, account.AccountId, onlyActive: false, loadAccount: true, loadAccountSru: true);
                        if (originalAccount == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "Account");

                        #endregion

                        #region Update Account

                        result = UpdateEntityItem(entities, originalAccount, account, "Account", transaction);
                        if (!result.Success)
                            return result;

                        if (originalAccount.AccountStd != null)
                            result = UpdateEntityItem(entities, originalAccount.AccountStd, account.AccountStd, "AccountStd", transaction);
                        else if (originalAccount.AccountInternal != null)
                            result = UpdateEntityItem(entities, originalAccount.AccountInternal, account.AccountInternal, "AccountInternal", transaction);

                        if (!result.Success)
                            return result;

                        #endregion

                        #region AccountHistory

                        if (userId > 0)
                        {
                            AccountHistory accountHistory = new AccountHistory()
                            {
                                Name = account.Name,
                                AccountNr = account.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = null,
                                SieKpTyp = account.AccountStd?.SieKpTyp,

                                //Set FK
                                UserId = userId,

                                //Set references
                                Account = originalAccount,

                            };

                            result = AddEntityItem(entities, accountHistory, "AccountHistory", transaction);
                        }

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult SaveAccount(AccountEditDTO inputAccount, List<CompTermDTO> inputTranslations, List<AccountMappingDTO> inputAccountMappings, List<CategoryAccountDTO> inputCategoryAccounts, List<ExtraFieldRecordDTO> extraFields = null, bool skipStateValidation = false)
        {
            ActionResult result = new ActionResult();

            #region Init

            int accountId = inputAccount.AccountId;

            #endregion

            using (var entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        AccountDim accountDim = GetAccountDim(entities, inputAccount.AccountDimId, base.ActorCompanyId);
                        if (accountDim == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                        if (accountDim.OnlyAllowAccountsWithParent && !inputAccount.ParentAccountId.ToNullable().HasValue)
                        {
                            if (!accountDim.ParentReference.IsLoaded)
                                accountDim.ParentReference.Load();
                            AccountDim parentAccountDim = accountDim.Parent != null ? GetAccountDim(entities, accountDim.Parent.AccountDimId, base.ActorCompanyId) : null;
                            return new ActionResult((int)ActionResultSave.IncorrectInput, string.Format(GetText(7473, "Kontonivå kräver att konto på övre nivå {0} anges"), parentAccountDim?.Name ?? ""));
                        }

                        AccountDim accountDimStd = GetAccountDimStd(entities, base.ActorCompanyId);
                        Account account = accountId > 0 ? GetAccount(entities, base.ActorCompanyId, accountId, onlyActive: false, loadAccount: true, loadAccountMapping: true, loadAccountSru: true) : null;

                        if (account == null)
                        {
                            #region Add

                            Account existingAccount = GetAccountByNr(entities, inputAccount.AccountNr, accountDim.AccountDimId, base.ActorCompanyId, onlyActive: false);
                            if (existingAccount != null)
                            {
                                if (existingAccount.State == (int)SoeEntityState.Inactive)
                                    return new ActionResult((int)ActionResultSave.AccountExist, GetText(7472, "Finns redan ett inaktivt konto med samma nummer"));
                                return new ActionResult((int)ActionResultSave.AccountExist, GetText(7471, "Kontonumret finns redan"));
                            }

                            account = new Account()
                            {
                                //Set FK
                                ActorCompanyId = base.ActorCompanyId,
                                AccountDimId = accountDim.AccountDimId,
                            };

                            if (accountDim.IsStandard)
                                account.AccountStd = new AccountStd();
                            else
                                account.AccountInternal = new AccountInternal();
                            entities.Account.AddObject(account);
                            SetCreatedProperties(account);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            if (!inputAccount.AccountNr.Equals(account.AccountNr))
                            {
                                Account existingAccount = GetAccountByNr(entities, inputAccount.AccountNr, accountDim.AccountDimId, base.ActorCompanyId, onlyActive: false);
                                if (existingAccount != null)
                                {
                                    if (existingAccount.State == (int)SoeEntityState.Inactive)
                                        return new ActionResult((int)ActionResultSave.AccountExist, GetText(7472, "Finns redan ett inaktivt konto med samma nummer"));
                                    return new ActionResult((int)ActionResultSave.AccountExist, GetText(7471, "Kontonumret finns redan"));
                                }
                            }

                            if (accountDim.IsStandard)
                            {
                                if (!account.AccountStdReference.IsLoaded)
                                    account.AccountStdReference.Load();
                            }
                            else
                            {
                                if (!account.AccountInternalReference.IsLoaded)
                                    account.AccountInternalReference.Load();
                            }

                            result = TryUpdateAccountState(entities, accountDimStd, account, inputAccount.Active ? SoeEntityState.Active : SoeEntityState.Inactive, skipStateValidation);
                            if (!result.Success)
                                return result;

                            SetModifiedProperties(account);

                            #endregion
                        }

                        #endregion

                        #region Account

                        account.AccountNr = inputAccount.AccountNr?.Trim();
                        account.Name = inputAccount.Name?.Trim();
                        account.Description = inputAccount.Description?.Trim();
                        account.ParentAccountId = inputAccount.ParentAccountId.HasValue && inputAccount.ParentAccountId.Value != 0 ? inputAccount.ParentAccountId : null;
                        account.ExternalCode = inputAccount.ExternalCode;
                        account.HierarchyOnly = inputAccount.HierarchyOnly;
                        account.HierarchyNotOnSchedule = inputAccount.HierarchyNotOnSchedule;

                        if (accountDim.IsStandard)
                        {
                            #region AccountStd

                            account.AccountStd.SysVatAccountId = inputAccount.SysVatAccountId;
                            account.AccountStd.AccountTypeSysTermId = inputAccount.AccountTypeSysTermId;
                            account.AccountStd.AmountStop = inputAccount.AmountStop;
                            account.AccountStd.Unit = inputAccount.Unit;
                            account.AccountStd.UnitStop = inputAccount.UnitStop;
                            account.AccountStd.RowTextStop = inputAccount.RowTextStop;
                            account.AccountStd.SieKpTyp = inputAccount.SieKpTyp;
                            account.AccountStd.ExcludeVatVerification = inputAccount.ExcludeVatVerification;
                            account.AccountStd.isAccrualAccount = inputAccount.isAccrualAccount;

                            #endregion
                        }
                        else
                        {
                            #region AccountInternal

                            account.AccountInternal.AccountInternalType = null; //Not used
                            account.AttestWorkFlowHeadId = inputAccount.AttestWorkFlowHeadId;
                            account.AccountInternal.UseVatDeduction = inputAccount.UseVatDeduction;
                            account.AccountInternal.VatDeduction = inputAccount.VatDeduction;

                            #endregion
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;

                        accountId = account.AccountId;

                        #endregion

                        #region Save relations

                        #region Translations

                        if (inputTranslations != null)
                        {
                            var langIdsToSave = inputTranslations.Select(i => (int)i.Lang).Distinct().ToList();
                            var existingTranslations = TermManager.GetCompTerms(entities, CompTermsRecordType.AccountName, account.AccountId);

                            #region Delete existing translations for other languages

                            foreach (var existingTranslation in existingTranslations)
                            {
                                if (langIdsToSave.Contains(existingTranslation.LangId))
                                    continue;

                                existingTranslation.State = (int)SoeEntityState.Deleted;
                            }

                            #endregion

                            #region Add or update translations for languages

                            foreach (int langId in langIdsToSave)
                            {
                                CompTerm translation = null;
                                var inputTranslation = inputTranslations.FirstOrDefault(i => (int)i.Lang == langId);

                                var existingTranslationsForLang = existingTranslations.Where(i => i.LangId == langId).ToList();
                                if (existingTranslationsForLang.Count == 0)
                                {
                                    #region Add

                                    translation = new CompTerm { ActorCompanyId = base.ActorCompanyId };
                                    entities.CompTerm.AddObject(translation);

                                    #endregion
                                }
                                else
                                {
                                    #region Update

                                    for (int i = 0; i < existingTranslationsForLang.Count; i++)
                                    {
                                        if (i > 0)
                                        {
                                            //Remove duplicates
                                            existingTranslationsForLang[i].State = (int)SoeEntityState.Deleted;
                                            continue;
                                        }

                                        translation = existingTranslationsForLang[i];
                                    }

                                    #endregion
                                }

                                #region Set values

                                translation.RecordType = (int)inputTranslation.RecordType;
                                translation.RecordId = account.AccountId;
                                translation.LangId = (int)inputTranslation.Lang;
                                translation.Name = inputTranslation.Name;
                                translation.State = (int)SoeEntityState.Active;

                                #endregion
                            }

                            #endregion
                        }

                        #endregion

                        if (accountDim.IsStandard)
                        {
                            #region AccountMapping

                            if (inputAccountMappings != null)
                            {
                                if (!account.AccountMapping.IsLoaded)
                                    account.AccountMapping.Load();

                                var accountDimInternals = GetAccountDimsByCompany(entities, base.ActorCompanyId, onlyInternal: true, loadAccounts: true).ToDTOs(true);
                                foreach (var accountDimInternal in accountDimInternals)
                                {
                                    AccountMappingDTO inputAccountMapping = inputAccountMappings.FirstOrDefault(i => i.AccountDimId == accountDimInternal.AccountDimId);
                                    if (inputAccountMapping != null)
                                    {
                                        AccountMapping accountMapping = account.AccountMapping.FirstOrDefault(i => i.AccountDimId == accountDimInternal.AccountDimId);
                                        if (accountMapping == null)
                                        {
                                            accountMapping = new AccountMapping()
                                            {
                                                AccountDimId = inputAccountMapping.AccountDimId,
                                                MandatoryLevel = (int)inputAccountMapping.MandatoryLevel,
                                                DefaultAccountId = inputAccountMapping.DefaultAccountId.ToNullable(),
                                                Account = account,
                                            };
                                            SetCreatedProperties(accountMapping);
                                            entities.AccountMapping.AddObject(accountMapping);
                                        }
                                        else
                                        {
                                            accountMapping.AccountDimId = inputAccountMapping.AccountDimId;
                                            accountMapping.MandatoryLevel = (int)inputAccountMapping.MandatoryLevel;
                                            accountMapping.DefaultAccountId = inputAccountMapping.DefaultAccountId != 0 ? inputAccountMapping.DefaultAccountId : null;

                                            SetModifiedProperties(accountMapping);
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region AccountSru

                            if (!account.AccountStd.AccountSru.IsLoaded)
                                account.AccountStd.AccountSru.Load();

                            //Prevent duplicates
                            if (inputAccount.SysAccountSruCode1Id.HasValue && inputAccount.SysAccountSruCode2Id.HasValue && inputAccount.SysAccountSruCode1Id.Value == inputAccount.SysAccountSruCode2Id.Value)
                                inputAccount.SysAccountSruCode2Id = null;

                            AccountSru accountSru1 = account.AccountStd.AccountSru.Count >= 1 ? account.AccountStd.AccountSru.FirstOrDefault() : null;
                            AccountSru accountSru2 = account.AccountStd.AccountSru.Count >= 2 ? account.AccountStd.AccountSru.Skip(1).FirstOrDefault() : null;
                            SetAccountSru(entities, account.AccountStd, accountSru1, inputAccount.SysAccountSruCode1Id);
                            SetAccountSru(entities, account.AccountStd, accountSru2, inputAccount.SysAccountSruCode2Id);

                            #endregion
                        }
                        else
                        {
                            #region CategoryAccounts

                            if (inputCategoryAccounts != null)
                            {
                                var existingCategoryAccounts = CategoryManager.GetCategoryAccountsByAccount(entities, accountId, base.ActorCompanyId, false);

                                #region Delete existing 

                                foreach (var existingCategoryAccount in existingCategoryAccounts)
                                {
                                    if (inputCategoryAccounts.Any(x => x.CategoryAccountId == existingCategoryAccount.CategoryAccountId))
                                        continue;

                                    existingCategoryAccount.State = (int)SoeEntityState.Deleted;
                                }

                                #endregion

                                #region Add or update

                                foreach (var inputCategoryAccount in inputCategoryAccounts)
                                {
                                    if (inputCategoryAccount.CategoryId == 0)
                                        continue;

                                    var categoryAccount = existingCategoryAccounts.FirstOrDefault(x => x.CategoryAccountId == inputCategoryAccount.CategoryAccountId);
                                    if (categoryAccount == null)
                                    {
                                        #region Add

                                        categoryAccount = new CategoryAccount()
                                        {
                                            DateFrom = null,
                                            DateTo = null,
                                            State = (int)SoeEntityState.Active,

                                            //Set FK
                                            CategoryId = inputCategoryAccount.CategoryId,
                                            AccountId = accountId,
                                            ActorCompanyId = base.ActorCompanyId,
                                        };

                                        entities.CategoryAccount.AddObject(categoryAccount);

                                        #endregion
                                    }
                                    else
                                    {
                                        #region Update

                                        categoryAccount.CategoryId = inputCategoryAccount.CategoryId;

                                        #endregion
                                    }
                                }

                                #endregion
                            }

                            #endregion

                            #region CompanyExternalCodes

                            ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExport, accountId, inputAccount.AccountHierachyPayrollExportExternalCode, base.ActorCompanyId, false);
                            ActorManager.UpsertExternalNbrs(entities, TermGroup_CompanyExternalCodeEntity.AccountHierachyPayrollExportUnit, accountId, inputAccount.AccountHierachyPayrollExportUnitExternalCode, base.ActorCompanyId, false);

                            #endregion
                        }

                        #region AccountHistory

                        AccountHistory accountHistory = new AccountHistory()
                        {
                            Name = account.Name,
                            AccountNr = account.AccountNr,
                            Date = DateTime.Now,
                            SysAccountStdTypeId = null,
                            SieKpTyp = account.AccountStd?.SieKpTyp,

                            //Set FK
                            UserId = base.UserId,

                            //References
                            Account = account,
                        };

                        entities.AccountHistory.AddObject(accountHistory);

                        #endregion

                        #region ExtraFields

                        if (extraFields != null && extraFields.Count > 0)
                        {
                            result = ExtraFieldManager.SaveExtraFieldRecords(entities, extraFields, (int)SoeEntityType.Account, accountId, base.ActorCompanyId);
                            if (!result.Success)
                            {
                                result.ErrorNumber = (int)ActionResultSave.EntityNotUpdated;
                                return result;
                            }
                        }

                        #endregion

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = accountId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        public ActionResult DeleteAccount(int accountId)
        {
            using (CompEntities entities = new CompEntities())
            {
                Account account = GetAccount(entities, base.ActorCompanyId, accountId, onlyActive: false, loadAccount: true);
                if (account == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "Account");

                AccountDim accountDimStd = GetAccountDimStd(base.ActorCompanyId);

                var result = TryUpdateAccountState(entities, accountDimStd, account, SoeEntityState.Deleted);
                if (!result.Success)
                    return result;

                return SaveChanges(entities);
            }
        }

        public ActionResult UpdateAccountsState(Dictionary<int, bool> accountIdStateDict, bool skipStateValidation = false)
        {
            using (var entities = new CompEntities())
            {
                var accountDimStd = GetAccountDimStd(entities, base.ActorCompanyId);
                var accounts = GetAccounts(entities, accountIdStateDict.Keys.ToList(), base.ActorCompanyId, includeInactive: true);

                foreach (var account in accounts)
                {
                    if (!accountIdStateDict.TryGetValue(account.AccountId, out bool isActive))
                        continue;

                    var result = TryUpdateAccountState(entities, accountDimStd, account, isActive ? SoeEntityState.Active : SoeEntityState.Inactive, skipStateValidation);
                    if (!result.Success)
                        return result;
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult TryUpdateAccountState(CompEntities entities, AccountDim accountDimStd, Account account, SoeEntityState toState, bool skipStateValidation = false)
        {
            if (!skipStateValidation)
            {
                var result = ValidateAccountState(entities, accountDimStd, account, toState);
                if (!result.Success)
                    return result;
            }

            return ChangeEntityState(entities, account, toState, saveChanges: false);
        }

        public ActionResult ValidateAccount(string accountNr, int accountId, int accountDimId, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            var existingAccount = (from a in entities.Account
                                   where a.ActorCompanyId == actorCompanyId &&
                                   a.AccountId != accountId &&
                                   a.AccountDimId == accountDimId &&
                                   a.AccountNr == accountNr &&
                                   a.State != (int)SoeEntityState.Deleted
                                   select a).FirstOrDefault();

            if (existingAccount != null)
            {
                if (existingAccount.State == (int)SoeEntityState.Inactive)
                    return new ActionResult((int)ActionResultSave.AccountExist, GetText(7472, "Finns redan ett inaktivt konto med samma nummer"));
                return new ActionResult((int)ActionResultSave.AccountExist, GetText(7471, "Kontonumret finns redan"));
            }

            return result;
        }

        private ActionResult ValidateAccountState(CompEntities entities, AccountDim accountDimStd, Account account, SoeEntityState toState)
        {
            if (toState == SoeEntityState.Inactive)
            {
                if (account.IsAccountInternal(accountDimStd) && IsAccountInternalUsedInEmployeeAccount(entities, account.AccountId, base.ActorCompanyId, out var employeeAccounts))
                {
                    bool hasActiveOrFutureEmployeeAccounts = employeeAccounts.Any(ea =>
                        // Active today (including starts today)
                        (ea.DateFrom <= DateTime.Today && (!ea.DateTo.HasValue || ea.DateTo.Value >= DateTime.Today))
                        // Or not started yet (starts after today)
                        || ea.DateFrom > DateTime.Today
                    );

                    var result = new ActionResult((int)ActionResultDelete.AccountInternalUsedInEmployeeAccount)
                    {
                        ErrorMessage = hasActiveOrFutureEmployeeAccounts
                            ? string.Format(GetText(110681, "Konto {0}.{1} kan inte inaktiveras. Det finns anställda som har det som ekonomisk tillhörighet"), account.AccountNr, account.Name)
                            : string.Format(GetText(110682, "Det finns anställda som haft konto {0}.{1} som ekonomisk tillhörighet. Om kontot inaktiveras kommer de inte vara synliga historiskt. Vill du fortsätta?"), account.AccountNr, account.Name),
                        CanUserOverride = !hasActiveOrFutureEmployeeAccounts
                    };

                    return result;
                }
            }
            else if (toState == SoeEntityState.Deleted)
            {
                if (account.IsAccountStd(accountDimStd) && IsAccountStdUsed(entities, account.AccountId, base.ActorCompanyId))
                    return new ActionResult((int)ActionResultDelete.AccountStdExists);
                else if (account.IsAccountInternal(accountDimStd) && IsAccountInternalUsed(entities, account.AccountId, base.ActorCompanyId))
                    return new ActionResult((int)ActionResultDelete.AccountInternalExists);
            }

            return new ActionResult(true);
        }

        public List<AccountDTO> FilterByDefaultEmployeeAccountDimEmployee(List<AccountDTO> accounts)
        {
            int defaultEmployeeAccountDimEmployeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, base.ActorCompanyId, 0);
            return accounts.Where(a => a.AccountDim.AccountDimId == defaultEmployeeAccountDimEmployeeAccountDimId).OrderBy(x => x.Name).ToList();
        }

        #endregion

        #region AccountHierachyRepository

        public AccountRepository CreateAccountRepository(CompEntities entities, int actorCompanyId, int userId, DateTime dateFrom, DateTime dateTo, AccountHierarchyInput input = null, bool ignoreAttestRoles = false, bool addAccountInfo = false, bool discardCache = false)
        {
            return new AccountRepository(
                ignoreAttestRoles ? new List<AttestRoleUser>() : AttestManager.GetAttestRoleUsers(entities, actorCompanyId, userId, dateFrom, dateTo, includeAttestRole: true, ignoreDates: input.GetValue(AccountHierarchyParamType.IgnoreAttestRoleDates)),
                GetAccountDimsFromCache(entities, CacheConfig.Company(actorCompanyId, discardCache: discardCache)),
                GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId, discardCache: discardCache)),
                GetEmployeeAccountsFromCache(entities, CacheConfig.Company(actorCompanyId, discardCache: discardCache)),
                addAccountInfo
            );
        }

        public string GetAccountHierachyCacheKey(List<int> accountIds, DateTime? dateFrom, DateTime? dateTo, int? userId)
        {
            var accountIdString = !accountIds.IsNullOrEmpty() ? string.Join("#", accountIds) : "noIds";
            var dateFromString = dateFrom.HasValue ? dateFrom.ToString() : "noFrom";
            var dateToString = dateTo.HasValue ? dateTo.ToString() : "noTo";
            var userString = userId.HasValue ? $"|u{userId}" : Guid.NewGuid().ToString();
            var roleString = base.RoleId != 0 ? $"|r{base.RoleId}" : Guid.NewGuid().ToString();
            var key = accountIdString + dateFromString + dateToString + userString + roleString;

            return key;
        }

        public bool TryGetAccountHierachyRepositoryFromCache(int actorCompanyId, int? userId, DateTime? dateFrom, DateTime? dateTo, List<int> accountIds, AccountHierarchyInput input, out AccountRepositoryCache accountRepositoryCache, out AccountRepository accountRepository)
        {
            userId = userId.ToNullable();
            accountRepositoryCache = BusinessMemoryCache<AccountRepositoryCache>.Get(GetAccountHierachyCacheKey(accountIds, dateFrom, dateTo, userId));
            accountRepository = accountRepositoryCache?.Get(actorCompanyId, userId, dateFrom, dateTo, input);
            accountRepository?.ClearEmployeeAccounts();
            return accountRepository != null;
        }

        public void AddAccountHierarchy(int actorCompanyId, int? userId, DateTime? dateFrom, DateTime? dateTo, List<int> accountIds, AccountHierarchyInput input, AccountRepositoryCache accountRepositoryCache, AccountRepository accountRepository)
        {
            userId = userId.ToNullable();
            if (accountRepositoryCache == null)
                accountRepositoryCache = new AccountRepositoryCache();
            accountRepositoryCache.Add(accountRepository, actorCompanyId, userId, dateFrom, dateTo, input);
            BusinessMemoryCache<AccountRepositoryCache>.Set(GetAccountHierachyCacheKey(accountIds, dateFrom, dateTo, userId), accountRepositoryCache);
        }

        public AccountRepository GetAccountHierarchyRepositoryByIds(CompEntities entities, List<int> accountIds, int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null, bool ignoreAttestRoles = false)
        {
            if (accountIds.IsNullOrEmpty())
                return null;

            if (!TryGetAccountHierachyRepositoryFromCache(actorCompanyId, userId, dateFrom, dateTo, accountIds, input, out AccountRepositoryCache hierachyRepositoryCache, out AccountRepository accountRepository))
            {
                if (!TryLoadAccountHierachyRepositoryRequisites(entities, actorCompanyId, out List<AccountDimDTO> accountDims, out List<AccountDTO> accountInternals))
                    return null;
                if (ignoreAttestRoles)
                    return null;

                List<AttestRoleUser> attestRoleUsers = null;
                if (dateFrom.HasValue)
                {
                    attestRoleUsers = AttestManager.GetAttestRoleUsers(entities, actorCompanyId, userId, dateFrom, dateTo, includeAttestRole: true, onlyDefaultAccounts: input.GetValue(AccountHierarchyParamType.OnlyDefaultAccounts));
                    if (attestRoleUsers.IsNullOrEmpty())
                        return null;

                    List<AccountDTO> attestRoleAccounts = attestRoleUsers.GetValidAccounts(accountDims, accountInternals, dateFrom, dateTo);
                    if (attestRoleAccounts.IsNullOrEmpty() || (!attestRoleUsers.ShowAll(dateFrom)) && !accountIds.ContainsAny(attestRoleAccounts.Select(i => i.AccountId).ToList()))
                        return null;
                }

                List<AccountDTO> accounts = accountInternals.GetIdentifiableAccounts(accountIds);
                if (accounts.IsNullOrEmpty())
                    return null;

                if (!input.GetValue(AccountHierarchyParamType.OnlyDefaultAccounts) && attestRoleUsers != null)
                {
                    List<int> secondaryAccountIds = attestRoleUsers
                        .Where(i =>
                            i.AccountId.HasValue &&
                            !accountIds.Contains(i.AccountId.Value) &&
                            i.AccountPermissionType == (int)TermGroup_AttestRoleUserAccountPermissionType.Secondary)
                        .Select(i => i.AccountId.Value)
                        .ToList();

                    if (secondaryAccountIds.Any())
                        accounts.AddRange(accountInternals.Where(i => secondaryAccountIds.Contains(i.AccountId)));
                }

                AccountRepositorySettings settings = CreateAccountRepositorySettings(entities, input, actorCompanyId);
                accountRepository = new AccountRepository(attestRoleUsers, accountDims, accountInternals, accounts, settings);
                if (!attestRoleUsers.IsNullOrEmpty())
                    AddAccountHierarchy(actorCompanyId, userId, dateFrom, dateTo, accountIds, input, hierachyRepositoryCache, accountRepository);
            }

            return accountRepository;
        }

        public AccountRepository GetAccountHierarchyRepositoryByUser(CompEntities entities, int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, List<AccountDTO> accountInternalsInput = null, List<AccountDimDTO> accountDimsInput = null, AccountHierarchyInput input = null, bool ignoreAttestRoles = false)
        {
            if (!base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId))
                return null;

            if (!TryGetAccountHierachyRepositoryFromCache(actorCompanyId, userId, dateFrom, dateTo, null, input, out AccountRepositoryCache hierachyRepositoryCache, out AccountRepository accountRepository))
            {
                List<AccountDimDTO> accountDims = accountDimsInput;
                List<AccountDTO> accountInternals = accountInternalsInput;
                if ((accountDims == null || accountInternals == null) && !TryLoadAccountHierachyRepositoryRequisites(entities, actorCompanyId, out accountDims, out accountInternals))
                    return null;

                List<AccountDTO> accounts = null;
                bool ignoreDates = input.GetValue(AccountHierarchyParamType.IgnoreAttestRoleDates);
                List<AttestRoleUser> attestRoleUsers = !ignoreAttestRoles ? AttestManager.GetAttestRoleUsers(entities, actorCompanyId, userId, dateFrom, dateTo, includeAttestRole: true, ignoreDates: ignoreDates, onlyDefaultAccounts: input.GetValue(AccountHierarchyParamType.OnlyDefaultAccounts)) : new List<AttestRoleUser>();
                if (attestRoleUsers.IsNullOrEmpty())
                {
                    if (!input.GetValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole))
                        return null;

                    int employeeId = EmployeeManager.GetEmployeeIdForUser(entities, userId, actorCompanyId);
                    if (employeeId != 0)
                    {
                        List<int> employeeAccountIds = EmployeeManager.GetEmployeeAccountIds(entities, actorCompanyId, employeeId, DateTime.Today);
                        if (!accountInternals.IsNullOrEmpty())
                            accounts = accountInternals.Where(a => employeeAccountIds.Contains(a.AccountId)).ToList();
                    }
                }
                else
                {
                    if (!ignoreAttestRoles && base.RoleId != 0)
                        attestRoleUsers = attestRoleUsers.Where(w => !w.RoleId.HasValue || w.RoleId.Value == base.RoleId).ToList();

                    accounts = attestRoleUsers.GetValidAccounts(accountDims, accountInternals, ignoreDates ? null : dateFrom, ignoreDates ? null : dateTo);
                }

                if (accounts.IsNullOrEmpty())
                    return null;

                AccountRepositorySettings settings = CreateAccountRepositorySettings(entities, input, actorCompanyId);
                accountRepository = new AccountRepository(attestRoleUsers, accountDims, accountInternals, accounts, settings);
                if (!attestRoleUsers.IsNullOrEmpty())
                    AddAccountHierarchy(actorCompanyId, userId, dateFrom, dateTo, null, input, hierachyRepositoryCache, accountRepository);
            }

            return accountRepository;
        }

        public AccountRepository GetAccountHierarchyRepositoryByUserSetting(CompEntities entities, int actorCompanyId, int roleId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null, bool ignoreAttestRoles = false)
        {
            var (hierarchyId, isValidSetting, accountIds) = GetAccountHierarchySetting(entities, actorCompanyId, userId);
            if (isValidSetting)
            {
                if (accountIds.Count == 1 && accountIds[0] == 0)
                    isValidSetting = false;
                else if (base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId)).Count(w => accountIds.Contains(w.AccountId)) < accountIds.Count)
                    isValidSetting = false;
            }

            AccountRepository accountRepository;
            if (isValidSetting)
                accountRepository = GetAccountHierarchyRepositoryByIds(entities, accountIds, actorCompanyId, userId, dateFrom, dateTo, input, ignoreAttestRoles: ignoreAttestRoles);
            else
                accountRepository = GetAccountHierarchyRepositoryByUser(entities, actorCompanyId, userId, dateFrom, dateTo, input: input, ignoreAttestRoles: ignoreAttestRoles);

            accountRepository?.SetUserSettingAccountHierarchyId(hierarchyId);

            return accountRepository;
        }

        public (string, bool, List<int>) GetAccountHierarchySetting(CompEntities entities, int actorCompanyId, int userId)
        {
            string accountHierarchyId = SettingManager.GetStringSetting(entities, SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, userId, actorCompanyId, 0);
            var (isValidSetting, accountIds) = GetAccountHierarchySetting(accountHierarchyId);
            return (accountHierarchyId, isValidSetting, accountIds);
        }

        public (bool, List<int>) GetAccountHierarchySetting(string hierarchyId)
        {
            bool isValidSetting = StringUtility.TryGetIntParts(hierarchyId, AccountDTO.HIERARCHYDELIMETER, out List<int> accountIds);
            return (isValidSetting, accountIds);
        }

        public List<int> GetAccountHierarchySettingAccountIds(CompEntities entities, int actorCompanyId, int userId)
        {
            var (_, _, accountIds) = GetAccountHierarchySetting(entities, actorCompanyId, userId);
            return accountIds;
        }

        private AccountRepositorySettings CreateAccountRepositorySettings(CompEntities entities, AccountHierarchyInput input, int actorCompanyId)
        {
            int? selectorAccountDimId = input.GetValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector) ? SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimSelector, 0, actorCompanyId, 0) : (int?)null;
            int? employeeAccountDimId = input.GetValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee) ? SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0) : (int?)null;
            bool useLimitedEmployeeAccountDimLevels = employeeAccountDimId.HasValue && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseLimitedEmployeeAccountDimLevels, 0, actorCompanyId, 0);
            bool useExtendedEmployeeAccountDimLevels = employeeAccountDimId.HasValue && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.UseExtendedEmployeeAccountDimLevels, 0, actorCompanyId, 0);
            bool includeOnlyChildrenOneLevel = input.GetValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel);
            return new AccountRepositorySettings(selectorAccountDimId, employeeAccountDimId, useLimitedEmployeeAccountDimLevels, useExtendedEmployeeAccountDimLevels, includeOnlyChildrenOneLevel);
        }

        private bool TryLoadAccountHierachyRepositoryRequisites(CompEntities entities, int actorCompanyId, out List<AccountDimDTO> accountDims, out List<AccountDTO> accountInternals)
        {
            accountDims = GetAccountDimsFromCache(entities, CacheConfig.Company(actorCompanyId)).Where(i => i.IsInternal).ToList();
            accountInternals = GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));

            return !accountDims.IsNullOrEmpty() && !accountInternals.IsNullOrEmpty();
        }

        #endregion

        #region Accounts from AccountHierachy

        #region By User

        public List<AccountDTO> GetAccountsFromHierarchyById(int actorCompanyId, int accountId, AccountHierarchyInput input = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountsFromHierarchyById(entities, actorCompanyId, accountId, input);
        }

        public List<AccountDTO> GetAccountsFromHierarchyById(CompEntities entities, int actorCompanyId, int accountId, AccountHierarchyInput input = null)
        {
            AccountRepository accountRepository = GetAccountHierarchyRepositoryByIds(entities, new List<int> { accountId }, actorCompanyId, 0, input: input);
            return accountRepository?.GetAccounts(input.GetValue(AccountHierarchyParamType.IncludeVirtualParented)) ?? new List<AccountDTO>();
        }

        public List<AccountDTO> GetAccountsFromHierarchyByUser(int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null, List<AccountDTO> accountInternals = null, List<AccountDimDTO> accountDims = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountsFromHierarchyByUser(entities, actorCompanyId, userId, dateFrom, dateTo, input, accountInternals, accountDims);
        }

        public List<AccountDTO> GetAccountsFromHierarchyByUser(CompEntities entities, int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null, List<AccountDTO> accountInternals = null, List<AccountDimDTO> accountDims = null)
        {
            AccountRepository accountRepository = GetAccountHierarchyRepositoryByUser(entities, actorCompanyId, userId, dateFrom, dateTo, accountInternals, accountDims, input);
            if (input.GetValue(AccountHierarchyParamType.IncludeAbstract))
                return accountRepository?.GetAccountsWithAbstract(input.GetValue(AccountHierarchyParamType.IncludeVirtualParented)) ?? new List<AccountDTO>();
            else
                return accountRepository?.GetAccounts(input.GetValue(AccountHierarchyParamType.IncludeVirtualParented)) ?? new List<AccountDTO>();
        }

        public List<int> GetAccountIdsFromHierarchyByUser(int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null, int employeeId = 0)
        {
            bool useEmployeeAccounts = SettingManager.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.BaseSelectableAccountsOnEmployeeInsteadOfAttestRole, userId, actorCompanyId, 0);

            if (employeeId != 0 && dateFrom.HasValue && useEmployeeAccounts)
            {
                AccountDimSmallDTO dim = GetDefaultEmployeeAccountDimAndSelectableAccounts(base.ActorCompanyId, base.UserId, employeeId, dateFrom.Value);
                return dim.Accounts.Select(a => a.AccountId).ToList();
            }
            else
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                return GetAccountIdsFromHierarchyByUser(entities, actorCompanyId, userId, dateFrom, dateTo, input);
            }
        }

        public List<int> GetAccountIdsFromHierarchyByUser(CompEntities entities, int actorCompanyId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null)
        {
            List<AccountDTO> accounts = GetAccountsFromHierarchyByUser(entities, actorCompanyId, userId, dateFrom, dateTo, input);
            return accounts.Select(a => a.AccountId).ToList();
        }

        public Dictionary<string, string> GetAccountHierarchyStringsByUser(int actorCompanyId, int userId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountHierarchyStringsFromCache(entities, CacheConfig.User(actorCompanyId, userId, roleId: base.RoleId));
        }

        #endregion

        #region By UserSetting

        public List<int> GetAccountIdsFromHierarchyByUserSetting(int actorCompanyId, int roleId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountIdsFromHierarchyByUserSetting(entities, actorCompanyId, roleId, userId, dateFrom, dateTo, input);
        }

        public List<int> GetAccountIdsFromHierarchyByUserSetting(CompEntities entities, int actorCompanyId, int roleId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, AccountHierarchyInput input = null)
        {
            return GetAccountsFromHierarchyByUserSetting(entities, actorCompanyId, roleId, userId, dateFrom, dateTo, doFilterByDefaultEmployeeAccountDimEmployee: false, input).Select(a => a.AccountId).ToList();
        }

        public List<AccountDTO> GetAccountsFromHierarchyByUserSetting(int actorCompanyId, int roleId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, bool doFilterByDefaultEmployeeAccountDimEmployee = false, AccountHierarchyInput input = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountsFromHierarchyByUserSetting(entities, actorCompanyId, roleId, userId, dateFrom, dateTo, doFilterByDefaultEmployeeAccountDimEmployee, input);
        }

        public List<AccountDTO> GetAccountsFromHierarchyByUserSetting(CompEntities entities, int actorCompanyId, int roleId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, bool doFilterByDefaultEmployeeAccountDimEmployee = false, AccountHierarchyInput input = null)
        {
            return GetAccountsFromHierarchyByUserSetting(entities, out _, actorCompanyId, roleId, userId, dateFrom, dateTo, doFilterByDefaultEmployeeAccountDimEmployee, input);
        }

        public List<AccountDTO> GetAccountsFromHierarchyByUserSetting(CompEntities entities, out AccountRepository accountRepository, int actorCompanyId, int roleId, int userId, DateTime? dateFrom = null, DateTime? dateTo = null, bool doFilterByDefaultEmployeeAccountDimEmployee = false, AccountHierarchyInput input = null)
        {
            accountRepository = GetAccountHierarchyRepositoryByUserSetting(entities, actorCompanyId, roleId, userId, dateFrom, dateTo, input: input);

            List<AccountDTO> accounts;
            if (input.GetValue(AccountHierarchyParamType.IncludeAbstract))
                accounts = accountRepository?.GetAccountsWithAbstract(input.GetValue(AccountHierarchyParamType.IncludeVirtualParented)) ?? new List<AccountDTO>();
            else
                accounts = accountRepository?.GetAccounts(input.GetValue(AccountHierarchyParamType.IncludeVirtualParented)) ?? new List<AccountDTO>();

            if (doFilterByDefaultEmployeeAccountDimEmployee)
                accounts = FilterByDefaultEmployeeAccountDimEmployee(accounts);
            return accounts;
        }

        #endregion

        public List<Account> GetAccountHierarchySettingAccounts(int userId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountHierarchySettingAccounts(entities, userId, actorCompanyId);
        }

        public List<Account> GetAccountHierarchySettingAccounts(CompEntities entities, int userId, int actorCompanyId)
        {
            var (_, isValidSetting, accountIds) = GetAccountHierarchySetting(entities, actorCompanyId, userId);
            return isValidSetting ? GetAccounts(entities, accountIds, actorCompanyId) : new List<Account>();
        }

        public (bool, Account) GetAccountHierarchySettingAccounts(CompEntities entities, int userId, int actorCompanyId, int accountDimId)
        {
            var (_, isValidSetting, accountIds) = GetAccountHierarchySetting(entities, actorCompanyId, userId);
            if (accountIds.IsNullOrEmpty() || accountIds.All(id => id == 0))
                isValidSetting = false;
            Account account = isValidSetting ? GetAccount(entities, actorCompanyId, accountIds, accountDimId) : null;
            return (isValidSetting, account);
        }

        public List<AccountDTO> GetSelectableEmployeeShiftAccounts(int userId, int actorCompanyId, int employeeId, DateTime date, List<AccountDTO> accountInternals = null, List<AccountDimDTO> accountDims = null, bool includeAbstract = false, bool excludeHierarchyNotOnSchedule = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetSelectableEmployeeShiftAccounts(entities, userId, actorCompanyId, employeeId, date, accountInternals, accountDims, includeAbstract: includeAbstract, excludeHierarchyNotOnSchedule: excludeHierarchyNotOnSchedule);
        }

        public List<AccountDTO> GetSelectableEmployeeShiftAccounts(CompEntities entities, int userId, int actorCompanyId, int employeeId, DateTime date, List<AccountDTO> accountInternals = null, List<AccountDimDTO> accountDims = null, bool useEmployeeAccountIfNoAttestRole = false, bool includeAbstract = false, bool excludeHierarchyNotOnSchedule = true)
        {
            int defaultDimId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);

            AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee);
            input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, false);
            input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, useEmployeeAccountIfNoAttestRole);
            input.AddParamValue(AccountHierarchyParamType.IncludeAbstract, includeAbstract);
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, true);
            List<AccountDTO> userSelectableAccountsByDefaultDimSetting = AccountManager.GetAccountsFromHierarchyByUser(entities, actorCompanyId, userId, date, date, input, accountInternals, accountDims).Where(x => x.AccountDimId == defaultDimId).ToList();
            if (employeeId == 0 || employeeId == base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId)))
                return userSelectableAccountsByDefaultDimSetting;

            List<EmployeeAccount> employeeAccounts = EmployeeManager.GetEmployeeAccounts(entities, actorCompanyId, employeeId).GetParentsWithAccountIds().Where(x => x.IsDateValid(date, date)).ToList();
            List<int> intersectedAccountIds = employeeAccounts.Where(x => x.AccountId.HasValue).Select(x => x.AccountId.Value).Intersect(userSelectableAccountsByDefaultDimSetting.Select(x => x.AccountId)).ToList();
            List<AccountDTO> userSelectableAccountsIntersected = userSelectableAccountsByDefaultDimSetting.Where(x => intersectedAccountIds.Contains(x.AccountId)).ToList();

            List<AccountDTO> selectableAccounts = new List<AccountDTO>();
            if (userSelectableAccountsIntersected.Any(a => a.HierarchyNotOnSchedule))
            {
                List<int> employeeAccountIdsWithNoParents = employeeAccounts.Where(e => e.AccountId.HasValue && !e.ParentEmployeeAccountId.HasValue).Select(e => e.AccountId.Value).ToList();
                List<AccountDTO> allAccounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId)).ToList();
                List<int> accountDimIdsOnSameLevel = allAccounts.Where(a => intersectedAccountIds.Contains(a.AccountId)).Select(a => a.AccountDimId).Distinct().ToList();
                foreach (var accountDimId in accountDimIdsOnSameLevel)
                {
                    selectableAccounts.AddRange(allAccounts.Where(a => a.AccountDimId == accountDimId && (!excludeHierarchyNotOnSchedule || !a.HierarchyNotOnSchedule) && employeeAccountIdsWithNoParents.Contains(a.AccountId)));
                }
            }
            else
            {
                selectableAccounts = userSelectableAccountsIntersected;
            }

            return selectableAccounts;
        }

        public bool IsUserExecutiveForEmployee(CompEntities entities, int userId, int actorCompanyId, int employeeId, DateTime date, bool useEmployeeAccountIfNoAttestRole = false, bool includeAbstract = false)
        {
            if (employeeId == 0 || employeeId == base.GetHiddenEmployeeIdFromCache(entities, CacheConfig.Company(actorCompanyId)))
                return true;

            List<AccountDTO> accounts = GetSelectableEmployeeShiftAccounts(entities, userId, actorCompanyId, employeeId, date, null, null, useEmployeeAccountIfNoAttestRole, includeAbstract, false);
            return accounts.Any();
        }

        public List<AccountDTO> GetValidAccountsForEmployee(int actorCompanyId, int employeeId, int roleId, int userId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultAccounts = false, bool onlyParents = false, bool includeAccountsBeneathChild = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetValidAccountsForEmployee(entities, actorCompanyId, employeeId, roleId, userId, dateFrom, dateTo, onlyDefaultAccounts, onlyParents, includeAccountsBeneathChild);
        }

        public List<AccountDTO> GetValidAccountsForEmployee(CompEntities entities, int actorCompanyId, int employeeId, int roleId, int userId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultAccounts = false, bool onlyParents = false, bool includeAccountsBeneathChild = false)
        {
            List<AccountDTO> validAccounts = new List<AccountDTO>();
            AccountDimDTO shiftTypeAccountDim = includeAccountsBeneathChild ? base.GetShiftTypeAccountDimFromCache(entities, actorCompanyId, loadAccounts: true) : null;

            // Get all accounts on specified employee
            List<EmployeeAccount> employeeAccounts = (from ea in entities.EmployeeAccount.Include("Account")
                                                      where ea.EmployeeId == employeeId &&
                                                      ea.AccountId.HasValue &&
                                                      ea.State == (int)SoeEntityState.Active
                                                      select ea).ToList();

            // Filter on dates and default
            employeeAccounts = employeeAccounts.GetEmployeeAccountsByAccount(employeeId, null, dateFrom, dateTo, onlyDefaultAccounts: onlyDefaultAccounts);
            if (employeeAccounts.Count == 0)
                return validAccounts;

            if (shiftTypeAccountDim == null && userId != 0)
            {
                //fix for Axfood, but why cant we just always use GetAccountsFromHierarchyByUser even for employees

                AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.IncludeVirtualParented);
                input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, onlyDefaultAccounts);
                input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, true);

                validAccounts = GetAccountsFromHierarchyByUser(entities, actorCompanyId, userId, dateFrom, dateTo, input);
            }
            else
            {
                // Get parent accounts
                List<EmployeeAccount> employeeAccountParents = employeeAccounts.GetParentsWithAccountIds();
                foreach (EmployeeAccount employeeAccountParent in employeeAccountParents)
                {
                    // Validate date range on parent
                    if (!employeeAccountParent.IsDateValid(dateFrom, dateTo))
                        continue;

                    AccountDTO parentAccount = employeeAccountParent.Account.ToDTO();
                    if (parentAccount == null)
                        continue;

                    if (onlyParents)
                    {
                        validAccounts.Add(parentAccount);
                        continue;
                    }

                    // Get child accounts. If no children, parent is valid
                    List<EmployeeAccount> employeeAccountChildrens = employeeAccounts.GetChildrensWithAccountId(employeeAccountParent);
                    if (!employeeAccountChildrens.Any())
                        validAccounts.Add(parentAccount);

                    if (employeeAccountChildrens.Any())
                    {
                        foreach (EmployeeAccount employeeAccountChild in employeeAccountChildrens)
                        {
                            // Validate date range on child
                            if (!employeeAccountChild.IsDateValid(dateFrom, dateTo))
                                continue;

                            AccountDTO childAccount = employeeAccountChild.Account.ToDTO();
                            if (childAccount == null)
                                continue;

                            childAccount.ParentAccountId = parentAccount.AccountId;

                            // Child is valid
                            validAccounts.Add(childAccount);

                            if (includeAccountsBeneathChild && shiftTypeAccountDim != null && shiftTypeAccountDim.Accounts != null)
                                validAccounts.AddRange(childAccount.GetChildrens(shiftTypeAccountDim.Accounts, true));
                        }
                    }
                    else
                    {
                        if (includeAccountsBeneathChild && shiftTypeAccountDim != null && shiftTypeAccountDim.Accounts != null)
                            validAccounts.AddRange(parentAccount.GetChildrens(shiftTypeAccountDim.Accounts, true));
                    }
                }

            }
            return validAccounts;
        }

        public Dictionary<DateTime, bool> GetValidDatesOnGivenAccounts<T1, T2>(CompEntities entities, int employeeId, List<DateTime> dates, int accountId, List<AccountDTO> allAccounts, List<T1> scheduleBlocks, List<T2> transactions, bool acceptEmptyAsValid = true)
            where T1 : IScheduleBlockAccounting
            where T2 : IPayrollTransactionAccounting
        {
            return GetValidDatesOnGivenAccounts(entities, employeeId, dates.Min(), dates.Max(), accountId, allAccounts, scheduleBlocks, transactions, acceptEmptyAsValid);
        }


        public Dictionary<DateTime, bool> GetValidDatesOnGivenAccounts<T1, T2>(int employeeId, DateTime dateFrom, DateTime dateTo, int accountId, List<AccountDTO> allAccounts, List<T1> scheduleBlocks, List<T2> transactions, bool acceptEmptyAsValid = true)
            where T1 : IScheduleBlockAccounting
            where T2 : IPayrollTransactionAccounting
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetValidDatesOnGivenAccounts(entities, employeeId, dateFrom, dateTo, accountId, allAccounts, scheduleBlocks, transactions, acceptEmptyAsValid);
        }

        public Dictionary<DateTime, bool> GetValidDatesOnGivenAccounts<T1, T2>(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo, int accountId, List<AccountDTO> allAccounts, List<T1> scheduleBlocks, List<T2> transactions, bool acceptEmptyAsValid = true)
            where T1 : IScheduleBlockAccounting
            where T2 : IPayrollTransactionAccounting
        {
            if (accountId == 0)
                return new Dictionary<DateTime, bool>();

            Dictionary<DateTime, List<int>> accountIdsByDate = new Dictionary<DateTime, List<int>>();
            List<int> accountIds = new List<int> { accountId };

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                accountIdsByDate.Add(date, accountIds);
                date = date.AddDays(1);
            }

            return GetValidDatesOnGivenAccounts(entities, employeeId, dateFrom, dateTo, accountIdsByDate, allAccounts, scheduleBlocks, transactions, acceptEmptyAsValid);
        }

        /// <summary>
        /// Get dates that contains schedule or transactions with any of the given accounts
        /// </summary>
        /// <typeparam name="T1">Type of schedule</typeparam>
        /// <typeparam name="T2">Type of transactions</typeparam>
        /// <param name="employeeId">The Employee</param>
        /// <param name="dateFrom">The start of the interval to check</param>
        /// <param name="dateTo">The stop of the interval to check</param>
        /// <param name="accountIdsByDate">The given account to check schedule and transactions against</param>
        /// <param name="allAccounts">All of the AccounInternals for the Company</param>
        /// <param name="scheduleBlocks">The schedule for the Employee during the interval</param>
        /// <param name="transactions">The transactions for the Employee during the interval</param>
        /// <returns>List of valid dates. The value is true if the wholeday is valid, and false if part of the day is valid</returns>
        public Dictionary<DateTime, bool> GetValidDatesOnGivenAccounts<T1, T2>(int employeeId, DateTime dateFrom, DateTime dateTo, Dictionary<DateTime, List<int>> accountIdsByDate, List<AccountDTO> allAccounts, List<T1> scheduleBlocks, List<T2> transactions, bool acceptEmptyAsValid = true)
            where T1 : IScheduleBlockAccounting
            where T2 : IPayrollTransactionAccounting
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetValidDatesOnGivenAccounts(entities, employeeId, dateFrom, dateTo, accountIdsByDate, allAccounts, scheduleBlocks, transactions, acceptEmptyAsValid);
        }

        public Dictionary<DateTime, bool> GetValidDatesOnGivenAccounts<T1, T2>(CompEntities entities, int employeeId, DateTime dateFrom, DateTime dateTo, Dictionary<DateTime, List<int>> accountIdsByDate, List<AccountDTO> allAccounts, List<T1> scheduleBlocks, List<T2> transactions, bool acceptEmptyAsValid = true)
            where T1 : IScheduleBlockAccounting
            where T2 : IPayrollTransactionAccounting
        {
            Dictionary<DateTime, bool> dates = new Dictionary<DateTime, bool>();

            if (accountIdsByDate.IsNullOrEmpty() || accountIdsByDate.SelectMany(i => i.Value).IsNullOrEmpty())
                return dates;

            var scheduleBlocksValid = scheduleBlocks?.Where(i => i.EmployeeId == employeeId && i.Date.HasValue).ToList();
            var transactionsValid = transactions?.Where(i => i.EmployeeId == employeeId && !i.AccountInternalIds.IsNullOrEmpty()).ToList();
            if (scheduleBlocksValid.IsNullOrEmpty() && transactionsValid.IsNullOrEmpty())
            {
                if (acceptEmptyAsValid)
                    return CalendarUtility.GetDatesInInterval(dateFrom, dateTo).ToDictionary(k => k, v => true);
                else
                    return dates;
            }

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                if (accountIdsByDate.ContainsKey(date))
                {
                    List<int> attestRoleAccountIds = accountIdsByDate[date];
                    bool hasData = false;
                    if (transactionsValid.AnyContainsAccount(date, attestRoleAccountIds, out bool allContainsAccount, ref hasData) ||
                        scheduleBlocksValid.ContainsAccount(date, attestRoleAccountIds, allAccounts, out allContainsAccount, ref hasData))
                        dates.Add(date, allContainsAccount);
                    else if (!hasData && acceptEmptyAsValid)
                        dates.Add(date, true);
                    else if (base.IsMartinServera(entities) && IsUserExecutiveForEmployee(entities, base.UserId, base.ActorCompanyId, employeeId, date, includeAbstract: true))
                        dates.Add(date, true);
                }

                date = date.AddDays(1);
            }

            return dates;
        }

        public List<int> GetValidAccountIdsForEmployee(int actorCompanyId, int employeeId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultAccounts, bool onlyParents, bool includeAccountsBeneathChild)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetValidAccountIdsForEmployee(entities, actorCompanyId, employeeId, 0, 0, dateFrom, dateTo, onlyDefaultAccounts, onlyParents, includeAccountsBeneathChild);
        }

        public List<int> GetValidAccountIdsForEmployee(CompEntities entities, int actorCompanyId, int employeeId, int roleId, int userId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultAccounts, bool onlyParents, bool includeAccountsBeneathChild)
        {
            return GetValidAccountsForEmployee(entities, actorCompanyId, employeeId, roleId, userId, dateFrom, dateTo, onlyDefaultAccounts, onlyParents, includeAccountsBeneathChild)
                .Select(a => a.AccountId)
                .ToList();
        }

        public int GetAccountHierarchySettingAccountId(CompEntities entities, bool? useAccountHierarchy = null)
        {
            return GetAccountHierarchySettingAccount(entities, useAccountHierarchy)?.AccountId ?? 0;
        }

        public Account GetAccountHierarchySettingAccount(bool? useAccountHierarchy = null, int? actorCompanyId = null, int? userId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountHierarchySettingAccount(entities, useAccountHierarchy, actorCompanyId, userId);
        }

        public Account GetAccountHierarchySettingAccount(CompEntities entities, bool? useAccountHierarchy = null, int? actorCompanyId = null, int? userId = null)
        {
            if (!TryLoadAccountHierarchyParams(entities, out int defaultDimId, ref useAccountHierarchy, ref actorCompanyId, ref userId))
                return null;

            var (_, account) = GetAccountHierarchySettingAccounts(entities, userId.Value, actorCompanyId.Value, defaultDimId);
            return account;
        }

        public List<int> GetAccountHierarchySettingAccounts(CompEntities entities, bool? useAccountHierarchy = null, int? actorCompanyId = null, int? userId = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool forceDefaultDim = false)
        {
            if (!TryLoadAccountHierarchyParams(entities, out int defaultDimId, ref useAccountHierarchy, ref actorCompanyId, ref userId))
                return null;

            var (isValidSetting, account) = GetAccountHierarchySettingAccounts(entities, userId.Value, actorCompanyId.Value, defaultDimId);
            if (forceDefaultDim && account?.AccountDimId != defaultDimId)
                isValidSetting = false;
            if (isValidSetting)
                return account?.AccountId.ObjToList();

            if (!TryLoadAccountHierachyRepositoryRequisites(entities, actorCompanyId.Value, out List<AccountDimDTO> accountDims, out List<AccountDTO> accountInternals))
                return null;

            List<AttestRoleUser> attestRoleUsers = AttestManager.GetAttestRoleUsers(entities, actorCompanyId.Value, userId.Value, dateFrom, dateTo, onlyDefaultAccounts: true, includeAttestRole: true);
            return attestRoleUsers.GetValidAccounts(accountDims, accountInternals, dateFrom, dateTo).Where(a => a.AccountDimId == defaultDimId).Select(a => a.AccountId).ToList();
        }

        public string GetAccountHierarchySettingAccountNames(int userId, int actorCompanyId, bool addChooseAccountIfSettingNotExists)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var (hierarchyId, isValidSetting, accountIds) = GetAccountHierarchySetting(entitiesReadOnly, actorCompanyId, userId);
            if (string.IsNullOrEmpty(hierarchyId) && addChooseAccountIfSettingNotExists)
                return GetText(8815, "Alla dina konton");
            if (!isValidSetting)
                return "";

            List<Account> accounts = (from a in entitiesReadOnly.Account
                                      where accountIds.Contains(a.AccountId) &&
                                      a.ActorCompanyId == actorCompanyId
                                      select a).ToList();

            StringBuilder sb = new StringBuilder();

            int counter = 0;
            foreach (int accountId in accountIds)
            {
                counter++;
                Account account = accounts.FirstOrDefault(a => a.AccountId == accountId);
                if (account != null)
                {
                    sb.Append(account.Name);
                    if (counter < accountIds.Count)
                        sb.Append(AccountDTO.HIERARCHYDELIMETER);
                }
            }

            return sb.ToString();
        }

        public bool TryGetAccountIdsForEmployeeAccountDim(CompEntities entities, out AccountRepository accountRepository, int actorCompanyId, int roleId, int userId, DateTime dateFrom, DateTime dateTo, out List<int> validAccountIds, out int employeeAccountDimId)
        {
            accountRepository = null;
            validAccountIds = new List<int>();
            employeeAccountDimId = 0;

            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId);

            if (useAccountHierarchy)
            {
                AccountHierarchyInput input = AccountHierarchyInput.GetInstance(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee, AccountHierarchyParamType.IncludeVirtualParented);
                List<AccountDTO> validAccounts = AccountManager.GetAccountsFromHierarchyByUserSetting(entities, out accountRepository, actorCompanyId, roleId, userId, dateFrom, dateTo, input: input);

                bool doAttestByEmployeeAccount = accountRepository?.AttestRoleUsers?.Any(i => i.AttestRole.AttestByEmployeeAccount) ?? false;
                if (!doAttestByEmployeeAccount)
                {
                    validAccountIds = validAccounts?.Select(i => i.AccountId).ToList() ?? new List<int>();
                    employeeAccountDimId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);
                }
            }

            return !validAccountIds.IsNullOrEmpty() && employeeAccountDimId > 0;
        }

        private bool TryLoadAccountHierarchyParams(CompEntities entities, out int defaultDimId, ref bool? useAccountHierarchy, ref int? actorCompanyId, ref int? userId)
        {
            actorCompanyId = actorCompanyId ?? base.ActorCompanyId;
            userId = userId ?? base.UserId;
            useAccountHierarchy = useAccountHierarchy ?? base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId.Value);
            defaultDimId = useAccountHierarchy.Value ? SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId.Value, 0) : 0;
            return useAccountHierarchy.Value;
        }

        public List<AccountDTO> GetSiblingAccounts(int actorCompanyId, int accountId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            bool accountHierachySiblingsHaveSameParent = SettingManager.GetBoolSetting(entitiesReadOnly, SettingMainType.Company, (int)CompanySettingType.AccountHierachySiblingsHaveSameParent, 0, actorCompanyId, 0);

            var account = accounts.FirstOrDefault(f => f.AccountId == accountId);
            if (account != null && account.ParentAccountId.HasValue)
            {
                accounts = (from a in accounts
                            where a.AccountDimId == account.AccountDimId &&
                            (!accountHierachySiblingsHaveSameParent || account.ParentAccountId == a.ParentAccountId) &&
                            a.State == (int)SoeEntityState.Active
                            select a).ToList();
            }

            return accounts;
        }

        #endregion

        #region AccountVatRateView

        /// <summary>
        /// Get all Accounts with VAT rates for the given AccountDim
        /// </summary>
        /// <param name="accountDimId">The AccountDimId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A collection of AccountVatRateView entities</returns>
        public IEnumerable<AccountVatRateViewDTO> GetAccountVatRates(int accountDimId, int actorCompanyId, bool addVatFreeRow)
        {
            var list = new List<AccountVatRateViewDTO>();

            if (addVatFreeRow)
            {
                string vatFreeLabel = GetText(3389, "Momsfri");
                list.Add(new AccountVatRateViewDTO() { AccountId = 0, AccountNr = vatFreeLabel, Name = vatFreeLabel, VatRate = 0, AccountDimId = accountDimId, ActorCompanyId = actorCompanyId });
            }

            var vatRates = GetSysVatRates();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountVatRateView.NoTracking();
            var query = (from a in entities.AccountVatRateView
                         where a.AccountDimId == accountDimId &&
                         a.ActorCompanyId == actorCompanyId
                         orderby a.AccountNr
                         select a).ToList();

            var dtos = query.ToDTOs();

            foreach (var dto in dtos)
            {
                if (dto.SysVatAccountId.HasValue)
                {
                    var rate = vatRates.FirstOrDefault(r => r.SysVatAccountId == dto.SysVatAccountId);
                    if (rate != null && rate.IsActive == 1)
                    {
                        dto.VatRate = rate.VatRate;
                        list.Add(dto);
                    }
                }
            }

            return dtos;
        }

        public IEnumerable<AccountVatRateViewSmallDTO> GetAccountVatRatesForStdDim(int actorCompanyId, bool addVatFreeRow)
        {
            int accountDimId = GetAccountDimStdId(actorCompanyId);

            var dtos = GetAccountVatRates(accountDimId, actorCompanyId, addVatFreeRow);

            var smallDtos = new List<AccountVatRateViewSmallDTO>();

            foreach (var dto in dtos)
            {
                smallDtos.Add(new AccountVatRateViewSmallDTO
                {
                    AccountId = dto.AccountId,
                    AccountNr = dto.AccountNr,
                    Name = dto.Name,
                    VatRate = dto.VatRate
                });
            }

            return smallDtos;
        }

        /// <summary>
        /// Get a Account with VAT rate
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountId">The AccountId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A collection of AccountVatRateView entities</returns>
        public AccountVatRateViewDTO GetAccountVatRate(CompEntities entities, int accountId, int actorCompanyId)
        {
            var vatRates = GetSysVatRates();

            var rateView = (from a in entities.AccountVatRateView
                            where a.AccountId == accountId &&
                            a.ActorCompanyId == actorCompanyId
                            select a).FirstOrDefault();

            var dto = rateView.ToDTO();

            // Initialize to zero
            if (dto != null)
            {
                dto.VatRate = 0;
                if (dto.SysVatAccountId.HasValue)
                {
                    var rate = vatRates.FirstOrDefault(r => r.SysVatAccountId == dto.SysVatAccountId);
                    if (rate != null && rate.IsActive == 1)
                    {
                        dto.VatRate = rate.VatRate;
                    }
                }
            }

            return dto;
        }

        #endregion

        #region AccountStd

        // Replace account names with translated ones when applicable 
        // Used when printing bookkeeping reports on companies which
        // Have several comp. languages and wishes to translate account 
        // Names on language of the user printing report
        public List<AccountStd> TranslateAccountNamesByLang(List<AccountStd> accounts)
        {
            int langId = GetLangId();
            if (langId <= 0)
                return accounts;

            var terms = TermManager.GetCompTermsByLang(CompTermsRecordType.AccountName, langId).ToLookup(t => t.RecordId);

            if (terms.Any())
            {
                foreach (var account in accounts.Where(i => i.AccountId > 0))
                {
                    var compTerm = terms[account.Account.AccountId].FirstOrDefault();
                    if (compTerm != null && !string.IsNullOrEmpty(compTerm.Name))
                        account.Account.Name = compTerm.Name;
                }
            }
            return accounts;
        }

        public List<AccountDTO> TranslateAccountNamesByLang(List<AccountDTO> accounts)
        {
            int langId = GetLangId();
            if (langId <= 0)
                return accounts;

            var terms = TermManager.GetCompTermsByLang(CompTermsRecordType.AccountName, langId).ToLookup(t => t.RecordId);

            if (terms.Any())
            {
                foreach (var account in accounts.Where(i => i.AccountId > 0))
                {
                    var compTerm = terms[account.AccountId].FirstOrDefault();
                    if (compTerm != null && !string.IsNullOrEmpty(compTerm.Name))
                        account.Name = compTerm.Name;
                }
            }

            return accounts;
        }

        public List<AccountStd> GetAccountStdsByCompany(int actorCompanyId, bool? active, bool loadAccountSru = false, bool loadAccountDim = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountStd.NoTracking();
            return GetAccountStdsByCompany(entities, actorCompanyId, active, loadAccountSru, loadAccountDim);
        }

        public List<AccountStd> GetAccountStdsByCompany(CompEntities entities, int actorCompanyId, bool? active, bool loadAccountSru = false, bool loadAccountDim = false)
        {
            IQueryable<AccountStd> query = (from a in entities.AccountStd.Include("Account")
                                            where a.Account.ActorCompanyId == actorCompanyId
                                            select a);

            if (loadAccountSru)
                query = query.Include("AccountSru");
            if (loadAccountDim)
                query = query.Include("Account.AccountDim");

            List<AccountStd> accountStds = null;
            if (active == true)
                accountStds = query.Where(a => a.Account.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                accountStds = query.Where(a => a.Account.State == (int)SoeEntityState.Inactive).ToList();
            else
                accountStds = query.Where(a => a.Account.State != (int)SoeEntityState.Deleted).ToList();

            return accountStds;
        }

        public List<AccountStd> GetAccountStdsByCompanyIgnoreState(CompEntities entities, int actorCompanyId)
        {
            return (from a in entities.AccountStd
                              .Include("Account")
                    where a.Account.ActorCompanyId == actorCompanyId
                    select a).ToList();
        }

        public List<AccountDTO> GetAccountStdBySearch(int actorCompanyId, string search, int take)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountStd.NoTracking();
            return GetAccountStdBySearch(entities, actorCompanyId, search, take);
        }

        public List<AccountDTO> GetAccountStdBySearch(CompEntities entities, int actorCompanyId, string search, int take)
        {

            return (from a in entities.AccountStd
                    where a.Account.ActorCompanyId == actorCompanyId &&
                    (a.Account.Name.ToLower().Contains(search.ToLower()) || a.Account.AccountNr.ToLower().Contains(search.ToLower())) &&
                    a.Account.State == (int)SoeEntityState.Active
                    orderby a.Account.Name, a.Account.AccountNr ascending
                    select new AccountDTO()
                    {
                        AccountDimId = a.Account.AccountDimId,
                        AccountDimNr = a.Account.AccountDim.AccountDimNr,
                        AccountId = a.AccountId,
                        AccountNr = a.Account.AccountNr,
                        Name = a.Account.Name,
                        AmountStop = a.AmountStop,
                        UnitStop = a.UnitStop,
                    }).Take(take).ToList();
        }

        public List<AccountDTO> GetAccountStds(CompEntities entities, int actorCompanyId, List<int> accountIds, bool onlyActive, List<AccountDTO> allAccounts = null)
        {
            if (!allAccounts.IsNullOrEmpty())
                return allAccounts.Where(w => accountIds.Contains(w.AccountId)).ToList();

            return GetAccountStds(entities, actorCompanyId, accountIds, onlyActive, true, true).ToDTOs().ToList();
        }

        public List<AccountStd> GetAccountStds(CompEntities entities, int actorCompanyId, List<int> accountIds, bool onlyActive, bool loadAccount, bool loadAccountDim)
        {
            IQueryable<AccountStd> query = (from a in entities.AccountStd
                                            where accountIds.Contains(a.AccountId) &&
                                            a.Account.ActorCompanyId == actorCompanyId &&
                                            a.Account.State != (int)SoeEntityState.Deleted
                                            select a);


            if (loadAccount)
                query = query.Include("Account");
            if (loadAccountDim)
                query = query.Include("Account.AccountDim");

            if (onlyActive)
                query = query.Where(a => a.Account.State == (int)SoeEntityState.Active);

            return query.ToList();
        }

        public AccountStd GetAccountStdFromCompanySetting(CompanySettingType setting, int actorCompanyId, bool loadAccountDim = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountStd.NoTracking();
            return GetAccountStdFromCompanySetting(entities, setting, actorCompanyId, loadAccountDim);
        }

        public AccountStd GetAccountStdFromCompanySetting(CompEntities entities, CompanySettingType setting, int actorCompanyId, bool loadAccountDim = false)
        {
            int accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)setting, 0, actorCompanyId, 0);
            return GetAccountStd(entities, accountId, actorCompanyId, true, loadAccountDim);
        }

        public AccountStd GetAccountStd(int actorCompanyId, int accountId, bool loadAccount, bool loadAccountDim)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountStd.NoTracking();
            return GetAccountStd(entities, accountId, actorCompanyId, loadAccount, loadAccountDim);
        }
        public List<AccountStd> GetAccountStds(int actorCompanyId, string accounts)
        {
            List<AccountStd> stdAccounts = new List<AccountStd>();
            if (String.IsNullOrEmpty(accounts))
                return stdAccounts;

            string[] records = accounts.Split(',');
            if (records.Length > 0)
            {
                foreach (var record in records)
                {
                    string[] valuePair = record.Split(':');

                    int accountId = 0;
                    Int32.TryParse(valuePair[0], out accountId);
                    if (accountId == 0)
                        continue;

                    AccountStd accountStd = GetAccountStd(actorCompanyId, accountId, true, false);
                    if (accountStd != null)
                    {
                        stdAccounts.Add(accountStd);
                    }
                }
            }

            return stdAccounts;
        }

        public AccountStd GetAccountStd(CompEntities entities, int accountId, int actorCompanyId, bool loadAccount, bool loadAccountDim, bool loadInactive = false)
        {
            if (accountId == 0)
                return null;

            IQueryable<AccountStd> query = entities.AccountStd;

            if (loadAccount)
                query = query.Include("Account").Include("AccountSru");
            if (loadAccountDim)
                query = query.Include("Account.AccountDim");

            if (loadInactive)
                return query.Where(a => a.AccountId == accountId && a.Account.ActorCompanyId == actorCompanyId && (a.Account.State == (int)SoeEntityState.Active || a.Account.State == (int)SoeEntityState.Inactive)).FirstOrDefault();
            else
                return query.Where(a => a.AccountId == accountId && a.Account.ActorCompanyId == actorCompanyId && a.Account.State == (int)SoeEntityState.Active).FirstOrDefault();
        }

        public AccountStd GetAccountStdByNr(string accountNr, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountStd.NoTracking();
            return GetAccountStdByNr(entities, accountNr, actorCompanyId);
        }

        public AccountStd GetAccountStdByNr(CompEntities entities, string accountNr, int actorCompanyId)
        {
            return (from a in entities.AccountStd
                    .Include("Account")
                    where a.Account.AccountNr == accountNr &&
                    a.Account.ActorCompanyId == actorCompanyId &&
                    a.Account.State == (int)SoeEntityState.Active
                    select a).FirstOrDefault<AccountStd>();
        }

        public AccountStd GetAccountStdFromInvoiceProductOrCompany(InvoiceProduct invoiceProduct, ProductAccountType productAccountType, CompanySettingType companySettingType, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountStd.NoTracking();
            return GetAccountStdFromInvoiceProductOrCompany(entities, invoiceProduct, productAccountType, companySettingType, actorCompanyId);
        }

        public AccountStd GetAccountStdFromInvoiceProductOrCompany(CompEntities entities, InvoiceProduct invoiceProduct, ProductAccountType productAccountType, CompanySettingType companySettingType, int actorCompanyId)
        {
            if (invoiceProduct == null)
                return null;

            AccountStd accountStd = null;

            if (!invoiceProduct.ProductAccountStd.IsLoaded)
                invoiceProduct.ProductAccountStd.Load();

            //Try get AccountStd from ProductAccountStd
            ProductAccountStd productAccountStd = invoiceProduct.ProductAccountStd.FirstOrDefault(i => i.Type == (int)productAccountType);
            if (productAccountStd?.AccountStd != null)
            {
                if (!productAccountStd.AccountStdReference.IsLoaded)
                    productAccountStd.AccountStdReference.Load();
                accountStd = productAccountStd.AccountStd;
            }
            else
            {
                //Get AccountStd from CompanySetting
                int accountId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)companySettingType, 0, actorCompanyId, 0);
                accountStd = GetAccountStd(entities, accountId, actorCompanyId, false, false);
            }

            return accountStd;
        }
        public List<AccountStd> GetAccountStds(int actorCompanyId, bool addEmptyRow)
        {
            return GetAccountStdsByCompany(actorCompanyId, true);
        }

        public Dictionary<int, string> GetAccountStdsDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<AccountStd> accounts = GetAccountStdsByCompany(actorCompanyId, true);
            foreach (AccountStd account in accounts)
            {
                dict.Add(account.AccountId, account.Account.AccountNrPlusName);
            }

            return dict;
        }

        public List<AccountNumberNameDTO> GetAccountStdsNumberName(int actorCompanyId, bool addEmptyRow, int? accountTypeId = null)
        {
            List<AccountNumberNameDTO> dtos = new List<AccountNumberNameDTO>();

            if (addEmptyRow)
                dtos.Add(new AccountNumberNameDTO() { AccountId = 0, Name = String.Empty, Number = String.Empty, NumberName = String.Empty });

            List<AccountStd> accounts = GetAccountStdsByCompany(actorCompanyId, true);
            if (accountTypeId.HasValue)
                accounts = accounts.Where(a => a.AccountTypeSysTermId == accountTypeId.Value).ToList();
            foreach (AccountStd account in accounts)
            {
                dtos.Add(new AccountNumberNameDTO() { AccountId = account.AccountId, Number = account.Account.AccountNr, Name = account.Account.Name, NumberName = account.Account.AccountNr + " " + account.Account.Name });
            }

            return dtos;
        }

        public int GetNrOfAccountStds(int actorCompanyId)
        {
            return GetAccountStdsByCompany(actorCompanyId, true).Count;
        }

        public bool IsAccountStdUsed(CompEntities entities, int accountId, int actorCompanyId)
        {
            //Ecnonomy/Billing
            if (IsAccountStdUsedInVoucherRow(entities, accountId))
                return true;
            if (IsAccountStdUsedInProject(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInInventory(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInAccountDistributionEntryRow(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInBudgetRow(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInAccountBalance(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInAccountSru(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInPaymentMethod(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInVatCode(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInInvoiceTransaction(entities, accountId, actorCompanyId))
                return true;

            //Time/Payroll
            if (IsAccountStdUsedInEmployee(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInEmployeeGroup(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInPayrollGroup(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInPayrollProduct(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInPayrollScheduleTransaction(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInPayrollTransaction(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInMassRegistrationTemplateHead(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountStdUsedInStaffingNeedsFrequancy(entities, accountId, actorCompanyId))
                return true;

            return false;
        }

        public bool IsAccountStdUsedInVoucherRow(CompEntities entities, int accountId)
        {
            return (from vr in entities.VoucherRow
                    where vr.AccountId == accountId
                    select vr).Any();
        }

        public bool IsAccountStdUsedInProject(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.ProjectAccountStd
                    where e.Project.ActorCompanyId == actorCompanyId &&
                    e.Project.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInInventory(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.InventoryAccountStd
                    where e.Inventory.ActorCompanyId == actorCompanyId &&
                    e.Inventory.State == (int)SoeEntityState.Active &&
                    e.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInAccountDistributionEntryRow(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.AccountDistributionEntryRow
                    where e.AccountDistributionEntry.ActorCompanyId == actorCompanyId &&
                    e.AccountDistributionEntry.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInBudgetRow(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.BudgetRow
                    where e.BudgetHead.ActorCompanyId == actorCompanyId &&
                    e.BudgetHead.State == (int)SoeEntityState.Active &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInAccountBalance(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.AccountBalance
                    where e.AccountYear.ActorCompanyId == actorCompanyId &&
                    e.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInAccountSru(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.AccountSru
                    where e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInPaymentMethod(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.PaymentMethod
                    where e.Company.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInVatCode(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.VatCode
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInEmployee(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.EmploymentAccountStd
                    where e.Employment.ActorCompanyId == actorCompanyId &&
                    e.Employment.State == (int)SoeEntityState.Active &&
                    e.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInEmployeeGroup(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.EmployeeGroupAccountStd
                    where e.EmployeeGroup.ActorCompanyId == actorCompanyId &&
                    e.EmployeeGroup.State == (int)SoeEntityState.Active &&
                    e.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInPayrollGroup(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.PayrollGroupAccountStd
                    where e.PayrollGroup.ActorCompanyId == actorCompanyId &&
                    e.PayrollGroup.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInPayrollProduct(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.ProductAccountStd
                    where e.Product.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    e.Product.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInPayrollScheduleTransaction(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.TimePayrollScheduleTransaction
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInPayrollTransaction(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.TimePayrollTransaction
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInInvoiceTransaction(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.TimeInvoiceTransaction
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInMassRegistrationTemplateHead(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.MassRegistrationTemplateHead
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountStd.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountStdUsedInStaffingNeedsFrequancy(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.StaffingNeedsFrequency
                    where e.ActorCompanyId == actorCompanyId &&
                    e.AccountId == accountId
                    select e).Any();
        }

        public int? GetAccountStdTypeFromAccountNr(string accountNr)
        {
            if (!String.IsNullOrEmpty(accountNr) && Int32.TryParse(accountNr.Substring(0, 1), out int accountClass))
            {
                switch (accountClass)
                {
                    case 1:
                        return (int)TermGroup_AccountType.Asset;
                    case 2:
                        return (int)TermGroup_AccountType.Debt;
                    case 3:
                        return (int)TermGroup_AccountType.Income;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        return (int)TermGroup_AccountType.Cost;
                }
            }

            return null;
        }

        public int GetAccountStdIdFromAccountNr(string accountNr, int actorCompanyId)
        {
            if (String.IsNullOrEmpty(accountNr))
                return 0;

            AccountStd account = GetAccountStdByNr(accountNr, actorCompanyId);
            return account != null ? account.AccountId : 0;
        }

        #endregion

        #region AccountInternal

        public List<AccountInternalDTO> ConvertToAccountInternalDTOs(List<AccountDimDTO> accountDims, List<AccountInternalDTO> allCompanyAccountInternalDTOs, string dim2AccountNr, string dim3AccountNr, string dim4AccountNr, string dim5AccountNr, string dim6AccountNr)
        {
            List<AccountInternalDTO> accountInternalDTOs = new List<AccountInternalDTO>();

            int accounDim2Id = 0;
            int accounDim3Id = 0;
            int accounDim4Id = 0;
            int accounDim5Id = 0;
            int accounDim6Id = 0;
            int accountDimCounter = 2;

            //Number the AccountDims
            if (accountDims.Any())
            {
                accountDims = accountDims.OrderBy(a => a.AccountDimNr).ToList();

                foreach (var dim in accountDims.OrderBy(a => a.AccountDimNr))
                {
                    if (accountDimCounter == 2) accounDim2Id = dim.AccountDimId;
                    if (accountDimCounter == 3) accounDim3Id = dim.AccountDimId;
                    if (accountDimCounter == 4) accounDim4Id = dim.AccountDimId;
                    if (accountDimCounter == 5) accounDim5Id = dim.AccountDimId;
                    if (accountDimCounter == 6) accounDim6Id = dim.AccountDimId;

                    accountDimCounter++;
                }
            }

            if (accounDim2Id != 0 && !string.IsNullOrEmpty(dim2AccountNr))
            {
                AccountInternalDTO accountInternalDim2DTO = new AccountInternalDTO();

                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accounDim2Id && a.AccountNr.Equals(dim2AccountNr));
                if (account != null && accountDims.Any(d => d.AccountDimId == accounDim2Id))
                {
                    accountInternalDim2DTO.AccountId = account.AccountId;
                    accountInternalDim2DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim2Id)?.AccountDimNr ?? 0;
                    accountInternalDim2DTO.AccountDimId = accounDim2Id;
                    accountInternalDim2DTO.AccountNr = dim2AccountNr;
                    accountInternalDim2DTO.Name = account.Name;
                    accountInternalDim2DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim2Id)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalDim2DTO);
                }
            }

            if (accounDim3Id != 0 && !string.IsNullOrEmpty(dim3AccountNr))
            {
                AccountInternalDTO accountInternalDim3DTO = new AccountInternalDTO();

                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accounDim3Id && a.AccountNr.Equals(dim3AccountNr));
                if (account != null && accountDims.Any(d => d.AccountDimId == accounDim3Id))
                {
                    accountInternalDim3DTO.AccountId = account.AccountId;
                    accountInternalDim3DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim3Id)?.AccountDimNr ?? 0;
                    accountInternalDim3DTO.AccountDimId = accounDim3Id;
                    accountInternalDim3DTO.AccountNr = dim3AccountNr;
                    accountInternalDim3DTO.Name = account.Name;
                    accountInternalDim3DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim3Id)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalDim3DTO);
                }
            }


            if (accounDim4Id != 0 && !string.IsNullOrEmpty(dim4AccountNr))
            {
                AccountInternalDTO accountInternalDim4DTO = new AccountInternalDTO();

                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accounDim4Id && a.AccountNr.Equals(dim4AccountNr));
                if (account != null && accountDims.Any(d => d.AccountDimId == accounDim4Id))
                {
                    accountInternalDim4DTO.AccountId = account.AccountId;
                    accountInternalDim4DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim4Id)?.AccountDimNr ?? 0;
                    accountInternalDim4DTO.AccountDimId = accounDim4Id;
                    accountInternalDim4DTO.AccountNr = dim4AccountNr;
                    accountInternalDim4DTO.Name = account.Name;
                    accountInternalDim4DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim4Id)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalDim4DTO);
                }
            }


            if (accounDim5Id != 0 && !string.IsNullOrEmpty(dim5AccountNr))
            {
                AccountInternalDTO accountInternalDim5DTO = new AccountInternalDTO();

                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accounDim5Id && a.AccountNr.Equals(dim5AccountNr));
                if (account != null && accountDims.Any(d => d.AccountDimId == accounDim5Id))
                {
                    accountInternalDim5DTO.AccountId = account.AccountId;
                    accountInternalDim5DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim5Id)?.AccountDimNr ?? 0;
                    accountInternalDim5DTO.AccountDimId = accounDim5Id;
                    accountInternalDim5DTO.AccountNr = dim5AccountNr;
                    accountInternalDim5DTO.Name = account.Name;
                    accountInternalDim5DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim5Id)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalDim5DTO);
                }
            }

            if (accounDim6Id != 0 && !string.IsNullOrEmpty(dim6AccountNr))
            {
                AccountInternalDTO accountInternalDim6DTO = new AccountInternalDTO();

                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accounDim6Id && a.AccountNr.Equals(dim6AccountNr));
                if (account != null && accountDims.Any(d => d.AccountDimId == accounDim6Id))
                {
                    accountInternalDim6DTO.AccountId = account.AccountId;
                    accountInternalDim6DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim6Id)?.AccountDimNr ?? 0;
                    accountInternalDim6DTO.AccountDimId = accounDim6Id;
                    accountInternalDim6DTO.AccountNr = dim6AccountNr;
                    accountInternalDim6DTO.Name = account.Name;
                    accountInternalDim6DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accounDim6Id)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalDim6DTO);
                }
            }

            return accountInternalDTOs;
        }

        public List<AccountInternalDTO> ConvertToAccountInternalDTOs(List<AccountDimDTO> accountDims, List<AccountInternalDTO> allCompanyAccountInternalDTOs, string sieDim1, string sieDim2, string sieDim6, string sieDim7, string sieDim8, string sieDim9, string sieDim10, string sieDim30, string sieDim40, string sieDim50)
        {
            List<AccountInternalDTO> accountInternalDTOs = new List<AccountInternalDTO>();

            //    CostCentre = 1,
            //CostUnit = 2,
            //Project = 6,
            //Employee = 7,
            //Customer = 8,
            //Supplier = 9,
            //Invoice = 10,
            //Region = 30,
            //Shop = 40,
            //Department = 50,

            int accountDimIdSie1 = 0;
            int accountDimIdSie2 = 0;
            int accountDimIdSie6 = 0;
            int accountDimIdSie7 = 0;
            int accountDimIdSie8 = 0;
            int accountDimIdSie9 = 0;
            int accountDimIdSie10 = 0;
            int accountDimIdSie30 = 0;
            int accountDimIdSie40 = 0;
            int accountDimIdSie50 = 0;

            //Number the AccountDims
            if (accountDims.Any())
            {
                accountDims = accountDims.OrderBy(a => a.AccountDimNr).ToList();

                foreach (var dim in accountDims.OrderBy(a => a.AccountDimNr))
                {
                    if (dim.SysSieDimNr.HasValue)
                    {
                        TermGroup_SieAccountDim sie = (TermGroup_SieAccountDim)dim.SysSieDimNr;

                        switch (sie)
                        {
                            case TermGroup_SieAccountDim.CostCentre:
                                accountDimIdSie1 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.CostUnit:
                                accountDimIdSie2 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Project:
                                accountDimIdSie6 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Employee:
                                accountDimIdSie7 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Customer:
                                accountDimIdSie8 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Supplier:
                                accountDimIdSie9 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Invoice:
                                accountDimIdSie10 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Region:
                                accountDimIdSie30 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Shop:
                                accountDimIdSie40 = dim.AccountDimId;
                                break;
                            case TermGroup_SieAccountDim.Department:
                                accountDimIdSie50 = dim.AccountDimId;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(sieDim1) && accountDimIdSie1 != 0)
            {
                AccountInternalDTO accountInternalSie1DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie1 && a.AccountNr.Equals(sieDim1, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie1DTO.AccountId = account.AccountId;
                    accountInternalSie1DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie1)?.AccountDimNr ?? 0;
                    accountInternalSie1DTO.AccountDimId = accountDimIdSie1;
                    accountInternalSie1DTO.AccountNr = account.AccountNr;
                    accountInternalSie1DTO.Name = account.Name;
                    accountInternalSie1DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie1)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie1DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim2) && accountDimIdSie2 != 0)
            {
                AccountInternalDTO accountInternalSie2DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie2 && a.AccountNr.Equals(sieDim2, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie2DTO.AccountId = account.AccountId;
                    accountInternalSie2DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie2)?.AccountDimNr ?? 0;
                    accountInternalSie2DTO.AccountDimId = accountDimIdSie2;
                    accountInternalSie2DTO.AccountNr = account.AccountNr;
                    accountInternalSie2DTO.Name = account.Name;
                    accountInternalSie2DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie2)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie2DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim6) && accountDimIdSie6 != 0)
            {
                AccountInternalDTO accountInternalSie6DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie6 && a.AccountNr.Equals(sieDim6, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie6DTO.AccountId = account.AccountId;
                    accountInternalSie6DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie6)?.AccountDimNr ?? 0;
                    accountInternalSie6DTO.AccountDimId = accountDimIdSie6;
                    accountInternalSie6DTO.AccountNr = account.AccountNr;
                    accountInternalSie6DTO.Name = account.Name;
                    accountInternalSie6DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie6)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie6DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim7) && accountDimIdSie7 != 0)
            {
                AccountInternalDTO accountInternalSie7DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie7 && a.AccountNr.Equals(sieDim7, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie7DTO.AccountId = account.AccountId;
                    accountInternalSie7DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie7)?.AccountDimNr ?? 0;
                    accountInternalSie7DTO.AccountDimId = accountDimIdSie7;
                    accountInternalSie7DTO.AccountNr = account.AccountNr;
                    accountInternalSie7DTO.Name = account.Name;
                    accountInternalSie7DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie7)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie7DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim8) && accountDimIdSie8 != 0)
            {
                AccountInternalDTO accountInternalSie8DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie8 && a.AccountNr.Equals(sieDim8, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie8DTO.AccountId = account.AccountId;
                    accountInternalSie8DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie8)?.AccountDimNr ?? 0;
                    accountInternalSie8DTO.AccountDimId = accountDimIdSie8;
                    accountInternalSie8DTO.AccountNr = account.AccountNr;
                    accountInternalSie8DTO.Name = account.Name;
                    accountInternalSie8DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie8)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie8DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim9) && accountDimIdSie9 != 0)
            {
                AccountInternalDTO accountInternalSie9DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie9 && a.AccountNr.Equals(sieDim9, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie9DTO.AccountId = account.AccountId;
                    accountInternalSie9DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie9)?.AccountDimNr ?? 0;
                    accountInternalSie9DTO.AccountDimId = accountDimIdSie9;
                    accountInternalSie9DTO.AccountNr = account.AccountNr;
                    accountInternalSie9DTO.Name = account.Name;
                    accountInternalSie9DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie9)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie9DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim10) && accountDimIdSie10 != 0)
            {
                AccountInternalDTO accountInternalSie10DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie10 && a.AccountNr.Equals(sieDim10, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie10DTO.AccountId = account.AccountId;
                    accountInternalSie10DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie10)?.AccountDimNr ?? 0;
                    accountInternalSie10DTO.AccountDimId = accountDimIdSie10;
                    accountInternalSie10DTO.AccountNr = account.AccountNr;
                    accountInternalSie10DTO.Name = account.Name;
                    accountInternalSie10DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie10)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie10DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim30) && accountDimIdSie30 != 0)
            {
                AccountInternalDTO accountInternalSie30DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie30 && a.AccountNr.Equals(sieDim30, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie30DTO.AccountId = account.AccountId;
                    accountInternalSie30DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie30)?.AccountDimNr ?? 0;
                    accountInternalSie30DTO.AccountDimId = accountDimIdSie30;
                    accountInternalSie30DTO.AccountNr = account.AccountNr;
                    accountInternalSie30DTO.Name = account.Name;
                    accountInternalSie30DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie30)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie30DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim40) && accountDimIdSie40 != 0)
            {
                AccountInternalDTO accountInternalSie40DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie40 && a.AccountNr.Equals(sieDim40, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie40DTO.AccountId = account.AccountId;
                    accountInternalSie40DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie40)?.AccountDimNr ?? 0;
                    accountInternalSie40DTO.AccountDimId = accountDimIdSie40;
                    accountInternalSie40DTO.AccountNr = account.AccountNr;
                    accountInternalSie40DTO.Name = account.Name;
                    accountInternalSie40DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie40)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie40DTO);
                }
            }

            if (!string.IsNullOrEmpty(sieDim50) && accountDimIdSie50 != 0)
            {
                AccountInternalDTO accountInternalSie50DTO = new AccountInternalDTO();
                AccountInternalDTO account = allCompanyAccountInternalDTOs.FirstOrDefault(a => a.AccountDimId == accountDimIdSie50 && a.AccountNr.Equals(sieDim50, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    accountInternalSie50DTO.AccountId = account.AccountId;
                    accountInternalSie50DTO.AccountDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie50)?.AccountDimNr ?? 0;
                    accountInternalSie50DTO.AccountDimId = accountDimIdSie50;
                    accountInternalSie50DTO.AccountNr = account.AccountNr;
                    accountInternalSie50DTO.Name = account.Name;
                    accountInternalSie50DTO.SysSieDimNr = accountDims.FirstOrDefault(d => d.AccountDimId == accountDimIdSie50)?.SysSieDimNr;
                    accountInternalDTOs.Add(accountInternalSie50DTO);
                }
            }

            return accountInternalDTOs;
        }

        public List<AccountInternal> GetAccountInternals(int actorCompanyId, bool? active, bool loadDims = false)
        {
            //Active true -> only active. Active false -> only inactive. Active null -> both inactive and active
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountInternal.NoTracking();
            return GetAccountInternals(entities, actorCompanyId, active, loadDims);
        }

        public List<AccountInternal> GetAccountInternals(CompEntities entities, int actorCompanyId, bool? active, bool loadDims = false)
        {
            IQueryable<AccountInternal> query = entities.AccountInternal;
            if (loadDims)
                query = query.Include("Account.AccountDim");
            else
                query = query.Include("Account");

            var result = (from a in query
                          where a.Account.ActorCompanyId == actorCompanyId &&
                          a.Account.State != (int)SoeEntityState.Deleted
                          select a).ToList();

            List<AccountInternal> accountInternals = null;
            if (active == true)
                accountInternals = result.Where(a => a.Account.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                accountInternals = result.Where(a => a.Account.State == (int)SoeEntityState.Inactive).ToList();
            else
                accountInternals = result.ToList();

            return accountInternals;
        }

        public List<AccountInternal> GetAccountInternals(IEnumerable<int> accountIds, int actorCompanyId, bool loadAccount = false, bool loadAccountDim = false, bool discardState = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountInternal.NoTracking();
            return GetAccountInternals(entities, accountIds, actorCompanyId, loadAccount, loadAccountDim, discardState);
        }

        public List<AccountInternal> GetAccountInternals(CompEntities entities, IEnumerable<int> accountIds, int actorCompanyId, bool loadAccount = false, bool loadAccountDim = false, bool discardState = false)
        {
            if (accountIds.IsNullOrEmpty())
                return new List<AccountInternal>();

            var query = (from a in entities.AccountInternal
                         where a.Account.ActorCompanyId == actorCompanyId &&
                         accountIds.Contains(a.AccountId)
                         select a);

            if (loadAccount && loadAccountDim)
                query = query.Include("Account.AccountDim");
            else if (loadAccount)
                query = query.Include("Account");

            if (!discardState)
                query = query.Where(a => a.Account.State == (int)SoeEntityState.Active);

            return query.ToList();
        }

        public List<AccountInternal> GetAccountInternalsByDim(int accountDimId, int actorCompanyId, bool? active = true, bool loadDims = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountInternal.NoTracking();
            return GetAccountInternalsByDim(entities, accountDimId, actorCompanyId, active, loadDims);
        }

        public List<AccountInternal> GetAccountInternalsByDim(CompEntities entities, int accountDimId, int actorCompanyId, bool? active = true, bool loadDims = false)
        {
            var query = (from a in entities.AccountInternal
                         .Include("Account")
                         where a.Account.AccountDimId == accountDimId &&
                         a.Account.ActorCompanyId == actorCompanyId
                         select a);

            List<AccountInternal> accountInternal = null;
            if (active == true)
                accountInternal = query.Where(a => a.Account.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                accountInternal = query.Where(a => a.Account.State == (int)SoeEntityState.Inactive).ToList();
            else
                accountInternal = query.ToList();

            return accountInternal;
        }

        public List<AccountDTO> GetAccountInternalsFromTemplateBlocks(List<TimeScheduleTemplateBlockDTO> templateBlocks, int actorCompanyId, List<ShiftType> shiftTypes = null, List<AccountDTO> allAccountInternals = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountInternalsFromTemplateBlocks(entities, templateBlocks, actorCompanyId, shiftTypes, allAccountInternals);
        }

        public List<AccountDTO> GetAccountInternalsFromTemplateBlocks(CompEntities entities, List<TimeScheduleTemplateBlockDTO> templateBlocks, int actorCompanyId, List<ShiftType> shiftTypes = null, List<AccountDTO> allAccountInternals = null)
        {
            List<AccountDTO> accounts = new List<AccountDTO>();
            int defaultEmployeeAccountDimId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);

            foreach (TimeScheduleTemplateBlockDTO templateBlock in templateBlocks.Where(i => i.AccountId.HasValue))
            {
                List<AccountDTO> accountsForTemplateBlock = new List<AccountDTO>();

                //Add Account from TemplateBlock
                AccountDTO templateBlockAccount = GetAccountDTO(entities, actorCompanyId, templateBlock.AccountId.Value, true, allAccountInternals);
                if (accountsForTemplateBlock.TryAdd<AccountDTO>(templateBlockAccount) && templateBlock.ShiftTypeId.HasValue)
                {
                    //Add Account from ShiftType
                    ShiftType shiftType = TimeScheduleManager.GetShiftType(entities, templateBlock.ShiftTypeId.Value, shiftTypes);
                    if (shiftType != null && shiftType.AccountId.HasValue)
                    {
                        AccountDTO shiftTypeAccount = GetAccountDTO(entities, actorCompanyId, shiftType.AccountId.Value, true, allAccountInternals);
                        if (accountsForTemplateBlock.TryAdd<AccountDTO>(shiftTypeAccount) && shiftTypeAccount.AccountDimId != defaultEmployeeAccountDimId && shiftTypeAccount.ParentAccountId.HasValue)
                        {
                            //Add parent Account if it isnt the account on TemplateBlock
                            AccountDTO shiftTypeParentAccount = GetAccountDTO(actorCompanyId, shiftTypeAccount.ParentAccountId.Value, true, allAccountInternals);
                            if (accountsForTemplateBlock.TryAdd<AccountDTO>(shiftTypeParentAccount) && shiftTypeParentAccount != null && shiftTypeParentAccount.AccountDimId != defaultEmployeeAccountDimId && shiftTypeParentAccount.ParentAccountId.HasValue)
                            {
                                //Add grand parent Account if it isnt the account on TemplateBlock
                                AccountDTO shiftTypeGrandParentAccount = GetAccountDTO(actorCompanyId, shiftTypeParentAccount.ParentAccountId.Value, true, allAccountInternals);
                                if (shiftTypeGrandParentAccount != null && shiftTypeGrandParentAccount.AccountDimId != defaultEmployeeAccountDimId)
                                    accountsForTemplateBlock.TryAdd<AccountDTO>(shiftTypeGrandParentAccount);
                            }
                        }
                    }
                }

                foreach (AccountDTO account in accountsForTemplateBlock)
                {
                    if (accounts.Any(i => i.AccountId == account.AccountId))
                        continue;

                    accounts.Add(account);
                }
            }

            return accounts;
        }

        public List<AccountDTO> GetAccountInternalBySearch(int actorCompanyId, string search, int take)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountInternal.NoTracking();
            return (from a in entities.AccountInternal
                    where a.Account.ActorCompanyId == actorCompanyId &&
                    a.Account.Name.ToLower().Contains(search.ToLower()) &&
                    a.Account.State == (int)SoeEntityState.Active
                    orderby a.Account.Name, a.Account.AccountNr ascending
                    select new AccountDTO()
                    {
                        AccountDimId = a.Account.AccountDimId,
                        AccountDimNr = a.Account.AccountDim.AccountDimNr,
                        AccountId = a.AccountId,
                        AccountNr = a.Account.AccountNr,
                        Name = a.Account.Name,
                    }).Take(take).ToList();
        }

        public List<AccountInternalDTO> GetAccountInternalAndParents(CompEntities entities, int accountId, int actorCompanyId)
        {
            string key = $"GetAccountInternalAndParents(accountId{accountId}actorCompanyId{actorCompanyId})";

            List<AccountInternalDTO> dtos = BusinessMemoryCache<List<AccountInternalDTO>>.Get(key);
            if (dtos != null)
                return dtos;

            List<AccountInternalDTO> accounts = new List<AccountInternalDTO>();
            AccountInternal currentAccount = GetAccountInternal(entities, accountId, actorCompanyId, loadAccount: true);
            if (currentAccount == null)
                return accounts;

            accounts.Add(currentAccount.ToDTO());
            while (currentAccount != null && currentAccount.Account != null && currentAccount.Account.ParentAccountId.HasValue)
            {
                AccountInternal parentAccount = GetAccountInternal(entities, currentAccount.Account.ParentAccountId.Value, actorCompanyId, loadAccount: true);
                if (parentAccount != null)
                    accounts.Add(parentAccount.ToDTO());
                currentAccount = parentAccount;
            }

            BusinessMemoryCache<List<AccountInternalDTO>>.Set(key, accounts);

            return accounts;
        }

        public AccountInternal GetAccountInternal(int accountId, int actorCompanyId, bool loadAccount = false, bool loadAccountDim = false, bool discardState = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountInternal.NoTracking();
            return GetAccountInternal(entities, accountId, actorCompanyId, discardState, loadAccount, loadAccountDim);
        }

        public AccountInternal GetAccountInternal(CompEntities entities, int accountId, int actorCompanyId, bool discardState = false, bool loadAccount = false, bool loadAccountDim = false)
        {
            if (accountId == 0)
                return null;

            var query = (from a in entities.AccountInternal
                         where a.Account.ActorCompanyId == actorCompanyId &&
                         a.AccountId == accountId
                         select a);

            if (loadAccount && loadAccountDim)
                query = query.Include("Account.AccountDim");
            else if (loadAccount)
                query = query.Include("Account");

            if (!discardState)
                query = query.Where(a => a.Account.State == (int)SoeEntityState.Active);

            return query.FirstOrDefault();
        }

        public Dictionary<int, string> GetAccountInternalsDict(int accountDimId, int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<AccountInternal> accountInternals = GetAccountInternalsByDim(accountDimId, actorCompanyId);
            foreach (AccountInternal accountInternal in accountInternals)
            {
                dict.Add(accountInternal.AccountId, accountInternal.Account.Name);
            }

            return dict;
        }

        public bool IsAccountInternalUsed(CompEntities entities, int accountId, int actorCompanyId)
        {
            //Ecnonomy/Billing
            if (IsAccountInternalUsedInVoucherRow(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInProject(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInInventory(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInAccountDistributionEntryRow(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInBudgetHead(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInBudgetRow(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInCategory(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInInvoiceTransaction(entities, accountId, actorCompanyId))
                return true;

            //Time/Payroll
            if (IsAccountInternalUsedInEmploymentAccountStd(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInEmployeeAccount(entities, accountId, actorCompanyId, out _))
                return true;
            if (IsAccountInternalUsedInEmployeeGroup(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInShiftType(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInPayrollProduct(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInTimeScheduleTemplateBlock(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInPayrollScheduleTransaction(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInPayrollTransaction(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInMassRegistrationTemplateHead(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInMassRegistrationTemplateRow(entities, accountId, actorCompanyId))
                return true;
            if (IsAccountInternalUsedInStaffingNeedsFrequancy(entities, accountId, actorCompanyId))
                return true;

            return false;
        }

        public bool IsAccountInternalUsedInVoucherRow(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.VoucherRow
                    where e.VoucherHead.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInProject(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.ProjectAccountStd
                    where e.Project.ActorCompanyId == actorCompanyId &&
                    e.Project.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInInventory(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.InventoryAccountStd
                    where e.Inventory.ActorCompanyId == actorCompanyId &&
                    e.Inventory.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInAccountDistributionEntryRow(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.AccountDistributionEntryRow
                    where e.AccountDistributionEntry.ActorCompanyId == actorCompanyId &&
                    e.AccountDistributionEntry.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInBudgetHead(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.BudgetHead
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInBudgetRow(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.BudgetRow
                    where e.BudgetHead.ActorCompanyId == actorCompanyId &&
                    e.BudgetHead.State == (int)SoeEntityState.Active &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInCategory(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.CategoryAccount
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountId == accountId
                    select e).Any();
        }

        public bool IsAccountInternalUsedInEmployeeAccount(CompEntities entities, int accountId, int actorCompanyId, out List<EmployeeAccount> employeeAccounts)
        {
            employeeAccounts = (from e in entities.EmployeeAccount
                                where e.ActorCompanyId == actorCompanyId &&
                                e.AccountId == accountId &&
                                e.Employee.State == (int)SoeEntityState.Active &&
                                e.State == (int)SoeEntityState.Active
                                select e).ToList();

            return employeeAccounts.Any();
        }

        public bool IsAccountInternalUsedInEmploymentAccountStd(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.EmploymentAccountStd
                    where e.Employment.ActorCompanyId == actorCompanyId &&
                    e.Employment.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInEmployeeGroup(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.EmployeeGroupAccountStd
                    where e.EmployeeGroup.ActorCompanyId == actorCompanyId &&
                    e.EmployeeGroup.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInShiftType(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.ShiftType
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInPayrollProduct(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.ProductAccountStd
                    where e.Product.Company.Any(c => c.ActorCompanyId == actorCompanyId) &&
                    e.Product.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInTimeScheduleTemplateBlock(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.TimeScheduleTemplateBlock
                    where e.TimeCode.ActorCompanyId == actorCompanyId &&
                    e.TimeCode.State == (int)SoeEntityState.Active &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInPayrollScheduleTransaction(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.TimePayrollScheduleTransaction
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInPayrollTransaction(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.TimePayrollTransaction
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInInvoiceTransaction(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.TimeInvoiceTransaction
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInMassRegistrationTemplateHead(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.MassRegistrationTemplateHead
                    where e.ActorCompanyId == actorCompanyId &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInMassRegistrationTemplateRow(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.MassRegistrationTemplateRow
                    where e.MassRegistrationTemplateHead.ActorCompanyId == actorCompanyId &&
                    e.MassRegistrationTemplateHead.State == (int)SoeEntityState.Active &&
                    e.State == (int)SoeEntityState.Active &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalUsedInStaffingNeedsFrequancy(CompEntities entities, int accountId, int actorCompanyId)
        {
            return (from e in entities.StaffingNeedsFrequency
                    where e.ActorCompanyId == actorCompanyId &&
                    e.AccountInternal.Any(i => i.AccountId == accountId)
                    select e).Any();
        }

        public bool IsAccountInternalsCollectionIdentical(List<AccountInternal> accountInternalsOuter, List<AccountInternal> accountInternalsInner)
        {
            //Check null values
            if (accountInternalsOuter == null && accountInternalsInner == null)
                return true;
            else if (accountInternalsOuter == null || accountInternalsInner == null)
                return false;

            //Check counter
            if (accountInternalsOuter.Count != accountInternalsInner.Count)
                return false;
            if (!accountInternalsOuter.Any() && !accountInternalsInner.Any())
                return true;

            //Check and compare content
            bool identical = true;
            foreach (AccountInternal accountInternalOuter in accountInternalsOuter)
            {
                if (!accountInternalsInner.Any(a => a.AccountId == accountInternalOuter.AccountId))
                {
                    identical = false;
                    break;
                }
            }

            return identical;
        }

        public bool IsAccountInternalInIntervalRange(List<AccountDim> accountDimInternals, List<AccountInternal> accountInternals1, List<AccountInternal> accountInternals2)
        {
            //No account to compare
            if (accountInternals1 == null || accountInternals1.Count == 0)
                return true;

            //No account to compare with
            if (accountDimInternals == null || !accountInternals2.IsNullOrEmpty())
                return false;

            //Validate per dim
            foreach (var accountDimInternal in accountDimInternals)
            {
                var accountInternals1ForDim = accountInternals1.Where(a => a.Account.AccountDimId == accountDimInternal.AccountDimId).ToList();
                if (!accountInternals1ForDim.Any())
                    continue; //Continue to next dim

                var accountInternals2ForDim = accountInternals2.Where(a => a.Account.AccountDimId == accountDimInternal.AccountDimId).ToList();
                if (!accountInternals2ForDim.Any())
                    return false; //Has no accountIdState in dim

                //Each accountIdState in accountInternals2ForDim must be in accountInternals1ForDim
                foreach (var accountInternal2 in accountInternals2ForDim)
                {
                    if (!accountInternals1ForDim.Any(ai => ai.AccountId == accountInternal2.AccountId))
                        return false;
                }
            }

            return true;
        }

        public bool IsAccountInternalDTOInIntervalRange(List<AccountDimDTO> accountDimInternals, List<AccountInternalDTO> accountInternals1, List<AccountInternalDTO> accountInternals2)
        {
            //No account to compare
            if (accountInternals1 == null || accountInternals1.Count == 0)
                return true;

            //No account to compare with
            if (accountDimInternals == null || accountInternals2.IsNullOrEmpty())
                return false;

            //Validate per dim
            foreach (var accountDimInternal in accountDimInternals)
            {
                var accountInternals1ForDim = accountInternals1.Where(a => a.AccountDimId == accountDimInternal.AccountDimId).ToList();
                if (!accountInternals1ForDim.Any())
                    continue; //Continue to next dim

                var accountInternals2ForDim = accountInternals2.Where(a => a.AccountDimId == accountDimInternal.AccountDimId).ToList();
                if (!accountInternals2ForDim.Any())
                    return false; //Has no accountIdState in dim

                //Each accountIdState in accountInternals2ForDim must be in accountInternals1ForDim
                foreach (var accountInternal2 in accountInternals2ForDim)
                {
                    if (!accountInternals1ForDim.Any(ai => ai.AccountId == accountInternal2.AccountId))
                        return false;
                }
            }

            return true;
        }

        #endregion

        #region Account settings

        public int GetProductAccountInternalPurchaseSettingId(int value)
        {
            int accountInternalPurchaseSettingId = 0;
            switch (value)
            {
                case 1: accountInternalPurchaseSettingId = (int)CompanySettingType.AccountInvoiceProductPurchaseInternal1; break;
                case 2: accountInternalPurchaseSettingId = (int)CompanySettingType.AccountInvoiceProductPurchaseInternal2; break;
                case 3: accountInternalPurchaseSettingId = (int)CompanySettingType.AccountInvoiceProductPurchaseInternal3; break;
                case 4: accountInternalPurchaseSettingId = (int)CompanySettingType.AccountInvoiceProductPurchaseInternal4; break;
                case 5: accountInternalPurchaseSettingId = (int)CompanySettingType.AccountInvoiceProductPurchaseInternal5; break;
            }
            return accountInternalPurchaseSettingId;
        }

        public int GetProductAccountInternalSalesSettingId(int value)
        {
            int accountInternalSalesSettingId = 0;
            switch (value)
            {
                case 1: accountInternalSalesSettingId = (int)CompanySettingType.AccountInvoiceProductSalesInternal1; break;
                case 2: accountInternalSalesSettingId = (int)CompanySettingType.AccountInvoiceProductSalesInternal2; break;
                case 3: accountInternalSalesSettingId = (int)CompanySettingType.AccountInvoiceProductSalesInternal3; break;
                case 4: accountInternalSalesSettingId = (int)CompanySettingType.AccountInvoiceProductSalesInternal4; break;
                case 5: accountInternalSalesSettingId = (int)CompanySettingType.AccountInvoiceProductSalesInternal5; break;
            }
            return accountInternalSalesSettingId;
        }

        public int GetProductAccountInternalSalesVatFreeSettingId(int value)
        {
            int accountInternalSalesVatFreeSettingId = 0;
            switch (value)
            {
                case 1: accountInternalSalesVatFreeSettingId = (int)CompanySettingType.AccountInvoiceProductSalesVatFreeInternal1; break;
                case 2: accountInternalSalesVatFreeSettingId = (int)CompanySettingType.AccountInvoiceProductSalesVatFreeInternal2; break;
                case 3: accountInternalSalesVatFreeSettingId = (int)CompanySettingType.AccountInvoiceProductSalesVatFreeInternal3; break;
                case 4: accountInternalSalesVatFreeSettingId = (int)CompanySettingType.AccountInvoiceProductSalesVatFreeInternal4; break;
                case 5: accountInternalSalesVatFreeSettingId = (int)CompanySettingType.AccountInvoiceProductSalesVatFreeInternal5; break;
            }
            return accountInternalSalesVatFreeSettingId;
        }

        public AccountingSettingsRowDTO GetAccountingFromString(string accountingString, int actorCompanyId)
        {
            AccountingSettingsRowDTO dto = new AccountingSettingsRowDTO();

            string[] accountNumbers = accountingString.Split(';');

            List<AccountDim> dims;

            if (accountNumbers.Length > 0)
            {
                dims = GetAccountDimsByCompany(actorCompanyId, loadAccounts: true);
                int dimCounter = 0;
                foreach (string accountNumber in accountNumbers)
                {
                    if (!accountNumber.IsNullOrEmpty() && dims.Count > dimCounter)
                    {
                        Account account = null;
                        if (accountNumber == "-")
                        {
                            account = new Account()
                            {
                                AccountId = -1,
                                AccountNr = "-",
                                Name = GetText(3608, 1002, "Ingen kontering"),
                            };
                        }
                        else
                            account = dims[dimCounter].Account.FirstOrDefault(a => a.AccountNr == accountNumbers[dimCounter]);

                        if (account != null)
                        {
                            if (dimCounter == 0)
                            {
                                dto.AccountDim1Nr = Constants.ACCOUNTDIM_STANDARD;
                                dto.Account1Id = account.AccountId;
                                dto.Account1Nr = account.AccountNr;
                                dto.Account1Name = account.Name;
                            }
                            else if (dimCounter == 1)
                            {
                                dto.AccountDim2Nr = dims[dimCounter].AccountDimNr;
                                dto.Account2Id = account.AccountId;
                                dto.Account2Nr = account.AccountNr;
                                dto.Account2Name = account.Name;
                            }
                            else if (dimCounter == 2)
                            {
                                dto.AccountDim3Nr = dims[dimCounter].AccountDimNr;
                                dto.Account3Id = account.AccountId;
                                dto.Account3Nr = account.AccountNr;
                                dto.Account3Name = account.Name;
                            }
                            else if (dimCounter == 3)
                            {
                                dto.AccountDim4Nr = dims[dimCounter].AccountDimNr;
                                dto.Account4Id = account.AccountId;
                                dto.Account4Nr = account.AccountNr;
                                dto.Account4Name = account.Name;
                            }
                            else if (dimCounter == 4)
                            {
                                dto.AccountDim5Nr = dims[dimCounter].AccountDimNr;
                                dto.Account5Id = account.AccountId;
                                dto.Account5Nr = account.AccountNr;
                                dto.Account5Name = account.Name;
                            }
                            else if (dimCounter == 5)
                            {
                                dto.AccountDim6Nr = dims[dimCounter].AccountDimNr;
                                dto.Account6Id = account.AccountId;
                                dto.Account6Nr = account.AccountNr;
                                dto.Account6Name = account.Name;
                            }
                        }
                    }
                    dimCounter++;
                }
            }

            return dto;
        }

        #endregion

        #region Accounting priority

        #region InvoiceProduct

        /// <summary>
        /// Get invoice product account based on accounting priority settings and accountIdState type
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="productId">Product ID</param>
        /// <param name="projectId">Project ID</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="type">Product accountIdState type</param>
        /// <param name="vatType">Invoice VAT type</param>
        /// <param name="getInternalAccounts">If true, internal account are also included</param>
        /// <returns>AccountingPrioDTO</returns>
        public AccountingPrioDTO GetInvoiceProductAccount(int actorCompanyId, int productId, int projectId, int customerId, int employeeId, ProductAccountType type, TermGroup_InvoiceVatType vatType, bool getInternalAccounts, bool isTimeProjectRow = false, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetInvoiceProductAccount(entities, actorCompanyId, productId, projectId, customerId, employeeId, type, vatType, getInternalAccounts, isTimeProjectRow);
        }

        /// <summary>
        /// Get invoice product account based on accounting priority settings and accountIdState type
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="productId">Product ID</param>
        /// <param name="projectId">Project ID</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="type">Product accountIdState type</param>
        /// <param name="vatType">Invoice VAT type</param>
        /// <param name="getInternalAccounts">If true, internal account are also included</param>
        /// <returns>AccountingPrioDTO</returns>
        public AccountingPrioDTO GetInvoiceProductAccount(CompEntities entities, int actorCompanyId, int productId, int projectId, int customerId, int employeeId, ProductAccountType type, TermGroup_InvoiceVatType vatType, bool getInternalAccounts, bool isTimeProjectRow = false, DateTime? date = null, InvoiceProduct product = null)
        {
            AccountingPrioDTO dto = null;

            // Special treatment for cent rounding, freight amount and invoice fee
            int centProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductCentRounding, 0, actorCompanyId, 0);
            int freightProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductFreight, 0, actorCompanyId, 0);
            int invoiceFeeProductId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductInvoiceFee, 0, actorCompanyId, 0);

            if (type == ProductAccountType.Sales || type == ProductAccountType.SalesContractor || type == ProductAccountType.SalesNoVat)
            {
                // Cent rounding
                if (productId == centProductId)
                    dto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, CompanySettingType.AccountCommonCentRounding);
            }
            else if (type == ProductAccountType.VAT && (productId == freightProductId || productId == invoiceFeeProductId))
            {
                dto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, CompanySettingType.AccountCommonVatPayable1);
            }

            if (dto != null)
                return dto;

            // Get product
            if (product == null)
            {
                product = (from p in entities.Product.OfType<InvoiceProduct>()
                                              .Include("ProductAccountStd")
                           where p.ProductId == productId &&
                           p.State == (int)SoeEntityState.Active
                           select p).FirstOrDefault();
            }
            //accountIdState on product?
            else if (!product.ProductAccountStd.IsLoaded)
            {
                product.ProductAccountStd.Load();
            }

            TermGroup_InvoiceProductAccountingPrio accountingPrio = TermGroup_InvoiceProductAccountingPrio.NotUsed;
            if (product != null)
            {
                #region Load references

                if (product.ProductAccountStd != null)
                {
                    foreach (ProductAccountStd productAccountStd in product.ProductAccountStd)
                    {
                        //AccountStd
                        if (!productAccountStd.AccountStdReference.IsLoaded)
                            productAccountStd.AccountStdReference.Load();

                        if (productAccountStd.AccountStd != null && !productAccountStd.AccountStd.AccountReference.IsLoaded)
                            productAccountStd.AccountStd.AccountReference.Load();

                        //AccountInternal
                        if (!productAccountStd.AccountInternal.IsLoaded)
                            productAccountStd.AccountInternal.Load();

                        foreach (AccountInternal accountInternal in productAccountStd.AccountInternal)
                        {
                            if (!accountInternal.AccountReference.IsLoaded)
                                accountInternal.AccountReference.Load();
                            if (!accountInternal.Account.AccountDimReference.IsLoaded)
                                accountInternal.Account.AccountDimReference.Load();
                        }
                    }
                }

                #endregion

                #region Product priorities

                if (type == ProductAccountType.VAT && SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultVatCode, 0, actorCompanyId, 0) != 0)
                {
                    // If a default VAT code is set as company setting, only check if a VAT accountIdState is set directly on product,
                    // otherwise the default VAT code should be used. This is done on the client side, if null is returned.
                    return GetProductAccountStds(product, ProductAccountType.VAT);
                }

                // Get accounting priority for standard accountIdState
                accountingPrio = (TermGroup_InvoiceProductAccountingPrio)GetProductAccountingPrio(product, Constants.ACCOUNTDIM_STANDARD);

                #endregion
            }

            switch (accountingPrio)
            {
                case TermGroup_InvoiceProductAccountingPrio.NotUsed:
                    #region NotUsed

                    // No priority set on current product, search from project level and up
                    List<TermGroup_TimeProjectInvoiceProductAccountingPrio> projectPrios = GetTimeProjectInvoiceProductAccountingPrios(entities, projectId);
                    if (projectPrios.Count > 0)
                    {
                        #region Project priorities

                        // Project priorities found, loop through them and return first accounting found
                        foreach (TermGroup_TimeProjectInvoiceProductAccountingPrio prio in projectPrios)
                        {
                            AccountingPrioDTO localDto = null;

                            switch (prio)
                            {
                                case TermGroup_TimeProjectInvoiceProductAccountingPrio.Project:
                                    // Get account for project
                                    localDto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), getInternalAccounts, isTimeProjectRow);
                                    break;
                                case TermGroup_TimeProjectInvoiceProductAccountingPrio.Customer:
                                    // Get account for customer
                                    localDto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), getInternalAccounts);
                                    break;
                                case TermGroup_TimeProjectInvoiceProductAccountingPrio.EmploymentAccount:
                                    // Get accountIdState for employment
                                    localDto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, date);
                                    break;
                                case TermGroup_TimeProjectInvoiceProductAccountingPrio.EmployeeAccount:
                                    // Get accountIdState for employee
                                    localDto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, null, date);
                                    break;
                                case TermGroup_TimeProjectInvoiceProductAccountingPrio.InvoiceProduct:
                                    // Get account for product
                                    localDto = GetProductAccountStds(product, type);
                                    break;
                            }

                            if (localDto != null && dto == null)
                            {
                                dto = localDto;
                            }

                            if (dto != null && !dto.AccountId.HasValue && localDto != null && localDto.AccountId.HasValue)
                            {
                                dto.AccountId = localDto.AccountId;
                            }

                            if (dto != null && dto.AccountId.HasValue)
                                return dto;
                        }

                        #endregion
                    }
                    else
                    {
                        // No project priorities found, search on employee group
                        List<TermGroup_EmployeeGroupInvoiceProductAccountingPrio> employeeGroupPrios = GetEmployeeGroupInvoiceProductAccountingPrios(entities, employeeId, actorCompanyId);
                        if (employeeGroupPrios.Count > 0)
                        {
                            #region EmployeeGroup priorities

                            // Employee group priorities found, loop through them and return first accounting found
                            foreach (TermGroup_EmployeeGroupInvoiceProductAccountingPrio prio in employeeGroupPrios)
                            {
                                switch (prio)
                                {
                                    case TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Project:
                                        #region Project

                                        dto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), getInternalAccounts, isTimeProjectRow);
                                        if (dto != null)
                                            return dto;

                                        #endregion
                                        break;
                                    case TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Customer:
                                        #region Customer

                                        dto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), getInternalAccounts);
                                        if (dto != null)
                                            return dto;

                                        #endregion
                                        break;
                                    case TermGroup_EmployeeGroupInvoiceProductAccountingPrio.EmploymentAccount:
                                        #region Employee

                                        dto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, date);
                                        if (dto != null)
                                            return dto;

                                        #endregion
                                        break;
                                    case TermGroup_EmployeeGroupInvoiceProductAccountingPrio.EmployeeAccount:
                                        #region Employee

                                        // Get accountIdState for employee
                                        dto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, null, date);
                                        if (dto != null)
                                            return dto;

                                        #endregion
                                        break;
                                    case TermGroup_EmployeeGroupInvoiceProductAccountingPrio.InvoiceProduct:
                                        #region InvoiceProduct

                                        dto = GetProductAccountStds(product, type);
                                        if (dto != null)
                                            return dto;

                                        #endregion
                                        break;
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            // No employee group priorities found, search on company settings
                            List<TermGroup_CompanyInvoiceProductAccountingPrio> companyPrios = GetCompanyInvoiceProductAccountingPrios(entities, actorCompanyId);
                            if (companyPrios.Count > 0)
                            {
                                #region Company priorities

                                // Company priorities found, loop through them and return first accounting found
                                foreach (TermGroup_CompanyInvoiceProductAccountingPrio prio in companyPrios)
                                {
                                    AccountingPrioDTO localDto = null;

                                    switch (prio)
                                    {
                                        case TermGroup_CompanyInvoiceProductAccountingPrio.Project:
                                            localDto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), getInternalAccounts, isTimeProjectRow);
                                            break;
                                        case TermGroup_CompanyInvoiceProductAccountingPrio.Customer:
                                            localDto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), getInternalAccounts);
                                            break;
                                        case TermGroup_CompanyInvoiceProductAccountingPrio.EmploymentAccount:
                                            localDto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, date);
                                            break;
                                        case TermGroup_CompanyInvoiceProductAccountingPrio.EmployeeAccount:
                                            localDto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, null, date);
                                            break;
                                        case TermGroup_CompanyInvoiceProductAccountingPrio.InvoiceProduct:
                                            localDto = GetProductAccountStds(product, type, getInternalAccounts);
                                            break;
                                    }

                                    if (dto != null && localDto != null && !localDto.AccountInternals.IsNullOrEmpty())
                                    {
                                        dto.MergeAccountInternalsByDim(localDto.AccountInternals);
                                    }

                                    if (localDto != null && dto == null)
                                    {
                                        dto = localDto;
                                    }

                                    if (dto != null && !dto.AccountId.HasValue && localDto != null && localDto.AccountId.HasValue)
                                    {
                                        dto.AccountId = localDto.AccountId;
                                    }

                                    if (dto != null && dto.AccountId.HasValue)
                                        return dto;
                                }

                                #endregion
                            }
                        }
                    }

                    #endregion
                    break;
                case TermGroup_InvoiceProductAccountingPrio.NoAccounting:
                    #region NoAccounting

                    // Accounting is not used for current product

                    #endregion
                    return null;
                case TermGroup_InvoiceProductAccountingPrio.Project:
                    #region Project

                    // Accounting is fetched from the project
                    dto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), getInternalAccounts, isTimeProjectRow);

                    #endregion
                    break;
                case TermGroup_InvoiceProductAccountingPrio.Customer:
                    #region Customer

                    // Accounting is fetched from the customer
                    dto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), getInternalAccounts);

                    #endregion
                    break;
                case TermGroup_InvoiceProductAccountingPrio.EmploymentAccount:
                    #region Employee

                    // Accounting is fetched from the employee
                    dto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, date);

                    #endregion
                    break;
                case TermGroup_InvoiceProductAccountingPrio.EmployeeAccount:
                    #region Employee

                    // Get accountIdState for employee
                    dto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, getInternalAccounts, null, date);
                    if (dto != null)
                        return dto;

                    #endregion
                    break;
                case TermGroup_InvoiceProductAccountingPrio.InvoiceProduct:
                    #region InvoiceProduct

                    // Accounting id fetched from the product
                    dto = GetProductAccountStds(product, type);

                    #endregion
                    break;
            }

            // Final try, get default company settings account
            if (dto == null || !dto.AccountId.HasValue)
            {
                #region Company settings
                AccountingPrioDTO companyDto = null;

                // Check if product is a base product with its own accounting settings
                if (type == ProductAccountType.Sales || type == ProductAccountType.SalesNoVat)
                {
                    // Freight
                    if (productId == freightProductId)
                        companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, CompanySettingType.AccountCustomerFreight);
                    // Invoice fee
                    else if (productId == invoiceFeeProductId)
                        companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, CompanySettingType.AccountCustomerOrderFee);
                    // Cent rounding
                    else if (productId == centProductId)
                        companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, CompanySettingType.AccountCommonCentRounding);
                }

                // Household tax deduction
                if (productId == SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductHouseholdTaxDeduction, 0, actorCompanyId, 0) || productId == SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.ProductHousehold50TaxDeduction, 0, actorCompanyId, 0))
                {
                    companyDto = GetProductAccountStds(product, type);
                    if (companyDto != null && companyDto.AccountId.HasValue)
                        companyDto.CompanyType = CompanySettingType.ProductHouseholdTaxDeduction;
                    else
                        companyDto = null;
                }

                // Get default sales accountIdState for vat free
                if ((type == ProductAccountType.Sales || type == ProductAccountType.SalesNoVat || type == ProductAccountType.SalesContractor) && vatType == TermGroup_InvoiceVatType.NoVat)
                    companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, CompanySettingType.AccountCustomerSalesNoVat);

                // Get default sales accountIdState for contractor
                if ((type == ProductAccountType.Sales || type == ProductAccountType.SalesNoVat || type == ProductAccountType.SalesContractor) && vatType == TermGroup_InvoiceVatType.Contractor)
                    companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, CompanySettingType.AccountCommonReverseVatSales);

                if (type == ProductAccountType.ExportWithinEU)
                    companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, product != null && product.VatType == (int)TermGroup_InvoiceProductVatType.Service ? CompanySettingType.AccountCustomerSalesWithinEUService : CompanySettingType.AccountCustomerSalesWithinEU);

                if (type == ProductAccountType.ExportOutsideEU)
                    companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, product != null && product.VatType == (int)TermGroup_InvoiceProductVatType.Service ? CompanySettingType.AccountCustomerSalesOutsideEUService : CompanySettingType.AccountCustomerSalesOutsideEU);

                if (companyDto == null)
                    companyDto = GetCompanyInvoiceProductAccounts(entities, actorCompanyId, GetCompanyInvoiceProductAccountType(type));

                if (dto == null)
                {
                    dto = companyDto;
                }
                else if (!dto.AccountId.HasValue && companyDto != null)
                {
                    dto.AccountId = companyDto.AccountId;
                }

                #endregion
            }

            return dto;
        }

        /// <summary>
        /// Get product account from company settings
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="type">Company setting type specifying the accountIdState type</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetCompanyInvoiceProductAccounts(CompEntities entities, int actorCompanyId, CompanySettingType type)
        {
            AccountingPrioDTO dto = null;

            // Standard accountIdState
            Account account = GetAccount(entities, actorCompanyId, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)type, 0, actorCompanyId, 0));
            if (account != null)
            {
                dto = new AccountingPrioDTO()
                {
                    AccountId = account.AccountId,
                    AccountNr = account.AccountNr,
                    AccountName = account.Name,
                    CompanyType = type,
                    Percent = 100,
                };

                // Internal account
                GetCompanyInvoiceProductInternalAccount(entities, dto, actorCompanyId, CompanySettingType.AccountInvoiceProductSalesInternal1);
                GetCompanyInvoiceProductInternalAccount(entities, dto, actorCompanyId, CompanySettingType.AccountInvoiceProductSalesInternal2);
                GetCompanyInvoiceProductInternalAccount(entities, dto, actorCompanyId, CompanySettingType.AccountInvoiceProductSalesInternal3);
                GetCompanyInvoiceProductInternalAccount(entities, dto, actorCompanyId, CompanySettingType.AccountInvoiceProductSalesInternal4);
                GetCompanyInvoiceProductInternalAccount(entities, dto, actorCompanyId, CompanySettingType.AccountInvoiceProductSalesInternal5);
            }

            return dto;
        }

        public ProductAccountsItem GetInvoiceProductAccounts(int actorCompanyId, int productId, int projectId, int customerId, int employeeId, TermGroup_InvoiceVatType vatType, bool getSalesAccounts, bool getPurchaseAccounts, bool getVatAccounts, bool getInternalAccounts, bool isTimeProjectRow = false, bool tripartiteTrade = false, DateTime? date = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetInvoiceProductAccounts(entities, actorCompanyId, productId, projectId, customerId, employeeId, vatType, getSalesAccounts, getPurchaseAccounts, getVatAccounts, getInternalAccounts, isTimeProjectRow, tripartiteTrade, date);
        }

        public ProductAccountsItem GetInvoiceProductAccounts(CompEntities entities, int actorCompanyId, int productId, int projectId, int customerId, int employeeId, TermGroup_InvoiceVatType vatType, bool getSalesAccounts, bool getPurchaseAccounts, bool getVatAccounts, bool getInternalAccounts, bool isTimeProjectRow = false, bool tripartiteTrade = false, DateTime? date = null)
        {
            ProductAccountsItem item = new ProductAccountsItem();
            AccountingPrioDTO dto;
            AccountInternalDTO accountInternal;

            #region Sales accounts

            ProductAccountType accountType = ProductAccountType.Sales;
            if (vatType == TermGroup_InvoiceVatType.NoVat)
                accountType = ProductAccountType.SalesNoVat;
            else if (vatType == TermGroup_InvoiceVatType.Contractor)
                accountType = ProductAccountType.SalesContractor;
            else if (vatType == TermGroup_InvoiceVatType.ExportWithinEU)
                accountType = tripartiteTrade ? ProductAccountType.TripartiteTrade : ProductAccountType.ExportWithinEU;
            else if (vatType == TermGroup_InvoiceVatType.Contractor)
                accountType = tripartiteTrade ? ProductAccountType.TripartiteTrade : ProductAccountType.ExportOutsideEU;

            dto = getSalesAccounts ? GetInvoiceProductAccount(entities, actorCompanyId, productId, projectId, customerId, employeeId, accountType, vatType, getInternalAccounts, isTimeProjectRow, date) : null;
            item.SalesAccountDim1Id = dto?.AccountId ?? 0;
            item.SalesAccountDim1Nr = dto?.AccountNr ?? string.Empty;
            item.SalesAccountDim1Name = dto?.AccountName ?? string.Empty;

            if (getInternalAccounts && dto != null && !dto.AccountInternals.IsNullOrEmpty())
            {
                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 2);
                item.SalesAccountDim2Id = accountInternal?.AccountId ?? 0;
                item.SalesAccountDim2Nr = accountInternal?.AccountNr ?? string.Empty;
                item.SalesAccountDim2Name = accountInternal?.Name ?? string.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 3);
                item.SalesAccountDim3Id = accountInternal?.AccountId ?? 0;
                item.SalesAccountDim3Nr = accountInternal?.AccountNr ?? string.Empty;
                item.SalesAccountDim3Name = accountInternal?.Name ?? string.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 4);
                item.SalesAccountDim4Id = accountInternal?.AccountId ?? 0;
                item.SalesAccountDim4Nr = accountInternal?.AccountNr ?? string.Empty;
                item.SalesAccountDim4Name = accountInternal?.Name ?? string.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 5);
                item.SalesAccountDim5Id = accountInternal?.AccountId ?? 0;
                item.SalesAccountDim5Nr = accountInternal?.AccountNr ?? string.Empty;
                item.SalesAccountDim5Name = accountInternal?.Name ?? string.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 6);
                item.SalesAccountDim6Id = accountInternal?.AccountId ?? 0;
                item.SalesAccountDim6Nr = accountInternal?.AccountNr ?? string.Empty;
                item.SalesAccountDim6Name = accountInternal?.Name ?? string.Empty;
            }

            #endregion

            #region Purchase accounts

            dto = getPurchaseAccounts ? GetInvoiceProductAccount(entities, actorCompanyId, productId, projectId, customerId, employeeId, ProductAccountType.Purchase, vatType, getInternalAccounts, isTimeProjectRow, date) : null;
            item.PurchaseAccountDim1Id = dto != null && dto.AccountId.HasValue ? dto.AccountId.Value : 0;
            item.PurchaseAccountDim1Nr = dto != null ? dto.AccountNr : string.Empty;
            item.PurchaseAccountDim1Name = dto != null ? dto.AccountName : string.Empty;

            if (getInternalAccounts && dto != null && dto.AccountInternals != null && dto.AccountInternals.Count > 0)
            {
                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 2);
                item.PurchaseAccountDim2Id = accountInternal != null ? accountInternal.AccountId : 0;
                item.PurchaseAccountDim2Nr = accountInternal != null ? accountInternal.AccountNr : String.Empty;
                item.PurchaseAccountDim2Name = accountInternal != null ? accountInternal.Name : String.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 3);
                item.PurchaseAccountDim3Id = accountInternal != null ? accountInternal.AccountId : 0;
                item.PurchaseAccountDim3Nr = accountInternal != null ? accountInternal.AccountNr : String.Empty;
                item.PurchaseAccountDim3Name = accountInternal != null ? accountInternal.Name : String.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 4);
                item.PurchaseAccountDim4Id = accountInternal != null ? accountInternal.AccountId : 0;
                item.PurchaseAccountDim4Nr = accountInternal != null ? accountInternal.AccountNr : String.Empty;
                item.PurchaseAccountDim4Name = accountInternal != null ? accountInternal.Name : String.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 5);
                item.PurchaseAccountDim5Id = accountInternal != null ? accountInternal.AccountId : 0;
                item.PurchaseAccountDim5Nr = accountInternal != null ? accountInternal.AccountNr : String.Empty;
                item.PurchaseAccountDim5Name = accountInternal != null ? accountInternal.Name : String.Empty;

                accountInternal = dto.AccountInternals.FirstOrDefault(a => a.AccountDimNr == 6);
                item.PurchaseAccountDim6Id = accountInternal != null ? accountInternal.AccountId : 0;
                item.PurchaseAccountDim6Nr = accountInternal != null ? accountInternal.AccountNr : String.Empty;
                item.PurchaseAccountDim6Name = accountInternal != null ? accountInternal.Name : String.Empty;
            }

            #endregion

            #region VAT accounts

            dto = getVatAccounts ? GetInvoiceProductAccount(entities, actorCompanyId, productId, projectId, customerId, employeeId, ProductAccountType.VAT, vatType, getInternalAccounts, isTimeProjectRow, date) : null;
            item.VatAccountDim1Id = dto != null && dto.AccountId.HasValue ? dto.AccountId.Value : 0;
            item.VatAccountDim1Nr = dto != null ? dto.AccountNr : string.Empty;
            item.VatAccountDim1Name = dto != null ? dto.AccountName : string.Empty;

            // Get VAT rate
            SysVatRate sysVatRate = GetSysVatRate(item.VatAccountDim1Id);
            item.VatRate = sysVatRate != null ? sysVatRate.VatRate : 0;

            #endregion

            return item;
        }

        /// <summary>
        /// Get accounting priority settings for specified company
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="Company ID"></param>
        /// <returns>List of company accounting priority settings</returns>
        private List<TermGroup_CompanyInvoiceProductAccountingPrio> GetCompanyInvoiceProductAccountingPrios(CompEntities entities, int actorCompanyId)
        {
            List<TermGroup_CompanyInvoiceProductAccountingPrio> prios = new List<TermGroup_CompanyInvoiceProductAccountingPrio>();

            if (actorCompanyId != 0)
            {
                // Get company accounting priority settings
                TermGroup_CompanyInvoiceProductAccountingPrio prio;
                string accountingPrioSetting = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeCompanyInvoiceProductAccountingPrio, 0, actorCompanyId, 0);
                if (!String.IsNullOrEmpty(accountingPrioSetting))
                {
                    string[] accountingPrios = accountingPrioSetting.Split(',');
                    foreach (string accountingPrio in accountingPrios)
                    {
                        prio = (TermGroup_CompanyInvoiceProductAccountingPrio)Int32.Parse(accountingPrio);
                        if (prio != TermGroup_CompanyInvoiceProductAccountingPrio.NotUsed)
                            prios.Add(prio);
                    }
                }
            }

            return prios;
        }

        /// <summary>
        /// Get accounting priority settings for specified employees employee group
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of employee group accounting priority settings</returns>
        private List<TermGroup_EmployeeGroupInvoiceProductAccountingPrio> GetEmployeeGroupInvoiceProductAccountingPrios(CompEntities entities, int employeeId, int actorCompanyId)
        {
            List<TermGroup_EmployeeGroupInvoiceProductAccountingPrio> prios = new List<TermGroup_EmployeeGroupInvoiceProductAccountingPrio>();

            if (employeeId != 0)
            {
                // Get EmployeeGroup
                EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroupForEmployee(entities, employeeId, actorCompanyId, DateTime.Today);
                if (employeeGroup != null)
                {
                    // Get EmployeeGroup accounting priority settings
                    TermGroup_EmployeeGroupInvoiceProductAccountingPrio prio;
                    string[] accountingPrios = employeeGroup.InvoiceProductAccountingPrio.Split(',');
                    foreach (string accountingPrio in accountingPrios)
                    {
                        prio = (TermGroup_EmployeeGroupInvoiceProductAccountingPrio)Int32.Parse(accountingPrio);
                        if (prio != TermGroup_EmployeeGroupInvoiceProductAccountingPrio.NotUsed)
                            prios.Add(prio);
                    }
                }
            }

            return prios;
        }

        /// <summary>
        /// Get accounting priority settings for specified project
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of project accounting priority settings</returns>
        private List<TermGroup_TimeProjectInvoiceProductAccountingPrio> GetTimeProjectInvoiceProductAccountingPrios(CompEntities entities, int projectId)
        {
            List<TermGroup_TimeProjectInvoiceProductAccountingPrio> prios = new List<TermGroup_TimeProjectInvoiceProductAccountingPrio>();

            if (projectId != 0)
            {
                // Get project
                TimeProject project = entities.Project.OfType<TimeProject>().Where(p => p.ProjectId == projectId).FirstOrDefault();
                if (project != null)
                {
                    if (project.ParentProjectId != null && (int)project.ParentProjectId != 0 && !project.UseAccounting)
                    {
                        int topProjectId = ProjectManager.FindTopProject(entities, (int)project.ParentProjectId);
                        project = entities.Project.OfType<TimeProject>().Where(p => p.ProjectId == topProjectId).FirstOrDefault();

                        if (project != null)
                        {
                            // Get project accounting priority settings
                            TermGroup_TimeProjectInvoiceProductAccountingPrio prio;
                            string[] accountingPrios = project.InvoiceProductAccountingPrio.Split(',');
                            foreach (string accountingPrio in accountingPrios)
                            {
                                prio = (TermGroup_TimeProjectInvoiceProductAccountingPrio)Int32.Parse(accountingPrio);
                                if (prio != TermGroup_TimeProjectInvoiceProductAccountingPrio.NotUsed)
                                    prios.Add(prio);
                            }
                        }
                    }
                    else
                    {
                        // Get project accounting priority settings
                        TermGroup_TimeProjectInvoiceProductAccountingPrio prio;
                        string[] accountingPrios = project.InvoiceProductAccountingPrio.Split(',');
                        foreach (string accountingPrio in accountingPrios)
                        {
                            prio = (TermGroup_TimeProjectInvoiceProductAccountingPrio)Int32.Parse(accountingPrio);
                            if (prio != TermGroup_TimeProjectInvoiceProductAccountingPrio.NotUsed)
                                prios.Add(prio);
                        }
                    }
                }
            }

            return prios;
        }

        /// <summary>
        /// Get product internal account from company settings
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="dto">Product standard accountIdState</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="type">Company setting type specifying the accountIdState type</param>
        private void GetCompanyInvoiceProductInternalAccount(CompEntities entities, AccountingPrioDTO dto, int actorCompanyId, CompanySettingType type)
        {
            Account account = GetAccount(entities, actorCompanyId, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)type, 0, actorCompanyId, 0));
            if (account != null)
            {
                dto.AccountInternals = new List<AccountInternalDTO>()
                {
                    new AccountInternalDTO()
                    {
                        AccountId = account.AccountId
                    }
                };
            }
        }

        #endregion

        #region PayrollProduct

        public AccountingPrioDTO GetPayrollProductAccount(ProductAccountType type, int actorCompanyId, int employeeId, int productId, int projectId, int customerId, bool getInternalAccounts, DateTime? date = null, List<EmployeeGroup> employeeGroups = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollProductAccount(entities, type, actorCompanyId, employeeId, productId, projectId, customerId, getInternalAccounts, date, employeeGroups);
        }

        public AccountingPrioDTO GetPayrollProductAccount(CompEntities entities, ProductAccountType type, int actorCompanyId, int employeeId, int productId, int projectId, int customerId, bool getInternalAccounts, DateTime? date = null, List<EmployeeGroup> employeeGroups = null)
        {
            Employee employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true);
            return GetPayrollProductAccount(entities, type, actorCompanyId, employee, productId, projectId, customerId, getInternalAccounts, date, employeeGroups: employeeGroups);
        }

        public AccountingPrioDTO GetPayrollProductAccount(ProductAccountType type, int actorCompanyId, Employee employee, int productId, int projectId, int customerId, bool getInternalAccounts, DateTime? date = null, List<EmployeeGroup> employeeGroups = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetPayrollProductAccount(entities, type, actorCompanyId, employee, productId, projectId, customerId, getInternalAccounts, date, employeeGroups: employeeGroups);
        }

        private static readonly Dictionary<int, List<PayrollProductSetting>> payrollProductSettingCache = new Dictionary<int, List<PayrollProductSetting>>();
        public AccountingPrioDTO GetPayrollProductAccount(CompEntities entities, ProductAccountType type, int actorCompanyId, Employee employee, int productId, int projectId, int customerId, bool getInternalAccounts, DateTime? date = null, PayrollProduct payrollProductWithAllSettings = null, List<EmployeeGroup> employeeGroups = null, List<AccountDim> accountDims = null)
        {
            AccountingPrioDTO prioDto = new AccountingPrioDTO();

            int? payrollGroupId = employee?.GetPayrollGroupId(date);

            // Get payroll product setting
            PayrollProductSetting setting = null;
            if (productId != 0)
            {
                if (payrollProductWithAllSettings != null)
                {
                    var settingsForProduct = (from p in payrollProductWithAllSettings.PayrollProductSetting
                                              where p.ProductId == productId &&
                                              p.State == (int)SoeEntityState.Active
                                              select p).ToList();

                    setting = settingsForProduct.FirstOrDefault(i => i.PayrollGroupId == payrollGroupId);
                    if (setting == null)
                        setting = settingsForProduct.FirstOrDefault(i => !i.PayrollGroupId.HasValue);
                }

                if (setting == null)
                {
                    if (!payrollProductSettingCache.TryGetValue(productId, out var settingsForProduct))
                    {
                        settingsForProduct = (from p in entities.PayrollProductSetting
                                              .Include("PayrollProduct")
                                              .Include("PayrollProductAccountStd.AccountStd.Account")
                                              .Include("PayrollProductAccountStd.AccountInternal.Account.AccountDim")
                                              where p.ProductId == productId &&
                                              p.State == (int)SoeEntityState.Active
                                              select p).ToList();
                        payrollProductSettingCache[productId] = settingsForProduct;
                    }

                    setting = settingsForProduct.FirstOrDefault(i => i.PayrollGroupId == payrollGroupId);
                    if (setting == null)
                        setting = settingsForProduct.FirstOrDefault(i => !i.PayrollGroupId.HasValue);
                }
            }

            if (getInternalAccounts)
            {
                if (accountDims == null)
                    accountDims = GetAccountDimsByCompany(entities, actorCompanyId);

                var accountInternals = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));

                foreach (AccountDim accountDim in accountDims.OrderBy(d => d.AccountDimNr))
                {
                    AccountingPrioDTO dto = GetPayrollProductAccountByPrio(entities, type, setting, actorCompanyId, payrollGroupId, customerId, projectId, employee, accountDim.AccountDimNr, date, employeeGroups);

                    if (accountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD)
                    {
                        #region AccountStd

                        if (dto != null)
                            prioDto = dto;

                        if (prioDto != null)
                            prioDto.AccountInternals = new List<AccountInternalDTO>();

                        #endregion
                    }
                    else
                    {
                        #region AccountInternal
                        if (dto == null)
                            continue;

                        var accountIds = dto.AccountInternals?.Select(s => s.AccountId).ToList() ?? new List<int>();
                        var accountInternalsOnDim = accountInternals.Where(a => !a.HierarchyOnly && a.AccountDimId == accountDim.AccountDimId && accountIds.Contains(a.AccountId)).ToList();
                        AccountInternalDTO accInt = dto.AccountInternals?.FirstOrDefault(a => (accountInternalsOnDim?.FirstOrDefault()?.AccountId ?? 0) == a.AccountId);

                        if (accInt != null)
                        {
                            if (prioDto.AccountInternals == null)
                                prioDto.AccountInternals = new List<AccountInternalDTO>();
                            prioDto.AccountInternals.Add(accInt);
                        }

                        #endregion
                    }
                }
            }
            else
            {
                prioDto = GetPayrollProductAccountByPrio(entities, type, setting, actorCompanyId, payrollGroupId, customerId, projectId, employee, Constants.ACCOUNTDIM_STANDARD, date, employeeGroups: employeeGroups);
            }

            return prioDto;
        }

        /// <summary>
        /// Get payroll product account based on accounting priority settings and accountIdState type
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="type">Project accountIdState type</param>
        /// <param name="setting">Payroll product setting to use prio from</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="productId">Product ID</param>
        /// <param name="payrollGroupId">PayrollGroup ID</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="projectId">Project ID</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dimNr">Account dim number to get accountIdState from</param>
        /// <param name="date">Date</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetPayrollProductAccountByPrio(CompEntities entities, ProductAccountType type, PayrollProductSetting setting, int actorCompanyId, int? payrollGroupId, int customerId, int projectId, Employee employee, int dimNr, DateTime? date = null, List<EmployeeGroup> employeeGroups = null)
        {
            AccountingPrioDTO dto = null;
            int employeeId = employee?.EmployeeId ?? 0;

            TermGroup_PayrollProductAccountingPrio accountingPrio = TermGroup_PayrollProductAccountingPrio.NotUsed;
            if (setting != null)
            {
                #region Product priorities

                // Get accounting priority
                accountingPrio = (TermGroup_PayrollProductAccountingPrio)GetPayrollProductAccountingPrio(setting, dimNr);

                #endregion
            }

            switch (accountingPrio)
            {
                case TermGroup_PayrollProductAccountingPrio.NotUsed:
                    #region NotUsed

                    // No priority set on current product, search hard coded priority
                    // No priority setting on Employee

                    #region Project

                    List<TermGroup_TimeProjectPayrollProductAccountingPrio> projectPrios = GetTimeProjectPayrollProductAccountingPrios(entities, projectId);
                    if (projectPrios.Any())
                    {
                        #region Project priorities

                        // Project priorities found, loop through them and return first accounting found
                        foreach (TermGroup_TimeProjectPayrollProductAccountingPrio prio in projectPrios)
                        {
                            switch (prio)
                            {
                                case TermGroup_TimeProjectPayrollProductAccountingPrio.PayrollProduct:
                                    dto = GetPayrollProductAccounts(setting, type);
                                    if (dto != null && dto.HasAccountOnDim(dimNr))
                                        return dto;
                                    break;
                                case TermGroup_TimeProjectPayrollProductAccountingPrio.EmploymentAccount:
                                    dto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, date);
                                    if (dto != null && dto.HasAccountOnDim(dimNr))
                                        return dto;
                                    break;
                                case TermGroup_TimeProjectPayrollProductAccountingPrio.Project:
                                    dto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD, true);
                                    if (dto != null && dto.HasAccountOnDim(dimNr))
                                        return dto;
                                    break;
                                case TermGroup_TimeProjectPayrollProductAccountingPrio.Customer:
                                    dto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD);
                                    if (dto != null && dto.HasAccountOnDim(dimNr))
                                        return dto;
                                    break;
                                case TermGroup_TimeProjectPayrollProductAccountingPrio.EmployeeAccount:
                                    #region Employee
                                    // Accounting is fetched from the employee
                                    dto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, dimNr, date);
                                    if (dto != null && dto.HasAccountOnDim(dimNr))
                                        return dto;
                                    #endregion
                                    break;
                                case TermGroup_TimeProjectPayrollProductAccountingPrio.EmployeeGroup:
                                    dto = GetEmployeeGroupAccounts(entities, GetEmployeeGroupAccountType(type), employeeId, actorCompanyId, dimNr != Constants.ACCOUNTDIM_STANDARD);
                                    if (dto != null && dto.HasAccountOnDim(dimNr))
                                        return dto;
                                    break;
                            }
                        }

                        #endregion
                    }

                    #endregion

                    //No No priority setting on Customer
                    //No priority setting on PayrollGroup

                    #region EmployeeGroup

                    if (dto == null || !dto.HasAccountOnDim(dimNr))
                    {
                        if (employee == null)
                            employee = EmployeeManager.GetEmployee(entities, employeeId, actorCompanyId, loadEmployment: true);

                        List<TermGroup_EmployeeGroupPayrollProductAccountingPrio> employeeGroupPrios = GetEmployeeGroupPayrollProductAccountingPrios(actorCompanyId, date, employee, employeeGroups);
                        if (employeeGroupPrios.Any())
                        {
                            #region EmployeeGroup priorities

                            // Employee group priorities found, loop through them and return first accounting found
                            foreach (TermGroup_EmployeeGroupPayrollProductAccountingPrio prio in employeeGroupPrios)
                            {
                                switch (prio)
                                {
                                    case TermGroup_EmployeeGroupPayrollProductAccountingPrio.PayrollProduct:
                                        dto = GetPayrollProductAccounts(setting, type);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmploymentAccount:
                                        dto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, date);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmployeeAccount:
                                        dto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, dimNr, date);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_EmployeeGroupPayrollProductAccountingPrio.Project:
                                        dto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD, true);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_EmployeeGroupPayrollProductAccountingPrio.Customer:
                                        dto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmployeeGroup:
                                        dto = GetEmployeeGroupAccounts(entities, GetEmployeeGroupAccountType(type), employeeId, actorCompanyId, dimNr != Constants.ACCOUNTDIM_STANDARD);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion

                    #region Company

                    if (dto == null || !dto.HasAccountOnDim(dimNr))
                    {
                        List<TermGroup_CompanyPayrollProductAccountingPrio> companyPrios = GetCompanyPayrollProductAccountingPrios(entities, actorCompanyId);
                        if (companyPrios.Any())
                        {
                            #region Company priorities

                            // Company priorities found, loop through them and return first accounting found
                            foreach (TermGroup_CompanyPayrollProductAccountingPrio prio in companyPrios)
                            {
                                switch (prio)
                                {
                                    case TermGroup_CompanyPayrollProductAccountingPrio.PayrollProduct:
                                        dto = GetPayrollProductAccounts(setting, type);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_CompanyPayrollProductAccountingPrio.EmploymentAccount:
                                        dto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, date);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_CompanyPayrollProductAccountingPrio.EmployeeAccount:
                                        dto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, dimNr, date);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_CompanyPayrollProductAccountingPrio.Project:
                                        dto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD, true);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_CompanyPayrollProductAccountingPrio.Customer:
                                        dto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                    case TermGroup_CompanyPayrollProductAccountingPrio.EmployeeGroup:
                                        dto = GetEmployeeGroupAccounts(entities, GetEmployeeGroupAccountType(type), employeeId, actorCompanyId, dimNr != Constants.ACCOUNTDIM_STANDARD);
                                        if (dto != null && dto.HasAccountOnDim(dimNr))
                                            return dto;
                                        break;
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion

                    #region Employment

                    if (dto == null || !dto.HasAccountOnDim(dimNr))
                    {
                        bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);

                        if (!useAccountHierarchy)
                            dto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, date);
                        else
                            dto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, dimNr, date);

                        if (dto != null)
                            return dto;
                    }

                    #endregion

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.PayrollProduct:
                    #region PayrollProduct

                    // Accounting id fetched from the product
                    dto = GetPayrollProductAccounts(setting, type);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.EmploymentAccount:
                    #region Employee

                    // Accounting is fetched from the employee
                    dto = GetEmploymentAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, date);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.EmployeeAccount:
                    #region Employee

                    // Accounting is fetched from the employee
                    dto = GetEmployeeAccounts(entities, GetEmployeeAccountType(type), employeeId, dimNr != Constants.ACCOUNTDIM_STANDARD, dimNr, date);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.Project:
                    #region Project

                    // Accounting is fetched from the project
                    dto = GetProjectAccounts(entities, projectId, GetProjectAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD, true);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.Customer:
                    #region Customer

                    // Accounting is fetched from the customer
                    dto = GetCustomerAccounts(entities, customerId, GetCustomerAccountType(type), dimNr != Constants.ACCOUNTDIM_STANDARD);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.PayrollGroup:
                    #region PayrollGroup

                    if (payrollGroupId.HasValue)
                        dto = GetPayrollGroupAccounts(entities, payrollGroupId.Value, GetPayrollGroupAccountType(type), employeeId, actorCompanyId);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.EmployeeGroup:
                    #region EmployeeGroup

                    dto = GetEmployeeGroupAccounts(entities, GetEmployeeGroupAccountType(type), employeeId, actorCompanyId, dimNr != Constants.ACCOUNTDIM_STANDARD);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.Company:
                    #region Company

                    if (setting != null && setting.PayrollProduct.IsEmploymentTax())
                        dto = GetCompanyPayrollProductAccounts(entities, CompanySettingType.AccountPayrollEmploymentTax, actorCompanyId);
                    else if (setting != null && setting.PayrollProduct.IsSupplementCharge())
                        dto = GetCompanyPayrollProductAccounts(entities, CompanySettingType.AccountPayrollOwnSupplementCharge, actorCompanyId);
                    else
                        dto = GetCompanyPayrollProductAccounts(entities, GetCompanyPayrollProductAccountType(type), actorCompanyId);

                    #endregion
                    break;
                case TermGroup_PayrollProductAccountingPrio.NoAccounting:
                    #region NoAccounting

                    // Accounting is not used for current product
                    return null;

                    #endregion
            }

            // Final try, get default company settings account
            if (dto == null)
            {
                #region Company settings

                if (setting != null && setting.PayrollProduct.IsEmploymentTax())
                    dto = GetCompanyPayrollProductAccounts(entities, CompanySettingType.AccountPayrollEmploymentTax, actorCompanyId);
                else if (setting != null && setting.PayrollProduct.IsSupplementCharge())
                    dto = GetCompanyPayrollProductAccounts(entities, CompanySettingType.AccountPayrollOwnSupplementCharge, actorCompanyId);
                else
                    dto = GetCompanyPayrollProductAccounts(entities, GetCompanyPayrollProductAccountType(type), actorCompanyId);

                #endregion
            }

            return dto;
        }

        /// <summary>
        /// Get product account from company settings
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="type">Company setting type specifying the accountIdState type</param>
        /// <param name="actorCompanyId">Company ID</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetCompanyPayrollProductAccounts(CompEntities entities, CompanySettingType type, int actorCompanyId)
        {
            string key = $"GetCompanyPayrollProductAccountingPrios(CompanySettingType{type}actorCompanyId{actorCompanyId})";

            AccountingPrioDTO dto = null;
            AccountDTO account = BusinessMemoryCache<AccountDTO>.Get(key);
            if (account == null)
            {
                var accountFromDB = GetAccount(entities, actorCompanyId, SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)type, 0, actorCompanyId, 0), loadAccount: true, loadAccountDim: true);
                if (accountFromDB != null)
                    account = accountFromDB.ToDTO();
            }

            if (account != null)
            {
                dto = new AccountingPrioDTO()
                {
                    AccountId = account.AccountId,
                    AccountNr = account.AccountNr,
                    AccountName = account.Name,
                    CompanyType = type,
                    Percent = 100,
                };
                BusinessMemoryCache<AccountDTO>.Set(key, account);
            }

            return dto == null || dto.AccountId == 0 ? null : dto;
        }

        /// <summary>
        /// Get accounting priority settings for specified company
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="Company ID"></param>
        /// <returns>List of company accounting priority settings</returns>
        private List<TermGroup_CompanyPayrollProductAccountingPrio> GetCompanyPayrollProductAccountingPrios(CompEntities entities, int actorCompanyId)
        {
            string key = $"GetCompanyPayrollProductAccountingPrios(actorCompanyId{actorCompanyId})";
            List<TermGroup_CompanyPayrollProductAccountingPrio> prios = BusinessMemoryCache<List<TermGroup_CompanyPayrollProductAccountingPrio>>.Get(key);

            if (prios == null)
            {
                prios = new List<TermGroup_CompanyPayrollProductAccountingPrio>();
                if (actorCompanyId != 0)
                {
                    // Get company accounting priority settings
                    TermGroup_CompanyPayrollProductAccountingPrio prio;
                    string accountingPrioSetting = SettingManager.GetStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.TimeCompanyPayrollProductAccountingPrio, 0, actorCompanyId, 0);
                    if (!String.IsNullOrEmpty(accountingPrioSetting))
                    {
                        string[] accountingPrios = accountingPrioSetting.Split(',');
                        foreach (string accountingPrio in accountingPrios)
                        {
                            prio = (TermGroup_CompanyPayrollProductAccountingPrio)Int32.Parse(accountingPrio);
                            if (prio != TermGroup_CompanyPayrollProductAccountingPrio.NotUsed)
                                prios.Add(prio);
                        }
                    }
                    BusinessMemoryCache<List<TermGroup_CompanyPayrollProductAccountingPrio>>.Set(key, prios);
                }
            }

            return prios;
        }

        /// <summary>
        /// Get accounting priority settings for specified employees employee group
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of employee group accounting priority settings</returns>
        private List<TermGroup_EmployeeGroupPayrollProductAccountingPrio> GetEmployeeGroupPayrollProductAccountingPrios(int actorCompanyId, DateTime? date, Employee employee, List<EmployeeGroup> employeeGroups = null)
        {
            if (employee == null)
                return new List<TermGroup_EmployeeGroupPayrollProductAccountingPrio>();

            if (!date.HasValue)
                date = DateTime.Today;

            string key = $"GetEmployeeGroupPayrollProductAccountingPrios(actorCompanyId{actorCompanyId}#employeeId{employee.EmployeeId}#date{date.Value})";
            List<TermGroup_EmployeeGroupPayrollProductAccountingPrio> prios = BusinessMemoryCache<List<TermGroup_EmployeeGroupPayrollProductAccountingPrio>>.Get(key);

            if (prios == null)
            {
                prios = new List<TermGroup_EmployeeGroupPayrollProductAccountingPrio>();

                // Get EmployeeGroup
                EmployeeGroup employeeGroup = employee.GetEmployeeGroup(date, employeeGroups);
                if (employeeGroup != null)
                {
                    // Get employee group accounting priority settings
                    TermGroup_EmployeeGroupPayrollProductAccountingPrio prio;
                    string[] accountingPrios = employeeGroup.PayrollProductAccountingPrio.Split(',');
                    foreach (string accountingPrio in accountingPrios)
                    {
                        prio = (TermGroup_EmployeeGroupPayrollProductAccountingPrio)Int32.Parse(accountingPrio);
                        if (prio != TermGroup_EmployeeGroupPayrollProductAccountingPrio.NotUsed)
                        {
                            prios.Add(prio);
                        }
                    }

                    BusinessMemoryCache<List<TermGroup_EmployeeGroupPayrollProductAccountingPrio>>.Set(key, prios);
                }
            }

            return prios;
        }

        /// <summary>
        /// Get accounting priority settings for specified project
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of project accounting priority settings</returns>
        private List<TermGroup_TimeProjectPayrollProductAccountingPrio> GetTimeProjectPayrollProductAccountingPrios(CompEntities entities, int projectId)
        {
            List<TermGroup_TimeProjectPayrollProductAccountingPrio> prios = new List<TermGroup_TimeProjectPayrollProductAccountingPrio>();

            if (projectId != 0)
            {
                // Get project
                TimeProject project = entities.Project.OfType<TimeProject>().Where(p => p.ProjectId == projectId).FirstOrDefault();
                if (project != null)
                {
                    if (project.ParentProjectId != null && (int)project.ParentProjectId != 0 && !project.UseAccounting)
                    {
                        int topProjectId = ProjectManager.FindTopProject(entities, (int)project.ParentProjectId);
                        project = entities.Project.OfType<TimeProject>().Where(p => p.ProjectId == topProjectId).FirstOrDefault();

                        if (project != null)
                        {
                            // Get project accounting priority settings
                            TermGroup_TimeProjectPayrollProductAccountingPrio prio;
                            string[] accountingPrios = project.PayrollProductAccountingPrio.Split(',');
                            foreach (string accountingPrio in accountingPrios)
                            {
                                prio = (TermGroup_TimeProjectPayrollProductAccountingPrio)Int32.Parse(accountingPrio);
                                if (prio != TermGroup_TimeProjectPayrollProductAccountingPrio.NotUsed)
                                    prios.Add(prio);
                            }
                        }
                    }
                    else
                    {
                        // Get project accounting priority settings
                        TermGroup_TimeProjectPayrollProductAccountingPrio prio;
                        string[] accountingPrios = project.PayrollProductAccountingPrio.Split(',');
                        foreach (string accountingPrio in accountingPrios)
                        {
                            prio = (TermGroup_TimeProjectPayrollProductAccountingPrio)Int32.Parse(accountingPrio);
                            if (prio != TermGroup_TimeProjectPayrollProductAccountingPrio.NotUsed)
                                prios.Add(prio);
                        }
                    }
                }
            }

            return prios;
        }

        #endregion

        #region Help-methods

        #region Convert between account types

        /// <summary>
        /// Convert product accountIdState type to customer accountIdState type
        /// </summary>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>Customer accountIdState type</returns>
        private CustomerAccountType GetCustomerAccountType(ProductAccountType type)
        {
            switch (type)
            {
                case ProductAccountType.Sales:
                case ProductAccountType.SalesNoVat:
                case ProductAccountType.SalesContractor:
                    return CustomerAccountType.Credit;
                case ProductAccountType.Purchase:
                    return CustomerAccountType.Debit;
                case ProductAccountType.VAT:
                    return CustomerAccountType.VAT;
            }

            // Should never end up here
            return CustomerAccountType.Credit;
        }

        /// <summary>
        /// Convert product accountIdState type to employee accountIdState type
        /// </summary>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>Employee accountIdState type</returns>
        private EmploymentAccountType GetEmployeeAccountType(ProductAccountType type)
        {
            switch (type)
            {
                case ProductAccountType.Sales:
                case ProductAccountType.SalesNoVat:
                case ProductAccountType.SalesContractor:
                    return EmploymentAccountType.Income;
                case ProductAccountType.Purchase:
                    return EmploymentAccountType.Cost;
            }

            // Should never end up here
            return EmploymentAccountType.Income;
        }

        /// <summary>
        /// Convert product accountIdState type to employee group accountIdState type
        /// </summary>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>Employee group accountIdState type</returns>
        private EmployeeGroupAccountType GetEmployeeGroupAccountType(ProductAccountType type)
        {
            switch (type)
            {
                case ProductAccountType.Sales:
                case ProductAccountType.SalesNoVat:
                case ProductAccountType.SalesContractor:
                    return EmployeeGroupAccountType.Income;
                case ProductAccountType.Purchase:
                    return EmployeeGroupAccountType.Cost;
            }

            // Should never end up here
            return EmployeeGroupAccountType.Income;
        }

        /// <summary>
        /// Convert product accountIdState type to payroll group accountIdState type
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>Payroll group accountIdState type</returns>
        private PayrollGroupAccountType GetPayrollGroupAccountType(ProductAccountType type)
        {
            switch (type)
            {
                case ProductAccountType.Purchase:
                    return PayrollGroupAccountType.EmploymentTax;
                case ProductAccountType.Sales:
                case ProductAccountType.SalesContractor:
                case ProductAccountType.SalesNoVat:
                    return PayrollGroupAccountType.PayrollTax;
                case ProductAccountType.VAT:
                    return PayrollGroupAccountType.OwnSupplementCharge;
                default:
                    return PayrollGroupAccountType.Unknown;
            }
        }

        /// <summary>
        /// Convert product accountIdState type to project accountIdState type
        /// </summary>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>Project accountIdState type</returns>
        private ProjectAccountType GetProjectAccountType(ProductAccountType type)
        {
            switch (type)
            {
                case ProductAccountType.Sales:
                    return ProjectAccountType.Credit;
                case ProductAccountType.SalesNoVat:
                    return ProjectAccountType.SalesNoVat;
                case ProductAccountType.SalesContractor:
                    return ProjectAccountType.SalesContractor;
                case ProductAccountType.Purchase:
                    return ProjectAccountType.Debit;
            }

            // Should never end up here
            return ProjectAccountType.Credit;
        }

        /// <summary>
        /// Convert product accountIdState type to company setting accountIdState type
        /// </summary>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>Company setting accountIdState type</returns>
        private CompanySettingType GetCompanyInvoiceProductAccountType(ProductAccountType type)
        {
            switch (type)
            {
                case ProductAccountType.Sales:
                    return CompanySettingType.AccountInvoiceProductSales;
                case ProductAccountType.SalesNoVat:
                    return CompanySettingType.AccountInvoiceProductSalesVatFree;
                case ProductAccountType.SalesContractor:
                    return CompanySettingType.AccountCommonReverseVatSales;
                case ProductAccountType.Purchase:
                    return CompanySettingType.AccountInvoiceProductPurchase;
                case ProductAccountType.VAT:
                    return CompanySettingType.AccountCommonVatPayable1;
                case ProductAccountType.TripartiteTrade:
                    return CompanySettingType.AccountCustomerSalesTripartiteTrade;
            }

            // Should never end up here
            return CompanySettingType.AccountInvoiceProductSales;
        }

        /// <summary>
        /// Convert product accountIdState type to company setting accountIdState type
        /// </summary>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>Company setting accountIdState type</returns>
        private CompanySettingType GetCompanyPayrollProductAccountType(ProductAccountType type)
        {
            switch (type)
            {
                case ProductAccountType.Sales:
                case ProductAccountType.SalesNoVat:
                case ProductAccountType.SalesContractor:
                    return CompanySettingType.AccountEmployeeGroupIncome;
                case ProductAccountType.Purchase:
                    return CompanySettingType.AccountEmployeeGroupCost;
            }

            // Should never end up here
            return CompanySettingType.AccountEmployeeGroupCost;
        }

        #endregion

        #region Accounting priority

        /// <summary>
        /// Get accounting priority value for specified dimension
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="dimNr">Accounting dim number</param>
        /// <returns>Priority value</returns>
        public int GetProductAccountingPrio(Product product, int dimNr)
        {
            if (product == null)
                return 0;
            return GetProductAccountingPrio(product.AccountingPrio.Split(','), dimNr);
        }

        public int GetPayrollProductAccountingPrio(PayrollProductSetting setting, int dimNr)
        {
            if (setting == null)
                return 0;
            return GetProductAccountingPrio(setting.AccountingPrio.Split(','), dimNr);
        }

        /// <summary>
        /// Get accounting priority value for specified dimension
        /// </summary>
        /// <param name="accountingPrios">String array och accounting priorities in format n=p</param>
        /// <param name="dimNr">Accounting dim number</param>
        /// <returns>Priority value</returns>
        public int GetProductAccountingPrio(string[] accountingPrios, int dimNr)
        {
            if (accountingPrios != null)
            {
                try
                {
                    string[] keyValue;
                    foreach (string prio in accountingPrios)
                    {
                        keyValue = prio.Split('=');
                        if (keyValue.Length == 2 && Int32.Parse(keyValue[0]) == dimNr)
                        {
                            Int32.TryParse(keyValue[1], out int val);
                            return val;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }
            }

            return 0;
        }

        #endregion

        #region Get product standard accounts from different entities

        /// <summary>
        /// Get product standard account of specified type
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="type">Product accountIdState type</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetProductAccountStds(Product product, ProductAccountType type, bool getInternalAccounts = false)
        {
            AccountingPrioDTO dto = null;

            if (product != null)
            {
                // Extra check to see that the standards really are loaded
                if (!product.ProductAccountStd.IsLoaded)
                    product.ProductAccountStd.Load();

                ProductAccountStd productAccountStd = product.ProductAccountStd?.FirstOrDefault(pas => pas.Type == (int)type);
                if (productAccountStd != null)
                {
                    dto = new AccountingPrioDTO()
                    {
                        AccountId = productAccountStd.AccountStd?.AccountId,
                        AccountNr = productAccountStd.AccountStd?.Account?.AccountNr ?? string.Empty,
                        AccountName = productAccountStd.AccountStd?.Account?.Name ?? string.Empty,
                        ProductType = type,
                        Percent = 100,
                        AccountInternals = getInternalAccounts ? productAccountStd.AccountInternal?.ToDTOs() : null,
                    };
                }
            }

            return dto;

        }

        /// <summary>
        /// Get account for specified payroll product
        /// </summary>
        /// <param name="setting">Payroll product setting</param>
        /// <param name="type">Payroll product accountIdState type</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetPayrollProductAccounts(PayrollProductSetting setting, ProductAccountType type)
        {
            if (setting == null)
                return null;

            PayrollProductAccountStd productAccountStd = setting.PayrollProductAccountStd.FirstOrDefault(i => i.Type == (int)type);
            if (productAccountStd == null)
                return null;

            bool hasAccountStd = productAccountStd.AccountStd != null;
            bool hasAccountInternals = !productAccountStd.AccountInternal.IsNullOrEmpty();
            if (!hasAccountStd && !hasAccountInternals)
                return null;

            Account account = productAccountStd.AccountStd?.Account;
            AccountingPrioDTO dto = new AccountingPrioDTO()
            {
                AccountId = account?.AccountId,
                AccountNr = account?.AccountNr ?? String.Empty,
                AccountName = account?.Name ?? String.Empty,
                ProductType = (ProductAccountType)productAccountStd.Type,
                Percent = 100,
            };

            if (hasAccountInternals)
            {
                dto.AccountInternals = new List<AccountInternalDTO>();
                foreach (AccountInternal accountInternal in productAccountStd.AccountInternal)
                {
                    dto.AccountInternals.Add(accountInternal.ToDTO());
                }
            }
            if (dto != null)
                dto.AddLog("GetPayrollProductAccounts");
            return dto;
        }

        /// <summary>
        /// Get account for specified employees employee group
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="type">Employee group accountIdState type</param>
        /// <param name="getInternalAccounts">If true, internal account are also included</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetEmployeeGroupAccounts(CompEntities entities, EmployeeGroupAccountType type, int employeeId, int actorCompanyId, bool getInternalAccounts = false)
        {
            if (employeeId == 0)
                return null;

            string key = $"GetEmployeeGroupAccounts#EmployeeGroupAccountType{type}#employeeId{employeeId}#actorCompanyId{actorCompanyId}#getInternalAccounts{getInternalAccounts}";
            string keyGroup = "";
            AccountingPrioDTO dto = BusinessMemoryCache<AccountingPrioDTO>.Get(key);

            if (dto == null)
            {
                EmployeeGroup employeeGroup = EmployeeManager.GetEmployeeGroupForEmployee(entities, employeeId, actorCompanyId, DateTime.Today);
                if (employeeGroup != null)
                {
                    keyGroup = $"GetEmployeeGroupAccount#EmployeeGroupAccountType{type}#EmployeeGroupId{employeeGroup.EmployeeGroupId}#actorCompanyId{actorCompanyId}#getInternalAccounts{getInternalAccounts}";
                    dto = BusinessMemoryCache<AccountingPrioDTO>.Get(keyGroup);

                    if (dto == null)
                    {
                        EmployeeGroupAccountStd employeeGroupAccountStd = EmployeeManager.GetEmployeeGroupAccount(entities, employeeGroup.EmployeeGroupId, type);
                        if (employeeGroupAccountStd != null)
                        {
                            bool hasAccountStd = employeeGroupAccountStd.AccountStd != null;
                            bool hasAccountInternals = !employeeGroupAccountStd.AccountInternal.IsNullOrEmpty();
                            if (!(!hasAccountStd && !hasAccountInternals))
                            {
                                Account account = employeeGroupAccountStd.AccountStd?.Account;
                                dto = new AccountingPrioDTO()
                                {
                                    AccountId = account?.AccountId,
                                    AccountNr = account?.AccountNr ?? String.Empty,
                                    AccountName = account?.Name ?? String.Empty,
                                    EmployeeGroupType = (EmployeeGroupAccountType)employeeGroupAccountStd.Type,
                                    Percent = 100,
                                };

                                if (getInternalAccounts && hasAccountInternals)
                                {
                                    dto.AccountInternals = new List<AccountInternalDTO>();
                                    foreach (AccountInternal accountInternal in employeeGroupAccountStd.AccountInternal)
                                    {
                                        dto.AccountInternals.Add(accountInternal.ToDTO());
                                    }
                                }
                            }
                        }
                    }
                }

                if (dto == null)
                    dto = new AccountingPrioDTO() { AccountId = -1 };

                BusinessMemoryCache<AccountingPrioDTO>.Set(key, dto);
                if (!string.IsNullOrEmpty(keyGroup))
                    BusinessMemoryCache<AccountingPrioDTO>.Set(keyGroup, dto);
            }

            if (dto.AccountId == -1)
                return null;

            if (dto != null)
                dto.AddLog("GetEmployeeGroupAccounts");

            return dto;
        }

        /// <summary>
        /// Get account for specified employee, converted into collection of product account
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="type">Employee accountIdState type</param>
        /// <param name="getInternalAccounts">If true, internal account are also included</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetEmploymentAccounts(CompEntities entities, EmploymentAccountType type, int employeeId, bool getInternalAccounts = false, DateTime? date = null)
        {
            if (employeeId == 0)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            string key = $"GetEmploymentAccounts#EmploymentAccountType{type}#employeeId{employeeId}#getInternalAccounts{getInternalAccounts}#date{date}";
            AccountingPrioDTO dto = BusinessMemoryCache<AccountingPrioDTO>.Get(key);

            if (dto == null)
            {
                string allEmploymentAccountStdsOnEmployeeKey = $"GetEmploymentAccountsFromEmployee#EmploymentAccountType{type}#employeeId{employeeId}";
                List<EmploymentAccountStd> employmentAccountStds = BusinessMemoryCache<List<EmploymentAccountStd>>.Get(allEmploymentAccountStdsOnEmployeeKey);
                if (employmentAccountStds == null)
                {
                    employmentAccountStds = EmployeeManager.GetEmploymentAccountsFromEmployee(entities, employeeId, type);
                    BusinessMemoryCache<List<EmploymentAccountStd>>.Set(allEmploymentAccountStdsOnEmployeeKey, employmentAccountStds);
                }

                bool addToCache = employmentAccountStds.Count <= 1;

                EmploymentAccountStd employmentAccountStd = employmentAccountStds.GetAccount(employeeId, type, date);
                if (employmentAccountStd != null)
                {
                    bool hasAccountStd = employmentAccountStd.AccountStd != null;
                    bool hasAccountInternals = !employmentAccountStd.AccountInternal.IsNullOrEmpty();

                    //Must have AccountStd or AccountInternal
                    if (hasAccountStd || (getInternalAccounts && hasAccountInternals))
                    {
                        Account account = employmentAccountStd.AccountStd?.Account;

                        dto = new AccountingPrioDTO()
                        {
                            AccountId = account?.AccountId,
                            AccountNr = account?.AccountNr ?? String.Empty,
                            AccountName = account?.Name ?? String.Empty,
                            EmploymentType = (EmploymentAccountType)employmentAccountStd.Type,
                            Percent = 100,
                        };

                        // Internal account
                        if (getInternalAccounts && hasAccountInternals)
                        {
                            dto.AccountInternals = new List<AccountInternalDTO>();
                            foreach (AccountInternal accountInternal in employmentAccountStd.AccountInternal)
                            {
                                dto.AccountInternals.Add(accountInternal.ToDTO());
                            }
                        }
                    }
                }
                if (addToCache)
                {
                    if (dto == null)
                        dto = new AccountingPrioDTO() { AccountId = -1 };

                    BusinessMemoryCache<AccountingPrioDTO>.Set(key, dto);
                }
            }
            else
            {
                if (dto.AccountId == -1)
                    return null;
                else
                    return dto;
            }

            if (dto != null)
                dto.AddLog("GetEmploymentAccounts");
            return dto == null || dto.AccountId == -1 ? null : dto;
        }

        public string GetEmployeeAccountCacheKey(int employeeId, EmploymentAccountType type)
        {
            return $"GetEmployeeAccounts#EmploymentAccountType{type}#employeeId{employeeId}";
        }

        /// <summary>
        /// Get account for specified employee from employeeAccounts, converted into collection of product account
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="type">Employee accountIdState type</param>
        /// <param name="getInternalAccounts">If true, internal account are also included</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetEmployeeAccounts(CompEntities entities, EmploymentAccountType type, int employeeId, bool getInternalAccounts = false, int? dimNr = null, DateTime? date = null)
        {
            if (employeeId == 0)
                return null;

            if (!date.HasValue)
                date = DateTime.Today;

            AccountingPrioDTO dto = null;

            bool useAccountHierarchy = base.UseAccountHierarchyOnCompanyFromCache(entities, base.ActorCompanyId);
            if (useAccountHierarchy && getInternalAccounts)
            {
                string allEmployeeAccountsOnEmployeeKey = GetEmployeeAccountCacheKey(employeeId, type);
                List<EmployeeAccountDTO> allEmployeeAccountsOnEmployee = BusinessMemoryCache<List<EmployeeAccountDTO>>.Get(allEmployeeAccountsOnEmployeeKey);
                if (allEmployeeAccountsOnEmployee == null)
                {
                    allEmployeeAccountsOnEmployee = EmployeeManager.GetEmployeeAccounts(entities, base.ActorCompanyId, employeeId).ToDTOs().ToList();
                    BusinessMemoryCache<List<EmployeeAccountDTO>>.Set(allEmployeeAccountsOnEmployeeKey, allEmployeeAccountsOnEmployee);
                }

                List<EmployeeAccountDTO> employeeAccounts = allEmployeeAccountsOnEmployee.GetEmployeeAccounts(date).Where(i => i.AccountId.HasValue).ToList();

                if (dimNr.HasValue)
                {
                    var accountInternalsFromCache = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(base.ActorCompanyId));
                    var accountIdsOnDim = accountInternalsFromCache.Where(w => w.AccountDimNr == dimNr.Value && !w.HierarchyOnly).Select(s => s.AccountId).ToList();

                    if (!accountIdsOnDim.Any())
                        accountIdsOnDim = accountInternalsFromCache.Where(w => w.AccountDimNr == dimNr.Value).Select(s => s.AccountId).ToList();

                    if (accountIdsOnDim.Any())
                    {
                        var filteredEmployeeAccounts = employeeAccounts.Where(w => !w.ParentEmployeeAccountId.HasValue && w.AccountId.HasValue && accountIdsOnDim.Contains(w.AccountId.Value)).ToList();

                        if (!filteredEmployeeAccounts.Any())
                        {
                            filteredEmployeeAccounts = employeeAccounts.Where(w => w.AccountId.HasValue && accountIdsOnDim.Contains(w.AccountId.Value)).ToList();

                            if (filteredEmployeeAccounts.Count == 1)
                                employeeAccounts = filteredEmployeeAccounts;
                        }
                        else
                        {
                            employeeAccounts = filteredEmployeeAccounts;
                        }
                    }
                }

                if (!employeeAccounts.IsNullOrEmpty())
                {
                    int accountId = 0;
                    if (employeeAccounts.Count == 1)
                    {
                        accountId = employeeAccounts.First().AccountId.Value;
                    }
                    else
                    {
                        var (_, isValidSetting, accountIds) = GetAccountHierarchySetting(entities, base.ActorCompanyId, base.UserId);
                        if (isValidSetting)
                        {
                            EmployeeAccountDTO employeeAccount = employeeAccounts.FirstOrDefault(i => accountIds.Contains(i.AccountId.Value));
                            if (employeeAccount != null)
                                accountId = employeeAccount.AccountId.Value;
                        }

                        if (accountId == 0)
                        {
                            var employeeAccountsMainAllocation = employeeAccounts.Where(x => !x.ParentEmployeeAccountId.HasValue && x.MainAllocation).ToList();
                            if (employeeAccountsMainAllocation.Count == 1)
                            {
                                accountId = employeeAccountsMainAllocation.First().AccountId.Value;
                            }
                        }

                        if (accountId == 0)
                        {
                            var employeeAccountsOnlyParents = employeeAccounts.Where(x => !x.ParentEmployeeAccountId.HasValue).ToList();
                            if (employeeAccountsOnlyParents.Count == 1)
                            {
                                accountId = employeeAccountsOnlyParents.First().AccountId.Value;
                            }
                        }
                    }

                    if (accountId > 0)
                    {
                        dto = new AccountingPrioDTO()
                        {
                            AccountId = accountId,
                            AccountNr = String.Empty,
                            AccountName = String.Empty,
                            EmploymentType = EmploymentAccountType.Cost,
                            Percent = 100,
                        };
                        dto.AccountInternals = GetAccountInternalAndParents(entities, accountId, base.ActorCompanyId);
                    }
                }
            }
            if (dto != null)
                dto.AddLog("GetEmployeeAccounts");
            return dto == null || dto.AccountId == -1 ? null : dto;
        }


        /// <summary>
        /// Get account for specified project, converted into collection of product account
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="projectId">Project ID</param>
        /// <param name="type">Project accountIdState type</param>
        /// <param name="getInternalAccounts">If true, internal account are also included</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetProjectAccounts(CompEntities entities, int projectId, ProjectAccountType type, bool getInternalAccounts, bool isTimeProjectRow = false)
        {
            AccountingPrioDTO dto = null;

            if (type == ProjectAccountType.Debit && !isTimeProjectRow)
                return null;

            if (projectId != 0)
            {
                // ProjectAccountStd
                ProjectAccountStd pAccountStd = ProjectManager.GetProjectAccount(entities, projectId, isTimeProjectRow ? ProjectAccountType.Debit : type);

                bool hasAccountInternals = pAccountStd != null && pAccountStd.AccountInternal != null && pAccountStd.AccountInternal.Count > 0;
                if (pAccountStd != null) //Must have AccountStd
                {
                    Account account = pAccountStd.AccountStd != null && pAccountStd.AccountStd.Account != null ? pAccountStd.AccountStd.Account : null;

                    dto = new AccountingPrioDTO
                    {
                        AccountId = pAccountStd?.AccountStd?.AccountId,
                        AccountNr = account != null ? account.AccountNr : string.Empty,
                        AccountName = account != null ? account.Name : string.Empty,
                        ProjectType = (ProjectAccountType)pAccountStd.Type,
                        Percent = 100,
                    };

                    // Internal account
                    if (getInternalAccounts && hasAccountInternals)
                    {
                        dto.AccountInternals = new List<AccountInternalDTO>();
                        foreach (AccountInternal accountInternal in pAccountStd.AccountInternal)
                        {
                            dto.AccountInternals.Add(accountInternal.ToDTO());
                        }
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Get account for specified customer, converted into collection of product account
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="customerId">Customer ID</param>
        /// <param name="type">Customer accountIdState type</param>
        /// <param name="getInternalAccounts">If true, internal account are also included</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetCustomerAccounts(CompEntities entities, int customerId, CustomerAccountType type, bool getInternalAccounts = false)
        {
            if (getInternalAccounts)
            {
                return (from cas in entities.CustomerAccountStd

                        where (
                            cas.ActorCustomerId == customerId &&
                            cas.Type == (int)type
                        )
                        select new AccountingPrioDTO
                        {
                            AccountId = cas.AccountStd.AccountId,
                            AccountNr = cas.AccountStd.Account.AccountNr,
                            AccountName = cas.AccountStd.Account.Name,
                            CustomerType = (CustomerAccountType)cas.Type,
                            Percent = 100,
                            AccountInternals = cas.AccountInternal.Select(aci => new AccountInternalDTO
                            {
                                AccountId = aci.AccountId,
                                AccountNr = aci.Account.AccountNr,
                                Name = aci.Account.Name,
                                AccountDimId = aci.Account.AccountDimId,
                                AccountDimNr = aci.Account.AccountDim.AccountDimNr,
                                SysSieDimNr = aci.Account.AccountDim.SysSieDimNr,
                                UseVatDeduction = aci.UseVatDeduction,
                                VatDeduction = aci.VatDeduction,
                            }).ToList()
                        }).FirstOrDefault();
            }
            else
            {
                return (from cas in entities.CustomerAccountStd

                        where (
                            cas.ActorCustomerId == customerId &&
                            cas.Type == (int)type
                        )
                        select new AccountingPrioDTO
                        {
                            AccountId = cas.AccountStd.AccountId,
                            AccountNr = cas.AccountStd.Account.AccountNr ?? string.Empty,
                            AccountName = cas.AccountStd.Account.Name ?? string.Empty,
                            CustomerType = (CustomerAccountType)cas.Type,
                            Percent = 100
                        }).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get account for specified payroll group, converted into collection of product account
        /// </summary>
        /// <param name="entities">Object context</param>
        /// <param name="payrollGroupId">PayrollGroup ID</param>
        /// <param name="type">Customer accountIdState type</param>
        /// <returns>AccountingPrioDTO</returns>
        private AccountingPrioDTO GetPayrollGroupAccounts(CompEntities entities, int payrollGroupId, PayrollGroupAccountType type, int employeeId, int actorCompanyId)
        {
            AccountingPrioDTO dto = null;

            DateTime? birthDate = EmployeeManager.GetEmployeeBirthDate(entities, employeeId, actorCompanyId);
            if (!birthDate.HasValue)
                return dto;

            if (payrollGroupId != 0)
            {
                PayrollGroupAccountStd payrollGroupAccountStd = PayrollManager.GetPayrollGroupAccountStd(entities, payrollGroupId, type, birthDate.Value.Year);
                if (payrollGroupAccountStd?.AccountStd != null)
                {
                    dto = new AccountingPrioDTO()
                    {
                        AccountId = payrollGroupAccountStd.AccountId,
                        AccountNr = payrollGroupAccountStd.AccountStd.Account?.AccountNr ?? string.Empty,
                        AccountName = payrollGroupAccountStd.AccountStd.Account?.Name ?? string.Empty,
                        PayrollGroupType = (PayrollGroupAccountType)payrollGroupAccountStd.Type,
                        Percent = 100,
                    };
                }
            }

            return dto;
        }

        #endregion

        #endregion

        #endregion

        #region AccountSru

        public ActionResult SaveAccountSru(AccountStd accountStd, int?[] sruCodes)
        {
            if (accountStd == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountStd");

            // Default result is successful
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                AccountStd accountStdOriginal = (from a in entities.AccountStd
                                                 where a.AccountId == accountStd.AccountId &&
                                                 (a.Account.State == (int)SoeEntityState.Active || a.Account.State == (int)SoeEntityState.Inactive)
                                                 select a).FirstOrDefault();

                if (accountStdOriginal == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                accountStdOriginal.AccountSru.Load();

                //First, delete all existing AccountSru entities that not is in passed sruCodes
                for (int i = accountStdOriginal.AccountSru.Count - 1; i >= 0; i--)
                {
                    AccountSru accountSru = accountStdOriginal.AccountSru.ElementAt(i);

                    bool exist = false;
                    for (int j = 0; j < sruCodes.Length; j++)
                    {
                        if (sruCodes[j] == null)
                            continue;

                        if (sruCodes[j].Value == accountSru.SysAccountSruCodeId)
                        {
                            sruCodes[i] = null;
                            exist = true;
                            break;
                        }
                    }

                    if (!exist)
                        result = DeleteEntityItem(entities, accountSru);
                }

                //Second, add the new AccountSru entities for Account
                for (int i = 0; i < sruCodes.Length; i++)
                {
                    if (sruCodes[i] == null)
                        continue;

                    AccountSru accountSru = new AccountSru()
                    {
                        SysAccountSruCodeId = sruCodes[i].Value,
                        AccountStd = accountStdOriginal,
                    };

                    result = AddEntityItem(entities, accountSru, "AccountSru");
                }
            }

            return result;
        }

        public AccountSru SetAccountSru(CompEntities entities, AccountStd accountStd, AccountSru accountSru, int? accountSruId)
        {
            accountSruId = accountSruId.ToNullable();

            if (accountSru != null)
            {
                if (accountSruId.HasValue)
                    accountSru.SysAccountSruCodeId = accountSruId.Value;
                else
                    entities.DeleteObject(accountSru);
            }
            else if (accountSruId.HasValue)
            {
                accountSru = new AccountSru()
                {
                    SysAccountSruCodeId = accountSruId.Value,
                };

                entities.AccountSru.AddObject(accountSru);
                accountStd.AccountSru.Add(accountSru);
            }

            return accountSru;
        }

        #endregion

        #region AccrualAccountMapping

        public List<AccrualAccountMappingDTO> GetAccrualAccountMappings(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountMapping.NoTracking();
            return GetAccrualAccountMappings(entities, actorCompanyId).ToDTOList();
        }

        public List<AccrualAccountMapping> GetAccrualAccountMappings(CompEntities entities, int actorCompanyId, int? accountId = null)
        {
            var query = from aam in entities.AccrualAccountMapping.Include("Account").Include("Account1")
                        where aam.ActorCompanyId == actorCompanyId
                        select aam;
            if (!accountId.IsNullOrEmpty())
                query.Where(aam => aam.Account1.AccountId == accountId);
            return query.ToList();
        }

        public ActionResult SaveAccrualAccountMappings(List<AccrualAccountMappingDTO> accrualAccountMappings, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var existingAccrualAccountMappings = GetAccrualAccountMappings(entities, actorCompanyId);

                        foreach (var accrualAccountMapping in accrualAccountMappings)
                        {
                            AccrualAccountMapping mapping = existingAccrualAccountMappings.Find(acc => acc.SourceAccountId == accrualAccountMapping.SourceAccountId);

                            if (mapping != null)
                            {
                                mapping.TargetAccrualAccountId = accrualAccountMapping.TargetAccrualAccountId;
                            }
                            else
                            {
                                mapping = new AccrualAccountMapping();
                                mapping.ActorCompanyId = actorCompanyId;
                                mapping.SourceAccountId = accrualAccountMapping.SourceAccountId;
                                mapping.TargetAccrualAccountId = accrualAccountMapping.TargetAccrualAccountId;

                                entities.AccrualAccountMapping.AddObject(mapping);
                            }
                        }

                        foreach (var exMappings in existingAccrualAccountMappings)
                        {
                            if (!accrualAccountMappings.Any(m => m.SourceAccountId == exMappings.SourceAccountId))
                            {
                                entities.AccrualAccountMapping.DeleteObject(exMappings);
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return result;
                        else
                            transaction.Complete();
                    }
                }
                finally
                {
                    if (!result.Success)
                    {
                        base.LogTransactionFailed(this.ToString(), this.log);
                    }

                    entities.Connection.Close();
                }

                return result;
            }
        }


        #endregion

        #region AccountMapping

        public List<AccountMapping> GetAccountMappings(CompEntities entities, int actorCompanyId, int accountId)
        {
            return (from am in entities.AccountMapping
                    where am.AccountId == accountId &&
                    am.Account.ActorCompanyId == actorCompanyId
                    select am).ToList();
        }

        public List<AccountMappingDTO> GetAccountMappingsForAllDims(int actorCompanyId, int accountId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountMapping.NoTracking();
            return GetAccountMappingsForAllDims(entities, actorCompanyId, accountId);
        }

        public List<AccountMappingDTO> GetAccountMappingsForAllDims(CompEntities entities, int actorCompanyId, int accountId)
        {
            var dtos = new List<AccountMappingDTO>();

            var mandatoryLevels = base.GetTermGroupContent(TermGroup.AccountMandatoryLevel, addEmptyRow: true, skipUnknown: true);
            var accountMappings = accountId == 0 ? new List<AccountMapping>() : GetAccountMappings(entities, actorCompanyId, accountId);
            var accountDimInternals = GetAccountDimsByCompany(actorCompanyId, onlyInternal: true, loadAccounts: true).ToDTOs(true);

            foreach (var accountDimInternal in accountDimInternals.OrderBy(i => i.AccountDimNr))
            {
                var accountMapping = accountMappings.FirstOrDefault(i => i.AccountDimId == accountDimInternal.AccountDimId);

                dtos.Add(new AccountMappingDTO()
                {
                    AccountId = accountMapping != null ? accountMapping.AccountId : 0,
                    AccountDimId = accountMapping != null ? accountMapping.AccountDimId : accountDimInternal.AccountDimId,
                    DefaultAccountId = accountMapping != null ? accountMapping.DefaultAccountId : (int?)null,
                    MandatoryLevel = accountMapping != null && accountMapping.MandatoryLevel.HasValue ? (TermGroup_AccountMandatoryLevel)accountMapping.MandatoryLevel.Value : TermGroup_AccountMandatoryLevel.None,
                    AccountDimName = accountDimInternal.Name,
                    Accounts = accountDimInternal.Accounts,
                    MandatoryLevels = mandatoryLevels,
                });
            }

            return dtos;
        }

        public AccountMapping GetAccountMapping(int accountId, int accountDimId, int actorCompanyId, bool onlyActiveAccount, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountInternal = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountMapping.NoTracking();
            return GetAccountMapping(entities, accountId, accountDimId, actorCompanyId, onlyActiveAccount, loadAccount, loadAccountDim, loadAccountInternal);
        }

        public AccountMapping GetAccountMapping(CompEntities entities, int accountId, int accountDimId, int actorCompanyId, bool onlyActiveAccount = false, bool loadAccount = false, bool loadAccountDim = false, bool loadAccountInternal = false)
        {
            AccountMapping accountMapping = (from am in entities.AccountMapping
                                             where am.AccountId == accountId &&
                                             am.AccountDimId == accountDimId &&
                                             am.Account.ActorCompanyId == actorCompanyId &&
                                             am.Account.State != (int)SoeEntityState.Deleted &&
                                             am.AccountDim.State == (int)SoeEntityState.Active
                                             select am).FirstOrDefault();

            if (accountMapping != null)
            {
                if (loadAccount || onlyActiveAccount)
                {
                    if (!accountMapping.AccountReference.IsLoaded)
                        accountMapping.AccountReference.Load();

                    if (onlyActiveAccount && accountMapping.Account.State != (int)SoeEntityState.Active)
                        return null;
                }

                if (loadAccountDim && (!accountMapping.AccountDimReference.IsLoaded))
                {
                    accountMapping.AccountDimReference.Load();
                }

                if (loadAccountInternal && (!accountMapping.AccountInternalReference.IsLoaded))
                {
                    accountMapping.AccountInternalReference.Load();
                }
            }

            return accountMapping;
        }

        /// <summary>
        /// Add a given AccountMapping
        /// </summary>
        /// <param name="entities">The AccountMapping to add</param>
        /// <param name="accountMapping">The AccountMapping to add</param>
        /// <param name="accountId">The Account to reference</param>
        /// <param name="accountDimId">The AccountDim to reference</param>
        /// <param name="defaultAccountId">The default Account</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult AddAccountMapping(CompEntities entities, AccountMapping accountMapping, int accountId, int accountDimId, int defaultAccountId, int actorCompanyId)
        {
            if (accountMapping == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountMapping");

            accountMapping.AccountDim = GetAccountDim(entities, accountDimId, actorCompanyId);
            if (accountMapping.AccountDim == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

            accountMapping.Account = GetAccount(entities, actorCompanyId, accountId, onlyActive: false);
            if (accountMapping.Account == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "Account");

            accountMapping.AccountInternal = GetAccountInternal(entities, defaultAccountId, actorCompanyId);

            return AddEntityItem(entities, accountMapping, "AccountMapping");

        }

        /// <summary>
        /// Add a given AccountMapping
        /// </summary>
        /// <param name="accountMapping">The AccountMapping to add</param>
        /// <param name="accountId">The Account to reference</param>
        /// <param name="accountDimId">The AccountDim to reference</param>
        /// <param name="defaultAccountId">The default Account</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult AddAccountMapping(AccountMapping accountMapping, int accountId, int accountDimId, int defaultAccountId, int actorCompanyId)
        {
            if (accountMapping == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountMapping");

            using (CompEntities entities = new CompEntities())
            {
                accountMapping.AccountDim = GetAccountDim(entities, accountDimId, actorCompanyId);
                if (accountMapping.AccountDim == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                accountMapping.Account = GetAccount(entities, actorCompanyId, accountId, onlyActive: false);
                if (accountMapping.Account == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "Account");

                accountMapping.AccountInternal = GetAccountInternal(entities, defaultAccountId, actorCompanyId);

                return AddEntityItem(entities, accountMapping, "AccountMapping");
            }
        }

        /// <summary>
        /// Updates a given AccountMapping
        /// </summary>
        /// <param name="entities">The AccountMapping</param>
        /// <param name="accountMapping">The AccountMapping</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult UpdateAccountMapping(CompEntities entities, AccountMapping accountMapping, int actorCompanyId, int mandatoryLevel, int defaultAccountId)
        {
            accountMapping = GetAccountMapping(entities, accountMapping.Account.AccountId, accountMapping.AccountDim.AccountDimId, actorCompanyId, false, true, false, true);
            if (accountMapping == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountMapping");

            accountMapping.AccountInternal = GetAccountInternal(entities, defaultAccountId, actorCompanyId);
            accountMapping.MandatoryLevel = mandatoryLevel;

            return SaveChanges(entities);
        }

        /// <summary>
        /// Updates a given AccountMapping
        /// </summary>
        /// <param name="accountMapping">The AccountMapping</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult UpdateAccountMapping(AccountMapping accountMapping, int actorCompanyId, int? mandatoryLevel, int defaultAccountId)
        {
            using (CompEntities entities = new CompEntities())
            {
                accountMapping = GetAccountMapping(entities, accountMapping.Account.AccountId, accountMapping.AccountDim.AccountDimId, actorCompanyId, false, true, false, true);
                if (accountMapping == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountMapping");

                accountMapping.AccountInternal = GetAccountInternal(entities, defaultAccountId, actorCompanyId);
                accountMapping.MandatoryLevel = mandatoryLevel;

                return SaveChanges(entities);
            }
        }

        #endregion

        #region AccountDim

        public List<AccountDim> GetAccountDimInternalsByCompany(int actorCompanyId, bool? active = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDimInternalsByCompany(entities, actorCompanyId, active);
        }

        public List<AccountDim> GetAccountDimInternalsByCompany(CompEntities entities, int actorCompanyId, bool? active = true)
        {
            return GetAccountDimsByCompany(entities, actorCompanyId, false, true, active);
        }

        public List<AccountDim> GetAccountDimsByCompany(bool onlyStandard = false, bool onlyInternal = false, bool? active = true, bool loadAccounts = false, bool loadInternalAccounts = false)
        {
            return GetAccountDimsByCompany(base.ActorCompanyId, onlyStandard, onlyInternal, active, loadAccounts, loadInternalAccounts);
        }

        public List<AccountDim> GetAccountDimsByCompany(int actorCompanyId, bool onlyStandard = false, bool onlyInternal = false, bool? active = true, bool loadAccounts = false, bool loadInternalAccounts = false, bool loadParentOrCalculateLevels = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDimsByCompany(entities, actorCompanyId, onlyStandard, onlyInternal, active, loadAccounts, loadInternalAccounts, loadParentOrCalculateLevels);
        }

        public List<AccountDim> GetAccountDimsByCompany(CompEntities entities, int actorCompanyId, bool onlyStandard = false, bool onlyInternal = false, bool? active = true, bool loadAccounts = false, bool loadInternalAccounts = false, bool loadParentOrCalculateLevels = false)
        {
            IQueryable<AccountDim> query = (from ad in entities.AccountDim
                                            where ad.ActorCompanyId == actorCompanyId &&
                                            (!onlyStandard || ad.AccountDimNr == Constants.ACCOUNTDIM_STANDARD) &&
                                            (!onlyInternal || ad.AccountDimNr != Constants.ACCOUNTDIM_STANDARD) &&
                                            ad.State != (int)SoeEntityState.Deleted
                                            select ad);

            if (loadAccounts || loadInternalAccounts)
            {
                query = query.Include("Account.AccountStd");
                if (loadInternalAccounts)
                {
                    query = query.Include("Account.AccountInternal");
                    query = query.Include("Account.AccountMapping.AccountDim");
                    query = query.Include("Account.AccountMapping.AccountInternal.Account");
                }
            }
            if (loadParentOrCalculateLevels)
                query = query.Include("Parent");

            List<AccountDim> accountDims = null;
            if (active == true)
                accountDims = query.Where(a => a.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                accountDims = query.Where(a => a.State == (int)SoeEntityState.Inactive).ToList();
            else
                accountDims = query.ToList();

            if (loadParentOrCalculateLevels)
            {
                accountDims.CalculateLevels();
                return accountDims.OrderBy(a => a.Level).ThenBy(a => a.AccountDimNr).ToList();
            }

            return accountDims.OrderBy(a => a.AccountDimNr).ToList();
        }

        public List<AccountDim> GetAccountDims(int actorCompanyId, bool onlyStandard, bool onlyInternal, bool? loadInactiveDims = null, int? accountDimId = null)
        {
            List<AccountDim> accountDims = GetAccountDimsByCompany(actorCompanyId, onlyStandard, onlyInternal, loadInactiveDims);

            if (accountDimId.HasValue)
                accountDims = accountDims.Where(x => x.AccountDimId == accountDimId.Value).ToList();

            return accountDims;
        }

        public Dictionary<int, string> GetAccountDimsByCompanyDict(int actorCompanyId, bool addEmptyRow, bool onlyStandard = false, bool onlyInternal = false)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<AccountDim> accountDims = GetAccountDimsByCompany(actorCompanyId, onlyStandard, onlyInternal);
            foreach (AccountDim accountDim in accountDims.OrderBy(i => i.AccountDimNr))
            {
                if (accountDim.AccountDimNr == 1)
                    dict.Add(accountDim.AccountDimId, GetText(1258, "Konto"));
                else
                    dict.Add(accountDim.AccountDimId, accountDim.Name);
            }

            return dict;
        }

        public AccountDimIdsDTO GetAccountDimIds(int actorCompanyId)
        {
            List<AccountDim> accountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId, onlyInternal: true);

            var dimIds = new AccountDimIdsDTO();

            int accountDimNr = 2;
            foreach (var dimId in accountDims.OrderBy(d => d.AccountDimNr).Select(x => x.AccountDimId))
            {
                if (accountDimNr == 2)
                    dimIds.AccountDimId2 = dimId;

                if (accountDimNr == 3)
                    dimIds.AccountDimId3 = dimId;

                if (accountDimNr == 4)
                    dimIds.AccountDimId4 = dimId;

                if (accountDimNr == 5)
                    dimIds.AccountDimId5 = dimId;

                if (accountDimNr == 6)
                    dimIds.AccountDimId6 = dimId;

                accountDimNr++;
            }

            return dimIds;
        }

        public List<AccountDimSmallDTO> GetAccountDimsForPlanning(int actorCompanyId, int userId, bool loadAccounts = true, bool onlyDefaultAccounts = true, bool includeParentAccounts = false, bool useEmployeeAccountIfNoAttestRole = false, bool includeAbstractAccounts = false, bool isMobile = false, TimeSchedulePlanningDisplayMode displayMode = TimeSchedulePlanningDisplayMode.Admin, bool filterOnHierarchyHideOnSchedule = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            List<AccountDim> accountDims = (from ad in entities.AccountDim.Include("Parent").Include("Account.AccountStd")
                                            where ad.ActorCompanyId == actorCompanyId &&
                                                  ad.AccountDimNr != Constants.ACCOUNTDIM_STANDARD &&
                                                  // ad.UseInSchedulePlanning == true && is needed for calculatelevels
                                                  ad.State == (int)SoeEntityState.Active
                                            select ad).ToList();

            accountDims.CalculateLevels();
            accountDims = accountDims.Where(x => x.UseInSchedulePlanning).ToList();
            var dtos = accountDims.OrderBy(a => a.Level).ThenBy(a => a.AccountDimNr).ToSmallDTOs(true, false).ToList();

            if (loadAccounts)
                FilterAccountsOnAccountDims(dtos, actorCompanyId, userId, onlyDefaultAccounts: onlyDefaultAccounts, includeParentAccounts, useEmployeeAccountIfNoAttestRole, includeAbstractAccounts, isMobile, displayMode == TimeSchedulePlanningDisplayMode.User, filterOnHierarchyHideOnSchedule: filterOnHierarchyHideOnSchedule);

            return dtos;
        }

        public void FilterAccountsOnAccountDims(List<AccountDimSmallDTO> accountDims, int actorCompanyId, int userId, bool ignoreHierarchyOnly, bool onlyDefaultAccounts = true, bool includeParentAccounts = false, bool useEmployeeAccountIfNoAttestRole = false, bool includeAbstractAccounts = false, bool isMobile = false, bool ignoreAttestRoles = false, bool filterOnHierarchyHideOnSchedule = false, bool includeOrphanAccounts = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, actorCompanyId))
            {
                FilterAccountsOnAccountDims(accountDims, actorCompanyId, userId,
                    onlyDefaultAccounts: onlyDefaultAccounts,
                    includeParentAccounts: includeParentAccounts,
                    useEmployeeAccountIfNoAttestRole: useEmployeeAccountIfNoAttestRole,
                    includeAbstractAccounts: includeAbstractAccounts,
                    isMobile: isMobile,
                    ignoreAttestRoles: ignoreAttestRoles,
                    filterOnHierarchyHideOnSchedule: filterOnHierarchyHideOnSchedule,
                    includeOrphanAccounts: includeOrphanAccounts);


            }

            if (ignoreHierarchyOnly)
                FilterAccountsOnHierarchyOnly(accountDims);
        }
        public void FilterAccountsOnAccountDims(List<AccountDimSmallDTO> accountDims, int actorCompanyId, int userId, bool onlyDefaultAccounts = true, bool includeParentAccounts = false, bool useEmployeeAccountIfNoAttestRole = false, bool includeAbstractAccounts = false, bool isMobile = false, bool ignoreAttestRoles = false, bool filterOnHierarchyHideOnSchedule = false, bool includeOrphanAccounts = false)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            FilterAccountsOnAccountDims(entitiesReadOnly, accountDims, actorCompanyId, userId, onlyDefaultAccounts, includeParentAccounts, useEmployeeAccountIfNoAttestRole, includeAbstractAccounts, isMobile, ignoreAttestRoles, filterOnHierarchyHideOnSchedule, includeOrphanAccounts);
        }
        public void FilterAccountsOnAccountDims(CompEntities entities, List<AccountDimSmallDTO> accountDims, int actorCompanyId, int userId, bool onlyDefaultAccounts = true, bool includeParentAccounts = false, bool useEmployeeAccountIfNoAttestRole = false, bool includeAbstractAccounts = false, bool isMobile = false, bool ignoreAttestRoles = false, bool filterOnHierarchyHideOnSchedule = false, bool includeOrphanAccounts = false)
        {
            if (!base.UseAccountHierarchyOnCompanyFromCache(entities, actorCompanyId))
                return;

            int defaultEmployeeAccountDimId = SettingManager.GetCompanyIntSetting(entities, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            var defaultEmployeeAccountDim = accountDims.FirstOrDefault(ad => ad.AccountDimId == defaultEmployeeAccountDimId);
            if (defaultEmployeeAccountDim != null)
                accountDims.Where(ad => ad.AccountDimNr != Constants.ACCOUNTDIM_STANDARD && ad.Level < defaultEmployeeAccountDim.Level).ToList().ForEach(ad => ad.IsAboveCompanyStdSetting = true);

            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.OnlyDefaultAccounts, onlyDefaultAccounts);
            input.AddParamValue(AccountHierarchyParamType.UseEmployeeAccountIfNoAttestRole, useEmployeeAccountIfNoAttestRole);

            AccountRepository accountRepository = GetAccountHierarchyRepositoryByUser(entities, actorCompanyId, userId, input: input, ignoreAttestRoles: ignoreAttestRoles);
            if (accountRepository != null)
            {
                List<AccountDTO> accounts = includeAbstractAccounts ? accountRepository.GetAccountsWithAbstract(includeVirtualParented: true) : accountRepository.GetAccounts(includeVirtualParented: true);
                List<int> ids = accounts.Select(s => s.AccountId).ToList();
                for (int i = 0; i < accountDims.Count; i++)
                {
                    AccountDimSmallDTO accountDim = accountDims[i];

                    // Always return all account for standard dim
                    if (accountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD)
                        continue;

                    List<int> parentAccountIds = new List<int>();
                    if (includeParentAccounts)
                    {
                        AccountDimSmallDTO childDim = accountDims.FirstOrDefault(d => d.Level == accountDim.Level + 1);
                        if (childDim != null)
                            parentAccountIds = childDim.Accounts.Where(a => a.ParentAccountId.HasValue).Select(a => a.ParentAccountId.Value).Distinct().ToList();
                    }

                    List<AccountDTO> accountsForDim = accountDim.Accounts != null ? accountDim.Accounts.Where(w => ids.Contains(w.AccountId) || parentAccountIds.Contains(w.AccountId) || (includeOrphanAccounts && !w.ParentAccountId.HasValue)).ToList() : new List<AccountDTO>();
                    if (isMobile)
                    {
                        //get AccountDto instances from account since GetAccountHierarchyRepositoryByUser has set some properties (i.e isAbstract) that will be used in MobileManager
                        // Also schedule planning is setting isMobile now, to get virtual parent account
                        accountDim.Accounts = accounts.Where(a => accountsForDim.Select(x => x.AccountId).Contains(a.AccountId)).ToList();
                    }
                    else
                    {
                        accountDim.Accounts = accountsForDim;
                    }

                    if (filterOnHierarchyHideOnSchedule)
                    {
                        int nbrOfAccountsBeforeFilter = accountDim.Accounts.Count;
                        accountDim.Accounts = accountDim.Accounts.Where(a => !a.HierarchyNotOnSchedule).ToList();

                        if (nbrOfAccountsBeforeFilter > 0 && accountDim.Accounts.Count == 0)
                        {
                            // If we have removed all accounts on this dim, we need to reload accounts from cache to be able to filter on HierarchyNotOnSchedule again
                            var accountsOnDim = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId)).Where(w => w.AccountDimId == accountDim.AccountDimId).ToList();
                            accountDim.Accounts = accountsOnDim.Where(w => !w.HierarchyNotOnSchedule).ToList();
                        }
                    }

                }
            }
        }

        public void FilterAccountsOnAccountDims(List<AccountDimDTO> accountDims, int actorCompanyId, int userId, bool ignoreHierarchyOnly)
        {
            FilterAccountsOnAccountDims(accountDims, actorCompanyId, userId);
            if (ignoreHierarchyOnly)
                FilterAccountsOnHierarchyOnly(accountDims);
        }

        public void FilterAccountsOnAccountDims(List<AccountDimDTO> accountDims, int actorCompanyId, int userId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            if (!base.UseAccountHierarchyOnCompanyFromCache(entitiesReadOnly, actorCompanyId))
                return;

            var repository = GetAccountHierarchyRepositoryByUser(entitiesReadOnly, actorCompanyId, userId);
            if (repository != null)
            {
                var accounts = repository.GetAccounts(includeVirtualParented: true);
                List<int> ids = accounts.Select(s => s.AccountId).ToList();
                for (int i = 0; i < accountDims.Count; i++)
                {
                    var accountDim = accountDims[i];
                    accountDim.Accounts = accountDim.Accounts != null ? accountDim.Accounts.Where(w => ids.Contains(w.AccountId)).ToList() : new List<AccountDTO>();
                }
            }
        }

        public void FilterAccountsOnHierarchyOnly(List<AccountDimSmallDTO> accountDimDTOs)
        {
            foreach (var accountDimDto in accountDimDTOs)
            {
                if (accountDimDto.Accounts != null)
                    accountDimDto.Accounts = accountDimDto.Accounts.Where(w => !w.HierarchyOnly).ToList();
            }
        }

        public void FilterAccountsOnHierarchyOnly(List<AccountDimDTO> accountDimDTOs)
        {
            foreach (var accountDimDto in accountDimDTOs)
            {
                accountDimDto.Accounts = accountDimDto.Accounts.Where(w => !w.HierarchyOnly).ToList();
            }
        }

        public AccountDim GetDefaultEmployeeAccountDim(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetDefaultEmployeeAccountDim(entities, actorCompanyId);
        }

        public AccountDim GetDefaultEmployeeAccountDim(CompEntities entities, int actorCompanyId)
        {
            int accountDimId = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);


            AccountDim accountDim = (from ad in entities.AccountDim.Include("Account.AccountStd")
                                     where ad.ActorCompanyId == actorCompanyId &&
                                           ad.AccountDimId == accountDimId &&
                                           ad.State == (int)SoeEntityState.Active
                                     select ad).FirstOrDefault();

            return accountDim;
        }

        public AccountDimSmallDTO GetDefaultEmployeeAccountDimAndSelectableAccounts(int actorCompanyId, int userId, int employeeId, DateTime date)
        {
            int accountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);
            AccountDimSmallDTO accountDim = GetAccountDim(accountDimId, actorCompanyId).ToSmallDTO();
            if (accountDim != null)
            {
                var accounts = GetSelectableEmployeeShiftAccounts(userId, actorCompanyId, employeeId, date, includeAbstract: true);
                accountDim.Accounts = accounts;
            }
            return accountDim;
        }

        public Dictionary<int, int> GetAccountDimsMapping(int actorCompanyId)
        {
            Dictionary<int, int> accountDimsMapping = new Dictionary<int, int>();
            int index = 2; //Skip AccountDim std

            List<AccountDim> accountDimInternals = GetAccountDimsByCompany(actorCompanyId, false, true, true);
            foreach (AccountDim accountDim in accountDimInternals.OrderBy(i => i.AccountDimNr).ThenBy(i => i.AccountDimId))
            {
                if (accountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD || accountDimsMapping.ContainsKey(accountDim.AccountDimNr))
                    continue;
                if (index > Constants.ACCOUNTDIM_NROFDIMENSIONS)
                    break;

                accountDimsMapping.Add(accountDim.AccountDimNr, index);
                index++;
            }

            return accountDimsMapping;
        }

        public AccountDim GetAccountDim(int accountDimId, int actorCompanyId, bool includeInactive = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDim(entities, accountDimId, actorCompanyId, includeInactive);
        }

        public AccountDim GetAccountDim(CompEntities entities, int accountDimId, int actorCompanyId, bool includeInactive = false)
        {
            IQueryable<AccountDim> query = (from ad in entities.AccountDim.Include("Account")
                                            where ad.AccountDimId == accountDimId &&
                                            ad.ActorCompanyId == actorCompanyId
                                            select ad);

            AccountDim accountDim = null;
            if (includeInactive)
                accountDim = query.FirstOrDefault(a => a.State != (int)SoeEntityState.Deleted);
            else
                accountDim = query.FirstOrDefault(a => a.State == (int)SoeEntityState.Active);

            return accountDim;
        }

        public AccountDim GetAccountDimByNr(int accountDimNr, int actorCompanyId, bool includeAccounts = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDimByNr(entities, accountDimNr, actorCompanyId, includeAccounts);
        }

        public AccountDim GetAccountDimByNr(CompEntities entities, int accountDimNr, int actorCompanyId, bool includeAccounts = false, bool includeParent = false, bool includeInactive = false)
        {
            IQueryable<AccountDim> query = entities.AccountDim;

            if (includeAccounts)
                query = query.Include("Account");

            if (includeParent)
                query = query.Include("Parent");

            query = (from ad in query
                     where ad.AccountDimNr == accountDimNr &&
                     ad.ActorCompanyId == actorCompanyId
                     select ad);

            if (includeInactive)
                query = query.Where(ad => ad.State == (int)SoeEntityState.Active || ad.State == (int)SoeEntityState.Inactive);
            else
                query = query.Where(ad => ad.State == (int)SoeEntityState.Active);

            return query.FirstOrDefault();
        }

        public AccountDim GetAccountDimBySieNr(int sieDimNr, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDimBySieNr(entities, sieDimNr, actorCompanyId);
        }

        public AccountDim GetAccountDimBySieNr(CompEntities entities, int sieDimNr, int actorCompanyId)
        {
            return (from ad in entities.AccountDim
                    where ad.SysSieDimNr == sieDimNr &&
                    ad.ActorCompanyId == actorCompanyId &&
                    ad.State == (int)SoeEntityState.Active
                    select ad).FirstOrDefault();
        }

        public AccountDim GetAccountDimStd(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDimStd(entities, actorCompanyId);
        }

        public AccountDim GetAccountDimStd(CompEntities entities, int actorCompanyId, bool loadAccounts = false)
        {
            if (loadAccounts)
            {
                return (from ad in entities.AccountDim
                            .Include("Account")
                            .Include("Account.AccountStd")
                        where ad.AccountDimNr == Constants.ACCOUNTDIM_STANDARD &&
                        ad.ActorCompanyId == actorCompanyId
                        select ad).FirstOrDefault();
            }
            else
            {
                return (from ad in entities.AccountDim
                        where ad.AccountDimNr == Constants.ACCOUNTDIM_STANDARD &&
                        ad.ActorCompanyId == actorCompanyId
                        select ad).FirstOrDefault();
            }
        }

        public AccountDim GetPrevNextAccountDim(int accountDimId, int actorCompanyId, SoeFormMode mode)
        {
            AccountDim accountDim = null;

            using var entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.AccountDim.NoTracking();
            if (mode == SoeFormMode.Next)
            {
                accountDim = (from ad in entitiesReadOnly.AccountDim
                              where (ad.AccountDimId > accountDimId) &&
                              (ad.ActorCompanyId == actorCompanyId) &&
                              (ad.State == (int)SoeEntityState.Active)
                              orderby ad.AccountDimId ascending
                              select ad).FirstOrDefault();
            }
            else if (mode == SoeFormMode.Prev)
            {
                accountDim = (from ad in entitiesReadOnly.AccountDim
                              where ad.AccountDimId < accountDimId &&
                              ad.ActorCompanyId == actorCompanyId &&
                              ad.State == (int)SoeEntityState.Active
                              orderby ad.AccountDimId descending
                              select ad).FirstOrDefault();
            }

            return accountDim;
        }

        public AccountDim GetAccountDimFromSieDimNr(int sieDimNr, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDimFromSieDimNr(entities, sieDimNr, actorCompanyId);
        }

        public AccountDim GetAccountDimFromSieDimNr(CompEntities entities, int sieDimNr, int actorCompanyId)
        {
            return (from ad in entities.AccountDim
                    where ad.SysSieDimNr == sieDimNr &&
                    ad.ActorCompanyId == actorCompanyId &&
                    ad.State == (int)SoeEntityState.Active
                    select ad).FirstOrDefault();
        }

        public AccountDimDTO GetAccountDimDTOFromSieDimNr(int sieDimNr, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetAccountDimDTOFromSieDimNr(entities, sieDimNr, actorCompanyId);
        }

        public AccountDimDTO GetAccountDimDTOFromSieDimNr(CompEntities entities, int sieDimNr, int actorCompanyId)
        {
            string cacheKey = $"AccountDimFromSieDimNr#sieDimNr{sieDimNr}#actorCompanyId{actorCompanyId}";
            AccountDimDTO accountDimDTO = BusinessMemoryCache<AccountDimDTO>.Get(cacheKey);

            if (accountDimDTO == null)
            {
                accountDimDTO = GetAccountDimFromSieDimNr(entities, sieDimNr, actorCompanyId).ToDTO();

                if (accountDimDTO == null)
                {
                    accountDimDTO = new AccountDimDTO { AccountDimId = 0 };
                }

                BusinessMemoryCache<AccountDimDTO>.Set(cacheKey, accountDimDTO, 120);
            }

            if (accountDimDTO.AccountDimId == 0)
            {
                accountDimDTO = null;
            }

            return accountDimDTO;
        }

        public AccountDim GetProjectAccountDim(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetProjectAccountDim(entities, actorCompanyId);
        }

        public AccountDim GetProjectAccountDim(CompEntities entities, int actorCompanyId)
        {
            return (from ad in entities.AccountDim
                    where ad.LinkedToProject &&
                    ad.ActorCompanyId == actorCompanyId &&
                    ad.State == (int)SoeEntityState.Active
                    select ad).FirstOrDefault();
        }

        public AccountDimDTO GetShiftTypeAccountDimDTO(int actorCompanyId, bool loadAccounts = false, bool useCache = true)
        {
            if (useCache == false)
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                return GetShiftTypeAccountDim(entities, actorCompanyId, loadAccounts)?.ToDTO(loadAccounts, loadAccounts);
            }
            else
            {
                using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
                return base.GetShiftTypeAccountDimFromCache(entitiesReadOnly, actorCompanyId, loadAccounts);
            }
        }

        public AccountDim GetShiftTypeAccountDim(int actorCompanyId, bool loadAccounts = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return GetShiftTypeAccountDim(entities, actorCompanyId, loadAccounts);
        }


        public AccountDim GetShiftTypeAccountDim(CompEntities entities, int actorCompanyId, bool loadAccounts = false)
        {
            AccountDim accountDim = (from ad in entities.AccountDim
                                     .Include("Parent")
                                     where ad.LinkedToShiftType &&
                                     ad.ActorCompanyId == actorCompanyId &&
                                     ad.State == (int)SoeEntityState.Active
                                     select ad).FirstOrDefault();

            if (accountDim != null && loadAccounts)
                accountDim.Account.Load();

            return accountDim;
        }

        public List<GenericType> GetAccountDimChars()
        {
            List<GenericType> chars = new List<GenericType>();
            for (int i = 1; i <= 9; i++)
            {
                chars.Add(new GenericType()
                {
                    Id = i,
                    Name = i.ToString(),
                });
            }

            return chars;
        }

        public int GetAccountDimStdId(int actorCompanyId)
        {
            AccountDim accountDim = GetAccountDimStd(actorCompanyId);
            return accountDim != null ? accountDim.AccountDimId : 0;
        }

        public ActionResult ValidateAccountDimNr(int accountDimNr, int accountDimId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return ValidateAccountDimNr(entities, accountDimNr, accountDimId, actorCompanyId);
        }

        public ActionResult ValidateAccountDimNr(CompEntities entity, int accountDimNr, int accountDimId, int actorCompanyId)
        {
            ActionResult result = new ActionResult();

            var existingAccountDim = GetAccountDimByNr(entity, accountDimNr, actorCompanyId, includeInactive: true);

            if (existingAccountDim != null && existingAccountDim.AccountDimId != accountDimId)
            {
                if (existingAccountDim.State == (int)SoeEntityState.Inactive)
                    return new ActionResult((int)ActionResultSave.AccountExist, GetText(7472, "Finns redan ett inaktivt konto med samma nummer"));
                return new ActionResult((int)ActionResultSave.AccountExist, GetText(7471, "Kontonumret finns redan"));
            }

            return result;
        }

        public bool AccountDimExist(int accountDimNr, int actorCompanyId)
        {
            AccountDim accountDim = GetAccountDimByNr(accountDimNr, actorCompanyId);
            return accountDim != null;
        }

        public bool AccountDimExist(CompEntities entity, int accountDimNr, int actorCompanyId, bool includeInactive = false)
        {
            AccountDim accountDim = GetAccountDimByNr(entity, accountDimNr, actorCompanyId, includeInactive: includeInactive);
            return accountDim != null;
        }

        public bool AccountDimSieExist(int sieDimNr, int actorCompanyId)
        {
            AccountDim accountDim = GetAccountDimBySieNr(sieDimNr, actorCompanyId);
            return accountDim != null;
        }

        public bool AccountDimSieExist(CompEntities entity, int sieDimNr, int actorCompanyId)
        {
            AccountDim accountDim = GetAccountDimBySieNr(entity, sieDimNr, actorCompanyId);
            return accountDim != null;
        }

        public bool IsAccountValidInAccountDim(int? accountDimMinChar, int? accountDimMaxChar, string accountNr)
        {
            bool valid = true;
            int accountNrLength = accountNr.Length;

            if ((accountDimMinChar != null) && (Convert.ToInt32(accountDimMinChar, CultureInfo.InvariantCulture) > 0) && (Convert.ToInt32(accountDimMinChar, CultureInfo.InvariantCulture) > accountNrLength))
                valid = false;
            if ((accountDimMaxChar != null) && (Convert.ToInt32(accountDimMaxChar, CultureInfo.InvariantCulture) > 0) && (Convert.ToInt32(accountDimMaxChar, CultureInfo.InvariantCulture) < accountNrLength))
                valid = false;

            return valid;
        }

        private bool AccountDimHasAccounts(AccountDim accountDim)
        {
            if (accountDim == null)
                return false;

            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from a in entitiesReadOnly.Account
                    where a.AccountDimId == accountDim.AccountDimId &&
                    a.State == (int)SoeEntityState.Active
                    select a).Any();
        }

        private bool AccountDimHasAccounts(int accountDimId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return (from a in entitiesReadOnly.Account
                    where a.AccountDimId == accountDimId &&
                    a.State != (int)SoeEntityState.Deleted
                    select a).Any();
        }

        /// <summary>
        /// Add a given AccountDim
        /// </summary>
        /// <param name="accountDim">The AccountDim</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult AddAccountDim(AccountDim accountDim, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountDim.NoTracking();
            return AddAccountDim(entities, accountDim, actorCompanyId);
        }

        public ActionResult AddAccountDim(CompEntities entities, AccountDim accountDim, int actorCompanyId)
        {
            if (accountDim == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountDim");

            // Only one AccountDim can be linked to Project
            if (accountDim.LinkedToProject)
            {
                AccountDim projDim = GetProjectAccountDim(entities, actorCompanyId);
                if (projDim != null)
                    return new ActionResult((int)ActionResultSave.ProjectAccountDimExists);
            }

            accountDim.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (accountDim.Company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            return AddEntityItem(entities, accountDim, "AccountDim", addToContext: false);
        }

        /// <summary>
        /// Updates a given AccountDim
        /// </summary>
        /// <param name="accountDim">The AccountDim entity with update values</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult UpdateAccountDim(AccountDim accountDim, int actorCompanyId)
        {
            if (accountDim == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountDim");

            // Only one AccountDim can be linked to Project
            if (accountDim.LinkedToProject)
            {
                AccountDim projDim = GetProjectAccountDim(actorCompanyId);
                if (projDim != null && projDim.AccountDimId != accountDim.AccountDimId)
                    return new ActionResult((int)ActionResultSave.ProjectAccountDimExists);
            }

            using (CompEntities entities = new CompEntities())
            {
                AccountDim originalAccountDim = GetAccountDim(entities, accountDim.AccountDimId, actorCompanyId);
                if (originalAccountDim == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                return this.UpdateEntityItem(entities, originalAccountDim, accountDim, "AccountDim");
            }
        }

        /// <summary>
        /// Sets a AccountDim to Deleted
        /// </summary>
        /// <param name="accountDim">AccountDim to delete</param>
        /// <returns>ActionResult</returns>
        public ActionResult DeleteAccountDim(AccountDim accountDim, int actorCompanyId)
        {
            if (accountDim == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "AccountDim");

            //Check relation dependencies
            if (AccountDimHasAccounts(accountDim))
                return new ActionResult((int)ActionResultDelete.AccountDimHasAccounts);

            using (CompEntities entities = new CompEntities())
            {
                AccountDim originalAccountDim = GetAccountDim(entities, accountDim.AccountDimId, actorCompanyId);
                if (originalAccountDim == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountDim");

                return ChangeEntityState(entities, originalAccountDim, SoeEntityState.Deleted, true);
            }
        }

        public ActionResult DeleteAccountDims(IEnumerable<int> accountDimIds)
        {
            ActionResult actionResult = new ActionResult();
            int delete_error = 0;

            using (CompEntities entities = new CompEntities())
            {
                foreach (int accountDimId in accountDimIds)
                {
                    // Loop each one to delete

                    if (AccountDimHasAccounts(accountDimId))
                    {
                        actionResult.ErrorNumber = (int)ActionResultDelete.AccountDimHasAccounts;
                        delete_error++;
                    }
                    else
                    {
                        AccountDim originalAccountDim = GetAccountDim(
                            entities, accountDimId, base.ActorCompanyId, true);
                        if (originalAccountDim == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountDim");

                        actionResult = ChangeEntityState(entities, originalAccountDim, SoeEntityState.Deleted, true);
                    }
                }
            }

            if (delete_error > 0)
            {
                actionResult.ObjectsAffected = accountDimIds.Count() - delete_error;
                actionResult.IntegerValue = accountDimIds.Count() - delete_error;
                actionResult.IntegerValue2 = delete_error;  // Return errors

                actionResult.InfoMessage += actionResult.IntegerValue.ToString() + " ";
                if (actionResult.IntegerValue == 1)
                    actionResult.InfoMessage += GetText(1111, 1003, "rad") + " ";
                else
                    actionResult.InfoMessage += GetText(1112, 1003, "rader") + " ";

                actionResult.InfoMessage += GetText(1113, 1003, "togs bort") + ".<br/>";

                actionResult.InfoMessage += actionResult.IntegerValue2.ToString() + " ";
                if (actionResult.IntegerValue2 == 1)
                    actionResult.InfoMessage += GetText(1111, 1003, "rad") + " ";
                else
                    actionResult.InfoMessage += GetText(1112, 1003, "rader") + " ";

                actionResult.InfoMessage += GetText(1114, 1003, "kunde inte tas bort") + ".";
            }
            else
            {
                actionResult.IntegerValue = accountDimIds.Count();

                actionResult.InfoMessage += actionResult.IntegerValue.ToString() + " ";
                if (actionResult.IntegerValue == 1)
                    actionResult.InfoMessage += GetText(1111, 1003, "rad") + " ";
                else
                    actionResult.InfoMessage += GetText(1112, 1003, "rader") + " ";

                actionResult.InfoMessage += GetText(1113, 1003, "togs bort") + ".";
            }

            return actionResult;
        }

        public ActionResult SaveAccountDim(AccountDimDTO inputAccountDim, bool reset, int roleId)
        {
            ActionResult result = new ActionResult();

            #region Init

            int accountDimId = inputAccountDim.AccountDimId;

            #endregion

            #region Validation

            // Only one AccountDim can be linked to Project
            if (inputAccountDim.LinkedToProject)
            {
                AccountDim projDim = GetProjectAccountDim(base.ActorCompanyId);
                if (projDim != null && projDim.AccountDimId != inputAccountDim.AccountDimId)
                    return new ActionResult((int)ActionResultSave.ProjectAccountDimExists, GetText(3356, "Konteringsnivå länkad till projekt finns redan"));
            }

            // Only one AccountDim can be linked to ShiftType
            if (inputAccountDim.LinkedToShiftType)
            {
                AccountDim shiftTypeDim = GetShiftTypeAccountDim(base.ActorCompanyId);
                if (shiftTypeDim != null && shiftTypeDim.AccountDimId != inputAccountDim.AccountDimId)
                    return new ActionResult((int)ActionResultSave.ShiftTypeAccountDimExists, GetText(10124, "Konteringsnivå länkad till passtyp finns redan"));
            }

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        #region Prereq

                        AccountDim accountDim = accountDimId > 0 ? GetAccountDim(entities, accountDimId, base.ActorCompanyId, true) : null;

                        if (accountDim == null)
                        {
                            // Do some validation first
                            var validationResult = ValidateAccountDimNr(entities, inputAccountDim.AccountDimNr, inputAccountDim.AccountDimId, base.ActorCompanyId);
                            if (!validationResult.Success)
                                return validationResult;

                            if (inputAccountDim.SysSieDimNr.HasValue && inputAccountDim.SysSieDimNr.Value > 0 && AccountDimSieExist(entities, inputAccountDim.SysSieDimNr.Value, base.ActorCompanyId))
                            {
                                result.Success = false;
                                result.ErrorMessage = GetText(1145, "Konteringsnivå med angiven SIE dimension finns redan");
                                return result;
                            }

                            #region Add

                            accountDim = new AccountDim()
                            {
                                State = (int)inputAccountDim.State,

                                //Set FK
                                ActorCompanyId = base.ActorCompanyId,

                                //References
                                Parent = null,
                            };

                            entities.AccountDim.AddObject(accountDim);
                            SetCreatedProperties(accountDim);

                            #endregion
                        }
                        else
                        {
                            #region Update

                            // Do some validation first
                            if (accountDim.AccountDimNr != inputAccountDim.AccountDimNr)
                            {
                                var validationResult = ValidateAccountDimNr(entities, inputAccountDim.AccountDimNr, accountDim.AccountDimId, base.ActorCompanyId);
                                if (!validationResult.Success)
                                    return validationResult;
                            }

                            if (inputAccountDim.SysSieDimNr.HasValue)
                            {
                                AccountDim ad = GetAccountDimFromSieDimNr(entities, inputAccountDim.SysSieDimNr.Value, base.ActorCompanyId);
                                if (ad != null && (ad.AccountDimNr != inputAccountDim.AccountDimNr))
                                {
                                    result.Success = false;
                                    result.ErrorMessage = GetText(1145, "Konteringsnivå med angiven SIE dimension finns redan");
                                    return result;
                                }
                            }

                            if (inputAccountDim.AccountDimNr < 1)
                            {
                                result.Success = false;
                                result.ErrorMessage = GetText(2155, "Nummer för konteringsnivån måste vara större än ett");
                                return result;
                            }

                            // Can only update toState on internal account
                            if (inputAccountDim.AccountDimNr != Constants.ACCOUNTDIM_STANDARD && accountDim.State != (int)inputAccountDim.State && FeatureManager.HasRolePermission(Feature.Economy_Accounting_AccountRoles_Inactivate, Permission.Modify, roleId, base.ActorCompanyId, base.LicenseId, entities))
                                accountDim.State = (int)inputAccountDim.State;

                            SetModifiedProperties(accountDim);

                            #endregion
                        }

                        #endregion

                        #region AccountDim

                        accountDim.AccountDimNr = inputAccountDim.AccountDimNr;
                        accountDim.ShortName = inputAccountDim.ShortName;
                        accountDim.Name = inputAccountDim.Name;
                        accountDim.UseInSchedulePlanning = inputAccountDim.UseInSchedulePlanning;
                        accountDim.ExcludeinAccountingExport = inputAccountDim.ExcludeinAccountingExport;
                        accountDim.ExcludeinSalaryExport = inputAccountDim.ExcludeinSalaryReport;
                        accountDim.UseVatDeduction = inputAccountDim.UseVatDeduction;
                        accountDim.MandatoryInCustomerInvoice = inputAccountDim.MandatoryInCustomerInvoice;
                        accountDim.MandatoryInOrder = inputAccountDim.MandatoryInOrder;
                        accountDim.OnlyAllowAccountsWithParent = inputAccountDim.OnlyAllowAccountsWithParent;

                        if (inputAccountDim.ParentAccountDimId.HasValue)
                        {
                            AccountDim parentAccountDim = GetAccountDim(entities, (int)inputAccountDim.ParentAccountDimId, base.ActorCompanyId);
                            accountDim.Parent = parentAccountDim;
                        }

                        if (inputAccountDim.ParentAccountDimId.HasValue)
                        {
                            AccountDim parentAccountDim = GetAccountDim(entities, (int)inputAccountDim.ParentAccountDimId, base.ActorCompanyId);
                            accountDim.Parent = parentAccountDim;
                        }

                        if (accountDim.AccountDimNr == Constants.ACCOUNTDIM_STANDARD)
                        {
                            accountDim.SysAccountStdTypeParentId = inputAccountDim.SysAccountStdTypeParentId;
                        }
                        else
                        {
                            accountDim.MinChar = inputAccountDim.MinChar;
                            accountDim.MaxChar = inputAccountDim.MaxChar;
                            accountDim.SysSieDimNr = inputAccountDim.SysSieDimNr;
                            accountDim.LinkedToProject = inputAccountDim.LinkedToProject;
                            accountDim.LinkedToShiftType = inputAccountDim.LinkedToShiftType;
                        }

                        if (reset)
                        {
                            List<AccountInternal> accountInternals = GetAccountInternalsByDim(entities, accountDim.AccountDimId, base.ActorCompanyId);
                            foreach (AccountInternal account in accountInternals)
                            {
                                account.UseVatDeduction = false;
                                account.VatDeduction = 100;

                                SetModifiedProperties(account);
                            }
                        }

                        result = SaveChanges(entities);
                        if (!result.Success)
                            return result;

                        accountDimId = accountDim.AccountDimId;

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                finally
                {
                    if (result.Success)
                    {
                        //Set success properties
                        result.IntegerValue = accountDimId;
                    }
                    else
                        base.LogTransactionFailed(this.ToString(), this.log);

                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion

        #region AccountYear

        public List<AccountYear> GetAccountYears(int actorCompanyId, bool onlyOpenAccountYears, bool loadPeriods, bool excludeNew = false, bool loadVoucherSeries = false, bool loadVoucherSeriesType = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYear.NoTracking();
            return GetAccountYears(entities, actorCompanyId, onlyOpenAccountYears, loadPeriods, excludeNew, loadVoucherSeries, loadVoucherSeriesType);
        }

        public List<AccountYear> GetAccountYears(CompEntities entities, int actorCompanyId, bool onlyOpenAccountYears, bool loadPeriods, bool excludeNew = false, bool loadVoucherSeries = false, bool loadVoucherSeriesType = false)
        {
            IQueryable<AccountYear> query = (from ay in entities.AccountYear
                                             where ay.ActorCompanyId == actorCompanyId &&
                                             (!onlyOpenAccountYears || ay.Status == (int)TermGroup_AccountStatus.Open) &&
                                             (!excludeNew || ay.Status != (int)TermGroup_AccountStatus.New)
                                             orderby ay.From ascending
                                             select ay);
            if (loadPeriods)
            {
                query = query.Include("AccountPeriod");
            }

            if (loadVoucherSeries)
            {
                if (loadVoucherSeriesType)
                    query = query.Include("VoucherSeries.VoucherSeriesType");
                else
                    query = query.Include("VoucherSeries");
            }

            return query.ToList();
        }

        public List<AccountYear> GetAccountYears(int actorCompanyId, bool onlyOpenAccountYears, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYear.NoTracking();
            return GetAccountYears(entities, actorCompanyId, onlyOpenAccountYears, dateFrom, dateTo);
        }

        public List<AccountYear> GetAccountYears(CompEntities entities, int actorCompanyId, bool onlyOpenAccountYears, DateTime dateFrom, DateTime dateTo)
        {
            List<AccountYear> accountYears = new List<AccountYear>();

            List<AccountYear> allAcountYears = GetAccountYears(entities, actorCompanyId, onlyOpenAccountYears, false);
            foreach (AccountYear accountYear in allAcountYears)
            {
                if (CalendarUtility.IsDatesOverlapping(accountYear.From, accountYear.To, dateFrom, dateTo, true))
                    accountYears.Add(accountYear);
            }

            return accountYears;
        }

        public Dictionary<int, string> GetAccountYearsDict(int actorCompanyId, bool onlyOpen, bool excludeNew, bool includeStatusText, bool addEmptyRow)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetAccountYearsDict(entities, actorCompanyId, onlyOpen, excludeNew, includeStatusText, addEmptyRow);
        }

        public Dictionary<int, string> GetAccountYearsDict(CompEntities entities, int actorCompanyId, bool onlyOpen, bool excludeNew, bool includeStatusText, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            List<AccountYear> accountYears = GetAccountYears(entities, actorCompanyId, onlyOpen, false);
            foreach (AccountYear accountYear in accountYears.OrderBy(y => y.From))
            {
                if (excludeNew && accountYear.Status == (int)TermGroup_AccountStatus.New)
                    continue;

                string value = accountYear.From.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + " - " + accountYear.To.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
                if (includeStatusText)
                {
                    if (accountYear.Status == (int)TermGroup_AccountStatus.Locked)
                        value += " (" + GetText(1993, "Året låst") + ")";
                    else if (accountYear.Status == (int)TermGroup_AccountStatus.Closed)
                        value += " (" + GetText(1994, "Året stängt") + ")";
                }

                dict.Add(accountYear.AccountYearId, value);
            }

            return dict;
        }

        public AccountYear GetAccountYear(int accountYearId, bool loadPeriods = false, bool doVoucherCheck = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYear.NoTracking();
            return GetAccountYear(entities, accountYearId, loadPeriods, doVoucherCheck);
        }

        public AccountYear GetAccountYear(CompEntities entities, int accountYearId, bool loadPeriods = false, bool doVoucherCheck = false)
        {
            IQueryable<AccountYear> query = (from ay in entities.AccountYear
                                             where ay.AccountYearId == accountYearId
                                             select ay);
            if (loadPeriods)
                query = query.Include("AccountPeriod");

            var year = query.FirstOrDefault();

            if (loadPeriods && doVoucherCheck)
            {
                var periodIds = year.AccountPeriod.Select(s => s.AccountPeriodId).ToList();
                var periodIdsWithVoucherHeads = entities.VoucherHead.Where(w => periodIds.Contains(w.AccountPeriodId)).Select(s => s.AccountPeriodId).Distinct().ToList();

                foreach (var period in year.AccountPeriod)
                {
                    period.HasVouchers = periodIdsWithVoucherHeads.Contains(period.AccountPeriodId);
                }
            }

            return year;
        }

        public AccountYear GetAccountYear(DateTime date, int actorCompanyId, bool loadPeriods = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYear.NoTracking();
            return GetAccountYear(entities, date, actorCompanyId, loadPeriods);
        }

        public List<AccountYear> GetAccountYears(CompEntities entities, DateTime fromDate, DateTime toDate, int actorCompanyId, bool loadPeriods = false)
        {
            if (loadPeriods)
            {
                return (from ay in entities.AccountYear
                        .Include("AccountPeriod")
                        where ay.ActorCompanyId == actorCompanyId &&
                              ay.From <= toDate.Date &&
                              ay.To >= fromDate.Date
                        select ay).ToList();
            }
            else
            {
                return (from ay in entities.AccountYear
                        where ay.ActorCompanyId == actorCompanyId &&
                              ay.From <= toDate.Date &&
                              ay.To >= fromDate.Date
                        select ay).ToList();
            }
        }

        public AccountYear GetAccountYear(CompEntities entities, DateTime date, int actorCompanyId, bool loadPeriods = false)
        {
            if (loadPeriods)
            {
                return (from ay in entities.AccountYear
                            .Include("AccountPeriod")
                        where ay.ActorCompanyId == actorCompanyId &&
                        ay.From <= date.Date &&
                        ay.To >= date.Date
                        select ay).FirstOrDefault();
            }
            else
            {
                return (from ay in entities.AccountYear
                        where ay.ActorCompanyId == actorCompanyId &&
                        ay.From <= date.Date &&
                        ay.To >= date.Date
                        select ay).FirstOrDefault();
            }
        }

        public AccountYear GetCurrentAccountYear(int actorCompanyId, bool loadPeriods = false)
        {
            return GetAccountYear(DateTime.Now, actorCompanyId, loadPeriods);
        }

        public AccountYear GetSelectedAccountYear(int actorCompanyId, int userId, bool loadPeriods = false)
        {
            int accountYearId = SettingManager.GetIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, base.UserId, actorCompanyId, 0);
            return GetAccountYear(accountYearId, loadPeriods);
        }

        public AccountYear GetCurrentAccountYear(CompEntities entities, int actorCompanyId, bool loadPeriods = false)
        {
            return GetAccountYear(entities, DateTime.Now, actorCompanyId, loadPeriods);
        }

        public AccountYear GetFirstAccountYear(CompEntities entities, int actorCompanyId, bool loadPeriods = false)
        {
            if (loadPeriods)
            {
                return (from ay in entities.AccountYear
                            .Include("AccountPeriod")
                        where ay.ActorCompanyId == actorCompanyId
                        orderby ay.From ascending
                        select ay).FirstOrDefault();
            }
            else
            {
                return (from ay in entities.AccountYear
                        where ay.ActorCompanyId == actorCompanyId
                        orderby ay.From ascending
                        select ay).FirstOrDefault();
            }
        }

        public AccountYear GetPreviousAccountYear(AccountYear currentAccountYear, bool loadPeriods = false)
        {
            if (currentAccountYear == null)
                return null;

            return GetPreviousAccountYear(currentAccountYear.From, currentAccountYear.ActorCompanyId, loadPeriods);
        }

        public AccountYear GetPreviousAccountYear(DateTime date, int actorCompanyId, bool loadPeriods = false)
        {
            if (loadPeriods)
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                entities.AccountYear.NoTracking();
                return (from ay in entities.AccountYear
                                .Include("AccountPeriod")
                        where ay.From < date.Date &&
                        ay.ActorCompanyId == actorCompanyId
                        orderby ay.From descending
                        select ay).FirstOrDefault();
            }
            else
            {
                using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
                entities.AccountYear.NoTracking();
                return (from ay in entities.AccountYear
                        where ay.From < date.Date &&
                        ay.ActorCompanyId == actorCompanyId
                        orderby ay.From descending
                        select ay).FirstOrDefault();
            }
        }

        public AccountYear GetPreviousAccountYear(CompEntities entities, DateTime date, int actorCompanyId, bool loadPeriods = false)
        {
            if (loadPeriods)
            {
                return (from ay in entities.AccountYear
                            .Include("AccountPeriod")
                        where ay.From < date.Date &&
                        ay.ActorCompanyId == actorCompanyId
                        orderby ay.From descending
                        select ay).FirstOrDefault();
            }
            else
            {
                return (from ay in entities.AccountYear
                        where ay.From < date.Date &&
                        ay.ActorCompanyId == actorCompanyId
                        orderby ay.From descending
                        select ay).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get AccountYear from nr
        /// </summary>
        /// <param name="entities">The Object Context</param>
        /// <param name="currentAccountYear">The AccountYear to start from</param>
        /// <param name="accountYearNr">The AccountYear nr to find. 0 = Current, -1 = Previous and so on</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A AccountYear entity</returns>
        public AccountYear GetPreviousAccountYearFromNr(CompEntities entities, AccountYear currentAccountYear, int accountYearNr)
        {
            if (currentAccountYear != null || accountYearNr > 0)
            {
                if (accountYearNr == 0)
                    return currentAccountYear;

                var accountYears = (from ay in entities.AccountYear
                                    where ay.From < currentAccountYear.From &&
                                    ay.ActorCompanyId == currentAccountYear.ActorCompanyId
                                    orderby ay.From descending
                                    select ay);

                int accountYearNrCounter = -1;
                foreach (AccountYear accountYear in accountYears)
                {
                    if (accountYearNrCounter == accountYearNr)
                        return accountYear;

                    accountYearNrCounter--;
                }
            }

            return null;
        }

        public int GetAccountYearId(DateTime date, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYear.NoTracking();
            return GetAccountYearId(entities, date, actorCompanyId);
        }

        public int GetAccountYearId(CompEntities entities, DateTime date, int actorCompanyId)
        {
            DateTime dateWithZeroTime = new DateTime(date.Year, date.Month, date.Day);
            return (from ay in entities.AccountYear
                    where ay.ActorCompanyId == actorCompanyId &&
                    ay.From <= dateWithZeroTime &&
                    ay.To >= dateWithZeroTime
                    select ay.AccountYearId).FirstOrDefault();
        }

        public void GetAccountYearInfo(AccountYear accountYear, out int accountYearId, out bool accountYearIsOpen)
        {
            accountYearId = 0;
            accountYearIsOpen = false;

            if (accountYear != null)
            {
                accountYearId = accountYear.AccountYearId;
                accountYearIsOpen = accountYear.Status != (int)TermGroup_AccountStatus.Closed && accountYear.Status != (int)TermGroup_AccountStatus.Locked;
            }
        }

        /// <summary>
        /// Check if specified date is within current accountIdState year (based on todays date)
        /// </summary>
        /// <param name="actorCompanyId">Company ID</param>
        /// <param name="date">Date to check</param>
        /// <returns>True if within, else false</returns>
        public bool IsDateWithinCurrentAccountYear(int actorCompanyId, DateTime date)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return IsDateWithinCurrentAccountYear(entities, actorCompanyId, date);
        }

        public bool IsDateWithinCurrentAccountYear(CompEntities entities, int actorCompanyId, DateTime date)
        {
            AccountYear currentAccountYear = GetAccountYear(entities, DateTime.Now, actorCompanyId, false);
            AccountYear specifiedAccountYear = GetAccountYear(entities, date, actorCompanyId, false);

            return currentAccountYear != null && specifiedAccountYear != null && currentAccountYear.AccountYearId == specifiedAccountYear.AccountYearId;
        }

        public ActionResult ValidateAccountYear(AccountYear accountYear, DateTime? date = null)
        {
            if (accountYear == null)
            {
                return new ActionResult
                {
                    Success = false,
                    ErrorNumber = (int)ActionResultSave.AccountYearNotFound,
                    ErrorMessage = GetText(8404, "Redovisningsår saknas"),
                    StringValue = date.HasValue ? date.Value.ToShortDateString() : ""
                };
            }
            else if (accountYear.Status != (int)TermGroup_AccountStatus.Open)
            {
                return new ActionResult
                {
                    Success = false,
                    StringValue = accountYear.From.ToShortDateString() + "-" + accountYear.To.ToShortDateString(),
                    ErrorMessage = GetText(1434, "Angivet redovisningsår är inte öppet") + ": " + accountYear.From.ToShortDateString() + "-" + accountYear.To.ToShortDateString(),
                    ErrorNumber = (int)ActionResultSave.AccountYearNotOpen
                };
            }
            else if (date.HasValue && !CalendarUtility.IsDateInRange(date.Value, accountYear.From, accountYear.To))
            {
                return new ActionResult
                {
                    Success = false,
                    ErrorMessage = GetText(444, "Bokf.datum stämmer inte med aktuellt år") + ": " + date.ToShortDateString(),
                    ErrorNumber = (int)ActionResultSave.AccountYearVoucherDateDoNotMatch
                };
            }

            return new ActionResult(true);
        }

        public ActionResult ValidateAccountYear(DateTime date, DateTime fromDate, DateTime toDate)
        {
            if (!CalendarUtility.IsDateInRange(date, fromDate, toDate))
            {
                return new ActionResult
                {
                    Success = false,
                    ErrorMessage = GetText(444, "Bokf.datum stämmer inte med aktuellt år") + ": " + date.ToShortDateString(),
                    ErrorNumber = (int)ActionResultSave.AccountYearVoucherDateDoNotMatch
                };
            }

            return new ActionResult(true);
        }

        public ActionResult SaveAccountYear(AccountYearDTO accountYear, List<VoucherSeriesDTO> series, int actorCompanyId, bool keepNumberSeries)
        {
            ActionResult result = new ActionResult(false);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereqs

                bool isNew = (accountYear.AccountYearId == 0);

                AccountYear previousAccountYear = GetPreviousAccountYear(entities, accountYear.From, actorCompanyId);
                List<AccountYear> accountYears = GetAccountYears(entities, actorCompanyId, false, false);

                var noOfOpenYearsSetting = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountingMaxYearOpen, 0, actorCompanyId, 0);
                var noOfOpenYears = accountYears.Count(a => (isNew || a.AccountYearId != accountYear.AccountYearId) && a.Status == (int)TermGroup_AccountStatus.Open);

                if (accountYear.Status == TermGroup_AccountStatus.Open && noOfOpenYears >= noOfOpenYearsSetting)
                    return new ActionResult((int)ActionResultSave.NothingSaved, String.Format(GetText(7488, "Det är endast tillåtet att ha {0} öppna redovisningsår samtidigt."), noOfOpenYearsSetting.ToString()));

                List<VoucherSeriesType> voucherSeriesTypes = (from vst in entities.VoucherSeriesType
                                                              where vst.ActorCompanyId == actorCompanyId &&
                                                              vst.State == (int)SoeEntityState.Active &&
                                                              !vst.Template
                                                              orderby vst.VoucherSeriesTypeNr ascending
                                                              select vst).ToList();

                List<VoucherSeries> voucherSeries = isNew ? new List<VoucherSeries>() : VoucherManager.GetVoucherSeriesByYear(entities, accountYear.AccountYearId, actorCompanyId, false, false);
                List<VoucherSeries> previousVoucherSeries = previousAccountYear != null ? VoucherManager.GetVoucherSeriesByYear(entities, previousAccountYear.AccountYearId, actorCompanyId, false, false) : new List<VoucherSeries>();

                #endregion

                #region AccountYear

                AccountYear newAccountYear = null;

                if (accountYear.AccountYearId > 0)
                {
                    newAccountYear = GetAccountYear(entities, accountYear.AccountYearId, true);
                    if (newAccountYear == null)
                        return new ActionResult((int)ActionResultSave.AccountYearNotFound, "AccountYear");

                    if (newAccountYear.From != accountYear.From)
                        newAccountYear.From = accountYear.From;

                    if (newAccountYear.To != accountYear.To)
                        newAccountYear.To = accountYear.To;

                    newAccountYear.Status = (int)accountYear.Status;
                    SetModifiedProperties(newAccountYear);

                    // Validate to see that new period is not overlapping existing ones
                    foreach (AccountYear year in accountYears.Where(a => a.AccountYearId != newAccountYear.AccountYearId))
                    {
                        if ((newAccountYear.From < year.From && newAccountYear.To > year.From) ||
                            (newAccountYear.From > year.From && newAccountYear.To < year.To) ||
                            (newAccountYear.From < year.To && newAccountYear.To > year.To) ||
                            (newAccountYear.From == year.From) ||
                            (newAccountYear.To == year.To))
                        {
                            return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(9326, "Året innehåller en eller flera perioder som överlappar annat års perioder. Vänligen kontrollera och spara igen."));
                        }
                    }
                }
                else
                {
                    newAccountYear = new AccountYear()
                    {
                        Status = (int)TermGroup_AccountStatus.New,
                        From = accountYear.From,
                        To = accountYear.To,
                        ActorCompanyId = actorCompanyId,
                    };

                    SetCreatedProperties(newAccountYear);

                    // Validate to see that new period is not overlapping existing ones
                    foreach (AccountYear year in accountYears)
                    {
                        if ((newAccountYear.From < year.From && newAccountYear.To > year.From) ||
                            (newAccountYear.From > year.From && newAccountYear.To < year.To) ||
                            (newAccountYear.From < year.To && newAccountYear.To > year.To) ||
                            (newAccountYear.From == year.From) ||
                            (newAccountYear.To == year.To))
                        {
                            return new ActionResult((int)ActionResultSave.EntityNotCreated, GetText(9326, "Året innehåller en eller flera perioder som överlappar annat års perioder. Vänligen kontrollera och spara igen."));
                        }
                    }

                    result = AddEntityItem(entities, newAccountYear, "AccountYear");
                    if (!result.Success)
                        return result;
                }

                #endregion

                #region AccountPeriods

                int? lowestStatus = null;
                if (isNew)
                {
                    foreach (AccountPeriodDTO period in accountYear.Periods)
                    {
                        AccountPeriod newPeriod = new AccountPeriod()
                        {
                            From = period.From,
                            To = new DateTime(period.From.Year, period.From.Month, DateTime.DaysInMonth(period.From.Year, period.From.Month)),
                            Status = (int)period.Status,
                            PeriodNr = period.PeriodNr,
                            AccountYear = newAccountYear,
                        };
                        SetCreatedProperties(newPeriod);
                        entities.AccountPeriod.AddObject(newPeriod);

                        if (lowestStatus == null || (lowestStatus < newPeriod.Status && lowestStatus != (int)TermGroup_AccountStatus.Open) || newPeriod.Status == (int)TermGroup_AccountStatus.Open)
                            lowestStatus = newPeriod.Status;
                    }
                }
                else
                {
                    foreach (var period in accountYear.Periods)
                    {
                        if (period.IsDeleted)
                        {
                            var periodToDelete = newAccountYear.AccountPeriod.FirstOrDefault(p => p.AccountPeriodId == period.AccountPeriodId);
                            if (periodToDelete == null)
                                return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountPeriod");

                            if (!periodToDelete.VoucherHead.IsLoaded)
                                periodToDelete.VoucherHead.Load();

                            if (periodToDelete.VoucherHead.Any(v => !v.Template))
                                return new ActionResult((int)ActionResultDelete.NothingSaved, GetText(7493, "En eller flera perioder har sparade verifikat och kan därför inte tas bort."));

                            foreach (var head in periodToDelete.VoucherHead.ToList())
                            {
                                if (!head.VoucherRow.IsLoaded)
                                    head.VoucherRow.Load();

                                foreach (VoucherRow voucherRow in head.VoucherRow.ToList())
                                {
                                    if (!voucherRow.AccountInternal.IsLoaded)
                                        voucherRow.AccountInternal.Load();

                                    voucherRow.AccountInternal.Clear();

                                    if (!SaveEntityItem(entities, voucherRow).Success)
                                        return new ActionResult((int)ActionResultDelete.VoucherRowAccountNotDeleted);

                                    if (!DeleteEntityItem(entities, voucherRow).Success)
                                        return new ActionResult((int)ActionResultDelete.VoucherRowAccountNotDeleted);
                                }

                                // Remove voucher
                                DeleteEntityItem(entities, head, null);
                            }

                            DeleteEntityItem(entities, periodToDelete);
                        }
                        else if (period.AccountPeriodId == 0)
                        {
                            AccountPeriod newPeriod = new AccountPeriod()
                            {
                                From = period.From,
                                To = new DateTime(period.From.Year, period.From.Month, DateTime.DaysInMonth(period.From.Year, period.From.Month)),
                                Status = (int)period.Status,
                                PeriodNr = period.PeriodNr,
                                AccountYear = newAccountYear,
                            };
                            SetCreatedProperties(newPeriod);
                            entities.AccountPeriod.AddObject(newPeriod);

                            if (lowestStatus == null || (lowestStatus < newPeriod.Status && lowestStatus != (int)TermGroup_AccountStatus.Open) || newPeriod.Status == (int)TermGroup_AccountStatus.Open)
                                lowestStatus = newPeriod.Status;
                        }
                        else
                        {
                            AccountPeriod existingPeriod = newAccountYear.AccountPeriod.FirstOrDefault(p => p.AccountPeriodId == period.AccountPeriodId);
                            if (existingPeriod != null)
                            {
                                if (existingPeriod.Status != (int)period.Status)
                                {
                                    existingPeriod.Status = (int)period.Status;
                                    SetModifiedProperties(existingPeriod);
                                    VoucherManager.UpdateVoucherHeadsStatus(entities, actorCompanyId, period.AccountPeriodId, (int)period.Status);
                                }

                                if (lowestStatus == null || (lowestStatus < existingPeriod.Status && lowestStatus != (int)TermGroup_AccountStatus.Open) || existingPeriod.Status == (int)TermGroup_AccountStatus.Open)
                                    lowestStatus = existingPeriod.Status;
                            }
                        }
                    }
                }

                #endregion

                #region VoucherSeries

                foreach (var serie in series)
                {
                    if (serie.IsDeleted)
                    {
                        var voucherSerie = voucherSeries.FirstOrDefault(s => s.VoucherSeriesId == serie.VoucherSeriesId);
                        if (voucherSerie == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherSerie");

                        DeleteEntityItem(entities, voucherSerie);
                    }
                    else if (serie.VoucherSeriesId == 0)
                    {
                        var voucherSeriesType = voucherSeriesTypes.FirstOrDefault(t => t.VoucherSeriesTypeId == serie.VoucherSeriesTypeId);
                        if (voucherSeriesType == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherSerieType");

                        var previousVoucherSerie = previousVoucherSeries.FirstOrDefault(s => s.VoucherSeriesTypeId == serie.VoucherSeriesTypeId);
                        VoucherSeries voucherSerie = new VoucherSeries()
                        {
                            VoucherNrLatest = keepNumberSeries && previousVoucherSerie != null ? previousVoucherSerie.VoucherNrLatest : voucherSeriesType.StartNr - 1,

                            //Set references
                            AccountYear = newAccountYear,
                            VoucherSeriesType = voucherSeriesType,
                        };
                        SetCreatedProperties(voucherSerie);
                        entities.VoucherSeries.AddObject(voucherSerie);

                    }
                    else
                    {
                        var voucherSerie = voucherSeries.FirstOrDefault(s => s.VoucherSeriesId == serie.VoucherSeriesId);
                        if (voucherSerie == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherSerie");

                        var voucherSeriesType = voucherSeriesTypes.FirstOrDefault(t => t.VoucherSeriesTypeId == serie.VoucherSeriesTypeId);
                        if (voucherSeriesType == null)
                            return new ActionResult((int)ActionResultDelete.EntityNotFound, "VoucherSerieType");

                        voucherSerie.VoucherNrLatest = serie.VoucherNrLatest;
                        voucherSerie.VoucherSeriesType = voucherSeriesType;

                        SetModifiedProperties(voucherSerie);
                    }
                }

                #endregion

                // Set status on year
                if (lowestStatus.HasValue && (lowestStatus < newAccountYear.Status || lowestStatus.Value == (int)TermGroup_AccountStatus.Open))
                    newAccountYear.Status = lowestStatus.Value;

                result = SaveChanges(entities);

                if (result.Success)
                {
                    result.IntegerValue = newAccountYear.AccountYearId;

                    if (isNew)
                    {
                        // Add template voucher serie to new accountIdState year
                        var templateType = VoucherManager.GetTemplateVoucherSeriesType(actorCompanyId, true);
                        VoucherManager.AddVoucherSeries(new VoucherSeries(), actorCompanyId, newAccountYear.AccountYearId, templateType.VoucherSeriesTypeId);
                    }
                }
            }

            return result;
        }

        public ActionResult AddAccountYear(AccountYear accountYear, int actorCompanyId)
        {
            if (accountYear == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountYear");

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYear.NoTracking();
            return AddAccountYear(entities, accountYear, actorCompanyId);
        }

        public ActionResult AddAccountYear(CompEntities entities, AccountYear accountYear, int actorCompanyId)
        {
            accountYear.Company = CompanyManager.GetCompany(entities, actorCompanyId);
            if (accountYear.Company == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

            return AddEntityItem(entities, accountYear, "AccountYear", addToContext: false);
        }

        public ActionResult UpdateAccountYear(AccountYear accountYear)
        {
            if (accountYear == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountYear");

            using (CompEntities entities = new CompEntities())
            {
                AccountYear orginalAccountYear = GetAccountYear(entities, accountYear.AccountYearId);
                if (orginalAccountYear == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

                return UpdateEntityItem(entities, orginalAccountYear, accountYear, "AccountYear");
            }
        }

        public ActionResult DeleteAccountYear(int accountYearId, int actorCompanyId)
        {
            ActionResult result = null;

            using (CompEntities entities = new CompEntities())
            {
                List<GrossProfitCode> grossProfitCodes = GrossProfitManager.GetGrossProfitCodes(entities, actorCompanyId, accountYearId, null, true);
                var hasActiveGrossProfitCodes = grossProfitCodes.Any(code => code.State == (int)SoeEntityState.Active);
                if (hasActiveGrossProfitCodes)
                    return new ActionResult((int)ActionResultDelete.EntityInUse, GetText(92039, "Redovisningsåret har en eller flera aktiva bruttovinstkoder och kan därför inte tas bort."));

                foreach (GrossProfitCode grossProfitCode in grossProfitCodes)
                {
                    result = DeleteEntityItem(entities, grossProfitCode);
                    if (!result.Success)
                        return result;
                }

                List<VoucherSeries> voucherSeries = VoucherManager.GetVoucherSeriesByYear(entities, accountYearId, actorCompanyId, true);
                foreach (VoucherSeries voucherSerie in voucherSeries)
                {
                    if (VoucherManager.VoucherSeriesHasVoucherHeads(entities, voucherSerie.VoucherSeriesTypeId, voucherSerie.AccountYearId))
                        return new ActionResult((int)ActionResultDelete.VoucherSeriesHasVoucherSeries);

                    result = DeleteEntityItem(entities, voucherSerie);
                    if (!result.Success)
                        return result;
                }

                List<AccountPeriod> accountPeriods = GetAccountPeriods(entities, accountYearId, false);
                foreach (AccountPeriod accountPeriod in accountPeriods)
                {
                    result = DeleteEntityItem(entities, accountPeriod);
                    if (!result.Success)
                        return result;
                }

                AccountYear orginalAccountYear = GetAccountYear(entities, accountYearId);
                if (orginalAccountYear == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountYear");

                result = DeleteEntityItem(entities, orginalAccountYear);
            }

            return result;
        }

        public ActionResult DeleteAccountYear(AccountYear accountYear)
        {
            if (accountYear == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "AccountYear");

            using (CompEntities entities = new CompEntities())
            {
                AccountYear orginalAccountYear = GetAccountYear(entities, accountYear.AccountYearId);
                if (orginalAccountYear == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "AccountYear");

                return DeleteEntityItem(entities, orginalAccountYear);
            }
        }

        public int GetNumberOfOpenAccountYears(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYear.NoTracking();
            return GetNumberOfOpenAccountYears(entities, actorCompanyId);
        }

        public int GetNumberOfOpenAccountYears(CompEntities entities, int actorCompanyId)
        {
            return GetAccountYears(entities, actorCompanyId, true, false).Count;
        }

        #endregion

        #region AccountPeriod

        public List<AccountPeriod> GetAccountPeriods(int accountYearId, bool loadAccountYear)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return GetAccountPeriods(entities, accountYearId, loadAccountYear);
        }

        public List<AccountPeriod> GetAccountPeriods(CompEntities entities, int accountYearId, bool loadAccountYear)
        {
            if (loadAccountYear)
            {
                return (from ap in entities.AccountPeriod
                            .Include("AccountYear")
                        where ap.AccountYearId == accountYearId
                        select ap).ToList();
            }
            else
            {
                return (from ap in entities.AccountPeriod
                        where ap.AccountYearId == accountYearId
                        select ap).ToList();
            }
        }

        public List<AccountPeriod> GetAccountPeriodsInDateInterval(CompEntities entities, int accountYearId, DateTime dateFrom, DateTime dateTo)
        {
            return (from ap in entities.AccountPeriod
                    where ap.AccountYearId == accountYearId &&
                    ap.From >= dateFrom &&
                    ap.To <= dateTo
                    orderby ap.From ascending
                    select ap).Distinct().ToList();
        }

        public List<AccountPeriod> GetAccountPeriodsInDateInterval(int accountYearId, DateTime dateFrom, DateTime dateTo)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return (from ap in entities.AccountPeriod
                    where ap.AccountYearId == accountYearId &&
                    ap.From >= dateFrom &&
                    ap.To <= dateTo
                    orderby ap.From ascending
                    select ap).Distinct().ToList();
        }

        public Dictionary<int, string> GetAccountPeriodsInDateIntervalDict(int accountYearId, DateTime dateFrom, DateTime dateTo, bool addEmptyValue = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            List<AccountPeriod> accountPeriods = (from ap in entities.AccountPeriod
                                                  where ap.AccountYearId == accountYearId &&
                                                  ap.From >= dateFrom &&
                                                  ap.To <= dateTo
                                                  orderby ap.From ascending
                                                  select ap).Distinct().ToList();

            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyValue)
                dict.Add(0, " ");

            foreach (AccountPeriod accountPeriod in accountPeriods)
            {
                string value = accountPeriod.From.ToString("yyyyMM", CultureInfo.CurrentCulture);
                dict.Add(accountPeriod.AccountPeriodId, value);
            }

            return dict;
        }

        public List<AccountPeriod> GetAccountPeriods(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return (from ap in entities.AccountPeriod
                    where ap.AccountYear.ActorCompanyId == actorCompanyId
                    orderby ap.From ascending
                    select ap).Distinct().ToList();
        }

        public Dictionary<int, string> GetAccountPeriodsIntervalDict(int accountYearId, bool addEmptyValue = true)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            List<AccountPeriod> accountPeriods = (from ap in entities.AccountPeriod
                                                  where ap.AccountYearId == accountYearId
                                                  orderby ap.From, ap.To ascending
                                                  select ap).ToList();

            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyValue)
                dict.Add(0, " ");

            foreach (AccountPeriod accountPeriod in accountPeriods)
            {
                string value = accountPeriod.From.ToString("yyyyMM", CultureInfo.CurrentCulture);
                dict.Add(accountPeriod.AccountPeriodId, value);
            }

            return dict;
        }

        public AccountPeriod GetAccountPeriod(int accountPeriodId, bool loadAccountYear = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return GetAccountPeriod(entities, accountPeriodId, loadAccountYear);
        }

        public AccountPeriod GetAccountPeriod(CompEntities entities, int accountPeriodId, bool loadAccountYear = false)
        {
            if (loadAccountYear)
            {
                return (from ap in entities.AccountPeriod
                            .Include("AccountYear")
                        where ap.AccountPeriodId == accountPeriodId
                        select ap).FirstOrDefault();
            }
            else
            {
                return (from ap in entities.AccountPeriod
                        where ap.AccountPeriodId == accountPeriodId
                        select ap).FirstOrDefault();
            }
        }

        public AccountPeriod GetAccountPeriod(DateTime date, int actorCompanyId, bool loadAccountYear = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return GetAccountPeriod(entities, date, actorCompanyId, loadAccountYear);
        }

        public AccountPeriod GetAccountPeriod(CompEntities entities, DateTime date, int actorCompanyId, bool loadAccountYear = false)
        {
            if (!CalendarUtility.IsDateTimeSqlServerValid(date))
                return null;

            //make sure datetime has no hours, otherwise gives wrong result if date is last day of month
            date = date.Date;

            if (loadAccountYear)
            {
                return (from ap in entities.AccountPeriod
                            .Include("AccountYear")
                        where ap.AccountYear.ActorCompanyId == actorCompanyId &&
                        ap.From <= date &&
                        ap.To >= date
                        select ap).FirstOrDefault();
            }
            else
            {
                return (from ap in entities.AccountPeriod
                        where ap.AccountYear.ActorCompanyId == actorCompanyId &&
                        ap.From <= date &&
                        ap.To >= date
                        select ap).FirstOrDefault();
            }
        }

        public AccountPeriod GetAccountPeriod(int accountYearId, DateTime date, int actorCompanyId, bool loadAccountYear = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return GetAccountPeriod(entities, accountYearId, date, actorCompanyId, loadAccountYear);
        }

        public AccountPeriod GetAccountPeriod(CompEntities entities, int accountYearId, DateTime date, int actorCompanyId, bool loadAccountYear = false)
        {
            if (!CalendarUtility.IsDateTimeSqlServerValid(date))
                return null;

            //make sure datetime has no hours, otherwise gives wrong result if date is last day of month
            date = date.Date;

            if (loadAccountYear)
            {
                return (from ap in entities.AccountPeriod
                            .Include("AccountYear")
                        where ap.AccountYear.AccountYearId == accountYearId &&
                        ap.AccountYear.ActorCompanyId == actorCompanyId &&
                        ap.From <= date &&
                        ap.To >= date
                        select ap).FirstOrDefault();
            }
            else
            {
                return (from ap in entities.AccountPeriod
                        where ap.AccountYear.AccountYearId == accountYearId &&
                        ap.AccountYear.ActorCompanyId == actorCompanyId &&
                        ap.From <= date &&
                        ap.To >= date
                        select ap).FirstOrDefault();
            }
        }

        public AccountPeriod GetFirstAccountPeriod(CompEntities entities, int accountYearId, int actorCompanyId, bool loadAccountYear)
        {
            if (loadAccountYear)
            {
                return (from ap in entities.AccountPeriod
                            .Include("AccountYear")
                        where ap.AccountYearId == accountYearId &&
                        ap.AccountYear.ActorCompanyId == actorCompanyId
                        orderby ap.From ascending
                        select ap).FirstOrDefault();
            }
            else
            {
                return (from ap in entities.AccountPeriod
                        where ap.AccountYearId == accountYearId &&
                        ap.AccountYear.ActorCompanyId == actorCompanyId
                        orderby ap.From ascending
                        select ap).FirstOrDefault();
            }
        }

        public AccountPeriod GetFirstAccountPeriodInterval(int accountYearId, int actorCompanyId, bool loadAccountYear, DateTime From, DateTime To)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return GetFirstAccountPeriodInterval(entities, accountYearId, actorCompanyId, loadAccountYear, From, To);
        }

        public AccountPeriod GetFirstAccountPeriodInterval(CompEntities entities, int accountYearId, int actorCompanyId, bool loadAccountYear, DateTime From, DateTime To)
        {
            if (loadAccountYear)
            {
                return (from ap in entities.AccountPeriod
                            .Include("AccountYear")
                        where ap.AccountYearId == accountYearId &&
                        ap.From <= From &&
                        ap.To >= To &&
                        ap.AccountYear.ActorCompanyId == actorCompanyId
                        orderby ap.From ascending
                        select ap).FirstOrDefault();
            }
            else
            {
                return (from ap in entities.AccountPeriod
                        where ap.AccountYearId == accountYearId &&
                         ap.From <= From &&
                        ap.To >= To &&
                        ap.AccountYear.ActorCompanyId == actorCompanyId
                        orderby ap.From ascending
                        select ap).FirstOrDefault();
            }
        }

        public AccountPeriod GetCurrentAccountPeriod(int accountYearId, int actorCompanyId, DateTime From, DateTime To)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return (from ap in entities.AccountPeriod
                    where ap.AccountYearId == accountYearId &&
                    ap.AccountYear.ActorCompanyId == actorCompanyId &&
                    ap.From <= From &&
                    ap.To >= To
                    select ap).FirstOrDefault();
        }

        public AccountPeriod GetNextAccountPeriod(AccountPeriod currentAccountPeriod, int actorCompanyId)
        {
            if (currentAccountPeriod == null)
                return null;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return (from ap in entities.AccountPeriod
                    where ap.From > currentAccountPeriod.From &&
                    ap.AccountYear.ActorCompanyId == actorCompanyId
                    orderby ap.From ascending
                    select ap).FirstOrDefault();
        }

        public AccountPeriod GetNextAccountPeriod(DateTime dt, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return (from ap in entities.AccountPeriod
                    where ap.From > dt.Date &&
                    ap.AccountYear.ActorCompanyId == actorCompanyId
                    orderby ap.From ascending
                    select ap).FirstOrDefault();
        }

        public int GetAccountPeriodId(int accountYearId, int actorCompanyId, DateTime date)
        {
            if (!CalendarUtility.IsDateTimeSqlServerValid(date))
                return 0;

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            AccountPeriod accountPeriod = (from ap in entities.AccountPeriod
                                           where ap.AccountYear.AccountYearId == accountYearId &&
                                           ap.AccountYear.ActorCompanyId == actorCompanyId &&
                                           ap.From <= date &&
                                           ap.To >= date
                                           select ap).FirstOrDefault();

            return accountPeriod != null ? accountPeriod.AccountPeriodId : 0;
        }

        public int GetNrOfAccountPeriodsOpen(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountPeriod.NoTracking();
            return (from ap in entities.AccountPeriod
                    where ap.AccountYear.ActorCompanyId == actorCompanyId &&
                    ap.Status == (int)TermGroup_AccountStatus.Open
                    select ap).Count();
        }

        public ActionResult ValidateAccountPeriod(AccountPeriod accountPeriod, DateTime? date = null)
        {
            ActionResult result = new ActionResult(true);

            if (accountPeriod == null)
            {
                result.Success = false;
                if (date.HasValue)
                    result.StringValue = date.Value.ToShortDateString();
                result.ErrorNumber = (int)ActionResultSave.AccountPeriodNotFound;
                result.ErrorMessage = GetText(7305, "Perioden saknas") + date.ToString();
            }
            else if (accountPeriod.Status != (int)TermGroup_AccountStatus.Open)
            {
                result.Success = false;
                result.StringValue = accountPeriod.From.ToShortDateString() + "-" + accountPeriod.To.ToShortDateString();
                result.ErrorNumber = (int)ActionResultSave.AccountPeriodNotOpen;
                result.ErrorMessage = GetText(7304, "Perioden är inte öppen");
            }
            else if (date.HasValue && !CalendarUtility.IsDateInRange(date.Value, accountPeriod.From, accountPeriod.To))
            {
                result.Success = false;
                result.ErrorMessage = GetText(7647, "Bokf.datum stämmer inte med aktuell period") + ": " + date.ToShortDateString();
                result.ErrorNumber = (int)ActionResultSave.AccountYearVoucherDateDoNotMatch;

            }
            return result;
        }

        public ActionResult AddAccountPeriod(AccountPeriod accountPeriod, AccountYear accountYear)
        {
            using (CompEntities entities = new CompEntities())
            {
                return AddAccountPeriod(entities, accountPeriod, accountYear);
            }
        }

        public ActionResult AddAccountPeriod(CompEntities entities, AccountPeriod accountPeriod, AccountYear accountYear)
        {
            accountPeriod.AccountYear = GetAccountYear(entities, accountYear.AccountYearId);
            if (accountPeriod.AccountYear == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

            return AddEntityItem(entities, accountPeriod, "AccountPeriod");
        }

        public ActionResult UpdateAccountPeriod(AccountPeriod accountPeriod)
        {
            if (accountPeriod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountPeriod");

            using (CompEntities entities = new CompEntities())
            {
                AccountPeriod orginalAccountPeriod = GetAccountPeriod(entities, accountPeriod.AccountPeriodId);
                if (orginalAccountPeriod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountPeriod");

                return UpdateEntityItem(entities, orginalAccountPeriod, accountPeriod, "AccountPeriod");
            }
        }

        public ActionResult UpdateAccountPeriodStatus(int accountPeriodId, TermGroup_AccountStatus status)
        {
            using (CompEntities entities = new CompEntities())
            {
                AccountPeriod accountPeriod = GetAccountPeriod(entities, accountPeriodId);
                if (accountPeriod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountPeriod");

                if (status == TermGroup_AccountStatus.Open) //if trying to open this accountperiod check if next is not closed
                {
                    AccountPeriod nextAccountPeriod = GetNextAccountPeriod(accountPeriod, ActorCompanyId);
                    if (nextAccountPeriod != null && nextAccountPeriod.Status == (int)TermGroup_AccountStatus.Closed) //if next accountperiod is closed, is not allowed to open current period
                        return new ActionResult((int)ActionResultSave.AccountPeriodNotOpen, "Next accountperiod is closed, not allowed to open this accountperiod");
                }

                accountPeriod.Status = (int)status;
                return SaveChanges(entities);
            }
        }

        public ActionResult UpdateAllAccountPeriods(int accountYearId, int status)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<AccountPeriod> accountPeriods = GetAccountPeriods(entities, accountYearId, false);
                foreach (AccountPeriod accountPeriod in accountPeriods)
                {
                    accountPeriod.Status = status;
                }

                return SaveChanges(entities);
            }
        }

        public ActionResult DeleteAccountPeriodsForYear(int accountYearId, DateTime? newToDate)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<AccountPeriod> accountPeriods = GetAccountPeriods(entities, accountYearId, false);
                foreach (AccountPeriod accountPeriod in accountPeriods)
                {
                    if (!newToDate.HasValue || newToDate.Value < accountPeriod.From)
                        entities.DeleteObject(accountPeriod);
                }

                return SaveDeletions(entities);
            }
        }

        public ActionResult DeleteAccountPeriodsForYearFromStart(int accountYearId, DateTime newFromDate)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<AccountPeriod> accountPeriods = GetAccountPeriods(entities, accountYearId, false);
                foreach (AccountPeriod accountPeriod in accountPeriods)
                {
                    if (newFromDate > accountPeriod.From && accountPeriod.Status == (int)TermGroup_AccountStatus.New)
                        entities.DeleteObject(accountPeriod);
                }

                return SaveDeletions(entities);
            }
        }

        public ActionResult RenumberAllPeriods(int accountYearId)
        {
            using (CompEntities entities = new CompEntities())
            {
                int period = 1;
                List<AccountPeriod> accountPeriods = GetAccountPeriods(entities, accountYearId, false);
                foreach (AccountPeriod accountPeriod in accountPeriods)
                {
                    accountPeriod.PeriodNr = period;
                    period++;
                }

                return SaveChanges(entities);
            }
        }

        #endregion

        #region AccountType

        public int GetAccountType(int accountId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Account.NoTracking();
            return GetAccountType(entities, accountId);
        }

        public int GetAccountType(CompEntities entities, int accountId)
        {
            return (from a in entities.AccountStd
                    where a.AccountId == accountId
                    select a.AccountTypeSysTermId).FirstOrDefault();
        }

        #endregion

        #region SysAccountStd

        public List<SysAccountStd> GetSysAccountStds()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysAccountStd
                            .Include("SysAccountStdType")
                            .Include("SysAccountSruCode")
                            .Include("SysVatAccount")
                            .ToList<SysAccountStd>();
        }

        public SysAccountStd GetSysAccountStd(int sysAccountStdTypeParentId, string accountNr, bool includeRelations)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SysAccountStd> query = sysEntitiesReadOnly.Set<SysAccountStd>();
            if (includeRelations)
            {
                query = query.Include("SysAccountStdType");
                query = query.Include("SysAccountSruCode");
                query = query.Include("SysVatAccount");
            }

            return (from sas in query
                    where sas.AccountNr == accountNr
                    select sas).FirstOrDefault();
        }

        public SysAccountStd GetSysAccountStd(int sysAccountStdId, bool includeRelations)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SysAccountStd> query = sysEntitiesReadOnly.Set<SysAccountStd>();
            if (includeRelations)
            {
                query = query.Include("SysAccountStdType");
                query = query.Include("SysAccountSruCode");
                query = query.Include("SysVatAccount");
            }

            return (from sas in query
                    where sas.SysAccountStdId == sysAccountStdId
                    select sas).FirstOrDefault();
        }

        public Account ImportSysAccountStd(int actorCompanyId, int sysAccountStdId)
        {
            SysAccountStdDTO sysAccountStd = GetSysAccountStd(sysAccountStdId, true).ToDTO(true);
            return ImportSysAccountStd(actorCompanyId, sysAccountStd);
        }

        public Account ImportSysAccountStd(int actorCompanyId, SysAccountStdDTO sysAccountStd)
        {
            using (CompEntities entities = new CompEntities())
            {
                AccountDim accountDim = GetAccountDimStd(entities, actorCompanyId);
                if (accountDim != null)
                {
                    //Check that AccountStd fulfilles the AccountDim rules
                    if (IsAccountValidInAccountDim(accountDim.MinChar, accountDim.MaxChar, sysAccountStd.AccountNr))
                    {
                        AccountStd accStd = GetAccountStdByNr(entities, sysAccountStd.AccountNr, actorCompanyId);
                        if (accStd == null)
                        {
                            #region Account

                            Account account = new Account()
                            {
                                AccountNr = sysAccountStd.AccountNr,
                                Name = sysAccountStd.Name,

                                //Set FK
                                ActorCompanyId = actorCompanyId,

                                //Set references
                                AccountDim = accountDim,
                            };
                            SetCreatedProperties(account);

                            #endregion

                            #region AccountStd

                            account.AccountStd = new AccountStd()
                            {
                                SysVatAccountId = sysAccountStd.SysVatAccountId,
                                AccountTypeSysTermId = sysAccountStd.AccountTypeSysTermId,
                                AmountStop = sysAccountStd.AmountStop,
                                Unit = sysAccountStd.Unit,
                                UnitStop = sysAccountStd.UnitStop,
                            };

                            #endregion

                            #region SysAccountSruCode

                            if (sysAccountStd.SysAccountSruCodeIds != null)
                            {
                                foreach (int sysAccountSruCodeId in sysAccountStd.SysAccountSruCodeIds)
                                {
                                    AccountSru accountSru = new AccountSru()
                                    {
                                        SysAccountSruCodeId = sysAccountSruCodeId,
                                    };
                                    account.AccountStd.AccountSru.Add(accountSru);
                                }
                            }

                            #endregion

                            #region AccountHistory

                            account.AccountHistory.Add(new AccountHistory()
                            {
                                Name = account.Name,
                                AccountNr = account.AccountNr,
                                Date = DateTime.Now,
                                SysAccountStdTypeId = sysAccountStd.SysAccountStdTypeId,
                                SieKpTyp = account.AccountStd.SieKpTyp,

                                //Set FK
                                UserId = base.UserId,
                            });

                            #endregion
                        }
                        else
                        {
                            if (log.IsInfoEnabled) log.Info("AccountStd " + sysAccountStd.AccountNr + " already exists");
                        }
                    }
                    else
                    {
                        //Konto uppfyller inte regler för sin dimension
                        if (log.IsInfoEnabled) log.Info("AccountStd " + sysAccountStd.AccountNr + " dont fulfilles the rules for AccountDim " + accountDim.Name);
                    }

                    ActionResult result = SaveChangesWithTransaction(entities);
                    if (result.Success)
                        return GetAccountByDimNr(sysAccountStd.AccountNr, accountDim.AccountDimNr, actorCompanyId, loadAccount: true, loadAccountDim: true, loadAccountSru: true);
                }
            }

            return null;
        }

        public ActionResult ImportSysAccountStds(int sysAccountStdTypeId)
        {
            var importPermission = FeatureManager.HasRolePermission(Feature.Economy_Import_Sie_Account, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId);
            if (sysAccountStdTypeId > 0 && importPermission)
            {
                return this.ImportSysAccountStds(sysAccountStdTypeId, base.ActorCompanyId);
            }
            return new ActionResult(false);
        }

        /// <summary>
        /// Move Accounts to given AccountDim from SysAccountStd with given AccountStdType
        /// </summary>
        /// <param name="sysAccountStdType">The SysAccountStdType the AccountStd should be imported from</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult ImportSysAccountStds(int sysAccountStdType, int actorCompanyId)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            var sysAccounts = (from sas in sysEntitiesReadOnly.SysAccountStd.Include("SysAccountStdType").Include("SysAccountSruCode").Include("SysVatAccount")
                               where sas.SysAccountStdType.SysAccountStdTypeId == sysAccountStdType
                               orderby sas.AccountNr
                               select sas).ToList();

            using (CompEntities entities = new CompEntities())
            {
                AccountDim accountDim = GetAccountDimStd(entities, actorCompanyId);
                if (accountDim != null)
                {
                    //Update AccountDim
                    accountDim.SysAccountStdTypeParentId = sysAccountStdType;
                    SetModifiedProperties(accountDim);

                    //Get AccountStds once!
                    List<AccountStd> accountStds = GetAccountStdsByCompany(entities, actorCompanyId, null);

                    int skip = 0;
                    foreach (SysAccountStd sysAccount in sysAccounts)
                    {
                        #region Prereq

                        int counter = (from a in accountStds
                                       where a.Account.AccountNr == sysAccount.AccountNr
                                       select a).Count();

                        //Check that AccountStd dont already exists
                        if (counter > 0)
                        {
                            if (log.IsInfoEnabled) log.Info("AccountStd " + sysAccount.AccountNr + " already exists");
                            skip++;
                            continue;
                        }

                        //Check that AccountStd fulfilles the AccountDim rules
                        if (!IsAccountValidInAccountDim(accountDim.MinChar, accountDim.MaxChar, sysAccount.AccountNr))
                        {
                            //Konto uppfyller inte regler för sin dimension
                            if (log.IsInfoEnabled) log.Info("AccountStd " + sysAccount.AccountNr + " dont fulfilles the rules for AccountDim " + accountDim.Name);
                            skip++;
                            continue;
                        }

                        #endregion

                        #region Account

                        Account account = new Account()
                        {
                            AccountNr = sysAccount.AccountNr,
                            Name = sysAccount.Name,

                            //Set FK
                            ActorCompanyId = actorCompanyId,
                            AccountDimId = accountDim.AccountDimId,
                        };
                        entities.Account.AddObject(account);
                        SetCreatedProperties(account);

                        account.AccountStd = new AccountStd()
                        {
                            AccountTypeSysTermId = sysAccount.AccountTypeSysTermId,
                            AmountStop = sysAccount.AmountStop,
                            Unit = sysAccount.Unit,
                            UnitStop = sysAccount.UnitStop,
                            SysVatAccountId = sysAccount.SysVatAccount != null ? sysAccount.SysVatAccount.SysVatAccountId : (int?)null,
                        };

                        #endregion

                        #region AccountSru

                        foreach (SysAccountSruCode sysAccountSruCode in sysAccount.SysAccountSruCode)
                        {
                            AccountSru accountSru = new AccountSru()
                            {
                                SysAccountSruCodeId = sysAccountSruCode.SysAccountSruCodeId,
                            };
                            account.AccountStd.AccountSru.Add(accountSru);
                        }

                        #endregion

                        #region AccountHistory

                        AccountHistory accountHistory = new AccountHistory()
                        {
                            Name = account.Name,
                            AccountNr = account.AccountNr,
                            Date = DateTime.Now,
                            SysAccountStdTypeId = sysAccount.SysAccountStdType.SysAccountStdTypeId,
                            SieKpTyp = account.AccountStd.SieKpTyp,

                            //Set FK
                            UserId = base.UserId,

                            //Set references
                            Account = account,
                        };
                        entities.AddToAccountHistory(accountHistory);

                        #endregion
                    }
                }

                return SaveChangesWithTransaction(entities);
            }
        }

        /// <summary>
        /// Fetch a Account from SOESys that doesnt exists in AccountStd table (SOEComp) but the mapped SysAccountStd table (SOESys)
        /// </summary>
        /// <param name="accountDimId">The AccountDimId</param>
        /// <param name="accountNr">The AccountNr</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>A SysAccountStd entity</returns>
        public SysAccountStd GetSysAccountStdParent(int accountDimId, string accountNr, int actorCompanyId)
        {
            SysAccountStd sysAccountStd = null;
            AccountDim accountDim = GetAccountDim(accountDimId, actorCompanyId);
            if (accountDim != null && accountDim.SysAccountStdTypeParentId.HasValue)
                sysAccountStd = GetSysAccountStd(accountDim.SysAccountStdTypeParentId.Value, accountNr, true);
            return sysAccountStd;
        }

        #endregion

        #region SysAccountStdType

        /// <summary>
        /// Get all SysAccountStdType's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysAccountStdType> GetSysAccountStdTypes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysAccountStdType
                            .Include("SysAccountStdType2")
                            .ToList<SysAccountStdType>();
        }

        public List<GenericType> GetSysAccountStdTypeItems()
        {
            List<GenericType> items = new List<GenericType>();

            foreach (var sysAccountStdType in GetSysAccountStdTypes())
            {
                items.Add(new GenericType()
                {
                    Id = sysAccountStdType.SysAccountStdTypeId,
                    Name = sysAccountStdType.Name,
                });
            }

            return items;
        }

        public Dictionary<int, string> GetSysAccountStdTypesDict(bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            //Uses SysDbCache
            foreach (var sysAccountStd in SysDbCache.Instance.SysAccountStdTypes)
            {
                dict.Add(sysAccountStd.SysAccountStdTypeId, sysAccountStd.Name);
            }

            return dict;
        }

        public int? GetSysAccountStdTypeParentIdForStandardDim()
        {
            return GetAccountDimStd(base.ActorCompanyId)?.SysAccountStdTypeParentId;
        }

        #endregion

        #region SysAccountSru

        /// <summary>
        /// Get all SysAccountSruCode's
        /// Accessor for SysDbCache
        /// </summary>
        /// <returns></returns>
        public List<SysAccountSruCode> GetSysAccountSruCodes()
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            return sysEntitiesReadOnly.SysAccountSruCode
                            .ToList<SysAccountSruCode>();
        }

        /// <summary>
        /// Get a Dictionary with SRU codes.
        /// </summary>
        /// <param name="addEmptyRow">Adds an empty row at the beginning of the dictionary</param>
        /// <returns>Collection of SysAccountSruCode entities</returns>
        public Dictionary<int, string> GetSysAccountSruCodesDict(bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            //Uses SysDbCache
            foreach (var sysAccountSruCode in SysDbCache.Instance.SysAccountSruCodes)
            {
                dict.Add(sysAccountSruCode.SysAccountSruCodeId, sysAccountSruCode.SruCode + ". " + sysAccountSruCode.Name);
            }

            return dict;
        }

        #endregion

        #region SysVatAccount

        public List<SysVatAccount> GetSysVatAccounts(bool loadSysVatRate = false)
        {
            using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
            IQueryable<SysVatAccount> oQuery = sysEntitiesReadOnly.Set<SysVatAccount>();
            if (loadSysVatRate)
                oQuery = oQuery.Include("SysVatRate");

            return oQuery.ToList();
        }

        /// <summary>
        /// Get a Dictionary with VAT accountIdState codes.
        /// </summary>
        /// <param name="addEmptyRow">Adds an empty row at the beginning of the dictionary</param>
        /// <returns>Collection of SysVatAccount entities</returns>
        public Dictionary<int, string> GetSysVatAccountCodesDict(bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            if (addEmptyRow)
                dict.Add(0, " ");

            //Uses SysDbCache
            foreach (var sysVatAccountCode in SysDbCache.Instance.SysVatAccounts)
            {
                dict.Add(sysVatAccountCode.SysVatAccountId, sysVatAccountCode.Description);
            }

            return dict;
        }
        /// <summary>
        /// Get a Dictionary with VAT accountIdState codes by company country.
        /// </summary>
        /// <param name="addEmptyRow">Adds an empty row at the beginning of the dictionary</param>
        /// <returns>Collection of SysVatAccount entities</returns>
        public Dictionary<int, string> GetSysVatAccountsDict(int sysCountryId, bool addEmptyRow)
        {
            if (sysCountryId == 0)
                sysCountryId = GetLangId();

            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            //Uses SysDbCache
            foreach (var sysVatAccountCode in SysDbCache.Instance.SysVatAccounts.Where(i => !i.LangId.HasValue || i.LangId.Value == sysCountryId).OrderBy(i => i.Description))
            {
                if (!dict.ContainsKey(sysVatAccountCode.SysVatAccountId))
                    dict.Add(sysVatAccountCode.SysVatAccountId, sysVatAccountCode.Description);
            }

            return dict;
        }

        #endregion

        #region SysVatRate

        public List<SysVatRate> GetSysVatRates()
        {
            using (var entities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    return entities.SysVatRate.AsNoTracking().ToList();
                }
            }
        }

        public SysVatRate GetSysVatRate(int accountId)
        {
            if (accountId == 0)
                return null;

            // Get specified accountIdState
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountStd.NoTracking();
            AccountStd accountStd = (from a in entities.AccountStd
                                     where a.AccountId == accountId
                                     select a).FirstOrDefault();

            SysVatRate sysVatRate = null;
            if (accountStd != null)
            {
                using var sysEntitiesReadOnly = SysEntitiesProvider.LeaseReadOnlyContext();
                sysVatRate = (from r in sysEntitiesReadOnly.SysVatRate
                              where r.SysVatAccountId == accountStd.SysVatAccountId
                              select r).FirstOrDefault();
            }

            return sysVatRate;
        }

        public decimal GetSysVatRateValueFromAccount(int accountId)
        {
            if (accountId == 0)
                return 0;

            SysVatRate sysVatRate = GetSysVatRate(accountId);
            return sysVatRate != null ? sysVatRate.VatRate : 0;
        }

        public decimal GetSysVatRateValue(int sysVatAccountId, bool onlyActive)
        {
            using (var entities = new SOESysEntities())
            {
                using (var transaction = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    if (onlyActive)
                        return (from r in entities.SysVatRate
                                where r.SysVatAccountId == sysVatAccountId &&
                                r.IsActive == 1
                                select r.VatRate).FirstOrDefault();
                    else
                        return (from r in entities.SysVatRate
                                where r.SysVatAccountId == sysVatAccountId
                                select r.VatRate).FirstOrDefault();
                }
            }
        }

        public decimal GetVatRateValue(CompEntities entities, int accountId)
        {
            int actorCompanyId = base.ActorCompanyId;
            decimal vatRate = 0;

            var sysVatAccountId = (from a in entities.AccountVatRateView
                                   where a.AccountId == accountId &&
                                   a.ActorCompanyId == actorCompanyId
                                   select a.SysVatAccountId).FirstOrDefault();

            if (sysVatAccountId.HasValue)
            {
                vatRate = GetSysVatRateValue(sysVatAccountId.Value, true);
            }

            return vatRate;
        }

        #endregion

        #region Selection

        /// <summary>
        /// Get all AccountStds and AccountInternals in given selection.
        /// If no interval is selected, all AccountStds and AccountInternals are returned.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="es">The EvaluatedSelection</param>
        /// <param name="accountDimStd">The AccountDim std</param>
        /// <param name="accountStds">Collection of AccountStds that will be filled</param>
        /// <param name="accountInternals">Collection of AccountInternals that will be filled</param>
        /// <returns>If interval was given, but gave no result, valid will be false. Otherwise true.</returns>
        public bool GetAccountsInInterval(CompEntities entities, EvaluatedSelection es, AccountDim accountDimStd, bool sieSelection, ref List<AccountStd> accountStds, ref List<AccountInternal> accountInternals)
        {
            //AccountStds
            accountStds = GetAccountStdsInInterval(entities, es.SA_AccountIntervals, accountDimStd, es.ActorCompanyId, es.OnlyActiveAccounts, out bool validAccountStdSelection);

            //AccountInternals
            bool validAccountInternalSelection;
            if (!es.SA_AccountIntervals.IsNullOrEmpty())
            {
                accountInternals = GetAccountInternalsInInterval(entities, es.SA_AccountIntervals, accountDimStd, es.ActorCompanyId, es.OnlyActiveAccounts, sieSelection, out validAccountInternalSelection);
            }
            else
            {
                accountInternals = GetAccountInternalsInInterval(entities, es.SA_AccountIntervals, accountDimStd, es.ActorCompanyId, es.OnlyActiveAccounts, sieSelection, out validAccountInternalSelection);
                validAccountInternalSelection = true;
            }
            bool validSelection = validAccountStdSelection && validAccountInternalSelection;
            if (!validSelection)
            {
                accountStds.Clear();
                accountInternals.Clear();
            }
            return validSelection;
        }

        public bool GetAccountsInInterval(CompEntities entities, CreateReportResult reportResult, EconomyReportParamsDTO reportParams, AccountDim accountDimStd, bool sieSelection, ref List<AccountStd> accountStds, ref List<AccountInternal> accountInternals)
        {
            //AccountStds
            accountStds = GetAccountStdsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, reportParams.OnlyActiveAccounts, out bool validAccountStdSelection);

            //AccountInternals
            bool validAccountInternalSelection;
            if (!reportParams.SA_AccountIntervals.IsNullOrEmpty())
            {
                accountInternals = GetAccountInternalsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, reportParams.OnlyActiveAccounts, sieSelection, out validAccountInternalSelection);
            }
            else
            {
                accountInternals = GetAccountInternalsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, reportParams.OnlyActiveAccounts, sieSelection, out validAccountInternalSelection);
                validAccountInternalSelection = true;
            }
            bool validSelection = validAccountStdSelection && validAccountInternalSelection;
            if (!validSelection)
            {
                accountStds.Clear();
                accountInternals.Clear();
            }
            return validSelection;
        }

        public bool GetAccountsInInterval(CompEntities entities, CreateReportResult reportResult, BillingReportParamsDTO reportParams, AccountDim accountDimStd, bool sieSelection, ref List<AccountStd> accountStds, ref List<AccountInternal> accountInternals)
        {
            //AccountStds
            accountStds = GetAccountStdsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, reportParams.OnlyActiveAccounts, out bool validAccountStdSelection);

            //AccountInternals
            bool validAccountInternalSelection;
            if (!reportParams.SA_AccountIntervals.IsNullOrEmpty())
            {
                accountInternals = GetAccountInternalsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, reportParams.OnlyActiveAccounts, sieSelection, out validAccountInternalSelection);
            }
            else
            {
                accountInternals = GetAccountInternalsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, reportParams.OnlyActiveAccounts, sieSelection, out validAccountInternalSelection);
                validAccountInternalSelection = true;
            }
            bool validSelection = validAccountStdSelection && validAccountInternalSelection;
            if (!validSelection)
            {
                accountStds.Clear();
                accountInternals.Clear();
            }
            return validSelection;
        }

        //Ny
        public bool GetAccountsInInterval(CompEntities entities, List<AccountIntervalDTO> accountIntervals, int ActorCompanyId, bool? onlyActiveAccounts, AccountDim accountDimStd, bool sieSelection, ref List<AccountStd> accountStds, ref List<AccountInternal> accountInternals)
        {
            //AccountStds
            accountStds = GetAccountStdsInInterval(entities, accountIntervals, accountDimStd, ActorCompanyId, onlyActiveAccounts, out bool validAccountStdSelection);

            //AccountInternals
            bool validAccountInternalSelection;
            if (!accountIntervals.IsNullOrEmpty())
            {
                accountInternals = GetAccountInternalsInInterval(entities, accountIntervals, accountDimStd, ActorCompanyId, onlyActiveAccounts, sieSelection, out validAccountInternalSelection);
            }
            else
            {
                accountInternals = GetAccountInternalsInInterval(entities, accountIntervals, accountDimStd, ActorCompanyId, onlyActiveAccounts, sieSelection, out validAccountInternalSelection);
                validAccountInternalSelection = true;
            }
            bool validSelection = validAccountStdSelection && validAccountInternalSelection;
            if (!validSelection)
            {
                accountStds.Clear();
                accountInternals.Clear();
            }
            return validSelection;
        }

        public bool GetAccountsInInterval(CompEntities entities, EvaluatedSelection es, AccountDimDTO accountDimStd, bool sieSelection, ref List<AccountDTO> accountStds, ref List<AccountInternalDTO> accountInternals)
        {
            //AccountStds
            accountStds = GetAccountStdsInInterval(entities, es.SA_AccountIntervals, accountDimStd, es.ActorCompanyId, es.OnlyActiveAccounts, out bool validAccountStdSelection);

            //AccountInternals
            bool onlyActiveInternAlaccount = false;
            if (!es.OnlyActiveAccounts == null || es.SSTD_SeparateAccountDim)
            {
                onlyActiveInternAlaccount = true;
            }
            accountInternals = GetAccountInternalsInInterval(entities, es.SA_AccountIntervals, accountDimStd, es.ActorCompanyId, onlyActiveInternAlaccount, sieSelection, out bool validAccountInternalSelection);

            if (es.SSTD_AccountDimId != 0)
            {
                List<AccountInternalDTO> AccountIntervalsForDimId = accountInternals.Where(i => i.AccountDimId == es.SSTD_AccountDimId).ToList();
                if (!AccountIntervalsForDimId.Any())
                {
                    List<AccountInternalDTO> separateInterval = AccountManager.GetAccountInternalsByDim(es.SSTD_AccountDimId, es.ActorCompanyId).ToDTOs();
                    accountInternals.AddRange(separateInterval);
                }
            }

            bool validSelection = validAccountStdSelection && validAccountInternalSelection;
            if (!validSelection)
            {
                accountStds.Clear();
                accountInternals.Clear();
            }
            return validSelection;
        }

        public bool GetAccountsInInterval(CompEntities entities, CreateReportResult reportResult, EconomyReportParamsDTO reportParams, AccountDimDTO accountDimStd, bool sieSelection, ref List<AccountDTO> accountStds, ref List<AccountInternalDTO> accountInternals)
        {
            //AccountStds
            accountStds = GetAccountStdsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, reportParams.OnlyActiveAccounts, out bool validAccountStdSelection);

            //AccountInternals
            bool onlyActiveInternAlaccount = false;
            if (!reportParams.OnlyActiveAccounts == null || reportParams.SSTD_SeparateAccountDim)
            {
                onlyActiveInternAlaccount = true;
            }
            accountInternals = GetAccountInternalsInInterval(entities, reportParams.SA_AccountIntervals, accountDimStd, reportResult.ActorCompanyId, onlyActiveInternAlaccount, sieSelection, out bool validAccountInternalSelection);

            if (reportParams.SSTD_AccountDimId != 0)
            {
                List<AccountInternalDTO> AccountIntervalsForDimId = accountInternals.Where(i => i.AccountDimId == reportParams.SSTD_AccountDimId).ToList();
                if (!AccountIntervalsForDimId.Any())
                {
                    List<AccountInternalDTO> separateInterval = AccountManager.GetAccountInternalsByDim(reportParams.SSTD_AccountDimId, reportResult.ActorCompanyId).ToDTOs();
                    accountInternals.AddRange(separateInterval);
                }
            }

            bool validSelection = validAccountStdSelection && validAccountInternalSelection;
            if (!validSelection)
            {
                accountStds.Clear();
                accountInternals.Clear();
            }
            return validSelection;
        }

        /// <summary>
        /// Get all AccountStds in the given interval.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="accountIntervals">The AccountIntervals</param>
        /// <param name="accountDimStd">The AccountDim std</param>
        /// <param name="onlyActive">If true, only active AccountStds are returned</param>
        /// <param name="valid">If interval was given, but gave no AccountStds, valid will be false. Otherwise true</param>
        /// <returns>Collection of AccountStd entities. If no interval is selected, all AccountStd are returned</returns>
        public List<AccountStd> GetAccountStdsInInterval(CompEntities entities, List<AccountIntervalDTO> accountIntervals, AccountDim accountDimStd, int actorCompanyId, bool? onlyActive, out bool valid)
        {
            valid = true;
            List<AccountStd> accountStdsInInterval = new List<AccountStd>();

            #region Prereq

            if (accountDimStd == null)
                accountDimStd = GetAccountDimStd(entities, actorCompanyId);
            if (accountIntervals == null)
                accountIntervals = new List<AccountIntervalDTO>();

            List<AccountStd> accountStds = GetAccountStdsByCompany(actorCompanyId, onlyActive);

            #endregion

            List<AccountIntervalDTO> validAccountIntervals = accountIntervals.Where(ai => ai.AccountDimId == accountDimStd.AccountDimId).ToList();
            if (validAccountIntervals.Count > 0)
            {
                foreach (AccountStd accountStd in accountStds)
                {
                    #region AccountStd

                    foreach (AccountIntervalDTO accountInterval in validAccountIntervals)
                    {
                        if (Validator.IsAccountInInterval(accountStd.Account.AccountNr, accountStd.Account.AccountDimId, accountInterval))
                        {
                            accountStdsInInterval.Add(accountStd);
                            break;
                        }
                    }

                    #endregion
                }

                //Invalid if interval gave no AccountStds
                if (accountStdsInInterval.Count == 0)
                    valid = false;
            }
            else
            {
                accountStdsInInterval.AddRange(accountStds);
            }

            return (from a in accountStdsInInterval
                    orderby a.Account.AccountNr ascending
                    select a).ToList();
        }

        public List<AccountDTO> GetAccountStdsInInterval(CompEntities entities, List<AccountIntervalDTO> accountIntervals, AccountDimDTO accountDimStd, int actorCompanyId, bool? onlyActive, out bool valid)
        {
            valid = true;
            List<AccountDTO> accountStdsInInterval = new List<AccountDTO>();

            #region Prereq

            if (accountDimStd == null)
                accountDimStd = GetAccountDimStd(entities, actorCompanyId).ToDTO();
            if (accountIntervals == null)
                accountIntervals = new List<AccountIntervalDTO>();

            List<AccountStd> accountStds = GetAccountStdsByCompany(actorCompanyId, onlyActive);
            List<AccountDTO> accountDTOs = new List<AccountDTO>();

            foreach (AccountStd account in accountStds)
            {
                AccountDTO accountDTO = account.Account.ToDTO();
                accountDTOs.Add(accountDTO);
            }

            #endregion

            List<AccountIntervalDTO> validAccountIntervals = accountIntervals.Where(ai => ai.AccountDimId == accountDimStd.AccountDimId).ToList();
            if (validAccountIntervals.Count > 0)
            {
                foreach (AccountDTO accountStd in accountDTOs)
                {
                    #region AccountStd

                    foreach (AccountIntervalDTO accountInterval in validAccountIntervals)
                    {
                        if (Validator.IsAccountInInterval(accountStd.AccountNr, accountStd.AccountDimId, accountInterval))
                        {
                            accountStdsInInterval.Add(accountStd);
                            break;
                        }
                    }

                    #endregion
                }

                //Invalid if interval gave no AccountStds
                if (accountStdsInInterval.Count == 0)
                    valid = false;
            }
            else
            {
                accountStdsInInterval.AddRange(accountDTOs);
            }

            return (accountStdsInInterval.OrderBy(a => a.AccountNr).ToList());
        }

        /// <summary>
        /// Get all AccountInternals in the given interval.
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountIntervals">The AccountIntervals</param>
        /// <param name="accountDimStd">The AccountDim std</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="onlyActive">If true, only active AccountInternals are returned</param>
        /// <param name="invalid">If interval was given, but gave no AccountInternals, valid will be false. Otherwise true</param>
        /// <returns>Collection of AccountInternal entities. If no interval is selected, all AccountInternals are returned</returns>
        public List<AccountInternal> GetAccountInternalsInInterval(CompEntities entities, List<AccountIntervalDTO> accountIntervals, AccountDim accountDimStd, int actorCompanyId, bool? onlyActive, bool sieSelection, out bool valid)
        {
            valid = true;
            List<AccountInternal> accountInternalsInInterval = new List<AccountInternal>();

            #region Prereq

            if (accountDimStd == null)
                accountDimStd = GetAccountDimStd(entities, actorCompanyId);
            if (accountIntervals == null)
                accountIntervals = new List<AccountIntervalDTO>();

            #endregion

            List<AccountIntervalDTO> validAccountIntervals = accountIntervals.Where(i => i.AccountDimId != accountDimStd.AccountDimId).ToList();
            if (validAccountIntervals.Count > 0)
            {
                List<AccountDim> accountDims = GetAccountDimInternalsByCompany(entities, actorCompanyId, onlyActive);
                foreach (AccountDim accountDim in accountDims)
                {
                    #region AccountDim

                    List<AccountIntervalDTO> accountIntervalsForDim = accountIntervals.Where(i => i.AccountDimId == accountDim.AccountDimId).ToList();
                    if (accountIntervalsForDim.Count > 0)
                    {
                        #region AccountInternal

                        List<AccountInternal> accountInternalsForDim = GetAccountInternalsByDim(entities, accountDim.AccountDimId, actorCompanyId, onlyActive);
                        foreach (AccountInternal accountInternal in accountInternalsForDim)
                        {
                            foreach (AccountIntervalDTO accountInterval in accountIntervalsForDim)
                            {
                                if (Validator.IsAccountInInterval(accountInternal.Account.AccountNr, accountInternal.Account.AccountDimId, accountInterval))
                                {
                                    accountInternalsInInterval.Add(accountInternal);
                                    break;
                                }
                            }
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                if (sieSelection)
                {
                    //Add all
                    List<AccountDim> accountDims = GetAccountDimInternalsByCompany(entities, actorCompanyId, onlyActive);
                    foreach (AccountDim accountDim in accountDims)
                    {
                        #region AccountInternal

                        accountInternalsInInterval.AddRange(GetAccountInternalsByDim(entities, accountDim.AccountDimId, actorCompanyId, onlyActive));

                        #endregion
                    }
                }
            }

            //Invalid if interval gave no AccountInternal
            if (accountInternalsInInterval.Count == 0 && sieSelection)
                valid = false;

            return (from a in accountInternalsInInterval
                    orderby a.Account.AccountDim.AccountDimNr ascending, a.Account.AccountNr ascending
                    select a).ToList();
        }

        public List<AccountInternalDTO> GetAccountInternalsInInterval(CompEntities entities, List<AccountIntervalDTO> accountIntervals, AccountDimDTO accountDimStd, int actorCompanyId, bool? onlyActive, bool sieSelection, out bool valid)
        {
            valid = true;
            List<AccountInternalDTO> accountInternalsInInterval = new List<AccountInternalDTO>();

            #region Prereq

            if (accountDimStd == null)
                accountDimStd = GetAccountDimStd(entities, actorCompanyId).ToDTO();
            if (accountIntervals == null)
                accountIntervals = new List<AccountIntervalDTO>();

            #endregion

            List<AccountIntervalDTO> validAccountIntervals = accountIntervals.Where(i => i.AccountDimId != accountDimStd.AccountDimId).ToList();
            if (validAccountIntervals.Count > 0)
            {
                List<AccountDimDTO> accountDims = GetAccountDimInternalsByCompany(entities, actorCompanyId, onlyActive).ToDTOs();
                foreach (AccountDimDTO accountDim in accountDims)
                {
                    #region AccountDim

                    List<AccountIntervalDTO> accountIntervalsForDim = accountIntervals.Where(i => i.AccountDimId == accountDim.AccountDimId).ToList();
                    if (accountIntervalsForDim.Count > 0)
                    {
                        #region AccountInternal

                        List<AccountInternalDTO> accountInternalsForDim = GetAccountInternalsByDim(entities, accountDim.AccountDimId, actorCompanyId, onlyActive).ToDTOs();
                        foreach (AccountInternalDTO accountInternal in accountInternalsForDim)
                        {
                            foreach (AccountIntervalDTO accountInterval in accountIntervalsForDim)
                            {
                                if (Validator.IsAccountInInterval(accountInternal.AccountNr, accountInternal.AccountDimId, accountInterval))
                                {
                                    accountInternalsInInterval.Add(accountInternal);
                                    break;
                                }
                            }
                        }

                        #endregion
                    }

                    #endregion
                }

                //Invalid if interval gave no AccountInternal
                if (accountInternalsInInterval.Count == 0)
                    valid = false;
            }
            else
            {
                if (sieSelection)
                {
                    //Add all
                    List<AccountDim> accountDims = GetAccountDimInternalsByCompany(entities, actorCompanyId, onlyActive);
                    foreach (AccountDim accountDim in accountDims)
                    {
                        #region AccountInternal

                        accountInternalsInInterval.AddRange(GetAccountInternalsByDim(entities, accountDim.AccountDimId, actorCompanyId, onlyActive).ToDTOs());

                        #endregion
                    }

                    //Invalid if interval gave no AccountInternal
                    if (accountInternalsInInterval.Count == 0)
                        valid = false;
                }
            }

            return (from a in accountInternalsInInterval
                    orderby a.AccountDimNr ascending, a.AccountNr ascending
                    select a).ToList();
        }


        #endregion

        #region VatCode

        public List<VatCodeGridDTO> GetVatCodeGridDTOs(int actorCompanyId, int? vatCodeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VatCode.NoTracking();
            return GetVatCodeGridDTOs(entities, actorCompanyId, vatCodeId);
        }
        public List<VatCodeGridDTO> GetVatCodeGridDTOs(CompEntities entities, int actorCompanyId, int? vatCodeId = null)
        {
            IQueryable<VatCode> query = (from v in entities.VatCode
                                         where v.ActorCompanyId == actorCompanyId &&
                                         v.State == (int)SoeEntityState.Active
                                         select v);

            if (vatCodeId != null)
            {
                query = query.Where(v => v.VatCodeId == vatCodeId);
            }

            return query.OrderBy(v => v.Code).Select(v => new VatCodeGridDTO
            {
                VatCodeId = v.VatCodeId,
                Code = v.Code,
                Name = v.Name,
                Percent = v.Percent,
                Account = v.AccountStd.Account.AccountNr + " " + v.AccountStd.Account.Name,
                PurchaseVATAccount = v.PurchaseVATAccountStd.Account.AccountNr + " " + v.PurchaseVATAccountStd.Account.Name
            }).ToList();
        }

        public List<VatCode> GetVatCodes(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VatCode.NoTracking();
            return GetVatCodes(entities, actorCompanyId);
        }

        public List<VatCode> GetVatCodes(CompEntities entities, int actorCompanyId)
        {
            return (from v in entities.VatCode.Include("AccountStd.Account").Include("PurchaseVATAccountStd.Account")
                    where v.ActorCompanyId == actorCompanyId &&
                    v.State == (int)SoeEntityState.Active
                    select v).OrderBy(v => v.Code).ToList();
        }

        public Dictionary<int, string> GetVatCodesDict(int actorCompanyId, bool addEmptyRow)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (addEmptyRow)
                dict.Add(0, " ");

            List<VatCode> vatCodes = GetVatCodes(actorCompanyId);
            foreach (VatCode vatCode in vatCodes)
            {
                dict.Add(vatCode.VatCodeId, vatCode.Code);
            }

            return dict;
        }

        public VatCode GetVatCode(int vatCodeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VatCode.NoTracking();
            return GetVatCode(entities, vatCodeId);
        }

        public VatCode GetVatCode(CompEntities entities, int vatCodeId)
        {
            return (from v in entities.VatCode
                    where v.VatCodeId == vatCodeId
                    select v).FirstOrDefault();
        }

        public VatCode GetVatCodeByCode(int actorCompanyId, string code)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VatCode.NoTracking();
            return GetVatCodeByCode(entities, actorCompanyId, code);
        }

        public VatCode GetVatCodeByCode(CompEntities entities, int actorCompanyId, string code)
        {
            return (from v in entities.VatCode.Include("AccountStd.Account").Include("PurchaseVATAccountStd.Account")
                    where v.ActorCompanyId == actorCompanyId &&
                    v.Code == code &&
                    v.State == (int)SoeEntityState.Active
                    select v).FirstOrDefault();
        }

        public VatCode GetVatCodeByVateRate(int actorCompanyId, decimal vatRate)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VatCode.NoTracking();
            return GetVatCodeByVateRate(entities, actorCompanyId, vatRate);
        }

        public VatCode GetVatCodeByVateRate(CompEntities entities, int actorCompanyId, decimal vatRate)
        {
            return (from v in entities.VatCode.Include("AccountStd.Account").Include("PurchaseVATAccountStd.Account")
                    where v.ActorCompanyId == actorCompanyId &&
                    v.Percent == vatRate &&
                    v.State == (int)SoeEntityState.Active
                    select v).FirstOrDefault();
        }

        public VatCode GetDefaultAccountingVatCode(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetDefaultAccountingVatCode(entities, actorCompanyId);
        }
        public VatCode GetDefaultAccountingVatCode(CompEntities entities, int actorCompanyId)
        {
            var vatCodeSetting = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingDefaultVatCode, 0, actorCompanyId, 0);
            return AccountManager.GetVatCode(entities, vatCodeSetting);
        }

        public VatCode GetDefaultBillingVatCode(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetDefaultBillingVatCode(entities, actorCompanyId);
        }
        public VatCode GetDefaultBillingVatCode(CompEntities entities, int actorCompanyId)
        {
            var vatCodeSetting = SettingManager.GetIntSetting(entities, SettingMainType.Company, (int)CompanySettingType.BillingDefaultVatCode, 0, actorCompanyId, 0);
            return AccountManager.GetVatCode(entities, vatCodeSetting);
        }

        private bool VatCodeNameExists(string code, string name, int actorCompanyId, int? vatCodeId = null)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.VatCode.NoTracking();
            IQueryable<VatCode> query = (from vc in entities.VatCode
                                         where vc.ActorCompanyId == actorCompanyId &&
                                         (vc.Code == code || vc.Name == name) &&
                                         vc.State != (int)SoeEntityState.Deleted
                                         select vc);
            if (vatCodeId.HasValue)
                query = query.Where(vc => vc.VatCodeId != vatCodeId.Value);

            return query.Any();
        }

        public ActionResult SaveVatCode(VatCodeDTO vatCodeInput, int actorCompanyId)
        {
            if (vatCodeInput == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "VatCode");

            int vatCodeId = vatCodeInput.VatCodeId;

            using (CompEntities entities = new CompEntities())
            {
                #region VatCode

                // Get existing vat code
                VatCode vatCode = GetVatCode(entities, vatCodeId);

                if (this.VatCodeNameExists(vatCodeInput.Code, vatCodeInput.Name, actorCompanyId, vatCode?.VatCodeId ?? null))
                {
                    return new ActionResult(GetText(92034, "Momskoden kunde inte sparas, kod och/eller namn finns redan."));
                }

                if (vatCode == null)
                {
                    #region VatCode Add

                    vatCode = new VatCode
                    {
                        ActorCompanyId = actorCompanyId,
                        State = (int)SoeEntityState.Active
                    };
                    SetCreatedProperties(vatCode);

                    entities.VatCode.AddObject(vatCode);

                    #endregion
                }
                else
                {
                    #region VatCode Update

                    SetModifiedProperties(vatCode);

                    #endregion
                }

                vatCode.Code = vatCodeInput.Code;
                vatCode.Name = vatCodeInput.Name;
                vatCode.Percent = vatCodeInput.Percent;
                vatCode.AccountId = vatCodeInput.AccountId;
                vatCode.PurchaseVATAccountId = vatCodeInput.PurchaseVATAccountId.ToNullable();

                #endregion

                ActionResult result = SaveChanges(entities);
                if (result.Success)
                    vatCodeId = vatCode.VatCodeId;

                result.IntegerValue = vatCodeId;
                return result;
            }
        }

        public ActionResult DeleteVatCode(int vatCodeId)
        {
            using (CompEntities entities = new CompEntities())
            {
                VatCode vatCode = GetVatCode(entities, vatCodeId);
                if (vatCode == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "VatCode");

                // Check relations
                int compVatCodeId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultVatCode, 0, vatCode.ActorCompanyId, 0);
                if (compVatCodeId == vatCodeId)
                    return new ActionResult((int)ActionResultDelete.VatCodeInUse_CompanySetting, GetText(3174, "Momskoden kunde inte tas bort, den används som standard momskod i företagsinställningarna."));

                if (ProductHasVatCode(vatCodeId))
                    return new ActionResult((int)ActionResultDelete.VatCodeInUse_Product, GetText(3175, "Momskoden kunde inte tas bort, den används på en eller flera artiklar."));

                if (CustomerInvoiceRowHasVatCode(vatCodeId))
                    return new ActionResult((int)ActionResultDelete.VatCodeInUse_InvoiceRow, GetText(3176, "Momskoden kunde inte tas bort, den används på en eller flera artikelrader."));

                return ChangeEntityState(entities, vatCode, SoeEntityState.Deleted, true);
            }
        }

        private bool ProductHasVatCode(int vatCodeId)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            entitiesReadOnly.Product.OfType<InvoiceProduct>().AsNoTracking();
            return (from p in entitiesReadOnly.Product.OfType<InvoiceProduct>()
                    where p.VatCodeId == vatCodeId
                    select p).Any();
        }

        private bool CustomerInvoiceRowHasVatCode(int vatCodeId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CustomerInvoiceRow.NoTracking();
            return (from r in entities.CustomerInvoiceRow
                    where r.VatCodeId == vatCodeId
                    select r).Any();
        }

        #endregion
    }
}
