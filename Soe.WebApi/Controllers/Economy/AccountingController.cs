using Soe.WebApi.Binders;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.Controllers.Economy
{
    [RoutePrefix("Economy/Accounting")]
    public class AccountingController : SoeApiController
    {
        #region Variables

        private readonly AccountManager am;
        private readonly AccountBalanceManager abm;
        private readonly AccountDistributionManager adm;
        private readonly BudgetManager bm;
        private readonly CompanyManager com;
        private readonly GeneralManager gm;
        private readonly GrossProfitManager gpm;
        private readonly ImportExportManager iem;
        private readonly InventoryManager invm;
        private readonly InvoiceManager im;
        private readonly LiquidityPlanningManager lpm;
        private readonly PaymentManager pm;
        private readonly ProductManager prm;
        private readonly ReportManager rm;
        private readonly VoucherManager vm;

        #endregion

        #region Constructor

        public AccountingController(AccountManager am, AccountBalanceManager abm, AccountDistributionManager adm, VoucherManager vm, GrossProfitManager gpm, InvoiceManager im, BudgetManager bm, PaymentManager pm, ImportExportManager iem, GeneralManager gm, ProductManager prm, CompanyManager com, ReportManager rm, LiquidityPlanningManager lpm, InventoryManager invm)
        {
            this.am = am;
            this.abm = abm;
            this.adm = adm;
            this.vm = vm;
            this.gpm = gpm;
            this.bm = bm;
            this.im = im;
            this.pm = pm;
            this.iem = iem;
            this.gm = gm;
            this.prm = prm;
            this.com = com;
            this.rm = rm;
            this.lpm = lpm;
            this.invm = invm;
        }

        #endregion

        #region Account

        [HttpGet]
        [Route("CurrentAccountYear/")]
        public IHttpActionResult GetCurrentAccountYear()
        {
            return Content(HttpStatusCode.OK, am.GetCurrentAccountYear(base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("Account/")]
        public IHttpActionResult GetAccounts(HttpRequestMessage message)
        {
            int accountDimId = message.GetIntValueFromQS("accountDimId");
            int accountYearId = message.GetIntValueFromQS("accountYearId");
            bool setLinkedToShiftType = message.GetBoolValueFromQS("setLinkedToShiftType");
            bool getCategories = message.GetBoolValueFromQS("getCategories");
            bool setParent = message.GetBoolValueFromQS("setParent");
            bool ignoreHierarchyOnly = message.GetBoolValueFromQS("ignoreHierarchyOnly");

            if (accountDimId == 0)
                accountDimId = am.GetAccountDimStdId(base.ActorCompanyId);

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, am.GetAccounts(base.ActorCompanyId, accountDimId, accountYearId, ignoreHierarchyOnly: ignoreHierarchyOnly).ToSmallDTOs());
            else
                return Content(HttpStatusCode.OK, am.GetAccounts(base.ActorCompanyId, accountDimId, accountYearId, false, false, true, false, true, setLinkedToShiftType, getCategories, setParent, ignoreHierarchyOnly: ignoreHierarchyOnly).ToGridDTOs(getCategories, setParent));
        }

        [HttpGet]
        [Route("Account/{accountNr}/{accountDimId}/{matchAll}")]
        public IHttpActionResult GetAccountByAccountNr(string accountNr, int accountDimId, bool matchAll)
        {
            return Content(HttpStatusCode.OK, am.GetAccountsByAccountNr(accountNr, accountDimId, matchAll, loadAccountStd: true).ToGridDTOs());
        }

        [HttpGet]
        [Route("Account/ById/")]
        public IHttpActionResult GetAccount(HttpRequestMessage message)
        {
            int accountId = message.GetIntValueFromQS("accountId");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, am.GetAccount(base.ActorCompanyId, accountId, onlyActive: false).ToSmallDTO());
            else
                return Content(HttpStatusCode.OK, am.GetAccount(base.ActorCompanyId, accountId, onlyActive: false, loadAccount: true, loadAccountDim: true, loadAccountMapping: true, loadAccountSru: true, loadCompanyExternalCodes: true).ToEditDTO(true));
        }

        [HttpGet]
        [Route("Account/Name/{accountId:int}")]
        public IHttpActionResult GetAccountName(int accountId)
        {
            return Content(HttpStatusCode.OK, am.GetAccountName(base.ActorCompanyId, accountId, false));
        }

        [HttpGet]
        [Route("AccountDict/{accountDimId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetAccountsDict(int accountDimId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetAccountsDict(base.ActorCompanyId, accountDimId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("AccountChildren/{parentAccountId:int}")]
        public IHttpActionResult GetAccountsChildrenDict(int parentAccountId)
        {
            return Content(HttpStatusCode.OK, am.GetChildrenAccounts(base.ActorCompanyId, parentAccountId).ToDTOs());
        }

        [HttpGet]
        [Route("Account/AccountsFromHierarchy/{accountId:int}/{includeVirtualParented:bool}/{includeOnlyChildrenOneLevel:bool}")]
        public IHttpActionResult GetAccountsFromHierarchy(int accountId, bool includeVirtualParented, bool includeOnlyChildrenOneLevel)
        {
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, includeVirtualParented);
            input.AddParamValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel, includeOnlyChildrenOneLevel);

            return Content(HttpStatusCode.OK, am.GetAccountsFromHierarchyById(base.ActorCompanyId, accountId, input));
        }

        [HttpGet]
        [Route("Account/SiblingAccounts/{accountId:int}")]
        public IHttpActionResult GetSiblingAccounts(int accountId)
        {
            return Content(HttpStatusCode.OK, am.GetSiblingAccounts(base.ActorCompanyId, accountId));
        }

        [HttpGet]
        [Route("Account/GetSelectableEmployeeShiftAccountIds/{employeeId:int}/{date}")]
        public IHttpActionResult GetSelectableEmployeeShiftAccountIds(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, am.GetSelectableEmployeeShiftAccounts(base.UserId, base.ActorCompanyId, employeeId, BuildDateTimeFromString(date, true).Value, includeAbstract: true).Select(a => a.AccountId).ToList());
        }

        [HttpGet]
        [Route("Account/SysVatRate/{accountId:int}")]
        public IHttpActionResult GetAccountSysVatRate(int accountId)
        {
            return Content(HttpStatusCode.OK, am.GetSysVatRateValueFromAccount(accountId));
        }

        [HttpGet]
        [Route("Account/GetAccountingFromString/{accountingString}")]
        public IHttpActionResult GetAccountingFromString(string accountingString)
        {
            return Content(HttpStatusCode.OK, am.GetAccountingFromString(accountingString, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Account/HouseholdProductAccountId/{productId:int}")]
        public IHttpActionResult GetHouseholdProductAccountId(int productId)
        {
            return Content(HttpStatusCode.OK, prm.GetProductAccountId(productId, ProductAccountType.Purchase));
        }

        [HttpGet]
        [Route("Account/Validate/{accountNr}/{accountId:int}/{accountDimId:int}")]
        public IHttpActionResult ValidateAccount(string accountNr, int accountId, int accountDimId)
        {
            return Content(HttpStatusCode.OK, am.ValidateAccount(accountNr, accountId, accountDimId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Account")]
        public IHttpActionResult SaveAccount(SaveAccountModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SaveAccount(model.Account, model.Translations, model.AccountMappings, model.CategoryAccounts, model.ExtraFields, model.SkipStateValidation ?? false));
        }

        [HttpPost]
        [Route("Account/Small")]
        public IHttpActionResult SaveAccountSmall(SaveAccountSmallModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.AddAccount(model.AccountNr, model.Name, model.AccountTypeId, model.VatAccountId, model.SruCode1Id, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("Account/UpdateState")]
        public IHttpActionResult UpdateAccountsState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.UpdateAccountsState(model.Dict));
        }

        [HttpDelete]
        [Route("Account/{accountId:int}")]
        public IHttpActionResult DeleteAccount(int accountId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.DeleteAccount(accountId));
        }

        #endregion

        #region AccountBalance

        [HttpGet]
        [Route("AccountBalance/ByAccount/{accountId:int}/{loadYear:bool}")]
        public IHttpActionResult GetAccountBalanceByAccount(int accountId, bool loadYear)
        {
            return Content(HttpStatusCode.OK, abm.GetAccountBalanceByAccount(accountId, loadYear: loadYear).ToDTOs(loadYear, false));
        }

        [HttpPost]
        [Route("AccountBalance/CalculateForAccounts/{accountYearId:int}")]
        public IHttpActionResult CalculateAccountBalanceForAccounts(int accountYearId)
        {
            return Content(HttpStatusCode.OK, abm.CalculateAccountBalanceForAccounts(base.ActorCompanyId, accountYearId));
        }

        [HttpPost]
        [Route("AccountBalance/CalculateForAccountsAllYears")]
        public IHttpActionResult CalculateForAccountsAllYears()
        {
            return Content(HttpStatusCode.OK, abm.CalculateAccountBalanceForAccounts(base.ActorCompanyId, null));
        }

        [HttpPost]
        [Route("AccountBalance/CalculateForAccountInAccountYears/{accountId:int}")]
        public IHttpActionResult CalculateAccountBalanceForAccountInAccountYears(int accountId)
        {
            return Content(HttpStatusCode.OK, abm.CalculateAccountBalanceForAccountInAccountYears(base.ActorCompanyId, accountId));
        }

        [HttpPost]
        [Route("AccountBalance/CalculateAccountBalanceForAccountsFromVoucher")]
        public IHttpActionResult CalculateAccountBalanceForAccountsFromVoucher(CalculateAccountBalanceForAccountsFromVoucherModel model)
        {
            abm.CalculateAccountBalanceForAccountsFromVoucher(base.ActorCompanyId, model.accountYearId, 3);
            return Content(HttpStatusCode.OK, "");
        }

        [HttpPost]
        [Route("AccountBalance/GetAccountBalances/{accountYearId:int}")]
        public IHttpActionResult GetAccountBalances(int accountYearId)
        {
            //AccountBalanceManager abm = new AccountBalanceManager(null, actorCompanyId);
            return Content(HttpStatusCode.OK, abm.GetAccountBalancesByYearDict(accountYearId));
        }

        #endregion

        #region AccountDim

        [HttpGet]
        [Route("AccountDim/")]
        public IHttpActionResult GetAccountDims(HttpRequestMessage message)
        {
            int companyId = message.GetIntValueFromQS("companyId");
            if (companyId == 0)
                companyId = base.ActorCompanyId;
            int accountDimId = message.GetIntValueFromQS("accountDimId");
            bool onlyStandard = message.GetBoolValueFromQS("onlyStandard");
            bool onlyInternal = message.GetBoolValueFromQS("onlyInternal");
            bool loadAccounts = message.GetBoolValueFromQS("loadAccounts");
            bool loadInternalAccounts = message.GetBoolValueFromQS("loadInternalAccounts");
            bool loadParent = message.GetBoolValueFromQS("loadParent");
            bool includeInactives = message.GetBoolValueFromQS("loadInactives");
            bool includeInactiveDims = message.GetBoolValueFromQS("loadInactiveDims");
            bool includeParentAccounts = message.GetBoolValueFromQS("includeParentAccounts");
            bool ignoreHierarchyOnly = message.GetBoolValueFromQS("ignoreHierarchyOnly");

            if (ignoreHierarchyOnly && !loadAccounts)
                loadAccounts = true;

            if (accountDimId != 0)
            {
                var dim = am.GetAccountDim(accountDimId, companyId, includeInactiveDims);

                if (dim != null)
                {
                    // Get one dim
                    if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                    {
                        var dto = dim.ToSmallDTO(loadAccounts, loadInternalAccounts, includeInactives);
                        am.FilterAccountsOnAccountDims(dto.ObjToList(), companyId, base.UserId, ignoreHierarchyOnly: ignoreHierarchyOnly);

                        return Content(HttpStatusCode.OK, dto);
                    }

                    else
                    {
                        var dto = dim.ToDTO();
                        am.FilterAccountsOnAccountDims(dto.ObjToList(), companyId, base.UserId, ignoreHierarchyOnly: ignoreHierarchyOnly);

                        return Content(HttpStatusCode.OK, dto);
                    }
                }
            }
            else
            {
                List<AccountDim> dims = am.GetAccountDimsByCompany(companyId, onlyStandard, onlyInternal, includeInactiveDims ? (bool?)null : true, loadAccounts, loadInternalAccounts, loadParent || includeParentAccounts);

                if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                {
                    var dtos = dims.ToSmallDTOs(loadAccounts, loadInternalAccounts, includeInactives).ToList();
                    am.FilterAccountsOnAccountDims(dtos, companyId, base.UserId, ignoreHierarchyOnly: ignoreHierarchyOnly, includeParentAccounts: includeParentAccounts, useEmployeeAccountIfNoAttestRole: true);

                    return Content(HttpStatusCode.OK, dtos);
                }

                else
                {
                    var dtos = dims.ToDTOs(loadAccounts, loadInternalAccounts);
                    am.FilterAccountsOnAccountDims(dtos, companyId, base.UserId, ignoreHierarchyOnly: ignoreHierarchyOnly);

                    return Content(HttpStatusCode.OK, dtos);
                }
            }

            return Content(HttpStatusCode.OK, new List<AccountDimSmallDTO>());
        }

        [HttpGet]
        [Route("AccountDimStd/")]
        public IHttpActionResult GetAccountDimStd()
        {
            return Content(HttpStatusCode.OK, am.GetAccountDimStd(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AccountDim/Project")]
        public IHttpActionResult GetProjectAccountDim()
        {
            return Content(HttpStatusCode.OK, am.GetProjectAccountDim(base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("AccountDim/ShiftType/{loadAccounts:bool}")]
        public IHttpActionResult GetShiftTypeAccountDim(bool loadAccounts)
        {
            return Content(HttpStatusCode.OK, am.GetShiftTypeAccountDimDTO(base.ActorCompanyId, loadAccounts));
        }

        [HttpGet]
        [Route("AccountDim/bySieNr/{sieDimNr:int}")]
        public IHttpActionResult GetAccountDimBySieNr(int sieDimNr)
        {
            return Content(HttpStatusCode.OK, am.GetAccountDimBySieNr(sieDimNr, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("AccountDim/Chars")]
        public IHttpActionResult GetAccountDimChars()
        {
            return Content(HttpStatusCode.OK, am.GetAccountDimChars());
        }

        [HttpGet]
        [Route("AccountDim/Validate/{accountDimNr:int}/{accountDimId:int}")]
        public IHttpActionResult ValidateAccountDim(int accountDimNr, int accountDimId)
        {
            return Content(HttpStatusCode.OK, am.ValidateAccountDimNr(accountDimNr, accountDimId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AccountDim")]
        public IHttpActionResult SaveAccountDim(SaveAccountDimModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SaveAccountDim(model.AccountDim, model.Reset, base.RoleId));
        }

        [HttpDelete]
        [Route("AccountDim")]
        public IHttpActionResult DeleteAccountDim([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] accDimIds)
        {
            return Content(HttpStatusCode.OK, am.DeleteAccountDims(accDimIds));
        }

        #endregion

        #region AccountInternal

        [HttpGet]
        [Route("Account/Internal/ByDim/{accountDimId:int}")]
        public IHttpActionResult GetAccountInternalsByDim(int accountDimId)
        {
            return Content(HttpStatusCode.OK, am.GetAccountInternalsByDim(accountDimId, base.ActorCompanyId).ToDTOs());
        }

        #endregion

        #region AccountMapping

        [HttpGet]
        [Route("AccountMapping/{accountId:int}")]
        public IHttpActionResult GetAccountMappings(int accountId)
        {
            return Content(HttpStatusCode.OK, am.GetAccountMappingsForAllDims(base.ActorCompanyId, accountId));
        }

        #endregion

        #region AccountDistribution

        [HttpGet]
        [Route("AccountDistribution/UsedIn")]
        public IHttpActionResult GetAccountDistributionHeadsUsedIn(SoeAccountDistributionType? type = null, TermGroup_AccountDistributionTriggerType? triggerType = null, DateTime? date = null, bool? useInVoucher = null, bool? useInSupplierInvoice = null, bool? useInCustomerInvoice = null, bool? useInImport = null, bool? useInPayrollVoucher = null, bool? useInPayrollVacationVoucher = null)
        {
            List<AccountDim> accountDimInternals = am.GetAccountDimsByCompany(base.ActorCompanyId);
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionHeadsUsedIn(base.ActorCompanyId, type, date, useInVoucher, useInSupplierInvoice, useInCustomerInvoice, useInImport, triggerType, useInPayrollVoucher, useInPayrollVacationVoucher).ToDTOs(true, true, accountDimInternals));
        }

        [HttpGet]
        [Route("AccountDistribution/{loadOpen:bool}/{loadClosed:bool}/{loadEntries:bool}")]
        public IHttpActionResult GetAccountDistributionHeads(bool loadOpen = true, bool loadClosed = true, bool loadEntries = true)
        {
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionHeads(base.ActorCompanyId, SoeAccountDistributionType.Period, false, loadOpen, loadClosed, loadEntries, true).ToSmallDTOs(true));
        }

        [HttpGet]
        [Route("AccountDistribution/{accountDistributionHeadId:int}")]
        public IHttpActionResult GetAccountDistributionHead(int accountDistributionHeadId)
        {
            List<AccountDim> accountDimInternals = am.GetAccountDimsByCompany(base.ActorCompanyId);
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionHead(accountDistributionHeadId).ToDTO(true, true, accountDimInternals));
        }

        [HttpGet]
        [Route("AccountDistributionAuto/")]
        public IHttpActionResult GetAccountDistributionHeadsAuto()
        {
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionHeads(base.ActorCompanyId, SoeAccountDistributionType.Auto, false, loadAccount: true).ToSmallDTOs(true));
        }

        [HttpGet]
        [Route("AccountDistribution/GetAccountDistributionTraceViews/{accountDistributionHeadId:int}")]
        public IHttpActionResult GetAccountDistributionTaceViews(int accountDistributionHeadId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, adm.GetAccountDistributionTraceViews(accountDistributionHeadId));
        }

        [HttpPost]
        [Route("AccountDistribution")]
        public IHttpActionResult SaveAccountDistribution(SaveAccountDistributionModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, adm.SaveAccountDistribution(model.AccountDistributionHead, model.AccountDistributionRows, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("AccountDistribution/{accountDistributionHeadId:int}")]
        public IHttpActionResult DeleteAccountDistribution(int accountDistributionHeadId)
        {
            return Content(HttpStatusCode.OK, adm.DeleteAccountDistribution(accountDistributionHeadId));
        }

        #endregion

        #region AccountDistributionEntry        
        [HttpGet]
        [Route("AccountDistributionEntries/{periodDate}/{accountDistributionType:int}/{onlyActive}")]
        public IHttpActionResult GetAccountDistributionEntries(string periodDate, int accountDistributionType, bool onlyActive)
        {
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionEntriesDTO(base.ActorCompanyId, BuildDateTimeFromString(periodDate, false).Value, (SoeAccountDistributionType)accountDistributionType, onlyActive));
        }

        [HttpGet]
        [Route("AccountDistributionEntriesForHead/{accountDistributionHeadId}")]
        public IHttpActionResult GetAccountDistributionEntriesForHead(int accountDistributionHeadId)
        {
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionEntriesForHead(accountDistributionHeadId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AccountDistributionEntriesForSource/{accountDistributionHeadId}/{registrationType}/{sourceId}")]
        public IHttpActionResult GetAccountDistributionEntriesForSource(int accountDistributionHeadId, int registrationType, int sourceId)
        {
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionEntryDTOsForSource(base.ActorCompanyId, accountDistributionHeadId, registrationType, sourceId));
        }

        [HttpPost]
        [Route("AccountDistributionEntries/TransferToAccountDistributionEntry")]
        public IHttpActionResult TransferToAccountDistributionEntry(TransferToAccountDistributionEntryModel model)
        {
            var type = (SoeAccountDistributionType)model.AccountDistributionType;
            if (SoeAccountDistributionType.Period == type)
                return Content(HttpStatusCode.OK, adm.CreateAccrualsForPeriod(ActorCompanyId, model.PeriodDate));
            else
                return Content(HttpStatusCode.OK, invm.TransferToAccountDistributionEntry(ActorCompanyId, model.PeriodDate));
        }

        [HttpPost]
        [Route("AccountDistributionEntries/TransferAccountDistributionEntryToVoucher")]
        public IHttpActionResult TransferAccountDistributionEntryToVoucher(TransferAccountDistributionEntryToVoucherModel model)
        {
            return Content(HttpStatusCode.OK, adm.TransferAccountDistributionEntryDTOsToVoucher(model.AccountDistributionEntryDTOs, base.ActorCompanyId, (SoeAccountDistributionType)model.AccountDistributionType));
        }

        [HttpPost]
        [Route("AccountDistributionEntries/RestoreAccountDistributionEntries")]
        public IHttpActionResult RestoreAccountDistributionEntries(RestoreAccountDistributionEntryModel model)
        {
            return Content(HttpStatusCode.OK, adm.RestoreAccountDistributionEntries(model.AccountDistributionEntryDTO, base.ActorCompanyId, (SoeAccountDistributionType)model.AccountDistributionType));
        }

        [HttpPost]
        [Route("AccountDistributionEntries/DeleteAccountDistributionEntries")]
        public IHttpActionResult DeleteAccountDistributionEntries(DeleteDistributionEntryModel model)
        {
            return Content(HttpStatusCode.OK, adm.DeleteAccountDistributionEntries(model.AccountDistributionEntryDTOs, base.ActorCompanyId, (SoeAccountDistributionType)model.AccountDistributionType));
        }

        [HttpPost]
        [Route("AccountDistributionEntries/DeleteAccountDistributionEntriesPermanently")]
        public IHttpActionResult DeleteAccountDistributionEntriesPermanently(DeletePermanentlyDistributionEntryModel model)
        {
            return Content(HttpStatusCode.OK, adm.DeleteAccountDistributionEntriesPermanently(model.AccountDistributionEntryDTO, base.ActorCompanyId, (SoeAccountDistributionType)model.AccountDistributionType));
        }

        [HttpPost]
        [Route("AccountDistributionEntries/DeleteAccountDistributionEntriesForSource/{accountDistributionHeadId}/{registrationType}/{sourceId}")]
        public IHttpActionResult DeleteAccountDistributionEntriesForSource(int accountDistributionHeadId, int registrationType, int sourceId)
        {
            return Content(HttpStatusCode.OK, adm.DeleteAccountDistributionEntriesForSource(base.ActorCompanyId, accountDistributionHeadId, registrationType, sourceId));
        }

        #endregion

        #region AccountPeriod

        [HttpGet]
        [Route("AccountPeriod/{accountYearId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetAccountPeriodDict(int accountYearId, bool addEmptyRow)
        {
            var dict = am.GetAccountPeriodsIntervalDict(accountYearId, addEmptyRow).ToSmallGenericTypes();
            dict.Sort((x, y) => x.Name.CompareTo(y.Name));
            return Content(HttpStatusCode.OK, dict);
        }

        [HttpGet]
        [Route("AccountPeriods/{accountYearId:int}")]
        public IHttpActionResult GetAccountPeriods(int accountYearId)
        {
            return Content(HttpStatusCode.OK, am.GetAccountPeriods(accountYearId, false).ToDTOs());
        }

        [HttpGet]
        [Route("AccountPeriod/{accountYearId:int}/{date}/{includeAccountYear:bool}")]
        public IHttpActionResult GetAccountPeriod(int accountYearId, string date, bool includeAccountYear)
        {
            return Content(HttpStatusCode.OK, am.GetAccountPeriod(accountYearId, BuildDateTimeFromString(date, true).Value, base.ActorCompanyId, includeAccountYear).ToDTO());
        }

        [HttpGet]
        [Route("AccountPeriod/Id/{accountYearId:int}/{date}")]
        public IHttpActionResult GetAccountPeriodId(int accountYearId, string date)
        {
            return Content(HttpStatusCode.OK, am.GetAccountPeriodId(accountYearId, base.ActorCompanyId, BuildDateTimeFromString(date, true).Value));
        }

        [HttpPost]
        [Route("AccountPeriod/UpdateStatus/{accountPeriodId:int}/{status:int}")]
        public IHttpActionResult UpdateAccountPeriodStatus(int accountPeriodId, int status)
        {
            return Content(HttpStatusCode.OK, am.UpdateAccountPeriodStatus(accountPeriodId, (TermGroup_AccountStatus)status));
        }

        #endregion

        #region AccountStd

        [HttpGet]
        [Route("AccountStds/{addEmptyRow:bool}")]
        public IHttpActionResult GetAccountStdst(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetAccountStds(base.ActorCompanyId, addEmptyRow).ToDTOs());
        }

        [HttpGet]
        [Route("AccountStd/{addEmptyRow:bool}")]
        public IHttpActionResult GetAccountStdsDict(bool addEmptyRow)
        {
            var dict = am.GetAccountStdsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes();
            dict.Sort((x, y) => x.Name.CompareTo(y.Name));
            return Content(HttpStatusCode.OK, dict);
        }

        [HttpGet]
        [Route("AccountStdNumberName/{addEmptyRow:bool}")]
        public IHttpActionResult GetAccountStdsNameNumber(bool addEmptyRow)
        {
            var dict = am.GetAccountStdsNumberName(base.ActorCompanyId, addEmptyRow);
            dict.Sort((x, y) => x.Name.CompareTo(y.Name));
            return Content(HttpStatusCode.OK, dict);
        }

        #endregion

        #region AccountVatRate

        [HttpGet]
        [Route("AccountVatRate/{addVatFreeRow:bool}")]
        public IHttpActionResult GetAccountVatRates(bool addVatFreeRow)
        {
            return Content(HttpStatusCode.OK, am.GetAccountVatRatesForStdDim(base.ActorCompanyId, addVatFreeRow));
        }

        #endregion

        #region AccountYear

        [HttpGet]
        [Route("AccountYearDict/{addEmptyRow:bool}/{excludeNew:bool}")]
        public IHttpActionResult GetAccountYears(bool addEmptyRow, bool excludeNew)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYearsDict(base.ActorCompanyId, false, excludeNew, false, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("AccountYear/Id/{date}")]
        public IHttpActionResult GetAccountYearId(string date)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYearId(BuildDateTimeFromString(date, true).Value, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AccountYear/{id:int}/{loadPeriods:bool}")]
        public IHttpActionResult GetAccountYearId(int id, bool loadPeriods)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYear(id, loadPeriods, loadPeriods).ToDTO(getPeriods: loadPeriods, doVoucherCheck: loadPeriods));
        }

        [HttpGet]
        [Route("AccountYear/{date}")]
        public IHttpActionResult GetAccountYear(string date)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYear(BuildDateTimeFromString(date, true).Value, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("AccountYear/All/{getPeriods:bool}/{excludeNew:bool}")]
        public IHttpActionResult GetAccountYearId(bool getPeriods, bool excludeNew)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYears(base.ActorCompanyId, false, true, excludeNew).ToDTOs(true, getPeriods));
        }

        [HttpPost]
        [Route("AccountYear/")]
        public IHttpActionResult SaveAccountYear(SaveAccountYearModel model)
        {
            return Content(HttpStatusCode.OK, am.SaveAccountYear(model.AccountYear, model.VoucherSeries, base.ActorCompanyId, model.KeepNumbers));
        }

        [HttpPost]
        [Route("AccountYear/CopyVoucherTemplates/{accountYearId:int}")]
        public IHttpActionResult CopyVoucherTemplatesFromPreviousAccountYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, vm.CopyVoucherTemplatesCheckExisting(base.ActorCompanyId, accountYearId));
        }

        [HttpPost]
        [Route("AccountYear/CopyGrossProfitCodes/{accountYearId:int}")]
        public IHttpActionResult CopyGrossProfitCodes(int accountYearId)
        {
            return Content(HttpStatusCode.OK, gpm.CopyGrossProfitCodesCheckExisting(base.ActorCompanyId, accountYearId));
        }

        [HttpDelete]
        [Route("AccountYear/{accountYearId:int}")]
        public IHttpActionResult DeleteAccountYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAccountYear(accountYearId, base.ActorCompanyId));
        }

        #endregion

        #region Balance

        [HttpGet]
        [Route("Balance/{accountYearId:int}")]
        public IHttpActionResult GetAccountYearBalance(int accountYearId)
        {
            var accountDims = am.GetAccountDimsByCompany(onlyInternal: true, loadInternalAccounts: true);
            return Content(HttpStatusCode.OK, abm.GetAccountYearBalanceHeads(accountYearId, base.ActorCompanyId).ToFlatDTOs(accountDims, false));
        }

        [HttpGet]
        [Route("Balance/Transfer/{accountYearId:int}")]
        public IHttpActionResult GetAccountYearBalanceFromPreviousYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, abm.GetAccountYearBalanceHeadsForPreviousYear(accountYearId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Balance")]
        public IHttpActionResult SaveAccountYearBalances(SaveAccountYearBalanceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, abm.SaveAccountYearBalances(model.items, model.AccountYearId, base.ActorCompanyId));
        }

        #endregion

        #region Budget

        [HttpGet]
        [Route("Budget/{budgetType:int}/{actorCompanyId:int}")]
        public IHttpActionResult GetBudgetList(int budgetType, int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, bm.GetBudgetHeadForGrid(actorCompanyId != 0 ? actorCompanyId : base.ActorCompanyId, budgetType));
        }

        [HttpGet]
        [Route("BudgetHead/{budgetHeadId:int}/{loadRows:bool}")]
        public IHttpActionResult GetBudget(int budgetHeadId, bool loadRows)
        {
            return Content(HttpStatusCode.OK, bm.GetBudgetHeadIncludingRows(budgetHeadId).ToFlattenedDTO());
        }

        [HttpPost]
        [Route("Budget")]
        public IHttpActionResult SaveBudgetHead(BudgetHeadFlattenedDTO dto)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, bm.SaveBudgetHeadFlattened(dto));
        }

        [HttpPost]
        [Route("SalesBudget")]
        public IHttpActionResult SaveSalesBudgetHead(BudgetHeadDTO dto)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, bm.SaveSalesBudgetHead(dto));
        }

        [HttpGet]
        [Route("SalesBudget")]
        public IHttpActionResult GetSalesBudgetHeads()
        {
            return Content(HttpStatusCode.OK, bm.GetSalesBudgetHeadForGrid(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("SalesBudgetV2/{budgetHeadId:int}")]
        public IHttpActionResult GetSalesBudgetV2(int budgetHeadId)
        {
            return Content(HttpStatusCode.OK, bm.GetSalesBudgetHeadIncludingRows(budgetHeadId));
        }

        [HttpPost]
        [Route("SalesBudgetV2")]
        public IHttpActionResult SaveSalesBudgetHeadV2(BudgetHeadSalesDTO dto)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, bm.SaveSalesBudgetHeadV2(dto));
        }

        [HttpGet]
        [Route("SalesBudgetResult/{budgetHeadId:int}/{loadRows:bool}/{interval:int}/{dateFrom}")]
        public IHttpActionResult GetBudgetResult(int budgetHeadId, bool loadRows, int interval, string dateFrom)
        {
            return Content(HttpStatusCode.OK, bm.GetSalesBudgetHeadIncludingRows(budgetHeadId, interval, BuildDateTimeFromString(dateFrom, false).Value));
        }

        [HttpDelete]
        [Route("Budget/{budgetHeadId:int}")]
        public IHttpActionResult DeleteBudget(int budgetHeadId)
        {
            return Content(HttpStatusCode.OK, bm.DeleteBudgetHead(budgetHeadId));
        }

        [HttpPost]
        [Route("Budget/Result")]
        public IHttpActionResult GetBalanceChangePerPeriod(GetResultPerPeriodModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                int actorCompanyId = base.ActorCompanyId;
                Guid guid = Guid.Parse(model.Key);
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                var workingThread = new Thread(() => GetBalanceChangePerPeriodThreading(cultureInfo, guid, model.NoOfPeriods, model.AccountYearId, model.AccountId, actorCompanyId, model.GetPrevious, model.Dims));
                workingThread.Start();
                return Content(HttpStatusCode.OK, new SoeProgressInfo(guid));
            }
        }

        [HttpGet]
        [Route("Budget/Result/{key}")]
        public IHttpActionResult GetBalanceChangeResult(Guid key)
        {
            return Content(HttpStatusCode.OK, bm.ConvertResultToBudgetRow(monitor.GetResult(key) as IEnumerable<BudgetBalanceDTO>));
        }

        private void GetBalanceChangePerPeriodThreading(CultureInfo cultureInfo, Guid key, int noOfPeriods, int accountYearId, int accountId, int actorCompanyId, bool getPrevious, List<int> dims)
        {
            SetLanguage(cultureInfo);

            SoeProgressInfo info = monitor.RegisterNewProgressProcess(key);
            bm.GetBalanceChangePerPeriod(noOfPeriods, accountYearId, accountId, actorCompanyId, DateTime.Today, getPrevious, dims, ref info, monitor);
        }

        #endregion

        #region CompanyGroupAdministration

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupAdministration/")]
        public IHttpActionResult GetCompanyGroupAdministrationList()
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupAdministrationList(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupAdministration/{companyGroupAdministrationId:int}")]
        public IHttpActionResult GetCompanyGroupAdministration(int companyGroupAdministrationId)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupAdministration(base.ActorCompanyId, companyGroupAdministrationId).ToDTO());
        }

        [HttpGet]
        [Route("ConsolidatingAccounting/ChildCompaniesDict/")]
        public IHttpActionResult GetGetChildCompaniesDict()
        {
            return Content(HttpStatusCode.OK, com.GetChildCompaniesByLicenseDict(base.LicenseId, base.ActorCompanyId, true, true));
        }

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupMappingHeadsDict/{addEmptyRow:bool}")]
        public IHttpActionResult GetCompanyGroupMappingHeadsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupMappingHeadsDict(base.ActorCompanyId, addEmptyRow));
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupAdministration")]
        public IHttpActionResult SaveCompanyGroupAdministration(CompanyGroupAdministrationDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, com.SaveCompanyGroupAdministration(base.ActorCompanyId, model, true));
        }

        [HttpDelete]
        [Route("ConsolidatingAccounting/CompanyGroupAdministration/{companyGroupAdministrationId:int}")]
        public IHttpActionResult DeleteCompanyGroupAdministration(int companyGroupAdministrationId)
        {
            return Content(HttpStatusCode.OK, com.DeleteCompanyGroupAdministration(base.ActorCompanyId, companyGroupAdministrationId));
        }

        #endregion

        #region CompanyGroupMappings

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupMappingHeads/")]
        public IHttpActionResult GetCompanyGroupMappingHeads()
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupMappingHeadList(base.ActorCompanyId).ToDTOs(false));
        }

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupMappingHeads/{companyGroupMappingHeadId:int}")]
        public IHttpActionResult GetCompanyGroupMappingHead(int companyGroupMappingHeadId)
        {
            return Content(HttpStatusCode.OK, com.GetCompanyGroupMapping(companyGroupMappingHeadId, true).ToDTO(true));
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupMappingHeads")]
        public IHttpActionResult SaveCompanyGroupMappingHead(CompanyGroupMappingHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, com.SaveCompanyGroupMapping(model, model.Rows, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ConsolidatingAccounting/CompanyGroupMappingHeads/{companyGroupMappingHeadId:int}")]
        public IHttpActionResult DeleteCompanyGroupMappingHead(int companyGroupMappingHeadId)
        {
            return Content(HttpStatusCode.OK, com.DeleteCompanyGroupMapping(companyGroupMappingHeadId));
        }

        #endregion

        #region CompanyGroupTransfer

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupVoucherHistory/{accountYearId:int}/{transferType:int}")]
        public IHttpActionResult GetCompanyGroupVoucherHistory(int accountYearId, int transferType)
        {
            if (transferType == 1)
                return Content(HttpStatusCode.OK, vm.GetCompanyGroupTransferHistoryResult(accountYearId, base.ActorCompanyId, transferType));
            else if (transferType == 2)
                return Content(HttpStatusCode.OK, com.GetCompanyGroupTransferHistoryBudget(accountYearId, base.ActorCompanyId, transferType));
            else
                return Content(HttpStatusCode.OK, com.GetCompanyGroupTransferHistoryBalance(accountYearId, base.ActorCompanyId, transferType));
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupVoucherSerie/{accountYearId:int}")]
        public IHttpActionResult SaveCompanyGroupVoucherSeries(int accountYearId)
        {
            return Content(HttpStatusCode.OK, vm.AddCompanyGroupVoucherSeries(accountYearId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupTransfer/")]
        public IHttpActionResult CompanyGroupTransfer(CompanyGroupTransferModel model)
        {
            var dims = am.GetAccountDimsByCompany(base.ActorCompanyId, false, false).ToDTOs();
            int companyGroupDimId = dims != null ? dims.Where(f => f.AccountDimNr == 1).Select(s => s.AccountDimId).FirstOrDefault() : 0;

            if (model.TransferType == CompanyGroupTransferType.Consolidation)
            {
                return Content(HttpStatusCode.OK, com.TransferCompanyGroupConsolidation(base.ActorCompanyId, base.LicenseId, model.AccountYearId, model.VoucherSeriesId, model.PeriodFrom, model.PeriodTo, model.IncludeIB, companyGroupDimId));
            }
            else if (model.TransferType == CompanyGroupTransferType.Budget)
            {
                return Content(HttpStatusCode.OK, com.TransferCompanyGroupBudget(base.ActorCompanyId, model.AccountYearId, model.BudgetCompanyGroup, model.BudgetCompanyFrom, model.BudgetChild, companyGroupDimId));
            }
            else
            {
                return Content(HttpStatusCode.OK, com.TransferCompanyGroupIncomingBalance(base.ActorCompanyId, model.AccountYearId, companyGroupDimId));
            }
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupTransfer/Delete/{companyGroupTransferHeadId:int}")]
        public IHttpActionResult DeleteCompanyGroupTransfer(int companyGroupTransferHeadId)
        {
            return Content(HttpStatusCode.OK, com.DeleteCompanyGroupTransfer(base.ActorCompanyId, companyGroupTransferHeadId));
        }

        #endregion

        #region CustomerExports

        [HttpPost]
        [Route("CustomerExports")]
        public IHttpActionResult CreateCustomerExport(LiquidityPlanningDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, new ActionResult());
        }

        #endregion

        #region LiquidityPlanning

        [HttpPost]
        [Route("LiquidityPlanning/Get")]
        public IHttpActionResult GetLiquidityPlanning(GetLiquidityPlanningModel model)
        {
            return Content(HttpStatusCode.OK, lpm.GetLiquidityPlanning(base.ActorCompanyId, model.From, model.To, model.Exclusion, model.Balance, model.Unpaid, model.PaidUnchecked, model.PaidChecked));
        }

        [HttpPost]
        [Route("LiquidityPlanning/Get/new")]
        public IHttpActionResult GetLiquidityPlanningv2(GetLiquidityPlanningModel model)
        {
            return Content(HttpStatusCode.OK, lpm.GetLiquidityPlanningv2(base.ActorCompanyId, model.From, model.To, model.Exclusion, model.Balance, model.Unpaid, model.PaidUnchecked, model.PaidChecked));
        }

        [HttpPost]
        [Route("LiquidityPlanning")]
        public IHttpActionResult SaveLiquidityPlanningTransaction(LiquidityPlanningDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, lpm.SaveLiquidityPlanningTransaction(model));
        }

        [HttpDelete]
        [Route("LiquidityPlanning/{liquidityPlanningTransactionId:int}")]
        public IHttpActionResult DeleteLiquidityPlanningTransaction(int liquidityPlanningTransactionId)
        {
            return Content(HttpStatusCode.OK, lpm.DeleteLiquidityPlanningTransaction(liquidityPlanningTransactionId));
        }

        #endregion

        #region MatchCode

        [HttpGet]
        [Route("MatchCodesForGrid/")]
        public IHttpActionResult GetMatchCodesForGrid()
        {
            return Content(HttpStatusCode.OK, im.GetMatchCodesForGrid(base.ActorCompanyId, null));
        }

        [HttpGet]
        [Route("MatchCodes/{matchCodeType:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetMatchCodes(SoeInvoiceMatchingType? matchCodeType, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetMatchCodes(base.ActorCompanyId, matchCodeType, addEmptyRow).ToDTOs());
        }

        [HttpGet]
        [Route("MatchCode/Dict/{type:int}")]
        public IHttpActionResult GetMatchCodesDict(int type)
        {
            SoeInvoiceMatchingType mt = (SoeInvoiceMatchingType)Enum.ToObject(typeof(SoeInvoiceMatchingType), type);
            return Content(HttpStatusCode.OK, im.GetMatchCodesDict(base.ActorCompanyId, mt, true).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("MatchCode/{matchCodeId:int}")]
        public IHttpActionResult GetMatchCode(int matchCodeId)
        {
            return Content(HttpStatusCode.OK, im.GetMatchCode(matchCodeId).ToDTO());
        }

        [HttpPost]
        [Route("MatchCode")]
        public IHttpActionResult SaveMatchCode(MatchCodeDTO matchCodeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveMatchCode(matchCodeDTO));
        }

        [HttpDelete]
        [Route("MatchCode/{matchCodeId:int}")]
        public IHttpActionResult DeleteMatchCode(int matchCodeId)
        {
            return Content(HttpStatusCode.OK, im.DeleteMatchCode(matchCodeId));
        }

        #endregion

        #region DistributionCode

        [HttpGet]
        [Route("DistributionCode")]
        public IHttpActionResult GetDistributionCodes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, bm.GetDistributionCodeDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, bm.GetDistributionCodesForGrid(base.ActorCompanyId));

            return Content(HttpStatusCode.OK, bm.GetDistributionCodes(base.ActorCompanyId, message.GetBoolValueFromQS("includePeriods"), true, (DistributionCodeBudgetType?)message.GetNullableIntValueFromQS("budgetType"), message.GetDateValueFromQS("fromDate"), message.GetDateValueFromQS("toDate")).ToDTOs());
        }

        [HttpGet]
        [Route("DistributionCodesByType/{distributionCodeType:int}/{loadPeriods:bool}")]
        public IHttpActionResult GetDistributionCodesByType(int distributionCodeType, bool loadPeriods)
        {
            return Content(HttpStatusCode.OK, bm.GetDistributionCodesByType(base.ActorCompanyId, distributionCodeType, loadPeriods).ToDTOs());
        }

        [HttpGet]
        [Route("DistributionCode/{distributionCodeHeadId}")]
        public IHttpActionResult GetDistributionCode(int distributionCodeHeadId)
        {
            return Content(HttpStatusCode.OK, bm.GetDistributionCode(base.ActorCompanyId, distributionCodeHeadId).ToDTO(true));
        }

        [HttpPost]
        [Route("DistributionCode")]
        public IHttpActionResult SaveDistributionCode(DistributionCodeHeadDTO model)
        {
            return Content(HttpStatusCode.OK, bm.SaveDistributionCode(base.ActorCompanyId, model));
        }

        [HttpDelete]
        [Route("DistributionCode/{distributionCodeHeadId}")]
        public IHttpActionResult DeleteDistributionCode(int distributionCodeHeadId)
        {
            return Content(HttpStatusCode.OK, bm.DeleteDistributionCode(base.ActorCompanyId, distributionCodeHeadId));
        }

        #endregion DistributionCode

        #region PaymentInformation

        [HttpGet]
        [Route("PaymentInformation/{actorId}/{loadPaymentInformation}/{loadActor}/{includeForeginPayments}")]
        public IHttpActionResult GetPaymentInformationFromActor(int actorId, bool loadPaymentInformation, bool loadActor, bool includeForeginPayments)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationFromActor(actorId, loadPaymentInformation, loadActor).ToDTO(loadPaymentInformation, includeForeginPayments, true));
        }

        [HttpGet]
        [Route("PaymentInformation/{actorId}")]
        public IHttpActionResult GetPaymentInformationViews(int actorId)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationViews(actorId, true));
        }

        #endregion

        #region SysAccountSru

        [HttpGet]
        [Route("SysAccountSruCode/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysAccountSruCodes(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetSysAccountSruCodesDict(addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region SysAccountStd

        [HttpGet]
        [Route("SysAccountStd/{sysAccountStdTypeId:int}/{accountNr}")]
        public IHttpActionResult GetSysAccountStd(int sysAccountStdTypeId, string accountNr)
        {
            if (sysAccountStdTypeId == 0)
                sysAccountStdTypeId = am.GetSysAccountStdTypeParentIdForStandardDim() ?? 0;

            return Content(HttpStatusCode.OK, am.GetSysAccountStd(sysAccountStdTypeId, accountNr, false).ToDTO(false));
        }

        [HttpGet]
        [Route("SysAccountStd/Copy/{sysAccountStdId:int}")]
        public IHttpActionResult CopySysAccountStd(int sysAccountStdId)
        {
            return Content(HttpStatusCode.OK, am.ImportSysAccountStd(base.ActorCompanyId, sysAccountStdId).ToDTO(includeAccountDim: true));
        }

        #endregion

        #region SysAccountStdType

        [HttpGet]
        [Route("SysAccountStdType/")]
        public IHttpActionResult GetSysAccountStdTypes()
        {
            return Content(HttpStatusCode.OK, am.GetSysAccountStdTypeItems());
        }

        #endregion

        #region SysVatAccount

        [HttpGet]
        [Route("SysVatAccount/{sysCountryId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysVatAccounts(int sysCountryId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetSysVatAccountsDict(sysCountryId, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region SysVatRate

        [HttpGet]
        [Route("SysVatRate/{sysVatAccountId:int}")]
        public IHttpActionResult GetSysVatRate(int sysVatAccountId)
        {
            return Content(HttpStatusCode.OK, am.GetSysVatRateValue(sysVatAccountId, false));
        }

        #endregion

        #region VatCode

        [HttpGet]
        [Route("VatCode/")]
        public IHttpActionResult GetVatCodes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, am.GetVatCodes(base.ActorCompanyId).ToGridDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, am.GetVatCodesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, am.GetVatCodes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("VatCode/{vatCodeId:int}")]
        public IHttpActionResult GetVatCode(int vatCodeId)
        {
            var vatCode = am.GetVatCode(vatCodeId);

            if (vatCode != null  && vatCode.ActorCompanyId != base.ActorCompanyId)
                return Content(HttpStatusCode.Forbidden, "Does not belong to the current company.");

            return Content(HttpStatusCode.OK, vatCode.ToDTO());
        }

        [HttpPost]
        [Route("VatCode")]
        public IHttpActionResult SaveVatCode(VatCodeDTO vatCodeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SaveVatCode(vatCodeDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("VatCode/{vatCodeId:int}")]
        public IHttpActionResult DeleteVatCode(int vatCodeId)
        {
            return Content(HttpStatusCode.OK, am.DeleteVatCode(vatCodeId));
        }

        #endregion

        #region Voucher

        [HttpGet]
        [Route("Voucher/BySeries/{accountYearId:int}/{voucherSeriesTypeId:int}")]
        public IHttpActionResult GetVouchersBySeries(int accountYearId, int voucherSeriesTypeId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherHeadsForGrid(accountYearId, base.ActorCompanyId, false, voucherSeriesTypeId));
        }

        [HttpGet]
        [Route("Voucher/Template")]
        public IHttpActionResult GetVoucherTemplates(HttpRequestMessage message)
        {
            var accountYearId = message.GetIntValueFromQS("accountYearId");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, vm.GetVoucherTemplatesByCompanyDict(accountYearId, base.ActorCompanyId).ToSmallGenericTypes());
            else
                return Content(HttpStatusCode.OK, vm.GetVoucherTemplatesByYear(accountYearId, base.ActorCompanyId, false, false).ToGridDTOs());
        }

        [HttpGet]
        [Route("Voucher/{voucherHeadId:int}/{loadVoucherSeries:bool}/{loadVoucherRows:bool}/{loadVoucherRowAccounts:bool}/{loadAccountBalance:bool}")]
        public IHttpActionResult GetVoucher(int voucherHeadId, bool loadVoucherSeries, bool loadVoucherRows, bool loadVoucherRowAccounts, bool loadAccountBalance)
        {
            List<AccountDim> dims = am.GetAccountDimsByCompany(ActorCompanyId);
            return Content(HttpStatusCode.OK, vm.GetVoucherHead(voucherHeadId, loadVoucherSeries, loadVoucherRows, loadVoucherRowAccounts, loadAccountBalance).ToDTO(loadVoucherRows, loadVoucherRowAccounts, dims));
        }

        [HttpPost]
        [Route("Voucher")]
        public IHttpActionResult SaveVoucher(SaveVoucherModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, vm.SaveVoucher(model.VoucherHead, model.AccountingRows, model.HouseholdRowIds, model.Files, model.RevertVatVoucherId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Voucher/SuperSupport/EditVoucherNr/")]
        public IHttpActionResult EditVoucherNrOnlySuperSupport(EditVoucherNrModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, vm.UpdateVoucherNumberOnlySuperSupport(model.VoucherHeadId, model.NewVoucherNr));
        }

        [HttpDelete]
        [Route("Voucher/{voucherHeadId:int}")]
        public IHttpActionResult DeleteVoucher(int voucherHeadId)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVoucher(voucherHeadId));
        }

        [HttpDelete]
        [Route("Voucher/SuperSupport/{voucherHeadId:int}/{checkTransfer:bool}")]
        public IHttpActionResult DeleteVoucherOnlySuperSupport(int voucherHeadId, bool checkTransfer)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVoucherOnlySuperSupport(voucherHeadId, checkTransfer));
        }

        [HttpDelete]
        [Route("Voucher/SuperSupport/Multiple/")]
        public IHttpActionResult DeleteVouchersOnlySuperSupport([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] voucherHeadIds)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVouchersOnlySuperSupport(voucherHeadIds.ToList()));
        }

        #endregion

        #region VoucherHistory

        [HttpGet]
        [Route("Voucher/VoucherRowHistory/{voucherHeadId:int}")]
        public IHttpActionResult GetVoucherRowHistory(int voucherHeadId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherRowHistoryDTO(base.ActorCompanyId, voucherHeadId));
        }

        #endregion

        #region VoucherRow

        [HttpGet]
        [Route("VoucherRow/{voucherHeadId:int}")]
        public IHttpActionResult GetVoucherRows(int voucherHeadId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherRows(voucherHeadId, true).ToDTOs(true));
        }

        #endregion

        #region VoucherSeries

        [HttpGet]
        [Route("VoucherSeries/{accountYearId:int}/{includeTemplate:bool}")]
        public IHttpActionResult GetVoucherSeriesByYear(int accountYearId, bool includeTemplate)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesByYear(accountYearId, base.ActorCompanyId, includeTemplate).ToDTOs());
        }

        [HttpGet]
        [Route("VoucherSeries/{accountYearDate}/{includeTemplate:bool}")]
        public IHttpActionResult GetVoucherSeriesByYear(string accountYearDate, bool includeTemplate)
        {
            var accountYearId = am.GetAccountYearId(BuildDateTimeFromString(accountYearDate, false).Value, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesByYear(accountYearId, base.ActorCompanyId, includeTemplate).ToDTOs());
        }

        //string periodDate
        [HttpGet]
        [Route("VoucherSeries/{accountYearId:int}/{type:int}")]
        public IHttpActionResult GetDefaultVoucherSeriesId(int accountYearId, CompanySettingType type)
        {
            return Content(HttpStatusCode.OK, vm.GetDefaultVoucherSeriesId(accountYearId, type, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("VoucherSeriesByYearRange/{fromAccountYearId:int}/{toAccountYearId:int}")]
        public IHttpActionResult GetVoucherSeriesByYearRange(int fromAccountYearId, int toAccountYearId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesByYearDict(fromAccountYearId, toAccountYearId, base.ActorCompanyId, false, true));
        }

        #endregion

        #region VoucherSeriesType

        [HttpGet]
        [Route("VoucherSeriesType/")]
        public IHttpActionResult GetVoucherSeriesTypes()
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesTypes(base.ActorCompanyId, false).ToDTOs());
        }

        [HttpGet]
        [Route("VoucherSeriesType/ByCompany/{actorCompanyId:int}")]
        public IHttpActionResult GetVoucherSeriesTypesByCompany(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesTypesDict(actorCompanyId, false, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("VoucherSeriesType/{voucherSeriesTypeId:int}")]
        public IHttpActionResult GetVoucherSeriesType(int voucherSeriesTypeId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesType(voucherSeriesTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("VoucherSeriesType")]
        public IHttpActionResult SaveVoucherSeriesType(VoucherSeriesTypeDTO voucherSeriesTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, vm.SaveVoucherSeriesType(voucherSeriesTypeDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("VoucherSeriesType/{voucherSeriesTypeId:int}")]
        public IHttpActionResult DeleteVoucherSeriesType(int voucherSeriesTypeId)
        {
            return Content(HttpStatusCode.OK, vm.DeleteVoucherSeriesType(voucherSeriesTypeId, base.ActorCompanyId));
        }

        #endregion

        #region VoucherSearch
        [HttpPost]
        [Route("VoucherSearch/")]
        public IHttpActionResult getSearchedVoucherRows(SearchVoucherRowsAngDTO dto)
        {
            return Content(HttpStatusCode.OK, vm.SearchVoucherRowsDto(base.ActorCompanyId, dto));
        }

        [HttpGet]
        [Route("VoucherSearch/Transactions/{accountId:int}/{accountYearId:int}/{accountPeriodIdFrom:int}/{accountPeriodIdTo:int}")]
        public IHttpActionResult getVoucherTransactions(int accountId, int accountYearId, int accountPeriodIdFrom, int accountPeriodIdTo)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherTransactions(accountId, accountYearId, accountPeriodIdFrom, accountPeriodIdTo));
        }
        #endregion

        #region VoucherRowMergeType
        [HttpGet]
        [Route("VoucherRowMergeType/")]
        public IHttpActionResult GetVoucherRowMergeType()
        {
            return Content(HttpStatusCode.OK, base.GetTermGroupContent(TermGroup.VoucherRowMergeType).ToSmallGenericTypes());
        }

        #endregion

        #region VatVerification        

        [HttpGet]
        [Route("VatVerification/VatVerifyVoucherRows/{fromDate}/{toDate}/{excludeDiffAmountLimit:decimal}")]
        public IHttpActionResult GetVatVerifyVoucherRows(string fromDate, string toDate, decimal excludeDiffAmountLimit)
        {
            return Content(HttpStatusCode.OK, vm.GetVatVerifyVoucherRows(base.ActorCompanyId, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true), excludeDiffAmountLimit));
        }

        #endregion

        #region GrossProfitCode

        [HttpGet]
        [Route("GrossProfitCode/")]
        public IHttpActionResult GetGrossProfitCodes()
        {
            return Content(HttpStatusCode.OK, gpm.GetGrossProfitCodes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("GrossProfitCode/ByYear/{accountYearId:int}")]
        public IHttpActionResult GetGrossProfitCodesByYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, gpm.GetGrossProfitCodes(base.ActorCompanyId, accountYearId).ToDTOs());
        }

        [HttpGet]
        //[Route("GrossProfitCode/{grossProfitCodeId:int}")]
        [Route("GrossProfitCode/{grossProfitCodeId}")]
        public IHttpActionResult GetGrossProfitCode(int grossProfitCodeId)
        {
            return Content(HttpStatusCode.OK, gpm.GetGrossProfitCode(base.ActorCompanyId, grossProfitCodeId).ToDTO());
        }

        [HttpPost]
        [Route("GrossProfitCode")]
        public IHttpActionResult SaveGrossProfitCode(GrossProfitCodeDTO grossProfitCodeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gpm.SaveGrossProfitCode(base.ActorCompanyId, grossProfitCodeDTO));
        }

        [HttpDelete]
        [Route("GrossProfitCode/{grossProfitCodeId:int}")]
        public IHttpActionResult DeleteGrossProfitCode(int grossProfitCodeId)
        {
            return Content(HttpStatusCode.OK, gpm.DeleteGrossProfitCode(base.ActorCompanyId, grossProfitCodeId));
        }

        #endregion

        #region Reconciliation

        [HttpGet]
        [Route("ReconciliationRows/{dim1Id:int}/{fromDim1}/{toDim1}/{fromDate}/{toDate}/")]
        public IHttpActionResult GetReconciliationRows(int dim1Id, string fromDim1, string toDim1, string fromDate, string toDate)
        {
            return Content(HttpStatusCode.OK, vm.GetReconciliationRows(base.ActorCompanyId, dim1Id, fromDim1, toDim1, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true)));
        }

        [HttpGet]
        [Route("ReconciliationPerAccount/{accountId:int}/{fromDate}/{toDate}/")]
        public IHttpActionResult GetReconciliationPerAccount(int accountId, string fromDate, string toDate)
        {
            return Content(HttpStatusCode.OK, vm.GetReconciliationPerAccount(base.ActorCompanyId, accountId, BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true)));
        }

        #endregion

        #region Paymentconditions
        [HttpGet]
        [Route("PaymentConditions/")]
        public IHttpActionResult GetPaymentConditions()
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentConditions(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("PaymentCondition/{paymentConditionId:int}")]
        public IHttpActionResult GetPaymentCondition(int paymentConditionId)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentCondition(paymentConditionId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("PaymentCondition")]
        public IHttpActionResult SavePaymentCondition(PaymentConditionDTO paymentConditionDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePaymentCondition(paymentConditionDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PaymentCondition/{paymentConditionId:int}")]
        public IHttpActionResult DeletePaymentCondition(int paymentConditionId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePaymentCondition(paymentConditionId, base.ActorCompanyId));
        }
        #endregion

        #region Paymentmethods

        [HttpGet]
        [Route("PaymentMethods/{originTypeId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetPaymentMethodsDict(int originTypeId, bool addEmptyRow)
        {
            return Content(
                HttpStatusCode.OK, 
                pm.GetPaymentMethodsDict(
                    [originTypeId], 
                    base.ActorCompanyId, 
                    addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("PaymentMethods/ForImport/{originTypeId:int}")]
        public IHttpActionResult GetPaymentMethodsForImport(int originTypeId)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentMethodsForImport(originTypeId, base.ActorCompanyId).ToDTOs(false));
        }

        [HttpGet]
        [Route("PaymentTypes/")]
        public IHttpActionResult GetSysPaymentTypes()
        {
            return Content(HttpStatusCode.OK, iem.GetSysPaymentTypeDict().ToSmallGenericTypes());
        }

        #endregion

        #region Report

        [HttpGet]
        [Route("InvoiceJournalReportId/{reportType:int}")]
        public IHttpActionResult GetInvoiceJournalReportId(int reportType)
        {
            var reports = rm.GetReportsByTemplateTypeDict(base.ActorCompanyId, (SoeReportTemplateType)reportType);
            return Content(HttpStatusCode.OK, reports != null && reports.Count > 0 ? reports.First().Key : 0);
        }

        #endregion

        #region ExportFiles
        [HttpGet]
        [Route("PaymentExports/{paymentTypeId:int}/{allItemsSelection:int}")]
        public IHttpActionResult GetPaymentExports(int paymentTypeId, int allItemsSelection)
        {
            List<AccountDim> dims = am.GetAccountDimsByCompany(base.ActorCompanyId);
            return Content(HttpStatusCode.OK, pm.GetPaymentExports(base.ActorCompanyId, paymentTypeId, (TermGroup_ChangeStatusGridAllItemsSelection)allItemsSelection).ToDTOs(true, dims));
        }

        [HttpGet]
        [Route("InvoiceExports/{recordTypeId:int}")]
        public IHttpActionResult GetInvoiceExports(int recordTypeId)
        {
            return Content(HttpStatusCode.OK, gm.GetDataStorages(base.ActorCompanyId, (SoeDataStorageRecordType)recordTypeId).ToDTOs());
        }

        [HttpGet]
        [Route("PaymentServiceRecords/")]
        public IHttpActionResult GetPaymentServiceRecords()
        {
            return Content(HttpStatusCode.OK, iem.GetInvoiceExports(base.ActorCompanyId).ToDTOs());
        }

        [HttpPost]
        [Route("CancelPaymentExport/{paymentExportId:int}")]
        public IHttpActionResult CancelPaymentExport(int paymentExportId)
        {
            return Content(HttpStatusCode.OK, pm.CancelPaymentFromPaymentExport(paymentExportId, base.ActorCompanyId));
        }
        [HttpGet]
        [Route("PaymentService/Invoices/{paymentService:int}")]
        public IHttpActionResult GetInvoicesForPaymentService(int paymentService)
        {
            return Ok(im.GetInvoicesForPaymentService(ActorCompanyId, paymentService));
        }

        [HttpGet]
        [Route("PaymentService/ExportedIOInvoices/{invoiceExportId:int}")]
        public IHttpActionResult GetExportedIOInvoices(int invoiceExportId)
        {
            return Ok(iem.GetExportedIOInvoices(base.ActorCompanyId, invoiceExportId).ToDTOs());
        }

        [HttpPost]
        [Route("PaymentService/Invoices/{paymentService:int}")]
        public IHttpActionResult SaveCustomerInvoicePaymentService(List<InvoiceExportIODTO> items, int paymentService)
        {
            return Ok(im.SaveCustomerInvoicePaymentService(items, ActorCompanyId, UserId, paymentService));
        }

        [HttpPost]
        [Route("UndoDataStorage/")]
        public IHttpActionResult UndoDataStorage(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.UndoDataStorage(base.ActorCompanyId, dataStorageId));
        }


        #endregion

        #region ImportFiles

        [HttpGet]
        [Route("PaymentImports/{importType:int}/{allItemsSelection:int}")]
        public IHttpActionResult GetPaymentImports(int importType, int allItemsSelection)
        {
            return Content(HttpStatusCode.OK, iem.GetPaymentImports(
                base.ActorCompanyId, 
                [importType], 
                (TermGroup_ChangeStatusGridAllItemsSelection)allItemsSelection));
        }

        [HttpPost]
        [Route("PaymentImportHeader/")]
        public IHttpActionResult SavePaymentImportHeader(PaymentImportDTO model)
        {
            return Content(HttpStatusCode.OK, pm.SavePaymentImportHeader(base.ActorCompanyId, model));
        }

        [HttpPost]
        [Route("PaymentImport/")]
        public IHttpActionResult StartPaymentImport(PaymentImportRowsDto model)
        {
            return Content(HttpStatusCode.OK, pm.StartPaymentImport(model.PaymentIOType, model.PaymentMethodId, model.Contents, model.FileName, ActorCompanyId, base.UserId, model.BatchId, model.PaymentImportId, model.ImportType));
        }

        [HttpGet]
        [Route("ImportedIoInvoices/{batchId:int}/{importType:int}")]
        public IHttpActionResult GetImportedIoInvoices(int batchId, ImportPaymentType importType)
        {
            return Ok(iem.GetImportedIOInvoices(ActorCompanyId, batchId, importType, true));
        }

        [HttpGet]
        [Route("PaymentImport/{importId:int}")]
        public IHttpActionResult GetPaymentImport(int importId)
        {
            return Content(HttpStatusCode.OK, iem.GetPaymentImport(base.ActorCompanyId, importId).ToDTO());
        }

        [HttpPost]
        [Route("PaymentImportIO/")]
        public IHttpActionResult UpdatePaymentImportIO(PaymentImportIODTO model)
        {
            return Content(HttpStatusCode.OK, im.UpdatePaymentImport(model));
        }

        [HttpPost]
        [Route("PaymentImportIOs/")]
        public IHttpActionResult UpdatePaymentImportIODTOS(List<PaymentImportIODTO> model)
        {
            return Content(HttpStatusCode.OK, pm.SavePaymentImportIOs(model));
        }

        [HttpPost]
        [Route("PaymentImportIODTOsUpdate/")]
        public IHttpActionResult UpdatePaymentImportIODTOS(SavePaymentImportIODTOModel savePaymentItems)
        {
            return Content(HttpStatusCode.OK, pm.SaveImportPaymentFromSupplierInvoice(savePaymentItems.items, savePaymentItems.bulkPayDate, SoeOriginType.SupplierPayment, false, savePaymentItems.accountYearId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("CustomerPaymentImportIODTOsUpdate/")]
        public IHttpActionResult UpdateCustomerPaymentImportIODTOS(SaveCustomerPaymentImportIODTOModel savePaymentItems)
        {
            return Content(HttpStatusCode.OK, pm.SaveImportPaymentFromCustomerInvoice(savePaymentItems.items, savePaymentItems.bulkPayDate, savePaymentItems.paymentMethodId, savePaymentItems.accountYearId, base.ActorCompanyId, false, true));
        }

        [HttpPost]
        [Route("PaymentImportIODTOsUpdateStatus/")]
        public IHttpActionResult UpdatePaymentImportIODTOSStatus(SavePaymentImportIODTOModel savePaymentItems)
        {
            return Content(HttpStatusCode.OK, pm.UpdatePaymentImportIOStatus(savePaymentItems.items));
        }

        [HttpPost]
        [Route("PaymentFileImport")]
        public async Task<IHttpActionResult> PaymentImport()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                {
                    ActionResult result = new ActionResult();
                    try
                    {
                        var contents = FileUtil.ConvertToStream(file.File, false);
                        result.Value = contents;
                        result.Value2 = file.Filename;
                    }
                    catch (Exception exception)
                    {
                        result.Success = false;
                        result.Exception = exception;
                    }

                    return Content(HttpStatusCode.OK, result);
                }
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("FinvoiceImport/")]
        public async Task<IHttpActionResult> ImportFinvoiceFiles(List<int> dataStorageIds)
        {
            var result = await iem.ImportFinvoiceFiles(dataStorageIds, base.ActorCompanyId).ConfigureAwait(false);
            return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("FinvoiceImport/Attachments/")]
        public async Task<IHttpActionResult> ImportFinvoiceAttachments()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var edi = new EdiManager(ParameterObject);
                var data = await Request.Content.ParseMultipartAsync();

                HttpPostedFile file = data.Files["file"];
                if (file != null)
                {
                    return Content(HttpStatusCode.OK, edi.AddFinvoiceAttachment(file.Filename, base.ActorCompanyId, new MemoryStream(file.File)));
                }
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpDelete]
        [Route("ImportedIoInvoices/{batchId:int}/{importType:int}")]
        public IHttpActionResult DeletePaymentImportHeader(int batchId, int importType)
        {
            return Content(HttpStatusCode.OK, iem.DeleteImportedIOInvoices(base.ActorCompanyId, batchId, (ImportPaymentType)importType));
        }

        [HttpDelete]
        [Route("PaymentImportIO/{paymentImportIOId:int}")]
        public IHttpActionResult DeletePaymentImportIO(int paymentImportIOId)
        {
            return Content(HttpStatusCode.OK, iem.DeletePaymentImportRow(base.ActorCompanyId, paymentImportIOId));
        }

        #endregion
    }
}