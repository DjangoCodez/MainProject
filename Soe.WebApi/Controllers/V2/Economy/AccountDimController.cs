using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System;
using Soe.WebApi.Binders;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Common.DTO;
using System.Linq;
using System.Net.Http;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Accounting")]
    public class AccountDimController : SoeApiController
    {
        #region Variables

        private readonly AccountManager am;
        private readonly SettingManager sm;


        #endregion

        #region Constructor

        public AccountDimController(AccountManager am, SettingManager sm)
        {
            this.am = am;
            this.sm = sm;
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
    }
}