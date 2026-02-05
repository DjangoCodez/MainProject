using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Accounting")]
    public class AccountDistributionController : SoeApiController
    {
        #region Variables

        private readonly AccountDistributionManager adm;
        private readonly AccountManager am;

        #endregion

        #region Constructor

        public AccountDistributionController(AccountDistributionManager adm, AccountManager am)
        {
            this.adm = adm;
            this.am = am;
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
        [Route("AccountDistribution/{loadOpen:bool}/{loadClosed:bool}/{loadEntries:bool}/{accountDistributionHeadId:int?}")]
        public IHttpActionResult GetAccountDistributionHeads(bool loadOpen = true, bool loadClosed = true, bool loadEntries = true, int? accountDistributionHeadId = null)
        {
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionHeads(base.ActorCompanyId, SoeAccountDistributionType.Period, false, loadOpen, loadClosed, loadEntries, true, accountDistributionHeadId).ToSmallDTOs(true));
        }

        [HttpGet]
        [Route("AccountDistribution/{accountDistributionHeadId:int}")]
        public IHttpActionResult GetAccountDistributionHead(int accountDistributionHeadId)
        {
            List<AccountDim> accountDimInternals = am.GetAccountDimsByCompany(base.ActorCompanyId);
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionHead(accountDistributionHeadId).ToDTO(true, true, accountDimInternals));
        }

        [HttpGet]
        [Route("AccountDistributionAuto/{accountDistributionHeadId:int?}")]
        public IHttpActionResult GetAccountDistributionHeadsAuto(int? accountDistributionHeadId = null)
        {
            return Content(HttpStatusCode.OK, adm.GetAccountDistributionHeads(base.ActorCompanyId, SoeAccountDistributionType.Auto, false, loadAccount: true, accountDistributionHeadId: accountDistributionHeadId).ToSmallDTOs(true));
        }

        [HttpGet]
        [Route("AccountDistribution/GetAccountDistributionTraceViews/{accountDistributionHeadId:int}")]
        public IHttpActionResult GetAccountDistributionTraceViews(int accountDistributionHeadId)
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

    }
}