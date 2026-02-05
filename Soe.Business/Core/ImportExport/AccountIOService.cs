using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.ImportSpecials;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Business.Core.ManagerFacades;
using Account = SoftOne.Soe.Data.Account;

namespace SoftOne.Soe.Business.Core.ImportExport
{
    public class AccountIOService
    {
        private const int DIM_NR_STD = 1;

        private static readonly Dictionary<string, TermGroup_AccountType> SieKpTypMappings = new Dictionary<string, TermGroup_AccountType>(StringComparer.OrdinalIgnoreCase)
        {
            // Single letter codes
            { "K", TermGroup_AccountType.Cost },
            { "I", TermGroup_AccountType.Income },
            { "S", TermGroup_AccountType.Debt },
            { "T", TermGroup_AccountType.Asset },
            
            // Numeric codes
            { "4", TermGroup_AccountType.Cost },
            { "3", TermGroup_AccountType.Income },
            { "2", TermGroup_AccountType.Debt },
            { "1", TermGroup_AccountType.Asset },
            
            // English names
            { "Cost", TermGroup_AccountType.Cost },
            { "Income", TermGroup_AccountType.Income },
            { "Debt", TermGroup_AccountType.Debt },
            { "Asset", TermGroup_AccountType.Asset },
            
            // Swedish names
            { "Kostnad", TermGroup_AccountType.Cost },
            { "Intäkt", TermGroup_AccountType.Income },
            { "Skuld", TermGroup_AccountType.Debt },
            { "Tillgång", TermGroup_AccountType.Asset }
        };

        private static readonly List<string> IgnoreDimCreationForSpecialFunctionality = new List<string> { "ICADepartmentMapping" };

        public AccountIOService(AccountManager accountManager, ITextService textService, ILoggerService loggerService)
        {
            AccountManager = accountManager;
            TextService = textService;
            LoggerService = loggerService;
        }

        private AccountManager AccountManager { get; }
        private ITextService TextService { get; }
        private ILoggerService LoggerService { get; }

        public ActionResult ImportFromAccountIO(List<AccountIODTO> accountIOs, int actorCompanyId, string specialFunctionality)
        {
            var operationActionResults = new List<(ActionResult, Account)>();
            var oldAccountDims = AccountManager.GetAccountDimsByCompany(actorCompanyId, false, false, true);

            SetMissingDimNrToAccountIoByDimSieNr(accountIOs, oldAccountDims);

            using (CompEntities entities = new CompEntities())
            {
                if (IgnoreDimCreationForSpecialFunctionality.All(ignored => !specialFunctionality.Contains(ignored)))
                    CreateMissingDims(accountIOs, oldAccountDims, entities, actorCompanyId);

                var accountStds = AccountManager.GetAccountsStdsByCompany(entities, actorCompanyId);
                var accountInternals = AccountManager.GetAccountInternals(actorCompanyId, null, true);
                var sysVatAccounts = SysDbCache.Instance.SysVatAccounts;
                var sysAccountSruCodes = SysDbCache.Instance.SysAccountSruCodes;

                if (ValidateAccountParentDeclarations(accountIOs) is ActionResult ard && !ard.Success)
                    return ard;

                if (ValidateParentExistence(accountIOs, accountStds, accountInternals, entities, actorCompanyId) is ActionResult are && !are.Success)
                    return are;

                var dims = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, onlyInternal: false, onlyStandard: false, active: null, loadAccounts: true);

                foreach (var accountIO in accountIOs)
                {
                    var dim = dims.FirstOrDefault(d => d.AccountDimNr == accountIO.AccountDimNr);

                    if (dim == null)
                        continue;

                    var result = CreateOrUpdateAccount(accountIO, dim, actorCompanyId, sysVatAccounts, sysAccountSruCodes, accountStds, accountInternals, oldAccountDims);
                    operationActionResults.Add(result);
                }
            }

            if (operationActionResults.Any(r => !r.Item1.Success))
                return GetClientErrorResponse(operationActionResults);

            return new ActionResult();
        }

        private (ActionResult, Account account) CreateOrUpdateAccount(AccountIODTO accountIO, AccountDim dim,
            int actorCompanyId, List<SysVatAccount> sysVatAccounts, List<SysAccountSruCode> sysAccountSruCodes,
            List<Account> accountStds, List<AccountInternal> accountInternals, List<AccountDim> oldAccountDims)
        {
            var account = dim.Account.FirstOrDefault(a => a.AccountNr == accountIO.AccountNr);
            var accExists = account != null;
            var isAccountStd = accountIO.AccountDimNr == 1;

            if (!accExists)
                account = new Account();

            account.AccountNr = accountIO.AccountNr;
            account.Name = accountIO.Name;
            account.Created = accountIO.Created != CalendarUtility.DATETIME_DEFAULT ? (DateTime?)accountIO.Created : null;
            account.CreatedBy = accountIO.CreatedBy;
            account.ActorCompanyId = actorCompanyId;
            account.ExternalCode = accountIO.ExternalCode;

            if (!string.IsNullOrEmpty(accountIO.ParentAccountNr) && dim.Parent != null)
            {
                var parentIsAccountStd = dim.Parent.AccountDimNr == 1;
                if (parentIsAccountStd)
                {
                    var mapToAccount = accountStds.FirstOrDefault(a => a.AccountNr == accountIO.ParentAccountNr);
                    if (mapToAccount != null)
                        account.ParentAccountId = mapToAccount.AccountId;
                }
                else
                {
                    var mapToAccount = accountInternals.FirstOrDefault(a => a.Account.AccountNr == accountIO.ParentAccountNr);
                    if (mapToAccount != null)
                        account.ParentAccountId = mapToAccount.AccountId;
                }
            }

            if (isAccountStd)
            {
                var accountResult = (AddOrUpdateStandardAccount(accountIO, account, accExists, sysVatAccounts, sysAccountSruCodes, dim, actorCompanyId), account);

                if (accExists)
                {
                    var hasDim2Config = !string.IsNullOrEmpty(accountIO.AccountDim2Default) || accountIO.AccountDim2Mandatory || accountIO.AccountDim2Stop;
                    if (hasDim2Config && oldAccountDims.FirstOrDefault(x => x.AccountDimNr == 2) is AccountDim aDim2)
                        UpdateDimLevelForStandardAccount(account, actorCompanyId, accountIO, aDim2, 2);

                    var hasDim3Config = !string.IsNullOrEmpty(accountIO.AccountDim3Default) || accountIO.AccountDim3Mandatory || accountIO.AccountDim3Stop;
                    if (hasDim3Config && oldAccountDims.FirstOrDefault(x => x.AccountDimNr == 3) is AccountDim aDim3)
                        UpdateDimLevelForStandardAccount(account, actorCompanyId, accountIO, aDim3, 3);
                }

                return accountResult;
            }

            if (accExists)
            {
                account.AccountInternalReference.Load();
                return (AccountManager.UpdateAccount(account, actorCompanyId, 0), account);
            }
            else
            {
                account.AccountInternal = new AccountInternal();
                return (AccountManager.AddAccount(account, dim.AccountDimId, actorCompanyId, 0), account);
            }
        }

        private ActionResult GetClientErrorResponse(List<(ActionResult, Account)> results)
        {
            if (results.All(rkvp => rkvp.Item1.Success))
                return new ActionResult();

            string GetClientErrorAccountLabel(Account ac) => ac != null ? $"{ac.Name} (Id: {ac.AccountId})" : "Null";

            var accountNotFoundErrors = results.Where(r => !r.Item1.Success && r.Item1.ErrorNumber == (int)ActionResultSave.EntityNotFound).ToList();
            var otherErrors = results.Where(r => !r.Item1.Success && r.Item1.ErrorNumber != (int)ActionResultSave.EntityNotFound).ToList();

            var accountsNotFoundErrorText = TextService.GetText(10, "Konton som inte hittades för uppdatering:");
            var otherErrorsText = TextService.GetText(12, "Konton med ospecifierade fel:");
            var errorMessage = "";

            if (accountNotFoundErrors.Any())
                errorMessage += $"{accountsNotFoundErrorText}\r\n" + string.Join("\r\n ", accountNotFoundErrors.Select(e => "• " + GetClientErrorAccountLabel(e.Item2)).ToList());

            if (otherErrors.Any())
                errorMessage += $"\r\n{otherErrorsText}\r\n" + string.Join("\r\n ", otherErrors.Select(e => "• " + GetClientErrorAccountLabel(e.Item2)).ToList());

            var errorResult = new ActionResult
            {
                Success = false,
                ErrorMessage = errorMessage.Trim()
            };

            return errorResult;
        }

        private ActionResult AddOrUpdateStandardAccount(AccountIODTO accountIO, Account account, bool accExists, List<SysVatAccount> sysVatAccounts, List<SysAccountSruCode> sysAccountSruCodes, AccountDim accountDim, int actorCompanyId)
        {
            int? sysVatAccountId = null;
            if (sysVatAccounts.Any(a => a.VatNr1.ToString() == accountIO.SysVatAccountNr) && accountIO.SysVatAccountNr != null)
                sysVatAccountId = sysVatAccounts.FirstOrDefault(a => a.VatNr1.ToString() == accountIO.SysVatAccountNr).SysVatAccountId;
            else if (accountIO.SysVatAccountNr != null)
                sysVatAccountId = SOPPP.GetSysVatAccountFromSOPCode(accountIO.SysVatAccountNr, sysVatAccounts);

            int accountType = GetAccountTypeFromSieKpTyp(accountIO.SieKpTyp);

            AccountStd accountStd;
            if (accExists)
            {
                if (!account.AccountStdReference.IsLoaded)
                    account.AccountStdReference.Load();
                if (account.AccountStd != null)
                    accountStd = account.AccountStd;
                else
                    accountStd = new AccountStd();
            }
            else
                accountStd = new AccountStd();

            accountStd.Account = account;
            accountStd.Unit = accountIO.QuantityUnit;
            accountStd.UnitStop = accountIO.QuantityStop;
            accountStd.SysVatAccountId = sysVatAccountId;
            accountStd.SieKpTyp = accountIO.SieKpTyp;
            accountStd.AmountStop = accountIO.AmountStop ? 1 : 0;
            accountStd.AccountTypeSysTermId = accountType;

            if (accountIO.SruCode1 != "0" && sysAccountSruCodes.Any(s => s.SruCode == accountIO.SruCode1))
            {
                AccountSru accountSru = new AccountSru();
                accountSru.AccountStd = accountStd;
                accountSru.SysAccountSruCodeId = sysAccountSruCodes.FirstOrDefault(s => s.SruCode == accountIO.SruCode1).SysAccountSruCodeId;
                accountStd.AccountSru.Add(accountSru);
            }

            if (accountIO.SruCode2 != "0" && sysAccountSruCodes.Any(s => s.SruCode == accountIO.SruCode2))
            {
                AccountSru accountSru = new AccountSru();
                accountSru.AccountStd = accountStd;
                accountSru.SysAccountSruCodeId = sysAccountSruCodes.FirstOrDefault(s => s.SruCode == accountIO.SruCode2).SysAccountSruCodeId;
                accountStd.AccountSru.Add(accountSru);
            }

            if (!accExists)
                account.AccountStd = accountStd;

            if (accExists)
                return AccountManager.UpdateAccount(account, actorCompanyId, 0);
            else
                return AccountManager.AddAccount(account, accountDim.AccountDimId, actorCompanyId, 0);
        }

        private static int GetAccountTypeFromSieKpTyp(string sieKpTyp)
        {
            if (string.IsNullOrEmpty(sieKpTyp))
                return (int)TermGroup_AccountType.Asset;

            if (SieKpTypMappings.TryGetValue(sieKpTyp, out var accountType))
                return (int)accountType;

            return (int)TermGroup_AccountType.Asset;
        }

        /// <summary>
        /// Updates the dimension level for a standard account.
        /// Errors here are only logged, not returned to the client as this would require significant refactoring efforts.
        /// </summary>
        private void UpdateDimLevelForStandardAccount(Account account, int actorCompanyId, AccountIODTO accountIO, AccountDim oldAccountDim, int dimLevel)
        {
            var accountDimMandatory = dimLevel == 2 ? accountIO.AccountDim2Mandatory : dimLevel == 3 ? accountIO.AccountDim3Mandatory : throw new ArgumentException("Must be either 2 or 3", nameof(dimLevel));
            var accountDimStop = dimLevel == 2 ? accountIO.AccountDim2Stop : dimLevel == 3 ? accountIO.AccountDim3Stop : throw new ArgumentException("Must be either 2 or 3", nameof(dimLevel));
            var accountDimDefault = dimLevel == 2 ? accountIO.AccountDim2Default : dimLevel == 3 ? accountIO.AccountDim3Default : throw new ArgumentException("Must be either 2 or 3", nameof(dimLevel));

            List<AccountInternal> accountInternals = AccountManager.GetAccountInternalsByDim(oldAccountDim.AccountDimId, actorCompanyId, active: true);
            var accountInternal = accountInternals.FirstOrDefault(a => a.Account.AccountNr == accountDimDefault && a.Account.AccountDimId == oldAccountDim.AccountDimId);
            int defAccountId = accountInternal?.AccountId ?? 0;

            var map = AccountManager.GetAccountMapping(account.AccountId, oldAccountDim.AccountDimId, actorCompanyId, onlyActiveAccount: true, loadAccount: false, loadAccountDim: true, loadAccountInternal: true);
            if (map == null)
            {
                map = new AccountMapping();

                AccountManager.AddAccountMapping(map, account.AccountId, oldAccountDim.AccountDimId, defAccountId, actorCompanyId);
                if (accountDimMandatory || accountDimStop)
                {
                    var mapDim = AccountManager.GetAccountMapping(account.AccountId, oldAccountDim.AccountDimId, actorCompanyId, onlyActiveAccount: true, loadAccount: false, loadAccountDim: true, loadAccountInternal: true);
                    if (accountDimMandatory)
                        mapDim.MandatoryLevel = (int?)TermGroup_AccountMandatoryLevel.Mandatory;
                    if (accountDimStop)
                        mapDim.MandatoryLevel = (int?)TermGroup_AccountMandatoryLevel.Stop;

                    var updateResult = AccountManager.UpdateAccountMapping(mapDim, actorCompanyId, mapDim.MandatoryLevel, defAccountId);
                    if (!updateResult.Success)
                        LoggerService.LogWarning(updateResult.ErrorMessage);
                }
            }
            else
            {
                map.MandatoryLevel = (int?)TermGroup_AccountMandatoryLevel.None;
                if (accountDimMandatory)
                    map.MandatoryLevel = (int?)TermGroup_AccountMandatoryLevel.Mandatory;
                else if (accountDimStop)
                    map.MandatoryLevel = (int?)TermGroup_AccountMandatoryLevel.Stop;

                if (!string.IsNullOrEmpty(accountDimDefault))
                    map.DefaultAccountId = accountInternal?.AccountId;

                int defaultAccountId = map.DefaultAccountId ?? 0;
                var updateResult = AccountManager.UpdateAccountMapping(map, actorCompanyId, map.MandatoryLevel, defaultAccountId);
                if (!updateResult.Success)
                    LoggerService.LogWarning(updateResult.ErrorMessage);
            }
        }

        private static void SetMissingDimNrToAccountIoByDimSieNr(List<AccountIODTO> accountIOs, List<AccountDim> oldAccountDims)
        {
            var accountIOsWithNewDimensions = accountIOs.Where(aio => oldAccountDims.All(d => d.AccountDimNr != aio.AccountDimNr));
            var whereDim0 = accountIOsWithNewDimensions.Where(aio => aio.AccountDimNr == 0);

            foreach (var accountIO in whereDim0)
            {
                var siedim = oldAccountDims.FirstOrDefault(d => d.SysSieDimNr == accountIO.AccountDimSieNr);
                if (oldAccountDims.Any(d => d.SysSieDimNr == accountIO.AccountDimSieNr))
                {
                    accountIO.AccountDimNr = siedim.AccountDimNr;
                }
            }
        }

        private static void CreateMissingDims(List<AccountIODTO> accountIOs, List<AccountDim> oldAccountDims, CompEntities entities, int actorCompanyId)
        {
            var accountIOsWithNewDimensions = accountIOs.Where(aio => oldAccountDims.All(d => d.AccountDimNr != aio.AccountDimNr));
            var missingDims = accountIOsWithNewDimensions.Select(aio => (aio.AccountDimNr, aio.AccountDimSieNr, aio.AccountDimName)).ToList();

            if (!missingDims.Any())
                return;

            ValidateDimConsistency(missingDims);

            var distinct = missingDims.Distinct().ToList();

            foreach (var newDim in distinct)
            {
                var dim = new AccountDim
                {
                    AccountDimNr = newDim.AccountDimNr,
                    ActorCompanyId = actorCompanyId,
                    ShortName = newDim.AccountDimName != "" ? newDim.AccountDimName : newDim.AccountDimNr.ToString(),
                    Name = newDim.AccountDimName != "" ? newDim.AccountDimName : newDim.AccountDimNr.ToString(),
                    SysSieDimNr = newDim.AccountDimSieNr
                };

                entities.AccountDim.AddObject(dim);
            }

            entities.SaveChanges();
        }

        private static void ValidateDimConsistency(List<(int AccountDimNr, int AccountDimSieNr, string AccountDimName)> missingDims)
        {
            var dimVariations = missingDims
                .GroupBy(d => d.AccountDimNr)
                .Select(g => new
                {
                    DimNr = g.Key,
                    SieNrs = g.Select(d => d.AccountDimSieNr).Distinct().ToList(),
                    Names = g.Select(d => d.AccountDimName).Distinct().ToList()
                })
                .Where(x => x.SieNrs.Count > 1 || x.Names.Count > 1)
                .ToList();

            if (dimVariations.Any())
            {
                var messages = new List<string>();

                foreach (var dim in dimVariations)
                {
                    if (dim.SieNrs.Count > 1)
                        messages.Add($"Kontonivå {dim.DimNr} har olika SIE-nr: {string.Join(", ", dim.SieNrs)}.");

                    if (dim.Names.Count > 1)
                        messages.Add($"Kontonivå {dim.DimNr} har olika namn: {string.Join(", ", dim.Names)}.");
                }

                var message = "Inkonsekvent data vid import av kontonivåer: " + string.Join(" ", messages);

                throw new InvalidOperationException(message);
            }
        }

        private ActionResult ValidateParentExistence(List<AccountIODTO> accountIOs, List<Account> accountStds, List<AccountInternal> accountInternals, CompEntities entities, int actorCompanyId)
        {
            if (ValidateAccountParentDeclarations(accountIOs) is ActionResult ar && !ar.Success)
                return ar;

            var parentData = accountIOs
                .Where(a => a.ParentAccountNr != string.Empty)
                .Select(a => new { a.ParentAccountNr, a.AccountDimNr })
                .Distinct()
                .Select(p => new { p.ParentAccountNr, AccountManager.GetAccountDimByNr(entities, p.AccountDimNr, actorCompanyId, includeParent: true).Parent })
                .ToList();

            var errors = new List<string>();
            foreach (var p in parentData)
            {
                if (p.Parent == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, "Kontonivå att mappa konton som underkonton mot saknas");

                if (p.Parent.AccountDimNr == DIM_NR_STD && accountStds.All(a => a.AccountNr != p.ParentAccountNr))
                    errors.Add(string.Format(TextService.GetText(7693, "Standardkonto med kontonummer {0} att mappa mot saknas."), p.ParentAccountNr));
                else if (!accountInternals.Any(a => a.Account.AccountNr == p.ParentAccountNr && a.Account.AccountDim.AccountDimNr == p.Parent.AccountDimNr))
                    errors.Add(string.Format(TextService.GetText(7694, "Internkonto med kontonummer {0} att mappa mot saknas."), p.ParentAccountNr));
            }

            if (errors.Any())
                return new ActionResult((int)ActionResultSave.NothingSaved, string.Join("\r\n", errors));

            return new ActionResult();
        }

        private ActionResult ValidateAccountParentDeclarations(List<AccountIODTO> accountIOs)
        {
            var parentSpec = accountIOs
                .Where(a => a.ParentAccountNr != string.Empty)
                .Select(a => new { a.ParentAccountNr, a.AccountDimNr })
                .ToList();

            var parentDataConsistencyData = parentSpec.GroupBy(p => p.ParentAccountNr);
            var inconsistencies = parentDataConsistencyData.Where(g => g.Select(p => p.AccountDimNr).Distinct().Count() > 1).ToList();

            if (!inconsistencies.Any())
                return new ActionResult();

            var errorMessages = new List<string>();
            foreach (var inconsistency in inconsistencies)
            {
                var dimNrs = string.Join(", ", inconsistency.Select(p => p.AccountDimNr).Distinct());
                errorMessages.Add(string.Format(TextService.GetText(13, "Föräldrakonto med kontonummer {0} har olika kontonivåer: {1}."), inconsistency.Key, dimNrs));
            }

            return new ActionResult((int)ActionResultSave.NothingSaved, string.Join("\r\n", errorMessages));
        }
    }
}
