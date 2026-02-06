using SoftOne.Soe.Business.Core.Voucher;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Transactions;

namespace SoftOne.Soe.Business.Core
{
    public class AccountBalanceManager : ManagerBase
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public AccountBalanceManager(ParameterObject parameterObject, int actorCompanyId) : base(parameterObject)
        {
            cachedActorCompanyId = actorCompanyId;

            accountYearBalanceHeadCache = new Hashtable();
            voucherHeadCache = new Hashtable();
        }

        #endregion

        #region Cache

        private readonly VoucherHeadDTOCache voucherHeadDTOCache = new VoucherHeadDTOCache();

        private readonly int cachedActorCompanyId;
        public bool InstanceIsValid(int actorCompanyId)
        {
            return cachedActorCompanyId == actorCompanyId;
        }

        /// <summary>Caches AccountYearBalanceHeads on AccountYearId</summary>
        private readonly Hashtable accountYearBalanceHeadCache;

        /// <summary>Caches VoucherHeadCache on AccountYearId</summary>
        private readonly Hashtable voucherHeadCache;

        /// <summary>
        /// Use when current ActorCompanyId differs from the cached ActorCompanyId
        /// </summary>
        private void ClearCache()
        {
            accountYearBalanceHeadCache.Clear();
            voucherHeadCache.Clear();
        }

        private BalanceChangeVoucherHeadCache balanceChangeVoucherHeadCache = null;

        public void InitBalanceChangeVoucherHeadCache()
        {
            this.balanceChangeVoucherHeadCache = new BalanceChangeVoucherHeadCache();
        }

        private BalanceChangeVoucherHeadDTOCache balanceChangeVoucherHeadDTOCache = null;

        public void InitBalanceChangeVoucherHeadDTOCache()
        {
            this.balanceChangeVoucherHeadDTOCache = new BalanceChangeVoucherHeadDTOCache();
        }

        #endregion

        #region AccountYearBalanceHead

        public List<AccountYearBalanceHead> GetAccountYearBalanceHeads(int accountYearId, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYearBalanceHead.NoTracking();
            return GetAccountYearBalanceHeads(entities, accountYearId, actorCompanyId);
        }

        public List<AccountYearBalanceHead> GetAccountYearBalanceHeads(CompEntities entities, int accountYearId, int actorCompanyId)
        {
            List<AccountYearBalanceHead> balanceHeads = (from bh in entities.AccountYearBalanceHead
                                                            .Include("AccountYear")
                                                            .Include("AccountStd.Account")
                                                            .Include("AccountInternal.Account.AccountDim")
                                                         where bh.AccountYear.AccountYearId == accountYearId &&
                                                         bh.AccountYear.ActorCompanyId == actorCompanyId
                                                         select bh).OrderBy(x => x.AccountStd.Account.AccountNr).ToList();

            // Get account types
            var accountTypes = base.GetTermGroupDict(TermGroup.AccountType);

            foreach (AccountYearBalanceHead balanceHead in balanceHeads)
            {
                // Account for AccountStd
                if (balanceHead.AccountStd != null && !balanceHead.AccountStd.AccountReference.IsLoaded)
                    balanceHead.AccountStd.AccountReference.Load();

                balanceHead.AccountTypeName = balanceHead.AccountStd != null && accountTypes.Keys.Contains(balanceHead.AccountStd.AccountTypeSysTermId) ? accountTypes[balanceHead.AccountStd.AccountTypeSysTermId] : "";
            }

            return balanceHeads;
        }

        public ActionResult GetAccountYearBalanceHeadsForPreviousYear(int accountYearId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region prereq

                // Get account types
                var accountTypes = base.GetTermGroupDict(TermGroup.AccountType);

                AccountYear accountYear = AccountManager.GetAccountYear(entities, accountYearId, true);
                if (accountYear == null)
                    return new ActionResult((int)ActionResultSave.AccountYearNotFound, "AccountYear");

                AccountYear previousAccountYear = AccountManager.GetPreviousAccountYear(entities, accountYear.From, accountYear.ActorCompanyId, true);
                if (previousAccountYear == null)
                    return new ActionResult((int)ActionResultSave.AccountYearNotFound, "PreviousAccountYear");

                List<AccountStd> accountStdsForCompany = AccountManager.GetAccountStdsByCompany(entities, base.ActorCompanyId, null, true, false);
                List<AccountStd> accountStds = accountStdsForCompany.Where(i => i.AccountTypeSysTermId == (int)TermGroup_AccountType.Asset || i.AccountTypeSysTermId == (int)TermGroup_AccountType.Debt).ToList();

                //Get AccountInternal's
                List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, base.ActorCompanyId, true);

                //Get AccountDim internals
                List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(entities, base.ActorCompanyId);

                //Get old balances
                List<AccountYearBalanceHead> previousYearBalanceHeads = GetAccountYearBalanceHeads(entities, previousAccountYear.AccountYearId, base.ActorCompanyId);

                //Get balance
                var balanceChangesDict = GetBalanceChange(entities, previousAccountYear, previousAccountYear.From, previousAccountYear.To, accountStds, accountDimInternals, accountInternals, base.ActorCompanyId, true);

                #endregion

                #region Perform

                decimal sum = 0;
                List<AccountYearBalanceHead> heads = new List<AccountYearBalanceHead>();
                foreach (var pair in balanceChangesDict)
                {
                    int accountId = pair.Key;
                    BalanceItemDTO balanceItem = pair.Value;

                    var previousYearBalanceHeadLists = previousYearBalanceHeads.Where(i => i.AccountStd.AccountId == accountId).ToList();

                    if (previousYearBalanceHeadLists.Count > 0)
                    {
                        foreach (var previousYearItem in previousYearBalanceHeadLists)
                        {
                            decimal IB = previousYearItem.Balance;
                            decimal UB = 0;
                            decimal IBQuant = previousYearItem.Quantity.HasValue ? (decimal)previousYearItem.Quantity : 0;
                            decimal UBQuant = 0;

                            var internAccountIds = previousYearItem.AccountInternal.Select(x => x.AccountId).ToList();
                            if (internAccountIds.Any())
                            {
                                //Find matching current balance with same internal accounts as IB
                                var internAccountBalance = balanceItem.BalanceItemInternals.FirstOrDefault(x => x.AccountInternals.Count(y => internAccountIds.Contains(y.AccountId)) == internAccountIds.Count);
                                if (internAccountBalance != null)
                                {
                                    UB = internAccountBalance.Balance;
                                    UBQuant = internAccountBalance.Quantity.HasValue ? (decimal)internAccountBalance.Quantity : 0;

                                    //set balance to 0 so that it is not processed under balanceItem.Balance
                                    //if not processed here it means that the combination dosent have a balance from previous year
                                    balanceItem.Balance -= internAccountBalance.Balance;
                                    internAccountBalance.Balance = 0;

                                }
                            }
                            else if (!balanceItem.BalanceItemInternals.Any())
                            {
                                UB = balanceItem.Balance;
                                balanceItem.Balance = 0;
                            }

                            decimal NB = IB + UB;
                            decimal? NBQuant = IBQuant + UBQuant;
                            sum += NB;
                            if (NB != 0M)
                            {
                                var accountStd = accountStds.FirstOrDefault(a => a.AccountId == accountId);
                                // account is not active, skipped
                                if (accountStd == null)
                                    continue;

                                var accountYearBalanceHead = new AccountYearBalanceHead()
                                {
                                    Balance = NB,
                                    Quantity = NBQuant,
                                    AccountStd = accountStd,
                                    AccountYear = accountYear,
                                    AccountTypeName = accountTypes[accountStd.AccountTypeSysTermId],
                                };

                                var strBuilder = new StringBuilder();
                                foreach (var prevInternAccount in previousYearItem.AccountInternal.OrderBy(i => i.AccountId))
                                {
                                    var accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == prevInternAccount.AccountId);
                                    if (accountInternal != null)
                                    {
                                        accountYearBalanceHead.AccountInternal.Add(accountInternal);
                                        strBuilder.Append(accountInternal.AccountId.ToString() + ";");
                                    }
                                }
                                accountYearBalanceHead.InternalIds = strBuilder.ToString();
                                heads.Add(accountYearBalanceHead);
                            }
                        }
                    }

                    //process items without IB from previous year...
                    if (balanceItem.Balance != 0 || (balanceItem.Quantity.HasValue && balanceItem.Quantity != 0))
                    {
                        decimal restBalance = balanceItem.Balance;
                        decimal restQuantity = balanceItem.Quantity != null ? balanceItem.Quantity.Value : 0;

                        var accountStd = accountStds.FirstOrDefault(a => a.AccountId == accountId);
                        // account is not active, skipped
                        if (accountStd == null)
                            continue;

                        var internalBalanceAccounts = balanceItem.BalanceItemInternals.Where(x => x.Balance != 0);
                        if (internalBalanceAccounts.Any())
                        {
                            foreach (var balanceItemIntern in internalBalanceAccounts)
                            {
                                sum += balanceItemIntern.Balance;
                                restBalance -= balanceItemIntern.Balance;
                                restQuantity -= balanceItemIntern.Quantity != null ? balanceItemIntern.Quantity.Value : 0;

                                var accountYearBalanceHead = new AccountYearBalanceHead()
                                {
                                    Balance = balanceItemIntern.Balance,
                                    Quantity = balanceItemIntern.Quantity,
                                    AccountStd = accountStd,
                                    AccountYear = accountYear,
                                    AccountTypeName = accountTypes[accountStd.AccountTypeSysTermId],
                                };

                                var strBuilder = new StringBuilder();
                                foreach (var balanceIntern in balanceItemIntern.AccountInternals.OrderBy(i => i.AccountId))
                                {
                                    var accountInternal = accountInternals.FirstOrDefault(a => a.AccountId == balanceIntern.AccountId);
                                    if (accountInternal != null)
                                    {
                                        accountYearBalanceHead.AccountInternal.Add(accountInternal);
                                        strBuilder.Append(accountInternal.AccountId.ToString() + ";");
                                    }
                                }
                                accountYearBalanceHead.InternalIds = strBuilder.ToString();
                                heads.Add(accountYearBalanceHead);
                            }

                            if (restBalance != 0)
                            {
                                sum += restBalance;
                                var accountYearBalanceHead = new AccountYearBalanceHead()
                                {
                                    Balance = restBalance,
                                    Quantity = restQuantity,
                                    AccountStd = accountStd,
                                    AccountYear = accountYear,
                                    AccountTypeName = accountTypes[accountStd.AccountTypeSysTermId],
                                    InternalIds = "",
                                };

                                heads.Add(accountYearBalanceHead);
                            }
                        }
                        else
                        {
                            sum += balanceItem.Balance;
                            var accountYearBalanceHead = new AccountYearBalanceHead()
                            {
                                Balance = balanceItem.Balance,
                                Quantity = balanceItem.Quantity,
                                AccountStd = accountStd,
                                AccountYear = accountYear,
                                AccountTypeName = accountTypes[accountStd.AccountTypeSysTermId],
                                InternalIds = "",
                            };

                            heads.Add(accountYearBalanceHead);
                        }
                    }

                }

                #endregion

                int counter = 1;
                var items = new List<AccountYearBalanceFlatDTO>();
                if (sum != 0 && SettingManager.GetBoolSetting(entities, SettingMainType.Company, (int)CompanySettingType.AccountingCreateDiffRowOnBalanceTransfer, 0, actorCompanyId, 0, true))
                {
                    var accountYearBalanceHead = new AccountYearBalanceHead()
                    {
                        Balance = (sum * -1),
                        Quantity = 0,
                        AccountYear = accountYear,
                        AccountTypeName = "diff",
                        isDiffRow = true,
                    };
                    var dto = accountYearBalanceHead.ToFlatDTO(accountDimInternals, false);
                    dto.RowNr = counter;
                    items.Add(dto);
                    counter++;
                }

                foreach (var accountGroup in heads.GroupBy(h => h.AccountStd.AccountId))
                {
                    foreach (var group in accountGroup.GroupBy(h => h.InternalIds))
                    {
                        if (group.Count() > 1)
                        {
                            var item = group.First();
                            for (int i = 1; i < group.Count(); i++)
                            {
                                item.Balance += group.ElementAt(i).Balance;
                                item.Quantity += group.ElementAt(i).Quantity;
                            }
                            var dto = item.ToFlatDTO(accountDimInternals, false);
                            dto.RowNr = counter;
                            items.Add(dto);
                            counter++;
                        }
                        else
                        {
                            var dto = group.First().ToFlatDTO(accountDimInternals, false);
                            dto.RowNr = counter;
                            items.Add(dto);
                            counter++;
                        }
                    }
                }

                result.Value = items;
            }

            return result;
        }

        public AccountYearBalanceHead GetAccountYearBalanceHeadByAccountNr(int accountYearId, string accountNr)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountYearBalanceHead.NoTracking();
            return GetAccountYearBalanceHeadByAccountNr(entities, accountYearId, accountNr);
        }

        public AccountYearBalanceHead GetAccountYearBalanceHeadByAccountNr(CompEntities entities, int accountYearId, string accountNr)
        {
            return (from b in entities.AccountYearBalanceHead
                    where ((b.AccountYear.AccountYearId == accountYearId) &&
                    (b.AccountStd.Account.AccountNr == accountNr))
                    select b).FirstOrDefault<AccountYearBalanceHead>();
        }

        public List<AccountYearBalanceHead> GetAccountYearBalanceHeadsByAccountNr(CompEntities entities, int accountYearId, string accountNr)
        {
            return (from b in entities.AccountYearBalanceHead
                    where ((b.AccountYear.AccountYearId == accountYearId) &&
                    (b.AccountStd.Account.AccountNr == accountNr))
                    select b).ToList();

        }

        public IEnumerable<AccountYearBalanceHead> GetAccountYearBalanceHeadsByAccountId(CompEntities entities, int accountYearId, int accountId)
        {
            return (from b in entities.AccountYearBalanceHead
                    where b.AccountYear.AccountYearId == accountYearId &&
                    b.AccountStd.AccountId == accountId
                    select b);
        }

        public AccountYearBalanceHead GetAccountYearBalanceHead(CompEntities entities, int accountYearBalanceHeadId, int actorCompanyId)
        {
            entities.AccountYearBalanceHead.NoTracking(); 
            var balanceHead = (from b in entities.AccountYearBalanceHead
                                                                             where b.AccountYearBalanceHeadId == accountYearBalanceHeadId &&
                                                                             b.AccountYear.ActorCompanyId == actorCompanyId
                                                                             select b).FirstOrDefault();

            if (balanceHead.AccountStd != null && !balanceHead.AccountStd.AccountReference.IsLoaded)
                balanceHead.AccountStd.AccountReference.Load();

            foreach (AccountInternal accountInternal in balanceHead.AccountInternal)
            {
                // Account for AccountInternal
                if (!accountInternal.AccountReference.IsLoaded)
                    accountInternal.AccountReference.Load();

                //AccountDim
                if (!accountInternal.Account.AccountDimReference.IsLoaded)
                    accountInternal.Account.AccountDimReference.Load();
            }

            return balanceHead;
        }

        public AccountYearBalanceHead GetAccountYearBalanceHeadWithInternals(CompEntities entities, int accountYearBalanceHeadId, int actorCompanyId, bool includeTransfers = false)
        {
            IQueryable<AccountYearBalanceHead> query = (from b in entities.AccountYearBalanceHead
                               .Include("AccountStd.Account")
                               .Include("AccountInternal.Account.AccountDim")
                                                        where b.AccountYearBalanceHeadId == accountYearBalanceHeadId &&
                                                        b.AccountYear.ActorCompanyId == actorCompanyId
                                                        select b);

            if (includeTransfers)
            {
                query = query.Include("CompanyGroupTransferRow.CompanyGroupTransferHead");
            }

            return query.FirstOrDefault();
        }

        public ActionResult DeleteAllAccountYearBalanceHead(int accountYearId, int actorCompanyId)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                List<AccountYearBalanceHead> accountYearBalanceHeads = GetAccountYearBalanceHeads(entities, accountYearId, actorCompanyId);
                if (accountYearBalanceHeads.Count > 0)
                {
                    foreach (AccountYearBalanceHead accountYearBalanceHead in accountYearBalanceHeads)
                    {
                        accountYearBalanceHead.AccountInternal.Clear();
                        entities.DeleteObject(accountYearBalanceHead);
                    }

                    result = SaveChanges(entities);
                }
            }

            return result;
        }

        public ActionResult AddAccountYearBalanceHead(AccountYearBalanceHead accountYearBalanceHead, int accountYearId, int actorCompanyId, string accountNr)
        {
            using (CompEntities entities = new CompEntities())
            {
                return AddAccountYearBalanceHead(entities, accountYearBalanceHead, accountYearId, actorCompanyId, accountNr);
            }
        }

        public ActionResult AddAccountYearBalanceHead(CompEntities entities, AccountYearBalanceHead accountYearBalanceHead, int accountYearId, int actorCompanyId, string accountNr)
        {
            if (accountYearBalanceHead == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountYearBalanceHead");

            using (entities)
            {
                accountYearBalanceHead.AccountYear = AccountManager.GetAccountYear(entities, accountYearId);
                if (accountYearBalanceHead.AccountYear == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYear");

                accountYearBalanceHead.AccountStd = (from a in entities.AccountStd
                                                     where ((a.Account.AccountNr == accountNr) &&
                                                     (a.Account.ActorCompanyId == actorCompanyId) &&
                                                     (a.Account.State == (int)SoeEntityState.Active))
                                                     select a).FirstOrDefault<AccountStd>();

                if (accountYearBalanceHead.AccountStd == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountStd");

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, accountYearBalanceHead);

                return AddEntityItem(entities, accountYearBalanceHead, "AccountYearBalanceHead");
            }
        }

        public ActionResult UpdateAccountYearBalanceHead(AccountYearBalanceHead accountYearBalanceHead, int actorCompanyId)
        {
            if (accountYearBalanceHead == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "AccountYearBalanceHead");

            using (CompEntities entities = new CompEntities())
            {
                AccountYearBalanceHead originalAccountYearBalanceHead = GetAccountYearBalanceHead(entities, accountYearBalanceHead.AccountYearBalanceHeadId, actorCompanyId);
                if (originalAccountYearBalanceHead == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountYearBalanceHead");

                //Set currency amounts
                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, accountYearBalanceHead);

                return this.UpdateEntityItem(entities, originalAccountYearBalanceHead, accountYearBalanceHead, "AccountYearBalanceHead");
            }
        }

        public ActionResult SaveAccountYearBalances(List<AccountYearBalanceFlatDTO> items, int accountYearId, int actorCompanyId)
        {
            var result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        AccountYear accountYear = AccountManager.GetAccountYear(entities, accountYearId);
                        if (accountYear == null)
                            return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(2206, "Kunde inte spara, redovisningsår saknas"));

                        List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, base.ActorCompanyId, null, true, false);
                        List<AccountInternal> accountInternals = AccountManager.GetAccountInternals(entities, base.ActorCompanyId, true);

                        List<AccountStd> updatedAccountStds = new List<AccountStd>();

                        foreach (var item in items)
                        {
                            if (item.IsDeleted)
                            {
                                if (item.AccountYearBalanceHeadId == 0)
                                    continue;

                                var accountYearBalanceHead = GetAccountYearBalanceHeadWithInternals(entities, item.AccountYearBalanceHeadId, actorCompanyId, true);

                                if (accountYearBalanceHead == null)
                                    continue;

                                accountYearBalanceHead.AccountInternal.Clear();

                                if (accountYearBalanceHead.CompanyGroupTransferRow != null)
                                {
                                    foreach (var transferRow in accountYearBalanceHead.CompanyGroupTransferRow)
                                    {
                                        transferRow.AccountYearBalanceHeadId = null;
                                        SetModifiedProperties(transferRow);
                                    }
                                }

                                entities.DeleteObject(accountYearBalanceHead);

                                if (entities.SaveChanges() == 0)
                                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2205, "Kunde inte spara"));
                            }
                            else
                            {
                                AccountYearBalanceHead accountYearBalanceHead = null;
                                if (item.AccountYearBalanceHeadId > 0)
                                {
                                    accountYearBalanceHead = GetAccountYearBalanceHeadWithInternals(entities, item.AccountYearBalanceHeadId, actorCompanyId);
                                    SetModifiedProperties(accountYearBalanceHead);
                                }
                                else
                                {
                                    accountYearBalanceHead = new AccountYearBalanceHead()
                                    {
                                        AccountYear = accountYear,
                                    };
                                    SetCreatedProperties(accountYearBalanceHead);
                                }

                                // Basic values
                                accountYearBalanceHead.Balance = item.DebitAmount - item.CreditAmount;
                                accountYearBalanceHead.Quantity = item.Quantity;

                                //Set currency amounts
                                CountryCurrencyManager.SetCurrencyAmounts(entities, actorCompanyId, accountYearBalanceHead);

                                AccountStd accountStd = accountStds.FirstOrDefault(a => a.AccountId == item.Dim1Id);
                                if (accountStd != null)
                                {
                                    accountYearBalanceHead.AccountStd = accountStd;
                                    updatedAccountStds.Add(accountStd);
                                }

                                // Remove internal accounts
                                accountYearBalanceHead.AccountInternal.Clear();
                                result = SaveChanges(entities, transaction);
                                if (!result.Success)
                                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2205, "Kunde inte spara"));

                                // Set new
                                if (item.Dim2Id > 0)
                                    accountYearBalanceHead.AccountInternal.Add(accountInternals.FirstOrDefault(a => a.AccountId == item.Dim2Id));
                                if (item.Dim3Id > 0)
                                    accountYearBalanceHead.AccountInternal.Add(accountInternals.FirstOrDefault(a => a.AccountId == item.Dim3Id));
                                if (item.Dim4Id > 0)
                                    accountYearBalanceHead.AccountInternal.Add(accountInternals.FirstOrDefault(a => a.AccountId == item.Dim4Id));
                                if (item.Dim5Id > 0)
                                    accountYearBalanceHead.AccountInternal.Add(accountInternals.FirstOrDefault(a => a.AccountId == item.Dim5Id));
                                if (item.Dim6Id > 0)
                                    accountYearBalanceHead.AccountInternal.Add(accountInternals.FirstOrDefault(a => a.AccountId == item.Dim6Id));
                            }
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                            return new ActionResult((int)ActionResultSave.NothingSaved, GetText(2205, "Kunde inte spara"));

                        //Update balance on all accounts that was updated
                        result = CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, accountYearId, updatedAccountStds);

                        if (result.Success)
                        {
                            //Commit transaction
                            transaction.Complete();
                        }
                    }
                }

                catch (Exception ex)
                {
                    base.LogError(ex, this.log);
                    result.Exception = ex;
                }
                finally
                {
                    entities.Connection.Close();
                }

                return result;
            }
        }

        #endregion

        #region BalanceItem

        public Dictionary<int, BalanceItemDTO> GetYearInBalance(CompEntities entities, AccountYear accountYear, List<AccountStd> accountStdsInInterval, List<AccountInternal> accountInternals, int actorCompanyId)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();

            #region Init

            InitBalanceItemDict(accountStdsInInterval, ref biDict);

            if (accountYear == null)
                return biDict;

            #endregion

            List<AccountYearBalanceHead> balanceHeads = (from h in entities.AccountYearBalanceHead
                                                            .Include("AccountStd")
                                                            .Include("AccountInternal")
                                                         where (h.AccountYear.AccountYearId == accountYear.AccountYearId)
                                                         select h).ToList();

            foreach (AccountYearBalanceHead balanceHead in balanceHeads)
            {
                #region AccountYearBalanceHead

                if (!biDict.ContainsKey(balanceHead.AccountStd.AccountId))
                    continue;

                bool valid = Validator.IsAccountInInterval(accountInternals, balanceHead.AccountInternal?.ToList(), approveOneAccountInternal: true);
                if (!valid)
                    continue;

                BalanceItemDTO balanceItem = biDict[balanceHead.AccountStd.AccountId];
                balanceItem.Balance += balanceHead.Balance;
                if (balanceHead.Quantity.HasValue)
                    if (balanceItem.Quantity != null)
                        balanceItem.Quantity += balanceHead.Quantity.Value;
                    else
                        balanceItem.Quantity = balanceHead.Quantity.Value;
                biDict.Remove(balanceHead.AccountStd.AccountId);
                biDict.Add(balanceHead.AccountStd.AccountId, balanceItem);

                #endregion
            }

            return biDict;
        }

        public Dictionary<int, BalanceItemDTO> GetPeriodOutBalance(CompEntities entities, AccountYear accountYear, DateTime dateTo, List<AccountStd> accountStdsInInterval, List<AccountDim> accountDimInternals, List<AccountInternal> accountInternalsInInterval, Dictionary<int, BalanceItemDTO> biYearInBalanceDict, int actorCompanyId)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();

            #region Init

            InitBalanceItemDict(accountStdsInInterval, ref biDict);

            if (accountYear == null)
                return biDict;

            #endregion

            #region Prereq

            //Can be passed as parameter for optimization
            if (biYearInBalanceDict == null)
                biYearInBalanceDict = GetYearInBalance(entities, accountYear, accountStdsInInterval, accountInternalsInInterval, actorCompanyId);

            Dictionary<int, BalanceItemDTO> biBalancechangePeriodDict = GetBalanceChange(entities, accountYear, accountYear.From, dateTo, accountStdsInInterval, accountDimInternals, accountInternalsInInterval, actorCompanyId);

            #endregion

            if (accountStdsInInterval != null)
            {
                foreach (var accountId in accountStdsInInterval.Select(x => x.AccountId))
                {
                    #region AccountStd

                    if (!biDict.ContainsKey(accountId))
                        continue;

                    decimal accountClosingBalance = 0;
                    decimal accountClosingBalanceEntCurrency = 0;
                    decimal accountClosingQuantity = 0;

                    //Year in balance
                    var biYearInBalance = biYearInBalanceDict[accountId];
                    if (biYearInBalance != null)
                    {
                        accountClosingBalance += biYearInBalance.Balance;
                        accountClosingBalanceEntCurrency += biYearInBalance.BalanceEntCurrency;
                        if (biYearInBalance.Quantity.HasValue)
                            accountClosingQuantity += biYearInBalance.Quantity.Value;
                    }

                    //Balance change to date
                    var biBalancechangePeriod = biBalancechangePeriodDict[accountId];
                    if (biBalancechangePeriod != null)
                    {
                        accountClosingBalance += biBalancechangePeriod.Balance;
                        accountClosingBalanceEntCurrency += biBalancechangePeriod.BalanceEntCurrency;
                        if (biBalancechangePeriod.Quantity.HasValue)
                            accountClosingQuantity += biBalancechangePeriod.Quantity.Value;
                    }

                    BalanceItemDTO balanceItem = biDict[accountId];
                    balanceItem.Balance = accountClosingBalance;
                    balanceItem.BalanceEntCurrency = accountClosingBalanceEntCurrency;
                    if (balanceItem.Quantity != null)
                        balanceItem.Quantity += accountClosingQuantity > 0 ? accountClosingQuantity : (decimal?)null;
                    else
                        balanceItem.Quantity = accountClosingQuantity > 0 ? accountClosingQuantity : (decimal?)null;
                    #endregion
                }
            }

            return biDict;
        }

        public void FillVoucherHeadDTOCache(DateTime dateFrom, DateTime dateTo, int actorCompanyId)
        {
            List<VoucherHeadDTO> voucherHeads = voucherHeadDTOCache.Get(VoucherManager, actorCompanyId, dateFrom, dateTo).OrderBy(x => x.VoucherNr).ToList(); // VoucherManager.GetVoucherHeadDTOs(actorCompanyId, dateFrom, dateTo).OrderBy(x => x.VoucherNr).ToList();
            voucherHeads = voucherHeads.Where(v => !v.Template).ToList();

            #region Add to cache

            foreach (var yearHeads in voucherHeads.GroupBy(v => v.AccountYearId))
            {
                var heads = new List<VoucherHeadDTO>();
                int accountYearId = yearHeads.First().AccountYearId;
                DateTime from = yearHeads.OrderBy(y => y.Date).First().Date;
                DateTime to = yearHeads.OrderBy(y => y.Date).Last().Date;

                heads.AddRange(yearHeads);

                if (this.balanceChangeVoucherHeadDTOCache != null)
                {
                    this.balanceChangeVoucherHeadDTOCache.AddItem(accountYearId, from, to, heads);
                }
                else
                {
                    this.balanceChangeVoucherHeadDTOCache = new BalanceChangeVoucherHeadDTOCache();
                    this.balanceChangeVoucherHeadDTOCache.AddItem(accountYearId, from, to, heads);
                }
            }

            #endregion
        }

        public Dictionary<int, BalanceItemDTO> GetBalanceChange(AccountYear accountYear, DateTime dateFrom, DateTime dateTo, List<AccountStd> accountStdsInInterval, List<AccountDim> accountDimInternals, List<AccountInternal> accountInternals, int actorCompanyId, bool ignoreValidation = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            return GetBalanceChange(entities, accountYear, dateFrom, dateTo, accountStdsInInterval, accountDimInternals, accountInternals, actorCompanyId, ignoreValidation);
        }

        public Dictionary<int, BalanceItemDTO> GetBalanceChange(CompEntities entities, AccountYear accountYear, DateTime dateFrom, DateTime dateTo, List<AccountStd> accountStdsInInterval, List<AccountDim> accountDimInternals, List<AccountInternal> accountInternals, int actorCompanyId, bool ignoreValidation = false)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();

            List<VoucherHead> voucherHeads = null;

            #region Init

            InitBalanceItemDict(accountStdsInInterval, ref biDict);

            if (accountYear == null)
                return biDict;

            #endregion

            //Check cache
            if (this.balanceChangeVoucherHeadCache != null)
            {
                var voucherHeadCacheItem = this.balanceChangeVoucherHeadCache.GetItem(accountYear.AccountYearId, dateFrom, dateTo);
                if (voucherHeadCacheItem != null)
                    voucherHeads = voucherHeadCacheItem.VoucherHeads;
            }

            if (voucherHeads == null)
            {
                //Get from db
                voucherHeads = (from vh in entities.VoucherHead
                                    .Include("VoucherRow.AccountInternal")
                                where vh.AccountPeriod.AccountYearId == accountYear.AccountYearId &&
                                vh.Date >= dateFrom && vh.Date <= dateTo &&
                                vh.ActorCompanyId == actorCompanyId &&
                                vh.VoucherSeries.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                                !vh.VoucherSeries.VoucherSeriesType.Template
                                select vh).ToList();

                //Add to cache
                if (this.balanceChangeVoucherHeadCache != null)
                    this.balanceChangeVoucherHeadCache.AddItem(accountYear.AccountYearId, dateFrom, dateTo, voucherHeads);
            }

            UpdateBalanceItemsFromVoucherHead(voucherHeads, biDict, accountDimInternals, accountInternals, ignoreValidation);

            return biDict;
        }

        #region New with DTO instead of entities

        public Dictionary<int, BalanceItemDTO> GetYearInBalanceFromDTO(CompEntities entities, AccountYearDTO accountYear, List<AccountDTO> accountStdsInInterval, List<AccountInternalDTO> accountInternals, int actorCompanyId)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();

            InitBalanceItemDict(accountStdsInInterval, ref biDict);

            if (accountYear == null)
                return biDict;

            List<AccountYearBalanceHead> balanceHeads = (from h in entities.AccountYearBalanceHead
                                                            .Include("AccountStd")
                                                            .Include("AccountInternal")
                                                         where h.AccountYear.AccountYearId == accountYear.AccountYearId
                                                         select h).ToList();

            foreach (AccountYearBalanceHeadDTO balanceHead in balanceHeads.ToDTOs())
            {
                if (!biDict.ContainsKey(balanceHead.AccountId))
                    continue;
                if (!Validator.IsAccountInInterval(accountInternals, balanceHead.rows, approveOneAccountInternal: true))
                    continue;

                BalanceItemDTO balanceItem = biDict[balanceHead.AccountId];
                balanceItem.Balance += balanceHead.Balance;
                if (balanceHead.Quantity.HasValue)
                {
                    if (balanceItem.Quantity != null)
                        balanceItem.Quantity += balanceHead.Quantity.Value;
                    else
                        balanceItem.Quantity = balanceHead.Quantity.Value;
                }
                biDict.Remove(balanceHead.AccountId);
                biDict.Add(balanceHead.AccountId, balanceItem);
            }

            return biDict;
        }

        public Dictionary<int, BalanceItemDTO> GetBalanceChangeFromDTO(AccountYearDTO accountYear, DateTime dateFrom, DateTime dateTo, List<AccountDTO> accountStdsInInterval, List<AccountDimDTO> accountDimInternals, List<AccountInternalDTO> accountInternals, int actorCompanyId, bool ignoreValidation = false, bool includeYearEndVouchers = false, bool includeExternalVouchers = false)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();
            List<VoucherHeadDTO> voucherHeads = new List<VoucherHeadDTO>();

            #region Init

            InitBalanceItemDict(accountStdsInInterval, ref biDict);

            if (accountYear == null)
                return biDict;

            #endregion

            //Check cache
            if (this.balanceChangeVoucherHeadDTOCache != null)
            {
                var voucherHeadCacheItem = this.balanceChangeVoucherHeadDTOCache.GetItem(accountYear.AccountYearId, dateFrom, dateTo);
                if (voucherHeadCacheItem != null)
                    voucherHeads = voucherHeadCacheItem.VoucherHeads;
            }

            if (voucherHeads.IsNullOrEmpty())
            {
                voucherHeads = voucherHeadDTOCache.Get(VoucherManager, actorCompanyId, dateFrom, dateTo);
                voucherHeads = voucherHeads.Where(v => !v.Template).ToList();

                //Add to cache
                if (this.balanceChangeVoucherHeadDTOCache != null)
                    this.balanceChangeVoucherHeadDTOCache.AddItem(accountYear.AccountYearId, dateFrom, dateTo, voucherHeads);
            }

            UpdateBalanceItemsFromVoucherHeadDTO(voucherHeads, biDict, accountDimInternals, accountInternals, ignoreValidation);

            return biDict;
        }

        public BalanceItemDTO GetAccountBalanceFromDTO(AccountYearDTO accountYear, int accountId, Dictionary<int, BalanceItemDTO> biOpeningYearBalanceDict, Dictionary<int, BalanceItemDTO> biBalanceChangeToPeriodDict)
        {
            //Ingående saldo för aktuell period
            BalanceItemDTO biAccountBalance = new BalanceItemDTO();

            if (accountYear == null)
                return biAccountBalance;

            biAccountBalance.AccountId = accountId;

            //Ingående balans för aktuellt år
            BalanceItemDTO biOpeningYearBalance = new BalanceItemDTO();
            if (biOpeningYearBalanceDict.ContainsKey(accountId))
                biOpeningYearBalance = biOpeningYearBalanceDict[accountId];

            //Från år start till period start
            BalanceItemDTO biBalanceChangeToPeriod = new BalanceItemDTO();
            if (biBalanceChangeToPeriodDict.ContainsKey(accountId))
                biBalanceChangeToPeriod = biBalanceChangeToPeriodDict[accountId];

            //Ingående saldo för aktuell period. (Ingående balans för aktuellt år + Från år start till period start)
            biAccountBalance.Balance = biOpeningYearBalance.Balance + biBalanceChangeToPeriod.Balance;
            if (biOpeningYearBalance.Quantity == null)
                biOpeningYearBalance.Quantity = 0;
            if (biBalanceChangeToPeriod.Quantity == null)
                biBalanceChangeToPeriod.Quantity = 0;
            biAccountBalance.Quantity = biOpeningYearBalance.Quantity + biBalanceChangeToPeriod.Quantity;
            return biAccountBalance;
        }

        public Dictionary<int, BalanceItemDTO> GetBalanceChangeFromDTO(AccountYearDTO accountYear, DateTime dateFrom, DateTime dateTo, List<AccountDTO> accountStdsInInterval, List<AccountDimDTO> accountDimInternals, List<AccountInternalDTO> accountInternals, int actorCompanyId, out List<VoucherHeadDTO> voucherHeads, bool ignoreValidation = false, bool includeYearEndVouchers = false, bool includeExternalVouchers = false, bool onlyEmptyInternals = false)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();

            voucherHeads = null;

            #region Init

            InitBalanceItemDict(accountStdsInInterval, ref biDict);

            if (accountYear == null)
                return biDict;

            //get voucherSeriesTypes that are left out from voucherheads
            List<VoucherSeriesType> voucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(actorCompanyId, false).Where(i => (i.YearEndSerie && !includeYearEndVouchers) || (i.ExternalSerie && !includeExternalVouchers)).ToList();

            #endregion

            //Check cache
            if (this.balanceChangeVoucherHeadDTOCache != null)
            {
                var voucherHeadCacheItem = this.balanceChangeVoucherHeadDTOCache.GetItem(accountYear.AccountYearId, dateFrom, dateTo);
                if (voucherHeadCacheItem != null)
                    voucherHeads = (from vh in voucherHeadCacheItem.VoucherHeads
                                    where !(from vst in voucherSeriesTypes
                                            select vst.VoucherSeriesTypeId)
                                      .Contains(vh.VoucherSeriesTypeId)
                                    select vh).ToList();
            }

            if (voucherHeads == null)
            {
                voucherHeads = voucherHeadDTOCache.Get(VoucherManager, actorCompanyId, dateFrom, dateTo); //VoucherManager.GetVoucherHeadDTOs(actorCompanyId, dateFrom, dateTo);                
                voucherHeads = (from vh in voucherHeads
                                where vh.Date >= dateFrom && vh.Date <= dateTo &&
                                !vh.Template &&
                                !(from vst in voucherSeriesTypes
                                  select vst.VoucherSeriesTypeId)
                                  .Contains(vh.VoucherSeriesTypeId)
                                select vh).ToList();

                //Add to cache
                if (this.balanceChangeVoucherHeadDTOCache != null)
                    this.balanceChangeVoucherHeadDTOCache.AddItem(accountYear.AccountYearId, dateFrom, dateTo, voucherHeads);
            }

            UpdateBalanceItemsFromVoucherHeadDTO(voucherHeads, biDict, accountDimInternals, accountInternals, ignoreValidation, onlyEmptyInternals);

            return biDict;
        }

        public Dictionary<int, BalanceItemDTO> GetBalanceChangeFromDTO(DateTime? dateFrom, DateTime dateTo, List<AccountDTO> accountStdsInInterval, List<AccountDimDTO> accountDimInternals, List<AccountInternalDTO> accountInternals, int actorCompanyId, out List<VoucherHeadDTO> voucherHeads, bool ignoreValidation = false, bool includeYearEndVouchers = false, bool includeExternalVouchers = false, bool onlyEmptyInternals = false)
        {
            Dictionary<int, BalanceItemDTO> biDict = new Dictionary<int, BalanceItemDTO>();

            voucherHeads = null;

            #region Init

            InitBalanceItemDict(accountStdsInInterval, ref biDict);

            #endregion

            List<VoucherSeriesType> voucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(actorCompanyId, false).Where(i => (i.YearEndSerie && !includeYearEndVouchers) || (i.ExternalSerie && !includeExternalVouchers)).ToList();

            dateFrom = dateFrom ?? CalendarUtility.DATETIME_DEFAULT;
            voucherHeads = voucherHeadDTOCache.Get(VoucherManager, actorCompanyId, dateFrom.Value, dateTo); //VoucherManager.GetVoucherHeadDTOs(actorCompanyId, dateFrom.Value, dateTo);
            voucherHeads = (from vh in voucherHeads
                            where vh.Date >= dateFrom && vh.Date <= dateTo &&
                            !vh.Template &&
                            !(from vst in voucherSeriesTypes
                              select vst.VoucherSeriesTypeId)
                              .Contains(vh.VoucherSeriesTypeId)
                            select vh).ToList();

            UpdateBalanceItemsFromVoucherHeadDTO(voucherHeads, biDict, accountDimInternals, accountInternals, ignoreValidation, onlyEmptyInternals);

            return biDict;
        }


        #endregion

        public Dictionary<int, List<GrossProfitAccountBalanceItemDTO>> GetPeriodGrossProfitChangesFromDTO(AccountYearDTO accountYear, DateTime dateFrom, DateTime dateTo, List<AccountDTO> allAccountStds, List<AccountDTO> accountStdsInInterval, List<AccountDimDTO> internalaccountDims, List<AccountInternalDTO> accountInternals, List<GrossProfitCodeDTO> grossProfitCodes, List<IReportHeaderInterval> grossProfitReportHeaderIntervals, int actorCompanyId, out List<VoucherHeadDTO> grossVoucherHeads, bool detailedInformation, bool ignoreValidation = false)
        {
            Dictionary<int, List<GrossProfitAccountBalanceItemDTO>> items = new Dictionary<int, List<GrossProfitAccountBalanceItemDTO>>();
            List<VoucherHeadDTO> voucherHeads = null;
            grossVoucherHeads = new List<VoucherHeadDTO>();


            #region Init

            if (dateTo < dateFrom)
                return items;

            DateTime actualFromDate;
            if (dateFrom < accountYear.From)
                actualFromDate = accountYear.From;
            else
                actualFromDate = dateFrom;

            DateTime actualToDate;
            if (dateTo > accountYear.To)
                actualToDate = accountYear.To;
            else
                actualToDate = dateTo;

            if (accountStdsInInterval.Count == 0)
                return items;

            if (accountYear == null)
                return items;



            #region Check cache

            if (this.balanceChangeVoucherHeadCache != null)
            {
                var voucherHeadCacheItem = this.balanceChangeVoucherHeadDTOCache.GetItem(accountYear.AccountYearId, dateFrom, dateTo);
                if (voucherHeadCacheItem != null)
                    voucherHeads = voucherHeadCacheItem.VoucherHeads;
            }

            #endregion

            if (voucherHeads == null)
            {
                voucherHeads = VoucherManager.GetVoucherHeadDTOs(actorCompanyId, dateFrom, dateTo);

                voucherHeads = (from vh in voucherHeads
                                where (vh.Date >= actualFromDate && vh.Date <= actualToDate) &&
                                !vh.Template
                                select vh).ToList();

                #region Add to cache

                if (this.balanceChangeVoucherHeadCache != null)
                {
                    this.balanceChangeVoucherHeadDTOCache.AddItem(accountYear.AccountYearId, dateFrom, dateTo, voucherHeads);
                }

                #endregion
            }

            List<int> validAccountIds = new List<int>();

            foreach (var accountStd in allAccountStds)
            {
                foreach (var interval in grossProfitReportHeaderIntervals)
                {
                    if (Validator.IsAccountInInterval(accountStd.AccountNr, interval.IntervalFrom, interval.IntervalTo) && accountStdsInInterval.Select(a => a.AccountId).Contains(accountStd.AccountId))
                        validAccountIds.Add(accountStd.AccountId);
                }
            }

            foreach (var head in voucherHeads)
            {
                foreach (var voucherRow in head.Rows)
                {
                    if (!validAccountIds.Contains(voucherRow.Dim1Id) && accountStdsInInterval.Select(a => a.AccountId).Contains(voucherRow.Dim1Id))
                    {
                        validAccountIds.Add(voucherRow.Dim1Id);
                    }
                }
            }

            validAccountIds = validAccountIds.Distinct().ToList();

            foreach (GrossProfitCodeDTO code in grossProfitCodes.Where(c => c.AccountYearId == accountYear.AccountYearId))
            {
                List<GrossProfitAccountBalanceItemDTO> gpas = new List<GrossProfitAccountBalanceItemDTO>();
                List<GrossProfitBalanceItemDTO> bl = GetGrossProfitBalanceItemsFromDTO(actualFromDate, actualToDate, code);

                if (!bl.IsNullOrEmpty())
                {
                    foreach (AccountDTO accountStd in accountStdsInInterval.Where(a => validAccountIds.Contains(a.AccountId)))
                    {
                        GrossProfitAccountBalanceItemDTO dto = new GrossProfitAccountBalanceItemDTO()
                        {
                            AccountId = accountStd.AccountId,
                            GrossProfitInternalAccountId = code.AccountId,
                            BalanceItems = bl.Select(b => b.CloneDTO()).ToList(),
                        };

                        gpas.Add(dto);
                    }

                    items.Add(code.GrossProfitCodeId, gpas);
                }
            }

            #endregion

            double lowestLevel = 0;
            double gpadTime = 0;
            double validateTime = 0;
            int loops = 0;

            foreach (VoucherHeadDTO voucherHead in voucherHeads)
            {
                List<VoucherRowDTO> grossVoucherRows = new List<VoucherRowDTO>();

                foreach (var voucherRow in voucherHead.Rows.Where(r => r.State == (int)SoeEntityState.Active && validAccountIds.Contains(r.Dim1Id)))
                {
                    #region VoucherRow

                    var validateStart = DateTime.Now;
                    bool valid = ignoreValidation || Validator.IsAccountInInterval(accountInternals, voucherRow.AccountInternalDTO_forReports?.ToList(), approveOneAccountInternal: true);
                    var validateStop = DateTime.Now;
                    validateTime += (validateStop - validateStart).TotalMilliseconds;

                    if (valid)
                    {
                        foreach (var item in items)
                        {
                            var gpadstart = DateTime.Now;

                            List<int> internals = voucherRow.AccountInternalDTO_forReports != null ? voucherRow.AccountInternalDTO_forReports.Select(a => a.AccountId).ToList() : new List<int>();

                            GrossProfitAccountBalanceItemDTO gpab = item.Value.FirstOrDefault(g => g.AccountId == voucherRow.Dim1Id && (g.GrossProfitInternalAccountId == null || (g.GrossProfitInternalAccountId != null && internals != null && internals.Contains((int)g.GrossProfitInternalAccountId))));
                            loops++;

                            var gpadstop = DateTime.Now;
                            gpadTime += (gpadstop - gpadstart).TotalMilliseconds;

                            if (gpab != null)
                            {
                                GrossProfitBalanceItemDTO dto = gpab.BalanceItems.FirstOrDefault(g => g.PeriodFrom.Date <= voucherHead.Date.Date && g.PeriodTo.Date >= voucherHead.Date.Date);
                                if (dto != null)
                                {
                                    var lowestTimeStatrt = DateTime.Now;

                                    decimal balance = (voucherRow.Amount * (dto.PeriodGrossProfitPercentage / 100));
                                    decimal balanceEntCurrency = (voucherRow.AmountEntCurrency * (dto.PeriodGrossProfitPercentage / 100));

                                    dto.Balance += balance;
                                    dto.BalanceEntCurrency += balanceEntCurrency;
                                    if (voucherRow.Quantity.HasValue)
                                        dto.Quantity += voucherRow.Quantity.Value;

                                    if (voucherRow.AccountInternalDTO_forReports != null && voucherRow.AccountInternalDTO_forReports.Count > 0)
                                    {
                                        BalanceItemInternalDTO balanceItemInternal = dto.GetBalanceItemInternal(voucherRow.AccountInternalDTO_forReports.ToList());
                                        balanceItemInternal.Balance += (voucherRow.Amount * (dto.PeriodGrossProfitPercentage / 100));
                                        balanceItemInternal.BalanceEntCurrency += (voucherRow.AmountEntCurrency * (dto.PeriodGrossProfitPercentage / 100));
                                        if (voucherRow.Quantity.HasValue)
                                            if (balanceItemInternal.Quantity != null)
                                                balanceItemInternal.Quantity += voucherRow.Quantity.Value;
                                            else
                                                balanceItemInternal.Quantity = voucherRow.Quantity.Value;
                                    }

                                    if (detailedInformation)
                                    {
                                        //Create temp VoucherRow 
                                        VoucherRowDTO grossVoucherRow = new VoucherRowDTO()
                                        {
                                            Text = voucherRow.Text,
                                            Date = voucherRow.Date,
                                            Quantity = voucherRow.Quantity,
                                            Merged = voucherRow.Merged,
                                            Amount = balance,
                                            AmountEntCurrency = balanceEntCurrency,
                                            Dim1Id = voucherRow.Dim1Id,
                                        };

                                        if (!voucherRow.AccountInternalDTO_forReports.IsNullOrEmpty())
                                        {
                                            int dimCounter = 2;

                                            foreach (var dim in internalaccountDims)
                                            {
                                                if (dimCounter == 2)
                                                {
                                                    foreach (var internalAccountTrans in voucherRow.AccountInternalDTO_forReports)
                                                    {
                                                        if (internalAccountTrans.Account != null && internalAccountTrans.Account.AccountDimId == dim.AccountDimId)
                                                        {
                                                            grossVoucherRow.Dim2Id = internalAccountTrans.AccountId;
                                                            grossVoucherRow.Dim2Name = internalAccountTrans.Account.Name;
                                                            grossVoucherRow.Dim2Nr = internalAccountTrans.Account.AccountNr;
                                                        }
                                                    }
                                                }
                                                else if (dimCounter == 3)
                                                {
                                                    foreach (var internalAccountTrans in voucherRow.AccountInternalDTO_forReports)
                                                    {
                                                        if (internalAccountTrans.Account != null && internalAccountTrans.Account.AccountDimId == dim.AccountDimId)
                                                        {
                                                            grossVoucherRow.Dim3Id = internalAccountTrans.AccountId;
                                                            grossVoucherRow.Dim3Name = internalAccountTrans.Account.Name;
                                                            grossVoucherRow.Dim3Nr = internalAccountTrans.Account.AccountNr;
                                                        }
                                                    }
                                                }
                                                else if (dimCounter == 4)
                                                {
                                                    foreach (var internalAccountTrans in voucherRow.AccountInternalDTO_forReports)
                                                    {
                                                        if (internalAccountTrans.Account != null && internalAccountTrans.Account.AccountDimId == dim.AccountDimId)
                                                        {
                                                            grossVoucherRow.Dim4Id = internalAccountTrans.AccountId;
                                                            grossVoucherRow.Dim4Name = internalAccountTrans.Account.Name;
                                                            grossVoucherRow.Dim4Nr = internalAccountTrans.Account.AccountNr;
                                                        }
                                                    }
                                                }
                                                else if (dimCounter == 5)
                                                {
                                                    foreach (var internalAccountTrans in voucherRow.AccountInternalDTO_forReports)
                                                    {
                                                        if (internalAccountTrans.Account != null && internalAccountTrans.Account.AccountDimId == dim.AccountDimId)
                                                        {
                                                            grossVoucherRow.Dim5Id = internalAccountTrans.AccountId;
                                                            grossVoucherRow.Dim5Name = internalAccountTrans.Account.Name;
                                                            grossVoucherRow.Dim5Nr = internalAccountTrans.Account.AccountNr;
                                                        }
                                                    }
                                                }
                                                else if (dimCounter == 6)
                                                {
                                                    foreach (var internalAccountTrans in voucherRow.AccountInternalDTO_forReports)
                                                    {
                                                        if (internalAccountTrans.Account != null && internalAccountTrans.Account.AccountDimId == dim.AccountDimId)
                                                        {
                                                            grossVoucherRow.Dim6Id = internalAccountTrans.AccountId;
                                                            grossVoucherRow.Dim6Name = internalAccountTrans.Account.Name;
                                                            grossVoucherRow.Dim6Nr = internalAccountTrans.Account.AccountNr;
                                                        }
                                                    }
                                                }

                                                dimCounter++;
                                            }
                                        }

                                        grossVoucherRows.Add(grossVoucherRow);
                                    }

                                    var lowestTimeStop = DateTime.Now;

                                    lowestLevel += (lowestTimeStop - lowestTimeStatrt).TotalMilliseconds;
                                }
                            }
                        }

                    }
                    #endregion
                }


                if (grossVoucherRows.Any() && detailedInformation)
                {
                    //Create temp VoucherHead
                    VoucherHeadDTO grossVoucherHead = new VoucherHeadDTO()
                    {
                        VoucherNr = voucherHead.VoucherNr,
                        Date = voucherHead.Date,
                        Text = voucherHead.Text,
                        Status = voucherHead.Status,
                        TypeBalance = voucherHead.TypeBalance,
                        VatVoucher = voucherHead.VatVoucher,
                        Note = voucherHead.Note,
                        AccountIds = grossVoucherRows.Where(i => i.Amount != 0).Select(i => i.Dim1Id).Distinct().ToList(),
                    };
                    if (grossVoucherHead.Rows == null)
                        grossVoucherHead.Rows = new List<VoucherRowDTO>();
                    grossVoucherHead.Rows.AddRange(grossVoucherRows);
                    grossVoucherHeads.Add(grossVoucherHead);

                }
            }

            return items;
        }

        public Dictionary<int, List<GrossProfitAccountBalanceItemDTO>> GetPeriodGrossProfitChangesBudgetFromDTO(int budgetHeadId, AccountYearDTO accountYear, DateTime dateFrom, DateTime dateTo, List<AccountDTO> accountStdsInInterval, List<AccountInternalDTO> accountInternals, List<GrossProfitCodeDTO> grossProfitCodes, int actorCompanyId, List<VoucherHeadDTO> budgetVoucherHeadsBalancechangePeriod, out List<VoucherHeadDTO> grossVoucherBudgetHeads, bool ignoreValidation = false, bool onlyEmptyInternals = false)
        {
            Dictionary<int, List<GrossProfitAccountBalanceItemDTO>> items = new Dictionary<int, List<GrossProfitAccountBalanceItemDTO>>();
            grossVoucherBudgetHeads = new List<VoucherHeadDTO>();
            List<AccountDTO> Accounts = accountStdsInInterval;

            #region Init

            DateTime actualFromDate;
            if (dateFrom < accountYear.From)
                actualFromDate = accountYear.From;
            else
                actualFromDate = dateFrom;

            DateTime actualToDate;
            if (dateTo > accountYear.To)
                actualToDate = accountYear.To;
            else
                actualToDate = dateTo;

            if (accountStdsInInterval.Count == 0)
                return items;

            if (accountYear == null)
                return items;

            IEnumerable<ReportHeaderInterval> headerIntervals = ReportManager.GetReportHeaderIntervalsForCompany(actorCompanyId, onlyGrossProfit: true);

            List<int> validAccountIds = new List<int>();

            foreach (var accountStd in Accounts)
            {
                foreach (var interval in headerIntervals)
                {
                    if (Validator.IsAccountInInterval(accountStd.AccountNr, interval.IntervalFrom, interval.IntervalTo) && accountStdsInInterval.Select(a => a.AccountId).Contains(accountStd.AccountId))
                        validAccountIds.Add(accountStd.AccountId);
                }
            }

            foreach (var head in budgetVoucherHeadsBalancechangePeriod)
            {
                foreach (var voucherRow in head.Rows)
                {
                    if (!validAccountIds.Contains(voucherRow.Dim1Id) && accountStdsInInterval.Select(a => a.AccountId).Contains(voucherRow.Dim1Id))
                    {
                        validAccountIds.Add(voucherRow.Dim1Id);
                    }
                }
            }

            validAccountIds = validAccountIds.Distinct().ToList();

            foreach (GrossProfitCodeDTO code in grossProfitCodes.Where(c => c.AccountYearId == accountYear.AccountYearId))
            {
                List<GrossProfitAccountBalanceItemDTO> gpas = new List<GrossProfitAccountBalanceItemDTO>();
                List<GrossProfitBalanceItemDTO> bl = GetGrossProfitBalanceItemsFromDTO(actualFromDate, actualToDate, code);

                if (!bl.IsNullOrEmpty())
                {
                    foreach (AccountDTO accountStd in accountStdsInInterval.Where(a => validAccountIds.Contains(a.AccountId)))
                    {
                        GrossProfitAccountBalanceItemDTO dto = new GrossProfitAccountBalanceItemDTO()
                        {
                            AccountId = accountStd.AccountId,
                            GrossProfitInternalAccountId = code.AccountId,
                            BalanceItems = bl.Select(b => b.CloneDTO()).ToList(),
                        };

                        gpas.Add(dto);
                    }

                    items.Add(code.GrossProfitCodeId, gpas);
                }
            }

            #endregion

            Dictionary<int, List<GrossProfitAccountBalanceItemDTO>> item2s = new Dictionary<int, List<GrossProfitAccountBalanceItemDTO>>();

            foreach (VoucherHeadDTO voucherHead in budgetVoucherHeadsBalancechangePeriod)
            {
                List<VoucherRowDTO> grossVoucherRows = new List<VoucherRowDTO>();

                foreach (VoucherRowDTO voucherRow in voucherHead.Rows)
                {
                    #region VoucherRow

                    // Skip inactive or deleted rows
                    if (voucherRow.State != (int)SoeEntityState.Active)
                        continue;

                    bool valid = ignoreValidation || (onlyEmptyInternals ? (voucherRow.AccountInternalDTO_forReports == null || voucherRow.AccountInternalDTO_forReports.Count == 0) : Validator.IsAccountInInterval(accountInternals, voucherRow.AccountInternalDTO_forReports, approveOneAccountInternal: true));
                    if (valid)
                    {
                        foreach (var item in items)
                        {
                            List<int> internals = voucherRow.AccountInternalDTO_forReports != null ? voucherRow.AccountInternalDTO_forReports.Select(a => a.AccountId).ToList() : new List<int>();
                            GrossProfitAccountBalanceItemDTO gpab = item.Value.FirstOrDefault(g => g.AccountId == voucherRow.Dim1Id && (g.GrossProfitInternalAccountId == null || (g.GrossProfitInternalAccountId != null && internals != null && internals.Contains((int)g.GrossProfitInternalAccountId))));

                            if (gpab != null)
                            {
                                GrossProfitBalanceItemDTO dto = gpab.BalanceItems.FirstOrDefault(g => g.PeriodFrom.Date <= voucherHead.Date.Date && g.PeriodTo.Date >= voucherHead.Date.Date);
                                if (dto != null)
                                {
                                    if (dto.PeriodGrossProfitPercentage == 0)
                                        continue;

                                    decimal balance = (voucherRow.Amount * (dto.PeriodGrossProfitPercentage / 100));
                                    decimal balanceEntCurrency = (voucherRow.AmountEntCurrency * (dto.PeriodGrossProfitPercentage / 100));

                                    if (balance == 0 && balanceEntCurrency == 0)
                                        continue;

                                    dto.Balance += balance;
                                    dto.BalanceEntCurrency += balanceEntCurrency;
                                    dto.Quantity = 99999;
                                    if (voucherRow.Quantity.HasValue)
                                        dto.Quantity += voucherRow.Quantity.Value;

                                    if (voucherRow.AccountInternalDTO_forReports != null && voucherRow.AccountInternalDTO_forReports.Count > 0)
                                    {
                                        BalanceItemInternalDTO balanceItemInternal = dto.GetBalanceItemInternal(voucherRow.AccountInternalDTO_forReports.ToList());
                                        balanceItemInternal.Balance += (voucherRow.Amount * (dto.PeriodGrossProfitPercentage / 100));
                                        balanceItemInternal.BalanceEntCurrency += (voucherRow.AmountEntCurrency * (dto.PeriodGrossProfitPercentage / 100));
                                        balanceItemInternal.AccountInternals = new List<AccountInternalDTO>();

                                        balanceItemInternal.AccountInternals.AddRange(voucherRow.AccountInternalDTO_forReports.ToList());

                                        if (voucherRow.Quantity.HasValue)
                                            if (balanceItemInternal.Quantity != null)
                                                balanceItemInternal.Quantity += voucherRow.Quantity.Value;
                                            else
                                                balanceItemInternal.Quantity = voucherRow.Quantity.Value;

                                        dto.BalanceItemInternals = new List<BalanceItemInternalDTO> { balanceItemInternal };
                                    }

                                    //Create and add new item
                                    List<GrossProfitAccountBalanceItemDTO> gpabs = new List<GrossProfitAccountBalanceItemDTO>();

                                    GrossProfitAccountBalanceItemDTO gpab2 = new GrossProfitAccountBalanceItemDTO();
                                    gpab2.BalanceItems = new List<GrossProfitBalanceItemDTO>();

                                    gpab2.AccountId = gpab.AccountId;
                                    gpab2.GrossProfitInternalAccountId = gpab.GrossProfitInternalAccountId;

                                    gpab2.BalanceItems.Add(dto);

                                    if (item2s.Any(k => k.Key == item.Key))
                                    {
                                        var en = item2s.FirstOrDefault(k => k.Key == item.Key).Value;
                                        en.Add(gpab2);
                                    }
                                    else
                                    {
                                        gpabs.Add(gpab2);
                                        item2s.Add(item.Key, gpabs);
                                    }


                                    //Create temp VoucherRow 
                                    VoucherRowDTO grossVoucherRow = new VoucherRowDTO()
                                    {
                                        Text = voucherRow.Text,
                                        Date = voucherRow.Date,
                                        Quantity = voucherRow.Quantity,
                                        Merged = voucherRow.Merged,
                                        Amount = balance,
                                        AmountEntCurrency = balanceEntCurrency,
                                        Dim1Id = voucherRow.Dim1Id,
                                    };

                                    if (grossVoucherRow.AccountInternalDTO_forReports == null)
                                        grossVoucherRow.AccountInternalDTO_forReports = new List<AccountInternalDTO>();

                                    if (voucherRow.AccountInternalDTO_forReports != null)
                                        grossVoucherRow.AccountInternalDTO_forReports.AddRange(voucherRow.AccountInternalDTO_forReports);

                                    grossVoucherRows.Add(grossVoucherRow);

                                }
                            }
                        }
                    }

                    #endregion
                }

                if (grossVoucherRows.Any())
                {
                    //Create temp VoucherHead
                    VoucherHeadDTO grossVoucherHead = new VoucherHeadDTO()
                    {
                        VoucherNr = voucherHead.VoucherNr,
                        Date = voucherHead.Date,
                        Text = voucherHead.Text,
                        Status = voucherHead.Status,
                        TypeBalance = voucherHead.TypeBalance,
                        VatVoucher = voucherHead.VatVoucher,
                        Note = voucherHead.Note,
                        AccountIds = grossVoucherRows.Where(i => i.Amount != 0).Select(i => i.Dim1Id).Distinct().ToList(),
                    };
                    if (grossVoucherHead.Rows == null)
                        grossVoucherHead.Rows = new List<VoucherRowDTO>();

                    grossVoucherHead.Rows.AddRange(grossVoucherRows);
                    grossVoucherBudgetHeads.Add(grossVoucherHead);
                }
            }

            return item2s;
        }

        public BalanceItemDTO GetBalanceChange(CompEntities entities, AccountYear accountYear, DateTime dateFrom, DateTime dateTo, AccountStd accountStd, List<AccountInternal> accountInternals, int actorCompanyId, bool includeVatVoucher = true, bool forceClearCache = false)
        {
            BalanceItemDTO balanceItem = new BalanceItemDTO();

            if (accountYear == null || accountStd == null || accountStd.Account == null)
                return balanceItem;

            if (cachedActorCompanyId != actorCompanyId || forceClearCache)
                ClearCache();

            balanceItem.AccountId = accountStd.AccountId;

            List<VoucherHead> voucherHeads = (List<VoucherHead>)voucherHeadCache[accountYear.AccountYearId];
            if (voucherHeads == null)
            {
                #region Load VoucherHeads

                voucherHeads = (from vh in entities.VoucherHead
                                    .Include("VoucherRow.AccountInternal")
                                where vh.ActorCompanyId == actorCompanyId &&
                                vh.AccountPeriod.AccountYearId == accountYear.AccountYearId &&
                                vh.VoucherSeries.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                                !vh.VoucherSeries.VoucherSeriesType.Template &&
                                vh.Date >= dateFrom && vh.Date <= dateTo
                                select vh).ToList();

                //filter by dates without hours done separately, because linq is not supported in entities
                //voucherHeads = voucherHeads.Where(i => i.Date.Date >= dateFrom && i.Date.Date <= dateTo).ToList();

                if (!includeVatVoucher)
                    voucherHeads = voucherHeads.Where(i => !i.VatVoucher.HasValue || !i.VatVoucher.Value).ToList();

                #endregion

                //Add to cache
                voucherHeadCache.Add(accountYear.AccountYearId, voucherHeads);
            }

            foreach (VoucherHead voucherHead in voucherHeads)
            {
                foreach (VoucherRow voucherRow in voucherHead.VoucherRow)
                {
                    //Skip inactive or deleted rows
                    if (voucherRow.State != (int)SoeEntityState.Active)
                        continue;

                    if (voucherRow.AccountId == accountStd.AccountId)
                    {
                        if (accountInternals.IsNullOrEmpty())
                        {
                            balanceItem.Balance += voucherRow.Amount;
                            balanceItem.BalanceEntCurrency += voucherRow.AmountEntCurrency;
                            if (voucherRow.Quantity.HasValue)
                                if (balanceItem.Quantity != null)
                                    balanceItem.Quantity += voucherRow.Quantity.Value;
                                else
                                    balanceItem.Quantity = voucherRow.Quantity.Value;
                        }
                        else if (accountInternals.Count != voucherRow.AccountInternal.Count)
                        {
                            continue;
                        }
                        else
                        {
                            bool exist = false;
                            foreach (AccountInternal accountInternalOuter in accountInternals)
                            {
                                foreach (AccountInternal accountInternalInner in voucherRow.AccountInternal)
                                {
                                    if (accountInternalOuter.AccountId == accountInternalInner.AccountId)
                                    {
                                        exist = true;
                                        break;
                                    }
                                }

                                if (!exist)
                                    break;
                            }

                            if (exist)
                            {
                                //Balance
                                balanceItem.Balance += voucherRow.Amount;
                                balanceItem.BalanceEntCurrency += voucherRow.AmountEntCurrency;

                                //Quantity
                                if (voucherRow.Quantity.HasValue)
                                    if (balanceItem.Quantity != null)
                                        balanceItem.Quantity += voucherRow.Quantity.Value;
                                    else
                                        balanceItem.Quantity = voucherRow.Quantity.Value;
                            }
                        }
                    }
                }
            }

            return balanceItem;
        }

        public BalanceItemDTO GetBalanceChangeMatchInternals(CompEntities entities, AccountYear accountYear, DateTime dateFrom, DateTime dateTo, AccountStd accountStd, List<AccountInternal> accountInternals, int actorCompanyId, bool checkInternals, bool includeVatVoucher = true)
        {
            List<VoucherHead> filteredVoucherHeads = null;
            BalanceItemDTO balanceItem = new BalanceItemDTO();

            if (accountYear == null || accountStd == null || accountStd.Account == null)
                return balanceItem;

            balanceItem.AccountId = accountStd.AccountId;

            List<VoucherHead> voucherHeads = (List<VoucherHead>)voucherHeadCache[accountYear.AccountYearId];
            if (voucherHeads == null)
            {
                #region Load VoucherHeads

                voucherHeads = (from vh in entities.VoucherHead
                                    .Include("VoucherRow.AccountInternal")
                                where vh.ActorCompanyId == actorCompanyId &&
                                vh.AccountPeriod.AccountYearId == accountYear.AccountYearId &&
                                (vh.Date >= accountYear.From && vh.Date <= accountYear.To) &&
                                vh.VoucherSeries.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                                !vh.VoucherSeries.VoucherSeriesType.Template
                                select vh).ToList();

                if (!includeVatVoucher)
                    voucherHeads = voucherHeads.Where(i => !i.VatVoucher.HasValue || !i.VatVoucher.Value).ToList();

                #endregion

                //Add to cache
                voucherHeadCache.Add(accountYear.AccountYearId, voucherHeads);
            }

            filteredVoucherHeads = voucherHeads.Where(v => v.Date >= dateFrom && v.Date <= dateTo).ToList();

            foreach (VoucherHead voucherHead in filteredVoucherHeads)
            {
                foreach (VoucherRow voucherRow in voucherHead.VoucherRow)
                {
                    //Skip inactive or deleted rows
                    if (voucherRow.State != (int)SoeEntityState.Active)
                        continue;

                    if (voucherRow.AccountId == accountStd.AccountId)
                    {
                        bool matched = false;

                        if (accountInternals.Count > 0)
                        {
                            foreach (AccountInternal accountInternalOuter in accountInternals)
                            {
                                foreach (AccountInternal accountInternalInner in voucherRow.AccountInternal)
                                {
                                    if (accountInternalOuter.AccountId == accountInternalInner.AccountId)
                                    {
                                        matched = true;
                                        break;
                                    }
                                }

                                if (!matched)
                                    break;
                            }
                        }
                        else
                        {
                            if (checkInternals)
                            {
                                if (voucherRow.AccountInternal.Count == 0)
                                    matched = true;
                            }
                            else
                            {
                                matched = true;
                            }
                        }

                        if (matched)
                        {
                            //Balance
                            balanceItem.Balance += voucherRow.Amount;
                            balanceItem.BalanceEntCurrency += voucherRow.AmountEntCurrency;

                            //Quantity
                            if (voucherRow.Quantity.HasValue)
                                if (balanceItem.Quantity != null)
                                    balanceItem.Quantity += voucherRow.Quantity.Value;
                                else
                                    balanceItem.Quantity = voucherRow.Quantity.Value;
                        }
                    }
                }
            }

            return balanceItem;
        }

        public BalanceItemDTO GetBalanceChangeForVatAccount(CompEntities entities, int actorCompanyId, int vatNr, List<AccountYear> accountYears, DateTime dateFrom, DateTime dateTo, List<SysVatAccount> sysVatAccounts, List<AccountStd> accountStds, bool includeVatVoucher = true, bool importVat = false, bool truncateDecimals = false)
        {
            BalanceItemDTO balanceItem = new BalanceItemDTO();
            foreach (AccountYear accountYear in accountYears)
            {
                BalanceItemDTO currentBalanceItem = GetBalanceChangeForVatAccount(entities, actorCompanyId, vatNr, accountYear, dateFrom, dateTo, sysVatAccounts, accountStds, includeVatVoucher, importVat);
                if (currentBalanceItem != null)
                {
                    balanceItem.Balance += currentBalanceItem.Balance;
                    balanceItem.BalanceEntCurrency += currentBalanceItem.BalanceEntCurrency;

                    if (currentBalanceItem.Quantity.HasValue)
                        if (balanceItem.Quantity != null)
                            balanceItem.Quantity += currentBalanceItem.Quantity.Value;
                        else
                            balanceItem.Quantity = currentBalanceItem.Quantity.Value;
                }
            }
            if (truncateDecimals)
            {
                balanceItem.Balance = decimal.Truncate(balanceItem.Balance);
                balanceItem.BalanceEntCurrency = decimal.Truncate(balanceItem.BalanceEntCurrency);
            }

            return balanceItem;
        }

        public BalanceItemDTO GetBalanceChangeForVatAccount(CompEntities entities, int actorCompanyId, int vatNr, AccountYear accountYear, DateTime dateFrom, DateTime dateTo, List<SysVatAccount> sysVatAccounts, List<AccountStd> accountStds, bool includeVatVoucher = true, bool importVat = false)
        {
            BalanceItemDTO balanceItem = new BalanceItemDTO();

            if (accountYear == null)
                return balanceItem;

            if (sysVatAccounts == null)
                sysVatAccounts = SysDbCache.Instance.SysVatAccounts;
            if (accountStds == null)
                accountStds = AccountManager.GetAccountStdsByCompany(actorCompanyId, null);

            if (importVat)
            {
                foreach (SysVatAccount sysVatAccount in sysVatAccounts.Where(a => (a.AccountCode == "IM") && ((a.VatNr1.HasValue && a.VatNr1.Value == vatNr) || (a.VatNr2.HasValue && a.VatNr2.Value == vatNr))))
                {
                    foreach (AccountStd accountStd in accountStds.Where(a => a.SysVatAccountId.HasValue && a.SysVatAccountId.Value == sysVatAccount.SysVatAccountId))
                    {
                        BalanceItemDTO balanceItemAccount = GetBalanceChange(entities, accountYear, dateFrom, dateTo, accountStd, null, actorCompanyId, includeVatVoucher);
                        if (balanceItemAccount != null)
                        {
                            balanceItem.Balance += balanceItemAccount.Balance;
                            balanceItem.BalanceEntCurrency += balanceItemAccount.BalanceEntCurrency;
                            if (balanceItemAccount.Quantity.HasValue)
                                if (balanceItem.Quantity != null)
                                    balanceItem.Quantity += balanceItemAccount.Quantity.Value;
                                else
                                    balanceItem.Quantity = balanceItemAccount.Quantity.Value;
                        }
                    }
                }
            }
            else
            {
                foreach (SysVatAccount sysVatAccount in sysVatAccounts.Where(a => (a.AccountCode != "IM") && ((a.VatNr1.HasValue && a.VatNr1.Value == vatNr) || (a.VatNr2.HasValue && a.VatNr2.Value == vatNr))))
                {
                    foreach (AccountStd accountStd in accountStds.Where(a => a.SysVatAccountId.HasValue && a.SysVatAccountId.Value == sysVatAccount.SysVatAccountId))
                    {
                        BalanceItemDTO balanceItemAccount = GetBalanceChange(entities, accountYear, dateFrom, dateTo, accountStd, null, actorCompanyId, includeVatVoucher);
                        if (balanceItemAccount != null)
                        {
                            balanceItem.Balance += balanceItemAccount.Balance;
                            balanceItem.BalanceEntCurrency += balanceItemAccount.BalanceEntCurrency;
                            if (balanceItemAccount.Quantity.HasValue)
                                if (balanceItem.Quantity != null)
                                    balanceItem.Quantity += balanceItemAccount.Quantity.Value;
                                else
                                    balanceItem.Quantity = balanceItemAccount.Quantity.Value;
                        }
                    }
                }
            }

            return balanceItem;
        }

        public void GetAccountsAndInternalsForBalance(CompEntities entities, AccountYear accountYear, int actorCompanyId, bool includeInternals, ref Dictionary<int, bool> usedAccounts, ref List<int> usedInternals)
        {
            List<VoucherHead> voucherHeads = (List<VoucherHead>)voucherHeadCache[accountYear.AccountYearId];
            if (voucherHeads == null)
            {
                #region Load VoucherHeads

                voucherHeads = (from vh in entities.VoucherHead
                                    .Include("VoucherRow.AccountInternal")
                                where vh.ActorCompanyId == actorCompanyId &&
                                vh.AccountPeriod.AccountYearId == accountYear.AccountYearId &&
                                vh.VoucherSeries.VoucherSeriesType.State == (int)SoeEntityState.Active &&
                                !vh.VoucherSeries.VoucherSeriesType.Template
                                select vh).ToList();

                #endregion

                //Add to cache
                voucherHeadCache.Add(accountYear.AccountYearId, voucherHeads);
            }

            foreach (VoucherHead voucherHead in voucherHeads)
            {
                foreach (VoucherRow voucherRow in voucherHead.VoucherRow)
                {
                    //Skip inactive or deleted rows
                    if (voucherRow.State != (int)SoeEntityState.Active)
                        continue;

                    if (!usedAccounts.Keys.Contains(voucherRow.AccountId))
                    {
                        usedAccounts.Add(voucherRow.AccountId, voucherRow.AccountInternal.Count > 0);
                    }
                    else
                    {
                        if (voucherRow.AccountInternal.Count > 0)
                            usedAccounts[voucherRow.AccountId] = true;
                    }

                    if (includeInternals)
                    {
                        foreach (AccountInternal accountInternalInner in voucherRow.AccountInternal)
                        {
                            if (!usedInternals.Contains(accountInternalInner.AccountId))
                            {
                                usedInternals.Add(accountInternalInner.AccountId);
                            }
                        }
                    }
                }
            }
        }

        #region Help-methods

        private void InitBalanceItemDict(List<AccountStd> accountStds, ref Dictionary<int, BalanceItemDTO> dict)
        {
            if (dict == null)
                dict = new Dictionary<int, BalanceItemDTO>();

            if (accountStds != null)
            {
                foreach (var accountId in accountStds.Select(x => x.AccountId))
                {
                    dict[accountId] = new BalanceItemDTO()
                    {
                        AccountId = accountId,
                    };
                }
            }
        }

        private void InitBalanceItemDict(List<AccountDTO> accountStds, ref Dictionary<int, BalanceItemDTO> dict)
        {
            if (dict == null)
                dict = new Dictionary<int, BalanceItemDTO>();

            if (accountStds != null)
            {
                foreach (var accountId in accountStds.Select(x => x.AccountId))
                {
                    dict[accountId] = new BalanceItemDTO()
                    {
                        AccountId = accountId,
                    };
                }
            }
        }

        private void UpdateBalanceItemsFromVoucherHead(List<VoucherHead> voucherHeads, Dictionary<int, BalanceItemDTO> dict, List<AccountDim> accountDimInternals, List<AccountInternal> accountInternals, bool ignoreValidation = false)
        {
            if (voucherHeads == null)
                return;

            foreach (var voucherHeadsRows in voucherHeads.Select(h => h.VoucherRow))
            {
                if (voucherHeadsRows == null)
                    continue;

                List<VoucherRow> voucherRows = voucherHeadsRows.Where(i => i.State == (int)SoeEntityState.Active && dict.ContainsKey(i.AccountId)).ToList();
                foreach (VoucherRow voucherRow in voucherRows)
                {
                    bool valid = ignoreValidation || AccountManager.IsAccountInternalInIntervalRange(accountDimInternals, accountInternals, voucherRow.AccountInternal?.ToList());
                    if (valid)
                        UpdateBalanceItemFromVoucherRow(voucherRow, dict);
                }
            }
        }

        private void UpdateBalanceItemsFromVoucherHeadDTO(List<VoucherHeadDTO> voucherHeads, Dictionary<int, BalanceItemDTO> dict, List<AccountDimDTO> accountDimInternals, List<AccountInternalDTO> accountInternals, bool ignoreValidation = false, bool onlyEmptyInternals = false)
        {
            if (voucherHeads == null)
                return;

            foreach (VoucherHeadDTO voucherHead in voucherHeads)
            {
                if (voucherHead.Rows == null)
                    continue;

                List<VoucherRowDTO> voucherRows = voucherHead.Rows.Where(i => i.State == (int)SoeEntityState.Active && dict.ContainsKey(i.Dim1Id)).ToList();
                foreach (VoucherRowDTO voucherRow in voucherRows)
                {
                    List<AccountInternalDTO> accountInternalDTOs = voucherRow.AccountInternalDTO_forReports;

                    bool valid = ignoreValidation || (onlyEmptyInternals ? (accountInternalDTOs == null || accountInternalDTOs.Count == 0) : AccountManager.IsAccountInternalDTOInIntervalRange(accountDimInternals, accountInternals, accountInternalDTOs));
                    if (valid)
                        UpdateBalanceItemFromVoucherRowDTO(voucherRow, dict);
                }
            }
        }

        private void UpdateBalanceItemFromVoucherRow(VoucherRow voucherRow, Dictionary<int, BalanceItemDTO> dict)
        {
            if (voucherRow == null)
                return;

            //Get from dict
            BalanceItemDTO balanceItem = dict[voucherRow.AccountId];
            if (balanceItem != null)
            {
                //Update AccountStd
                balanceItem.Balance += voucherRow.Amount;
                balanceItem.BalanceEntCurrency += voucherRow.AmountEntCurrency;
                if (voucherRow.Quantity.HasValue)
                    if (balanceItem.Quantity != null)
                        balanceItem.Quantity += voucherRow.Quantity.Value;
                    else
                        balanceItem.Quantity = voucherRow.Quantity.Value;

                if (voucherRow.AccountInternal != null && voucherRow.AccountInternal.Any())
                {
                    //Should already be loaded but hängslen och livrem....!
                    foreach (var accountInternal in voucherRow.AccountInternal)
                    {
                        if (accountInternal.Account != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                        {
                            accountInternal.Account.AccountDimReference.Load();
                        }
                    }

                    BalanceItemInternalDTO balanceItemInternal = balanceItem.GetBalanceItemInternal(voucherRow.AccountInternal.ToDTOs());
                    if (balanceItemInternal != null)
                    {
                        //Update AccountInternal
                        balanceItemInternal.Balance += voucherRow.Amount;
                        balanceItemInternal.BalanceEntCurrency += voucherRow.AmountEntCurrency;
                        if (voucherRow.Quantity.HasValue)
                            if (balanceItemInternal.Quantity != null)
                                balanceItemInternal.Quantity += voucherRow.Quantity.Value;
                            else
                                balanceItemInternal.Quantity = voucherRow.Quantity.Value;
                    }
                }

                //Update dict
                dict.Remove(voucherRow.AccountId);
                dict.Add(voucherRow.AccountId, balanceItem);
            }
        }

        private void UpdateBalanceItemFromVoucherRowDTO(VoucherRowDTO voucherRow, Dictionary<int, BalanceItemDTO> dict)
        {
            if (voucherRow == null)
                return;

            //Get from dict
            BalanceItemDTO balanceItem = dict[voucherRow.Dim1Id];
            if (balanceItem != null)
            {
                //Update AccountStd
                balanceItem.Balance += voucherRow.Amount;
                balanceItem.BalanceEntCurrency += voucherRow.AmountEntCurrency;
                if (voucherRow.Quantity.HasValue)
                    if (balanceItem.Quantity != null)
                        balanceItem.Quantity += voucherRow.Quantity.Value;
                    else
                        balanceItem.Quantity = voucherRow.Quantity.Value;

                if (voucherRow.AccountInternalDTO_forReports != null && voucherRow.AccountInternalDTO_forReports.Count > 0)
                {
                    BalanceItemInternalDTO balanceItemInternal = balanceItem.GetBalanceItemInternal(voucherRow.AccountInternalDTO_forReports);
                    if (balanceItemInternal != null)
                    {
                        //Update AccountInternal
                        balanceItemInternal.Balance += voucherRow.Amount;
                        balanceItemInternal.BalanceEntCurrency += voucherRow.AmountEntCurrency;
                        if (voucherRow.Quantity.HasValue)
                            if (balanceItemInternal.Quantity != null)
                                balanceItemInternal.Quantity += voucherRow.Quantity.Value;
                            else
                                balanceItemInternal.Quantity = voucherRow.Quantity.Value;
                    }
                }

                //Update dict
                dict.Remove(voucherRow.Dim1Id);
                dict.Add(voucherRow.Dim1Id, balanceItem);
            }
        }

        #endregion

        #endregion

        #region AccountBalance

        public List<AccountBalance> GetAccountBalancesByYear(int accountYearId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountBalance.NoTracking();
            return GetAccountBalancesByYear(entities, accountYearId);
        }

        public List<AccountBalance> GetAccountBalancesByYear(CompEntities entities, int accountYearId)
        {
            return (from a in entities.AccountBalance
                    where a.AccountYear.AccountYearId == accountYearId
                    select a).ToList();
        }

        public List<AccountBalance> GetAccountBalanceByAccount(int accountId, bool loadYear = false)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.AccountBalance.NoTracking();
            return GetAccountBalanceByAccount(entities, accountId, loadYear);
        }

        public List<AccountBalance> GetAccountBalanceByAccount(CompEntities entities, int accountId, bool loadYear = false)
        {
            if (loadYear)
            {
                return (from ab in entities.AccountBalance
                            .Include("AccountYear")
                        where ab.AccountId == accountId &&
                        ab.AccountYear.Status == (int)TermGroup_AccountStatus.Open
                        orderby ab.AccountYear.From
                        select ab).ToList();
            }
            else
            {
                return (from ab in entities.AccountBalance
                        where ab.AccountId == accountId &&
                        ab.AccountYear.Status == (int)TermGroup_AccountStatus.Open
                        orderby ab.AccountYear.From
                        select ab).ToList();
            }
        }

        public Dictionary<int, decimal> GetAccountBalancesByYearDict(int accountYearId)
        {
            Dictionary<int, decimal> dict = new Dictionary<int, decimal>();

            List<AccountBalance> balances = GetAccountBalancesByYear(accountYearId);
            foreach (AccountBalance balance in balances)
            {
                dict.Add(balance.AccountId, balance.Balance);
            }

            return dict;
        }

        private AccountBalance GetAccountBalance(CompEntities entities, int accountId, int accountYearId)
        {
            return (from ab in entities.AccountBalance
                    where ab.AccountId == accountId &&
                    ab.AccountYearId == accountYearId
                    select ab).FirstOrDefault();
        }

        /// <summary>
        /// Updates AccountBalance for a given Account
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="accountId">The AccountId</param>
        /// <param name="accountYearId">The AccountYearId</param>
        /// <param name="amount">The amount to add/update</param>
        /// <param name="addToExistingBalance">Will keep the current balance and increase (or decrease if negative) it with amount</param>
        /// <param name="saveChanges">Decide if changes should be saved to database or only ObjectContext</param>
        /// <param name="balances">List of AccountBalances to check before database</param>
        /// <returns>ActionResult</returns>
        public ActionResult SetAccountBalance(CompEntities entities, int actorCompanyId, int accountId, int accountYearId, decimal amount, bool addToExistingBalance, bool saveChanges, List<AccountBalance> balances)
        {
            ActionResult result = new ActionResult();

            AccountBalance balance = null;
            if (balances != null && !saveChanges)
            {
                balance = (from b in balances
                           where ((b.AccountId == accountId) || (b.AccountStd != null && b.AccountStd.AccountId == accountId)) &&
                           ((b.AccountYearId == accountYearId) || (b.AccountYear != null && b.AccountYear.AccountYearId == accountYearId))
                           select b).FirstOrDefault();
            }

            if (balance == null)
                balance = GetAccountBalance(entities, accountId, accountYearId);

            //decimal(15,2) max value
            decimal maxValue = Convert.ToDecimal(9999999999999.99);
            decimal newBalance = 0;

            if (balance == null)
            {
                // Get AccountYearBalanceHead for Account (ingående balans)
                var balanceHeads = GetAccountYearBalanceHeadsByAccountId(entities, accountYearId, accountId);
                foreach (var balanceHead in balanceHeads)
                {
                    newBalance += balanceHead.Balance;
                }

                //Add new amount
                newBalance += amount;

                if (newBalance <= maxValue)
                {
                    var accountStd = AccountManager.GetAccountStd(entities, accountId, actorCompanyId, true, false);

                    if (accountStd != null)
                    {
                        // Create new AccountBalance
                        balance = new AccountBalance()
                        {
                            AccountStd = accountStd,
                            AccountYear = AccountManager.GetAccountYear(entities, accountYearId, false),
                            Balance = newBalance,
                        };
                        SetCreatedProperties(balance);

                        if (saveChanges)
                        {
                            result = AddEntityItem(entities, balance, "AccountBalance");
                            if (!result.Success)
                                return result;
                        }
                        else
                        {
                            entities.AccountBalance.AddObject(balance);
                            if (balances != null)
                                balances.Add(balance);
                        }
                    }
                }
            }
            else
            {
                if (addToExistingBalance)
                {
                    //Preserve existing AccountBalance
                    newBalance = balance.Balance;
                }
                else
                {
                    //Reset existing AcccountBalance to AccountBalanceYearHead's value (ingående balans)
                    var balanceHeads = GetAccountYearBalanceHeadsByAccountId(entities, accountYearId, accountId);
                    foreach (var balanceHead in balanceHeads)
                    {
                        newBalance += balanceHead.Balance;
                    }
                }

                //Add new amount
                newBalance += amount;

                if (newBalance <= maxValue)
                {
                    balance.Balance = newBalance;
                    SetModifiedProperties(balance);

                    if (saveChanges)
                    {
                        result = SaveEntityItem(entities, balance);
                    }
                    else
                    {
                        if (balances != null)
                            balances.Add(balance);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Calcuate AccountBalance

        /// <summary>
        /// Calculate AccountBalance for a Account in all open AccountYears
        /// (Account:AccountYear = 1:M)
        /// </summary>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="accountId">The AccountId</param>
        public ActionResult CalculateAccountBalanceForAccountInAccountYears(int actorCompanyId, int accountId)
        {
            ActionResult result = new ActionResult();
            using (CompEntities entities = new CompEntities())
            {
                // Get open AccountYears
                List<AccountYear> accountYears = AccountManager.GetAccountYears(entities, actorCompanyId, true, false);
                foreach (AccountYear accountYear in accountYears)
                {
                    result = CalculateAccountBalanceForAccountInAccountYear(entities, accountId, accountYear.AccountYearId, actorCompanyId);
                    if (!result.Success)
                        return result;
                }
            }
            return result;
        }

        public void CalculateAccountBalanceForAccountsFromVoucher(int actorCompanyId, int accountYearId, int savedMinutesBack)
        {
            using (CompEntities entities = new CompEntities())
            {
                DateTime time = DateTime.Now.AddMinutes(-savedMinutesBack);
                List<VoucherHead> voucherHeads = (from vh in entities.VoucherHead // skip template
                                                    .Include("VoucherRow")
                                                    .Include("VoucherRow.AccountStd")
                                                    .Include("VoucherRow.AccountStd.Account")
                                                  where vh.AccountPeriod.AccountYearId == accountYearId &&
                                                  !vh.Template &&
                                                  ((vh.Created.HasValue && vh.Created.Value > time) || (vh.Modified.HasValue && vh.Modified.Value > time))
                                                  select vh).ToList();

                List<AccountStd> accountStds = new List<AccountStd>();
                foreach (VoucherHead voucherHead in voucherHeads)
                {
                    foreach (VoucherRow voucherRow in voucherHead.VoucherRow)
                    {
                        if (voucherRow.AccountStd == null || voucherRow.AccountStd.Account == null || voucherRow.State != (int)SoeEntityState.Active || voucherRow.AccountStd.Account.State != (int)SoeEntityState.Active)
                            continue;

                        if (!accountStds.Any(i => i.AccountId == voucherRow.AccountStd.AccountId))
                            accountStds.Add(voucherRow.AccountStd);
                    }
                }

                CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, accountYearId, accountStds);
            }
        }

        /// <summary>
        /// Calculate AccountBalance for all Accounts in a given Company and AccountYears
        /// (Account:AccountYear = M:1)
        /// </summary>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="accountYearId">The AccountYearId. If null the current AccountYear is calcuated</param>
        /// <returns>ActionResult</returns>
        public ActionResult CalculateAccountBalanceForAccounts(int actorCompanyId, int? accountYearId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, null);
                return CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, accountYearId, accountStds);
            }
        }

        /// <summary>
        /// Calculate AccountBalance for all given Accounts and AccountYear
        /// (Account:AccountYear = M:1)
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="accountYearId">The AccountYearId. If null the current AccountYear is calcuated</param>
        /// <param name="accountStds">The AccountStd collection to calculate AccountBalance for</param>
        /// <returns>ActionResult</returns>
        public ActionResult CalculateAccountBalanceForAccountsInAccountYear(CompEntities entities, int actorCompanyId, int? accountYearId, List<AccountStd> accountStds)
        {
            ActionResult result = new ActionResult();
            if (accountStds == null)
                return result;

            if (!accountYearId.HasValue)
            {
                //Get current AccountYear
                AccountYear currentAccountYear = AccountManager.GetCurrentAccountYear(actorCompanyId);
                if (currentAccountYear != null)
                    accountYearId = currentAccountYear.AccountYearId;
            }

            foreach (AccountStd accountStd in accountStds)
            {
                result = CalculateAccountBalanceForAccountInAccountYear(entities, accountStd.AccountId, accountYearId ?? 0, actorCompanyId);
                if (!result.Success)
                    break;
            }

            return result;
        }

        public ActionResult CalculateAccountBalanceForAccountsInAccountYearOptimized(CompEntities entities, int actorCompanyId, int? accountYearId, List<AccountStd> accountStds)
        {
            ActionResult result = new ActionResult();
            if (accountStds == null)
                return result;

            if (!accountYearId.HasValue)
            {
                //Get current AccountYear
                AccountYear currentAccountYear = AccountManager.GetCurrentAccountYear(actorCompanyId);
                if (currentAccountYear != null)
                    accountYearId = currentAccountYear.AccountYearId;
            }

            var accountIds = accountStds.Select(a => a.AccountId).ToHashSet();
            var sums = entities.VoucherRow
                .Where(vr => accountIds.Contains(vr.AccountId))
                .Where(vr => vr.VoucherHead.AccountPeriod.AccountYearId == accountYearId &&
                             !vr.VoucherHead.Template &&
                             vr.State == (int)SoeEntityState.Active &&
                             vr.AccountStd.Account.State == (int)SoeEntityState.Active)
                .GroupBy(vr => vr.AccountId)
                .Select(gr => new { AccountId = gr.Key, Sum = gr.Sum(vr => vr.Amount) });

            foreach (var accountSum in sums)
            {

                // Update AccountBalance
                var setAccountResult = SetAccountBalance(entities, actorCompanyId, accountSum.AccountId, accountYearId ?? 0, accountSum.Sum, false, true, null);

                if (!setAccountResult.Success)
                    break;
            }

            return result;

        }

        public void CalculateAccountBalanceForAccountsFromAccountYear(int actorCompanyId, int? accountYearId, List<Account> accounts)
        {
            using (CompEntities entities = new CompEntities())
            {
                CalculateAccountBalanceForAccountsInAccountYear(entities, actorCompanyId, accountYearId, accounts);
            }
        }

        /// <summary>
        /// Calculate AccountBalance for all given Accounts and AccountYear
        /// (Account:AccountYear = M:1)
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <param name="accountYearId">The AccountYearId. If null the current AccountYear is calcuated</param>
        /// <param name="accountStds">The AccountStd collection to calculate AccountBalance for</param>
        /// <returns>ActionResult</returns>
        public ActionResult CalculateAccountBalanceForAccountsInAccountYear(CompEntities entities, int actorCompanyId, int? accountYearId, List<Account> accounts)
        {
            ActionResult result = new ActionResult();
            if (accounts == null)
                return result;

            if (!accountYearId.HasValue)
            {
                //Get current AccountYear
                AccountYear currentAccountYear = AccountManager.GetCurrentAccountYear(actorCompanyId);
                if (currentAccountYear != null)
                    accountYearId = currentAccountYear.AccountYearId;
            }

            foreach (Account account in accounts)
            {
                result = CalculateAccountBalanceForAccountInAccountYear(entities, account.AccountId, accountYearId ?? 0, actorCompanyId);
                if (!result.Success)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Calculate AccountBalance for a given Account in a given AccountYear
        /// (Account:AccountYear = 1:1)
        /// </summary>
        /// <param name="entities">The ObjectContext</param>
        /// <param name="accountId">The AccountId</param>
        /// <param name="accountYearId">The AccountYearId</param>
        /// <param name="actorCompanyId">The ActorCompanyId</param>
        /// <returns>ActionResult</returns>
        public ActionResult CalculateAccountBalanceForAccountInAccountYear(CompEntities entities, int accountId, int accountYearId, int actorCompanyId)
        {
            // Get all voucher rows for specified account and current account year ( skip template )
            var sum = (from vr in entities.VoucherRow
                       where vr.AccountId == accountId &&
                       vr.VoucherHead.AccountPeriod.AccountYearId == accountYearId &&
                       !vr.VoucherHead.Template &&
                       vr.State == (int)SoeEntityState.Active &&
                       vr.AccountStd.Account.State == (int)SoeEntityState.Active
                       select vr).Sum(r => (decimal?)r.Amount) ?? 0;

            // Update AccountBalance
            return SetAccountBalance(entities, actorCompanyId, accountId, accountYearId, sum, false, true, null);
        }

        public void CalculateAccountBalanceForAccountsFromAccountYear(int actorCompanyId, int? accountYearId)
        {
            using (CompEntities entities = new CompEntities())
            {
                List<AccountStd> accountStds = AccountManager.GetAccountStdsByCompany(entities, actorCompanyId, true);
                CalculateAccountBalanceForAccountsFromAccountYear(entities, actorCompanyId, accountYearId, accountStds);
            }
        }

        public void CalculateAccountBalanceForAccountsFromAccountYear(CompEntities entities, int actorCompanyId, int? accountYearId, List<AccountStd> accountStds)
        {
            if (accountStds == null)
                return;

            if (!accountYearId.HasValue)
            {
                //Get current AccountYear
                AccountYear currentAccountYear = AccountManager.GetCurrentAccountYear(actorCompanyId);
                if (currentAccountYear != null)
                    accountYearId = currentAccountYear.AccountYearId;
            }

            foreach (AccountStd accountStd in accountStds)
            {
                CalculateAccountBalanceForAccountFromAccountYear(entities, accountStd.AccountId, accountYearId ?? 0, actorCompanyId);
            }
        }

        public ActionResult CalculateAccountBalanceForAccountFromAccountYear(CompEntities entities, int accountId, int accountYearId, int actorCompanyId)
        {
            decimal sum = 0M;
            List<AccountYear> accountYears = AccountManager.GetAccountYears(actorCompanyId, false, false);
            AccountYear startAccountYear = accountYears.First(i => i.AccountYearId == accountYearId);

            foreach (AccountYear accountYear in accountYears)
            {
                if (accountYear.AccountYearId == startAccountYear.AccountYearId || accountYear.From > startAccountYear.To)
                    sum = CalculateAccountBalanceForAccountInAccountYearNoUpdate(entities, accountId, accountYearId, actorCompanyId);
            }

            // Update AccountBalance
            return SetAccountBalance(entities, actorCompanyId, accountId, accountYearId, sum, false, true, null);
        }

        public decimal CalculateAccountBalanceForAccountInAccountYearNoUpdate(CompEntities entities, int accountId, int accountYearId, int actorCompanyId)
        {
            // Get all voucher rows for specified account and current account year ( skip template ) 
            return (from vr in entities.VoucherRow
                    where vr.AccountId == accountId &&
                    vr.VoucherHead.ActorCompanyId == actorCompanyId &&
                    vr.VoucherHead.AccountPeriod.AccountYearId == accountYearId &&
                    !vr.VoucherHead.Template &&
                    vr.State == (int)SoeEntityState.Active &&
                    vr.AccountStd.Account.State == (int)SoeEntityState.Active
                    select vr).Sum(r => (decimal?)r.Amount) ?? 0;
        }

        #endregion

        #region GrossProfitCodeBalance

        private List<GrossProfitBalanceItemDTO> GetGrossProfitBalanceItemsFromDTO(DateTime fromDate, DateTime toDate, GrossProfitCodeDTO grossProfitCode)
        {
            List<GrossProfitBalanceItemDTO> list = new List<GrossProfitBalanceItemDTO>();

            // TODO FIX THIS?
            //if (grossProfitCode.AccountYear.From.Year != fromDate.Year)
            //    return list;

            if (fromDate.Year == toDate.Year)
            {
                int fromMonth = fromDate.Month;
                int toMonth = toDate.Month;



                for (int i = fromMonth; i <= toMonth; i++)
                {
                    DateTime fromDateTime = new DateTime(fromDate.Year, i, 1);
                    DateTime toDateTime = new DateTime(fromDate.Year, i, DateTime.DaysInMonth(fromDate.Year, i));
                    decimal value = (decimal)grossProfitCode.GetType().GetProperty("Period" + i.ToString()).GetValue(grossProfitCode, null);

                    GrossProfitBalanceItemDTO dto = list.FirstOrDefault(g => g.PeriodFrom == fromDateTime && g.PeriodTo == toDateTime);

                    if (dto == null)
                    {
                        dto = new GrossProfitBalanceItemDTO()
                        {
                            GrossProfitCodeId = grossProfitCode.GrossProfitCodeId,
                            PeriodFrom = fromDateTime,
                            PeriodTo = toDateTime,
                            PeriodGrossProfitPercentage = value,
                        };

                        list.Add(dto);
                    }
                    else
                    {
                        dto.PeriodGrossProfitPercentage = value;
                    }
                }
            }
            else
            {
                int fromYear = fromDate.Year;
                int toYear = toDate.Year;
                int fromMonth = fromDate.Month;
                int toMonth = toDate.Month;

                int noOfYears = (toYear - fromYear) + 1;
                decimal yearPercentageValue = 0;

                for (int i = 1; i <= noOfYears; i++)
                {
                    for (int ix = 1; ix <= 12; ix++)
                    {
                        decimal value = (decimal)grossProfitCode.GetType().GetProperty("Period" + ix.ToString()).GetValue(grossProfitCode, null);

                        if (i == 1)
                        {
                            if (ix >= fromMonth)
                            {
                                DateTime fromDateTime = new DateTime(fromDate.Year, ix, 1);
                                DateTime toDateTime = new DateTime(fromDate.Year, ix, DateTime.DaysInMonth(fromDate.Year, ix));
                                GrossProfitBalanceItemDTO dto = list.FirstOrDefault(g => g.PeriodFrom == fromDateTime && g.PeriodTo == toDateTime);

                                if (dto == null)
                                {
                                    dto = new GrossProfitBalanceItemDTO()
                                    {
                                        GrossProfitCodeId = grossProfitCode.GrossProfitCodeId,
                                        PeriodFrom = fromDateTime,
                                        PeriodTo = toDateTime,
                                        PeriodGrossProfitPercentage = value,
                                    };

                                    list.Add(dto);
                                }
                                else
                                {
                                    dto.PeriodGrossProfitPercentage = value;
                                }
                            }
                        }
                        else if (i == noOfYears)
                        {
                            if (ix <= toMonth)
                            {
                                DateTime fromDateTime = new DateTime(toYear, ix, 1);
                                DateTime toDateTime = new DateTime(toYear, ix, DateTime.DaysInMonth(fromDate.AddYears(i - 1).Year, ix));
                                GrossProfitBalanceItemDTO dto = list.FirstOrDefault(g => g.PeriodFrom == fromDateTime && g.PeriodTo == toDateTime);

                                if (dto == null)
                                {
                                    dto = new GrossProfitBalanceItemDTO()
                                    {
                                        GrossProfitCodeId = grossProfitCode.GrossProfitCodeId,
                                        PeriodFrom = fromDateTime,
                                        PeriodTo = toDateTime,
                                        PeriodGrossProfitPercentage = value,
                                    };

                                    list.Add(dto);
                                }
                                else
                                {
                                    dto.PeriodGrossProfitPercentage = value;
                                }
                            }

                            yearPercentageValue += value;
                        }
                        else
                        {
                            DateTime fromDateTime = new DateTime(fromDate.AddYears(i - 1).Year, ix, 1);
                            DateTime toDateTime = new DateTime(fromDate.Year, ix, DateTime.DaysInMonth(fromDate.AddYears(i - 1).Year, ix));
                            GrossProfitBalanceItemDTO dto = list.FirstOrDefault(g => g.PeriodFrom == fromDateTime && g.PeriodTo == toDateTime);

                            if (dto == null)
                            {
                                dto = new GrossProfitBalanceItemDTO()
                                {
                                    GrossProfitCodeId = grossProfitCode.GrossProfitCodeId,
                                    PeriodFrom = fromDateTime,
                                    PeriodTo = toDateTime,
                                    PeriodGrossProfitPercentage = value,
                                };

                                list.Add(dto);
                            }
                            else
                            {
                                dto.PeriodGrossProfitPercentage = value;
                            }
                        }
                    }
                }
            }

            return list;
        }



        #endregion

        #region Help-classes

        public class BalanceChangeVoucherHeadCache
        {
            #region Variables

            private readonly List<BalanceChangeVoucherHeadListItemCache> items = null;

            #endregion

            #region Ctor

            public BalanceChangeVoucherHeadCache()
            {
                this.items = new List<BalanceChangeVoucherHeadListItemCache>();
            }

            #endregion

            #region Public methods

            public void AddItem(int accountYearId, DateTime dateFrom, DateTime dateTo, List<VoucherHead> voucherHeads)
            {
                if (Exists(accountYearId, dateFrom, dateTo))
                    return;

                this.items.Add(new BalanceChangeVoucherHeadListItemCache(accountYearId, dateFrom, dateTo, voucherHeads));
            }

            public BalanceChangeVoucherHeadListItemCache GetItem(int accountYearId, DateTime dateFrom, DateTime dateTo)
            {
                var item = this.items.FirstOrDefault(i => i.AccountYearId == accountYearId && i.DateFrom == dateFrom && i.DateTo == dateTo);
                if (item == null)
                {
                    var overlappingItem = this.items.FirstOrDefault(i => i.AccountYearId == accountYearId && i.DateFrom <= dateFrom && i.DateTo >= dateTo);
                    if (overlappingItem != null)
                    {
                        item = new BalanceChangeVoucherHeadListItemCache(accountYearId, dateFrom, dateTo, overlappingItem.VoucherHeads.Where(i => i.Date >= dateFrom && i.Date <= dateTo).ToList(), true);
                        this.items.Add(item);
                    }
                }
                return item;
            }

            public bool Exists(int accountYearId, DateTime dateFrom, DateTime dateTo)
            {
                return this.items.Any(i => i.AccountYearId == accountYearId && i.DateFrom == dateFrom.Date && i.DateTo == dateTo.Date);
            }

            public List<string> GetLogs()
            {
                List<string> logs = new List<string>();

                foreach (var item in this.items)
                {
                    logs.Add(String.Format("AID:{0},FROM={1},TO={2},RES={3},CACHE={4}", item.AccountYearId, item.DateFrom.ToShortDateString(), item.DateTo.ToShortDateString(), item.VoucherHeads.Count, item.OriginFromCache.ToInt()));
                }

                return logs;
            }

            #endregion
        }

        public class BalanceChangeVoucherHeadListItemCache
        {
            #region Variables

            public int AccountYearId { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public List<VoucherHead> VoucherHeads { get; set; }
            public bool OriginFromCache { get; set; }

            #endregion

            #region Ctor

            public BalanceChangeVoucherHeadListItemCache(int accountYearId, DateTime dateFrom, DateTime dateTo, List<VoucherHead> voucherHeads, bool originFromCache = false)
            {
                if (voucherHeads == null)
                    voucherHeads = new List<VoucherHead>();

                this.AccountYearId = accountYearId;
                this.DateFrom = dateFrom;
                this.DateTo = dateTo;
                this.VoucherHeads = voucherHeads;
                this.OriginFromCache = originFromCache;

                foreach (VoucherHead voucherHead in this.VoucherHeads.Where(i => !i.AccountIdsHandled))
                {
                    voucherHead.AccountIds = voucherHead.VoucherRow.Where(i => i.Amount != 0).Select(i => i.AccountId).Distinct().ToList();
                    voucherHead.AccountIdsHandled = true;
                }
            }

            #endregion
        }

        public class BalanceChangeVoucherHeadDTOCache
        {
            #region Variables

            private readonly List<BalanceChangeVoucherHeadDTOListItemCache> items = null;

            #endregion

            #region Ctor

            public BalanceChangeVoucherHeadDTOCache()
            {
                this.items = new List<BalanceChangeVoucherHeadDTOListItemCache>();
            }

            #endregion

            #region Public methods

            public void AddItem(int accountYearId, DateTime dateFrom, DateTime dateTo, List<VoucherHeadDTO> voucherHeads)
            {
                if (Exists(accountYearId, dateFrom, dateTo))
                    return;

                this.items.Add(new BalanceChangeVoucherHeadDTOListItemCache(accountYearId, dateFrom, dateTo, voucherHeads));
            }

            public BalanceChangeVoucherHeadDTOListItemCache GetItem(int accountYearId, DateTime dateFrom, DateTime dateTo)
            {
                var item = this.items.FirstOrDefault(i => i.AccountYearId == accountYearId && i.DateFrom == dateFrom && i.DateTo == dateTo);
                if (item == null)
                {
                    var overlappingItem = this.items.FirstOrDefault(i => i.AccountYearId == accountYearId && i.DateFrom <= dateFrom && i.DateTo >= dateTo);
                    if (overlappingItem != null)
                    {
                        item = new BalanceChangeVoucherHeadDTOListItemCache(accountYearId, dateFrom, dateTo, overlappingItem.VoucherHeads.Where(i => i.Date >= dateFrom && i.Date <= dateTo).ToList(), true);
                        this.items.Add(item);
                    }
                }
                return item;
            }

            public bool Exists(int accountYearId, DateTime dateFrom, DateTime dateTo)
            {
                return this.items.Any(i => i.AccountYearId == accountYearId && i.DateFrom == dateFrom.Date && i.DateTo == dateTo.Date);
            }

            #endregion
        }

        public class BalanceChangeVoucherHeadDTOListItemCache
        {
            #region Variables

            public int AccountYearId { get; set; }
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public List<VoucherHeadDTO> VoucherHeads { get; set; }
            public bool OriginFromCache { get; set; }

            #endregion

            #region Ctor

            public BalanceChangeVoucherHeadDTOListItemCache(int accountYearId, DateTime dateFrom, DateTime dateTo, List<VoucherHeadDTO> voucherHeads, bool originFromCache = false)
            {
                if (voucherHeads == null)
                    voucherHeads = new List<VoucherHeadDTO>();

                this.AccountYearId = accountYearId;
                this.DateFrom = dateFrom;
                this.DateTo = dateTo;
                this.VoucherHeads = voucherHeads;
                this.OriginFromCache = originFromCache;

                foreach (VoucherHeadDTO voucherHead in this.VoucherHeads.Where(i => !i.AccountIdsHandled))
                {
                    voucherHead.AccountIds = voucherHead.Rows.Where(i => i.Amount != 0).Select(i => i.Dim1Id).Distinct().ToList();
                    voucherHead.AccountIdsHandled = true;
                }
            }

            #endregion
        }



        #endregion
    }
}
