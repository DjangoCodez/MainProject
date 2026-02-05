using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.TimeStampServices
{

    [RoutePrefix("TimeStampServices/WebTimeStamp")]
    public class WebTimeStampController : ApiBase
    {
        private WebTimeStampManager wts;
        private TimeStampManager tsm;


        public WebTimeStampController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            this.wts = new WebTimeStampManager(null);
            this.tsm = new TimeStampManager(null);
        }

        [HttpGet]
        [Route("SyncNewTimeStampEntries")]
        [ResponseType(typeof(ActionResult))]
        public ActionResult SyncNewTimeStampEntries(int actorCompanyId, int terminalId, TimeStampEntryType timeStampEntryType, string employeeNr, int? causeId = null, int? accountId = null, bool isBreak = false)
        {
            return wts.SyncNewTimeStampEntries(actorCompanyId, terminalId, timeStampEntryType, employeeNr, causeId, accountId, isBreak);
        }

        [HttpGet]
        [Route("GetStampedInEmployees")]
        [ResponseType(typeof(List<EmployeeModel>))]
        public List<EmployeeModel> GetStampedInEmployees(int actorCompanyId, int timeTerminalId)
        {
            using (SysScheduledJobManager sysScheduledJobManager = new SysScheduledJobManager(null))
            {
                sysScheduledJobManager.CheckJobs();
            }
            return wts.GetStampedInEmployees(actorCompanyId, timeTerminalId).ToList();
        }

        [HttpGet]
        [Route("GetDefaultGroupName")]
        [ResponseType(typeof(string))]
        public string GetDefaultGroupName(int actorCompanyId, int timeTerminalId)
        {
            return wts.GetDefaultGroupName(actorCompanyId, timeTerminalId);
        }

        [HttpGet]
        [Route("GetImageUrl")]
        [ResponseType(typeof(string))]

        public string GetImageUrl(int actorCompanyId, int employeedId)
        {
            return wts.GetImageUrl(actorCompanyId, employeedId);
        }

        [HttpGet]
        [Route("GetTerminalCategories")]
        [ResponseType(typeof(List<int>))]
        public List<int> GetTerminalCategories(int actorCompanyId, int timeTerminalId)
        {
            return wts.GetTerminalCategories(actorCompanyId, timeTerminalId);
        }

        [HttpGet]
        [Route("GetAccountDimName")]
        [ResponseType(typeof(string))]
        public string GetAccountDimName(int actorCompanyId, int timeTerminalId)
        {
            return wts.GetAccountDimName(actorCompanyId, timeTerminalId);
        }

        [HttpGet]
        [Route("GetTerminalSetting")]
        [ResponseType(typeof(TimeTerminalSettingDTO))]
        public TimeTerminalSettingDTO GetTerminalSetting(TimeTerminalSettingType settingType, int terminalId)
        {
            return wts.GetTerminalSetting(settingType, terminalId).ToDTO();
        }

        [HttpGet]
        [Route("GetTimeTerminal")]
        [ResponseType(typeof(TimeTerminalDTO))]

        public TimeTerminalDTO GetTimeTerminal(int actorCompanyId, int terminalId)
        {
            List<TimeTerminalSettingType> validSettingTypes = tsm.GetValidSettingTypes(TimeTerminalType.WebTimeStamp);
            return tsm.GetTimeTerminalDiscardState(terminalId).ToDTO(true, true, false, validSettingTypes);
        }

        [HttpGet]
        [Route("GetTerminalName")]
        [ResponseType(typeof(string))]
        public string GetTerminalName(int terminalId)
        {
            var terminal = tsm.GetTimeTerminalDiscardState(terminalId);
            return terminal.Name;
        }

        [HttpGet]
        [Route("GetTimeDeviationCauseForEmployeeNow")]
        [ResponseType(typeof(Dictionary<int, string>))]
        public Dictionary<int, string> GetTimeDeviationCauseForEmployeeNow(int actorCompanyId, string employeeNr)
        {
            return wts.GetTimeDeviationCauseForEmployeeNow(actorCompanyId, employeeNr);
        }

        [HttpGet]
        [Route("GetAccounts")]
        [ResponseType(typeof(Dictionary<int, string>))]
        public Dictionary<int, string> GetAccounts(int actorCompanyId, int timeTerminalId)
        {
            return wts.GetAccounts(actorCompanyId, timeTerminalId);
        }

        [HttpGet]
        [Route("GetCurrentTime")]
        [ResponseType(typeof(string))]
        public string GetCurrentTime(int timeTerminalId)
        {
            return wts.GetCurrentTime(timeTerminalId);
        }

        [HttpGet]
        [Route("GetImage")]
        [ResponseType(typeof(ImagesDTO))]
        public ImagesDTO GetImage(int actorCompanyId, int employeeId)
        {
            var gm = new GraphicsManager(null);
            var image = gm.GetImage(actorCompanyId, SoftOne.Soe.Common.Util.SoeEntityImageType.EmployeePortrait, SoftOne.Soe.Common.Util.SoeEntityType.Employee, employeeId, true).ToDTO(true);
            return image;

        }
    }
}