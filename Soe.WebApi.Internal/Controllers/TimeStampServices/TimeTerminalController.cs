using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.TimeStampServices
{
    [RoutePrefix("TimeStampServices/TimeTerminal")]
    public class TimeTerminalController : ApiBase
    {

        private TimeStampManager tsm;
        private TimeCodeManager tcm;
        private TimeDeviationCauseManager tdm;

        public TimeTerminalController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {
            this.tsm = new TimeStampManager(null);
            this.tcm = new TimeCodeManager(null);
            this.tdm = new TimeDeviationCauseManager(null);
        }


        [HttpGet]
        [Route("AllTimeTerminal")]
        [ResponseType(typeof(List<TimeTerminalDTO>))]
        public List<TimeTerminalDTO> GetTimeTerminal()
        {
            return tsm.GetAllTimeTerminals().ToDTOs(true, false, false);
        }

        [HttpGet]
        [Route("AllTimeTerminalGuids")]
        [ResponseType(typeof(List<Guid>))]
        public List<Guid> GetTimeTerminalGuids()
        {
            return tsm.GetAllTimeTerminalGuids();
        }

        [HttpGet]
        [Route("TimeTerminal/")]
        [ResponseType(typeof(List<TimeTerminalDTO>))]
        public List<TimeTerminalDTO> GetTimeTerminal(int actorCompanyId)
        {
            return tsm.GetTimeTerminals(actorCompanyId, TimeTerminalType.Unknown, true, false, false, true, true, true).ToDTOs(true, true, false);
        }

        [HttpGet]
        [Route("TimeCode")]
        [ResponseType(typeof(List<TimeCodeDTO>))]
        public List<TimeCodeDTO> GetTimeCodes(int actorCompanyId)
        {
            return tcm.GetTimeCodes(actorCompanyId).ToDTOs(false).ToList();
        }

        [HttpGet]
        [Route("TimeDeviationCause")]
        [ResponseType(typeof(List<TimeDeviationCauseDTO>))]
        public List<TimeDeviationCauseDTO> GetTimeDeviationCauses(int actorCompanyId)
        {
            return tdm.GetTimeDeviationCauses(actorCompanyId, loadTimeCode: true, loadEmployeeGroups: true).ToDTOs().ToList();
        }

        [HttpGet]
        [Route("TimeStampEntry/sync")]
        [ResponseType(typeof(List<TimeStampEntryDTO>))]
        public List<TimeStampEntryDTO> GetTimeStampEntries(int actorCompanyId, DateTime fromTime, DateTime toTime, DateTime? lastSyncTime)
        {
            return tsm.GetTimeStampsForCompany(fromTime, toTime, lastSyncTime, actorCompanyId).ToDTOs().ToList();
        }

        [HttpGet]
        [Route("TimeStampEntry")]
        [ResponseType(typeof(List<TimeStampEntryDTO>))]
        public List<TimeStampEntryDTO> GetTimeStampEntries(int actorCompanyId, DateTime fromTime, DateTime toTime)
        {
            return tsm.GetTimeStampsForCompany(fromTime, toTime, null, actorCompanyId).ToDTOs().ToList();
        }
    }
}
