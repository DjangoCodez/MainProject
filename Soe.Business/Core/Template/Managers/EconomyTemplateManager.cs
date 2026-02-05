using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Economy;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Transactions;
using static System.Data.Entity.Infrastructure.Design.Executor;
using ZXing;

namespace SoftOne.Soe.Business.Core.Template.Managers
{
    public class EconomyTemplateManager : ManagerBase
    {
        private readonly CompanyTemplateManager companyTemplateManager;
        private readonly AttestTemplateManager attestTemplateManager;
        private readonly CoreTemplateManager coreTemplateManager;

        public EconomyTemplateManager(ParameterObject parameterObject) : base(parameterObject)
        {
            companyTemplateManager = new CompanyTemplateManager(parameterObject);
            attestTemplateManager = new AttestTemplateManager(parameterObject);
            coreTemplateManager = new CoreTemplateManager(parameterObject);
        }

        public TemplateCompanyEconomyDataItem GetTemplateCompanyEconomyDataItem(CopyFromTemplateCompanyInputDTO inputDTO)
        {
            TemplateCompanyEconomyDataItem item = new TemplateCompanyEconomyDataItem();

            item.AccountDimCopyItems = GetAccountDimCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountStds))
                item.AccountStdCopyItems = GetAccountStdCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountInternals))
                item.AccountInternalCopyItems = GetAccountInternalCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.VoucherSeriesTypes))
                item.VoucherSeriesTypeCopyItems = GetVoucherSeriesTypeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountYearsAndPeriods))
                item.AccountYearCopyItems = GetAccountYearCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.PaymentMethods))
                item.PaymentMethodCopyItems = GetPaymentMethodCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.PaymentConditions))
                item.PaymentConditionCopyItems = GetPaymentConditionCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.GrossProfitCodes))
                item.GrossProfitCodeCopyItems = GetGrossProfitCodeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.Inventory))
            {
                item.InventoryWriteOffMethodCopyItems = GetInventoryWriteOffMethodsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
                item.InventoryWriteOffTemplateCopyItems = GetInventoryWriteOffTemplateCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);
            }

            if (inputDTO.DoCopy(TemplateCompanyCopy.VatCodes))
                item.VatCodeCopyItems = GetVatCodeCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.DoCopy(TemplateCompanyCopy.AutomaticAccountDistributionTemplates))
                item.AutoAccountDistributionCopyItems = GetAccountDistribitionCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId, SoeAccountDistributionType.Auto);

            if (inputDTO.DoCopy(TemplateCompanyCopy.PeriodAccountDistributionTemplates))
                item.PeriodAccountDistributionCopyItems = GetAccountDistribitionCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId, SoeAccountDistributionType.Period);

            if (inputDTO.DoCopy(TemplateCompanyCopy.DistributionCodes))
                item.DistributionCodeHeadCopyItems = GetDistribitionCodeHeadCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.VoucherTemplates))
                item.VoucherTemplatesCopyItems = GetVoucherTemplatesCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.ResidualCodes))
                item.ResidualCodesCopyItems = GetResidualCodesCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            if (inputDTO.doCopy(TemplateCompanyCopy.Suppliers))
                item.SupplierCopyItem = GetSupplierCopyItemsFromApi(inputDTO.SysCompDbId, inputDTO.TemplateCompanyId);

            return item;
        }

        public List<TemplateResult> CopyTemplateCompanyEconomyDataItem(CopyFromTemplateCompanyInputDTO inputDTO, TemplateCompanyDataItem item)
        {
            List<TemplateResult> templateResult = new List<TemplateResult>();

            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountStds))
                templateResult.Add(CopyAccountStdsFromTemplateCompany(item));

            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountInternals) || inputDTO.DoCopyAll() || inputDTO.DoCopy(TemplateCompanyCopy.TimeSettings))
                templateResult.Add(CopyAccountDimsFromTemplateCompany(item));

            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountInternals))
                templateResult.Add(CopyAccountInternalsFromTemplateCompany(item));

            if (inputDTO.doCopy(TemplateCompanyCopy.VoucherSeriesTypes))
                templateResult.Add(CopyVoucherSeriesTypesFromTemplateCompany(item));

            if (inputDTO.DoCopy(TemplateCompanyCopy.AccountYearsAndPeriods))
                templateResult.Add(CopyAccountYearsAndPeriodsFromTemplateCompany(item));

            if (inputDTO.doCopy(TemplateCompanyCopy.PaymentMethods))
                templateResult.Add(CopyPaymentMethodsFromTemplateCompany(item));

            if (inputDTO.doCopy(TemplateCompanyCopy.PaymentConditions))
                templateResult.Add(CopyPaymentConditionsFromTemplateCompany(item));

            if (inputDTO.doCopy(TemplateCompanyCopy.GrossProfitCodes))
                templateResult.Add(CopyGrossProfitCodesFromTemplateCompany(item));

            if (inputDTO.doCopy(TemplateCompanyCopy.Inventory))
                templateResult.Add(CopyInventoryFromTemplateCompany(item));

            if (inputDTO.DoCopy(TemplateCompanyCopy.VatCodes))
                templateResult.Add(CopyVatCodesFromTemplateCompany(item));

            if (inputDTO.DoCopy(TemplateCompanyCopy.CompanyAttestSupplier))
                templateResult.Add(attestTemplateManager.CopyAttestRolesFromTemplateCompany(item, SoeModule.Economy));

            if (inputDTO.DoCopy(TemplateCompanyCopy.AutomaticAccountDistributionTemplates))
                templateResult.Add(new TemplateResult(CopyAccountDistributionTemplatesFromTemplateCompany(item, SoeAccountDistributionType.Auto)));

            if (inputDTO.DoCopy(TemplateCompanyCopy.PeriodAccountDistributionTemplates))
                templateResult.Add(new TemplateResult(CopyAccountDistributionTemplatesFromTemplateCompany(item, SoeAccountDistributionType.Period)));

            if (inputDTO.DoCopy(TemplateCompanyCopy.DistributionCodes))
                templateResult.Add(new TemplateResult(CopyDistributionCodesFromTemplateCompany(item)));

            if (inputDTO.DoCopy(TemplateCompanyCopy.VoucherTemplates))
                templateResult.Add(CopyVoucherTemplatesFromTemplateCompany(item));

            if (inputDTO.DoCopy(TemplateCompanyCopy.ResidualCodes))
                templateResult.Add(CopyResidualCodesFromTemplateCompany(item));

            if (inputDTO.DoCopy(TemplateCompanyCopy.Suppliers))
                templateResult.Add(new TemplateResult(CopySuppliersFromTemplateCompany(item)));

            return templateResult;
        }

        public TemplateResult CopyAccountDimsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    var existingAccountDims = AccountManager.GetAccountDimsByCompany(entities, templateCompanyDataItem.DestinationActorCompanyId, onlyInternal: true);

                    Company newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                        templateResult.ActionResults.Add(result);
                        return templateResult;
                    }

                    #endregion

                    foreach (var accountDimCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountDimCopyItems.Where(w => w.AccountDimNr != Constants.ACCOUNTDIM_STANDARD))
                    {
                        #region AccountDim

                        try
                        {
                            AccountDim newAccountDimInternal;
                            AccountDim existing = existingAccountDims.FirstOrDefault(w => w.AccountDimNr == accountDimCopyItem.AccountDimNr);

                            if (existing != null && templateCompanyDataItem.Update)
                            {
                                // Update existing AccountDim instance with properties from accountDimCopyItem
                                existing.Name = accountDimCopyItem.Name;
                                existing.ShortName = accountDimCopyItem.ShortName;
                                existing.SysSieDimNr = accountDimCopyItem.SysSieDimNr;
                                existing.LinkedToProject = accountDimCopyItem.LinkedToProject;
                                existing.UseInSchedulePlanning = accountDimCopyItem.UseInSchedulePlanning;
                                existing.ExcludeinAccountingExport = accountDimCopyItem.ExcludeinAccountingExport;
                                existing.ExcludeinSalaryExport = accountDimCopyItem.ExcludeinSalaryExport;
                                existing.UseVatDeduction = accountDimCopyItem.UseVatDeduction;
                                existing.MandatoryInCustomerInvoice = accountDimCopyItem.MandatoryInCustomerInvoice;
                                existing.MandatoryInOrder = accountDimCopyItem.MandatoryInOrder;
                                existing.OnlyAllowAccountsWithParent = accountDimCopyItem.OnlyAllowAccountsWithParent;

                                newAccountDimInternal = existing;
                                SetModifiedPropertiesOnEntity(newAccountDimInternal);
                            }
                            else
                            {
                                // Create new AccountDim instance and populate properties from accountDimCopyItem
                                newAccountDimInternal = new AccountDim()
                                {
                                    Company = newCompany,
                                    AccountDimNr = accountDimCopyItem.AccountDimNr,
                                    Name = accountDimCopyItem.Name,
                                    ShortName = accountDimCopyItem.ShortName,
                                    SysSieDimNr = accountDimCopyItem.SysSieDimNr,
                                    LinkedToProject = accountDimCopyItem.LinkedToProject,
                                    UseInSchedulePlanning = accountDimCopyItem.UseInSchedulePlanning,
                                    ExcludeinAccountingExport = accountDimCopyItem.ExcludeinAccountingExport,
                                    ExcludeinSalaryExport = accountDimCopyItem.ExcludeinSalaryExport,
                                    UseVatDeduction = accountDimCopyItem.UseVatDeduction,
                                    MandatoryInCustomerInvoice = accountDimCopyItem.MandatoryInCustomerInvoice,
                                    MandatoryInOrder = accountDimCopyItem.MandatoryInOrder,
                                    OnlyAllowAccountsWithParent = accountDimCopyItem.OnlyAllowAccountsWithParent,
                                };
                            }

                            templateCompanyDataItem.TemplateCompanyEconomyDataItem.AddAccountDimMapping(accountDimCopyItem.AccountDimId, newAccountDimInternal);
                        }
                        catch (Exception ex)
                        {
                            result = companyTemplateManager.LogCopyError("AccountDim", "AccountDimId", accountDimCopyItem.AccountDimId, accountDimCopyItem.AccountDimNr.ToString(), accountDimCopyItem.Name, templateCompanyDataItem.SourceActorCompanyId, templateCompanyDataItem.DestinationActorCompanyId, ex);
                        }

                        #endregion
                    }

                    var saveResult = SaveChangesWithTransaction(entities);
                    templateResult.ActionResults.Add(saveResult);
                    templateResult.ActionResults.Add(result);


                    foreach (var accountDimCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountDimCopyItems.Where(w => w.ParentId.HasValue))
                    {
                        var dim = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccountDim(accountDimCopyItem.AccountDimId);
                        var parent = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccountDim(accountDimCopyItem.ParentId.Value);

                        if (parent != null)
                        {
                            dim = entities.AccountDim.FirstOrDefault(f => f.ActorCompanyId == templateCompanyDataItem.DestinationActorCompanyId && dim.AccountDimId == f.AccountDimId);
                            var parentDim = entities.AccountDim.FirstOrDefault(f => f.ActorCompanyId == templateCompanyDataItem.DestinationActorCompanyId && f.AccountDimId == parent.AccountDimId);
                            if (dim != null && parentDim != null)
                            {
                                dim.Parent = parentDim;
                                var updateDimResult = SaveChangesWithTransaction(entities);
                                templateResult.ActionResults.Add(updateDimResult);
                            }
                        }
                    }

                    return templateResult;
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
                templateResult.ActionResults.Add(result);
                return templateResult;
            }
        }


        public List<AccountDimCopyItem> GetAccountDimCopyItems(int actorCompanyId)
        {
            List<AccountDimCopyItem> accountDimCopyItems = new List<AccountDimCopyItem>();
            var accountDims = AccountManager.GetAccountDimInternalsByCompany(actorCompanyId);

            foreach (var accountDim in accountDims)
            {
                AccountDimCopyItem accountDimCopyItem = new AccountDimCopyItem()
                {
                    AccountDimId = accountDim.AccountDimId,
                    AccountDimNr = accountDim.AccountDimNr,
                    SysSieDimNr = accountDim.SysSieDimNr,
                    Name = accountDim.Name,
                    ShortName = accountDim.ShortName,
                    LinkedToProject = accountDim.LinkedToProject,
                    UseInSchedulePlanning = accountDim.UseInSchedulePlanning,
                    ExcludeinAccountingExport = accountDim.ExcludeinAccountingExport,
                    ExcludeinSalaryExport = accountDim.ExcludeinSalaryExport,
                    UseVatDeduction = accountDim.UseVatDeduction,
                    MandatoryInCustomerInvoice = accountDim.MandatoryInCustomerInvoice,
                    MandatoryInOrder = accountDim.MandatoryInOrder,
                    OnlyAllowAccountsWithParent = accountDim.OnlyAllowAccountsWithParent,
                };

                if (!accountDim.ParentReference.IsLoaded)
                    accountDim.ParentReference.Load();

                if (accountDim.Parent != null)
                    accountDimCopyItem.ParentId = accountDim.Parent.AccountDimId;

                accountDimCopyItems.Add(accountDimCopyItem);
            }

            return accountDimCopyItems;
        }

        public List<AccountDimCopyItem> GetAccountDimCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAccountDimCopyItems(actorCompanyId);

            return economyTemplateConnector.GetAccountDimCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyAccountStdsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    #region Prereq

                    Company newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    if (newCompany == null)
                    {
                        result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));
                        return templateResult;
                    }

                    AccountDim newAccountDimStd = AccountManager.GetAccountDimStd(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    if (newAccountDimStd == null)
                    {
                        result = new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");
                        return templateResult;
                    }

                    User user = UserManager.GetUser(entities, templateCompanyDataItem.UserId);
                    if (user == null)
                    {
                        result = new ActionResult((int)ActionResultSave.EntityNotFound, "User");
                        return templateResult;
                    }

                    List<Account> existingAccounts = templateCompanyDataItem.Update ? AccountManager.GetAccountsByDim(entities, newAccountDimStd.AccountDimId, templateCompanyDataItem.DestinationActorCompanyId, true, true, false).ToList() : new List<Account>();

                    #endregion

                    foreach (var accountStdCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountStdCopyItems)
                    {
                        #region AccountStd

                        try
                        {
                            Account existingAccount = existingAccounts.FirstOrDefault(i => i.AccountNr == accountStdCopyItem.AccountNr && i.Name == accountStdCopyItem.AccountName);
                            if (existingAccount == null)
                            {
                                //Account
                                existingAccount = new Account()
                                {
                                    AccountNr = accountStdCopyItem.AccountNr,
                                    Name = accountStdCopyItem.AccountName,
                                    ExternalCode = accountStdCopyItem.ExternalCode,

                                    //Set references
                                    Company = newCompany,
                                    AccountDim = newAccountDimStd,
                                };
                                SetCreatedProperties(existingAccount);

                                //AccountStd
                                existingAccount.AccountStd = new AccountStd()
                                {
                                    AccountTypeSysTermId = accountStdCopyItem.AccountTypeSysTermId,
                                    SysVatAccountId = accountStdCopyItem.SysVatAccountId,
                                    Unit = accountStdCopyItem.Unit,
                                    UnitStop = accountStdCopyItem.UnitStop,
                                    AmountStop = accountStdCopyItem.AmountStop,
                                };


                                //AccountSRU
                                foreach (var accountSru in accountStdCopyItem.AccountSRUCopyItems)
                                {
                                    AccountSru accountSruNew = new AccountSru()
                                    {
                                        SysAccountSruCodeId = accountSru.SysAccountSruCodeId,
                                    };
                                    existingAccount.AccountStd.AccountSru.Add(accountSruNew);
                                }

                                //AccountHistory
                                AccountHistory accountHistory = new AccountHistory()
                                {
                                    Name = existingAccount.Name,
                                    AccountNr = existingAccount.AccountNr,
                                    Date = DateTime.Now,
                                    SysAccountStdTypeId = null,
                                    SieKpTyp = existingAccount.AccountStd.SieKpTyp,

                                    //Set references
                                    Account = existingAccount,
                                    User = user,
                                };

                                SetCreatedProperties(accountHistory);
                            }
                            templateCompanyDataItem.TemplateCompanyEconomyDataItem.AddAccountMapping(accountStdCopyItem.AccountId, existingAccount);
                        }
                        catch (Exception ex)
                        {
                            result = companyTemplateManager.LogCopyError("Account", "AccountId", accountStdCopyItem.AccountId, accountStdCopyItem.AccountNr, accountStdCopyItem.AccountName, templateCompanyDataItem, ex);
                        }

                        #endregion
                    }

                    result = SaveChangesWithTransaction(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("AccountStd", templateCompanyDataItem, saved: true);
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<AccountStdCopyItem> GetAccountStdCopyItems(int actorCompanyId)
        {
            List<AccountStdCopyItem> accountStdCopyItems = new List<AccountStdCopyItem>();
            var accountStds = AccountManager.GetAccountsByCompany(actorCompanyId, true, loadAccount: true, loadAccountDim: true);

            foreach (var accountStd in accountStds)
            {
                AccountStdCopyItem accountStdCopyItem = new AccountStdCopyItem()
                {
                    AccountId = accountStd.AccountId,
                    AccountNr = accountStd.AccountNr,
                    AccountName = accountStd.Name,
                    ExternalCode = accountStd.ExternalCode,
                    AccountTypeSysTermId = accountStd.AccountStd.AccountTypeSysTermId,
                    SysVatAccountId = accountStd.AccountStd.SysVatAccountId,
                    Unit = accountStd.AccountStd.Unit,
                    UnitStop = accountStd.AccountStd.UnitStop,
                    AmountStop = accountStd.AccountStd.AmountStop,
                };

                foreach (var accountSru in accountStd.AccountStd.AccountSru)
                {
                    accountStdCopyItem.AccountSRUCopyItems.Add(new AccountSRUCopyItem()
                    {
                        SysAccountSruCodeId = accountSru.SysAccountSruCodeId,
                    });
                }

                accountStdCopyItems.Add(accountStdCopyItem);
            }

            return accountStdCopyItems;
        }

        public List<AccountStdCopyItem> GetAccountStdCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAccountStdCopyItems(actorCompanyId);

            return economyTemplateConnector.GetAccountStdCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyAccountInternalsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            ActionResult result = new ActionResult();
            TemplateResult templateResult = new TemplateResult();
            var existingAccountInternals = AccountManager.GetAccountInternals(templateCompanyDataItem.DestinationActorCompanyId, true, loadDims: true);

            try
            {
                using (CompEntities entities = new CompEntities())
                {
                    foreach (var accountDimCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountDimCopyItems)
                    {
                        #region AccountDim

                        try
                        {
                            foreach (var templateAccountInternal in templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountInternalCopyItems.Where(i => i.AccountDimId == accountDimCopyItem.AccountDimId))
                            {
                                Account account = null;

                                #region AccountInternal

                                try
                                {
                                    if (existingAccountInternals.Any(i => i.Account.AccountNr == templateAccountInternal.AccountNr && i.Account.AccountDim.AccountDimNr == accountDimCopyItem.AccountDimNr))
                                    {
                                        if (!templateCompanyDataItem.Update)
                                            continue;

                                        account = entities.Account.FirstOrDefault(i => i.ActorCompanyId == templateCompanyDataItem.DestinationActorCompanyId && i.AccountNr == templateAccountInternal.AccountNr && i.AccountDim.AccountDimNr == accountDimCopyItem.AccountDimNr);

                                        if (account != null)
                                        {
                                            account.AccountNr = templateAccountInternal.AccountNr;
                                            account.Name = templateAccountInternal.Name;
                                            account.ExternalCode = templateAccountInternal.ExternalCode;
                                            account.Description = templateAccountInternal.Description;
                                            SetModifiedProperties(account);
                                        }
                                    }

                                    if (account == null)
                                    {
                                        //Account
                                        account = new Account()
                                        {
                                            AccountNr = templateAccountInternal.AccountNr,
                                            Name = templateAccountInternal.Name,
                                            ExternalCode = templateAccountInternal.ExternalCode,
                                            Description = templateAccountInternal.Description,

                                            //Set references
                                            ActorCompanyId = templateCompanyDataItem.DestinationActorCompanyId,
                                            AccountDimId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccountDim(accountDimCopyItem.AccountDimId).AccountDimId,
                                        };
                                        SetCreatedProperties(account);
                                        entities.Account.AddObject(account);

                                        //AccountInternal
                                        account.AccountInternal = new AccountInternal()
                                        {
                                        };
                                    }

                                    //AccountHistory
                                    AccountHistory accountHistory = new AccountHistory()
                                    {
                                        UserId = templateCompanyDataItem.UserId,
                                        Account = account,
                                        Name = account.Name,
                                        AccountNr = account.AccountNr,
                                        Date = DateTime.Now,
                                        SysAccountStdTypeId = null,
                                        SieKpTyp = null,
                                    };
                                    SetCreatedProperties(accountHistory);

                                    templateCompanyDataItem.TemplateCompanyEconomyDataItem.AddAccountMapping(templateAccountInternal.AccountId, account);
                                }
                                catch (Exception ex)
                                {
                                    result = companyTemplateManager.LogCopyError("AccountInternal", "AccountInternalId", templateAccountInternal.AccountId, templateAccountInternal.AccountNr, templateAccountInternal.Name, templateCompanyDataItem, ex);
                                }

                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            result = companyTemplateManager.LogCopyError("AccountInternal", "AccountDimId", accountDimCopyItem.AccountDimId, accountDimCopyItem.AccountDimNr.ToString(), accountDimCopyItem.Name, templateCompanyDataItem, ex);
                        }

                        #endregion
                    }

                    result = SaveChangesWithTransaction(entities);
                    if (!result.Success)
                        result = companyTemplateManager.LogCopyError("AccountInternal", templateCompanyDataItem, saved: true);

                    foreach (var accountInternalCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountInternalCopyItems.Where(w => w.ParentId.HasValue))
                    {
                        var acc = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(accountInternalCopyItem.AccountId);
                        var parent = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(accountInternalCopyItem.ParentId.Value);

                        if (parent != null)
                        {
                            acc = entities.Account.FirstOrDefault(f => f.ActorCompanyId == templateCompanyDataItem.DestinationActorCompanyId && acc.AccountId == f.AccountId);
                            var parentAcc = entities.Account.FirstOrDefault(f => f.ActorCompanyId == templateCompanyDataItem.DestinationActorCompanyId && f.AccountId == parent.AccountId);
                            if (acc != null && parentAcc != null)
                            {
                                acc.ParentAccountId = parentAcc.ParentAccountId;
                                var updateDimResult = SaveChangesWithTransaction(entities);
                                templateResult.ActionResults.Add(updateDimResult);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = new ActionResult(ex);
                LogError(ex.ToString());
            }

            templateResult.ActionResults.Add(result);
            return templateResult;
        }

        public List<AccountInternalCopyItem> GetAccountInternalCopyItems(int actorCompanyId)
        {
            List<AccountInternalCopyItem> accountInternalCopyItems = new List<AccountInternalCopyItem>();
            var accountInternals = AccountManager.GetAccountsByCompany(actorCompanyId, false, true, loadAccount: true, loadAccountDim: true);

            foreach (var accountInternal in accountInternals)
            {
                AccountInternalCopyItem accountInternalCopyItem = new AccountInternalCopyItem()
                {
                    AccountId = accountInternal.AccountId,
                    AccountNr = accountInternal.AccountNr,
                    Name = accountInternal.Name,
                    ExternalCode = accountInternal.ExternalCode,
                    Description = accountInternal.Description,

                    AccountDimNr = accountInternal.AccountDim.AccountDimNr,
                    AccountDimName = accountInternal.AccountDim.Name,
                    AccountDimShortName = accountInternal.AccountDim.ShortName,
                    AccountDimSysSieDimNr = accountInternal.AccountDim.SysSieDimNr,
                    AccountDimSysAccountStdTypeParentId = accountInternal.AccountDim.SysAccountStdTypeParentId,
                    AccountDimUseInSchedulePlanning = accountInternal.AccountDim.UseInSchedulePlanning,
                    AccountDimExcludeinSalaryExport = accountInternal.AccountDim.ExcludeinSalaryExport,
                    AccountDimUseVatDeduction = accountInternal.AccountDim.UseVatDeduction,
                    AccountDimLinkedToProject = accountInternal.AccountDim.LinkedToProject,
                    AccountDimLinkedToShiftType = accountInternal.AccountDim.LinkedToShiftType,
                    AccountDimId = accountInternal.AccountDimId,
                    ParentId = accountInternal.ParentAccountId

                };

                accountInternalCopyItems.Add(accountInternalCopyItem);
            }

            return accountInternalCopyItems;
        }


        public List<AccountInternalCopyItem> GetAccountInternalCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAccountInternalCopyItems(actorCompanyId);

            return economyTemplateConnector.GetAccountInternalCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyAccountYearsAndPeriodsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                //Get Company
                Company newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                if (newCompany == null)
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));

                //Always check existing, because standard voucher serie type is added elsewhere 
                List<AccountYear> existingAccountYears = AccountManager.GetAccountYears(entities, templateCompanyDataItem.DestinationActorCompanyId, false, false);

                // Get new company voucher series type
                List<VoucherSeriesType> existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(entities, templateCompanyDataItem.DestinationActorCompanyId, false);

                #endregion

                foreach (var accountYearCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountYearCopyItems)
                {
                    #region VoucherSeriesType

                    try
                    {
                        AccountYear existingAccountYear = existingAccountYears.FirstOrDefault(a => a.From == accountYearCopyItem.From && a.To == accountYearCopyItem.To && a.Status == (int)accountYearCopyItem.Status);

                        if (existingAccountYear == null)
                        {
                            existingAccountYear = new AccountYear()
                            {
                                Company = newCompany,
                                From = accountYearCopyItem.From,
                                To = accountYearCopyItem.To,
                                Status = (int)accountYearCopyItem.Status,
                                Created = DateTime.Now,
                            };

                            var result = AccountManager.AddAccountYear(entities, existingAccountYear, templateCompanyDataItem.DestinationActorCompanyId);
                            if (!result.Success)
                                result = companyTemplateManager.LogCopyError("AccountYear", templateCompanyDataItem, saved: false);

                            templateResult.ActionResults.Add(result);

                            List<AccountPeriod> existingAccountPeriods = AccountManager.GetAccountPeriods(existingAccountYear.AccountYearId, false);
                            foreach (var accountPeriodCopyItems in accountYearCopyItem.AccountPeriodCopyItems)
                            {
                                AccountPeriod existingAccountPeriod = existingAccountPeriods.FirstOrDefault(p => p.PeriodNr == accountPeriodCopyItems.PeriodNr && p.From == accountPeriodCopyItems.From && p.To == accountPeriodCopyItems.To && p.Status == (int)accountPeriodCopyItems.Status);

                                if (existingAccountPeriod == null)
                                {
                                    existingAccountPeriod = new AccountPeriod()
                                    {
                                        PeriodNr = accountPeriodCopyItems.PeriodNr,
                                        From = accountPeriodCopyItems.From,
                                        To = accountPeriodCopyItems.To,
                                        Status = (int)accountPeriodCopyItems.Status,
                                        Created = DateTime.Now,
                                    };

                                    var result2 = AccountManager.AddAccountPeriod(existingAccountPeriod, existingAccountYear);
                                    if (!result2.Success)
                                        result2 = companyTemplateManager.LogCopyError("AccountPeriod", templateCompanyDataItem, saved: false);
                                    templateResult.ActionResults.Add(result2);
                                }
                            }

                            // Voucher series
                            foreach (var serie in accountYearCopyItem.VoucherSeriesCopyItems)
                            {
                                var matchingSerieType = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == serie.VoucherSeriesTypeName);
                                if (matchingSerieType != null)
                                {
                                    VoucherSeries voucherSerie = new VoucherSeries()
                                    {
                                        Status = serie.Status,
                                        VoucherDateLatest = serie.VoucherDateLatest,
                                        VoucherNrLatest = serie.VoucherNrLatest,
                                        VoucherSeriesTypeId = matchingSerieType.VoucherSeriesTypeId
                                    };

                                    var result3 = VoucherManager.AddVoucherSeries(entities, voucherSerie, templateCompanyDataItem.DestinationActorCompanyId, existingAccountYear.AccountYearId, matchingSerieType.VoucherSeriesTypeId);
                                    if (!result3.Success)
                                        result3 = companyTemplateManager.LogCopyError("VoucherSerie", templateCompanyDataItem, saved: false);
                                    templateResult.ActionResults.Add(result3);
                                }
                            }
                        }
                        templateCompanyDataItem.TemplateCompanyEconomyDataItem.AddAccountYearMapping(accountYearCopyItem.AccountYearId, existingAccountYear);
                        
                    }
                    catch (Exception ex)
                    {
                        templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("AccountYear", templateCompanyDataItem, ex, saved: false));
                    }

                    #endregion
                }
            }

            return templateResult;

        }

        public List<AccountYearCopyItem> GetAccountYearCopyItems(int actorCompanyId)
        {
            List<AccountYearCopyItem> accountYearCopyItems = new List<AccountYearCopyItem>();
            var accountYears = AccountManager.GetAccountYears(actorCompanyId, false, loadPeriods: true, loadVoucherSeries: true, loadVoucherSeriesType: true);

            foreach (var accountYear in accountYears)
            {
                AccountYearCopyItem accountYearCopyItem = new AccountYearCopyItem()
                {
                    AccountYearId = accountYear.AccountYearId,
                    From = accountYear.From,
                    To = accountYear.To,
                    Status = (TermGroup_AccountStatus)accountYear.Status,

                };

                foreach (var period in accountYear.AccountPeriod)
                {
                    accountYearCopyItem.AccountPeriodCopyItems.Add(new AccountPeriodCopyItem()
                    {
                        AccountPeriodId = period.AccountPeriodId,
                        PeriodNr = period.PeriodNr,
                        From = period.From,
                        To = period.To,
                        Status = (TermGroup_AccountStatus)period.Status
                    });
                }

                foreach (var voucherSerie in accountYear.VoucherSeries)
                {
                    accountYearCopyItem.VoucherSeriesCopyItems.Add(new VoucherSerieCopyItem()
                    {
                        VoucherSeriesTypeName = voucherSerie.VoucherSeriesType.Name,
                        VoucherNrLatest = voucherSerie.VoucherNrLatest,
                        VoucherDateLatest = voucherSerie.VoucherDateLatest,
                        Status = voucherSerie.Status,
                    });
                }
                accountYearCopyItems.Add(accountYearCopyItem);
            }

            return accountYearCopyItems;
        }

        public List<AccountYearCopyItem> GetAccountYearCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAccountYearCopyItems(actorCompanyId);

            return economyTemplateConnector.GetAccountYearCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyVoucherSeriesTypesFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();

            #region Prereq

            List<VoucherSeriesType> existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(templateCompanyDataItem.DestinationActorCompanyId, true);

            #endregion

            foreach (var voucherSeriesTypeCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.VoucherSeriesTypeCopyItems)
            {
                #region VoucherSeriesType

                try
                {
                    var voucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(i => i.Name == voucherSeriesTypeCopyItem.Name && i.VoucherSeriesTypeNr == voucherSeriesTypeCopyItem.VoucherSeriesTypeNr);

                    if (voucherSeriesType == null)
                    {
                        voucherSeriesType = new VoucherSeriesType()
                        {
                            Name = voucherSeriesTypeCopyItem.Name,
                            StartNr = voucherSeriesTypeCopyItem.StartNr,
                            VoucherSeriesTypeNr = voucherSeriesTypeCopyItem.VoucherSeriesTypeNr,
                            Template = voucherSeriesTypeCopyItem.Template,
                        };

                        var result = VoucherManager.AddVoucherSeriesType(voucherSeriesType, templateCompanyDataItem.DestinationActorCompanyId);
                        if (!result.Success)
                            result = companyTemplateManager.LogCopyError("VoucherSeriesType", "VoucherSeriesTypeId", voucherSeriesTypeCopyItem.VoucherSeriesTypeNr, voucherSeriesTypeCopyItem.VoucherSeriesTypeNr.ToString(), voucherSeriesTypeCopyItem.Name, templateCompanyDataItem, add: true);

                        templateResult.ActionResults.Add(result);
                    }

                    templateCompanyDataItem.TemplateCompanyEconomyDataItem.AddVoucherSeriesTypeMapping(voucherSeriesTypeCopyItem.VoucherSeriesTypeId, voucherSeriesType);
                }
                catch (Exception ex)
                {
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VoucherSeriesType", "VoucherSeriesTypeId", voucherSeriesTypeCopyItem.VoucherSeriesTypeNr, voucherSeriesTypeCopyItem.VoucherSeriesTypeNr.ToString(), voucherSeriesTypeCopyItem.Name, templateCompanyDataItem, ex: ex));
                }

                #endregion
            }

            return templateResult;
        }

        public List<VoucherSeriesTypeCopyItem> GetVoucherSeriesTypeCopyItems(int actorCompanyId)
        {
            List<VoucherSeriesTypeCopyItem> voucherSeriesTypeCopyItems = new List<VoucherSeriesTypeCopyItem>();
            var voucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(actorCompanyId, false);

            foreach (var voucherSeriesType in voucherSeriesTypes)
            {
                voucherSeriesTypeCopyItems.Add(new VoucherSeriesTypeCopyItem()
                {
                    StartNr = voucherSeriesType.StartNr,
                    VoucherSeriesTypeNr = voucherSeriesType.VoucherSeriesTypeNr,
                    Name = voucherSeriesType.Name,
                    Template = voucherSeriesType.Template,
                    VoucherSeriesTypeId = voucherSeriesType.VoucherSeriesTypeId,
                });
            }

            return voucherSeriesTypeCopyItems;
        }

        public List<VoucherSeriesTypeCopyItem> GetVoucherSeriesTypeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetVoucherSeriesTypeCopyItems(actorCompanyId);

            return economyTemplateConnector.GetVoucherSeriesTypeCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyResidualCodesFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();

            #region Prereq

            List<MatchCode> existingResidualCodes = InvoiceManager.GetMatchCodes(templateCompanyDataItem.DestinationActorCompanyId, null, false).ToList();

            #endregion

            foreach (var residualCodesCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.ResidualCodesCopyItems)
            {
                #region ResidualCode

                try
                {
                    var account = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(residualCodesCopyItem.AccountId);
                    var vatAccount = residualCodesCopyItem.VatAccountId.HasValue ? templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(residualCodesCopyItem.VatAccountId.Value) : null;

                    if (account != null)
                    {
                        MatchCode existingResidualCode = existingResidualCodes.FirstOrDefault(i => i.Name == residualCodesCopyItem.Name && i.Type == residualCodesCopyItem.Type);

                        if (existingResidualCode == null)
                        {
                            existingResidualCode = new MatchCode()
                            {
                                MatchCodeId = 0,
                                Name = residualCodesCopyItem.Name,
                                Description = residualCodesCopyItem.Description,
                                Type = residualCodesCopyItem.Type,
                                AccountId = account.AccountId,
                                AccountNr = account.AccountNr,
                                VatAccountId = vatAccount != null ? vatAccount.AccountId : (int?)null,
                                VatAccountNr = vatAccount?.AccountNr,
                                State = residualCodesCopyItem.State,
                                ActorCompanyId = templateCompanyDataItem.DestinationActorCompanyId
                            };

                            var result = InvoiceManager.AddMatchCode(existingResidualCode);
                            if (!result.Success)
                                result = companyTemplateManager.LogCopyError("MatchCode", "MatchCodeId", residualCodesCopyItem.MatchCodeId, residualCodesCopyItem.Name, residualCodesCopyItem.Name, templateCompanyDataItem, add: true);

                            templateResult.ActionResults.Add(result);
                        }
                        else
                        {
                            existingResidualCode.Name = residualCodesCopyItem.Name;
                            existingResidualCode.Description = residualCodesCopyItem.Description;
                            existingResidualCode.Type = residualCodesCopyItem.Type;
                            existingResidualCode.AccountId = account.AccountId;
                            existingResidualCode.AccountNr = account.AccountNr;
                            existingResidualCode.VatAccountId = vatAccount != null ? vatAccount.AccountId : (int?)null;
                            existingResidualCode.VatAccountNr = vatAccount?.AccountNr;
                            existingResidualCode.State = residualCodesCopyItem.State;
                            existingResidualCode.ActorCompanyId = templateCompanyDataItem.DestinationActorCompanyId;

                            var result = InvoiceManager.UpdateMatchCode(existingResidualCode);
                            if (!result.Success)
                                result = companyTemplateManager.LogCopyError("MatchCode", "MatchCodeId", residualCodesCopyItem.MatchCodeId, residualCodesCopyItem.Name, residualCodesCopyItem.Name, templateCompanyDataItem, add: false, update: true);

                            templateResult.ActionResults.Add(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("MatchCode", "MatchCodeId", residualCodesCopyItem.MatchCodeId, residualCodesCopyItem.Name, residualCodesCopyItem.Name, templateCompanyDataItem, ex: ex));
                }

                #endregion
            }

            return templateResult;
        }

        public List<ResidualCodeCopyItem> GetResidualCodeCopyItems(int actorCompanyId)
        {
            List<ResidualCodeCopyItem> residualCodeCopyItems = new List<ResidualCodeCopyItem>();
            var residualCodes = InvoiceManager.GetMatchCodes(actorCompanyId, null, false);

            foreach (var residualCode in residualCodes)
            {
                residualCodeCopyItems.Add(new ResidualCodeCopyItem()
                {
                    MatchCodeId = residualCode.MatchCodeId,
                    ActorCompanyId = residualCode.ActorCompanyId,
                    Name = residualCode.Name,
                    Description = residualCode.Description,
                    Type = residualCode.Type,
                    AccountId = residualCode.AccountId,
                    AccountNr = residualCode.AccountNr,
                    VatAccountId = residualCode.VatAccountId,
                    VatAccountNr = residualCode.VatAccountNr,
                    State = residualCode.State,
                });
            }

            return residualCodeCopyItems;
        }

        public List<ResidualCodeCopyItem> GetResidualCodesCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetResidualCodesCopyItems(actorCompanyId);

            return economyTemplateConnector.GetResidualCodesCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<ResidualCodeCopyItem> GetResidualCodesCopyItems(int actorCompanyId)
        {
            List<ResidualCodeCopyItem> residualCodesCopyItems = new List<ResidualCodeCopyItem>();
            var residualCodes = InvoiceManager.GetMatchCodes(actorCompanyId, null, false);

            foreach (var residualCode in residualCodes)
            {
                residualCodesCopyItems.Add(new ResidualCodeCopyItem()
                {
                    MatchCodeId = residualCode.MatchCodeId,
                    ActorCompanyId = residualCode.ActorCompanyId,
                    Name = residualCode.Name,
                    Description = residualCode.Description,
                    Type = residualCode.Type,
                    AccountId = residualCode.AccountId,
                    AccountNr = residualCode.AccountNr,
                    VatAccountId = residualCode.VatAccountId,
                    VatAccountNr = residualCode.VatAccountNr,
                    State = residualCode.State
                });
            }

            return residualCodesCopyItems;
        }

        public SupplierCopyItem GetSupplierCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetSupplierCopyItems(actorCompanyId);

            return economyTemplateConnector.GetCopyItems<SupplierCopyItem>(sysCompDbId, actorCompanyId, "Internal/Template/Economy/SupplierCopyItems", new Dictionary<string, string>() { { "actorCompanyId", actorCompanyId.ToString() } }).FirstOrDefault();
        }

        public ActionResult CopySuppliersFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            ActionResult result = new ActionResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    #region Prereq

                    var newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    if(newCompany == null) 
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                    List<Supplier> newSuppliers = SupplierManager.GetSuppliers(entities, newCompany.ActorCompanyId, true, true, true, true, true, false);
                    List<Currency> newCurrencies = CountryCurrencyManager.GetCurrencies(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    List<PaymentCondition> newPaymentConditions = PaymentManager.GetPaymentConditions(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    List<VatCode> newVatCodes = AccountManager.GetVatCodes(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    List<DeliveryCondition> newDeliveryConditions = InvoiceManager.GetDeliveryConditions(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    List<DeliveryType> newDeliveryTypes = InvoiceManager.GetDeliveryTypes(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    List<AttestWorkFlowGroup> newAttestWorkFlowGroups = AttestManager.GetAttestWorkFlowGroupsSimple(entities, templateCompanyDataItem.DestinationActorCompanyId);
                    List<CommodityCodeDTO> newIntrastatCodes = CommodityCodeManager.GetCustomerCommodityCodes(entities, templateCompanyDataItem.DestinationActorCompanyId, true, true);

                    var newAccountStds = AccountManager.GetAccountStdsByCompany(entities, templateCompanyDataItem.DestinationActorCompanyId, true);
                    var newAccountInternals = AccountManager.GetAccountInternals(entities, templateCompanyDataItem.DestinationActorCompanyId, true, true);

                    List<AccountDim> accountDimsNew = AccountManager.GetAccountDimsByCompany(entities, templateCompanyDataItem.DestinationActorCompanyId, loadAccounts: true);
                    if (accountDimsNew.IsNullOrEmpty())
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                    // Get full account dim mapping
                    //List<Tuple<int, int, Dictionary<int, int>>> mappings = GetAccountDimMappingsWithAccounts(accountDimsTemplate, accountDimsNew);

                    #endregion

                    #region Perform

                    foreach (var templateSupplier in templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateSuppliers)
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            var newSupplier = newSuppliers.FirstOrDefault(s => s.Name == templateSupplier.Name && s.OrgNr == templateSupplier.OrgNr);
                            if (newSupplier == null)
                            {
                                newSupplier = new Supplier
                                {
                                    VatType = (int)templateSupplier.VatType,
                                    CurrencyId = templateSupplier.CurrencyId,
                                    SysCountryId = templateSupplier.SysCountryId,
                                    SysLanguageId = templateSupplier.SysLanguageId,
                                    SupplierNr = templateSupplier.SupplierNr.Trim(),
                                    Name = templateSupplier.Name.Trim(),
                                    OrgNr = templateSupplier.OrgNr,
                                    VatNr = templateSupplier.VatNr,
                                    InvoiceReference = templateSupplier.InvoiceReference,
                                    OurReference = templateSupplier.OurReference,
                                    BIC = templateSupplier.BIC,
                                    OurCustomerNr = templateSupplier.OurCustomerNr,
                                    CopyInvoiceNrToOcr = templateSupplier.CopyInvoiceNrToOcr,
                                    Interim = templateSupplier.Interim,
                                    ManualAccounting = templateSupplier.ManualAccounting,
                                    BlockPayment = templateSupplier.BlockPayment,
                                    RiksbanksCode = templateSupplier.RiksbanksCode,
                                    State = (int)SoeEntityState.Active,
                                    IsEDISupplier = templateSupplier.IsEDISupplier,
                                    ShowNote = templateSupplier.ShowNote,
                                    Note = templateSupplier.Note,
                                    SysWholeSellerId = templateSupplier.SysWholeSellerId.ToNullable(),
                                    IsPrivatePerson = templateSupplier.IsPrivatePerson,
                                };
                                SetCreatedProperties(newSupplier);
                                entities.Supplier.AddObject(newSupplier);

                                #region Actor Add

                                var actor = new Actor()
                                {
                                    ActorType = (int)SoeActorType.Supplier,

                                    //Set references
                                    Supplier = newSupplier,

                                };
                                SetCreatedProperties(newSupplier);
                                entities.Actor.AddObject(actor);

                                //supplierId = newSupplier.ActorSupplierId;

                                #endregion
                            }
                            else
                            {
                                if (!templateCompanyDataItem.Update)
                                    continue;
                            }

                            #region Add references

                            newSupplier.Company = newCompany;

                            // Payment condition
                            if (templateSupplier.PaymentConditionId.HasValue)
                            {
                                var templatePaymentCondition = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templatePaymentConditions.FirstOrDefault(p => p.PaymentConditionId == templateSupplier.PaymentConditionId.Value);
                                if (templatePaymentCondition != null)
                                {
                                    var newPaymentCondition = newPaymentConditions.FirstOrDefault(p => p.Code == templatePaymentCondition.Code && p.Name == templatePaymentCondition.Name);
                                    if (newPaymentCondition != null)
                                    {
                                        newSupplier.PaymentCondition = newPaymentCondition;
                                    }
                                    else
                                    {
                                        newPaymentCondition = new PaymentCondition()
                                        {
                                            Code = templatePaymentCondition.Code,
                                            Name = templatePaymentCondition.Name,
                                            Days = templatePaymentCondition.Days,
                                            DiscountDays = templatePaymentCondition.DiscountDays,
                                            DiscountPercent = templatePaymentCondition.DiscountPercent,

                                            Company = newCompany,
                                        };

                                        SetCreatedProperties(newPaymentCondition);
                                        entities.PaymentCondition.AddObject(newPaymentCondition);

                                        newSupplier.PaymentCondition = newPaymentCondition;

                                        // Add to new
                                        newPaymentConditions.Add(newPaymentCondition);
                                    }
                                }
                            }

                            // Vat code
                            if (templateSupplier.VatCodeId.HasValue)
                            {
                                var templateVatCode = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateVatCodes.FirstOrDefault(p => p.VatCodeId == templateSupplier.VatCodeId.Value);
                                if (templateVatCode != null)
                                {
                                    var newVatCode = newVatCodes.FirstOrDefault(p => p.Code == templateVatCode.Code && p.Name == templateVatCode.Name);
                                    if (newVatCode != null)
                                    {
                                        newSupplier.VatCode = newVatCode;
                                    }
                                    else
                                    {
                                        int? purchaseVatAccountId = null;
                                        if (templateVatCode.PurchaseVATAccountId.HasValue)
                                            purchaseVatAccountId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount((int)templateVatCode.PurchaseVATAccountId).AccountId;

                                        var vatCode = new VatCode()
                                        {
                                            AccountId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateVatCode.AccountId).AccountId,
                                            Code = templateVatCode.Code,
                                            Name = templateVatCode.Name,
                                            Percent = templateVatCode.Percent,
                                            PurchaseVATAccountId = purchaseVatAccountId,

                                            Company = newCompany,
                                        };

                                        SetCreatedProperties(vatCode);
                                        entities.VatCode.AddObject(vatCode);

                                        newSupplier.VatCode = newVatCode;

                                        // Add to new
                                        newVatCodes.Add(vatCode);
                                    }
                                }
                            }

                            // Delivery condition
                            if (templateSupplier.DeliveryConditionId.HasValue)
                            {
                                var templateDeliveryCondition = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateDeliveryConditions.FirstOrDefault(p => p.DeliveryConditionId == templateSupplier.DeliveryConditionId.Value);
                                if (templateDeliveryCondition != null)
                                {
                                    var newDeliveryCondition = newDeliveryConditions.FirstOrDefault(p => p.Code == templateDeliveryCondition.Code && p.Name == templateDeliveryCondition.Name);
                                    if (newDeliveryCondition != null)
                                    {
                                        newSupplier.DeliveryCondition = newDeliveryCondition;
                                    }
                                    else
                                    {
                                        newDeliveryCondition = new DeliveryCondition()
                                        {
                                            Code = templateDeliveryCondition.Code,
                                            Name = templateDeliveryCondition.Name,

                                            Company = newCompany,
                                        };

                                        SetCreatedProperties(newDeliveryCondition);
                                        entities.DeliveryCondition.AddObject(newDeliveryCondition);

                                        newSupplier.DeliveryCondition = newDeliveryCondition;

                                        // Add to new
                                        newDeliveryConditions.Add(newDeliveryCondition);
                                    }

                                    templateCompanyDataItem.TemplateCompanyBillingDataItem.AddDeliveryConditionMapping(templateDeliveryCondition.DeliveryConditionId, newDeliveryCondition);
                                }
                            }

                            // Delivery type
                            if (templateSupplier.DeliveryTypeId.HasValue)
                            {
                                var templateDeliveryType = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateDeliveryTypes.FirstOrDefault(p => p.DeliveryTypeId == templateSupplier.DeliveryTypeId.Value);
                                if (templateDeliveryType != null)
                                {
                                    var newDeliveryType = newDeliveryTypes.FirstOrDefault(p => p.Code == templateDeliveryType.Code && p.Name == templateDeliveryType.Name);
                                    if (newDeliveryType != null)
                                    {
                                        newSupplier.DeliveryType = newDeliveryType;

                                    }
                                    else
                                    {
                                        newDeliveryType = new DeliveryType()
                                        {
                                            Code = templateDeliveryType.Code,
                                            Name = templateDeliveryType.Name,

                                            Company = newCompany,
                                        };

                                        SetCreatedProperties(newDeliveryType);
                                        entities.DeliveryType.AddObject(newDeliveryType);

                                        newSupplier.DeliveryType = newDeliveryType;

                                        // Add to new
                                        newDeliveryTypes.Add(newDeliveryType);
                                    }
                                    templateCompanyDataItem.TemplateCompanyBillingDataItem.AddDeliveryTypeMapping(templateDeliveryType.DeliveryTypeId, newDeliveryType);
                                }
                            }

                            // Attest work flow group - only set if copied before hand
                            if (templateSupplier.AttestWorkFlowGroupId.HasValue)
                            {
                                var templateAttestWorkFlowGroup = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateAttestWorkFlowGroups.FirstOrDefault(p => p.AttestWorkFlowHeadId == templateSupplier.AttestWorkFlowGroupId.Value);
                                if (templateAttestWorkFlowGroup != null)
                                {
                                    var newAttestWorkFlowGroup = newAttestWorkFlowGroups.FirstOrDefault(p => p.AttestGroupCode == templateAttestWorkFlowGroup.AttestGroupCode && p.AttestGroupName == templateAttestWorkFlowGroup.AttestGroupName);
                                    if (newAttestWorkFlowGroup != null)
                                    {
                                        newSupplier.AttestWorkFlowGroup = newAttestWorkFlowGroup;
                                    }
                                }
                            }

                            // Intrastat code - only set if copied before hand
                            if (templateSupplier.IntrastatCodeId.HasValue)
                            {
                                var templateIntrastatCode = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateIntrastatCodes.FirstOrDefault(p => p.IntrastatCodeId == templateSupplier.IntrastatCodeId.Value);
                                if (templateIntrastatCode != null)
                                {
                                    var newIntrastatCode = newIntrastatCodes.FirstOrDefault(p => p.SysIntrastatCodeId == templateIntrastatCode.SysIntrastatCodeId);
                                    if (newIntrastatCode != null)
                                    {
                                        newSupplier.IntrastatCodeId = newIntrastatCode.IntrastatCodeId;
                                    }
                                }
                            }

                            #endregion

                            result = SaveChanges(entities, transaction);
                            if (!result.Success)
                                return result;

                            #region Addresses

                            // Template contact
                            var templateContact = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateContacts.FirstOrDefault(c => c.ActorId == templateSupplier.ActorSupplierId);

                            // Get contact
                            Contact newContact = ContactManager.GetContactFromActor(entities, newSupplier.ActorSupplierId, loadAllContactInfo: true);
                            if (newContact == null)
                            {
                                // Create new Contact
                                newContact = new Contact()
                                {
                                    Actor = newSupplier.Actor,
                                    SysContactTypeId = (int)TermGroup_SysContactType.Company,
                                };
                                SetCreatedProperties(newContact);
                                entities.Contact.AddObject(newContact);
                            }

                            foreach (var templateContactAddress in templateContact.ContactAddresses)
                            {
                                var newContactAddress = newContact.ContactAddress.FirstOrDefault(c => c.SysContactAddressTypeId == (int)templateContactAddress.SysContactAddressTypeId && c.Name == templateContactAddress.Name);
                                if (newContactAddress == null)
                                {
                                    newContactAddress = new ContactAddress()
                                    {
                                        SysContactAddressTypeId = (int)templateContactAddress.SysContactAddressTypeId,
                                        Name = templateContactAddress.Name,
                                        IsSecret = templateContactAddress.IsSecret,
                                    };

                                    foreach (var templateAddressRow in templateContactAddress.ContactAddressRows)
                                    {
                                        var newContactAddressRow = new ContactAddressRow()
                                        {
                                            SysContactAddressRowTypeId = (int)templateAddressRow.SysContactAddressRowTypeId,
                                            Text = templateAddressRow.Text,
                                        };

                                        SetCreatedProperties(newContactAddressRow);
                                        newContactAddress.ContactAddressRow.Add(newContactAddressRow);
                                    }

                                    SetCreatedProperties(newContactAddress);
                                    newContact.ContactAddress.Add(newContactAddress);
                                }
                            }

                            foreach (var templateContactEcom in templateContact.ContactEComs)
                            {
                                var newContactECom = newContact.ContactECom.FirstOrDefault(c => c.SysContactEComTypeId == (int)templateContactEcom.SysContactEComTypeId && c.Name == templateContactEcom.Name);
                                if (newContactECom == null)
                                {
                                    newContactECom = new ContactECom()
                                    {
                                        SysContactEComTypeId = (int)templateContactEcom.SysContactEComTypeId,
                                        Name = templateContactEcom.Name,
                                        Text = templateContactEcom.Text,
                                        Description = templateContactEcom.Description,
                                        IsSecret = templateContactEcom.IsSecret,
                                    };

                                    SetCreatedProperties(newContactECom);
                                    newContact.ContactECom.Add(newContactECom);
                                }

                                if (templateSupplier.ContactEcomId.HasValue && templateContactEcom.ContactEComId == templateSupplier.ContactEcomId.Value)
                                    newSupplier.ContactEcomId = newContactECom.ContactEComId;
                            }

                            #endregion

                            #region Categories

                            List<CompanyCategoryRecordDTO> templateCategoryRecords = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateCategoryRecords.Where(c => c.RecordId == templateSupplier.ActorSupplierId).ToList();
                            List<CompanyCategoryRecord> newCategoryRecords = CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Supplier, SoeCategoryRecordEntity.Supplier, newSupplier.ActorSupplierId, newCompany.ActorCompanyId);
                            List<Category> newCategories = CategoryManager.GetCategories(entities, SoeCategoryType.Supplier, new List<int>(), newCompany.ActorCompanyId);

                            foreach (var templateCategoryRecord in templateCategoryRecords)
                            {
                                if (templateCategoryRecord.Category != null)
                                {
                                    var newCategory = newCategories.FirstOrDefault(c => c.Name == templateCategoryRecord.Category.Name);
                                    if (newCategory != null)
                                    {
                                        if (!newCategoryRecords.Any(c => c.CategoryId == newCategory.CategoryId))
                                        {
                                            CompanyCategoryRecord categoryRecord = new CompanyCategoryRecord()
                                            {
                                                ActorCompanyId = newCompany.ActorCompanyId,
                                                CategoryId = newCategory.CategoryId,
                                                RecordId = newSupplier.ActorSupplierId,
                                                Entity = (int)SoeCategoryRecordEntity.Supplier,
                                            };
                                            entities.CompanyCategoryRecord.AddObject(categoryRecord);
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region ContactPersons

                            List<ContactPersonDTO> templateContactPersons = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateContactPersons[templateSupplier.ActorSupplierId];
                            if (templateContactPersons != null)
                            {
                                List<ContactPerson> newContactPersons = ContactManager.GetContactPersons(entities, newSupplier.ActorSupplierId);
                                List<ContactPerson> allNewContactPersons = ContactManager.GetContactPersonsAll(entities, newCompany.ActorCompanyId);

                                List<int> idsToMapTo = new List<int>();
                                foreach (var contactPerson in templateContactPersons)
                                {
                                    if (!newContactPersons.Any(p => p.FirstName == contactPerson.FirstName && p.LastName == contactPerson.LastName))
                                    {
                                        var newContactPerson = allNewContactPersons.FirstOrDefault(p => p.FirstName == contactPerson.FirstName && p.LastName == contactPerson.LastName);
                                        if (newContactPerson != null)
                                        {
                                            idsToMapTo.Add(newContactPerson.ActorContactPersonId);
                                        }
                                    }
                                }

                                if (idsToMapTo.Count > 0)
                                {
                                    result = ContactManager.SaveContactPersonMappings(entities, idsToMapTo, newSupplier.ActorSupplierId);
                                    if (!result.Success)
                                        return result;
                                }
                            }

                            #endregion

                            #region Payment information

                            var templatePaymentInformation = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templatePaymentInformations.FirstOrDefault(p => p.ActorId == templateSupplier.ActorSupplierId);

                            if (templatePaymentInformation != null)
                            {
                                var newPaymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, newSupplier.ActorSupplierId, true, false);

                                if (newPaymentInformation == null)
                                {
                                    newPaymentInformation = new PaymentInformation()
                                    {
                                        DefaultSysPaymentTypeId = templatePaymentInformation.DefaultSysPaymentTypeId,

                                        //Set references
                                        Actor = newSupplier.Actor,
                                    };
                                    SetCreatedProperties(newPaymentInformation);
                                    entities.PaymentInformation.AddObject(newPaymentInformation);
                                }

                                foreach (var templateRow in templatePaymentInformation.Rows)
                                {
                                    var newPaymentInformationRow = newPaymentInformation.PaymentInformationRow.FirstOrDefault(r => r.SysPaymentTypeId == templateRow.SysPaymentTypeId && r.PaymentNr == templateRow.PaymentNr);
                                    if (newPaymentInformationRow == null)
                                    {
                                        int? currencyId = null;
                                        if (templateRow.CurrencyId.HasValue)
                                        {
                                            var templateCurrency = templateCompanyDataItem.TemplateCompanyEconomyDataItem.SupplierCopyItem.templateCurrencies.FirstOrDefault(c => c.CurrencyId == templateRow.CurrencyId);
                                            if (templateCurrency != null)
                                            {
                                                var newCurrency = newCurrencies.FirstOrDefault(c => c.SysCurrencyId == templateCurrency.SysCurrencyId);
                                                if (newCurrency != null)
                                                    currencyId = newCurrency.CurrencyId;
                                            }
                                        }

                                        newPaymentInformationRow = new PaymentInformationRow
                                        {
                                            SysPaymentTypeId = templateRow.SysPaymentTypeId,
                                            PaymentNr = templateRow.PaymentNr,
                                            Default = templateRow.Default,
                                            ShownInInvoice = templateRow.ShownInInvoice,
                                            // Foreign payments
                                            BIC = templateRow.BIC,
                                            ClearingCode = templateRow.ClearingCode,
                                            PaymentCode = templateRow.PaymentCode,
                                            PaymentMethodCode = templateRow.PaymentMethodCode,
                                            PaymentForm = templateRow.PaymentForm,
                                            ChargeCode = templateRow.ChargeCode,
                                            IntermediaryCode = templateRow.IntermediaryCode,
                                            CurrencyAccount = templateRow.CurrencyAccount,
                                            //Set references
                                            PaymentInformation = newPaymentInformation,
                                            BankConnected = templateRow.BankConnected,
                                            CurrencyId = currencyId,
                                        };

                                        SetCreatedProperties(newPaymentInformationRow);
                                        entities.PaymentInformationRow.AddObject(newPaymentInformationRow);
                                    }
                                }
                            }

                            #endregion

                            #region Account settings

                            if (!newSupplier.SupplierAccountStd.IsLoaded)
                                newSupplier.SupplierAccountStd.Load();

                            foreach (var templateAccountSetting in templateSupplier.AccountingSettings)
                            {
                                if (newSupplier.SupplierAccountStd == null)
                                    newSupplier.SupplierAccountStd = new System.Data.Entity.Core.Objects.DataClasses.EntityCollection<SupplierAccountStd>();

                                var newSupplierAccountStd = newSupplier.SupplierAccountStd.FirstOrDefault(a => a.Type == templateAccountSetting.Type);
                                if (newSupplierAccountStd == null)
                                {
                                    if (templateAccountSetting.Account1Id > 0)
                                    {
                                        var accountId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateAccountSetting.Account1Id).AccountId;
                                        if (accountId > 0)
                                        {
                                            var accountStd = newAccountStds.FirstOrDefault(a => a.AccountId == accountId);
                                            SupplierAccountStd supplierAccountStd = new SupplierAccountStd
                                            {
                                                Type = templateAccountSetting.Type,
                                                AccountStd = accountStd,

                                                Supplier = newSupplier,
                                            };

                                            if(templateAccountSetting.Account2Id > 0)
                                            {
                                                var accountInternal = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateAccountSetting.Account2Id);
                                                if(accountInternal != null)
                                                {
                                                    var internalAccount = newAccountInternals.FirstOrDefault(a => a.AccountId == accountInternal.AccountId);
                                                    if (internalAccount != null)
                                                        supplierAccountStd.AccountInternal.Add(internalAccount);
                                                }
                                            }

                                            if (templateAccountSetting.Account3Id > 0)
                                            {
                                                var accountInternal = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateAccountSetting.Account3Id);
                                                if (accountInternal != null)
                                                {
                                                    var internalAccount = newAccountInternals.FirstOrDefault(a => a.AccountId == accountInternal.AccountId);
                                                    if (internalAccount != null)
                                                        supplierAccountStd.AccountInternal.Add(internalAccount);
                                                }
                                            }

                                            if (templateAccountSetting.Account4Id > 0)
                                            {
                                                var accountInternal = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateAccountSetting.Account4Id);
                                                if (accountInternal != null)
                                                {
                                                    var internalAccount = newAccountInternals.FirstOrDefault(a => a.AccountId == accountInternal.AccountId);
                                                    if (internalAccount != null)
                                                        supplierAccountStd.AccountInternal.Add(internalAccount);
                                                }
                                            }

                                            if (templateAccountSetting.Account5Id > 0)
                                            {
                                                var accountInternal = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateAccountSetting.Account5Id);
                                                if (accountInternal != null)
                                                {
                                                    var internalAccount = newAccountInternals.FirstOrDefault(a => a.AccountId == accountInternal.AccountId);
                                                    if (internalAccount != null)
                                                        supplierAccountStd.AccountInternal.Add(internalAccount);
                                                }
                                            }

                                            if (templateAccountSetting.Account6Id > 0)
                                            {
                                                var accountInternal = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateAccountSetting.Account6Id);
                                                if (accountInternal != null)
                                                {
                                                    var internalAccount = newAccountInternals.FirstOrDefault(a => a.AccountId == accountInternal.AccountId);
                                                    if (internalAccount != null)
                                                        supplierAccountStd.AccountInternal.Add(internalAccount);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            result = SaveChanges(entities, transaction);

                            var deliveryConditions = entities.DeliveryCondition.Where(dc => dc.Company.ActorCompanyId == newCompany.ActorCompanyId).ToList();

                            #endregion

                            //Commit transaction
                            if (result.Success)
                                transaction.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    base.LogError(ex.ToString());
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        public SupplierCopyItem GetSupplierCopyItems(int actorCompanyId)
        {
            SupplierCopyItem supplierCopyItem = new SupplierCopyItem();

            using (CompEntities entities = new CompEntities())
            {
                entities.Connection.Open();

                supplierCopyItem.templateSuppliers = SupplierManager.GetSuppliers(entities, actorCompanyId, true, false, true, true, true, false)
                    .ToDTOs(true)
                    .ToList();
                var accountSettings = SupplierManager.GetSuppliersAccountSettings(entities, actorCompanyId, 
                    supplierIds: supplierCopyItem.templateSuppliers.Select(s => s.ActorSupplierId).ToList()
                    );
                foreach (var supplier in supplierCopyItem.templateSuppliers)
                {
                    supplier.AccountingSettings = accountSettings.GetValue(supplier.ActorSupplierId); 
                }

                supplierCopyItem.templateCurrencies = CountryCurrencyManager.GetCurrencies(entities, actorCompanyId).Select(i => new CompCurrencyDTO() { CurrencyId = i.CurrencyId, SysCurrencyId = i.SysCurrencyId, Code = i.Code, Name = i.Name }).ToList();

                supplierCopyItem.templatePaymentConditions = PaymentManager.GetPaymentConditions(entities, actorCompanyId).ToDTOs().ToList();

                supplierCopyItem.templateVatCodes = AccountManager.GetVatCodes(entities, actorCompanyId).ToDTOs().ToList();

                supplierCopyItem.templateDeliveryConditions = InvoiceManager.GetDeliveryConditions(entities, actorCompanyId).ToDTOs().ToList();

                supplierCopyItem.templateDeliveryTypes = InvoiceManager.GetDeliveryTypes(entities, actorCompanyId).ToDTOs().ToList();

                supplierCopyItem.templateAttestWorkFlowGroups = AttestManager.GetAttestWorkFlowGroupsSimple(entities, actorCompanyId).ToAttestGroupDTOs(true, true).ToList();

                supplierCopyItem.templateIntrastatCodes = CommodityCodeManager.GetCustomerCommodityCodes(entities, actorCompanyId, true, true);

                supplierCopyItem.templateAccountInternals = AccountManager.GetAccountInternals(entities, actorCompanyId, true, true).ToDTOs().ToList();

                supplierCopyItem.accountDimsTemplate = AccountManager.GetAccountDimsByCompany(entities, actorCompanyId, loadAccounts: true).ToDTOs().ToList();

                foreach (var supplier in supplierCopyItem.templateSuppliers)
                {
                    supplierCopyItem.templateContacts.Add(ContactManager.GetContactFromActor(entities, supplier.ActorSupplierId, true, loadAllContactInfo: true).ToDTO(true, true));
                    supplierCopyItem.templateCategoryRecords.AddRange(CategoryManager.GetCompanyCategoryRecords(entities, SoeCategoryType.Supplier, SoeCategoryRecordEntity.Supplier, supplier.ActorSupplierId, actorCompanyId).ToDTOs(true));
                    supplierCopyItem.templateContactPersons.Add(supplier.ActorSupplierId, ContactManager.GetContactPersons(entities, supplier.ActorSupplierId).ToDTOs().ToList());
                    supplierCopyItem.templatePaymentInformations.Add(PaymentManager.GetPaymentInformationFromActor(entities, supplier.ActorSupplierId, true, false).ToDTO(true, false, true));
                }
            }

            return supplierCopyItem;
        }

        public List<VoucherTemplatesCopyItem> GetVoucherTemplatesCopyItems(int actorCompanyId)
        {
            List<VoucherTemplatesCopyItem> voucherTemplatesCopyItems = new List<VoucherTemplatesCopyItem>();
            var voucherTemplates = VoucherManager.GetVoucherTemplates(actorCompanyId);

            foreach (var sourceVoucherTemplate in voucherTemplates)
            {
                var item = new VoucherTemplatesCopyItem()
                {
                    VoucherHeadId = sourceVoucherTemplate.VoucherHeadId,
                    AccountPeriodId = sourceVoucherTemplate.AccountPeriodId,
                    VoucherSeriesId = sourceVoucherTemplate.VoucherSeriesId,
                    VoucherNr = sourceVoucherTemplate.VoucherNr,
                    Date = sourceVoucherTemplate.Date,
                    Text = sourceVoucherTemplate.Text,
                    Status = sourceVoucherTemplate.Status,
                    TypeBalance = sourceVoucherTemplate.TypeBalance,
                    VatVoucher = sourceVoucherTemplate.VatVoucher,
                    Note = sourceVoucherTemplate.Note,
                    CompanyGroupVoucher = sourceVoucherTemplate.CompanyGroupVoucher,
                    SourceType = sourceVoucherTemplate.SourceType,
                    Template = sourceVoucherTemplate.Template,
                    ActorCompanyId = actorCompanyId,
                    VoucherSeriesTypeName = sourceVoucherTemplate.VoucherSeriesTypeName,
                    VoucherSeriesTypeNr = sourceVoucherTemplate.VoucherSeriesTypeNr,
                    VoucherSeriesAccountYearFrom = sourceVoucherTemplate.VoucherSeries.AccountYear.From,
                    VoucherSeriesAccountYearTo = sourceVoucherTemplate.VoucherSeries.AccountYear.To,
                    VoucherSeriesAccountYearStatus = sourceVoucherTemplate.VoucherSeries.AccountYear.Status,

                    AccountPeriodPeriodNr = sourceVoucherTemplate.AccountPeriod.PeriodNr,
                    AccountPeriodFrom = sourceVoucherTemplate.AccountPeriod.From,
                    AccountPeriodTo = sourceVoucherTemplate.AccountPeriod.To,
                };
                if (item.VoucherTemplateRows == null)
                    item.VoucherTemplateRows = new List<VoucherTemplateRowCopyItem>();
                foreach (var row in sourceVoucherTemplate.VoucherRow)
                {
                    var rowItem = new VoucherTemplateRowCopyItem();
                    rowItem.Text = row.Text;
                    rowItem.Amount = row.Amount;
                    rowItem.AccountId = row.AccountId;
                    rowItem.Quantity = row.Quantity;
                    rowItem.Date = row.Date;
                    rowItem.Merged = row.Merged;
                    rowItem.State = row.State;
                    rowItem.AccountDistributionHeadId = row.AccountDistributionHeadId;
                    rowItem.AmountEntCurrency = row.AmountEntCurrency;
                    rowItem.RowNr = row.RowNr;
                    rowItem.AccountStdAccountName = row.AccountStd.Account.Name;
                    rowItem.AccountStdAccountAccountNr = row.AccountStd.Account.AccountNr;
                    rowItem.AccountDistributionHeadType = row.AccountDistributionHead != null ? row.AccountDistributionHead.Type : (int?)null;
                    rowItem.AccountDistributionHeadName = row.AccountDistributionHead != null ? row.AccountDistributionHead.Name : null;
                    item.VoucherTemplateRows.Add(rowItem);
                }
                voucherTemplatesCopyItems.Add(item);
            }

            return voucherTemplatesCopyItems;
        }

        public List<VoucherTemplatesCopyItem> GetVoucherTemplatesCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetVoucherTemplatesCopyItems(actorCompanyId);

            return economyTemplateConnector.GetVoucherTemplatesCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyVoucherTemplatesFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                List<VoucherSeriesType> existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(templateCompanyDataItem.DestinationActorCompanyId, true);
                List<VoucherHead> existingVoucherTemplates = VoucherManager.GetVoucherTemplates(templateCompanyDataItem.DestinationActorCompanyId);
                List<VoucherSeries> existingVoucherSeries = VoucherManager.GetVoucherSeries(templateCompanyDataItem.DestinationActorCompanyId, false);
                List<AccountYear> existingAccountYears = AccountManager.GetAccountYears(entities, templateCompanyDataItem.DestinationActorCompanyId, false, true);
                List<AccountPeriod> existingAccountPeriods = AccountManager.GetAccountPeriods(templateCompanyDataItem.DestinationActorCompanyId);

                List<AccountDistributionHead> existingAccountDistributionHeads = AccountDistributionManager.GetAccountDistributionHeads(templateCompanyDataItem.DestinationActorCompanyId);
                List<Account> existingAccounts = AccountManager.GetAccounts(templateCompanyDataItem.DestinationActorCompanyId);

                #endregion

                foreach (var voucherTemplateCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.VoucherTemplatesCopyItems)
                {
                    #region VoucherTemplates

                    try
                    {
                        VoucherSeriesType existingVoucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(i => i.Name == voucherTemplateCopyItem.VoucherSeriesTypeName && i.VoucherSeriesTypeNr == voucherTemplateCopyItem.VoucherSeriesTypeNr);
                        AccountYear existingAccountYear = existingAccountYears.FirstOrDefault(a => a.From == voucherTemplateCopyItem.VoucherSeriesAccountYearFrom && a.To == voucherTemplateCopyItem.VoucherSeriesAccountYearTo && a.Status == voucherTemplateCopyItem.VoucherSeriesAccountYearStatus);

                        if (existingVoucherSeriesType != null && existingAccountYear != null)
                        {
                            var accountPeriod = existingAccountYear.AccountPeriod.FirstOrDefault(x => x.PeriodNr == voucherTemplateCopyItem.AccountPeriodPeriodNr && x.From == voucherTemplateCopyItem.AccountPeriodFrom && x.To == voucherTemplateCopyItem.AccountPeriodTo);
                            if (accountPeriod != null)
                            {
                                VoucherSeries selectedVoucherSerie = existingVoucherSeries.OrderByDescending(o => o.VoucherSeriesId).FirstOrDefault(x => x.AccountYearId == existingAccountYear.AccountYearId && x.VoucherSeriesTypeId == existingVoucherSeriesType.VoucherSeriesTypeId);

                                if (selectedVoucherSerie == null)
                                {
                                    selectedVoucherSerie = new VoucherSeries();
                                    selectedVoucherSerie.VoucherSeriesTypeId = existingVoucherSeriesType.VoucherSeriesTypeId;
                                    selectedVoucherSerie.AccountYearId = existingAccountYear.AccountYearId;
                                    selectedVoucherSerie.VoucherNrLatest = 0;
                                    var result = VoucherManager.AddVoucherSeries(entities, selectedVoucherSerie, templateCompanyDataItem.DestinationActorCompanyId, existingAccountYear.AccountYearId, existingVoucherSeriesType.VoucherSeriesTypeId);

                                    if (!result.Success)
                                        templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VoucherSeries", templateCompanyDataItem, saved: false));
                                    templateResult.ActionResults.Add(result);
                                }
                                if (selectedVoucherSerie.VoucherSeriesId > 0)
                                {
                                    VoucherHead voucherTemplate = existingVoucherTemplates.FirstOrDefault(i => i.VoucherNr == voucherTemplateCopyItem.VoucherNr && i.VoucherSeriesTypeNr == voucherTemplateCopyItem.VoucherSeriesTypeNr && i.AccountPeriod.PeriodNr == voucherTemplateCopyItem.AccountPeriodPeriodNr && i.VoucherSeriesTypeName == voucherTemplateCopyItem.VoucherSeriesTypeName && i.Text == voucherTemplateCopyItem.Text);

                                    if (voucherTemplate == null)
                                    {
                                        voucherTemplate = new VoucherHead()
                                        {
                                            VoucherHeadId = 0,
                                            AccountPeriodId = accountPeriod.AccountPeriodId,
                                            VoucherSeriesId = selectedVoucherSerie.VoucherSeriesId,
                                            VoucherNr = voucherTemplateCopyItem.VoucherNr,
                                            Date = voucherTemplateCopyItem.Date,
                                            Text = voucherTemplateCopyItem.Text,
                                            Status = voucherTemplateCopyItem.Status,
                                            TypeBalance = voucherTemplateCopyItem.TypeBalance,
                                            VatVoucher = voucherTemplateCopyItem.VatVoucher,
                                            Note = voucherTemplateCopyItem.Note,
                                            CompanyGroupVoucher = voucherTemplateCopyItem.CompanyGroupVoucher,
                                            SourceType = voucherTemplateCopyItem.SourceType,
                                            Template = true,
                                            ActorCompanyId = templateCompanyDataItem.DestinationActorCompanyId,
                                        };
                                        var resultAddVoucher = VoucherManager.AddVoucherTemplate(voucherTemplate, templateCompanyDataItem.DestinationActorCompanyId);
                                        if (!resultAddVoucher.Success)
                                            templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VoucherHead", templateCompanyDataItem, saved: false));
                                        templateResult.ActionResults.Add(resultAddVoucher);
                                    }

                                    foreach (VoucherTemplateRowCopyItem templateVoucherRow in voucherTemplateCopyItem.VoucherTemplateRows)
                                    {
                                        var existingAccount = existingAccounts.FirstOrDefault(x => x.Name == templateVoucherRow.AccountStdAccountName && x.AccountNr == templateVoucherRow.AccountStdAccountAccountNr);
                                        var existingAccountDistributionHead = existingAccountDistributionHeads.FirstOrDefault(x => x.VoucherSeriesTypeId == selectedVoucherSerie.VoucherSeriesId && x.Type == templateVoucherRow.AccountDistributionHeadType && x.Name == templateVoucherRow.AccountDistributionHeadName);

                                        if (existingAccount != null)
                                        {
                                            VoucherRow row = voucherTemplate.VoucherRow.FirstOrDefault(x => x.RowNr == templateVoucherRow.RowNr && x.Text == templateVoucherRow.Text && x.AccountNr == templateVoucherRow.AccountStdAccountAccountNr);
                                            if (row == null)
                                            {
                                                row = new VoucherRow();
                                                row.Text = templateVoucherRow.Text;
                                                row.Amount = templateVoucherRow.Amount;
                                                row.AccountId = existingAccount.AccountId;
                                                row.Quantity = templateVoucherRow.Quantity;
                                                row.Date = templateVoucherRow.Date;
                                                row.Merged = templateVoucherRow.Merged;
                                                row.State = templateVoucherRow.State;
                                                row.AccountDistributionHeadId = existingAccountDistributionHead != null ? existingAccountDistributionHead.AccountDistributionHeadId : (int?)null;
                                                row.AmountEntCurrency = templateVoucherRow.AmountEntCurrency;
                                                row.RowNr = templateVoucherRow.RowNr;
                                                row.VoucherHeadId = voucherTemplate.VoucherHeadId;
                                                var resultVoucherRow = VoucherManager.AddVoucherTemplateRow(row, templateCompanyDataItem.DestinationActorCompanyId);
                                                if (!resultVoucherRow.Success)
                                                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VoucherHeadRow", templateCompanyDataItem, saved: false));
                                                templateResult.ActionResults.Add(resultVoucherRow);
                                            }
                                            else
                                            {
                                                row.Text = templateVoucherRow.Text;
                                                row.Amount = templateVoucherRow.Amount;
                                                row.AccountId = existingAccount.AccountId;
                                                row.Quantity = templateVoucherRow.Quantity;
                                                row.Date = templateVoucherRow.Date;
                                                row.Merged = templateVoucherRow.Merged;
                                                row.State = templateVoucherRow.State;
                                                row.AccountDistributionHeadId = existingAccountDistributionHead != null ? existingAccountDistributionHead.AccountDistributionHeadId : (int?)null;
                                                row.AmountEntCurrency = templateVoucherRow.AmountEntCurrency;
                                                row.RowNr = templateVoucherRow.RowNr;
                                                var resultVoucherRow = VoucherManager.UpdateVoucherTemplateRow(row);
                                                if (!resultVoucherRow.Success)
                                                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VoucherHeadRow", templateCompanyDataItem, saved: false));
                                                templateResult.ActionResults.Add(resultVoucherRow);
                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VoucherTemplates", templateCompanyDataItem, ex, saved: false));
                    }

                    #endregion
                }
            }
            return templateResult;
        }

        public TemplateResult CopyPaymentMethodsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);
                if (newCompany == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }

                foreach (var paymentMethodCopyItem in templateCompanyDataItem.TemplateCompanyEconomyDataItem.PaymentMethodCopyItems)
                {
                    try
                    {
                        List<PaymentMethod> existingPaymentMethods = PaymentManager.GetPaymentMethods(entities, paymentMethodCopyItem.IsCustomerPayment ? SoeOriginType.CustomerPayment : SoeOriginType.SupplierPayment, templateCompanyDataItem.DestinationActorCompanyId).ToList();

                        PaymentMethod existingPaymentMethod = existingPaymentMethods.FirstOrDefault(p => p.SysPaymentMethodId == paymentMethodCopyItem.SysPaymentMethodId && p.PaymentType == paymentMethodCopyItem.PaymentType && p.Name == paymentMethodCopyItem.Name);

                        if (existingPaymentMethod == null)
                        {
                            existingPaymentMethod = new PaymentMethod()
                            {
                                Company = newCompany,
                                SysPaymentMethodId = paymentMethodCopyItem.SysPaymentMethodId,
                                PaymentType = paymentMethodCopyItem.PaymentType,
                                Name = paymentMethodCopyItem.Name,
                                CustomerNr = paymentMethodCopyItem.CustomerNr,
                                State = (int)SoeEntityState.Active,
                            };

                            var account = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(paymentMethodCopyItem.AccountId);
                            var result = PaymentManager.AddPaymentMethod(existingPaymentMethod, paymentMethodCopyItem.PaymentInformationRowId, account.AccountNr, templateCompanyDataItem.DestinationActorCompanyId, paymentMethodCopyItem.SoeOriginType, entities);

                            if (!result.Success)
                                templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("PaymentMethod", templateCompanyDataItem, saved: false));
                            templateResult.ActionResults.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("PaymentMethod", templateCompanyDataItem, ex, saved: false));
                    }
                }
            }
            return templateResult;
        }

        public List<PaymentMethodCopyItem> GetPaymentMethodCopyItems(int actorCompanyId)
        {
            List<PaymentMethodCopyItem> paymentMethodCopyItems = new List<PaymentMethodCopyItem>();
            var paymentMethods = PaymentManager.GetPaymentMethods(actorCompanyId);

            foreach (var paymentMethod in paymentMethods)
            {
                if (!paymentMethod.AccountStdReference.IsLoaded)
                    paymentMethod.AccountStdReference.Load();

                PaymentMethodCopyItem item = new PaymentMethodCopyItem()
                {
                    SysPaymentMethodId = paymentMethod.SysPaymentMethodId,
                    PaymentType = paymentMethod.PaymentType,
                    Name = paymentMethod.Name,
                    CustomerNr = paymentMethod.CustomerNr,
                    PaymentInformationRowId = paymentMethod.PaymentInformationRow?.PaymentInformationRowId ?? 0,
                    AccountId = paymentMethod.AccountStd.AccountId,
                    IsCustomerPayment = paymentMethod.PaymentType == (int)SoeOriginType.CustomerPayment,
                    SoeOriginType = (SoeOriginType)paymentMethod.PaymentType
                };

                paymentMethodCopyItems.Add(item);
            }

            return paymentMethodCopyItems;
        }

        public List<PaymentMethodCopyItem> GetPaymentMethodCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPaymentMethodCopyItems(actorCompanyId);

            return economyTemplateConnector.GetPaymentMethodCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyGrossProfitCodesFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }
                try
                {
                    foreach (var grossProfitCodeCopyItem in item.TemplateCompanyEconomyDataItem.GrossProfitCodeCopyItems)
                    {

                        List<GrossProfitCode> existingGrossProfitCodes = GrossProfitManager.GetGrossProfitCodes(entities, item.DestinationActorCompanyId).ToList();

                        GrossProfitCode existingGrossProfitCode = existingGrossProfitCodes.FirstOrDefault(gp => gp.Code == grossProfitCodeCopyItem.Code && gp.Name == grossProfitCodeCopyItem.Name);

                        if (existingGrossProfitCode == null)
                        {
                            var dim = item.TemplateCompanyEconomyDataItem.GetAccountDim(grossProfitCodeCopyItem.AccountDimId);
                            var account = item.TemplateCompanyEconomyDataItem.GetAccount(grossProfitCodeCopyItem.AccountId);
                            var year = item.TemplateCompanyEconomyDataItem.GetAccountYear(grossProfitCodeCopyItem.AccountYearId);

                            if (dim != null && account != null && year != null)
                            {
                                existingGrossProfitCode = new GrossProfitCode()
                                {
                                    Company = newCompany,
                                    Code = grossProfitCodeCopyItem.Code,
                                    Name = grossProfitCodeCopyItem.Name,
                                    AccountDimId = dim.AccountDimId,
                                    AccountId = account.AccountId,
                                    Description = grossProfitCodeCopyItem.Description,
                                    OpeningBalance = grossProfitCodeCopyItem.OpeningBalance,
                                    Period1 = grossProfitCodeCopyItem.Period1,
                                    Period2 = grossProfitCodeCopyItem.Period2,
                                    Period3 = grossProfitCodeCopyItem.Period3,
                                    Period4 = grossProfitCodeCopyItem.Period4,
                                    Period5 = grossProfitCodeCopyItem.Period5,
                                    Period6 = grossProfitCodeCopyItem.Period6,
                                    Period7 = grossProfitCodeCopyItem.Period7,
                                    Period8 = grossProfitCodeCopyItem.Period8,
                                    Period9 = grossProfitCodeCopyItem.Period9,
                                    Period10 = grossProfitCodeCopyItem.Period10,
                                    Period11 = grossProfitCodeCopyItem.Period11,
                                    Period12 = grossProfitCodeCopyItem.Period12,
                                    ActorCompanyId = item.DestinationActorCompanyId,
                                    AccountYearId = year.AccountYearId,
                                };
                                entities.GrossProfitCode.AddObject(existingGrossProfitCode);
                                SetCreatedProperties(existingGrossProfitCode);
                            }
                        }
                    }

                    var result = SaveChanges(entities);
                    if (!result.Success)
                        templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("GrossProfitCode", item, saved: false));
                    templateResult.ActionResults.Add(result);
                }

                catch (Exception ex)
                {
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("GrossProfitCode", item, ex, saved: false));
                }
            }
            return templateResult;
        }


        public List<GrossProfitCodeCopyItem> GetGrossProfitCodeCopyItems(int actorCompanyId)
        {
            List<GrossProfitCodeCopyItem> grossProfitCodeCopyItems = new List<GrossProfitCodeCopyItem>();
            var grossProfitCodes = GrossProfitManager.GetGrossProfitCodes(actorCompanyId);

            foreach (var grossProfitCode in grossProfitCodes)
            {
                GrossProfitCodeCopyItem item = new GrossProfitCodeCopyItem()
                {
                    GrossProfitCodeId = grossProfitCode.GrossProfitCodeId,
                    Code = grossProfitCode.Code,
                    Name = grossProfitCode.Name,
                    AccountDimId = grossProfitCode.AccountDimId ?? 0,
                    AccountId = grossProfitCode.AccountId ?? 0,
                    Description = grossProfitCode.Description,
                    OpeningBalance = grossProfitCode.OpeningBalance,
                    Period1 = grossProfitCode.Period1,
                    Period2 = grossProfitCode.Period2,
                    Period3 = grossProfitCode.Period3,
                    Period4 = grossProfitCode.Period4,
                    Period5 = grossProfitCode.Period5,
                    Period6 = grossProfitCode.Period6,
                    Period7 = grossProfitCode.Period7,
                    Period8 = grossProfitCode.Period8,
                    Period9 = grossProfitCode.Period9,
                    Period10 = grossProfitCode.Period10,
                    Period11 = grossProfitCode.Period11,
                    Period12 = grossProfitCode.Period12,
                    AccountYearId = grossProfitCode.AccountYearId
                };

                grossProfitCodeCopyItems.Add(item);
            }

            return grossProfitCodeCopyItems;
        }

        public List<GrossProfitCodeCopyItem> GetGrossProfitCodeCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetGrossProfitCodeCopyItems(actorCompanyId);

            return economyTemplateConnector.GetGrossProfitCodeCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyInventoryFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }

                List<InventoryWriteOffMethod> existingWriteOffMethods = InventoryManager.GetInventoryWriteOffMethods(entities, item.DestinationActorCompanyId).ToList();
                List<InventoryWriteOffTemplate> existingWriteOffTemplates = InventoryManager.GetInventoryWriteOffTemplates(entities, item.DestinationActorCompanyId).ToList();
                List<VoucherSeriesType> existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(entities, item.DestinationActorCompanyId, false).ToList();

                try
                {

                    // Copy write off methods
                    foreach (var method in item.TemplateCompanyEconomyDataItem.InventoryWriteOffMethodCopyItems)
                    {
                        var existingMethod = existingWriteOffMethods.FirstOrDefault(m => m.Name == method.Name && m.Type == method.Type);
                        if (existingMethod == null)
                        {
                            existingMethod = new InventoryWriteOffMethod()
                            {
                                ActorCompanyId = item.DestinationActorCompanyId,
                                Name = method.Name,
                                Description = method.Description,
                                Type = method.Type,
                                PeriodType = method.PeriodType,
                                PeriodValue = method.PeriodValue,
                                YearPercent = method.YearPercent
                            };

                            SetCreatedProperties(existingMethod);
                            entities.InventoryWriteOffMethod.AddObject(existingMethod);
                            existingWriteOffMethods.Add(existingMethod);
                        }
                    }

                    var result1 = SaveChanges(entities);
                    if (!result1.Success)
                    {
                        templateResult.ActionResults.Add(result1);
                        return templateResult;
                    }

                    // Copy write off templates
                    foreach (var templateCopyItem in item.TemplateCompanyEconomyDataItem.InventoryWriteOffTemplateCopyItems)
                    {
                        var existingTemplate = existingWriteOffTemplates.FirstOrDefault(t => t.Name == templateCopyItem.Name);
                        if (existingTemplate == null)
                        {
                            var templateMethod = templateCopyItem.WriteOffMethod;
                            var templateSerie = templateCopyItem.VoucherSeriesType;

                            if (templateSerie == null || templateMethod == null)
                                continue;

                            var newMethod = existingWriteOffMethods.FirstOrDefault(m => m.Name == templateMethod.Name && m.Type == templateMethod.Type);
                            var newSerie = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == templateSerie.Name);

                            if (newMethod == null || newSerie == null)
                                continue;

                            existingTemplate = new InventoryWriteOffTemplate()
                            {
                                Name = templateCopyItem.Name,
                                Description = templateCopyItem.Description,
                                ActorCompanyId = item.DestinationActorCompanyId,
                                InventoryWriteOffMethodId = newMethod.InventoryWriteOffMethodId,
                                VoucherSeriesTypeId = newSerie.VoucherSeriesTypeId
                            };
                            entities.InventoryWriteOffTemplate.AddObject(existingTemplate);

                            // Copy InventoryAccountStds
                            foreach (var inventoryAccountStd in templateCopyItem.InventoryAccountStds)
                            {
                                var existingInventoryAccountStd = existingTemplate.InventoryAccountStd.FirstOrDefault(ias => ias.Type == inventoryAccountStd.Type);
                                if (existingInventoryAccountStd == null)
                                {
                                    
                                    if (inventoryAccountStd.AccountId.HasValue)
                                    {
                                        Account account = item
                                            .TemplateCompanyEconomyDataItem
                                            .GetAccount(inventoryAccountStd.AccountId.Value);

                                        if (account == null)
                                        {
                                            continue;
                                        }

                                    }

                                    existingInventoryAccountStd = new InventoryAccountStd()
                                    {
                                        Type = inventoryAccountStd.Type,
                                        AccountId = inventoryAccountStd.AccountId,

                                    };
                                    existingTemplate.InventoryAccountStd.Add(existingInventoryAccountStd);

                                    // Copy InternalAccounts
                                    foreach (var internalAccountCopyItem in inventoryAccountStd.InternalAccounts)
                                    {
                                        var existingAccount = item.TemplateCompanyEconomyDataItem.GetAccount(internalAccountCopyItem.AccountId);
                                        if (existingAccount != null)
                                        {
                                            var internalAccount = entities.AccountInternal.FirstOrDefault(f => f.AccountId == existingAccount.AccountId);
                                            existingInventoryAccountStd.AccountInternal.Add(internalAccount);
                                        }
                                    }
                                }
                            }

                            SetCreatedProperties(existingTemplate);
                            entities.InventoryWriteOffTemplate.AddObject(existingTemplate);
                            existingWriteOffTemplates.Add(existingTemplate);
                        }
                    }

                    var result2 = SaveChanges(entities);
                    if (!result2.Success)
                    {
                        templateResult.ActionResults.Add(result2);
                        return templateResult;
                    }

                    // Copy TriggerAccounts

                    var templateInventoryTriggerAccounts = item.TemplateCompanyCoreDataItem.GetCompanySetting(CompanySettingType.InventoryEditTriggerAccounts)?.StrData;

                    if (!string.IsNullOrEmpty(templateInventoryTriggerAccounts))
                    {
                        // Process and copy trigger accounts
                        string[] records = templateInventoryTriggerAccounts.Split(',');
                        StringBuilder settingStr = new StringBuilder();

                        foreach (var record in records)
                        {
                            string[] valuePair = record.Split(':');

                            if (valuePair.Length != 2 || !int.TryParse(valuePair[0], out int templateAccountId) || !int.TryParse(valuePair[1], out int templateWriteOffTemplateId))
                                continue;

                            var accountStd = item.TemplateCompanyEconomyDataItem.GetAccount(templateAccountId);
                            var templateTemplate = item.TemplateCompanyEconomyDataItem.InventoryWriteOffTemplateCopyItems.FirstOrDefault(t => t.InventoryWriteOffTemplateId == templateWriteOffTemplateId);

                            var newTemplate = templateTemplate != null ? existingWriteOffTemplates.FirstOrDefault(t => t.Name == templateTemplate.Name) : null;

                            if (accountStd != null && newTemplate != null)
                            {
                                if (settingStr.Length > 0)
                                    settingStr.Append(",");
                                settingStr.Append($"{accountStd.AccountId}:{newTemplate.InventoryWriteOffTemplateId}");
                            }
                        }

                        var result3 = SettingManager.UpdateInsertStringSetting(entities, SettingMainType.Company, (int)CompanySettingType.InventoryEditTriggerAccounts, settingStr.ToString(), 0, item.DestinationActorCompanyId, 0);
                        if (!result3.Success)
                        {
                            templateResult.ActionResults.Add(result3);
                        }
                    }

                    var result4 = SaveChanges(entities);
                    if (!result4.Success)
                    {
                        templateResult.ActionResults.Add(result4);
                    }
                }
                catch (Exception ex)
                {
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("Inventory", item, ex, saved: false));
                }

            }

            return templateResult;
        }


        public List<InventoryWriteOffMethodCopyItem> GetInventoryWriteOffMethods(int actorCompanyId)
        {
            List<InventoryWriteOffMethodCopyItem> copyItems = new List<InventoryWriteOffMethodCopyItem>();
            var inventoryWriteOffMethods = InventoryManager.GetInventoryWriteOffMethods(actorCompanyId);

            foreach (var inventoryWriteOffMethod in inventoryWriteOffMethods)
            {
                InventoryWriteOffMethodCopyItem writeOffMethod = new InventoryWriteOffMethodCopyItem()
                {
                    InventoryWriteOffMethodId = inventoryWriteOffMethod.InventoryWriteOffMethodId,
                    Name = inventoryWriteOffMethod.Name,
                    Description = inventoryWriteOffMethod.Description,
                    Type = inventoryWriteOffMethod.Type,
                    PeriodType = inventoryWriteOffMethod.PeriodType,
                    PeriodValue = inventoryWriteOffMethod.PeriodValue,
                    YearPercent = inventoryWriteOffMethod.YearPercent
                };
                copyItems.Add(writeOffMethod);
            }

            return copyItems;
        }


        public List<InventoryWriteOffMethodCopyItem> GetInventoryWriteOffMethodsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetInventoryWriteOffMethods(actorCompanyId);

            return economyTemplateConnector.GetInventoryWriteOffMethodCopyItems(sysCompDbId, actorCompanyId);
        }

        public List<InventoryWriteOffTemplateCopyItem> GetInventoryWriteOffTemplateCopyItems(int actorCompanyId)
        {
            List<InventoryWriteOffTemplateCopyItem> writeOffTemplateCopyItems = new List<InventoryWriteOffTemplateCopyItem>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var writeOffTemplates = (from i in entitiesReadOnly.InventoryWriteOffTemplate
                                     .Include("VoucherSeriesType")
                                     .Include("InventoryWriteOffMethod")
                                     .Include("InventoryAccountStd")
                                     .Include("InventoryAccountStd.AccountInternal.Account")
                                     where i.ActorCompanyId == actorCompanyId &&
                                     i.State == (int)SoeEntityState.Active
                                     orderby i.Name
                                     select i);

            foreach (var template in writeOffTemplates)
            {
                InventoryWriteOffTemplateCopyItem item = new InventoryWriteOffTemplateCopyItem()
                {
                    InventoryWriteOffTemplateId = template.InventoryWriteOffTemplateId,
                    Name = template.Name,
                    Description = template.Description,
                };

                if (template.InventoryWriteOffMethod != null)
                {
                    item.WriteOffMethod = new InventoryWriteOffMethodCopyItem()
                    {
                        InventoryWriteOffMethodId = template.InventoryWriteOffMethod.InventoryWriteOffMethodId,
                        Name = template.InventoryWriteOffMethod.Name,
                        Description = template.InventoryWriteOffMethod.Description,
                        Type = template.InventoryWriteOffMethod.Type,
                        PeriodType = template.InventoryWriteOffMethod.PeriodType,
                        PeriodValue = template.InventoryWriteOffMethod.PeriodValue,
                        YearPercent = template.InventoryWriteOffMethod.YearPercent
                    };
                }

                if (template.VoucherSeriesType != null)
                {
                    item.VoucherSeriesType = new VoucherSeriesTypeCopyItem()
                    {
                        VoucherSeriesTypeId = template.VoucherSeriesTypeId,
                        VoucherSeriesTypeNr = template.VoucherSeriesType.VoucherSeriesTypeNr,
                        Name = template.VoucherSeriesType.Name
                    };
                }

                if (template.InventoryAccountStd != null)
                {
                    foreach (var account in template.InventoryAccountStd)
                    {
                        var acc = new InventoryAccountStdCopyItem()
                        {
                            AccountId = account.AccountId,
                            Type = account.Type,
                            InternalAccounts = account.AccountInternal?.Select(s => new InventoryInternalAccountCopyItem() { AccountDimId = s.Account?.AccountDimId ?? 0, AccountId = s.AccountId }).ToList() ?? new List<InventoryInternalAccountCopyItem>()
                        };
                    }
                }

                writeOffTemplateCopyItems.Add(item);
            }

            return writeOffTemplateCopyItems;
        }

        public List<InventoryWriteOffTemplateCopyItem> GetInventoryWriteOffTemplateCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetInventoryWriteOffTemplateCopyItems(actorCompanyId);

            return economyTemplateConnector.GetInventoryWriteOffTemplateCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyPaymentConditionsFromTemplateCompany(TemplateCompanyDataItem item)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                Company newCompany = CompanyManager.GetCompany(entities, item.DestinationActorCompanyId);
                if (newCompany == null)
                {
                    templateResult.ActionResults.Add(new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte")));
                    return templateResult;
                }

                try
                {
                    foreach (var paymentConditionCopyItem in item.TemplateCompanyEconomyDataItem.PaymentConditionCopyItems)
                    {

                        List<PaymentCondition> existingPaymentConditions = PaymentManager.GetPaymentConditions(entities, item.DestinationActorCompanyId).ToList();

                        PaymentCondition existingPaymentCondition = existingPaymentConditions.FirstOrDefault(p => p.Code.ToLower() == paymentConditionCopyItem.Code.ToLower() && p.Name.ToLower() == paymentConditionCopyItem.Name.ToLower());

                        if (existingPaymentCondition == null)
                        {
                            existingPaymentCondition = new PaymentCondition()
                            {
                                Company = newCompany,
                                PaymentConditionId = paymentConditionCopyItem.PaymentConditionId,
                                Code = paymentConditionCopyItem.Code,
                                Name = paymentConditionCopyItem.Name,
                                Days = paymentConditionCopyItem.Days,
                            };
                        }
                        item.TemplateCompanyEconomyDataItem.AddPaymentConditionMapping(paymentConditionCopyItem.PaymentConditionId, existingPaymentCondition);
                    }

                    var result = SaveChanges(entities);
                    if (!result.Success)
                        templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("PaymentCondition", item, saved: false));
                    templateResult.ActionResults.Add(result);
                }
                catch (Exception ex)
                {
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("PaymentCondition", item, ex, saved: false));
                }
            }
            return templateResult;
        }

        public List<PaymentConditionCopyItem> GetPaymentConditionCopyItems(int actorCompanyId)
        {
            List<PaymentConditionCopyItem> paymentConditionCopyItems = new List<PaymentConditionCopyItem>();
            int defaultCustomerPaymentCondition = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentCondition, 0, actorCompanyId, 0);
            int defaultCustomerPaymentConditionHouseholdDeduction = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction, 0, actorCompanyId, 0);
            int defaultSupplierPaymentCondition = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.SupplierPaymentDefaultPaymentCondition, 0, actorCompanyId, 0);

            var paymentConditions = PaymentManager.GetPaymentConditions(actorCompanyId);

            foreach (var condition in paymentConditions)
            {
                paymentConditionCopyItems.Add(new PaymentConditionCopyItem()
                {
                    Code = condition.Code,
                    Name = condition.Name,
                    Days = condition.Days,
                    PaymentConditionId = condition.PaymentConditionId
                });
            }

            return paymentConditionCopyItems;
        }

        public List<PaymentConditionCopyItem> GetPaymentConditionCopyItemsFromApi(int sysCompDbId, int actorCompanyId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetPaymentConditionCopyItems(actorCompanyId);

            return economyTemplateConnector.GetPaymentConditionCopyItems(sysCompDbId, actorCompanyId);
        }

        public TemplateResult CopyVatCodesFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            TemplateResult templateResult = new TemplateResult();

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    Dictionary<int, int> vatCodesMapping = new Dictionary<int, int>();

                    if (!templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountStdCopyItems.Any())
                    {
                        templateResult.ActionResults.Add(new ActionResult("AccountStdCopyItems not loaded"));
                        return templateResult;
                    }

                    var existingVatCodes = AccountManager.GetVatCodes(templateCompanyDataItem.DestinationActorCompanyId);

                    foreach (var templateVatCode in templateCompanyDataItem.TemplateCompanyEconomyDataItem.VatCodeCopyItems)
                    {
                        int? accountId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateVatCode.AccountId)?.AccountId;

                        if (!accountId.HasValue)
                        {
                            templateResult.ActionResults.Add(new ActionResult("Account not found"));
                            continue;
                        }

                        int? purchaseVatAccountId = null;
                        if (templateVatCode.PurchaseVATAccountId.HasValue)
                            purchaseVatAccountId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(templateVatCode.PurchaseVATAccountId ?? 0)?.AccountId;
                        VatCode vatCode = existingVatCodes.FirstOrDefault(w => w.Code == templateVatCode.Code);

                        if (vatCode == null)
                        {
                            vatCode = new VatCode()
                            {
                                AccountId = accountId.Value,
                                ActorCompanyId = templateCompanyDataItem.DestinationActorCompanyId,
                                Code = templateVatCode.Code,
                                Name = templateVatCode.Name,
                                Percent = templateVatCode.Percent,
                                PurchaseVATAccountId = purchaseVatAccountId,
                            };

                            SetCreatedProperties(vatCode);
                            entities.VatCode.AddObject(vatCode);
                        }
                        else
                        {
                            vatCode.Name = templateVatCode.Name;
                            vatCode.Percent = templateVatCode.Percent;
                            vatCode.PurchaseVATAccountId = purchaseVatAccountId;
                            SetModifiedProperties(vatCode);
                        }
                        var saveResult = SaveChanges(entities);

                        if (!saveResult.Success)
                        {
                            templateResult.ActionResults.Add(saveResult);
                            templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VatCode", templateCompanyDataItem, saved: false));
                        }
                        else
                        {
                            templateCompanyDataItem.TemplateCompanyEconomyDataItem.AddVatCodeMapping(templateVatCode.VatCodeId, vatCode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    templateResult.ActionResults.Add(companyTemplateManager.LogCopyError("VatCodes", templateCompanyDataItem, ex, saved: false));
                    LogError(ex.ToString());
                }
            }

            return templateResult;
        }

        public List<VatCodeCopyItem> GetVatCodeCopyItems(int actorCompanyId)
        {
            List<VatCodeCopyItem> vatCodeCopyItems = new List<VatCodeCopyItem>();
            var vatCodes = AccountManager.GetVatCodes(actorCompanyId);

            foreach (var vatCode in vatCodes)
            {
                VatCodeCopyItem item = new VatCodeCopyItem()
                {
                    Code = vatCode.Code,
                    Name = vatCode.Name,
                    Percent = vatCode.Percent,
                    PurchaseVATAccountId = vatCode.PurchaseVATAccountId,
                    AccountId = vatCode.AccountId,
                    VatCodeId = vatCode.VatCodeId
                };

                vatCodeCopyItems.Add(item);
            };

            return vatCodeCopyItems;
        }

        public List<VatCodeCopyItem> GetVatCodeCopyItemsFromApi(int sysCompDbId, int actorCompanId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetVatCodeCopyItems(actorCompanId);

            return economyTemplateConnector.GetVatCodeCopyItems(sysCompDbId, actorCompanId);
        }

        public TemplateResult CopyBaseAccountsFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem, SoeModule module)
        {
            TemplateResult templataResult = new TemplateResult();
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                #region Prereq

                CompanySettingTypeGroup settingTypeGroup = CompanyManager.GetBaseProductCompanySettingTypeGroup(module);
                if (settingTypeGroup != CompanySettingTypeGroup.Unknown)
                {
                    templataResult.ActionResults.Add(result);
                    return templataResult;
                }

                #endregion

                #region Account

                //Account not copied from template Company. Should already be added.

                #endregion

                #region UserCompanySettings

                templataResult.ActionResults.Add(CopyCompanyAccountSettings(entities, settingTypeGroup, templateCompanyDataItem));

                #endregion
            }

            return templataResult;
        }

        public ActionResult CopyCompanyAccountSettings(CompEntities entities, CompanySettingTypeGroup settingTypeGroup, TemplateCompanyDataItem templateCompanyDataItem)
        {
            try
            {
                if (!templateCompanyDataItem.TemplateCompanyCoreDataItem.CompanySettingCopyItems.Any())
                    templateCompanyDataItem.TemplateCompanyCoreDataItem.CompanySettingCopyItems = coreTemplateManager.GetCompanySettingCopyItemsFromApi(templateCompanyDataItem.SysCompDbId, templateCompanyDataItem.SourceActorCompanyId);

                if (!templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountStdCopyItems.Any())
                    templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountStdCopyItems = GetAccountStdCopyItemsFromApi(templateCompanyDataItem.SysCompDbId, templateCompanyDataItem.SourceActorCompanyId);

                foreach (var templateSetting in templateCompanyDataItem.TemplateCompanyCoreDataItem.CompanySettingCopyItems.Where(w => w.IntData.HasValue && SettingManager.GetCompanySettingTypesForGroup((int)settingTypeGroup).Contains(w.SettingTypeId)))
                {
                    if (templateSetting.IntData.HasValue)
                    {
                        int accountId = templateSetting.IntData.Value;
                        var templateAccount = templateCompanyDataItem.TemplateCompanyEconomyDataItem.AccountStdCopyItems.FirstOrDefault(w => w.AccountId == accountId);
                        if (templateAccount != null)
                        {
                            Account account = AccountManager.GetAccountByDimNr(templateAccount.AccountNr, Constants.ACCOUNTDIM_STANDARD, templateCompanyDataItem.DestinationActorCompanyId);
                            if (account != null)
                            {
                                var result = SettingManager.UpdateInsertIntSetting(entities, SettingMainType.Company, (int)templateSetting.SettingTypeId, account.AccountId, 0, templateCompanyDataItem.DestinationActorCompanyId, 0);
                                if (!result.Success)
                                    companyTemplateManager.LogCopyError("UserCompanySetting", "SettingTypeId", (int)templateSetting.SettingTypeId, "", "", templateCompanyDataItem, add: true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(ex);
            }

            return new ActionResult();

        }

        private void CopyCompanyVoucherSeriesSetting(CompanySettingType companySettingType, TemplateCompanyDataItem templateCompanyDataItem)
        {
            if (!templateCompanyDataItem.TemplateCompanyEconomyDataItem.VoucherSeriesTypeCopyItems.Any())
                templateCompanyDataItem.TemplateCompanyEconomyDataItem.VoucherSeriesTypeCopyItems = GetVoucherSeriesTypeCopyItemsFromApi(templateCompanyDataItem.SysCompDbId, templateCompanyDataItem.SourceActorCompanyId);


            var templateSetting = templateCompanyDataItem.TemplateCompanyCoreDataItem.CompanySettingCopyItems.FirstOrDefault(w => w.IntData.HasValue && w.SettingTypeId == companySettingType);

            if (templateSetting == null)
                return;

            int voucherSeriesTypeId = templateSetting.IntData.Value;
            var templateVoucherSeriesType = templateCompanyDataItem.TemplateCompanyEconomyDataItem.VoucherSeriesTypeCopyItems.FirstOrDefault(w => w.VoucherSeriesTypeId == voucherSeriesTypeId);
            if (templateVoucherSeriesType != null)
            {
                VoucherSeriesType voucherSeriesType = VoucherManager.GetVoucherSeriesTypeByName(templateVoucherSeriesType.Name, templateCompanyDataItem.DestinationActorCompanyId);
                if (voucherSeriesType != null)
                {
                    var result = SettingManager.UpdateInsertIntSetting(SettingMainType.Company, (int)templateSetting.SettingTypeId, voucherSeriesType.VoucherSeriesTypeId, 0, templateCompanyDataItem.DestinationActorCompanyId, 0);
                    if (!result.Success)
                        companyTemplateManager.LogCopyError("UserCompanySetting", "SettingTypeId", (int)templateSetting.SettingTypeId, "", "", templateCompanyDataItem, add: true);
                }
            }

        }

        public List<AccountDistribitionCopyItem> GetAccountDistribitionCopyItems(int actorCompanyId, SoeAccountDistributionType distributionType)
        {
            List<AccountDistribitionCopyItem> copyItems = new List<AccountDistribitionCopyItem>();

            foreach (var templateAccDistrHead in AccountDistributionManager.GetAccountDistributionHeads(actorCompanyId, distributionType, true, true, false, false, true))
            {
                var item = new AccountDistribitionCopyItem()
                {
                    Type = templateAccDistrHead.Type,
                    Name = templateAccDistrHead.Name,
                    Description = templateAccDistrHead.Description,
                    TriggerType = templateAccDistrHead.TriggerType,
                    CalculationType = templateAccDistrHead.CalculationType,
                    Calculate = templateAccDistrHead.Calculate,
                    PeriodType = templateAccDistrHead.PeriodType,
                    PeriodValue = templateAccDistrHead.PeriodValue,
                    Sort = templateAccDistrHead.Sort,
                    StartDate = templateAccDistrHead.StartDate,
                    EndDate = templateAccDistrHead.EndDate,
                    DayNumber = templateAccDistrHead.DayNumber,
                    Amount = templateAccDistrHead.Amount,
                    AmountOperator = templateAccDistrHead.AmountOperator,
                    KeepRow = templateAccDistrHead.KeepRow,
                    UseInVoucher = templateAccDistrHead.UseInVoucher,
                    UseInSupplierInvoice = templateAccDistrHead.UseInSupplierInvoice,
                    UseInCustomerInvoice = templateAccDistrHead.UseInCustomerInvoice,
                    UseInImport = templateAccDistrHead.UseInImport,
                    UseInPayrollVoucher = templateAccDistrHead.UseInPayrollVoucher,
                    UseInPayrollVacationVoucher = templateAccDistrHead.UseInPayrollVacationVoucher,
                    VoucherSeriesTypeId = templateAccDistrHead.VoucherSeriesTypeId,

                    AccountDistribitionRowCopyItems = new List<AccountDistribitionRowCopyItem>(),
                    AccountDistributionHeadAccountDimMappingCopyItems = new List<AccountDistributionHeadAccountDimMappingCopyItem>(),
                };

                foreach (var accountExpression in templateAccDistrHead.AccountDistributionHeadAccountDimMapping)
                {
                    var newMapping = new AccountDistributionHeadAccountDimMappingCopyItem()
                    {
                        AccountDimId = accountExpression.AccountDimId,
                        AccountExpression = accountExpression.AccountExpression,
                    };

                    item.AccountDistributionHeadAccountDimMappingCopyItems.Add(newMapping);
                }

                foreach (var accountDistributionRow in templateAccDistrHead.AccountDistributionRow)
                {
                    var newAccountDistrRow = new AccountDistribitionRowCopyItem()
                    {
                        RowNbr = accountDistributionRow.RowNbr,
                        CalculateRowNbr = accountDistributionRow.CalculateRowNbr,
                        SameBalance = accountDistributionRow.SameBalance,
                        OppositeBalance = accountDistributionRow.OppositeBalance,
                        Description = accountDistributionRow.Description,
                        AccountId = accountDistributionRow.AccountId,

                        AccountDistributionRowAccountCopyItems = new List<AccountDistributionRowAccountCopyItem>(),
                    };

                    foreach (var accountDistributionRowAccount in accountDistributionRow.AccountDistributionRowAccount)
                    {
                        var newAccountDistributionRowAccount = new AccountDistributionRowAccountCopyItem()
                        {
                            DimNr = accountDistributionRowAccount.DimNr,
                            AccountId = accountDistributionRowAccount.AccountId ?? 0,
                            KeepSourceRowAccount = accountDistributionRowAccount.KeepSourceRowAccount,
                        };

                        newAccountDistrRow.AccountDistributionRowAccountCopyItems.Add(newAccountDistributionRowAccount);
                    }

                    item.AccountDistribitionRowCopyItems.Add(newAccountDistrRow);
                }

                copyItems.Add(item);
            };

            return copyItems;
        }

        public List<AccountDistribitionCopyItem> GetAccountDistribitionCopyItemsFromApi(int sysCompDbId, int actorCompanId, SoeAccountDistributionType distributionType)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetAccountDistribitionCopyItems(actorCompanId, distributionType);

            return economyTemplateConnector.GetCopyItems<AccountDistribitionCopyItem>(sysCompDbId, actorCompanId, "Internal/Template/Economy/AccountDistributionCopyItems", new Dictionary<string, string>() { { "actorCompanyId", actorCompanId.ToString() }, { "distributionType", ((int)distributionType).ToString() } }).ToList();
        }

        public ActionResult CopyAccountDistributionTemplatesFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem, SoeAccountDistributionType distributionType)
        {
            // Default result is successful
            ActionResult result = new ActionResult(true);
            bool copyError = false;

            try
            {

                using (CompEntities entities = new CompEntities())
                {
                    if (entities.Connection.State != ConnectionState.Open)
                        entities.Connection.Open();

                    #region Prereq

                    var newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);

                    var existingAccountDistributionHeads = AccountDistributionManager.GetAccountDistributionHeads(entities, templateCompanyDataItem.DestinationActorCompanyId, distributionType, true, true, false, false, true);
                    var newAccountStds = AccountManager.GetAccountStdsByCompany(entities, templateCompanyDataItem.DestinationActorCompanyId, true);
                    var newAccountInternals = AccountManager.GetAccountInternals(entities, templateCompanyDataItem.DestinationActorCompanyId, true, true);
                    var existingVoucherSeriesTypes = VoucherManager.GetVoucherSeriesTypes(entities, templateCompanyDataItem.DestinationActorCompanyId, false);

                    List<AccountDim> accountDimsNew = AccountManager.GetAccountDimsByCompany(entities, templateCompanyDataItem.DestinationActorCompanyId, loadAccounts: true);
                    if (accountDimsNew.IsNullOrEmpty())
                        return new ActionResult((int)ActionResultSave.EntityNotFound, "AccountDim");

                    #endregion

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        var accountDistributionCopyItems = distributionType == SoeAccountDistributionType.Auto ? templateCompanyDataItem.TemplateCompanyEconomyDataItem.AutoAccountDistributionCopyItems : templateCompanyDataItem.TemplateCompanyEconomyDataItem.PeriodAccountDistributionCopyItems;
                        foreach (var templateAccDistrHead in accountDistributionCopyItems)
                        {
                            var addDistribution = false;
                            var newAccountDistributionHead = existingAccountDistributionHeads.FirstOrDefault(h => h.Name == templateAccDistrHead.Name);
                            if (newAccountDistributionHead == null)
                            {
                                #region new
                                addDistribution = true;

                                newAccountDistributionHead = new AccountDistributionHead()
                                {
                                    Type = templateAccDistrHead.Type,
                                    Name = templateAccDistrHead.Name,
                                    Description = templateAccDistrHead.Description,
                                    TriggerType = templateAccDistrHead.TriggerType,
                                    CalculationType = templateAccDistrHead.CalculationType,
                                    Calculate = templateAccDistrHead.Calculate,
                                    PeriodType = templateAccDistrHead.PeriodType,
                                    PeriodValue = templateAccDistrHead.PeriodValue,
                                    Sort = templateAccDistrHead.Sort,
                                    StartDate = templateAccDistrHead.StartDate,
                                    EndDate = templateAccDistrHead.EndDate,
                                    DayNumber = templateAccDistrHead.DayNumber,
                                    Amount = templateAccDistrHead.Amount,
                                    AmountOperator = templateAccDistrHead.AmountOperator,
                                    KeepRow = templateAccDistrHead.KeepRow,
                                    UseInVoucher = templateAccDistrHead.UseInVoucher,
                                    UseInSupplierInvoice = templateAccDistrHead.UseInSupplierInvoice,
                                    UseInCustomerInvoice = templateAccDistrHead.UseInCustomerInvoice,
                                    UseInImport = templateAccDistrHead.UseInImport,
                                    State = (int)SoeEntityState.Active,
                                    UseInPayrollVoucher = templateAccDistrHead.UseInPayrollVoucher,
                                    UseInPayrollVacationVoucher = templateAccDistrHead.UseInPayrollVacationVoucher,

                                    // references
                                    Company = newCompany,
                                };

                                if (distributionType == SoeAccountDistributionType.Period && templateAccDistrHead.VoucherSeriesTypeId.HasValue)
                                {
                                    var templateVoucherSeriesType = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetVoucherSeriesType(templateAccDistrHead.VoucherSeriesTypeId.Value);
                                    if (templateVoucherSeriesType != null)
                                        newAccountDistributionHead.VoucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == templateVoucherSeriesType.Name);
                                }

                                // Set references
                                newAccountDistributionHead.Company = newCompany;

                                foreach (var accountExpression in templateAccDistrHead.AccountDistributionHeadAccountDimMappingCopyItems)
                                {
                                    var accountDim = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccountDim(accountExpression.AccountDimId); //mappings.FirstOrDefault(m => m.Item1 == accountExpression.AccountDimId);
                                    if (accountDim != null)
                                    {
                                        var accountDimFromDatabase = entities.AccountDim.FirstOrDefault(ad => ad.AccountDimId == accountDim.AccountDimId);
                                        if (accountDimFromDatabase != null)
                                        {
                                            var newMapping = new AccountDistributionHeadAccountDimMapping()
                                            {
                                                AccountDim = accountDimFromDatabase,
                                                AccountExpression = accountExpression.AccountExpression,
                                            };

                                            newAccountDistributionHead.AccountDistributionHeadAccountDimMapping.Add(newMapping);
                                        }
                                    }
                                }

                                SetCreatedProperties(newAccountDistributionHead);

                                #endregion
                            }
                            else
                            {
                                if (!templateCompanyDataItem.Update)
                                    continue;

                                #region update

                                newAccountDistributionHead.Type = templateAccDistrHead.Type;
                                newAccountDistributionHead.Name = templateAccDistrHead.Name;
                                newAccountDistributionHead.Description = templateAccDistrHead.Description;
                                newAccountDistributionHead.TriggerType = templateAccDistrHead.TriggerType;
                                newAccountDistributionHead.CalculationType = templateAccDistrHead.CalculationType;
                                newAccountDistributionHead.Calculate = templateAccDistrHead.Calculate;
                                newAccountDistributionHead.PeriodType = templateAccDistrHead.PeriodType;
                                newAccountDistributionHead.PeriodValue = templateAccDistrHead.PeriodValue;
                                newAccountDistributionHead.Sort = templateAccDistrHead.Sort;
                                newAccountDistributionHead.StartDate = templateAccDistrHead.StartDate;
                                newAccountDistributionHead.EndDate = templateAccDistrHead.EndDate;
                                newAccountDistributionHead.DayNumber = templateAccDistrHead.DayNumber;
                                newAccountDistributionHead.Amount = templateAccDistrHead.Amount;
                                newAccountDistributionHead.AmountOperator = templateAccDistrHead.AmountOperator;
                                newAccountDistributionHead.KeepRow = templateAccDistrHead.KeepRow;
                                newAccountDistributionHead.UseInVoucher = templateAccDistrHead.UseInVoucher;
                                newAccountDistributionHead.UseInSupplierInvoice = templateAccDistrHead.UseInSupplierInvoice;
                                newAccountDistributionHead.UseInCustomerInvoice = templateAccDistrHead.UseInCustomerInvoice;
                                newAccountDistributionHead.UseInImport = templateAccDistrHead.UseInImport;
                                newAccountDistributionHead.UseInPayrollVoucher = templateAccDistrHead.UseInPayrollVoucher;
                                newAccountDistributionHead.UseInPayrollVacationVoucher = templateAccDistrHead.UseInPayrollVacationVoucher;

                                if (distributionType == SoeAccountDistributionType.Period && templateAccDistrHead.VoucherSeriesTypeId.HasValue)
                                {
                                    var templateVoucherSeriesType = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetVoucherSeriesType(templateAccDistrHead.VoucherSeriesTypeId.Value);
                                    if (templateVoucherSeriesType != null)
                                        newAccountDistributionHead.VoucherSeriesType = existingVoucherSeriesTypes.FirstOrDefault(s => s.Name == templateVoucherSeriesType.Name);
                                }

                                foreach (var accountExpression in templateAccDistrHead.AccountDistributionHeadAccountDimMappingCopyItems)
                                {
                                    var accountDim = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccountDim(accountExpression.AccountDimId); //mappings.FirstOrDefault(m => m.Item1 == accountExpression.AccountDimId);
                                    if (accountDim != null)
                                    {
                                        var existingExpression = newAccountDistributionHead.AccountDistributionHeadAccountDimMapping.FirstOrDefault(m => m.AccountDimId == accountDim.AccountDimId);
                                        if (existingExpression != null)
                                        {
                                            existingExpression.AccountExpression = accountExpression.AccountExpression;
                                        }
                                        else
                                        {
                                            var newMapping = new AccountDistributionHeadAccountDimMapping()
                                            {
                                                AccountDim = accountDim,
                                                AccountExpression = accountExpression.AccountExpression,
                                            };

                                            newAccountDistributionHead.AccountDistributionHeadAccountDimMapping.Add(newMapping);
                                        }
                                    }
                                }

                                SetModifiedProperties(newAccountDistributionHead);

                                #endregion
                            }

                            foreach (var accountDistributionRow in templateAccDistrHead.AccountDistribitionRowCopyItems)
                            {
                                AccountDistributionRow newAccountDistrRow = null;
                                if (templateCompanyDataItem.Update)
                                    newAccountDistrRow = newAccountDistributionHead.AccountDistributionRow.FirstOrDefault(r => r.RowNbr == accountDistributionRow.RowNbr);

                                if (newAccountDistrRow == null)
                                {
                                    var accountStdId = accountDistributionRow.AccountId.HasValue ? templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(accountDistributionRow.AccountId.Value) : null;

                                    newAccountDistrRow = new AccountDistributionRow()
                                    {
                                        RowNbr = accountDistributionRow.RowNbr,
                                        CalculateRowNbr = accountDistributionRow.CalculateRowNbr,
                                        SameBalance = accountDistributionRow.SameBalance,
                                        OppositeBalance = accountDistributionRow.OppositeBalance,
                                        Description = accountDistributionRow.Description,

                                        //Set references
                                        AccountStd = accountStdId != null ? entities.AccountStd.FirstOrDefault(a => a.AccountId == accountStdId.AccountId) : null,
                                    };

                                    newAccountDistributionHead.AccountDistributionRow.Add(newAccountDistrRow);
                                }
                                else
                                {

                                    var accountStdId = templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(accountDistributionRow.AccountId.Value);
                                    newAccountDistrRow.RowNbr = accountDistributionRow.RowNbr;
                                    newAccountDistrRow.CalculateRowNbr = accountDistributionRow.CalculateRowNbr;
                                    newAccountDistrRow.SameBalance = accountDistributionRow.SameBalance;
                                    newAccountDistrRow.OppositeBalance = accountDistributionRow.OppositeBalance;
                                    newAccountDistrRow.Description = accountDistributionRow.Description;

                                    //Set references
                                    newAccountDistrRow.AccountStd = accountStdId != null ? entities.AccountStd.FirstOrDefault(a => a.AccountId == accountStdId.AccountId) : null;
                                }

                                foreach (var accountDistrRowAccount in accountDistributionRow.AccountDistributionRowAccountCopyItems)
                                {
                                    var newAccountDistrRowAccount = newAccountDistrRow.AccountDistributionRowAccount.FirstOrDefault(a => a.DimNr == accountDistrRowAccount.DimNr);
                                    if (newAccountDistrRowAccount == null)
                                    {
                                        int? accountId = accountDistrRowAccount.AccountId != 0 ? templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(accountDistrRowAccount.AccountId)?.AccountId : (int?)null;
                                        newAccountDistrRowAccount = new AccountDistributionRowAccount()
                                        {
                                            DimNr = 2,
                                            AccountId = accountId,
                                            KeepSourceRowAccount = accountDistrRowAccount.KeepSourceRowAccount
                                        };
                                        newAccountDistrRow.AccountDistributionRowAccount.Add(newAccountDistrRowAccount);
                                    }
                                    else
                                    {
                                        int? accountId = accountDistrRowAccount.AccountId != 0 ? templateCompanyDataItem.TemplateCompanyEconomyDataItem.GetAccount(accountDistrRowAccount.AccountId)?.AccountId : (int?)null;

                                        newAccountDistrRowAccount.AccountId = accountId;
                                        newAccountDistrRowAccount.KeepSourceRowAccount = accountDistrRowAccount.KeepSourceRowAccount;
                                    }
                                }
                            }

                            // Add AccountDistributionHead to context
                            if (addDistribution)
                                entities.AccountDistributionHead.AddObject(newAccountDistributionHead);
                        }

                        result = SaveChanges(entities, transaction);
                        if (!result.Success)
                        {
                            transaction.Dispose();
                            return result;
                        }

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            finally
            {
                if (copyError)
                    result = new ActionResult(false);
                else
                    result = new ActionResult(true);
            }

            return result;
        }

        #region DistributionCodes

        public List<DistributionCodeHeadCopyItem> GetDistribitionCodeHeadCopyItems(int actorCompanyId)
        {
            List<DistributionCodeHeadCopyItem> copyItems = new List<DistributionCodeHeadCopyItem>();

            foreach (var templateDistributionCode in BudgetManager.GetDistributionCodes(actorCompanyId, true, true))
            {
                var item = new DistributionCodeHeadCopyItem()
                {
                    DistributionCodeHeadId = templateDistributionCode.DistributionCodeHeadId,
                    Type = templateDistributionCode.Type,
                    Name = templateDistributionCode.Name,
                    NoOfPeriods = templateDistributionCode.NoOfPeriods,
                    SubType = templateDistributionCode.SubType,
                    OpeningHoursId = templateDistributionCode.OpeningHoursId,
                    AccountDimId = templateDistributionCode.AccountDimId,
                    FromDate = templateDistributionCode.FromDate,
                    ParentId = templateDistributionCode.ParentId,

                    DistributionCodePeriodCopyItem = new List<DistributionCodePeriodCopyItem>(),
                };

                if (templateDistributionCode.OpeningHoursId.HasValue)
                {
                    if (!templateDistributionCode.OpeningHoursReference.IsLoaded)
                        templateDistributionCode.OpeningHoursReference.Load();

                    item.OpeningHoursId = templateDistributionCode.OpeningHoursId.Value;
                    item.OpeningHoursName = templateDistributionCode.OpeningHours.Name;
                }

                if (templateDistributionCode.AccountDimId.HasValue)
                {
                    if (!templateDistributionCode.AccountDimReference.IsLoaded)
                        templateDistributionCode.AccountDimReference.Load();

                    item.AccountDimId = templateDistributionCode.AccountDimId.Value;
                    item.AccountDimNr = templateDistributionCode.AccountDim.AccountDimNr;
                    item.AccountDimName = templateDistributionCode.AccountDim.Name;
                }

                foreach (var templateDistributionCodePeriod in templateDistributionCode.DistributionCodePeriod)
                {
                    var period = new DistributionCodePeriodCopyItem()
                    {
                        Percent = templateDistributionCodePeriod.Percent,
                        Comment = templateDistributionCodePeriod.Comment,
                        ParentToDistributionCodeHeadId = templateDistributionCodePeriod.ParentToDistributionCodeHeadId,
                    };

                    item.DistributionCodePeriodCopyItem.Add(period);
                }

                copyItems.Add(item);
            };

            return copyItems;
        }

        public List<DistributionCodeHeadCopyItem> GetDistribitionCodeHeadCopyItemsFromApi(int sysCompDbId, int actorCompanId)
        {
            if (sysCompDbId == ConfigurationSetupUtil.GetCurrentSysCompDbId())
                return GetDistribitionCodeHeadCopyItems(actorCompanId);

            return economyTemplateConnector.GetCopyItems<DistributionCodeHeadCopyItem>(sysCompDbId, actorCompanId, "Internal/Template/Economy/DistributionCodeCopyItems", new Dictionary<string, string>() { { "actorCompanyId", actorCompanId.ToString() } }).ToList();
        }

        public ActionResult CopyDistributionCodesFromTemplateCompany(TemplateCompanyDataItem templateCompanyDataItem)
        {
            ActionResult result = new ActionResult(true);

            using (CompEntities entities = new CompEntities())
            {
                try
                {
                    entities.Connection.Open();

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        if (entities.Connection.State != ConnectionState.Open)
                            entities.Connection.Open();

                        #region Prereq

                        var newCompany = CompanyManager.GetCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);

                        var existingDistributionCodes = BudgetManager.GetDistributionCodes(entities, templateCompanyDataItem.DestinationActorCompanyId, true, true);
                        var existingAccountDims = AccountManager.GetAccountDimsByCompany(entities, templateCompanyDataItem.DestinationActorCompanyId, loadAccounts: true);
                        var existingOpeningHours = CalendarManager.GetOpeningHoursForCompany(entities, templateCompanyDataItem.DestinationActorCompanyId);

                        #endregion

                        #region Perform
                        Dictionary<int, DistributionCodeHead> mappedDistributionCodeHeads = new Dictionary<int, DistributionCodeHead>();

                        foreach (var templateDistributionCode in templateCompanyDataItem.TemplateCompanyEconomyDataItem.DistributionCodeHeadCopyItems)
                        {
                            var head = existingDistributionCodes.FirstOrDefault(h => h.Name == templateDistributionCode.Name);
                            if (head != null)
                            {
                                mappedDistributionCodeHeads.Add(templateDistributionCode.DistributionCodeHeadId, head);
                            }
                            else
                            {
                                //New
                                head = new DistributionCodeHead()
                                {
                                    Type = templateDistributionCode.Type,
                                    Name = templateDistributionCode.Name,
                                    NoOfPeriods = templateDistributionCode.NoOfPeriods,
                                    SubType = templateDistributionCode.SubType,
                                    OpeningHoursId = templateDistributionCode.OpeningHoursId,
                                    FromDate = templateDistributionCode.FromDate,

                                    Company = newCompany,
                                };

                                if (templateDistributionCode.AccountDimId.HasValue)
                                {
                                    var existingAccountDim = existingAccountDims.FirstOrDefault(d => d.AccountDimNr == templateDistributionCode.AccountDimNr && d.Name == templateDistributionCode.Name);
                                    if (existingAccountDim != null)
                                        head.AccountDim = existingAccountDim;
                                }

                                if (templateDistributionCode.OpeningHoursId.HasValue)
                                {
                                    var existingOpeningHour = existingOpeningHours.FirstOrDefault(o => o.Name == templateDistributionCode.OpeningHoursName);
                                    if (existingOpeningHour != null)
                                        head.OpeningHours = existingOpeningHour;
                                }

                                SetCreatedProperties(head);
                                entities.DistributionCodeHead.AddObject(head);

                                mappedDistributionCodeHeads.Add(templateDistributionCode.DistributionCodeHeadId, head);
                            }
                        }

                        foreach (var templateDistributionCode in templateCompanyDataItem.TemplateCompanyEconomyDataItem.DistributionCodeHeadCopyItems)
                        {
                            var head = mappedDistributionCodeHeads[templateDistributionCode.DistributionCodeHeadId];
                            if (head != null)
                            {
                                if (!templateDistributionCode.ParentId.IsNullOrEmpty())
                                {
                                    var parentHead = mappedDistributionCodeHeads[templateDistributionCode.ParentId.Value];
                                    if (parentHead != null)
                                        head.ParentId = parentHead.DistributionCodeHeadId;
                                }

                                foreach (var templateDistributionCodePeriod in templateDistributionCode.DistributionCodePeriodCopyItem)
                                {
                                    var newPeriod = new DistributionCodePeriod()
                                    {
                                        Percent = templateDistributionCodePeriod.Percent,
                                        Comment = templateDistributionCodePeriod.Comment,

                                        DistributionCodeHead = head,
                                    };

                                    if (!templateDistributionCode.ParentId.IsNullOrEmpty())
                                    {
                                        var parentHead = mappedDistributionCodeHeads[templateDistributionCode.ParentId.Value];
                                        if (parentHead != null)
                                            newPeriod.ParentToDistributionCodeHeadId = parentHead.DistributionCodeHeadId;
                                    }

                                    SetCreatedProperties(newPeriod);
                                    entities.DistributionCodePeriod.AddObject(newPeriod);
                                }
                            }
                        }

                        result = SaveChanges(entities, transaction);

                        #endregion

                        //Commit transaction
                        if (result.Success)
                            transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                }
                finally
                {
                    entities.Connection.Close();
                }
            }

            return result;
        }

        #endregion
    }
}
