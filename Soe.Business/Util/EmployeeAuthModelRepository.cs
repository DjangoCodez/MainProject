using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util
{
    public abstract class EmployeeAuthModelRepository
    {
        #region Variables

        public List<AttestRoleUser> AttestRoleUsers { get; set; }

        private readonly Dictionary<int, List<string>> authModelNamesByEmployee;
        protected bool addEmployeeAuthInfo;

        #endregion

        #region Ctor

        protected EmployeeAuthModelRepository(bool addEmployeeAuthInfo)
        {
            this.authModelNamesByEmployee = new Dictionary<int, List<string>>();
            this.addEmployeeAuthInfo = addEmployeeAuthInfo;
        }

        #endregion

        #region Public methods

        public List<AttestRole> GetAttestRolesForUser()
        {
            return this.AttestRoleUsers?.Select(i => i.AttestRole).Distinct().ToList() ?? new List<AttestRole>();
        }

        public virtual void SetEmployeeAuthNames(List<Employee> employees)
        {

        }

        #endregion

        #region Protected methods

        protected List<string> GetAuthModelNames(int employeeId)
        {
            return this.authModelNamesByEmployee.ContainsKey(employeeId) ? this.authModelNamesByEmployee[employeeId] : new List<string>();
        }

        protected void AddAuthModelNames(int employeeId, List<string> names)
        {
            if (!this.authModelNamesByEmployee.ContainsKey(employeeId))
                this.authModelNamesByEmployee.Add(employeeId, names);
        }

        #endregion
    }

    public class EmployeesRepositoryOutput
    {
        public List<Employee> Employees { get; private set; }
        public EmployeeAuthModelRepository EmployeeAuthModelRepository { get; private set; }

        public EmployeesRepositoryOutput(List<Employee> employees, EmployeeAuthModelRepository employeeAuthModelRepository)
        {
            this.Employees = employees;
            this.EmployeeAuthModelRepository = employeeAuthModelRepository;
        }
    }

    #region AccountRepository

    public class AccountRepositorySettings
    {
        public int? SelectorAccountDimId { get; set; }
        public int? EmployeeAccountDimId { get; set; }
        public bool UseLimitedEmployeeAccountDimLevels { get; set; }
        public bool UseExtendedEmployeeAccountDimLevels { get; set; }
        public bool IncludeOnlyChildrenOneLevel { get; set; }

        public AccountRepositorySettings() { }
        public AccountRepositorySettings(
            int? selectorAccountDimId,
            int? employeeAccountDimId,
            bool useLimitedEmployeeAccountDimLevels,
            bool useExtendedEmployeeAccountDimLevels,
            bool includeOnlyChildrenOneLevel
            )
        {
            this.SelectorAccountDimId = selectorAccountDimId;
            this.EmployeeAccountDimId = employeeAccountDimId;
            this.UseLimitedEmployeeAccountDimLevels = useLimitedEmployeeAccountDimLevels;
            this.UseExtendedEmployeeAccountDimLevels = useExtendedEmployeeAccountDimLevels;
            this.IncludeOnlyChildrenOneLevel = includeOnlyChildrenOneLevel;
        }
    }

    public class AccountRepository : EmployeeAuthModelRepository
    {
        #region Variables

        public List<AccountDTO> InputAccounts => inputAccounts;
        private readonly List<AccountDTO> inputAccounts;

        public List<AccountHierarchy> AccountHierarchys => accountHierarchys;
        private readonly List<AccountHierarchy> accountHierarchys;

        public AccountRepositorySettings Settings => settings;
        private readonly AccountRepositorySettings settings;

        public Dictionary<int, List<EmployeeAccount>> EmployeeAccounts { get; private set; }

        public List<AccountDimDTO> AllAccountDims { get; private set; }
        public List<AccountDTO> AllAccountInternals { get; private set; }

        private Dictionary<int, AccountDTO> allAccountInternalsDict;
        public Dictionary<int, AccountDTO> AllAccountInternalsDict
        {
            get
            {
                if (this.allAccountInternalsDict == null)
                    this.allAccountInternalsDict = this.AllAccountInternals.ToDict();
                return this.allAccountInternalsDict;
            }
        }

        private Dictionary<int, List<AccountDTO>> allAccountInternalsByDim;
        public Dictionary<int, List<AccountDTO>> AllAccountInternalsByDim
        {
            get
            {
                if (this.allAccountInternalsByDim == null)
                    this.allAccountInternalsByDim = this.AllAccountInternals.OrderBy(i => i.Name).GroupBy(a => a.AccountDimId).ToDictionary(k => k.Key, v => v.ToList());
                return this.allAccountInternalsByDim;
            }
        }

        public int? MaxSelectorAccountDimId => maxSelectorAccountDimId;
        private readonly int? maxSelectorAccountDimId;

        public int? MaxEmployeeAccountDimId => maxEmployeeAccountDimId;
        private readonly int? maxEmployeeAccountDimId;

        public AccountHierarchy CurrentHierarchy => currentHierarchy;
        private readonly AccountHierarchy currentHierarchy;

        public List<AccountDimDTO> CurrentAccountHiearchyHandledAccountDims => currentAccountHiearchyHandledAccountDims;
        private readonly List<AccountDimDTO> currentAccountHiearchyHandledAccountDims;

        private string userSettingAccountHierarchyId;
        public bool HasNoHiearchySetting => string.IsNullOrEmpty(this.userSettingAccountHierarchyId) || this.userSettingAccountHierarchyId.Equals("0");

        #endregion

        #region Ctor

        public AccountRepository(List<AttestRoleUser> attestRoleUsers, List<AccountDimDTO> allAccountDims, List<AccountDTO> allAccountInternals, List<AccountDTO> inputAccounts, AccountRepositorySettings settings = null) : base(false)
        {
            this.AttestRoleUsers = attestRoleUsers;
            this.AllAccountDims = allAccountDims;
            this.AllAccountInternals = allAccountInternals;
            this.inputAccounts = inputAccounts;
            this.accountHierarchys = new List<AccountHierarchy>();
            this.currentAccountHiearchyHandledAccountDims = new List<AccountDimDTO>();
            this.settings = settings ?? new AccountRepositorySettings();
            this.maxSelectorAccountDimId = CalculateSelectorMaxAccountDimId();
            this.maxEmployeeAccountDimId = CalculateEmployeeMaxAccountDimId();

            if (inputAccounts != null)
            {
                this.AllAccountDims.CalculateLevels();

                foreach (AccountDTO inputAccount in inputAccounts)
                {
                    if (this.accountHierarchys.Exists(h => h.ContainsAccountAsAbstract(inputAccount.AccountId)))
                        continue;

                    this.currentHierarchy = new AccountHierarchy(inputAccount);
                    ResetHiearachy();
                    AddInputAccountToHiearchy();
                    BuildHierarchy(this.AllAccountDims.GetAccountDim(inputAccount));
                    this.accountHierarchys.Add(this.currentHierarchy);
                }
            }
        }

        public AccountRepository(List<AttestRoleUser> attestRoleUsers, List<AccountDimDTO> allAccountDims, List<AccountDTO> allAccountInternals, List<EmployeeAccount> employeeAccounts, bool addAccountInfo = false) : base(addAccountInfo)
        {
            this.AttestRoleUsers = attestRoleUsers;
            this.AllAccountDims = allAccountDims;
            this.AllAccountInternals = allAccountInternals;
            this.SetEmployeeAccounts(employeeAccounts);
        }

        #endregion

        #region Public methods

        public AccountRepository Clone()
        {
            var clone = new AccountRepository(
                this.AttestRoleUsers?.Select(a => a).ToList(),
                this.AllAccountDims?.Select(a => a).ToList(),
                this.AllAccountInternals?.Select(a => a).ToList(),
                this.InputAccounts?.Select(a => a).ToList(),
                this.Settings != null
                ? new AccountRepositorySettings(
                    this.Settings.SelectorAccountDimId,
                    this.Settings.EmployeeAccountDimId,
                    this.Settings.UseLimitedEmployeeAccountDimLevels,
                    this.Settings.UseExtendedEmployeeAccountDimLevels,
                    this.Settings.IncludeOnlyChildrenOneLevel)
                : null
            );

            clone.SetEmployeeAccounts(this.EmployeeAccounts?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Select(ea => ea).ToList())); //Deep clone
            clone.SetAddAccountInfo(this.addEmployeeAuthInfo);
            clone.SetUserSettingAccountHierarchyId(this.userSettingAccountHierarchyId);

            return clone;
        }

        private List<AccountDTO> cacheGetAccounts = null;
        public List<AccountDTO> GetAccounts(bool? includeVirtualParented = false)
        {
            if (this.cacheGetAccounts == null)
                this.cacheGetAccounts = GetAccountsFromHierarchies(includeVirtualParented == true)
                    .Where(i => i.AccountDim != null)
                    .OrderByAccount();
            return this.cacheGetAccounts;
        }

        private List<AccountDTO> cacheGetAccountsWithAbstract = null;
        public List<AccountDTO> GetAccountsWithAbstract(bool? includeVirtualParented = false)
        {
            if (this.cacheGetAccountsWithAbstract == null)
            {
                this.cacheGetAccountsWithAbstract = GetAccountsFromHierarchies(includeVirtualParented == true, includeAbstract: true)
                    .Where(i => i.AccountDim != null)
                    .OrderByAccount();
            }
            return this.cacheGetAccountsWithAbstract.ToList();
        }

        private Dictionary<int, AccountDTO> cacheGetAccountsDict = null;
        public Dictionary<int, AccountDTO> GetAccountsDict(bool? includeVirtualParented = false)
        {
            if (this.cacheGetAccountsDict == null)
            {
                this.cacheGetAccountsDict = new Dictionary<int, AccountDTO>();
                foreach (AccountDTO account in GetAccountsWithAbstract(includeVirtualParented))
                {
                    if (!this.cacheGetAccountsDict.ContainsKey(account.AccountId))
                        this.cacheGetAccountsDict.Add(account.AccountId, account);
                }
                return this.cacheGetAccountsDict;

            }
            return this.cacheGetAccountsDict.ToDictionary(k => k.Key, v => v.Value);
        }

        public Dictionary<string, string> GetAccountStrings()
        {
            return GetAccountsFromHierarchies(includeVirtualParented: true).GetHierarchies();
        }

        public List<int> GetAccountDimIdsAboveEmployeeAccountDim(int maxEmployeeAccountDimId)
        {
            if (maxEmployeeAccountDimId <= 0 || this.AllAccountDims.IsNullOrEmpty() || this.AllAccountDims.All(a => a.Level == 0))
                return null;

            AccountDimDTO maxAccountDimId = this.AllAccountDims.FirstOrDefault(a => a.AccountDimId == maxEmployeeAccountDimId);
            if (maxAccountDimId == null)
                return null;

            return this.AllAccountDims.Where(a => a.Level > maxAccountDimId.Level).OrderBy(a => a.Level).Select(a => a.AccountDimId).ToList();
        }

        public List<AccountDTO> GetDefaultAccountsFromEmployeeAccounts()
        {
            if (this.EmployeeAccounts.IsNullOrEmpty())
                return new List<AccountDTO>();

            List<int> accountIds = this.EmployeeAccounts
                .SelectMany(e => e.Value)
                .Where(e => e.AccountId.HasValue && e.Default)
                .Select(a => a.AccountId.Value)
                .Distinct()
                .ToList();

            return this.AllAccountInternals.Where(a => accountIds.Contains(a.AccountId)).ToList();
        }

        public List<AccountDTO> GetSubAccounts(int parentAccountId, bool? includeVirtualParented = false)
        {
            return GetAccountsFromHierarchies(includeVirtualParented == true)
                .Where(i => i.AccountDim != null && i.ParentAccountId == parentAccountId)
                .OrderByAccount();
        }

        public List<EmployeeAccount> GetEmployeeAccounts()
        {
            return this.EmployeeAccounts?.SelectMany(e => e.Value).ToList();
        }

        public List<EmployeeAccount> GetEmployeeAccounts(int employeeId)
        {
            return this.EmployeeAccounts.GetList(employeeId);
        }

        public List<EmployeeAccount> GetEmployeeAccounts(int employeeId, DateTime dateFrom, DateTime dateTo, bool onlyDefaultAccounts = false)
        {
            return this.GetEmployeeAccounts(employeeId).GetEmployeeAccountsByAccount(employeeId, null, dateFrom, dateTo, onlyDefaultAccounts);
        }

        public void ClearEmployeeAccounts()
        {
            this.EmployeeAccounts = null;
        }

        public bool IsUserPermittedToSeeEmployee(
            int employeeId,
            List<EmployeeAccount> employeeAccounts,
            Employee currentUserEmployee,
            List<EmployeeAccount> currentUserEmployeeAccounts,
            DateTime startDate,
            DateTime stopDate,
            List<int> validEmployeeIdsByAttestRoleUserAccountIds = null,
            bool hasShowOtherEmployeesPermission = false,
            bool onlyDefaultAccounts = false,
            bool getHidden = false,
            int? hiddenEmployeeId = null
            )
        {
            // Check current User
            if (currentUserEmployee?.EmployeeId == employeeId)
                return true;

            // Check hidden
            if (getHidden && hiddenEmployeeId == employeeId)
                return true;

            // Check attest role colleagues on same account
            if (validEmployeeIdsByAttestRoleUserAccountIds != null && validEmployeeIdsByAttestRoleUserAccountIds.Contains(employeeId))
                return true;

            // Check User attest accounts
            if (this.HasAnyAttestRoleAnyAccount(employeeId, employeeAccounts, startDate, stopDate, onlyDefaultAccounts))
                return true;

            // Check employee colleagues permission
            if (hasShowOtherEmployeesPermission && currentUserEmployeeAccounts.ContainsAny(employeeAccounts))
                return true;

            return false;
        }

        public bool HasAnyAttestRoleAnyAccount(
            int employeeId,
            List<EmployeeAccount> employeeAccountsForEmployee,
            DateTime startDate,
            DateTime stopDate,
            bool onlyDefaultAccounts
            )
        {
            if (employeeAccountsForEmployee.IsNullOrEmpty())
            {
                return this.AttestRoleUsers.ShowUncategorized(startDate) && this.HasNoHiearchySetting;
            }
            else
            {
                if (this.AttestRoleUsers.Filter(startDate, stopDate).IsNullOrEmpty())
                    return false;

                bool showAll = this.AttestRoleUsers.ShowAll(startDate);
                string showAllUnderAccountHierarchyId = showAll ? this.userSettingAccountHierarchyId : "";
                var userPermittedAccounts = this.GetAccountsDict(true);

                return !employeeAccountsForEmployee
                    .GetValidAccounts(
                        employeeId,
                        startDate,
                        stopDate,
                        this.AllAccountInternalsDict,
                        userPermittedAccounts,
                        onlyDefaultAccounts: onlyDefaultAccounts,
                        showAllUnderAccountHierarchyId: showAllUnderAccountHierarchyId)
                    .IsNullOrEmpty();
            }
        }

        public void AddEmployeeAuthInfo(int employeeId, List<EmployeeAccount> employeeAccountsForEmployee, DateTime dateFrom, DateTime dateTo)
        {
            if (!base.addEmployeeAuthInfo)
                return;

            base.AddAuthModelNames(employeeId, employeeAccountsForEmployee.GetAccountNames(this.AllAccountInternals, dateFrom, dateTo));
        }

        public void SetUserSettingAccountHierarchyId(string userSettingAccountHierarchyId)
        {
            this.userSettingAccountHierarchyId = userSettingAccountHierarchyId;
        }

        public void SetAddAccountInfo(bool accAccountInfo)
        {
            base.addEmployeeAuthInfo = accAccountInfo;
        }

        public void SetEmployeeAccounts(List<EmployeeAccount> employeeAccounts)
        {
            this.EmployeeAccounts = employeeAccounts?.ToDict();
        }

        public void SetEmployeeAccounts(Dictionary<int, List<EmployeeAccount>> employeeAccounts)
        {
            this.EmployeeAccounts = employeeAccounts;
        }

        public void TrimEmployeeAccounts(List<int> employeeIds)
        {
            if (!this.EmployeeAccounts.IsNullOrEmpty())
                this.EmployeeAccounts = this.EmployeeAccounts.Where(i => employeeIds.Contains(i.Key)).ToDictionary(d => d.Key, d => d.Value);
        }

        public override void SetEmployeeAuthNames(List<Employee> employees)
        {
            if (!this.addEmployeeAuthInfo || employees.IsNullOrEmpty())
                return;

            foreach (Employee employee in employees)
            {
                employee.AccountNames = base.GetAuthModelNames(employee.EmployeeId);
            }
        }

        #endregion

        #region Private methods

        private List<AccountDTO> GetAccountsFromHierarchies(bool includeVirtualParented, bool includeAbstract = false)
        {
            if (this.accountHierarchys == null)
                return new List<AccountDTO>();

            var accountsDict = new Dictionary<int, AccountDTO>();
            foreach (var hierarchy in this.accountHierarchys)
            {
                var allAccounts = hierarchy.GetAllAccounts();
                foreach (var allAccountsById in allAccounts.GroupBy(a => a.AccountId))
                {
                    accountsDict.TryGetValue(allAccountsById.Key, out AccountDTO account);
                    foreach (var validAccount in allAccountsById)
                    {
                        if (account != null)
                        {
                            account.AddAccountHierarchy(validAccount.HierachyId, validAccount.HierachyName, validAccount.ParentHierachy, validAccount.ParentAccountId);
                        }
                        else
                        {
                            accountsDict.Add(validAccount.AccountId, validAccount);
                            account = validAccount;
                        }
                    }
                }
            }

            var accounts = accountsDict.Values.ToList();
            if (!includeAbstract)
                accounts = accounts.Where(i => !i.IsAbstract).ToList();
            if (!includeVirtualParented)
                accounts = accounts.Where(i => !i.HasVirtualParent).ToList();

            return accounts;
        }

        private void ResetHiearachy()
        {
            this.currentAccountHiearchyHandledAccountDims.Clear();
        }

        private void BuildHierarchy(AccountDimDTO parentAccountDim)
        {
            if (parentAccountDim == null)
                return;

            this.currentAccountHiearchyHandledAccountDims.Add(parentAccountDim);

            if (IsMaxAccountDimIdReached(parentAccountDim))
                return;

            List<AccountDTO> parentAccountsForParentDim = null;

            foreach (AccountDimDTO accountDim in parentAccountDim.GetChildrensByName(this.AllAccountDims))
            {
                if (this.currentAccountHiearchyHandledAccountDims.Exists(i => i.AccountDimId == accountDim.AccountDimId))
                    continue;
                if (!this.AllAccountInternalsByDim.ContainsKey(accountDim.AccountDimId))
                    continue;

                List<AccountDTO> accounts = this.AllAccountInternalsByDim[accountDim.AccountDimId];
                if (!accounts.IsNullOrEmpty())
                {
                    foreach (AccountDTO account in accounts)
                    {
                        if (this.currentHierarchy.ContainsAccount(account.AccountId))
                            continue;

                        if (account.ParentAccountId.HasValue)
                        {
                            List<AccountDTO> parentAccounts = this.currentHierarchy.GetAccounts(account.ParentAccountId.Value).ForAccountDim(parentAccountDim.AccountDimId);
                            AddAccountsToHierarchy(account, parentAccounts);
                        }
                        else if (accountDim.AllowAccountsWithoutParent)
                        {
                            if (parentAccountsForParentDim == null)
                                parentAccountsForParentDim = this.currentHierarchy.GetAllAccounts().ForAccountDim(parentAccountDim.AccountDimId).ByName();

                            AddClonedAccountsToHiearchy(account, parentAccountsForParentDim);
                        }
                    }
                }

                if (this.settings.IncludeOnlyChildrenOneLevel)
                    return;
                if (this.currentHierarchy.GetAllAccounts().ContainsAccountDim(accountDim.AccountDimId))
                    BuildHierarchy(accountDim);
            }
        }

        private void AddInputAccountToHiearchy()
        {
            AccountDTO prevAccount = null;
            if (this.currentHierarchy.InputAccount.ParentAccountId.HasValue)
            {
                List<AccountDTO> parentAccounts = this.currentHierarchy.InputAccount.GetParentAccounts(this.AllAccountInternals);
                AccountDTO currentAccount = parentAccounts.FirstOrDefault(i => !i.ParentAccountId.HasValue);
                while (currentAccount != null)
                {
                    AddAccountToHierarchy(currentAccount, prevAccount, isAbstract: !this.inputAccounts.Any(a => a.AccountId == currentAccount.AccountId));
                    prevAccount = currentAccount;
                    currentAccount = parentAccounts.FirstOrDefault(i => i.ParentAccountId == currentAccount.AccountId);
                }
            }

            //Do not add input accounts on settting EmployeeAccountDimId as abstract
            AddAccountToHierarchy(this.currentHierarchy.InputAccount, prevAccount, isAbstract: this.settings.IncludeOnlyChildrenOneLevel && (!this.settings.EmployeeAccountDimId.HasValue || this.settings.EmployeeAccountDimId.Value != this.currentHierarchy.InputAccount.AccountDimId));
        }

        private void AddAccountsToHierarchy(AccountDTO account, List<AccountDTO> parentAccounts)
        {
            if (parentAccounts.IsNullOrEmpty())
                return;

            foreach (AccountDTO parentAccount in parentAccounts)
            {
                AddAccountToHierarchy(account, parentAccount);
            }
        }
        private void AddAccountToHierarchy(AccountDTO account, AccountDTO parentAccount, bool isAbstract = false)
        {
            if (account == null)
                return;

            bool doAddParentAccount = parentAccount == null && account.TryGetVirtualParent(this.AllAccountInternals, out parentAccount);
            bool doCloneVirtualAccount = !account.ParentAccountId.HasValue && account.VirtualParentAccountId.HasValue && this.accountHierarchys.Any(h => h.InputAccount.AccountId == account.AccountId);
            if (doCloneVirtualAccount)
                account = account.CloneDTO();

            account.IsAbstract = isAbstract;
            account.AccountDim = this.AllAccountDims.FirstOrDefault(i => i.AccountDimId == account.AccountDimId);
            account.SetAccountHierarchy(parentAccount);
            this.currentHierarchy.AddAccount(account);

            if (doAddParentAccount)
                AddAccountToHierarchy(parentAccount, null, isAbstract: true);
        }

        private void AddClonedAccountsToHiearchy(AccountDTO account, List<AccountDTO> parentAccounts)
        {
            if (account == null || parentAccounts.IsNullOrEmpty())
                return;

            foreach (AccountDTO parentAccount in parentAccounts)
            {
                AccountDTO accountClone = new AccountDTO()
                {
                    AccountId = account.AccountId,
                    ParentAccountId = parentAccount.AccountId,
                    AccountDimId = account.AccountDimId,
                    AccountNr = account.AccountNr,
                    Name = account.Name,
                    Description = account.Description,
                    ExternalCode = account.ExternalCode,
                    HasVirtualParent = true,
                    HierarchyNotOnSchedule = account.HierarchyNotOnSchedule
                };
                AddAccountToHierarchy(accountClone, parentAccount);
            }
        }

        private int? CalculateSelectorMaxAccountDimId()
        {
            return this.settings.SelectorAccountDimId.ToNullable();
        }

        private int? CalculateEmployeeMaxAccountDimId()
        {
            if (this.settings.EmployeeAccountDimId <= 0)
                return null;

            AccountDimDTO accountDim = this.AllAccountDims.FirstOrDefault(i => i.AccountDimId == this.settings.EmployeeAccountDimId);
            if (accountDim == null)
                return null;

            //Limited
            if (this.settings.UseLimitedEmployeeAccountDimLevels)
                return accountDim.AccountDimId;

            int? parentAcountDimId = this.AllAccountDims.FirstOrDefault(i => i.ParentAccountDimId == accountDim.AccountDimId)?.AccountDimId;

            //Extended
            if (parentAcountDimId.HasValue && this.settings.UseExtendedEmployeeAccountDimLevels)
                parentAcountDimId = this.AllAccountDims.FirstOrDefault(i => i.ParentAccountDimId == parentAcountDimId.Value)?.AccountDimId;

            return parentAcountDimId;
        }

        private bool IsMaxAccountDimIdReached(AccountDimDTO accountDim)
        {
            return
                accountDim == null ||
                (this.maxSelectorAccountDimId.HasValue && this.settings.SelectorAccountDimId.Value == accountDim.AccountDimId) ||
                (this.maxEmployeeAccountDimId.HasValue && this.maxEmployeeAccountDimId.Value == accountDim.AccountDimId);
        }

        #endregion
    }

    public class AccountRepositoryCache
    {
        #region Variables

        private Dictionary<AccountRepositoryCacheItem, AccountRepository> cachedHierachyRepository = new Dictionary<AccountRepositoryCacheItem, AccountRepository>();
        private readonly object cacheLock = new object();

        #endregion

        #region Public methods

        public AccountRepository Get(int actorCompanyId, int? userId, DateTime? dateFrom, DateTime? dateTo, AccountHierarchyInput input)
        {
            if (this.cachedHierachyRepository.IsNullOrEmpty())
                return null;

            var cacheItem = new AccountRepositoryCacheItem(actorCompanyId, userId, dateFrom, dateTo, input);
            var repository = GetRepository(cacheItem);
            if (repository != null)
                return repository.Clone(); //Clone to avoid changes in the original repository
            return repository;
        }

        public void Add(AccountRepository accountRepository, int actorCompanyId, int? userId, DateTime? dateFrom, DateTime? dateTo, AccountHierarchyInput input)
        {
            if (accountRepository == null)
                return;

            var cacheItem = new AccountRepositoryCacheItem(actorCompanyId, userId, dateFrom, dateTo, input);
            AddRepository(accountRepository, cacheItem);
        }

        #endregion

        #region Private methods

        private AccountRepository GetRepository(AccountRepositoryCacheItem cacheItem)
        {
            if (cacheItem == null)
                return null;

            lock (cacheLock)
            {
                return this.cachedHierachyRepository?.FirstOrDefault(r => r.Key.IsIdentical(cacheItem)).Value;
            }
        }

        private void AddRepository(AccountRepository accountRepository, AccountRepositoryCacheItem cacheItem)
        {
            if (cacheItem == null)
                return;

            lock (cacheLock)
            {
                if (this.cachedHierachyRepository == null)
                    this.cachedHierachyRepository = new Dictionary<AccountRepositoryCacheItem, AccountRepository>();
                if (!HasRepository(cacheItem))
                    this.cachedHierachyRepository.Add(cacheItem, accountRepository);
            }
        }

        private bool HasRepository(AccountRepositoryCacheItem cacheItem)
        {
            return GetRepository(cacheItem) != null;
        }

        #endregion
    }

    public class AccountRepositoryCacheItem
    {
        #region Variables

        public int ActorCompanyId { get; }
        public int? UserId { get; }
        public DateTime? DateFrom { get; }
        public DateTime? DateTo { get; }
        public AccountHierarchyInput Input { get; }

        #endregion

        #region Ctor

        public AccountRepositoryCacheItem(int actorCompanyId, int? userId, DateTime? dateFrom, DateTime? dateTo, AccountHierarchyInput input)
        {
            this.ActorCompanyId = actorCompanyId;
            this.UserId = userId;
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.Input = input;
        }

        #endregion

        #region Public methods

        public bool IsIdentical(AccountRepositoryCacheItem cacheItem)
        {
            if (cacheItem == null)
                return false;
            if (!this.UserId.ToNullable().HasValue || !cacheItem.UserId.ToNullable().HasValue)
                return false;
            if (this.ActorCompanyId != cacheItem.ActorCompanyId)
                return false;
            if (this.UserId != cacheItem.UserId)
                return false;
            if (this.DateFrom != cacheItem.DateFrom)
                return false;
            if (this.DateTo != cacheItem.DateTo)
                return false;
            if ((this.Input != null) != (cacheItem.Input != null))
                return false;
            if (this.Input != null && !this.Input.IsIdentical(cacheItem.Input))
                return false;

            return true;
        }

        #endregion
    }

    #endregion

    #region CategoryRepository

    public class CategoryRepository : EmployeeAuthModelRepository
    {
        #region Variables

        public List<CompanyCategoryRecord> CategoryRecords { get; }

        private readonly Dictionary<int, List<CompanyCategoryRecord>> categoryRecordsByAttestrole;

        #endregion

        #region Ctor

        public CategoryRepository(List<AttestRoleUser> attestRoleUsers, List<CompanyCategoryRecord> categoryRecords, bool addCategoryInfo = false) : base(addCategoryInfo)
        {
            this.AttestRoleUsers = attestRoleUsers ?? new List<AttestRoleUser>();
            this.CategoryRecords = categoryRecords ?? new List<CompanyCategoryRecord>();

            this.categoryRecordsByAttestrole = new Dictionary<int, List<CompanyCategoryRecord>>();
        }

        #endregion

        #region Public methods

        public bool HasAnyAttestRoleAnyCategory(List<CompanyCategoryRecord> categoryRecordsForEmployee, DateTime? date = null)
        {
            if (categoryRecordsForEmployee.IsNullOrEmpty())
            {
                return this.AttestRoleUsers.ShowUncategorized(date);
            }
            else
            {
                if (this.AttestRoleUsers.Filter(date).IsNullOrEmpty())
                    return false;
                if (this.AttestRoleUsers.ShowAll(date))
                    return true;

                foreach (CompanyCategoryRecord employeeCategoryRecord in categoryRecordsForEmployee)
                {
                    if (HasAnyAttestRoleCategory(employeeCategoryRecord.CategoryId, date))
                        return true;
                }
            }

            return false;
        }

        public bool HasAnyAttestRoleCategory(int categoryId, DateTime? date = null)
        {
            List<AttestRole> attestRoles = base.GetAttestRolesForUser();
            if (attestRoles == null)
                return false;

            foreach (AttestRole attestRole in attestRoles)
            {
                if (HasAttestRoleCategory(attestRole.AttestRoleId, categoryId, date))
                    return true;
            }

            return false;
        }

        public bool HasAttestRoleCategory(int attestRoleId, int categoryId, DateTime? date = null)
        {
            if (this.CategoryRecords == null)
                return false;

            if (!this.categoryRecordsByAttestrole.ContainsKey(attestRoleId))
                this.categoryRecordsByAttestrole.Add(attestRoleId, this.CategoryRecords.GetCategoryRecords(SoeCategoryRecordEntity.AttestRole, attestRoleId));
            return this.categoryRecordsByAttestrole[attestRoleId].Any(i => i.CategoryId == categoryId && (!date.HasValue || i.IsDateValid(date.Value)));
        }

        public void AddEmployeeAuthInfo(int employeeId, List<CompanyCategoryRecord> categoryRecordsForEmployee)
        {
            if (!base.addEmployeeAuthInfo)
                return;

            base.AddAuthModelNames(employeeId, categoryRecordsForEmployee.GetCategoryNames());
        }

        public override void SetEmployeeAuthNames(List<Employee> employees)
        {
            if (!this.addEmployeeAuthInfo || employees.IsNullOrEmpty())
                return;

            foreach (Employee employee in employees)
            {
                employee.CategoryNames = base.GetAuthModelNames(employee.EmployeeId);
            }
        }

        #endregion
    }

    #endregion
}
