using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/Accounting")]
    public class AccountDistributionEntryController : SoeApiController
    {
        #region Variables

        private readonly AccountDistributionManager adm;
        private readonly InventoryManager im;

        #endregion

        #region Constructor

        public AccountDistributionEntryController(AccountDistributionManager adm, InventoryManager im)
        {
            this.adm = adm;
            this.im = im;
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
                return Content(HttpStatusCode.OK, im.TransferToAccountDistributionEntry(ActorCompanyId, model.PeriodDate));
        }

        [HttpPost]
        [Route("AccountDistributionEntries/TransferAccountDistributionEntryToVoucher")]
        public IHttpActionResult TransferAccountDistributionEntryToVoucher(TransferAccountDistributionEntryToVoucherModel model)
        {
            return Content(HttpStatusCode.OK, adm.TransferAccountDistributionEntryDTOsToVoucher(model.AccountDistributionEntryDTOs, base.ActorCompanyId, (SoeAccountDistributionType)model.AccountDistributionType));
        }

        
        [HttpPost]
        [Route("AccountDistributionEntries/Reverse")]
        public IHttpActionResult ReverseAccountDistributionEntries(ReverseAccountDistributionEntryModel model)
        {
            return Content(HttpStatusCode.OK, adm.ReverseInventoryAccountDistributionEntries(base.ActorCompanyId, (SoeAccountDistributionType)model.AccountDistributionType, model.AccountDistributionEntryDTOs));
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
    }
}