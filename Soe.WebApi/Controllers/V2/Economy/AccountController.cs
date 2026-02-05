using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Util;
using Soe.WebApi.Binders;
using Soe.WebApi.Models;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Account")]
    public class AccountController : SoeApiController
    {
        #region Variables

        private readonly AccountManager am;
        private readonly SettingManager sm;
        private readonly VoucherManager vm;

        #endregion

        #region Constructor

        public AccountController(AccountManager am, SettingManager sm, VoucherManager vm)
        {
            this.am = am;
            this.sm = sm;
            this.vm = vm;
        }

        #endregion

        #region AccountStd

        [HttpGet]
        [Route("GetStdAccounts")]
        public IHttpActionResult GetStdAccounts()
        {
            List<Account> accountStds = new List<Account>();
            AccountDim accountDimStd = am.GetAccountDimStd(base.ActorCompanyId);
            if (accountDimStd != null)
                accountStds = am.GetAccountsByDim(accountDimStd.AccountDimId, base.ActorCompanyId, null, true, true).ToList();
            return Content(HttpStatusCode.OK, accountStds.ToDTOs());
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
        [Route("AccountStdNumberName/{addEmptyRow:bool}/{accountTypeId:int?}")]
        public IHttpActionResult GetAccountStdsNameNumber(bool addEmptyRow, int? accountTypeId = null)
        {
            var dict = am.GetAccountStdsNumberName(base.ActorCompanyId, addEmptyRow, accountTypeId);
            dict.Sort((x, y) => x.Name.CompareTo(y.Name));
            return Content(HttpStatusCode.OK, dict);
        }

        #endregion

        #region Hierarchy accounts

        [HttpGet]
        [Route("AccountsFromHierarchyByUserSetting/{dateFrom}/{dateTo}/{useMaxAccountDimId:bool}/{includeVirtualParented:bool}/{includeOnlyChildrenOneLevel:bool}/{useDefaultEmployeeAccountDimEmployee:bool}")]
        public IHttpActionResult GetAccountsFromHierarchyByUserSetting(string dateFrom, string dateTo, bool useMaxAccountDimId, bool includeVirtualParented, bool includeOnlyChildrenOneLevel, bool useDefaultEmployeeAccountDimEmployee)
        {
            AccountHierarchyInput input = AccountHierarchyInput.GetInstance();
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimEmployee, true); //Always true for performance issue
            input.AddParamValue(AccountHierarchyParamType.UseDefaultEmployeeAccountDimSelector, useMaxAccountDimId);
            input.AddParamValue(AccountHierarchyParamType.IncludeVirtualParented, includeVirtualParented);
            input.AddParamValue(AccountHierarchyParamType.IncludeOnlyChildrenOneLevel, includeOnlyChildrenOneLevel);

            List<AccountDTO> accounts = am.GetAccountsFromHierarchyByUserSetting(base.ActorCompanyId, base.RoleId, base.UserId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), doFilterByDefaultEmployeeAccountDimEmployee: useDefaultEmployeeAccountDimEmployee, input: input);
            return Content(HttpStatusCode.OK, accounts);
        }

        #endregion

        #region AccountsInternals

        [HttpGet]
        [Route("AccountsInternals/{loadAccount:bool}/{loadAccountDim:bool}/{loadAccountMapping:bool}")]
        public IHttpActionResult GetAccountsInternalsByCompany(bool loadAccount, bool loadAccountDim, bool loadAccountMapping)
        {
            return Content(HttpStatusCode.OK, am.GetAccountsInternalsByCompany(base.ActorCompanyId, loadAccount, loadAccountDim, loadAccountMapping).ToDTOs());
        }

        #endregion

        #region AccountDim

        [HttpGet]
        [Route("GetAccountDimInternals/{active:bool?}")]
        public IHttpActionResult GetAccountDimInternals(bool? active = true)
        {
            return Content(HttpStatusCode.OK, am.GetAccountDimInternalsByCompany(base.ActorCompanyId, active).ToDTOs());
        }

        [HttpGet]
        [Route("AccountDim/Grid/{onlyStandard:bool}/{onlyInternal:bool}/{accountDimId:int?}")]
        public IHttpActionResult GetAccountDimGrid(bool onlyStandard, bool onlyInternal, int? accountDimId = null)
        {
            return Content(HttpStatusCode.OK, am.GetAccountDims(base.ActorCompanyId, onlyStandard, onlyInternal, null, accountDimId).ToGridDTOs());
        }


        [HttpGet]
        [Route("AccountDim/{accountDimId:int}/{loadInactiveDims:bool}")]
        public IHttpActionResult AccountDimByAccountDimId(int accountDimId, bool loadInactiveDims)
        {

            bool useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, base.ActorCompanyId, 0);


            var dim = am.GetAccountDim(accountDimId, base.ActorCompanyId, loadInactiveDims);

            if (dim != null)
            {
                var dto = dim.ToDTO();
                //if (useAccountHierarchy)
                //    am.FilterAccountsOnAccountDims(new List<AccountDimDTO>() { dto }, base.ActorCompanyId, base.UserId);
                return Content(HttpStatusCode.OK, dto);

            }
            return Content(HttpStatusCode.OK, new AccountDimDTO());
        }

        [HttpGet]
        [Route("AccountDimByAccountDimIdSmall/{accountDimId:int}/{onlyStandard:bool}/{onlyInternal:bool}/{loadAccounts:bool}/{loadInternalAccounts:bool}/{loadParent:bool}/{loadInactives:bool}/{loadInactiveDims:bool}/{includeParentAccounts:bool}")]
        public IHttpActionResult AccountDimByAccountDimIdSmall(int accountDimId, bool onlyStandard, bool onlyInternal, bool loadAccounts, bool loadInternalAccounts, bool loadParent, bool loadInactives, bool loadInactiveDims, bool includeParentAccounts)
        {

            var dim = am.GetAccountDim(accountDimId, base.ActorCompanyId, loadInactiveDims);
            bool useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, base.ActorCompanyId, 0);
            if (dim != null)
            {

                var dto = dim.ToSmallDTO(loadAccounts, loadInternalAccounts, loadInactives);
                if (useAccountHierarchy)
                    am.FilterAccountsOnAccountDims(dto.ObjToList(), base.ActorCompanyId, base.UserId);
                return Content(HttpStatusCode.OK, dto);
            }
            return Content(HttpStatusCode.OK, new List<AccountDimSmallDTO>());
        }

        [HttpGet]
        [Route("AccountDim/{onlyStandard:bool}/{onlyInternal:bool}/{loadAccounts:bool}/{loadInternalAccounts:bool}/{loadParent:bool}/{loadInactives:bool}/{loadInactiveDims:bool}/{includeParentAccounts:bool}")]
        public IHttpActionResult AccountDim(bool onlyStandard, bool onlyInternal, bool loadAccounts, bool loadInternalAccounts, bool loadParent, bool loadInactives, bool loadInactiveDims, bool includeParentAccounts)
        {
            bool useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, base.ActorCompanyId, 0);
            List<AccountDim> dims = am.GetAccountDimsByCompany(base.ActorCompanyId, onlyStandard, onlyInternal, loadInactiveDims ? (bool?)null : true, loadAccounts, loadInternalAccounts, loadParent || includeParentAccounts);

            var dtos = dims.ToDTOs(loadAccounts, loadInternalAccounts).ToList();
            if (useAccountHierarchy)
                am.FilterAccountsOnAccountDims(dtos, base.ActorCompanyId, base.UserId);
            return Content(HttpStatusCode.OK, dtos);
        }

        [HttpGet]
        [Route("GetAccountDimsSmall/{onlyStandard:bool}/{onlyInternal:bool}/{loadAccounts:bool}/{loadInternalAccounts:bool}/{loadParent:bool}/{loadInactives:bool}/{loadInactiveDims:bool}/{includeParentAccounts:bool}/{ignoreHierarchyOnly:bool}/{actorCompanyId:int}/{includeOrphanAccounts:bool}")]
        public IHttpActionResult GetAccountDimsSmall(bool onlyStandard, bool onlyInternal, bool loadAccounts, bool loadInternalAccounts, bool loadParent, bool loadInactives, bool loadInactiveDims, bool includeParentAccounts, bool ignoreHierarchyOnly, int actorCompanyId, bool includeOrphanAccounts)
        {
            if (ignoreHierarchyOnly && !loadAccounts)
                loadAccounts = true;

            if (actorCompanyId == 0)
                actorCompanyId = base.ActorCompanyId;

            bool useAccountHierarchy = sm.GetCompanyBoolSetting(CompanySettingType.UseAccountHierarchy);

            var dims = am.GetAccountDimsByCompany(actorCompanyId,
                    onlyStandard: onlyStandard,
                    onlyInternal: onlyInternal,
                    active: loadInactiveDims ? (bool?)null : true,
                    loadAccounts: loadAccounts,
                    loadInternalAccounts: loadInternalAccounts,
                    loadParentOrCalculateLevels: useAccountHierarchy || loadParent || includeParentAccounts
                    )
                .ToSmallDTOs(loadAccounts, loadInternalAccounts, loadInactives)
                .ToList();

            if (useAccountHierarchy)
                am.FilterAccountsOnAccountDims(dims, actorCompanyId, base.UserId, ignoreHierarchyOnly: ignoreHierarchyOnly, includeParentAccounts: includeParentAccounts, includeOrphanAccounts: includeOrphanAccounts);

            return Content(HttpStatusCode.OK, dims);
        }

        [HttpGet]
        [Route("AccountDim/ShiftType/{loadAccounts:bool}/{useCache:bool?}")]
        public IHttpActionResult GetShiftTypeAccountDim(bool loadAccounts, bool useCache = true)
        {
            return Content(HttpStatusCode.OK, am.GetShiftTypeAccountDimDTO(base.ActorCompanyId, loadAccounts, useCache));
        }

        [HttpGet]
        [Route("AccountDim/Chars")]
        public IHttpActionResult GetAccountDimChars()
        {
            return Content(HttpStatusCode.OK, am.GetAccountDimChars());
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

        [HttpGet]
        [Route("AccountDim/Project")]
        public IHttpActionResult GetProjectAccountDim()
        {
            return Content(HttpStatusCode.OK, am.GetProjectAccountDim(base.ActorCompanyId).ToDTO());
        }


        #endregion

        #region AccountYear

        [HttpGet]
        [Route("AccountYearDict/{addEmptyRow:bool}/{excludeNew:bool}")]
        public IHttpActionResult GetAccountYears(bool addEmptyRow, bool excludeNew)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYearsDict(base.ActorCompanyId, false, excludeNew, false, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region Account

        [HttpGet]
        [Route("Account/ById/{accountId:int}")]
        public IHttpActionResult GetAccount(int accountId)
        {
            return Content(HttpStatusCode.OK, am.GetAccount(base.ActorCompanyId, accountId, onlyActive: false, loadAccount: true, loadAccountDim: true, loadAccountMapping: true, loadAccountSru: true, loadCompanyExternalCodes: true).ToEditDTO(true));
        }

        [HttpGet]
        [Route("GetAccountsGrid/{accountDimId:int}/{accountYearId:int}/{setLinkedToShiftType:bool}/{getCategories:bool}/{setParent:bool}/{ignoreHierarchyOnly:bool}/{accountId:int?}")]
        public IHttpActionResult GetAccountsGrid(int accountDimId, int accountYearId, bool setLinkedToShiftType, bool getCategories, bool setParent, bool ignoreHierarchyOnly = false, int? accountId = null)
        {
            if (accountDimId == 0)
                accountDimId = am.GetAccountDimStdId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, am.GetAccounts(base.ActorCompanyId, accountDimId, accountYearId, false, false, true, false, true, setLinkedToShiftType, getCategories, setParent, ignoreHierarchyOnly, accountId).ToGridDTOs(getCategories, setParent));
        }

        [HttpGet]
        [Route("AccountDict/{accountDimId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetAccountsDict(int accountDimId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetAccountsDict(base.ActorCompanyId, accountDimId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("GetAccountsSmall/{accountDimId:int}/{accountYearId:int}")]
        public IHttpActionResult GetAccountsSmall(int accountDimId, int accountYearId)
        {
            if (accountDimId == 0)
                accountDimId = am.GetAccountDimStdId(base.ActorCompanyId);
            return Content(HttpStatusCode.OK, am.GetAccounts(base.ActorCompanyId, accountDimId, accountYearId).ToSmallDTOs());
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
        [HttpGet]
        [Route("AccountChildren/{parentAccountId:int}")]
        public IHttpActionResult GetChildrenAccounts(int parentAccountId)
        {
            return Content(HttpStatusCode.OK, am.GetChildrenAccounts(base.ActorCompanyId, parentAccountId).ToDTOs());
        }

        [HttpPost]
        [Route("Account/UpdateState")]
        public IHttpActionResult UpdateAccountsState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.UpdateAccountsState(model.Dict, model.SkipStateValidation ?? false));
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

        [HttpGet]
        [Route("Account/Validate/{accountNr}/{accountId:int}/{accountDimId:int}")]
        public IHttpActionResult ValidateAccount(string accountNr, int accountId, int accountDimId)
        {
            return Content(HttpStatusCode.OK, am.ValidateAccount(accountNr, accountId, accountDimId, base.ActorCompanyId));
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

        #region SysAccountSru

        [HttpGet]
        [Route("SysAccountSruCode/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysAccountSruCodes(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetSysAccountSruCodesDict(addEmptyRow).ToSmallGenericTypes());
        }

        #endregion
    }
}